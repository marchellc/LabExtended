using LabExtended.API.Collections.Unsafe;
using LabExtended.Core;
using LabExtended.Extensions;
using LabExtended.Utilities.Update;

using UnityEngine;

namespace LabExtended.API.Collections.Updateable;

/// <summary>
/// Represents a list of updateable objects. Wraps around <see cref="UnsafeList{T}"/>.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
public class UpdateableList<T> : UnsafeList<T>, IDisposable 
    where T : UpdateableObject
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
    /// Gets called once an object update is called.
    /// </summary>
    public event Action<T, float>? ObjectUpdated; 

    /// <summary>
    /// Initializes the list.
    /// </summary>
    /// <returns></returns>
    public UpdateableList<T> Initialize()
    {
        PlayerUpdateHelper.OnUpdate += Update;
        return this;
    }
    
    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        PlayerUpdateHelper.OnUpdate -= Update;
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
                    obj.OnUpdate(deltaInterval);
                    
                    ObjectUpdated?.InvokeSafe(obj, deltaInterval);
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