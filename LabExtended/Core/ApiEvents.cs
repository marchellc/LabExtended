using System.Reflection;

using HarmonyLib;

using LabExtended.Attributes;
using LabExtended.Extensions;

namespace LabExtended.Core;

/// <summary>
/// Event management.
/// </summary>
public static class ApiEvents
{
    /// <summary>
    /// The events namespace.
    /// </summary>
    public const string EventsNamespace = "LabExtended.Events";
    
    /// <summary>
    /// Gets a list of all event fields.
    /// </summary>
    public static Dictionary<Type, FieldInfo> EventFields { get; } = new();

    /// <summary>
    /// Gets a list of all events.
    /// </summary>
    public static Dictionary<Type, EventInfo> Events { get; } = new();

    /// <summary>
    /// Counts all event handlers for a specific event type.
    /// </summary>
    /// <param name="eventType">The event type.</param>
    /// <returns>The event handler count.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static int CountEvents(Type eventType)
    {
        if (eventType is null)
            throw new ArgumentNullException(nameof(eventType));

        if (!EventFields.TryGetValue(eventType, out var eventField))
            return 0;

        if (eventField.GetValue(null) is not Delegate eventDelegate)
            return 0;

        return eventDelegate.GetInvocationList().Length;
    }

    [LoaderInitialize(-1)]
    private static void OnInit()
    {
        foreach (var type in ApiLoader.Assembly.GetTypes())
        {
            if (type.Namespace != EventsNamespace)
                continue;
            
            if (!type.IsStatic())
                continue;

            foreach (var eventInfo in type.GetAllEvents())
            {
                var eventField = type.FindField(x => x.Name == eventInfo.Name);
                
                if (eventField is null)
                    continue;
                
                if (!eventField.IsStatic)
                    continue;

                var eventType = eventField.FieldType;
                var eventArgument = eventField.FieldType.GetGenericArguments();

                if (eventArgument?.Length == 1)
                    eventType = eventArgument[0];
                
                if (eventType == typeof(Action))
                    continue;
                
                if (!eventType.InheritsType<Delegate>() && !eventType.InheritsType<EventArgs>())
                    continue;
                
                if (Events.ContainsKey(eventType))
                    continue;
                
                EventFields.Add(eventType, eventField);
                Events.Add(eventType, eventInfo);
            }
        }
    }
}