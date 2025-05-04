using LabExtended.Core;

using UnityEngine;

#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace LabExtended.Utilities.Update;

/// <summary>
/// Provides a base class with an overridable async update method.
/// </summary>
public class AsyncUpdate
{
    private volatile bool isEnabled;

    /// <summary>
    /// Whether or not the update method is enabled.
    /// </summary>
    public bool IsUpdateEnabled => isEnabled;

    /// <summary>
    /// Gets called once per frame.
    /// </summary>
    public event Func<Awaitable>? OnUpdate;

    /// <summary>
    /// Enables the update method.
    /// </summary>
    public void EnableUpdate()
    {
        if (isEnabled)
            return;

        isEnabled = true;

        RunUpdate();
    }

    /// <summary>
    /// Disables the update method.
    /// </summary>
    public void StopUpdate()
    {
        isEnabled = false;
    }

    /// <summary>
    /// Gets called once per frame.
    /// </summary>
    public virtual Awaitable UpdateAsync() => null;

    private async Awaitable RunUpdate()
    {
        while (isEnabled)
        {
            await Awaitable.NextFrameAsync();

            try
            {
                var update = UpdateAsync();

                if (update != null)
                    await update;
                
                if (OnUpdate != null)
                {
                    var onUpdate = OnUpdate();

                    if (onUpdate != null)
                        await onUpdate;
                }
            }
            catch (Exception ex)
            {
                ApiLog.Error("Async Update", $"Caught an exception in the update loop:\n{ex}");
            }
        }
    }
}