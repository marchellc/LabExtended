using CommandSystem;
using CommandSystem.Commands.RemoteAdmin;

using GameCore;

using HarmonyLib;

using LabExtended.API;
using LabExtended.API.Internal;
using LabExtended.Attributes;
using LabExtended.Core.Hooking;
using LabExtended.Events.Player;

namespace LabExtended.Patches.Events
{
    public static class PlayerTogglingLobbyLockPatch
    {
        [HookPatch(typeof(PlayerTogglingLobbyLockArgs))]
        [HarmonyPatch(typeof(LobbyLockCommand), nameof(LobbyLockCommand.Execute))]
        public static bool Prefix(ArraySegment<string> arguments, ICommandSender sender, out string response, ref bool __result)
        {
            if (!sender.CheckPermission(PlayerPermissions.RoundEvents, out response))
                return __result = false;

            if (!ExPlayer.TryGet(sender, out var player))
                return true;

            if (arguments.Count >= 1)
            {
                var newArgs = new string[1] { arguments.Array[1] };
                var state = ExRound.IsLobbyLocked;

                if (Misc.TryCommandModeFromArgs(ref newArgs, out var commandOperationMode))
                {
                    switch (commandOperationMode)
                    {
                        case Misc.CommandOperationMode.Enable:
                            state = true;
                            break;

                        case Misc.CommandOperationMode.Disable:
                            state = false;
                            break;

                        case Misc.CommandOperationMode.Toggle:
                            state = !state;
                            break;
                    }
                }

                if (state == ExRound.IsLobbyLocked)
                {
                    response = $"Lobby Lock is already {(state ? "enabled" : "disabled")}.";
                    __result = true;
                    return false;
                }
                else
                {
                    var args = new PlayerTogglingLobbyLockArgs(player, !state);

                    if (!HookRunner.RunEvent(args, true))
                    {
                        response = $"Lobby Lock change prevented by a plugin.";
                        __result = false;
                        return false;
                    }

                    if (args.NewState)
                        ExRound.LobbyLock = new RoundLock(player);
                    else
                        ExRound.LobbyLock = null;

                    RoundStart.LobbyLock = ExRound.IsLobbyLocked;

                    response = $"Lobby Lock {(args.NewState ? "enabled" : "disabled")}.";
                    __result = true;
                    return false;
                }
            }
            else
            {
                var args = new PlayerTogglingLobbyLockArgs(player, ExRound.IsLobbyLocked);

                if (!HookRunner.RunEvent(args, true))
                {
                    response = $"Lobby Lock change prevented by a plugin.";
                    __result = false;
                    return false;
                }

                if (args.NewState)
                    ExRound.LobbyLock = new RoundLock(player);
                else
                    ExRound.LobbyLock = null;

                RoundStart.LobbyLock = ExRound.IsLobbyLocked;

                response = $"Lobby Lock {(args.NewState ? "enabled" : "disabled")}.";
                __result = true;
                return false;
            }
        }
    }
}