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
    public List<Scp914Output> Items { get; } = new();

    /// <summary>
    /// Picks a random item from the list.
    /// </summary>
    /// <param name="target">The target item owner.</param>
    /// <param name="list">The target list of outputs.</param>
    /// <returns>The picked recipe.</returns>
    public void Pick(ExPlayer? target, List<Scp914Output> list)
    {
        if (Items.Count == 0)
            return;

        if (Items.Count == 1)
        {
            list.Add(Items[0]);
            return;
        }

        while (list.Count < 1)
        {
            Items.ForEach(item =>
            {
                if (item.Chance >= 100f || WeightUtils.GetBool(item.Chance, 100f - item.Chance))
                {
                    list.Add(item);
                }
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
    public Scp914Recipe WithItem(Scp914Output item)
    {
        Items.Add(item);
        return this;
    }

    private float WeightPicker(Scp914Output output, ExPlayer? owner)
        => Mathf.Clamp(output.Chance + Scp914Recipes.GetAdditionalChance(owner, this, output), 0f, 100f);
}