using HarmonyLib;

using LabExtended.Extensions;

using System.Reflection;

namespace LabExtended.Utilities;

/// <summary>
/// Used for caching fast event invokers.
/// </summary>
public static class FastEvents
{
    /// <summary>
    /// Caches a new event field.
    /// </summary>
    public static FastEvent<THandler> DefineEvent<THandler>(Type type) where THandler : Delegate
        => DefineEvent<THandler>(type.FindEvent<THandler>());

    /// <summary>
    /// Caches a new event field.
    /// </summary>
    public static FastEvent<THandler> DefineEvent<THandler>(Type type, string eventName) where THandler : Delegate
        => DefineEvent<THandler>(type.FindEvent(eventName));

    /// <summary>
    /// Caches a new event field.
    /// </summary>
    public static FastEvent<THandler> DefineEvent<THandler>(EventInfo ev) where THandler : Delegate
    {
        var fieldRef = AccessTools.FieldRefAccess<THandler>(ev.DeclaringType, ev.Name);
        var evInvoker = FastReflection.ForDelegate(ev.EventHandlerType, ev.EventHandlerType.FindMethod("Invoke"));
        var fastEvent = new FastEvent<THandler>(fieldRef, evInvoker, ev);

        return fastEvent;
    }
}