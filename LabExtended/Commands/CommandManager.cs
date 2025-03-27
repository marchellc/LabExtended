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
    public static List<CommandData> Commands { get; } = new(byte.MaxValue);

    /// <summary>
    /// Registers all commands found in a given assembly.
    /// </summary>
    /// <param name="assembly">The assembly to search for commands in.</param>
    /// <returns>List of registered commands.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static List<CommandData> RegisterCommands(this Assembly assembly)
    {
        if (assembly is null)
            throw new ArgumentNullException(nameof(assembly));
        
        var registered = new List<CommandData>();

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
            
            var instance = new CommandData(type, commandAttribute.Name, commandAttribute.Permission, commandAttribute.Description,
                commandAttribute.IsStatic,  commandAttribute.IsHidden, commandAttribute.TimeOut > 0f ? commandAttribute.TimeOut : null, commandAttribute.Aliases);

            if (instance is { SupportsPlayer: false, SupportsServer: false, SupportsRemoteAdmin: false })
            {
                ApiLog.Warn("Command Manager", $"Command &1{type.FullName}&r does not have any enabled input sources. " +
                                               $"You can enable those by adding one of the source interfaces to the command class" +
                                               $"(for example &1IRemoteAdminCommand&r, or for simplicity &1IAllCommand&r or &1IServerSideCommand&r)");
                
                continue;
            }
            
            instance.Path.AddRange(instance.Name.Split(spaceSeparator, StringSplitOptions.RemoveEmptyEntries).Select(x => x.ToLowerInvariant()));

            foreach (var method in type.GetAllMethods())
            {
                if (method.IsStatic)
                    continue;
                
                if (!method.HasAttribute<CommandOverloadAttribute>(out var commandOverloadAttribute))
                    continue;

                if (method.ReturnType != typeof(void) && method.ReturnType != typeof(IEnumerator<float>))
                {
                    ApiLog.Warn("Command Manager", $"Method &3{method.GetMemberName()}&r cannot be used as an overload " +
                                                   $"because it's return type is not supported (&1{method.ReturnType.FullName}&r)." +
                                                   $"Command method's should return only &1void&r or an &1IEnumerator<float>&r coroutine.");
                    continue;
                }

                var overload = new CommandOverload(method);
                
                overload.Name = commandOverloadAttribute.Name;
                overload.Description = commandOverloadAttribute.Description;

                if (commandOverloadAttribute.isDefaultOverload)
                {
                    if (instance.DefaultOverload != null)
                    {
                        ApiLog.Error("Command Manager", $"Method &1{method.GetMemberName()}&r in command &1{instance.Name}&r " +
                                                        $"was specified as the default overload, but the command already has one " +
                                                        $"(&3{instance.DefaultOverload.Target.GetMemberName()}&r)");
                        
                        continue;
                    }
                    
                    instance.DefaultOverload = overload;
                }
                else
                {
                    if (instance.Overloads.ContainsKey(commandOverloadAttribute.Name))
                    {
                        ApiLog.Error("Command Manager", $"Method &1{method.GetMemberName()}&r in command &1{instance.Name}&r" +
                                                        $"cannot be added as an overload because an overload with the same name already exists.");
                        
                        continue;
                    }

                    instance.Overloads.Add(commandOverloadAttribute.Name, overload);
                }
            }

            if (instance.DefaultOverload is null && instance.Overloads.Count == 0)
            {
                ApiLog.Warn("Command Manager", $"Command &1{type.FullName}&r does not have any overloads.");
                continue;
            }

            var commandInstance = instance.GetInstance();

            if (commandInstance != null)
            {
                if (instance.DefaultOverload != null)
                {
                    commandInstance.OnInitializeOverload(null, instance.DefaultOverload.ParameterBuilders);
                    instance.DefaultOverload.IsInitialized = true;
                }
                
                foreach (var overload in instance.Overloads)
                {
                    commandInstance.OnInitializeOverload(overload.Key, overload.Value.ParameterBuilders);
                    overload.Value.IsInitialized = true;
                }
                
                if (ApiLoader.ApiConfig.CommandSection.AllowInstancePooling && !instance.IsStatic)
                    instance.DynamicPool.Add(commandInstance);
            }
            else
            {
                ApiLog.Warn("Command Manager", $"Overloads of command &3{instance.Name}&r could not be initialized due to not being able to construct " +
                                               $"a command instance, this command may behave unexpectedly when first used.");
            }

            Commands.Add(instance);
            registered.Add(instance);
            
            ApiLog.Debug("Command Manager", $"Registered command &3{instance.Name}&r (&6{type.FullName}&r)");
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

            var args = ListPool<string>.Shared.Rent(ev.Arguments.Array);

            if (!TryGetCommand(args, ev.CommandType, out var command))
                return;

            ev.IsAllowed = false;

            if (command.Permission != null && !player.HasPermissions(command.Permission))
            {
                ev.Sender.Respond(CommandResponseFormatter.FormatMissingPermissionsFailure(command.Permission, command.Name, ev.CommandType));
                
                ListPool<string>.Shared.Return(args);
                return;
            }

            if (args.Count > 0 && command.Overloads.TryGetValue(args[0].ToLowerInvariant(), out var overload))
            {
                args.RemoveAt(0);
            }
            else
            {
                if (command.DefaultOverload is null)
                {
                    ev.Sender.Respond(CommandResponseFormatter.FormatUnknownOverloadFailure(command, args[0]));

                    ListPool<string>.Shared.Return(args);
                    return;
                }
                
                overload = command.DefaultOverload;
            }

            line = string.Join(" ", args);
            
            var tokens = ListPool<ICommandToken>.Shared.Rent();
            var context = new CommandContext();
            
            context.Args = args;
            context.Line = line;
            context.Sender = player;
            context.Tokens = tokens;
            context.Command = command;
            context.Overload = overload;

            context.Type = ev.CommandType;

            if (line?.Length > 0 && !CommandTokenParserUtils.TryParse(line, tokens, overload.ParameterCount))
            {
                context.Response = new(false, false, context.FormatTokenParserFailure());

                OnExecuted(context);
                return;
            }

            var parserResults = ListPool<CommandParameterParserResult>.Shared.Rent();
            var parserResult = CommandParameterParserUtils.ParseParameters(context, parserResults);

            if (!parserResult.Success)
            {
                if (!context.Response.HasValue)
                {
                    if (parserResult.Error is "MISSING_ARGS")
                        context.Response = new(false, false, context.FormatMissingArgumentsFailure());
                    else if (parserResult.Error is "INVALID_ARGS")
                        context.Response = new(false, false, context.FormatInvalidArgumentsFailure(parserResults));
                }
                
                OnExecuted(context);
                return;
            }

            if (parserResults.Count(r => r.Success) != overload.ParameterCount)
            {
                context.Response ??= new(false, false, context.FormatInvalidArgumentsFailure(parserResults));
                
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

        if (!ctx.Command.IsStatic && ApiLoader.ApiConfig.CommandSection.AllowInstancePooling && ctx.Instance != null)
            ctx.Command.DynamicPool.Add(ctx.Instance);
        
        Executed.InvokeSafe(ctx);

        if (ctx.Args != null)
            ListPool<string>.Shared.Return(ctx.Args);

        if (ctx.Tokens != null)
        {
            ctx.Tokens.ForEach(t => t.ReturnToken());
            
            ListPool<ICommandToken>.Shared.Return(ctx.Tokens);
        }

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
    
    internal static bool TryGetCommand(List<string> args, CommandType? commandType, out CommandData? foundCommand)
    {
        foundCommand = null;
        
        if (args.Count < 1)
            return false;

        for (var i = 0; i < Commands.Count; i++)
        {
            var command = Commands[i];

            if (commandType is CommandType.Client && !command.SupportsPlayer)
                continue;

            if (commandType is CommandType.Console && !command.SupportsServer)
                continue;
            
            if (commandType is CommandType.RemoteAdmin && !command.SupportsRemoteAdmin)
                continue;
            
            if (args.Count < command.Path.Count)
                continue;

            var matched = true;

            for (var x = 0; x < command.Path.Count; x++)
            {
                if (string.Equals(command.Path[x], args[x], StringComparison.OrdinalIgnoreCase))
                    continue;

                if (x + 1 >= command.Path.Count
                    && command.Aliases.Any(alias => string.Equals(alias, args[x], StringComparison.OrdinalIgnoreCase)))
                    continue;
                
                matched = false;
                break;
            }

            if (matched)
            {
                args.RemoveRange(0, command.Path.Count);
                
                foundCommand = command;
                return true;
            }
        }

        return false;
    }

    [LoaderInitialize(1)]
    private static void OnInit()
    {
        ServerEvents.CommandExecuting += OnCommand;
    }
}