using HarmonyLib;

using InventorySystem.Items.Usables.Scp330;

using LabExtended.API;
using LabExtended.Attributes;

using LabExtended.Events;
using LabExtended.Events.Player;

namespace LabExtended.Patches.Events.Player
{
    public static class PlayerDroppingCandyPatch
    {
        [EventPatch(typeof(PlayerDroppingCandyEventArgs))]
        [HarmonyPatch(typeof(Scp330Bag), nameof(Scp330Bag.TryRemove))]
        public static bool Prefix(Scp330Bag __instance, int index, ref CandyKindID __result)
        {
            if (!ExPlayer.TryGet(__instance.Owner, out var player))
                return true;

            if (index < 0 || index >= __instance.Candies.Count)
            {
                __result = CandyKindID.None;
                return false;
            }

            var droppingArgs = new PlayerDroppingCandyEventArgs(player, __instance, index, __instance.Candies[index]);

            if (!ExPlayerEvents.OnDroppingCandy(droppingArgs))
            {
                __result = CandyKindID.None;
                return false;
            }

            if (droppingArgs.Index < 0 || droppingArgs.Index >= __instance.Candies.Count)
            {
                __result = CandyKindID.None;
                return false;
            }
            

            __instance.Candies.RemoveAt(index);
            __instance.ServerRefreshBag();

            __result = droppingArgs.Type;
            return false;
        }
    }
}
