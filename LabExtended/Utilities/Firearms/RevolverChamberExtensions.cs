using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace LabExtended.Utilities.Firearms;

/// <summary>
/// Extensions targeting the revolver's chambers.
/// </summary>
public static class RevolverChamberExtensions
{
    /// <summary>
    /// Rotates the firearm's cylinder.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="rotationCount">The amount of times to rotate the cylinder.</param>
    /// <returns>true if the cylinder was rotated</returns>
    public static bool RotateCylinder(this Firearm firearm, int rotationCount = 1)
    {
        if (!firearm.TryGetModule<CylinderAmmoModule>(out var cylinderAmmoModule))
            return false;

        cylinderAmmoModule.RotateCylinder(rotationCount);
        return true;
    }
    
    /// <summary>
    /// Whether or not the firearm has any live chambers.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>true if the firearm has any live chambers.</returns>
    public static bool AnyChambersLive(this Firearm firearm)
        => firearm.AreAnyChambersAtState(CylinderAmmoModule.ChamberState.Live);

    /// <summary>
    /// Whether or not the firearm has any empty chambers.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>true if the firearm has any empty chambers.</returns>
    public static bool AnyChambersEmpty(this Firearm firearm)
        => firearm.AreAnyChambersAtState(CylinderAmmoModule.ChamberState.Empty);

    /// <summary>
    /// Whether or not the firearm has any discharged chambers.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>true if the firearm has any discharged chambers.</returns>
    public static bool AnyChambersDischarged(this Firearm firearm)
        => firearm.AreAnyChambersAtState(CylinderAmmoModule.ChamberState.Discharged);
    
    /// <summary>
    /// Whether or not all of the chambers of this firearm are live.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>true if all of the chambers are live</returns>
    public static bool AllChambersLive(this Firearm firearm)
        => firearm.AreAllChambersAtState(CylinderAmmoModule.ChamberState.Live);

    /// <summary>
    /// Whether or not all of the chambers of this firearm are empty.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>true if all of the chambers are empty</returns>
    public static bool AllChambersEmpty(this Firearm firearm)
        => firearm.AreAllChambersAtState(CylinderAmmoModule.ChamberState.Empty);

    /// <summary>
    /// Whether or not all of the chambers of this firearm are discharged.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>true if all of the chambers are discharged</returns>
    public static bool AllChambersDischarged(this Firearm firearm)
        => firearm.AreAllChambersAtState(CylinderAmmoModule.ChamberState.Discharged);

    /// <summary>
    /// Counts the amount of live chambers in a firearm.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>The amount of live chambers.</returns>
    public static int CountLiveChambers(this Firearm firearm)
        => firearm.CountChambersAtState(CylinderAmmoModule.ChamberState.Live);

    /// <summary>
    /// Counts the amount of empty chambers in a firearm.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>The amount of empty chambers.</returns>
    public static int CountEmptyChambers(this Firearm firearm)
        => firearm.CountChambersAtState(CylinderAmmoModule.ChamberState.Empty);

    /// <summary>
    /// Counts the amount of discharged chambers in a firearm.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>The amount of discharged chambers.</returns>
    public static int CountDischargedChambers(this Firearm firearm)
        => firearm.CountChambersAtState(CylinderAmmoModule.ChamberState.Discharged);
    
    /// <summary>
    /// Counts the amount of chambers at a specific state.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="state">The target state.</param>
    /// <returns>The amount of chambers at the specified state.</returns>
    public static int CountChambersAtState(this Firearm firearm, CylinderAmmoModule.ChamberState state)
    {
        var count = 0;
        
        if (!firearm.TryGetChambers(out var chambers))
            return count;

        for (var i = 0; i < chambers.Length; i++)
        {
            if (chambers[i].ServerSyncState != state)
            {
                count++;
            }
        }

        return count;
    }
    
    /// <summary>
    /// Whether or not any of the firearm's chambers are at a specific state.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="state">The required chamber state.</param>
    /// <returns>true if any of the firearm's chambers are at the specified state.</returns>
    public static bool AreAnyChambersAtState(this Firearm firearm, CylinderAmmoModule.ChamberState state)
    {
        if (!firearm.TryGetChambers(out var chambers))
            return false;

        for (int i = 0; i < chambers.Length; i++)
        {
            if (chambers[i].ServerSyncState == state)
            {
                return true;
            }
        }

        return false;
    }
    
