﻿using CommandSystem;
using CommandSystem.Commands.RemoteAdmin;

using HarmonyLib;

using LabExtended.API;
using LabExtended.API.Internal;
using LabExtended.Attributes;
using LabExtended.Core.Hooking;
using LabExtended.Events.Player;

namespace LabExtended.Patches.Events
{
    public static class PlayerTogglingRoundLockPatch
    {
        [HookPatch(typeof(PlayerTogglingRoundLockArgs))]
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
                    var args = new PlayerTogglingRoundLockArgs(player, !state);

                    if (!HookRunner.RunEvent(args, true))
                    {
                        response = $"Round Lock change prevented by a plugin.";
                        
                        __result = false;
                        return false;
                    }

                    if (args.NewState)
                        ExRound.RoundLock = new RoundLock(player);
                    else
                        ExRound.RoundLock = null;

                    RoundSummary.RoundLock = ExRound.IsRoundLocked;

                    response = $"Round Lock {(args.NewState ? "enabled" : "disabled")}.";
                }
            }
            else
            {
                var args = new PlayerTogglingRoundLockArgs(player, ExRound.IsRoundLocked);

                if (!HookRunner.RunEvent(args, true))
                {
                    response = $"Round Lock change prevented by a plugin.";
                    
                    __result = false;
                    return false;
                }

                if (args.NewState)
                    ExRound.RoundLock = new RoundLock(player);
                else
                    ExRound.RoundLock = null;

                response = $"Round Lock {(args.NewState ? "enabled" : "disabled")}.";
            }

            __result = true;
            return false;
        }
    }
}