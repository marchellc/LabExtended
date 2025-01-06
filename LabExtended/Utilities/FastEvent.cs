using HarmonyLib;

using System.Reflection;

using LabExtended.Core;
using LabExtended.Extensions;

namespace LabExtended.Utilities;

public class FastEvent<THandler> where THandler : Delegate
{
    public AccessTools.FieldRef<object, THandler> Getter { get; }
    public Func<object, object[], object> Invoker { get; }
    public EventInfo Event { get; }

    public FastEvent(AccessTools.FieldRef<object, THandler> getter, Func<object, object[], object> invoker, EventInfo ev)
    {
        Getter = getter;
        Invoker = invoker;
        Event = ev;
    }

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