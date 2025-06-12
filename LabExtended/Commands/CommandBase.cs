using System.Text;

using LabApi.Features.Enums;

using LabExtended.Commands.Interfaces;
using LabExtended.Commands.Parameters;

using LabExtended.API;
using LabExtended.Commands.Tokens;
using LabExtended.Commands.Utilities;
using LabExtended.Extensions;
using LabExtended.Utilities;

using NorthwoodLib.Pools;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace LabExtended.Commands;

/// <summary>
/// Represents the base class for a command.
/// </summary>
public class CommandBase
{
    /// <summary>
    /// Gets the context of the current command execution.
    /// </summary>
    public CommandContext Context { get; internal set; }
    
    /// <summary>
    /// Gets the sender of the command.
    /// </summary>
    public ExPlayer Sender => Context.Sender;
    
    /// <summary>
    /// Gets information about the currently executing command.
    /// </summary>
    public CommandData CommandData => Context.Command;
    
    /// <summary>
    /// Gets the overload that is being executed.
    /// </summary>
    public CommandOverload Overload => Context.Overload;
    
    /// <summary>
    /// Gets a list of arguments split by space.
    /// </summary>
    public List<string> Args  => Context.Args;
    
    /// <summary>
    /// Gets a list of parsed command tokens.
    /// </summary>
    public List<ICommandToken> Tokens => Context.Tokens;
    
    /// <summary>
    /// Gets the type of the executing command.
    /// </summary>
    public CommandType CommandType => Context.Type;
    
    /// <summary>
    /// Gets the full command line.
    /// </summary>
    public string Line => Context.Line;

    /// <summary>
    /// Gets or sets the context's response.
    /// </summary>
    public CommandResponse? Response
    {
        get => Context.Response;
        set => Context.Response = value;
    }

    /// <summary>
    /// Called when an overload is called for the first time.
    /// <remarks><b><paramref name="overloadName"/> will be null if the default overload is being initialized!</b></remarks>
    /// </summary>
    /// <param name="overloadName">Name of the overload that is being initialized.</param>
    /// <param name="parameters">The overload's parameters.</param>
    public virtual void OnInitializeOverload(string? overloadName, Dictionary<string, CommandParameterBuilder> parameters) { }

    /// <summary>
    /// Responds to the command.
    /// </summary>
    /// <param name="content">The message to respond with.</param>
    /// <param name="isSuccess">Whether or not the command was successfully executed.</param>
    public void Respond(object content, bool isSuccess = true)
        => Response = new(isSuccess, false, false, null, content.ToString());

    /// <summary>
    /// Responds to the command.
    /// </summary>
    /// <param name="contentBuilder">Delegate used to build the command's response.</param>
    /// <param name="isSuccess">Whether or not the command was successfully executed.</param>
    public void Respond(Action<StringBuilder> contentBuilder, bool isSuccess = true)
        => Response = new(isSuccess, false, false, null, StringBuilderPool.Shared.BuildString(contentBuilder));
    
    /// <summary>
    /// Responds to the command with a success.
    /// </summary>
    /// <param name="content">The message to respond with.</param>
    public void Ok(object content)
        => Response = new(true, false, false, null, content.ToString());
    
    /// <summary>
    /// Responds to the command with a success.
    /// </summary>
    /// <param name="contentBuilder">The delegate used to build the command's response.</param>
    public void Ok(Action<StringBuilder> contentBuilder)
        => Response = new(true, false, false, null, StringBuilderPool.Shared.BuildString(contentBuilder));
    
    /// <summary>
    /// Responds to the command with a failure.
    /// </summary>
    /// <param name="content">The message to respond with.</param>
    public void Fail(object content)
        => Response = new(false, false, false, null, content.ToString());
    
    /// <summary>
    /// Responds to the command with a failure.
    /// </summary>
    /// <param name="contentBuilder">The delegate used to build the command's response.</param>
    public void Fail(Action<StringBuilder> contentBuilder)
        => Response = new(false, false, false, null, StringBuilderPool.Shared.BuildString(contentBuilder));

