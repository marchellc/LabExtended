namespace LabExtended.API.Scp914;

/// <summary>
/// Defines a recipe entry.
/// </summary>
public class Scp914Entry
{
    /// <summary>
    /// Gets the type of the input item.
    /// </summary>
    public ItemType InputType { get; }

    /// <summary>
    /// Gets a list of possible output recipes.
    /// </summary>
    public List<Scp914Recipe> Recipes { get; } = new();

    /// <summary>
    /// Creates a new <see cref="Scp914Entry"/> instance.
    /// </summary>
    /// <param name="inputType">The type of the input item.</param>
    public Scp914Entry(ItemType inputType)
    {
        InputType = inputType;
    }

    /// <summary>
    /// Adds a recipe to this entry.
    /// </summary>
    /// <param name="recipe">The recipe to add.</param>
    /// <returns>This entry instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public Scp914Entry WithRecipe(Scp914Recipe recipe)
    {
        if (recipe is null)
            throw new ArgumentNullException(nameof(recipe));
        
        Recipes.Add(recipe);
        return this;
    }
}