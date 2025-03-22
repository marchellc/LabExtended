using CommandSystem;
using CommandSystem.Commands.Shared;

using HarmonyLib;

using LabApi.Features.Enums;

using LabExtended.Commands;
using LabExtended.Commands.Utilities;

namespace LabExtended.Patches.Functions.RemoteAdmin;

/// <summary>
/// Inserts custom commands into the help command.
/// </summary>
public static class RemoteAdminHelpPatch
{
    [HarmonyPatch(typeof(HelpCommand), nameof(HelpCommand.Execute))]
    public static bool Prefix(HelpCommand __instance, ArraySegment<string> arguments, ICommandSender sender, 
        out string response, ref bool __result)
    {
        if (arguments.Count < 1)
        {
            response = __instance.GetCommandList(__instance._commandHandler, "Commands:");
            
            CommandFormatter.AppendCommands(ref response);

            __result = true;
            return false;
        }

        if (__instance._commandHandler.TryGetCommand(arguments.At(0), out var command))
        {
            var name = command.Command;
            var segment = arguments.Segment(1);

            while (segment.Count != 0 && command is ICommandHandler commandHandler &&
                   commandHandler.TryGetCommand(segment.At(0), out command))
            {
                segment = segment.Segment(1);
                name += $" {command.Command}";
            }

            if (command is IHelpProvider helpProvider)
                name += $" - {helpProvider.GetHelp(arguments)}";
            else
                name += $" - {command.Description}";

            if (command.Aliases != null)
                name += $"\nAliases: {string.Join(", ", command.Aliases)}";
            
            if (command is ICommandHandler handler)
                name += __instance.GetCommandList(handler, "\nSubcommands:");

            try
            {
                var cmdType = command.GetType();
                
                name += $"\nImplemented in {cmdType.Assembly.GetName().Name}:{cmdType.FullName}";
            }
            catch { }

            response = name;

            __result = true;
            return false;
        }
        else
        {
            if (CommandManager.TryFindCommand(string.Join(" ", arguments), null, out var foundCommand)
                && !foundCommand.IsHidden)
            {
                response = foundCommand.GetString();

                __result = true;
                return false;
            }
            else
            {
                response = $"Unknown command!";
                return __result = false;
            }
        }
    }
}