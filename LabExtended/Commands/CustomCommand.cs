using CommandSystem;

using LabExtended.API;
using LabExtended.API.Collections.Locked;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Contexts;

using LabExtended.Core.Commands;
using LabExtended.Core.Commands.Interfaces;
using LabExtended.Core.Commands.Responses;
using LabExtended.Extensions;
using LabExtended.Utilities;

using NorthwoodLib.Pools;

using PluginAPI.Events;

using System.Reflection;

namespace LabExtended.Commands
{
    public class CustomCommand : ICommand, IUsageProvider
    {
        internal static readonly LockedDictionary<uint, ContinuedContext> _continuedContexts = new LockedDictionary<uint, ContinuedContext>();

        private bool _isInitialized;

        private string _builtUsage;
        private string[] _usage;

        private ArgumentDefinition[] _customArgs;
        private ParameterInfo[] _customParams;
        private MethodInfo _customMethod;

        public virtual string Command { get; }
        public virtual string Description { get; }

        public virtual string[] Aliases { get; } = Array.Empty<string>();
        public virtual string[] Usage => _usage;

        public virtual bool SanitizeResponse => false;

        public virtual ArgumentDefinition[] Arguments => _customArgs;

        public virtual ICommandResponse CheckPreconditions(ExPlayer sender)
            => null;

        public virtual ICommandResponse CheckPermissions(ExPlayer sender)
            => null;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            try
            {
                if (!_isInitialized)
                    InitializeCommand();

                if (arguments.Count < Arguments.Count(a => !a.IsOptional))
                {
                    response = $"Missing command parameters.\n{GetUsage()}";
                    return false;
                }

                if (!ExPlayer.TryGet(sender, out var player))
                {
                    response = $"Command execution failed - cannot retrieve your player object.";
                    return false;
                }

                var permsResponse = CheckPermissions(player);

                if (permsResponse != null && !permsResponse.IsSuccess)
                {
                    response = $"Permission check failed: {permsResponse.Response}";
                    return false;
                }

                var preconditionsResponse = CheckPreconditions(player);

                if (preconditionsResponse != null && !preconditionsResponse.IsSuccess)
                {
                    response = $"Preconditions check failed: {preconditionsResponse.Response}";
                    return false;
                }

                var arg = string.Join(" ", arguments);

                var result = default(object);
                var success = false;

                if (_customMethod != null)
                {
                    var args = new object[_customParams.Length];

                    if (!CommandParser.TryParseCustomArgs(arg, this, args, out var failedArg, out var failedReason))
                    {
                        response = $"Failed while parsing command parameter '{failedArg.Name}': {failedReason}";
                        return false;
                    }
                    else
                    {
                        result = _customMethod.Invoke(this, args);
                        success = true;
                    }
                }
                else
                {
                    var collection = new ArgumentCollection();
                    var context = new CommandContext(arg, arguments.Array, collection, this, player);

                    if (!CommandParser.TryParseDefaultArgs(arg, this, collection, out var failedArg, out var failedReason))
                    {
                        response = $"Failed while parsing command parameter '{failedArg.Name}': {failedReason}";
                        return false;
                    }
                    else
                    {
                        OnCommand(player, context, collection);
                        result = context.Response;
                    }

                    var cmdResponse = (ICommandResponse)result;

                    if (cmdResponse is ContinuedResponse continuedResponse)
                        _continuedContexts[player.NetId] = new ContinuedContext(continuedResponse, context, context.RawInput, context.RawArgs);

                    response = cmdResponse.Response;
                    return cmdResponse.IsSuccess;
                }

                response = "Unknown error!";
                return false;
            }
            catch (Exception ex)
            {
                response = $"Command execution failed:\n{ex}";
                return false;
            }
        }

