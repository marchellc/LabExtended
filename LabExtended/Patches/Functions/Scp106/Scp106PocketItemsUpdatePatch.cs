using HarmonyLib;

using LabApi.Features.Wrappers;

using LabExtended.Extensions;

using LabExtended.Events;
using LabExtended.Events.Map;

using Mirror;

using PlayerRoles.PlayableScps.Scp106;

using UnityEngine;

namespace LabExtended.Patches.Functions.Scp106;

/// <summary>
/// Implements the <see cref="ExMapEvents.PocketDimensionDestroyingItem"/> and <see cref="ExMapEvents.PocketDimensionDroppingItem"/> events.
/// </summary>
public static class Scp106PocketItemsUpdatePatch
{
    [HarmonyPatch(typeof(Scp106PocketItemManager), nameof(Scp106PocketItemManager.Update))]
    private static bool Prefix()
    {
        var anyRemoved = false;

        foreach (var pair in Scp106PocketItemManager.TrackedItems)
        {
            if (pair.Key == null || !Scp106PocketItemManager.IsInPocketDimension(pair.Key.transform.position))
            {
                anyRemoved |= Scp106PocketItemManager.ToRemove.Add(pair.Key);
            }
            else
            {
                var item = pair.Value;
                var time = item.TriggerTime - NetworkTime.time;

                if (time <= 3)
                {
                    if (!item.Remove && !item.WarningSent)
                    {
                        NetworkServer.SendToAll(
                            new Scp106PocketItemManager.WarningMessage { Position = item.DropPosition }, 0, true);

                        item.WarningSent = true;
                    }

                    if (time <= 0)
                    {
                        var pickup = pair.Key;

                        if (item.Remove && ExMapEvents.OnPocketDimensionDestroyingItem(new(Pickup.Get(pickup))))
                        {
                            pickup.DestroySelf();
                        }
                        else if (pickup.TryGetRigidbody(out var rigidbody))
                        {
                            var droppingArgs = new PocketDimensionDroppingItemEventArgs(Pickup.Get(pickup),
                                item.DropPosition.Position,
                                new Vector3(Scp106PocketItemManager.RandomVel, Physics.gravity.y,
                                    Scp106PocketItemManager.RandomVel));

                            if (ExMapEvents.OnPocketDimensionDroppingItem(droppingArgs))
                            {
                                rigidbody.velocity = droppingArgs.Velocity;
                                pickup.transform.position = droppingArgs.Position;
                            }
                        }

                        anyRemoved |= Scp106PocketItemManager.ToRemove.Add(pickup);

                        pair.Value.TriggerTime = NetworkTime.time +
                                                 UnityEngine.Random.Range(Scp106PocketItemManager.TimerRange.x,
                                                     Scp106PocketItemManager.TimerRange.y);
                    }
                }
            }
        }

        if (anyRemoved)
            Scp106PocketItemManager.ToRemove.ForEach(x => Scp106PocketItemManager.TrackedItems.Remove(x));

        return false;
    }
}
