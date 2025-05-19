using HarmonyLib;

using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Modules.Misc;

using LabExtended.API;

using LabExtended.Core;
using LabExtended.Extensions;

using LabExtended.Events;
using LabExtended.Events.Firearms;

using UnityEngine;

#pragma warning disable CS8604 // Possible null reference argument.

namespace LabExtended.Patches.Functions.Items.Firearms;

/// <summary>
/// Implements the <see cref="ExFirearmEvents.ProcessingEvent"/> and <see cref="ExFirearmEvents.ProcessedEvent"/> events.
/// </summary>
public static class FirearmEventPatch
{
    [HarmonyPatch(typeof(EventManagerModule.LayerEventProcessor), nameof(EventManagerModule.LayerEventProcessor.Process))]
    private static bool Prefix(EventManagerModule.LayerEventProcessor __instance, ref AnimatorStateInfo stateInfo,
        ref Dictionary<int, float> continuationTimes)
    {
        if (__instance._curHash != stateInfo.shortNameHash)
        {
            __instance._curHash = stateInfo.shortNameHash;
            
            __instance._lastFrame = __instance._isCurrent ? continuationTimes.GetValueOrDefault(__instance._curHash) : 0f;
            __instance._hasEvents = __instance._eventManager._nameHashesToIndexes.TryGetValue(__instance._curHash, out __instance._prevEvents);
            
            continuationTimes[__instance._curHash] = 0f;
        }

        if (!__instance._hasEvents || __instance._curHash == 0)
            return false;

        var lastFrame = __instance._lastFrame;
        
        // ReSharper disable once PossibleNullReferenceException
        for (var index = 0; index < __instance._prevEvents.Count; index++)
        {
            var eventIndex = __instance._prevEvents[index];
            var firearmEvent = __instance._eventManager.Events[eventIndex];
            var eventTime = stateInfo.normalizedTime;

            if (stateInfo.loop)
                eventTime -= (float)(int)eventTime;

            __instance._lastFrame = eventTime * firearmEvent.LengthFrames;

            if (lastFrame < firearmEvent.Frame && __instance._lastFrame >= firearmEvent.Frame)
            {
                var details = new EventInvocationDetails(stateInfo, __instance._anim, __instance._layerIndex);
                var args = new FirearmProcessingEventEventArgs(ExPlayer.Get(__instance._eventManager.Firearm.Owner),
                    __instance._eventManager.Firearm, firearmEvent, details);

                FirearmEvent.CurrentlyInvokedEvent = firearmEvent;
                
                if (!ExFirearmEvents.OnProcessingEvent(args))
                    continue;

                var exception = default(Exception);

                try
                {
                    firearmEvent.LastInvocation = details;
                    firearmEvent.Action.Invoke();
                }
                catch (Exception ex)
                {
                    exception = ex;

                    ApiLog.Error("Firearm Event", ex);
                }

                FirearmEvent.CurrentlyInvokedEvent = null;

                if (!__instance._isCurrent)
                    continuationTimes[__instance._curHash] = __instance._lastFrame;

                ExFirearmEvents.OnProcessedEvent(new(args.Player, __instance._eventManager.Firearm, firearmEvent,
                    details, args.Module, args.Method, exception));
            }
        }

        return false;
    }
}