        public virtual void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args) { }

        private string GetUsage()
        {
            if (!string.IsNullOrWhiteSpace(_builtUsage))
                return _builtUsage;

            if (Usage is null)
            {
                _usage = new string[Arguments.Length];

                for (int i = 0; i < Arguments.Length; i++)
                    _usage[i] = $"[{Arguments[i].Name}]";
            }

            if (Arguments.Length < 1)
                return "";

            var builder = StringBuilderPool.Shared.Rent();

            for (int i = 0; i < Arguments.Length; i++)
            {
                var arg = Arguments[i];

                builder.Append($"[{i}] {arg.Name} [{arg.Description}]");

                if (arg.IsOptional)
                    builder.Append($"(optional, default value: {arg.Default?.ToString() ?? "null!"}");

                builder.Append("\n");
            }

            return _builtUsage = StringBuilderPool.Shared.ToStringReturn(builder);
        }

        private void InitializeCommand()
        {
            var type = GetType();
            var method = type.FindMethod(m =>
            {
                if (m.Name != "OnCommand")
                    return false;

                if (m.ReturnType != typeof(ICommandResponse))
                    return false;

                var parameters = m.GetAllParameters();

                if (parameters.Length == 3 && (parameters[0].ParameterType == typeof(ExPlayer) && parameters[1].ParameterType == typeof(ICommandContext) && parameters[2].ParameterType == typeof(ArgumentCollection)))
                    return false;

                return true;
            });

            if (method != null)
            {
                if (Arguments is null)
                {
                    var validParams = method.GetAllParameters().Skip(1).ToArray();
                    _customArgs = new ArgumentDefinition[validParams.Length];

                    for (int i = 0; i < validParams.Length; i++)
                        _customArgs[i] = ArgumentDefinition.FromParameter(validParams[i]);

                    ValidateArguments();
                }
                else
                {
                    ValidateArguments();
                }
            }
            else
            {
                _customArgs ??= Array.Empty<ArgumentDefinition>();
                ValidateArguments();
            }

            _isInitialized = true;
        }

        private void ValidateArguments()
        {
            if (Arguments is null)
                throw new Exception($"Command arguments were not defined.");

            for (int i = 0; i < Arguments.Length; i++)
                Arguments[i].ValidateArgument();
        }

        internal static bool InternalHandleGameConsoleCommand(PlayerGameConsoleCommandEvent ev)
        {
            if (!ExPlayer.TryGet(ev.Player, out var player))
                return true;

            if (!_continuedContexts.TryGetValue(player.NetId, out var continuedContext))
                return true;

            var ctx = new ContinuedContext(continuedContext.PreviousResponse, continuedContext, ev.Command, ev.Arguments);

            try
            {
                continuedContext.PreviousResponse._onContinued(ctx);
            }
            catch (Exception ex)
            {
                player.SendRemoteAdminMessage($"Command execution failed: {ex}", false);
            }

            if (ctx.Response is ContinuedResponse continuedResponse)
                _continuedContexts[player.NetId] = new ContinuedContext(continuedResponse, ctx, ev.Command, ev.Arguments);
            else
                _continuedContexts.Remove(player.NetId);

            player.SendRemoteAdminMessage(ctx.Response.Response, ctx.Response.IsSuccess, true, continuedContext.PreviousContext?.Command?.Command ?? string.Empty);
            return false;
        }

        internal static bool InternalHandleConsoleCommand(ConsoleCommandEvent ev)
        {
            if (!ExPlayer.TryGet(ev.Sender, out var player))
                return true;

            if (!_continuedContexts.TryGetValue(player.NetId, out var continuedContext))
                return true;

            var ctx = new ContinuedContext(continuedContext.PreviousResponse, continuedContext, ev.Command, ev.Arguments);

            try
            {
                continuedContext.PreviousResponse._onContinued(ctx);
            }
            catch (Exception ex)
            {
                player.SendRemoteAdminMessage($"Command execution failed: {ex}", false);
            }

            if (ctx.Response is ContinuedResponse continuedResponse)
                _continuedContexts[player.NetId] = new ContinuedContext(continuedResponse, ctx, ev.Command, ev.Arguments);
            else
                _continuedContexts.Remove(player.NetId);

            player.SendRemoteAdminMessage(ctx.Response.Response, ctx.Response.IsSuccess, true, continuedContext.PreviousContext?.Command?.Command ?? string.Empty);
            return false;
        }

        internal static bool InternalHandleRemoteAdminCommand(RemoteAdminCommandEvent ev)
        {
            if (!ExPlayer.TryGet(ev.Sender, out var player))
                return true;

            if (!_continuedContexts.TryGetValue(player.NetId, out var continuedContext))
                return true;

            var ctx = new ContinuedContext(continuedContext.PreviousResponse, continuedContext, ev.Command, ev.Arguments);

            try
            {
                continuedContext.PreviousResponse._onContinued(ctx);
            }
            catch (Exception ex)
            {
                player.SendRemoteAdminMessage($"Command execution failed: {ex}", false);
            }

            if (ctx.Response is ContinuedResponse continuedResponse)
                _continuedContexts[player.NetId] = new ContinuedContext(continuedResponse, ctx, ev.Command, ev.Arguments);
            else
                _continuedContexts.Remove(player.NetId);

            player.SendRemoteAdminMessage(ctx.Response.Response, ctx.Response.IsSuccess, true, continuedContext.PreviousContext?.Command?.Command ?? string.Empty);
            return false;
        }
    }
}