using HarmonyLib;
using System.Reflection.Emit;
using System.Reflection;

using CentralAuth;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Wrappers;

using LabExtended.API;
using LabExtended.Core;
using LabExtended.Events;

namespace LabExtended.Patches.Functions.Players;

public static class RegularPlayerPatch
{
    [HarmonyPatch(typeof(Player), nameof(Player.AddPlayer))]
    public static bool JoinPrefix(ReferenceHub referenceHub)
    {
        if (referenceHub != null && !referenceHub.isLocalPlayer)
            _ = new ExPlayer(referenceHub);

        return false;
    }

    [HarmonyPatch(typeof(PlayerAuthenticationManager), nameof(PlayerAuthenticationManager.ProcessAuthenticationResponse), [typeof(AuthenticationResponse)])]
    static IEnumerable<CodeInstruction> InvokePlayerJoinTranspiler(IEnumerable<CodeInstruction> instructions) {
        var code = new List<CodeInstruction>(instructions);

        for (int i = 0; i < code.Count; i++) {
            if (code[i].opcode == OpCodes.Newobj &&
               code[i].operand as ConstructorInfo == AccessTools.Constructor(typeof(PlayerJoinedEventArgs), [typeof(ReferenceHub)])) {
                code.InsertRange(i, [
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldfld, AccessTools.Field(typeof(PlayerAuthenticationManager), nameof(PlayerAuthenticationManager._hub))),
                    new(OpCodes.Call, AccessTools.Method(typeof(ExPlayer), nameof(ExPlayer.Get), [typeof(ReferenceHub)])),
                    new(OpCodes.Call, AccessTools.Method(typeof(InternalEvents), nameof(InternalEvents.HandlePlayerJoin), [typeof(ExPlayer)]))
                ]);
                return code;
            }
        }

        ApiLog.Error("Couldn't find Insertion index in " + nameof(InvokePlayerJoinTranspiler));
        return code;
    }

    [HarmonyPatch(typeof(Player), nameof(Player.RemovePlayer))]
    public static bool LeavePrefix(ReferenceHub referenceHub)
    {
        if (referenceHub != null && !referenceHub.isLocalPlayer && ExPlayer.TryGet(referenceHub, out var player) && player != null)
        {
            if (referenceHub.authManager.UserId != null)
                Player.UserIdCache.Remove(referenceHub.authManager.UserId);
            
            Player.Dictionary.Remove(referenceHub);
            
            InternalEvents.HandlePlayerLeave(player);
            
            player.Dispose();
        }

        return false;
    }
}