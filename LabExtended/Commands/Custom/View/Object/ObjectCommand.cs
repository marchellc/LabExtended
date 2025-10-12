using LabExtended.Commands.Attributes;

using Mirror;

using UnityEngine;

namespace LabExtended.Commands.Custom.View;

public partial class ViewCommand
{
    [CommandOverload("object", "Shows the description of a network object.", null)]
    public void ObjectOverload(
        [CommandParameter("ID", "ID of the target object.")] uint objectId)
    {
        if (!NetworkServer.spawned.TryGetValue(objectId, out var identity))
        {
            Fail($"Could not find an active network object of ID \"{objectId}\"");
            return;
        }

        Ok(x =>
        {
            x.AppendLine($"<- Object Network ID: {identity.netId}");
            x.AppendLine($"<- Object Asset ID: {identity.assetId}");
            x.AppendLine($"<- Object Scene ID: {identity.sceneId}");
            x.AppendLine($"<- Object Visibility: {identity.visible}");
            x.AppendLine($"<- Object Position: {identity.transform.position.ToPreciseString()} ({Sender.Position.DistanceTo(identity.transform)}m)");
            x.AppendLine($"<- Object Scale: {identity.transform.localScale.ToPreciseString()}");
            x.AppendLine($"<- Object Rotation: {identity.transform.rotation}");

            if (identity.connectionToClient != null)
                x.AppendLine($"<- Client Connection: {identity.connectionToClient.address} ({identity.connectionToClient.connectionId})");

            if (identity.connectionToServer != null)
                x.AppendLine($"<- Server Connection: {identity.connectionToServer.connectionId}");

            if (identity.lastSerialization.tick != 0)
                x.AppendLine($"<- Last Serialization at: " +
                             $"{identity.lastSerialization.tick} ({Time.frameCount - identity.lastSerialization.tick} tick(s) ago) " +
                             $"{Mirror.Utils.PrettyBytes(identity.lastSerialization.observersWriter.Position)} (observers) / " +
                             $"{Mirror.Utils.PrettyBytes(identity.lastSerialization.ownerWriter.Position)} (owner)");

            if (identity.NetworkBehaviours?.Length > 0)
            {
                x.AppendLine($"<- Object Behaviours ({identity.NetworkBehaviours.Length}):");

                foreach (var behaviour in identity.NetworkBehaviours)
                {
                    x.AppendLine($" [{behaviour.ComponentIndex}] {behaviour.name}");
                    x.AppendLine($"     >- Type: {behaviour.GetType().FullName}");
                    x.AppendLine($"     >- Dirty Bits: {behaviour.syncVarDirtyBits}");
                    x.AppendLine($"     >- Sync Mode: {behaviour.syncMode}");
                    x.AppendLine($"     >- Sync Direction: {behaviour.syncDirection}");
                    x.AppendLine($"     >- Sync Interval: {behaviour.syncInterval}s");
                }
            }

            x.AppendLine("<- Flags:");

            if (identity.SpawnedFromInstantiate)
                x.AppendLine(" >- Spawned from Object.Instantiate() (SpawnedFromInstantiate)");

            if (identity.isServer)
                x.AppendLine(" >- Server Identity (isServer)");

            if (identity.isServerOnly)
                x.AppendLine(" >- Server Only Identity (isServerOnly)");

            if (identity.isClient)
                x.AppendLine(" >- Client Identity (isClient)");

            if (identity.isClientOnly)
                x.AppendLine(" >- Client Only Identity (isClientOnly)");

            if (identity.isOwned)
                x.AppendLine(" >- Owned Identity (isOwned)");

            if (identity.isLocalPlayer)
                x.AppendLine(" >- Local Player Identity (isLocalPlayer)");

            if (identity.hasSpawned)
                x.AppendLine(" >- Spawned (hasSpawned)");

            if (identity.destroyCalled)
                x.AppendLine(" >- Object.Destroy() called (destroyCalled)");

            if (identity.clientStarted)
                x.AppendLine(" >- Client started (clientStarted)");
        });
    }
}