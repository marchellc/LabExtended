using CommandSystem;

using LabExtended.Extensions;

using RemoteAdmin;

using System.Reflection;

namespace LabExtended.Core
{
    public static class ApiCommands
    {
        public static ICommandHandler RemoteAdmin => CommandProcessor.RemoteAdminCommandHandler;
        public static ICommandHandler GameConsole => GameCore.Console.singleton.ConsoleCommandHandler;
        public static ICommandHandler PlayerConsole => QueryProcessor.DotCommandHandler;

        public static void RegisterCommands(Assembly assembly)
        {
            if (assembly is null)
                throw new ArgumentNullException(nameof(assembly));

            foreach (var type in assembly.GetTypes())
            {
                if (type.IsClass && typeof(ICommand).IsAssignableFrom(type))
                {
                    ICommand command = null;

                    foreach (var commandAttribute in type.GetCustomAttributesData())
                    {
                        if (commandAttribute.AttributeType != typeof(CommandHandlerAttribute))
                            continue;

                        command ??= (ICommand)Activator.CreateInstance(type);

                        RegisterCommand((Type)commandAttribute.ConstructorArguments[0].Value, command);
                    }
                }
            }
        }

        public static void RegisterCommand(Type handlerType, ICommand command)
        {
            if (handlerType is null)
                throw new ArgumentNullException(nameof(handlerType));

            if (handlerType == typeof(RemoteAdminCommandHandler))
                RegisterCommand(RemoteAdmin, command);
            else if (handlerType == typeof(GameConsoleCommandHandler))
                RegisterCommand(GameConsole, command);
            else if (handlerType == typeof(ClientCommandHandler))
                RegisterCommand(PlayerConsole, command);
            else
                throw new Exception($"Unknown command handler: {handlerType.FullName}");
        }

        public static void RegisterCommand(ICommandHandler handler, ICommand command)
        {
            if (handler is null)
                throw new ArgumentNullException(nameof(handler));

            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (handler.AllCommands.TryGetFirst(x => x.Command == command.Command || (x.Aliases != null && command.Aliases != null && x.Aliases.Any(y => command.Aliases.Contains(y))), out var similarCommand))
                handler.UnregisterCommand(similarCommand);

            handler.RegisterCommand(command);
        }

        internal static void InternalRegisterCommands()
            => RegisterCommands(typeof(ApiCommands).Assembly);
    }
}