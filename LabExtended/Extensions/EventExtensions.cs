using System.Reflection;

using HarmonyLib;
using LabExtended.Core;

namespace LabExtended.Extensions;

public static class EventExtensions
{
    public static void InsertFirst<T>(this Type type, string eventName, T listener, object classInstance = null)
        where T : Delegate
        => InsertFirst(type.FindEvent(x => x.Name == eventName), listener, classInstance);
    
    public static void InsertFirst<T>(this EventInfo eventInfo, T listener, object classInstance = null) where T : Delegate
        => InsertFirst(eventInfo, (Delegate)listener, classInstance);
    
    public static void InsertFirst(this EventInfo eventInfo, Delegate listener, object classInstance = null)
    {
        if (eventInfo is null)
            throw new ArgumentNullException(nameof(eventInfo));

        if (listener is null)
            throw new ArgumentNullException(nameof(listener));

        try
        {
            var field = eventInfo.DeclaringType.Field(eventInfo.Name);

            if (field is null)
                return;

            var instance = field.GetValue(classInstance) as Delegate;

            if (instance is null)
            {
                eventInfo.AddEventHandler(classInstance, listener);
            }
            else
            {
                var listeners = instance.GetInvocationList();

                for (int i = 0; i < listeners.Length; i++)
                    eventInfo.RemoveEventHandler(classInstance, listeners[i]);

                eventInfo.AddEventHandler(classInstance, listener);

                for (int i = 0; i < listeners.Length; i++)
                    eventInfo.AddEventHandler(classInstance, listeners[i]);
            }
        }
        catch (Exception ex)
        {
            ApiLog.Error("LabExtended API", ex);
        }
    }
}