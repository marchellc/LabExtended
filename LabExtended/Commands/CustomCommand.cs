using CommandSystem;

using LabExtended.API;
using LabExtended.API.Collections.Locked;

using LabExtended.Commands.Arguments;
using LabExtended.Commands.Contexts;
using LabExtended.Commands.Interfaces;
using LabExtended.Commands.Responses;

using NorthwoodLib.Pools;

using System.Reflection;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabExtended.Attributes;
using LabExtended.Extensions;

namespace LabExtended.Commands
{
    public abstract class CustomCommand : ICommand, IUsageProvider
    {
        internal static readonly LockedDictionary<uint, ContinuedContext> _continuedContexts = new LockedDictionary<uint, ContinuedContext>();

        private bool _isInitialized = false;

        private string _builtUsage = string.Empty;
        private string[] _usage;

        internal ArgumentDefinition[] _args;

        private ParameterInfo[] _customParams;
        private MethodInfo _customMethod;

        public abstract string Command { get; }

        public virtual string Description { get; } = "No description.";

        public virtual string[] Aliases { get; } = Array.Empty<string>();
        public virtual string[] Usage => _usage;

        public virtual ArgumentDefinition[] BuildArgs()
            => Array.Empty<ArgumentDefinition>();

        public virtual ICommandResponse CheckPreconditions(ExPlayer sender)
            => null;

        public virtual ICommandResponse CheckPermissions(ExPlayer sender)
            => null;

        public virtual void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args) { }

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            try
            {
                if (!_isInitialized)
                    InitializeCommand();

                if (arguments.Count < _args.Count(a => !a.IsOptional))
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

                    if (!CommandParser.TryParseCustomArgs(arg, _args, args, out var failedArg, out var failedReason))
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

                    if (!CommandParser.TryParseDefaultArgs(arg, _args, collection, out var failedArg, out var failedReason))
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
                    else
                        collection.Dispose();

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

        private string GetUsage()
        {
            if (!string.IsNullOrWhiteSpace(_builtUsage))
                return _builtUsage;

            if (Usage is null)
            {
                _usage = new string[_args.Length];

                for (int i = 0; i < _args.Length; i++)
                    _usage[i] = $"[{_args[i].Name}]";
            }

            if (_args.Length < 1)
                return "";

            var builder = StringBuilderPool.Shared.Rent();

            for (int i = 0; i < _args.Length; i++)
            {
                var arg = _args[i];

                builder.Append($"[{i}] {arg.Name} [{arg.Description}]");

                if (arg.IsOptional)
                    builder.Append($" (optional, default value: {arg.Default?.ToString() ?? "null!"})");

                builder.Append("\n");
            }

            return _builtUsage = StringBuilderPool.Shared.ToStringReturn(builder);
        }

        private void InitializeCommand()
        {
            if (_isInitialized)
                return;

            if (_args is null)
            {
                _args = BuildArgs();
                ValidateArguments();
            }

            _isInitialized = true;
        }

        private void ValidateArguments()
        {
            if (_args is null)
                throw new Exception($"Command arguments were not defined.");

            for (int i = 0; i < _args.Length; i++)
                _args[i].ValidateArgument();
        }

        #region Argument Building
        public static ArgumentDefinition[] GetArg<T>(string name, string description, ICommandParser parser = null)
            => ArgumentBuilder.Get(x => x.WithArg<T>(name, description, parser));

        public static ArgumentDefinition[] GetArg<T>(string name, ICommandParser parser = null)
            => ArgumentBuilder.Get(x => x.WithArg<T>(name, parser));

        public static ArgumentDefinition[] GetOptionalArg<T>(string name, string description, T defaultValue = default, ICommandParser parser = null)
            => ArgumentBuilder.Get(x => x.WithOptional<T>(name, description, defaultValue, parser));

        public static ArgumentDefinition[] GetOptionalArg<T>(string name, T defaultValue = default, ICommandParser parser = null)
            => ArgumentBuilder.Get(x => x.WithOptional<T>(name, defaultValue, parser));

        public static ArgumentDefinition[] GetArgs(Action<ArgumentBuilder> builder)
            => ArgumentBuilder.Get(builder);
        #endregion

        #region Continued Response Handling
        private static void HandleCommand(CommandExecutingEventArgs args)
        {
            if (!ExPlayer.TryGet(args.Sender, out var player))
                return;

            if (!_continuedContexts.TryGetValue(player.NetId, out var continuedContext))
                return;

            args.IsAllowed = false;
            
            var ctx = new ContinuedContext(continuedContext.PreviousResponse, continuedContext,
                args.Arguments.AsString(" "), args.Arguments.Array);

            try
            {
                continuedContext.PreviousResponse._onContinued(ctx);
            }
            catch (Exception ex)
            {
                player.SendRemoteAdminMessage($"Command execution failed: {ex}", false);
            }

            if (ctx.Response is ContinuedResponse continuedResponse)
                _continuedContexts[player.NetId] = new ContinuedContext(continuedResponse, ctx, ctx.RawInput, ctx.RawArgs);
            else
                _continuedContexts.Remove(player.NetId);

            player.SendRemoteAdminMessage(ctx.Response.Response, ctx.Response.IsSuccess, true, continuedContext.PreviousContext?.Command?.Command ?? string.Empty);
        }

        [LoaderInitialize(1)]
        private static void RegisterEvents()
            => ServerEvents.CommandExecuting += HandleCommand;
        #endregion
    }
}