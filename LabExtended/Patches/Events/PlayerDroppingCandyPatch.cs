using HarmonyLib;

using InventorySystem.Items.Usables.Scp330;

using LabExtended.API;
using LabExtended.Core.Hooking;
using LabExtended.Events.Player;

namespace LabExtended.Patches.Events
{
    [HarmonyPatch(typeof(Scp330Bag), nameof(Scp330Bag.TryRemove))]
    public static class PlayerDroppingCandyPatch
    {
        public static bool Prefix(Scp330Bag __instance, int index, ref CandyKindID __result)
        {
            if (!ExPlayer.TryGet(__instance.Owner, out var player))
                return true;

            if (index < 0 || index >= player.Inventory.CandyBag._candies.Count)
            {
                __result = CandyKindID.None;
                return false;
            }

            if (!HookRunner.RunEvent(new PlayerDroppingCandyArgs(player, player.Inventory.CandyBag.Candies[index]), true))
            {
                __result = CandyKindID.None;
                return false;
            }

            __result = __instance.Candies[index];

            __instance.Candies.RemoveAt(index);
            __instance.ServerRefreshBag();

            player.Inventory.CandyBag._candies.RemoveWhere(x => x.Index == index);
            return false;
        }
    }
}
