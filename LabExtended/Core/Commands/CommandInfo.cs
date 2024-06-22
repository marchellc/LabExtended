using CommandSystem;

using Common.Extensions;
using Common.Results;

using LabExtended.API;
using LabExtended.Core.Commands.Arguments;
using LabExtended.Core.Commands.Attributes;
using LabExtended.Core.Commands.Interfaces;

using RemoteAdmin;

using System.Reflection;

namespace LabExtended.Core.Commands
{
    public abstract class CommandInfo : ICommand
    {
        private readonly MethodInfo _onCall;
        private readonly object[] _emptyArgs;
        private readonly ParameterInfo[] _parameters;

        private bool _isInitialized;

        public CommandInfo()
        {
            _onCall = TryFindMethod();
            _parameters = _onCall.Parameters();
            _emptyArgs = new object[] { };
        }

        public abstract string Command { get; }
        public abstract string Description { get; }

        public virtual string[] Aliases { get; } = Array.Empty<string>();

        public virtual ICommandArgument[] Arguments { get; set; }

        public virtual bool SanitizeResponse => false;

        public virtual bool CheckPermissions(ExPlayer player, out string message)
        {
            message = null;
            return true;
        }

        public virtual bool CheckConditions(ExPlayer player, out string message)
        {
            message = null;
            return true;
        }

        public virtual void OnInitialized() { }

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            try
            {
                if (!_isInitialized)
                {
                    if (_parameters.Length > 1 && (Arguments is null || Arguments.Length != (_parameters.Length - 1)))
                        DetectArguments();

                    OnInitialized();

                    if (Arguments != null)
                    {
                        for (int i = 0; i < Arguments.Length; i++)
                            Arguments[i].Parameter = _parameters[i + 1];
                    }

                    _isInitialized = true;
                }

                var player = default(ExPlayer);

                if (sender is PlayerCommandSender playerCommandSender)
                    player = ExPlayer.Get(playerCommandSender.ReferenceHub);
                else
                    player = ExPlayer.Host;

                if (player is null)
                {
                    response = "Command execution failed - unable to find your player object.";
                    return false;
                }

                if (!CheckPermissions(player, out var message))
                {
                    response = $"You don't have permissions to use this command.\n{message}";
                    return false;
                }

                if (!CheckConditions(player, out var conditionMessage))
                {
                    response = $"You don't meet the command's conditions.\n{conditionMessage}";
                    return false;
                }

                var argString = string.Join(" ", arguments);
                var parsedArgs = new object[_parameters.Length];

                parsedArgs[0] = player;

                if (Arguments != null)
                {
                    var requiredArgs = Arguments.Count(arg => !arg.IsOptional);

                    if (string.IsNullOrWhiteSpace(argString) && requiredArgs > 0)
                    {
                        response = BuildUsageResponse();
                        return false;
                    }

                    if (!CommandUtils.TryParseArgs(argString, this, parsedArgs, out var failedArg, out var failedReason))
                    {
                        if (failedArg != null)
                            response = $"Failed while parsing argument '{failedArg.Name}': {failedReason}";
                        else
                            response = $"Failed while parsing arguments: {failedReason}";

                        return false;
                    }
                }

                var returnedValue = _onCall.Invoke(this, parsedArgs);

                if (returnedValue is null)
                {
                    response = "Command succesfully executed, but did not return anything.";
                    return true;
                }

                if (returnedValue is string responseStr)
                {
                    response = responseStr;
                    return true;
                }

                if (returnedValue is IResult result)
                {
                    if (!result.IsSuccess)
                    {
                        response = result.ReadErrorMessage();
                        return false;
                    }
                    else
                    {
                        response = result.TryReadValue<string>(out responseStr) ? responseStr : "Command did not return a response.";
                        return true;
                    }
                }

                if (returnedValue is IEnumerable<string> lines)
                {
                    response = string.Join("\n", lines);
                    return true;
                }

                if (returnedValue is KeyValuePair<string, bool> returnPair)
                {
                    response = returnPair.Key;
                    return returnPair.Value;
                }

                if (returnedValue is Tuple<string, bool> returnTuple)
                {
                    response = returnTuple.Item1;
                    return returnTuple.Item2;
                }

                response = $"Command returned an unsupported type: {returnedValue.GetType().FullName}";
                return true;
            }
            catch (Exception ex)
            {
                response = $"Command execution failed.\n{ex}";
                return false;
            }
        }

        public void DetectArguments()
        {
            if (_parameters.Length == 1)
            {
                Arguments = Array.Empty<ICommandArgument>();
                return;
            }

            var array = new ICommandArgument[_parameters.Length - 1];

            for (int i = 1; i < _parameters.Length; i++)
            {
                var parameter = _parameters[i];
                var attributes = parameter.GetCustomAttributes();

                var name = parameter.Name;
                var description = string.Empty;
                var type = parameter.ParameterType;
                var defaultValue = parameter.DefaultValue;

                var argument = new CommandArgument(name, description, type, defaultValue, null);

                if (attributes.TryGetFirst<RemainderAttribute>(out var remainderAttribute))
                    argument.IsRemainder = true;

                if (attributes.TryGetFirst<OptionalAttribute>(out var optionalAttribute))
                {
                    argument.IsOptional = true;
                    argument.DefaultValue = optionalAttribute.DefaultValue;
                }

                array[i - 1] = argument;
            }

            Arguments = array;
        }

        internal string BuildUsageResponse()
        {
            var str = $"Command '{Command}' (aliases: {(Aliases != null && Aliases.Length > 0 ? string.Join(",", Aliases) : "none")})\n";
            var usageStr = $"Usage: {Command}";

            if (Arguments != null)
            {
                for (int i = 0; i < Arguments.Length; i++)
                {
                    str += $"Argument [{i}]: ";

                    var arg = Arguments[i];

                    if (arg.IsOptional)
                        str += $"(optional - default value: {arg.DefaultValue ?? "null"})";

                    str += $"{arg.Name} ({arg.Description})\n";
                    usageStr += arg.IsOptional ? $" ({arg.Name})" : $" <{arg.Name}>";
                }
            }

            str += $"\n{usageStr}";
            return str;
        }

        /* Method example
        public object OnCalled(ExPlayer sender)
        {

        }
        */

        private MethodInfo TryFindMethod()
        {
            var type = GetType();
            var method = type.Method("OnCalled");

            if (method is null)
                throw new MethodAccessException($"Failed to find command method 'OnCalled' in class {type.FullName}");

            return method;
        }
    }
}