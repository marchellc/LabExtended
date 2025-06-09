using HarmonyLib;

using LabExtended.Core;
using LabExtended.Events;
using LabExtended.Extensions;

using Mirror;

using UnityEngine;

namespace LabExtended.Patches.Functions.Networking
{
    public static class NetworkSpawnPatch
    {
        [HarmonyPatch(typeof(NetworkServer), nameof(NetworkServer.SpawnObject), typeof(GameObject), typeof(NetworkConnection))]
        public static bool Prefix(GameObject obj, NetworkConnection ownerConnection)
        {
            try
            {
                if (Mirror.Utils.IsPrefab(obj))
                    return false;

                if (!obj.TryGetComponent<NetworkIdentity>(out var identity))
                    return false;

                if (identity.SpawnedFromInstantiate)
                    return false;

                if (NetworkServer.spawned.ContainsKey(identity.netId))
                    return false;

                identity.connectionToClient = (NetworkConnectionToClient)ownerConnection;

                if (ownerConnection is LocalConnectionToClient)
                    identity.isOwned = true;

                if (!identity.isServer && identity.netId == 0)
                {
                    identity.isLocalPlayer = NetworkClient.localPlayer == identity;
                    identity.isClient = NetworkClient.active;

                    identity.isServer = true;

                    identity.netId = NetworkIdentity.GetNextNetworkId();

                    NetworkServer.spawned[identity.netId] = identity;

                    identity.OnStartServer();
                }

                if (NetworkServer.aoi)
                {
                    try
                    {
                        NetworkServer.aoi.OnSpawned(identity);
                    }
                    catch { }
                }

                NetworkServer.RebuildObservers(identity, true);
                MirrorEvents.OnSpawning(identity);
            }
            catch (Exception ex)
            {
                ApiLog.Error("NetworkSpawnPatch", $"Failed to spawn network identity:\n{ex.ToColoredString()}");
            }

            return false;
        }
    }
}