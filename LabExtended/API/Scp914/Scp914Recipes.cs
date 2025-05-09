using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.Firearms.Ammo;

using LabExtended.API.Scp914.Interfaces;

using LabExtended.Extensions;
using LabExtended.Attributes;
using LabExtended.Core;

using Scp914;
using Scp914.Processors;

#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
#pragma warning disable CS8604 // Possible null reference argument.

namespace LabExtended.API.Scp914;

/// <summary>
/// Used to handle custom SCP-914 recipes.
/// </summary>
public static class Scp914Recipes
{
    private static readonly List<Scp914Entry> emptyList = new();
    
    /// <summary>
    /// Gets a dictionary of all SCP-914 recipes.
    /// </summary>
    public static Dictionary<Scp914KnobSetting, List<Scp914Entry>> Recipes { get; } = new();

    /// <summary>
    /// Gets a list of recipe entries for the current knob setting (or an empty list).
    /// </summary>
    public static List<Scp914Entry> CurrentRecipes
    {
        get
        {
            if (Scp914Controller.Singleton != null
                && Recipes.TryGetValue(Scp914Controller.Singleton.KnobSetting, out var result))
                return result;

            return emptyList;
        }
    }

    /// <summary>
    /// Gets called when additional chance for a player's upgrade is being collected.
    /// </summary>
    public static event Func<ExPlayer, Scp914Recipe, IScp914Output, float>? CollectingChance;

    /// <summary>
    /// Adds (or replaces) an entry.
    /// </summary>
    /// <param name="setting">The knob setting to add the entry to.</param>
    /// <param name="entry">The entry to add (or replace).</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static void AddEntry(Scp914KnobSetting setting, Scp914Entry entry)
    {
        if (entry is null)
            throw new ArgumentNullException(nameof(entry));

        if (entry.InputType == ItemType.None)
            throw new ArgumentException("Cannot add an entry of type None", nameof(entry));
        
        if (!Recipes.TryGetValue(setting, out var result))
            Recipes.Add(setting, result = new());

        if (result.Count > 0)
            result.RemoveAll(x => x.InputType == entry.InputType);
        
        result.Add(entry);
    }
    
    /// <summary>
    /// Attempts to remove a specific item entry.
    /// </summary>
    /// <param name="setting">The entry knob setting.</param>
    /// <param name="inputType">The </param>
    /// <returns></returns>
    public static bool TryRemoveEntry(Scp914KnobSetting setting, ItemType inputType)
    {
        if (!Recipes.TryGetValue(setting, out var list))
            return false;
        
        return list.RemoveAll(x => x.InputType == inputType) > 0;
    }

    /// <summary>
    /// Attempts to modify an entry.
    /// </summary>
    /// <param name="setting">The entry knob setting.</param>
    /// <param name="inputType">The entry item input type.</param>
    /// <param name="modifier">The delegate used to modify the entry.</param>
    /// <returns>true if the entry was found and modified</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool TryModifyEntry(Scp914KnobSetting setting, ItemType inputType, Action<Scp914Entry> modifier)
    {
        if (modifier is null)
            throw new ArgumentNullException(nameof(modifier));

        if (!TryGetEntry(setting, inputType, out var entry))
            return false;
        
        modifier(entry);
        return true;
    }
    
    /// <summary>
    /// Attempts to find an entry for a specific item.
    /// </summary>
    /// <param name="setting">The knob setting.</param>
    /// <param name="inputType">The item entry type.</param>
    /// <param name="entry">The found entry instance.</param>
    /// <returns>true if the entry was found</returns>
    public static bool TryGetEntry(Scp914KnobSetting setting, ItemType inputType, out Scp914Entry? entry)
    {
        entry = null;

        if (!Recipes.TryGetValue(setting, out var list))
            return false;
        
        return list.TryGetFirst(x => x != null && x.InputType == inputType, out entry);
    }

    internal static float GetAdditionalChance(ExPlayer? owner, Scp914Recipe recipe, IScp914Output output)
        => CollectingChance.InvokeCollect(owner, recipe, output, (prev, result) => prev + result, 0f);

    internal static void PostProcessItem(ExPlayer player, ItemBase item, ItemBase originalItem, ItemPickupBase pickup,
        ItemPickupBase originalPickup, Scp914Entry entry, Scp914Recipe recipe, IScp914Output output,
        ref bool destroyPickup)
    {
        if (pickup is AmmoPickup ammoPickup && originalPickup is AmmoPickup originalAmmo)
        {
            AmmoItemProcessor.ExchangeAmmo(originalPickup.Info.ItemId, ammoPickup.Info.ItemId,
                originalAmmo.SavedAmmo, out var exchangedAmmo, out var change);

            destroyPickup = change == 0;

            if (!destroyPickup)
                originalAmmo.NetworkSavedAmmo = (ushort)change;

            ammoPickup.NetworkSavedAmmo = (ushort)exchangedAmmo;
        }
    }

    [LoaderInitialize(1)]
    private static void OnInit()
    {
        Scp914Utils.FillDefaultRecipes(Recipes);
        
        if (!ApiLoader.ApiConfig.OtherSection.Scp914CustomRecipes)
            return;
        
        Scp914Utils.PrintRecipes(Recipes);
    }
}