    /// <summary>
    /// Responds to the command with an input request.
    /// </summary>
    /// <param name="response">The text to respond with.</param>
    /// <param name="onInput">The delegate used as callback.</param>
    public void Read(object response, Action<string> onInput)
        => Response = new(true, false, true, onInput, response?.ToString() ?? "Command requires more input.");

    /// <summary>
    /// Responds to the command with an input request.
    /// </summary>
    /// <param name="contentBuilder">The delegate used to build text to respond with.</param>
    /// <param name="onInput">The delegate used as callback.</param>
    public void Read(Action<StringBuilder> contentBuilder, Action<string> onInput)
        => Response = new(true, false, true, onInput, StringBuilderPool.Shared.BuildString(contentBuilder));

    /// <summary>
    /// Responds to the command with an input request.
    /// </summary>
    /// <param name="response">The text to respond with.</param>
    /// <param name="onInput">The delegate used as callback.</param>
    /// <typeparam name="T">The type the input string will be parsed to.</typeparam>
    public void Read<T>(object response, Action<T> onInput)
    {
        void OnInput(string result)
        {
            var token = StringToken.Instance.NewToken<StringToken>();
            
            var list = ListPool<ICommandToken>.Shared.Rent();
            var argList = ListPool<string>.Shared.Rent();
            
            var ctx = new CommandContext();
            var parameter = new CommandParameter(typeof(T), string.Empty, string.Empty, null, false);

            ctx.Args = argList;
            ctx.Line = result;
            ctx.Sender = Sender;
            ctx.Type = CommandType;
            ctx.Command = CommandData;
            ctx.Overload = Overload;
            
            token.Value = result;
            
            list.Add(token);

            var parsed = parameter.Type.Parser.Parse(list, token, 0, ctx, parameter);

            if (!parsed.Success)
            {
                Read($"Could not parse input to '{parameter.Type.Parser.FriendlyAlias}' ({typeof(T).Name}): {parsed.Error}, try again.", OnInput);
                
                ListPool<string>.Shared.Return(argList);
                ListPool<ICommandToken>.Shared.Return(list);
                
                token.ReturnToken();
                return;
            }
            
            onInput?.InvokeSafe((T)parsed.Value);
            
            ListPool<string>.Shared.Return(argList);
            ListPool<ICommandToken>.Shared.Return(list);
                
            token.ReturnToken();
        }
        
        Read(response, OnInput);
    }

    /// <summary>
    /// Writes a message into the sender's console.
    /// </summary>
    /// <param name="content">The message to show.</param>
    /// <param name="success">Whether or not to show the message as successful.</param>
    public void Write(object content, bool success = true)
        => Sender.SendRemoteAdminMessage(content, success, true, CommandData.Name);

    /// <summary>
    /// Writes a message into the sender's console.
    /// </summary>
    /// <param name="contentBuilder">The delegate used to build the message.</param>
    /// <param name="success">Whether or not to show the message as successful.</param>
    public void Write(Action<StringBuilder> contentBuilder, bool success = true)
    {
        if (CommandType is CommandType.Client)
            Sender.SendConsoleMessage(StringBuilderPool.Shared.BuildString(contentBuilder), success ? "green" : "red");
        else
            Sender.SendRemoteAdminMessage(StringBuilderPool.Shared.BuildString(contentBuilder), success, true, CommandData.Name);
    }

    /// <summary>
    /// Writes a message into the sender's console (you should use this method if you want to respond from another thread).
    /// </summary>
    /// <param name="content">The content of the message.</param>
    /// <param name="success">Whether or not to show the message as successful.</param>
    public void WriteThread(object content, bool success = true)
    {
        ThreadUtils.RunOnMainThread(() => Write(content, success));
    }
    
    /// <summary>
    /// Writes a message into the sender's console (you should use this method if you want to respond from another thread).
    /// </summary>
    /// <param name="contentBuilder">The delegate used to build the message.</param>
    /// <param name="success">Whether or not to show the message as successful.</param>
    public void WriteThread(Action<StringBuilder> contentBuilder, bool success = true)
    {
        ThreadUtils.RunOnMainThread(() => Write(contentBuilder, success));
    }
}