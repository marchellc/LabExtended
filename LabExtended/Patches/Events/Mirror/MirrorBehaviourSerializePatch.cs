using HarmonyLib;

using LabExtended.Events;
using LabExtended.Events.Mirror;

using Mirror;

namespace LabExtended.Patches.Events.Mirror
{
    /// <summary>
    /// Implements the <see cref="MirrorEvents.SerializingBehaviour"/> and <see cref="MirrorEvents.SerializedBehaviour"/> events.
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
                var serializingBehaviourEventArgs = new MirrorSerializingBehaviourEventArgs(null!, null!);
                var serializedBehaviourEventArgs = new MirrorSerializedBehaviourEventArgs(null!, null!, !initialState);

                for (var i = 0; i < behaviours.Length; i++)
                {
                    var behaviour = behaviours[i];

                    var isOwnerDirty = NetworkIdentity.IsDirty(ownerDirtyBits, i);
                    var isObserversDirty = NetworkIdentity.IsDirty(observersDirtyBits, i);

                    if (isOwnerDirty || isObserversDirty)
                    {
                        using var writer = NetworkWriterPool.Get();

                        serializingBehaviourEventArgs.Behaviour = behaviour;
                        serializingBehaviourEventArgs.Writer = writer;
                        serializingBehaviourEventArgs.IsAllowed = true;

                        if (MirrorEvents.OnSerializingBehaviour(serializingBehaviourEventArgs))
                            behaviour.Serialize(writer, initialState);

                        if (writer.Position > 0)
                        {
                            var segment = writer.ToArraySegment();

                            if (isOwnerDirty)
                                ownerWriter.WriteBytes(segment.Array, segment.Offset, segment.Count);

                            if (isObserversDirty)
                                observersWriter.WriteBytes(segment.Array, segment.Offset, segment.Count);

                            serializedBehaviourEventArgs.Behaviour = behaviour;
                            serializedBehaviourEventArgs.Writer = writer;
                            serializedBehaviourEventArgs.ResetBits = !initialState;

                            MirrorEvents.OnSerializedBehaviour(serializedBehaviourEventArgs);

                            if (serializedBehaviourEventArgs.ResetBits)
                                behaviour.ClearAllDirtyBits();
                        }
                    }
                }
            }

            return false;
        }
    }
}