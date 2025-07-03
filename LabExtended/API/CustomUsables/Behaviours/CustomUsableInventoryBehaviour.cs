using LabExtended.API.CustomItems;
using LabExtended.API.CustomItems.Behaviours;

using UnityEngine;

namespace LabExtended.API.CustomUsables.Behaviours;

/// <summary>
/// Inventory behaviour of custom usable items.
/// </summary>
public class CustomUsableInventoryBehaviour : CustomItemInventoryBehaviour
{
    /// <summary>
    /// Gets the last use time of this item (or null if not used).
    /// <remarks>Returns time since startup (in seconds) - <see cref="UnityEngine.Time.realtimeSinceStartup"/></remarks>
    /// </summary>
    public float? LastUse { get; internal set; }

    /// <summary>
    /// Gets the amount of seconds that have passed since the player last used the item (or null if the item was not used).
    /// </summary>
    public float? TimeSinceLastUse
    {
        get
        {
            if (!LastUse.HasValue)
                return null;
            
            return Time.realtimeSinceStartup - LastUse.Value;
        }
    }
    
    /// <summary>
    /// Gets the item's remaining use duration (in seconds).
    /// </summary>
    public float RemainingUse { get; internal set; }
    
    /// <summary>
    /// Gets the item's remaining cooldown (in seconds).
    /// </summary>
    public float RemainingCooldown { get; internal set; }
    
    /// <summary>
    /// Gets or sets the item's use duration (in seconds).
    /// </summary>
    public float UseDuration { get; set; }
    
    /// <summary>
    /// Gets or sets the item's cooldown after use (in seconds).
    /// </summary>
    public float CooldownDuration { get; set; }
    
    /// <summary>
    /// Whether or not the item is being used.
    /// </summary>
    public bool IsUsing { get; internal set; }

    /// <summary>
    /// Whether or not the item is ready to be used.
    /// </summary>
    public bool IsReady
    {
        get
        {
            if (IsUsing)
                return false;

            if (RemainingCooldown > 0f)
                return false;

            return true;
        }
    }

    /// <summary>
    /// Gets called when the player starts using the item.
    /// <param name="duration">The use duration for the player (in seconds).</param>
    /// </summary>
    /// <returns>true if the player should be allowed to use the item</returns>
    public virtual bool OnUsing(ref float duration) => true;
    
    /// <summary>
    /// Gets called when the player finishes using the item.
    /// </summary>
    /// <param name="cooldown">The cooldown duration for the player (in seconds).</param>
    public virtual void OnUsed(ref float cooldown) { }
    
    /// <summary>
    /// Gets called before the player cancels the item's usage.
    /// </summary>
    /// <param name="byUser">true if the usage was cancelled by the player, false if it was cancelled due to the player's held item changing</param>
    /// <returns>true if the player should be allowed to cancel the item's usage</returns>
    public virtual bool OnCancelling(bool byUser) => true;
    
    /// <summary>
    /// Gets called when the player cancels the item's usage.
    /// </summary>
    /// <param name="cooldown">The cooldown duration for the player (in seconds).</param>
    /// <param name="byUser">true if the usage was cancelled by the player, false if it was cancelled due to the player's held item changing</param>
    public virtual void OnCancelled(ref float cooldown, bool byUser) { }
    
    /// <summary>
    /// Gets called when the player's item usage cooldown expires.
    /// </summary>
    public virtual void OnCooldownOver() { }

    /// <inheritdoc cref="CustomItemBehaviour.OnUpdate"/>
    public override void OnUpdate()
    {
        base.OnUpdate();

        if (IsUsing)
        {
            if (!IsSelected)
            {
                InternalCancel(false);
            }
            else
            {
                RemainingUse -= Time.deltaTime;

                if (RemainingUse > 0f)
                    return;

                var cooldown = CooldownDuration;
                
                OnUsed(ref cooldown);

                LastUse = Time.realtimeSinceStartup;

                RemainingCooldown = cooldown;
                RemainingUse = 0f;

                IsUsing = false;
            }
        }
        else
        {
            if (RemainingCooldown != 0f)
            {
                RemainingCooldown -= Time.deltaTime;

                if (RemainingCooldown > 0f)
                    return;

                RemainingCooldown = 0f;
                
                OnCooldownOver();
            }
        }
    }

    internal void InternalStart(float duration)
    {
        RemainingCooldown = 0f;
        RemainingUse = duration;
        
        IsUsing = true;
    }

    internal void InternalCancel(bool byUser)
    {
        if (OnCancelling(byUser))
        {
            var cooldown = CooldownDuration;
                    
            OnCancelled(ref cooldown, byUser);

            RemainingCooldown = cooldown;
            RemainingUse = 0f;

            IsUsing = false;

            LastUse = Time.realtimeSinceStartup;
        }
    }
}