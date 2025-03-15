using HarmonyLib;

using InventorySystem.Items.Usables.Scp330;

using LabExtended.API;
using LabExtended.Attributes;

using LabExtended.Events;
using LabExtended.Events.Player;

namespace LabExtended.Patches.Events.Player
{
    public static class PlayerReceivingCandyPatch
    {
        [EventPatch(typeof(PlayerReceivingCandyEventArgs))]
        [HarmonyPatch(typeof(Scp330Bag), nameof(Scp330Bag.TryAddSpecific))]
        public static bool Prefix(Scp330Bag __instance, CandyKindID kind, ref bool __result)
        {
            if (!ExPlayer.TryGet(__instance.Owner, out var player))
                return true;

            if (__instance.Candies.Count + 1 > 6)
                return __result = false;

            var receivingArgs = new PlayerReceivingCandyEventArgs(player, kind);

            if (!ExPlayerEvents.OnReceivingCandy(receivingArgs) || receivingArgs.CandyType is CandyKindID.None)
                return __result = false;

            __instance.Candies.Add(receivingArgs.CandyType);

            __result = true;
            return false;
        }
    }
}