using HarmonyLib;

using LabExtended.Core;
using LabExtended.Events;

using Mirror;

using UnityEngine;

namespace LabExtended.Patches.Functions.Networking
{
    /// <summary>
    /// Provides the <see cref="MirrorEvents.Destroying"/> and <see cref="MirrorEvents.Destroyed"/> patches.
    /// </summary>
    public static class NetworkDestroyPatch
    {
        [HarmonyPatch(typeof(NetworkServer), nameof(NetworkServer.DestroyObject), typeof(NetworkIdentity), typeof(NetworkServer.DestroyMode))]
        private static bool Prefix(NetworkIdentity identity, NetworkServer.DestroyMode mode)
        {
            try
            {
                MirrorEvents.OnDestroying(identity, mode);

                if (NetworkServer.active && NetworkServer.aoi)
                {
                    try
                    {
                        NetworkServer.aoi.OnDestroyed(identity);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }

                NetworkServer.spawned.Remove(identity.netId);
                NetworkConnectionToClient connectionToClient = identity.connectionToClient;

                if (connectionToClient != null)
                    connectionToClient.RemoveOwnedObject(identity);

                NetworkServer.SendToObservers(identity, new ObjectDestroyMessage
                {
                    netId = identity.netId
                }, 0);

                identity.ClearObservers();

                if (NetworkClient.active && NetworkServer.activeHost)
                {
                    if (identity.isLocalPlayer)
                        identity.OnStopLocalPlayer();

                    identity.OnStopClient();
                    identity.isOwned = false;
                    identity.NotifyAuthority();

                    NetworkClient.connection.owned.Remove(identity);
                    NetworkClient.spawned.Remove(identity.netId);
                }

                identity.OnStopServer();

                if (mode != NetworkServer.DestroyMode.Destroy)
                {
                    if (mode == NetworkServer.DestroyMode.Reset)
                        identity.Reset();

                    MirrorEvents.OnDestroyed(identity, mode);
                    return false;
                }

                identity.destroyCalled = true;

                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(identity.gameObject);

                    MirrorEvents.OnDestroyed(identity, mode);
                    return false;
                }

                UnityEngine.Object.DestroyImmediate(identity.gameObject);

                MirrorEvents.OnDestroyed(identity, mode);
                return false;
            }
            catch (Exception ex)
            {
                ApiLog.Error("NetworkDestroyPatch", ex);
                return false;
            }
        }
    }
}
