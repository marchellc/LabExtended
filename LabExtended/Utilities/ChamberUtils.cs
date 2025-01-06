using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;

namespace LabExtended.Utilities;

public static class ChamberUtils
{
    public static bool RotateCylinder(this Firearm firearm, int rotationCount = 1)
    {
        if (!firearm.TryGetModule<CylinderAmmoModule>(out var cylinderAmmoModule))
            return false;

        cylinderAmmoModule.RotateCylinder(rotationCount);
        return true;
    }
    
    public static bool AnyChambersLive(this Firearm firearm)
        => firearm.AreAnyChambersAtState(CylinderAmmoModule.ChamberState.Live);

    public static bool AnyChambersEmpty(this Firearm firearm)
        => firearm.AreAnyChambersAtState(CylinderAmmoModule.ChamberState.Empty);

    public static bool AnyChambersDischarged(this Firearm firearm)
        => firearm.AreAnyChambersAtState(CylinderAmmoModule.ChamberState.Discharged);

    public static bool AllChambersLive(this Firearm firearm)
        => firearm.AreAllChambersAtState(CylinderAmmoModule.ChamberState.Live);

    public static bool AllChambersEmpty(this Firearm firearm)
        => firearm.AreAllChambersAtState(CylinderAmmoModule.ChamberState.Empty);

    public static bool AllChambersDischarged(this Firearm firearm)
        => firearm.AreAllChambersAtState(CylinderAmmoModule.ChamberState.Discharged);

    public static int CountLiveChambers(this Firearm firearm)
        => firearm.CountChambersAtState(CylinderAmmoModule.ChamberState.Live);

    public static int CountEmptyChambers(this Firearm firearm)
        => firearm.CountChambersAtState(CylinderAmmoModule.ChamberState.Empty);

    public static int CountDischargedChambers(this Firearm firearm)
        => firearm.CountChambersAtState(CylinderAmmoModule.ChamberState.Discharged);
    
    public static int CountChambersAtState(this Firearm firearm, CylinderAmmoModule.ChamberState state)
    {
        var count = 0;
        
        if (!firearm.TryGetChambers(out var chambers))
            return count;

        for (int i = 0; i < chambers.Length; i++)
        {
            if (chambers[i].ServerSyncState != state)
            {
                count++;
            }
        }

        return count;
    }
    
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
    
    public static bool AreAllChambersAtState(this Firearm firearm, CylinderAmmoModule.ChamberState state)
    {
        if (!firearm.TryGetChambers(out var chambers))
            return false;

        for (int i = 0; i < chambers.Length; i++)
        {
            if (chambers[i].ServerSyncState != state)
            {
                return false;
            }
        }

        return true;
    }

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
    
    public static int GetChamberCount(this Firearm firearm)
        => firearm.GetChambers().Length;

    public static CylinderAmmoModule.ChamberState GetCurrentChamberState(this Firearm firearm)
        => firearm.GetChamber(0)?.ServerSyncState ?? CylinderAmmoModule.ChamberState.Empty;

    public static CylinderAmmoModule.Chamber GetCurrentChamber(this Firearm firearm)
        => firearm.GetChamber(0);

    public static CylinderAmmoModule.ChamberState GetChamberState(this Firearm firearm, int chamberIndex)
        => firearm.GetChamber(chamberIndex)?.ContextState ?? CylinderAmmoModule.ChamberState.Empty;
    
    public static CylinderAmmoModule.Chamber GetChamber(this Firearm firearm, int chamberIndex)
        => firearm.TryGetChamber(chamberIndex, out var chamber) ? chamber : null;
    
    public static CylinderAmmoModule.Chamber[] GetChambers(this Firearm firearm)
        => firearm.TryGetChambers(out var chambers) ? chambers : default;
    
    public static CylinderAmmoModule GetCylinderModule(this Firearm firearm)
        => firearm.GetModule<CylinderAmmoModule>();

    public static void SyncChambers(this Firearm firearm)
        => firearm.GetModule<CylinderAmmoModule>()!._needsResyncing = true;

    public static bool TryGetChamber(this Firearm firearm, int chamberIndex, out CylinderAmmoModule.Chamber chamber)
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

    public static bool TryGetChambers(this Firearm firearm, out CylinderAmmoModule.Chamber[] chambers)
        => CylinderAmmoModule.ChambersCache.TryGetValue(firearm.ItemSerial, out chambers);

    public static bool TryGetCylinderModule(this Firearm firearm, out CylinderAmmoModule cylinderAmmoModule)
        => firearm.TryGetModule(out cylinderAmmoModule);
}