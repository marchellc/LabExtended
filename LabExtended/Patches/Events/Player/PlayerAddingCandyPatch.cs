using HarmonyLib;

using InventorySystem.Items.Usables.Scp330;

using LabExtended.API;
using LabExtended.API.Items.Candies;
using LabExtended.Attributes;
using LabExtended.Core.Hooking;
using LabExtended.Events.Player;

namespace LabExtended.Patches.Events
{
    public static class PlayerAddingCandyPatch
    {
        [HookPatch(typeof(PlayerAddingCandyArgs))]
        [HarmonyPatch(typeof(Scp330Bag), nameof(Scp330Bag.TryAddSpecific))]
        public static bool Prefix(Scp330Bag __instance, CandyKindID kind, ref bool __result)
        {
            if (!ExPlayer.TryGet(__instance.Owner, out var player))
                return true;

            var addingArgs = new PlayerAddingCandyArgs(player, kind, __instance.Candies.Count < 6);

            if (!HookRunner.RunEvent(addingArgs, true) || !addingArgs.CanAdd || addingArgs.CandyType == CandyKindID.None)
                return __result = false;

            __instance.Candies.Add(addingArgs.CandyType);

            player.Inventory.CandyBag._candies.Add(new CandyItem(player.Inventory.CandyBag, __instance.Candies.Count));

            __result = true;
            return false;
        }
    }
}