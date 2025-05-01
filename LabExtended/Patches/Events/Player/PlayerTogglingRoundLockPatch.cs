using CommandSystem;
using CommandSystem.Commands.RemoteAdmin;

using HarmonyLib;

using LabExtended.API;
using LabExtended.Attributes;

using LabExtended.Events;
using LabExtended.Events.Player;

namespace LabExtended.Patches.Events.Player
{
    public static class PlayerTogglingRoundLockPatch
    {
        [EventPatch(typeof(PlayerTogglingRoundLockEventArgs), true)]
        [HarmonyPatch(typeof(RoundLockCommand), nameof(RoundLockCommand.Execute))]
        public static bool Prefix(ArraySegment<string> arguments, ICommandSender sender, out string response, ref bool __result)
        {
            if (!sender.CheckPermission(PlayerPermissions.RoundEvents, out response))
                return __result = false;

            if (!ExPlayer.TryGet(sender, out var player))
                return true;

            if (arguments.Count >= 1)
            {
                var newArgs = new string[1] { arguments.Array[1] };
                var state = ExRound.IsRoundLocked;

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

                if (state == ExRound.IsRoundLocked)
                {
                    response = $"Round Lock is already {(state ? "enabled" : "disabled")}.";
                }
                else
                {
                    var args = new PlayerTogglingRoundLockEventArgs(player, state);

                    if (!ExPlayerEvents.OnTogglingRoundLock(args))
                    {
                        response = $"Round Lock change prevented by a plugin.";
                        
                        __result = false;
                        return false;
                    }

                    if (args.IsEnabled)
                        ExRound.RoundLock.Enable(player);
                    else
                        ExRound.RoundLock.Disable(player);

                    response = $"Round Lock {(args.IsEnabled ? "enabled" : "disabled")}.";
                }
            }
            else
            {
                var args = new PlayerTogglingRoundLockEventArgs(player, !ExRound.IsRoundLocked);

                if (!ExPlayerEvents.OnTogglingRoundLock(args))
                {
                    response = $"Round Lock change prevented by a plugin.";
                    
                    __result = false;
                    return false;
                }
                
                if (args.IsEnabled)
                    ExRound.RoundLock.Enable(player);
                else
                    ExRound.RoundLock.Disable(player);

                response = $"Round Lock {(args.IsEnabled ? "enabled" : "disabled")}.";
            }

            __result = true;
            return false;
        }
    }
}