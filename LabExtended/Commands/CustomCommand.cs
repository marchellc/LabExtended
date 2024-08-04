﻿using CommandSystem;

using LabExtended.API;
using LabExtended.API.Collections.Locked;

using LabExtended.Commands.Arguments;
using LabExtended.Commands.Contexts;
using LabExtended.Commands.Interfaces;
using LabExtended.Commands.Responses;

using LabExtended.Extensions;

using NorthwoodLib.Pools;

using PluginAPI.Events;

using System.Reflection;

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

        public virtual void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args) { }

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
                if (_args is null)
                {
                    var validParams = method.GetAllParameters().Skip(1).ToArray();

                    _args = new ArgumentDefinition[validParams.Length];

                    for (int i = 0; i < validParams.Length; i++)
                        _args[i] = ArgumentDefinition.FromParameter(validParams[i]);

                    ValidateArguments();
                }
                else
                {
                    ValidateArguments();
                }
            }
            else
            {
                _args ??= BuildArgs();

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
        internal static bool HandleCommand(ExPlayer player, string cmd, string[] args)
        {
            if (player is null)
                return true;

            if (!_continuedContexts.TryGetValue(player.NetId, out var continuedContext))
                return true;

            var ctx = new ContinuedContext(continuedContext.PreviousResponse, continuedContext, cmd, args);

            try
            {
                continuedContext.PreviousResponse._onContinued(ctx);
            }
            catch (Exception ex)
            {
                player.SendRemoteAdminMessage($"Command execution failed: {ex}", false);
            }

            if (ctx.Response is ContinuedResponse continuedResponse)
                _continuedContexts[player.NetId] = new ContinuedContext(continuedResponse, ctx, cmd, args);
            else
                _continuedContexts.Remove(player.NetId);

            player.SendRemoteAdminMessage(ctx.Response.Response, ctx.Response.IsSuccess, true, continuedContext.PreviousContext?.Command?.Command ?? string.Empty);
            return false;
        }

        internal static bool InternalHandleGameConsoleCommand(PlayerGameConsoleCommandEvent ev)
            => HandleCommand(ev.Player.ReferenceHub, ev.Command, ev.Arguments);

        internal static bool InternalHandleConsoleCommand(ConsoleCommandEvent ev)
            => HandleCommand(ExPlayer.Get(ev.Sender), ev.Command, ev.Arguments);

        internal static bool InternalHandleRemoteAdminCommand(RemoteAdminCommandEvent ev)
            => HandleCommand(ExPlayer.Get(ev.Sender), ev.Command, ev.Arguments);
        #endregion
    }
}