using LabExtended.Attributes;
using LabExtended.Extensions;

using PlayerRoles;

namespace LabExtended.API.CustomEffects;

/// <summary>
/// Represents a custom player effect.
/// </summary>
public class CustomPlayerEffect
{
    /// <summary>
    /// A list of known custom effects by type.
    /// </summary>
    public static HashSet<Type> Effects { get; } = new(10);

    /// <summary>
    /// Tries to find an effect by it's type name.
    /// </summary>
    /// <param name="name">The name of the effect's class.</param>
    /// <param name="onlyFullName">Whether or not to accept only full type names (eg. with namespaces)</param>
    /// <param name="ignoreCase">Whether or not to ignore uppercase / lowercase letters.</param>
    /// <param name="effectType">The found effect's type.</param>
    /// <returns>true if the effect was found</returns>
    public static bool TryGetEffect(string name, bool onlyFullName, bool ignoreCase, out Type? effectType)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            effectType = null;
            return false;
        }

        foreach (var type in Effects)
        {
            if (string.Equals(type.FullName, name, ignoreCase
                    ? StringComparison.InvariantCultureIgnoreCase
                    : StringComparison.InvariantCulture))
            {
                effectType = type;
                return true;
            }

            if (!onlyFullName && string.Equals(type.Name, name, ignoreCase
                    ? StringComparison.InvariantCultureIgnoreCase
                    : StringComparison.InvariantCulture))
            {
                effectType = type;
                return true;
            }
        }

        effectType = null;
        return false;
    }
    
    /// <summary>
    /// Gets the player that this effect belongs to.
    /// </summary>
    public ExPlayer? Player { get; internal set; }
    
    /// <summary>
    /// Whether or not this effect is active.
    /// </summary>
    public bool IsActive { get; internal set; }
    
    /// <summary>
    /// Called when this effect is initially added.
    /// </summary>
    public virtual void Start() { }
    
    /// <summary>
    /// Called when this effect is removed.
    /// </summary>
    public virtual void Stop() { }
    
    /// <summary>
    /// Called when this effect gets applied.
    /// </summary>
    public virtual void ApplyEffects() { }
    
    /// <summary>
    /// Called when this effect gets removed.
    /// </summary>
    public virtual void RemoveEffects() { }

    /// <summary>
    /// Called when the player's role changes.
    /// <para>The returning value determines whether or not to keep the effect active.</para>
    /// <para>Returning TRUE will keep the effect, FALSE will remove it.</para>
    /// </summary>
    /// <param name="newRole">The role the player changed to.</param>
    /// <returns></returns>
    public virtual bool RoleChanged(RoleTypeId newRole) => false;

    /// <summary>
    /// Called when this effect gets enabled (ie when the player changes role while this effect is disabled).
    /// </summary>
    public void Enable()
    {
        if (IsActive)
            return;
        
        IsActive = true;
        
        OnApplyEffects();
    }

    /// <summary>
    /// Called when this effect gets disabled (ie when the player changes role while this effect is enabled).
    /// </summary>
    public void Disable()
    {
        if (!IsActive)
            return;
        
        IsActive = false;
        
        OnRemoveEffects();
    }
    
    internal virtual void OnApplyEffects() => ApplyEffects();
    internal virtual void OnRemoveEffects() => RemoveEffects();
    
    internal virtual bool OnRoleChanged(RoleTypeId newRole) => RoleChanged(newRole);

    [LoaderInitialize(20)]
    private static void OnInit()
    {
        TypeExtensions.ForEachLoadedType(type =>
        {
            if (!type.InheritsType<CustomPlayerEffect>()
                || type == typeof(CustomTickingEffect)
                || type == typeof(CustomDurationEffect))
                return;

            Effects.Add(type);
        });
    }
}