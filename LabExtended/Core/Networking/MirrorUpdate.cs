using LabExtended.Attributes;
using LabExtended.Core.Networking.Synchronization.Position;
using LabExtended.Core.Networking.Synchronization.Role;
using LabExtended.Extensions;
using LabExtended.Utilities.Unity;

using Mirror;

using UnityEngine;

namespace LabExtended.Core.Networking;

/// <summary>
/// Runs Mirror's internal update loop via Unity's Awaitable
/// </summary>
public static class MirrorUpdate
{
    /// <summary>
    /// Whether or not the caller is enabled.
    /// </summary>
    public static bool IsEnabled
    {
        get => field;
        set
        {
            if (value == field)
                return;

            field = value;
            
            if (value)
                OnUpdateAsync();
        }
    }
    
    private static async Awaitable OnUpdateAsync()
    {
        while (IsEnabled)
        {
            await Awaitable.NextFrameAsync();

            RoleSynchronizer.OnUpdate();
            PositionSynchronizer.OnUpdate();

            NetworkServer.NetworkEarlyUpdate();
            NetworkClient.NetworkEarlyUpdate();

            NetworkLoop.OnEarlyUpdate.InvokeSafe();

            await Awaitable.EndOfFrameAsync();

            NetworkLoop.OnLateUpdate.InvokeSafe();

            NetworkServer.NetworkLateUpdate();
            NetworkClient.NetworkLateUpdate();
        }
    }

    [LoaderInitialize(1)]
    private static void OnInit()
        => IsEnabled = ApiLoader.ApiConfig.OtherSection.MirrorAsync;
}