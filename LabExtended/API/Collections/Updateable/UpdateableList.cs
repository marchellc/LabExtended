using LabExtended.Core;
using LabExtended.Extensions;
using LabExtended.Utilities.Update;
using LabExtended.API.Collections.Unsafe;

using UnityEngine;

namespace LabExtended.API.Collections.Updateable;

/// <summary>
/// Represents a list of elements that can be updated at a specified interval and provides events for element and list
/// updates.
/// </summary>
/// <remarks>The update loop is managed automatically after calling <see cref="Initialize"/>, which subscribes the
/// list to periodic updates. The <see cref="Updated"/> event is raised after each update cycle, and the <see
/// cref="ElementUpdated"/> event is raised for each element as it is updated. Call <see cref="Dispose"/> to unsubscribe
/// the list from updates and release resources. This class is not thread-safe.</remarks>
/// <typeparam name="T">The type of elements in the list. Must implement <see cref="IUpdateableElement"/>.</typeparam>
public class UpdateableList<T> : UnsafeList<T>, IDisposable 
    where T : IUpdateableElement
{
    private float remainingInterval = 0.1f;
    private float deltaInterval = 0f;
    
    /// <summary>
    /// Gets or sets the update interval (in seconds).
    /// </summary>
    public float Interval { get; set; } = 0.1f;

    /// <summary>
    /// Gets called once an update loop is finished.
    /// </summary>
    public event Action<float>? Updated;

    /// <summary>
    /// Gets called once an element's update is called.
    /// </summary>
    public event Action<T, float>? ElementUpdated; 

    /// <summary>
    /// Initializes the list.
    /// </summary>
    /// <returns></returns>
    public UpdateableList<T> Initialize()
    {
        PlayerUpdateHelper.Component.OnUpdate += Update;
        return this;
    }
    
    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        PlayerUpdateHelper.Component.OnUpdate -= Update;
    }
    
    private void Update()
    {
        if (Interval > 0f)
        {
            remainingInterval -= Time.deltaTime;

            if (remainingInterval > 0f)
            {
                deltaInterval += Time.deltaTime;
                return;
            }

            remainingInterval = Interval;
        }
        else
        {
            deltaInterval = Time.deltaTime;
        }

        try
        {
            for (var i = 0; i < Count; i++)
            {
                var obj = this[i];

                if (obj != null)
                {
                    obj.OnUpdate(this, deltaInterval);
                    
                    ElementUpdated?.InvokeSafe(obj, deltaInterval);
                }
            }
        }
        catch (Exception ex)
        {
            ApiLog.Warn("LabExtended API", $"Caught an exception while updating objects:\n{ex}");
        }
        
        Updated?.InvokeSafe(deltaInterval);

        deltaInterval = 0;
    }
}