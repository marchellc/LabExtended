using System.Reflection;

using HarmonyLib;
using LabExtended.Core;

namespace LabExtended.Extensions;

public static class EventExtensions
{
    public static void InsertFirst<T>(Type type, string eventName, T listener, object classInstance = null)
        where T : Delegate
        => InsertFirst(type.FindEvent(x => x.Name == eventName), listener, classInstance);
    
    public static void InsertFirst<T>(EventInfo eventInfo, T listener, object classInstance = null) where T : Delegate
        => InsertFirst(eventInfo, (Delegate)listener, classInstance);
    
    public static void InsertFirst(EventInfo eventInfo, Delegate listener, object classInstance = null)
    {
        if (eventInfo is null)
            throw new ArgumentNullException(nameof(eventInfo));

        if (listener is null)
            throw new ArgumentNullException(nameof(listener));

        try
        {
            ApiLog.Debug("LabExtended API", $"EventExtensions::InsertFirst() | Inserting listener &1{listener.Method.GetMemberName()}&r into event &1{eventInfo.GetMemberName()}&r");
            
            var field = eventInfo.DeclaringType.Field(eventInfo.Name);

            if (field is null)
            {
                ApiLog.Debug("LabExtended API", $"EventExtensions::InsertFirst() | Field {eventInfo.Name} does not exist");
                return;
            }
            
            ApiLog.Debug("LabExtended API", $"EventExtensions::InsertFirst() | Found field &1{field.GetMemberName(true)}&r, type: &1{field.FieldType.FullName}&r");
            
            var instance = field.GetValue(classInstance) as Delegate;

            if (instance is null)
            {
                ApiLog.Debug("LabExtended API", "EventExtensions::InsertFirst() | Field instance is null, adding event listener");
                eventInfo.AddEventHandler(classInstance, listener);
            }
            else
            {
                ApiLog.Debug("LabExtended API", "EventExtensions::InsertFirst() | Field instance is not null, retrieving listeners");
                
                var listeners = instance.GetInvocationList();
                
                ApiLog.Debug("LabExtended API", $"EventExtensions::InsertFirst() | Listeners: &1{listeners?.Length ?? -1}&r");

                for (int i = 0; i < listeners.Length; i++)
                {
                    eventInfo.RemoveEventHandler(classInstance, listeners[i]);
                    ApiLog.Debug("LabExtended API", $"EventExtensions::InsertFirst() | Removed listener &1{listeners[i].Method.GetMemberName()}&r");
                }

                eventInfo.AddEventHandler(classInstance, listener);
                
                ApiLog.Debug("LabExtended API", "EventExtensions::InsertFirst() | Inserted listener");

                for (int i = 0; i < listeners.Length; i++)
                {
                    eventInfo.AddEventHandler(classInstance, listeners[i]);
                    ApiLog.Debug("LabExtended API", $"EventExtensions::InsertFirst() | Added listener &1{listeners[i].Method.GetMemberName()}&r");
                }
                
                ApiLog.Debug("LabExtended API", "EventExtensions::InsertFirst() | Finished inserting event listener");
            }
        }
        catch (Exception ex)
        {
            ApiLog.Error("LabExtended API", ex);
        }
    }
}