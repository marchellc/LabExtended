using HarmonyLib;

using InventorySystem;
using InventorySystem.Disarming;

using LabExtended.API;
using LabExtended.Extensions;

#pragma warning disable CS8604 // Possible null reference argument.

namespace LabExtended.Patches.Functions.Players;

/// <summary>
/// Implements disarming toggles as well as a custom auto-disarm distance config.
/// </summary>
public static class DisarmValidateEntryPatch
{
    /// <summary>
    /// Gets or sets the maximum distance (squared) at which a disarmed player will be uncuffed.
    /// </summary>
    public static float DisarmDistance { get; set; } = DisarmedPlayers.AutoDisarmDistanceSquared;

    /// <summary>
    /// Gets called when a disarmed entry is being validated. The first argument is the disarming player and the second argument is the disarmed player.
    /// The function should return true if the player should be disarmed or false if it should be prevented.
    /// </summary>
    public static event Func<ExPlayer, ExPlayer, bool>? ValidatingDisarm;  
    
    [HarmonyPatch(typeof(DisarmedPlayers), nameof(DisarmedPlayers.ValidateEntry))]
    private static bool Prefix(DisarmedPlayers.DisarmedEntry entry, ref bool __result)
    {
        if (entry.Disarmer == 0)
        {
            __result = true;
            return false;
        }

        if (!ExPlayer.TryGet(entry.DisarmedPlayer, out var disarmedPlayer)
            || !disarmedPlayer.Toggles.CanBeDisarmed)
        {
            __result = false;
            return false;
        }

        if (!ExPlayer.TryGet(entry.Disarmer, out var disarmer)
            || !disarmer.Toggles.CanDisarm)
        {
            __result = false;
            return false;
        }

        if (!disarmer.ReferenceHub.ValidateDisarmament(disarmedPlayer.ReferenceHub))
        {
            __result = false;
            return false;
        }

        if (DisarmDistance > 0f && (disarmer.Position.Position - disarmedPlayer.Position.Position).sqrMagnitude >
            DisarmDistance)
        {
            __result = false;
            return false;
        }

        if (!ValidatingDisarm.InvokeCollect(disarmer, disarmedPlayer,
                (curValue, retValue) =>
                {
                    curValue |= retValue;
                    return curValue;
                }, true))
        {
            __result = false;
            return false;
        }
        
        disarmedPlayer.ReferenceHub.inventory.ServerDropEverything();

        __result = true;
        return false;
    }
}