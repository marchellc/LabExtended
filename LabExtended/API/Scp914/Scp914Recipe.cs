using LabExtended.API.Scp914.Interfaces;
using LabExtended.Utilities;

using UnityEngine;

namespace LabExtended.API.Scp914;

/// <summary>
/// Defines a SCP-914 recipe.
/// </summary>
public class Scp914Recipe
{
    /// <summary>
    /// Gets the chance of this recipe (0 - 100).
    /// </summary>
    public float Chance
    {
        get;
        set => field = Mathf.Clamp(value, 0f, 100f);
    }

    /// <summary>
    /// Gets a list of all possible items to be upgraded to.
    /// </summary>
    public List<IScp914Output> Items { get; } = new();

    /// <summary>
    /// Picks a random item from the list.
    /// </summary>
    /// <param name="target">The target item owner.</param>
    /// <param name="list">The target list of outputs.</param>
    /// <returns>The picked recipe.</returns>
    public void Pick(ExPlayer? target, List<IScp914Output> list)
    {
        if (Items.Count == 0)
            return;

        if (Items.Count == 1)
        {
            var output = Items[0];
            var chance = Mathf.Clamp(output.Chance + Scp914Recipes.GetAdditionalChance(target, this, output), 0f, 100f);
            
            if (chance != 0f && (chance == 100f || WeightUtils.GetBool(chance, 100f - chance)))
                list.Add(Items[0]);
            
            return;
        }

        while (list.Count < 1)
        {
            list.Add(Items.GetRandomWeighted(x => list.Contains(x)
                ? 0f 
                : Mathf.Clamp(x.Chance + Scp914Recipes.GetAdditionalChance(target, this, x), 0f, 100f)));
            
            Items.ForEach(item =>
            {
                if (item.Chance != 100f)
                    return;

                if (list.Contains(item))
                    return;
                
                list.Add(item);
            });
        }
    }

    /// <summary>
    /// Sets the chance of the recipe.
    /// </summary>
    /// <param name="chance">The chance of this recipe.</param>
    /// <returns>This recipe instance.</returns>
    public Scp914Recipe WithChance(float chance)
    {
        Chance = chance;
        return this;
    }

    /// <summary>
    /// Adds a new item to the recipe.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns>This recipe instance.</returns>
    public Scp914Recipe WithItem(IScp914Output item)
    {
        Items.Add(item);
        return this;
    }
}