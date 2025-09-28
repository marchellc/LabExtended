using HarmonyLib;

using LabExtended.Events;

using Mirror;

namespace LabExtended.Patches.Events.Mirror
{
    /// <summary>
    /// Implements the <see cref="MirrorEvents.BehaviourSerializing"/> and <see cref="MirrorEvents.BehaviourSerialized"/> events.
    /// </summary>
    public static class MirrorBehaviourSerializePatch
    {
        [HarmonyPatch(typeof(NetworkIdentity), nameof(NetworkIdentity.SerializeServer))]
        private static bool Prefix(NetworkIdentity __instance, bool initialState, NetworkWriter ownerWriter, NetworkWriter observersWriter)
        {
            __instance.ValidateComponents();

            var behaviours = __instance.NetworkBehaviours;
            var dirtyBits = __instance.ServerDirtyMasks(initialState);
             
            var ownerDirtyBits = dirtyBits.Item1;
            var observersDirtyBits = dirtyBits.Item2;

            if (ownerDirtyBits != 0)
                Compression.CompressVarUInt(ownerWriter, ownerDirtyBits);

            if (observersDirtyBits != 0)
                Compression.CompressVarUInt(observersWriter, observersDirtyBits);

            if (ownerDirtyBits != 0 || observersDirtyBits != 0)
            {
                for (var i = 0; i < behaviours.Length; i++)
                {
                    var behaviour = behaviours[i];

                    var isOwnerDirty = NetworkIdentity.IsDirty(ownerDirtyBits, i);
                    var isObserversDirty = NetworkIdentity.IsDirty(observersDirtyBits, i);

                    if (isOwnerDirty || isObserversDirty)
                    {
                        using var writer = NetworkWriterPool.Get();

                        if (MirrorEvents.OnBehaviourSerializing(behaviour, writer))
                            behaviour.Serialize(writer, initialState);

                        var segment = writer.ToArraySegment();

                        if (isOwnerDirty)
                            ownerWriter.WriteBytes(segment.Array, segment.Offset, segment.Count);

                        if (isObserversDirty)
                            observersWriter.WriteBytes(segment.Array, segment.Offset, segment.Count);

                        MirrorEvents.OnBehaviourSerialized(behaviour, writer);
                    }

                    if (!initialState)
                        behaviour.ClearAllDirtyBits();
                }
            }

            return false;
        }
    }
}