using System.Reflection;

using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Enums;
using LabApi.Features.Permissions;
using LabExtended.Commands.Utilities;
using LabExtended.Commands.Contexts;
using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;
using LabExtended.Commands.Parameters;
using LabExtended.Commands.Tokens.Parsing;


using LabExtended.API;
using LabExtended.Core;
using LabExtended.Attributes;
using LabExtended.Extensions;
using LabExtended.Utilities.Unity;

using MEC;

using NorthwoodLib.Pools;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace LabExtended.Commands;

/// <summary>
/// Manages in-game commands.
/// </summary>
public static class CommandManager
{
    internal static readonly char[] spaceSeparator = [' '];
    internal static IEnumerator<float>? helperCoroutine;

    /// <summary>
    /// Gets called after a command is executed.
    /// </summary>
    public static event Action<CommandContext>? Executed; 
    
    /// <summary>
    /// Gets a list of all registered commands.
    /// </summary>
    public static List<CommandInstance> Commands { get; } = new(byte.MaxValue);

    /// <summary>
    /// Registers all commands found in a given assembly.
    /// </summary>
    /// <param name="assembly">The assembly to search for commands in.</param>
    /// <returns>List of registered commands.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static List<CommandInstance> RegisterCommands(this Assembly assembly)
    {
        if (assembly is null)
            throw new ArgumentNullException(nameof(assembly));
        
        var registered = new List<CommandInstance>();

        foreach (var type in assembly.GetTypes())
        {
            if (!type.InheritsType<CommandBase>() || type == typeof(CommandBase) 
                                                  || type == typeof(ContinuableCommandBase))
                continue;
            
            if (!type.HasAttribute<CommandAttribute>(out var commandAttribute))
                continue;

            if (string.IsNullOrWhiteSpace(commandAttribute.Name))
            {
                ApiLog.Warn("Command Manager", $"Command &3{type.FullName}&r could not be registered because " +
                                               $"it's name is whitespace or empty.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(commandAttribute.Description))
            {
                ApiLog.Warn("Command Manager", $"Command &3{type.FullName}&r could not be registered because " +
                                               $"it's description is whitespace or empty.");
                continue;
            }
            
            if (!commandAttribute.IsStatic && type.InheritsType<ContinuableCommandBase>())
                ApiLog.Warn("Command Manager", $"Command &3{type.FullName}&r disabled it's &6IsStatic&r property, " +
                                               $"but continuable commands must be static.");
            
            var instance = new CommandInstance(type, commandAttribute.Name, commandAttribute.Permission, commandAttribute.Description,
                commandAttribute.IsStatic,  commandAttribute.IsHidden, commandAttribute.TimeOut > 0f ? commandAttribute.TimeOut : null, commandAttribute.Aliases);

            if (instance is { SupportsPlayer: false, SupportsServer: false, SupportsRemoteAdmin: false })
            {
                ApiLog.Warn("Command Manager", $"Command &1{type.FullName}&r does not have any enabled input sources." +
                                               $"You can enable those by adding one of the source interfaces to the command class" +
                                               $"(for example &2IRemoteAdminCommand&r, or for simplicity &2IAllCommand&r or &2IServerCommand&r)");
                
                continue;
            }

            foreach (var method in type.GetAllMethods())
            {
                if (method.IsStatic)
                    continue;
                
                if (!method.HasAttribute<CommandOverloadAttribute>(out var commandOverloadAttribute))
                    continue;

                if (method.ReturnType != typeof(void) && method.ReturnType != typeof(IEnumerator<float>))
                {
                    ApiLog.Warn("Command Manager", $"Method &3{method.GetMemberName()}&r cannot be used as an overload" +
                                                   $"because it's return type is not supported (&1{method.ReturnType.FullName}&r)." +
                                                   $"Command method's should return only &2void&r or an &2IEnumerator<float>&r coroutine.");
                    continue;
                }

                var overload = new CommandOverload(method);
                
                if (!string.IsNullOrWhiteSpace(commandOverloadAttribute.Name))
                    overload.Name = commandOverloadAttribute.Name;
                
                instance.Overloads.Add(overload);
            }

            if (instance.Overloads.Count < 1)
            {
                ApiLog.Warn("Command Manager", $"Command &1{type.FullName}&r does not have any suitable overloads.");
                continue;
            }
            
            Commands.Add(instance);
            registered.Add(instance);
            
            ApiLog.Debug("Command Manager", $"Registered command &3{instance.Name}&r (&6{type.FullName}&r) " +
                                            $"with &3{instance.Overloads.Count} overload(s)");
        }

        return registered;
    }

    /// <summary>
    /// Attempts to invoke a command.
    /// </summary>
    /// <param name="command">The command to invoke.</param>
    /// <param name="results">Parsing results to copy to buffer.</param>
    /// <returns>true if the command was successfully invoked</returns>
    public static bool TryInvokeCommand(CommandBase command, List<CommandParameterParserResult> results)
    {
        var buffer = CopyBuffer(command.Context, results);
        
        try
        {
            var result = command.Overload.Method(command, buffer);

            if (result is IEnumerator<float> coroutine)
                RunCoroutineCommand(command.Context, coroutine);
            
            command.Overload.Buffer.Return(buffer);
            return true;
        }
        catch (Exception ex)
        {
            command.Overload.Buffer.Return(buffer);
            command.Context.Response = new(false, false, command.Context.FormatExceptionResponse(ex));
            
            return false;
        }
    }

    /// <summary>
    /// Attempts to find an overload compatible with parsed arguments.
    /// </summary>
    /// <param name="ctx">The command context.</param>
    /// <param name="results">The parsed results.</param>
    /// <param name="result">The parser result.</param>
    /// <returns>true if an overload was found</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool TryFindOverload(CommandContext ctx, List<CommandParameterParserResult> results, out CommandParameterParserResult result)
    {
        if (ctx is null)
            throw new ArgumentNullException(nameof(ctx));
        
        if (results is null)
            throw new ArgumentNullException(nameof(results));
        
        if (ctx.Command.Overloads.Count == 1)
        {
            ctx.Overload = ctx.Command.Overloads[0];

            result = CommandParameterParserUtils.ParseParameters(ctx, results);

            if (!result.Success)
                return false;

            if (results.Count(r => r.Success) != ctx.Overload.ParameterCount)
            {
                result = new(false, null, "INVALID_ARGS");
                return false;
            }
            
            return true;
        }

        for (var i = 0; i < ctx.Command.Overloads.Count; i++)
        {
            var overload = ctx.Command.Overloads[i];

            results.Clear();
            result = CommandParameterParserUtils.ParseParameters(ctx, results);

            if (!result.Success)
                continue;

            if (results.Count(r => r.Success) != overload.ParameterCount)
            {
                result = new(false, null, "INVALID_ARGS");
                return false;
            }

            ctx.Overload = overload;
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Attempts to find a command for a given command line.
    /// </summary>
    /// <param name="commandLine">The command line.</param>
    /// <param name="type">The type of executed command.</param>
    /// <param name="command">The target command.</param>
    /// <returns>true if the command was found</returns>
    public static bool TryFindCommand(string commandLine, CommandType? type, out CommandInstance? command)
        => TryFindCommand(commandLine, type, null, out command, out _);

    /// <summary>
    /// Attempts to find a command for a given command line.
    /// </summary>
    /// <param name="commandLine">The command line.</param>
    /// <param name="type">The type of executed command.</param>
    /// <param name="rawArgs">A list of raw arguments separated by a space.</param>
    /// <param name="targetCommand">The target command.</param>
    /// <param name="fixedCommandLine">The fixed command line.</param>
    /// <returns>true if the command was found</returns>
    public static bool TryFindCommand(string commandLine, CommandType? type, List<string>? rawArgs, out CommandInstance? targetCommand, out string? fixedCommandLine)
    {
        if (string.IsNullOrWhiteSpace(commandLine))
            throw new ArgumentNullException(nameof(commandLine));
        
        var commandLineSplit = commandLine.Split(spaceSeparator, StringSplitOptions.RemoveEmptyEntries);
        
        for (var i = 0; i < Commands.Count; i++)
        {
            var cmd = Commands[i];

            if (type.HasValue)
            {
                switch (type)
                {
                    case CommandType.Client when !cmd.SupportsPlayer:
                    case CommandType.Console when !cmd.SupportsServer:
                    case CommandType.RemoteAdmin when !cmd.SupportsRemoteAdmin:
                        continue;
                }
            }

            if (commandLineSplit.Length < cmd.NameParts.Length)
                continue;

            var isMatched = false;
                    
            for (var x = 0; x < commandLineSplit.Length; x++)
            {
                if (x < cmd.NameParts.Length)
                {
                    if (!string.Equals(commandLineSplit[x], cmd.NameParts[x], StringComparison.InvariantCultureIgnoreCase))
                    {
                        isMatched = false;
                        break;
                    }
                }
                
                rawArgs?.Add(commandLineSplit[x].Trim(spaceSeparator));
                
                isMatched = true;
            }
                    
            if (!isMatched)
                continue;

            targetCommand = cmd;
            
            fixedCommandLine = commandLine.Substring(cmd.Name.Length, commandLine.Length - cmd.Name.Length)
                .TrimStart(spaceSeparator)
                .TrimEnd(spaceSeparator);
            return true;
        }

        fixedCommandLine = null;
        targetCommand = null;

        return false;
    }

    // Handles custom command execution.
    private static void OnCommand(CommandExecutingEventArgs ev)
    {
        if (ev.CommandFound && !ApiLoader.ApiConfig.CommandSection.AllowOverride)
            return;
        
        try
        {       
            if (!ExPlayer.TryGet(ev.Sender, out var player))
                return;
            
            if (HandleContinuable(ev, player, out var line))
                return;

            var args = ListPool<string>.Shared.Rent();

            if (!TryFindCommand(line, ev.CommandType, args, out var command, out line))
            {
                ListPool<string>.Shared.Return(args);
                return;
            }

            ev.IsAllowed = false;

            if (command.Permission != null && !player.HasPermissions(command.Permission))
            {
                ev.Sender.Respond(CommandResponseFormatter.FormatMissingPermissionsFailure(command.Permission, command.Name, ev.CommandType));
                
                ListPool<string>.Shared.Return(args);
                return;
            }

            var tokens = ListPool<ICommandToken>.Shared.Rent();
            var context = new CommandContext();
            
            context.Args = args;
            context.Line = line;
            context.Sender = player;
            context.Tokens = tokens;
            context.Command = command;

            context.Type = ev.CommandType;

            if (line?.Length > 0)
            {
                var tokenParsingResult = CommandTokenParser.ParseTokens(line, tokens);

                if (!tokenParsingResult.IsSuccess)
                {
                    context.Response = new(false, false, context.FormatTokenParserFailure(tokenParsingResult));

                    OnExecuted(context);
                    return;
                }
            }
            
            var parserResults = ListPool<CommandParameterParserResult>.Shared.Rent();

            if (!TryFindOverload(context, parserResults, out var parserResult))
            {
                if (!context.Response.HasValue)
                {
                    if (parserResult.Error is "MISSING_ARGS")
                        context.Response = new(false, false, context.FormatMissingArgumentsFailure());
                    else if (parserResult.Error is "INVALID_ARGS")
                        context.Response = new(false, false, context.FormatInvalidArgumentsFailure(parserResults));
                    else
                        context.Response = new(false, false, context.FormatNoOverloadsFailure());
                }

                OnExecuted(context);
                return;
            }
            
            var instance = command.GetInstance();

            if (instance is null)
            {
                context.Response = new(false, false, "Failed to retrieve command instance!");
                
                OnExecuted(context);
                return;
            }

            instance.Context = context;
            context.Instance = instance;

            if (!context.Overload.IsInitialized)
            {
                instance.OnInitializeOverload(context.Overload.Name, context.Overload.ParameterBuilders);
                
                context.Overload.IsInitialized = true;
            }

            TryInvokeCommand(instance, parserResults);
            
            OnExecuted(context);
            
            ListPool<CommandParameterParserResult>.Shared.Return(parserResults);
        }
        catch (Exception ex)
        {
            ApiLog.Error("Command Manager", $"An error occured while executing command:\n{ex.ToColoredString()}");

            ev.IsAllowed = false;
            ev.Sender.Respond(ex.Message, false);
        }
    }

    private static void RunContinuableCommand(CommandContext ctx, ContinuableCommandBase command)
    {
        try
        {
            command.OnContinued();
        }
        catch (Exception ex)
        {
            ApiLog.Error("Command Manager", $"An error occured while handling continuable command &1{ctx.Command.Name}&r:\n" +
                                            $"{ex.ToColoredString()}");

            ctx.Response = new(false, false, ex.Message);
        }
        
        OnExecuted(ctx);
    }

    private static void RunCoroutineCommand(CommandContext ctx, IEnumerator<float> coroutine)
    {
        ctx.WriteResponse(out _);

        Timing.RunCoroutine(helperCoroutine ??= HelperCoroutine(), Segment.FixedUpdate);
        return;

        IEnumerator<float> HelperCoroutine()
        {
            while (coroutine.MoveNext())
                yield return coroutine.Current;
            
            OnExecuted(ctx);
        }
    }

    private static bool HandleContinuable(CommandExecutingEventArgs ev, ExPlayer player, out string line)
    {
        line = string.Join(" ", ev.Arguments.Array);
        
        if (!ContinuableCommandBase.History.TryGetValue(player.NetworkId, out var history)) 
            return false;
        
        ContinuableCommandBase.History.Remove(player.NetworkId);

        ev.IsAllowed = false;

        var command = history.CommandData.GetInstance() as ContinuableCommandBase;
        var args = ListPool<string>.Shared.Rent(ev.Arguments.Array);

        var context = new CommandContext()
        {
            Args = args,
            Line = line,
            Sender = player,
            Instance = command,
                
            Type = ev.CommandType,
            Command = history.CommandData,
        };

        command.PreviousContext = command.Context;
        command.Context = context;

        RunContinuableCommand(context, command);
        return true;
    }

    private static void OnExecuted(CommandContext ctx)
    {
        if (ctx.WriteResponse(out var continuableCommand))
        {
            ContinuableCommandBase.History[ctx.Sender.NetworkId] = continuableCommand;

            if (continuableCommand.CommandData.TimeOut.HasValue)
            {
                continuableCommand.remainingTime = continuableCommand.CommandData.TimeOut.Value;

                if (!continuableCommand.updateAssigned)
                {
                    PlayerLoopHelper.AfterLoop += continuableCommand.Update;

                    continuableCommand.updateAssigned = true;
                    continuableCommand.Reset();
                }
            }
        }

        if (!ctx.Command.IsStatic && ApiLoader.ApiConfig.CommandSection.AllowInstancePooling)
            ctx.Command.DynamicPool.Add(ctx.Instance);
        
        Executed.InvokeSafe(ctx);

        if (ctx.Args != null)
            ListPool<string>.Shared.Return(ctx.Args);
        
        if (ctx.Tokens != null)
            ListPool<ICommandToken>.Shared.Return(ctx.Tokens);
        
        ctx.Args = null;
        ctx.Tokens = null;
    }

    private static object[] CopyBuffer(CommandContext ctx, List<CommandParameterParserResult> results)
    {
        var buffer = ctx.Overload.Buffer.Rent();

        if (ctx.Overload.ParameterCount != 0)
        {
            for (var i = 0; i < ctx.Overload.ParameterCount; i++)
            {
                buffer[i] = results[i].Value;
            }
        }

        return buffer;
    }

    [LoaderInitialize(1)]
    private static void OnInit()
    {
        ServerEvents.CommandExecuting += OnCommand;
    }
}