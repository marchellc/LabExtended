using HarmonyLib;

using System.Reflection;

using LabExtended.Core;
using LabExtended.Extensions;

namespace LabExtended.Utilities;

/// <summary>
/// Defines a cached fast event invoker.
/// </summary>
public class FastEvent<THandler> where THandler : Delegate
{
    /// <summary>
    /// The reference to the event's field.
    /// </summary>
    public AccessTools.FieldRef<object, THandler> Getter { get; }

    /// <summary>
    /// The delegate used to invoke the event.
    /// </summary>
    public Func<object, object[], object> Invoker { get; }

    /// <summary>
    /// The information about the targeted event.
    /// </summary>
    public EventInfo Event { get; }

    /// <summary>
    /// Creates a new <see cref="FastEvent{THandler}"/> instance.
    /// </summary>
    public FastEvent(AccessTools.FieldRef<object, THandler> getter, Func<object, object[], object> invoker, EventInfo ev)
    {
        Getter = getter;
        Invoker = invoker;
        Event = ev;
    }

    /// <summary>
    /// Invokes the targeted event.
    /// </summary>
    public object InvokeEvent(object instance, params object[] args)
    {
        var result = default(object);
        
        try
        {
            var value = Getter(instance);
                
            if (value is null)
                return result;
            
            result = Invoker(value, args);
        }
        catch (Exception ex)
        {
            ApiLog.Error("Fast Events",
                $"Event &3{Event.GetMemberName()}&r caught an exception while executing:\n{ex.ToColoredString()}");
        }

        return result;
    }
}