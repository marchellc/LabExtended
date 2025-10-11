using HarmonyLib;

using LabExtended.Core;

using LabExtended.Events;
using LabExtended.Events.Mirror;

using Mirror;

using UnityEngine;

namespace LabExtended.Patches.Events.Mirror
{
    /// <summary>
    /// Provides the <see cref="MirrorEvents.Destroying"/> and <see cref="MirrorEvents.Destroyed"/> patches.
    /// </summary>
    public static class MirrorDestroyPatch
    {
        [HarmonyPatch(typeof(NetworkServer), nameof(NetworkServer.DestroyObject), typeof(NetworkIdentity), typeof(NetworkServer.DestroyMode))]
        private static bool Prefix(NetworkIdentity identity, NetworkServer.DestroyMode mode)
        {
            try
            {
                if (MirrorEvents.OnDestroying(new MirrorDestroyingIdentityEventArgs(identity, mode)))
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

                    (identity.connectionToClient as NetworkConnectionToClient)?.RemoveOwnedObject(identity);

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

                    MirrorEvents.OnDestroyed(new MirrorDestroyedIdentityEventArgs(identity, mode));

                    if (mode != NetworkServer.DestroyMode.Destroy)
                    {
                        if (mode == NetworkServer.DestroyMode.Reset)
                            identity.Reset();

                        return false;
                    }

                    identity.destroyCalled = true;

                    if (Application.isPlaying)
                        UnityEngine.Object.Destroy(identity.gameObject);
                    else
                        UnityEngine.Object.DestroyImmediate(identity.gameObject);
                }

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
