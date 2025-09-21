using System.Reflection;

using HarmonyLib;

using LabExtended.API;
using LabExtended.Core;
using LabExtended.Events;

using VoiceChat.Networking;

namespace LabExtended.Extensions;

/// <summary>
/// Extensions targeting C# events.
/// </summary>
public static class EventExtensions
{
    /// <summary>
    /// Invokes the <see cref="ExVoiceChatEvents.ReceivingVoiceMessageEventHandler"/> delegate.
    /// </summary>
    public static void InvokeEvent(this ExVoiceChatEvents.ReceivingVoiceMessageEventHandler receivingVoiceMessageEventHandler,
        ExPlayer player, ExPlayer receiver, ref VoiceMessage message)
    {
        if (receivingVoiceMessageEventHandler is null)
            return;

        try
        {
            receivingVoiceMessageEventHandler(player, receiver, ref message);
        }
        catch (Exception ex)
        {
            ApiLog.Error("ExVoiceChatEvents.ReceivingVoiceMessage", ex);
        }
    }

    /// <summary>
    /// Invokes the <see cref="ExVoiceChatEvents.SendingVoiceMessageEventHandler"/> delegate.
    /// </summary>
    public static void InvokeEvent(this ExVoiceChatEvents.SendingVoiceMessageEventHandler sendingVoiceMessageEventHandler,
        ExPlayer player,
        ref VoiceMessage message)
    {
        if (sendingVoiceMessageEventHandler is null)
            return;

        try
        {
            sendingVoiceMessageEventHandler(player, ref message);
        }
        catch (Exception ex)
        {
            ApiLog.Error("ExVoiceChatEvents.SendingVoiceMessage", ex);
        }
    }

    /// <summary>
    /// Invokes the <see cref="ExVoiceChatEvents.StoppedSpeakingEventHandler"/> delegate.
    /// </summary>
    public static void InvokeEvent(this ExVoiceChatEvents.StoppedSpeakingEventHandler sendingVoiceMessageEventHandler, ExPlayer player,
        float time, Dictionary<long, VoiceMessage>? packets)
    {
        if (sendingVoiceMessageEventHandler is null)
            return;

        try
        {
            sendingVoiceMessageEventHandler(player, time, packets);
        }
        catch (Exception ex)
        {
            ApiLog.Error("ExVoiceChatEvents.StoppedSpeaking", ex);
        }
    }

    /// <summary>
    /// Invokes a delegate.
    /// </summary>
    public static void InvokeEvent<T>(this Action<T> eventField, T eventArgs) where T : EventArgs
    {
        if (eventField is null)
            return;

        try
        {
            eventField(eventArgs);
        }
        catch (Exception ex)
        {
            ApiLog.Error($"LabExtended", $"Caught an exception while executing event &3{typeof(T).Name}&r:\n{ex.ToColoredString()}");
        }
    }

    /// <summary>
    /// Invokes a delegate.
    /// </summary>
    public static void InvokeEvent(this Action eventField, string eventName)
    {
        if (eventField is null)
            return;

        try
        {
            eventField();
        }
        catch (Exception ex)
        {
            ApiLog.Error($"LabExtended", $"Caught an exception while executing event &3{eventName}&r:\n{ex.ToColoredString()}");
        }
    }

    /// <summary>
    /// Invokes a delegate.
    /// </summary>
    public static bool InvokeBooleanEvent<T>(this Action<T> eventField, T eventArgs) where T : BooleanEventArgs
    {
        if (eventField is null)
            return eventArgs.IsAllowed;

        try
        {
            eventField(eventArgs);
        }
        catch (Exception ex)
        {
            ApiLog.Error($"LabExtended", $"Caught an exception while executing event &3{typeof(T).Name}&r:\n{ex.ToColoredString()}");
        }

        return eventArgs.IsAllowed;
    }
    
    /// <summary>
    /// Inserts the provided listener as the first one to be invoked.
    /// </summary>
    public static void InsertFirst<T>(this Type type, string eventName, T listener, object classInstance = null)
        where T : Delegate
        => InsertFirst(type.FindEvent(x => x.Name == eventName), listener, classInstance);

    /// <summary>
    /// Inserts the provided listener as the first one to be invoked.
    /// </summary>
    public static void InsertFirst<T>(this EventInfo eventInfo, T listener, object classInstance = null) where T : Delegate
        => InsertFirst(eventInfo, (Delegate)listener, classInstance);

    /// <summary>
    /// Inserts the provided listener as the first one to be invoked.
    /// </summary>
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