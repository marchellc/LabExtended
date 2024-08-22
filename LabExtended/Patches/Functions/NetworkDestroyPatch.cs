using HarmonyLib;
using LabExtended.Events;

using Mirror;

using UnityEngine;

namespace LabExtended.Patches.Functions
{
    [HarmonyPatch(typeof(NetworkServer), nameof(NetworkServer.DestroyObject), typeof(NetworkIdentity), typeof(NetworkServer.DestroyMode))]
    public static class NetworkDestroyPatch
    {
        public static bool Prefix(NetworkIdentity identity, NetworkServer.DestroyMode mode)
        {
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

            identity.connectionToClient?.RemoveOwnedObject(identity);

            NetworkServer.SendToObservers(identity, new ObjectDestroyMessage
            {
                netId = identity.netId
            });

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

            NetworkEvents.InternalHandleDestroy(identity);

            identity.OnStopServer();

            if (mode != NetworkServer.DestroyMode.Destroy)
            {
                if (mode == NetworkServer.DestroyMode.Reset)
                    identity.Reset();

                return false;
            }

            identity.destroyCalled = true;

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(identity.gameObject);
                return false;
            }

            UnityEngine.Object.DestroyImmediate(identity.gameObject);
            return false;
        }
    }
}