    /// <summary>
    /// Whether or not all of the firearm's chambers are at a specific state.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="state">The required chamber state.</param>
    /// <returns>true if all of the firearm's chambers are at the specified state.</returns>
    public static bool AreAllChambersAtState(this Firearm firearm, CylinderAmmoModule.ChamberState state)
    {
        if (!firearm.TryGetChambers(out var chambers))
            return false;

        for (var i = 0; i < chambers.Length; i++)
        {
            if (chambers[i].ServerSyncState != state)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Sets the state of a chamber at a specific index.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="chamberIndex">The target chamber index.</param>
    /// <param name="state">The chamber's new state.</param>
    /// <returns>true if the chamber's state was changed.</returns>
    public static bool SetChamberState(this Firearm firearm, int chamberIndex, CylinderAmmoModule.ChamberState state)
    {
        if (chamberIndex < 0)
            return false;
        
        if (!firearm.TryGetModule<CylinderAmmoModule>(out var cylinderAmmoModule))
            return false;
        
        if (!firearm.TryGetChambers(out var chambers))
            return false;

        if (chamberIndex >= chambers.Length)
            return false;

        var chamber = chambers[chamberIndex];

        if (chamber.ContextState == state)
            return false;

        chamber.ContextState = state;
        
        cylinderAmmoModule._needsResyncing = true;
        return true;
    }

    /// <summary>
    /// Changes the state of all chambers.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="stateGetter">The delegate used to determine a chamber's new state.</param>
    /// <returns>true if any chambers were changed</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool SetChamberStates(this Firearm firearm, Func<int, CylinderAmmoModule.Chamber, CylinderAmmoModule.ChamberState> stateGetter)
    {
        if (stateGetter is null)
            throw new ArgumentNullException(nameof(stateGetter));
        
        if (!firearm.TryGetModule<CylinderAmmoModule>(out var cylinderAmmoModule))
            return false;
        
        if (!firearm.TryGetChambers(out var chambers))
            return false;

        var anyChanged = false;

        for (int i = 0; i < chambers.Length; i++)
        {
            var chamber = chambers[i];
            var state = stateGetter(i, chamber);

            if (chamber.ContextState != state)
            {
                chamber.ContextState = state;
                anyChanged = true;
            }
        }

        if (anyChanged)
            cylinderAmmoModule._needsResyncing = true;

        return anyChanged;
    }
    
    /// <summary>
    /// Gets the amount of chambers in a firearm.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>The amount of chambers.</returns>
    public static int GetChamberCount(this Firearm firearm)
        => firearm.GetChambers()?.Length ?? 0;

    /// <summary>
    /// Gets the state of the currently loaded chamber.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>The state of the currently loaded chamber (or <see cref="CylinderAmmoModule.ChamberState.Empty"/> if not found)</returns>
    public static CylinderAmmoModule.ChamberState GetCurrentChamberState(this Firearm firearm)
        => firearm.GetChamber(0)?.ServerSyncState ?? CylinderAmmoModule.ChamberState.Empty;

    /// <summary>
    /// Gets the firearm's currently loaded chamber.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>The currently loaded chamber instance, if any</returns>
    public static CylinderAmmoModule.Chamber? GetCurrentChamber(this Firearm firearm)
        => firearm.GetChamber(0);

    /// <summary>
    /// Gets the state of a chamber at a specific index.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="chamberIndex">The target chamber index.</param>
    /// <returns>The state of the chamber at the specified index (or <see cref="CylinderAmmoModule.ChamberState.Empty"/> if not found)</returns>
    public static CylinderAmmoModule.ChamberState GetChamberState(this Firearm firearm, int chamberIndex)
        => firearm.GetChamber(chamberIndex)?.ContextState ?? CylinderAmmoModule.ChamberState.Empty;
    
    /// <summary>
    /// Gets the chamber at a specific index.
    /// </summary>
    /// <param name="firearm">The target firearm</param>
    /// <param name="chamberIndex">The target chamber index</param>
    /// <returns>The chamber instance, if found</returns>
    public static CylinderAmmoModule.Chamber? GetChamber(this Firearm firearm, int chamberIndex)
        => firearm.TryGetChamber(chamberIndex, out var chamber) ? chamber : null;
    
    /// <summary>
    /// Gets the firearm's chambers in an array.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>Array of firearm chambers if found</returns>
    public static CylinderAmmoModule.Chamber[]? GetChambers(this Firearm firearm)
        => firearm.TryGetChambers(out var chambers) ? chambers : null;

    /// <summary>
    /// Synchronizes all of the firearm's chambers.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    public static void SyncChambers(this Firearm firearm)
        => firearm.GetModule<CylinderAmmoModule>()!._needsResyncing = true;

    /// <summary>
    /// Tries to get a chamber at a specific index.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="chamberIndex">The target chamber index.</param>
    /// <param name="chamber">The found chamber instance.</param>
    /// <returns>true if the chamber was found</returns>
    public static bool TryGetChamber(this Firearm firearm, int chamberIndex, out CylinderAmmoModule.Chamber? chamber)
    {
        chamber = null;

        if (chamberIndex < 0)
            return false;
        
        if (!firearm.TryGetChambers(out var chambers))
            return false;

        if (chamberIndex >= chambers.Length)
            return false;

        chamber = chambers[chamberIndex];
        return true;
    }

    /// <summary>
    /// Tries to get the chambers of a firearm.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="chambers">The chambers array.</param>
    /// <returns>true if the chambers were found</returns>
    public static bool TryGetChambers(this Firearm firearm, out CylinderAmmoModule.Chamber[]? chambers)
        => CylinderAmmoModule.ChambersCache.TryGetValue(firearm.ItemSerial, out chambers);
    
    /// <summary>
    /// Gets the firearm's cylinder module.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>The found cylinder ammo module instance</returns>
    public static CylinderAmmoModule? GetCylinderModule(this Firearm firearm)
        => firearm.GetModule<CylinderAmmoModule>();

    /// <summary>
    /// Tries to get the firearm's cylinder module.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="cylinderAmmoModule">The found cylinder module instance.</param>
    /// <returns>true if the module was found</returns>
    public static bool TryGetCylinderModule(this Firearm firearm, out CylinderAmmoModule? cylinderAmmoModule)
        => firearm.TryGetModule(out cylinderAmmoModule);
}