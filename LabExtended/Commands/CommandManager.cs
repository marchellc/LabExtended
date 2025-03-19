using System.Reflection;

using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Enums;

using LabExtended.Commands.Contexts;
using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;
using LabExtended.Commands.Parameters;
using LabExtended.Commands.Tokens.Parsing;

using LabExtended.Core;

using LabExtended.API;
using LabExtended.Attributes;
using LabExtended.Extensions;

using MEC;

using NorthwoodLib.Pools;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace LabExtended.Commands;

/// <summary>
/// Manages in-game commands.
/// </summary>
public static class CommandManager
{
    private static readonly char[] spaceSeparator = [' '];
    
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
            
            if (commandAttribute.IsStatic && type.InheritsType<ContinuableCommandBase>())
                ApiLog.Warn("Command Manager", $"Command &3{type.FullName}&r enabled it's &6IsStatic&r property, " +
                                               $"but continuable commands cannot be static.");
            
            var instance = new CommandInstance(type, commandAttribute.Name, commandAttribute.Description,
                commandAttribute.IsStatic, commandAttribute.Aliases);

            if (instance is { SupportsPlayer: false, SupportsServer: false, SupportsRemoteAdmin: false })
            {
                ApiLog.Warn("Command Manager", $"Command &1{type.FullName}&r does not have any enabled input sources." +
                                               $"You can enable those by adding one of the source interfaces to the command class" +
                                               $"(for example IRemoteAdminCommand, or for simplicity IAllCommand or IServerCommand)");
                
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

                foreach (var parameter in method.GetAllParameters())
                    overload.ParameterBuilders.Add(parameter.Name, new(parameter));
                
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

    // Handles custom command execution.
    private static void OnCommand(CommandExecutingEventArgs args)
    {
        if (!ExPlayer.TryGet(args.Sender, out var player))
            return;
        
        ApiLog.Debug("Command Manager", $"Handling command event, sender &1{args.Sender.LogName}&r");

        try
        {
            var commandLine = string.Join(" ", args.Arguments.Array);
            
            ApiLog.Debug("Command Manager", $"Joined commandLine: &6{commandLine}&r");

            if (ContinuableCommandBase.History.TryGetValue(player.NetworkId, out var history))
            {
                ApiLog.Debug("Command Manager", $"Player has an active continuable command");
                
                ContinuableCommandBase.History.Remove(player.NetworkId);

                args.IsAllowed = false;

                var newCommand = history.CommandData.GetInstance() as ContinuableCommandBase;
                var newArgs = ListPool<string>.Shared.Rent(args.Arguments.Array);

                var commandContext = new CommandContext()
                {
                    Sender = player,
                    Command = history.CommandData,
                    Line = commandLine,
                    Args = newArgs,
                    Type = args.CommandType,
                    Instance = newCommand
                };

                newCommand.Previous = history;
                newCommand.Context = commandContext;
                
                ApiLog.Debug("Command Manager", $"Running continuable command");

                RunContinuableCommand(commandContext, newCommand);
            }
            else
            {
                if (args.CommandFound && !ApiLoader.ApiConfig.CommandSection.AllowOverride)
                {
                    ApiLog.Debug("Command Manager", $"Base-game command was found and override is disabled");
                    return;
                }

                var commandFound = false;
                var command = default(CommandInstance);
                
                ApiLog.Debug("Command Manager", $"Searching for commands .. (&1{Commands.Count}&r)");
                
                Commands.ForEach(cmd =>
                {
                    ApiLog.Debug("Command Manager", $"Checking command &1{cmd.Name}&r");
                        
                    if (commandFound)
                        return;

                    switch (args.CommandType)
                    {
                        case CommandType.Client when !cmd.SupportsPlayer:
                        case CommandType.Console when !cmd.SupportsServer:
                        case CommandType.RemoteAdmin when !cmd.SupportsRemoteAdmin:
                            ApiLog.Debug("Command Manager", $"Command does not support source console");
                            return;
                    }

                    if (!commandLine.StartsWith(cmd.Name))
                    {
                        ApiLog.Debug("Command Manager", $"commandLine check failed");
                        return;
                    }

                    commandLine = commandLine.Substring(0, cmd.Name.Length);
                    command = cmd;
                    commandFound = true;
                        
                    ApiLog.Debug("Command Manager", $"Command has been found: &1{cmd.Name}&r");
                });

                if (commandFound)
                {
                    args.IsAllowed = false;

                    var commandArgs =
                        ListPool<string>.Shared.Rent(commandLine.Split(spaceSeparator,
                            StringSplitOptions.RemoveEmptyEntries));
                    var commandTokens = ListPool<ICommandToken>.Shared.Rent();

                    var commandContext = new CommandContext()
                    {
                        Sender = player,
                        Command = command,
                        Args = commandArgs,
                        Line = commandLine,
                        Tokens = commandTokens,
                        Type = args.CommandType
                    };

                    ApiLog.Debug("Command Manager", $"Parsing tokens");

                    if (!string.IsNullOrWhiteSpace(commandLine))
                    {
                        var tokenResult = CommandTokenParser.ParseTokens(commandLine, commandTokens);

                        if (!tokenResult.IsSuccess)
                        {
                            ApiLog.Debug("Command Manager", $"Token parsing failed: &1{tokenResult.Error}&r");

                            ListPool<ICommandToken>.Shared.Return(commandTokens);
                            ListPool<string>.Shared.Rent(commandArgs);

                            if (args.CommandType is CommandType.Console or CommandType.RemoteAdmin)
                            {
                                player.SendRemoteAdminMessage(FormatTokenParserFailure(commandContext, tokenResult,
                                    true, false), false, true, command.Name);
                            }
                            else
                            {
                                player.SendConsoleMessage(FormatTokenParserFailure(commandContext, tokenResult,
                                    false, true), "red");
                            }

                            return;
                        }
                    }

                    ApiLog.Debug("Command Manager", $"Token parsing success");

                    var parserResults = ListPool<CommandParameterParserResult>.Shared.Rent(5);

                    var targetOverload = default(CommandOverload);
                    var targetBuffer = default(object[]);

                    if (command.Overloads.Count == 1)
                    {
                        ApiLog.Debug("Command Manager", $"Selecting single overload");

                        targetOverload = command.Overloads[0];

                        var successCount = parserResults.Count(x => x.Success);

                        ApiLog.Debug("Command Manager",
                            $"Successful parameters: &2{successCount}&r / &3{targetOverload.ParameterCount}&r");

                        if (successCount != targetOverload.Parameters.Count)
                        {
                            ApiLog.Debug("Command Manager", $"Overload parsing failed");
                            return;
                        }

                        targetBuffer = targetOverload.Buffer.Rent();

                        for (int i = 0; i < parserResults.Count; i++)
                            targetBuffer[i] = parserResults[i].Value;

                        ApiLog.Debug("Command Manager",
                            $"Selected first overload (&1{targetOverload.Target.GetMemberName(true)}&r)");
                    }
                    else
                    {
                        ApiLog.Debug("Command Manager", $"Selecting from multiple overloads");

                        command.Overloads.ForEach(ov =>
                        {
                            if (targetOverload != null)
                                return;

                            parserResults.Clear();

                            if (parserResults.Capacity < ov.Parameters.Count)
                                parserResults.Capacity = ov.Parameters.Count;

                            ApiLog.Debug("Command Manager", $"Parsing overload &1{ov.Target.GetMemberName()}&r");

                            CommandParameterParser.ParseParameters(ov, commandTokens, parserResults,
                                commandContext);

                            var successCount = parserResults.Count(x => x.Success);

                            ApiLog.Debug("Command Manager",
                                $"Successful parameters: &2{successCount}&r / &3{ov.ParameterCount}&r");

                            if (successCount != ov.Parameters.Count)
                            {
                                ApiLog.Debug("Command Manager", $"Overload parsing failed");
                                return;
                            }

                            targetOverload = ov;
                            targetBuffer = ov.Buffer.Rent();

                            for (int i = 0; i < parserResults.Count; i++)
                                targetBuffer[i] = parserResults[i].Value;

                            ApiLog.Debug("Command Manager",
                                $"Selected target overload &1{targetOverload.Target.GetMemberName(true)}&r");
                        });
                    }

                    ListPool<CommandParameterParserResult>.Shared.Return(parserResults);

                    if (targetOverload is null)
                    {
                        ApiLog.Debug("Command Manager", $"No overload was found");

                        ListPool<ICommandToken>.Shared.Return(commandTokens);
                        ListPool<string>.Shared.Rent(commandArgs);

                        if (args.CommandType is CommandType.Console or CommandType.RemoteAdmin)
                        {
                            player.SendRemoteAdminMessage(FormatNoOverloadsFailure(commandContext, true, false),
                                false, true, command.Name);
                        }
                        else
                        {
                            player.SendConsoleMessage(FormatNoOverloadsFailure(commandContext, false, true),
                                "red");
                        }
                    }
                    else
                    {
                        ApiLog.Debug("Command Manager", $"Overload found, constructing instance");

                        var instance = command.GetInstance();

                        instance.Context = commandContext;

                        commandContext.Instance = instance;
                        commandContext.Overload = targetOverload;

                        ApiLog.Debug("Command Manager", $"Set up instance");

                        if (!targetOverload.IsInitialized)
                        {
                            ApiLog.Debug("Command Manager", $"Initializing overload");

                            instance.OnInitializeOverload(targetOverload.Name, targetOverload.ParameterBuilders);

                            targetOverload.Parameters.Clear();
                            targetOverload.ParameterBuilders.ForEach(
                                p => targetOverload.Parameters.Add(p.Value.Result));

                            targetOverload.IsInitialized = true;

                            ApiLog.Debug("Command Manager", $"Overload initialized");
                        }

                        ApiLog.Debug("Command Manager", $"Invoking overload");

                        object? result = null;

                        try
                        {
                            result = targetOverload.Method(instance, targetBuffer);
                        }
                        catch (Exception ex)
                        {
                            ApiLog.Error("Command Manager", ex);

                            if (args.CommandType is CommandType.Console or CommandType.RemoteAdmin)
                            {
                                player.SendRemoteAdminMessage(FormatExceptionResponse(commandContext, ex,
                                    true, false), false, true, command.Name);
                            }
                            else
                            {
                                player.SendConsoleMessage(FormatExceptionResponse(commandContext, ex,
                                    false, true), "red");
                            }

                            return;
                        }

                        ApiLog.Debug("Command Manager",
                            $"Overload invoked, result: &1{result?.GetType()?.FullName ?? "null"}&r");

                        if (result is IEnumerator<float> coroutine)
                        {
                            ApiLog.Debug("Command Manager", $"Running coroutine");

                            RunCoroutineCommand(commandContext, coroutine);
                        }
                        else
                        {
                            ApiLog.Debug("Command Manager", $"Returning response");

                            if (commandContext.Response.HasValue)
                            {
                                if (args.CommandType is CommandType.Console or CommandType.RemoteAdmin)
                                {
                                    player.SendRemoteAdminMessage(
                                        FormatCommandResponse(commandContext, true, false),
                                        commandContext.Response?.IsSuccess ?? false, true, command.Name);
                                }
                                else
                                {
                                    player.SendConsoleMessage(FormatCommandResponse(commandContext, false, true),
                                        commandContext.Response is { IsSuccess: true } ? "green" : "red");
                                }
                            }

                            if (commandContext is
                                {
                                    Response: { IsContinuted: true },
                                    Instance: ContinuableCommandBase continuableCommand
                                })
                                ContinuableCommandBase.History[commandContext.Sender.NetworkId] = continuableCommand;
                        }

                        ApiLog.Debug("Command Manager", $"Returning buffer");

                        targetOverload.Buffer.Return(targetBuffer);

                        if (!command.IsStatic && ApiLoader.ApiConfig.CommandSection.AllowInstancePooling)
                            command.DynamicPool.Add(instance);

                        ApiLog.Debug("Command Manager", $"Command execution completed");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ApiLog.Error("Command Manager", $"An error occured while executing command:\n{ex.ToColoredString()}");
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
            
            if (ctx.Type is CommandType.Console or CommandType.RemoteAdmin)
            {
                ctx.Sender.SendRemoteAdminMessage(FormatExceptionResponse(ctx, ex, 
                    true, false), false, true, ctx.Command.Name);
            }
            else
            {
                ctx.Sender.SendConsoleMessage(FormatExceptionResponse(ctx, ex, 
                    false, true), "red");
            }

            return;
        }

        if (ctx.Response.HasValue)
        {
            if (ctx.Type is CommandType.Console or CommandType.RemoteAdmin)
            {
                ctx.Sender.SendRemoteAdminMessage(FormatCommandResponse(ctx, true, false),
                    ctx.Response.Value.IsSuccess, true, ctx.Command.Name);
            }
            else
            {
                ctx.Sender.SendConsoleMessage(FormatCommandResponse(ctx, false, true),
                    ctx.Response.Value.IsSuccess ? "green" : "red");
            }
        }

        if (command.Response is { IsContinuted: true })
            ContinuableCommandBase.History[ctx.Sender.NetworkId] = command;
        
        if (ApiLoader.ApiConfig.CommandSection.AllowInstancePooling)
            ctx.Command.DynamicPool.Add(command);
    }

    private static void RunCoroutineCommand(CommandContext ctx, IEnumerator<float> coroutine)
    {
        if (ctx.Response.HasValue)
        {
            if (ctx.Type is CommandType.Console or CommandType.RemoteAdmin)
            {
                ctx.Sender.SendRemoteAdminMessage(FormatCommandResponse(ctx, true, false),
                    ctx.Response?.IsSuccess ?? false, true, ctx.Command.Name);
            }
            else
            {
                ctx.Sender.SendConsoleMessage(FormatCommandResponse(ctx, false, true),
                    ctx.Response is { IsSuccess: true } ? "green" : "red");
            }
        }

        IEnumerator<float> HelperCoroutine()
        {
            var handle = Timing.RunCoroutine(coroutine);
            
            yield return Timing.WaitUntilDone(handle);
            
            if (ctx.Type is CommandType.Console or CommandType.RemoteAdmin)
            {
                ctx.Sender.SendRemoteAdminMessage(FormatCommandResponse(ctx, true, false),
                    ctx.Response?.IsSuccess ?? false, true, ctx.Command.Name);
            }
            else
            {
                ctx.Sender.SendConsoleMessage(FormatCommandResponse(ctx, false, true),
                    ctx.Response is { IsSuccess: true } ? "green" : "red");
            }
            
            if (ctx is { Response: { IsContinuted: true }, Instance: ContinuableCommandBase continuableCommand })
                ContinuableCommandBase.History[ctx.Sender.NetworkId] = continuableCommand;

            if (!ctx.Command.IsStatic && ApiLoader.ApiConfig.CommandSection.AllowInstancePooling)
                ctx.Command.DynamicPool.Add(ctx.Instance);
        }

        Timing.RunCoroutine(HelperCoroutine());
    }

    private static string FormatCommandResponse(CommandContext ctx, bool allowColors, bool isConsole)
    {
        return StringBuilderPool.Shared.BuildString(x =>
        {
            if (isConsole)
            {
                x.Append($"[");
                x.Append(ctx.Command.Name);
                x.Append("] ");
            }

            x.AppendLine(ctx.Response.Value.Content);
        });
    }

    private static string FormatExceptionResponse(CommandContext ctx, Exception ex, bool allowColors, bool isConsole)
    {
        return StringBuilderPool.Shared.BuildString(x =>
        {
            if (isConsole)
            {
                x.Append($"[");
                x.Append(ctx.Command.Name);
                x.Append("] ");
            }

            x.AppendLine("Failed while invoking command: ");
            x.AppendLine(ex.Message);
        });
    }

    private static string FormatNoOverloadsFailure(CommandContext ctx, bool allowColors, bool isConsole)
    {
        return StringBuilderPool.Shared.BuildString(x =>
        {
            if (isConsole)
            {
                x.Append($"[");
                x.Append(ctx.Command.Name);
                x.Append("] ");
            }

            x.AppendLine("Failed while searching for compatible command overloads.");
            x.AppendLine($"Use \"help {ctx.Command.Name}\" to get a list of command overloads.");
        });
    }

    private static string FormatTokenParserFailure(CommandContext ctx, CommandTokenParserResult result, bool allowColors, bool isConsole)
    {
        return StringBuilderPool.Shared.BuildString(x =>
        {
            if (isConsole)
            {
                x.Append($"[");
                x.Append(ctx.Command.Name);
                x.Append("] ");
            }

            x.Append("Failed while parsing command arguments at position ");
            x.Append(result.Position.Value);
            x.Append(", faulty character: ");
            x.Append(result.Character.Value);
                
            x.AppendLine();
            x.AppendLine();
            
            if (allowColors)
            {
                x.Append("<color=green>");

                for (var i = 0; i < result.Position.Value; i++)
                    x.Append(result.Input[i]);
                
                x.Append("</color>");
                
                x.Append("<color=red>");
                x.Append(result.Character.Value);
                x.Append("</color>");

                x.Append("<color=green>");
                
                for (var i = result.Position.Value; i < result.Input.Length; i++)
                    x.Append(result.Input[i]);
                
                x.AppendLine("</color>");
                
                x.Append("<color=red>");

                for (var i = 0; i < result.Position.Value; i++)
                    x.Append(" ");

                x.Append("^^</color>");
            }
            else
            {
                x.AppendLine(result.Input);
                
                for (var i = 0; i < result.Position.Value; i++)
                    x.Append(" ");
                
                x.Append("^^");
            }
            
        });
    }

    [LoaderInitialize(1)]
    private static void OnInit()
    {
        ServerEvents.CommandExecuting += OnCommand;
    }
}