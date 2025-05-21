using HarmonyLib;

using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;

using LabApi.Features.Enums;

using RemoteAdmin;

namespace LabExtended.Patches.Fixes;

/// <summary>
/// Fixes custom commands for player consoles resulting in "Command not found" by removing a check for CommandFound.
/// </summary>
public static class ClientCommandFoundPatch
{
    [HarmonyPatch(typeof(QueryProcessor), nameof(QueryProcessor.ProcessGameConsoleQuery))]
    private static bool Prefix(QueryProcessor __instance, string query)
    {
        var args = query.Trim().Split(QueryProcessor.SpaceArray, 512, StringSplitOptions.RemoveEmptyEntries);
        var found = QueryProcessor.DotCommandHandler.TryGetCommand(args[0], out var command);

        var executingArgs =
            new CommandExecutingEventArgs(__instance._sender, CommandType.Client, found, command, args.Segment(1));

        ServerEvents.OnCommandExecuting(executingArgs);

        if (!executingArgs.IsAllowed)
            return false;

        if (executingArgs.CommandFound)
        {
            var response = string.Empty;
            var result = false;
            var color = "green";

            try
            {
                if (found)
                {
                    result = command.Execute(executingArgs.Arguments, __instance._sender, out response);
                    response = Misc.CloseAllRichTextTags(response);
                    color = result ? "green" : "red";
                }
                else
                {
                    result = false;
                    response = "Command not found.";
                    color = "red";
                }
            }
            catch (Exception ex)
            {
                response = $"Command execution failed! Error: {ex}";
                result = false;
                color = "magenta";
            }

            __instance._hub.gameConsoleTransmission.SendToClient(response, color);

            ServerEvents.OnCommandExecuted(new(__instance._sender, CommandType.Client, command, executingArgs.Arguments,
                result, response));
            return false;
        }

        __instance._hub.gameConsoleTransmission.SendToClient("Command not found.", "red");

        ServerEvents.OnCommandExecuted(new(__instance._sender, CommandType.Client, null, executingArgs.Arguments,
            false, "Command not found."));
        return false;
    }
}