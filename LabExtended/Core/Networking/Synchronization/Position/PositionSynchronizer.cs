﻿using LabApi.Events;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.Attributes;
using LabExtended.Extensions;

using LabExtended.Events;

using LabExtended.Utilities.Unity;

using Mirror;

using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.NetworkMessages;

using PlayerRoles.Visibility;

using RelativePositioning;

using UnityEngine;
using UnityEngine.PlayerLoop;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace LabExtended.Core.Networking.Synchronization.Position;

/// <summary>
/// Used to synchronize player positions (replaces the base game's way).
/// </summary>
public static class PositionSynchronizer
{
    /// <summary>
    /// Exposes the update loop.
    /// </summary>
    public struct PositionUpdateLoop { }

    private static NetworkWriter writer = NetworkWriterPool.Get();
    private static ExPlayer[]? validBuffer;
    private static float sendTime = 0f;

    /// <summary>
    /// Gets the current position send delay.
    /// </summary>
    public static float SendRate => ApiLoader.ApiConfig.SynchronizationSection.PositionSyncRate > 0f
        ? ApiLoader.ApiConfig.SynchronizationSection.PositionSyncRate
        : FpcServerPositionDistributor.SendRate;

    // Gets called each frame
    internal static void OnUpdate()
    {
        // Check send delay
        sendTime -= Time.deltaTime;

        if (sendTime > 0f)
            return;

        sendTime = SendRate;

        try
        {
            // Initialize or resize the buffer
            if (validBuffer is null || validBuffer.Length < ExPlayer.AllCount)
                validBuffer = new ExPlayer[ExPlayer.AllCount];
            
            // Share positions of each player
            ExPlayer.AllPlayers.ForEach(player =>
            {
                if (player is null || !player.Toggles.ShouldReceivePositions)
                    return;

                var ghosted = (ExPlayer.GhostedFlags & (1 << player.PlayerId)) != 0; // Handle invisibility
                var hasRole = player.Role.Is<ICustomVisibilityRole>(out var customVisibilityRole); // Handle base game invisibility
                
                var bufferIndex = 0;

                // Get a list of valid receivers (into validBuffer)
                ExPlayer.AllPlayers.ForEach(other =>
                {
                    if (!other.Toggles.ShouldSendPosition) return;
                    if (!other.Role.Is<IFpcRole>(out _)) return;
                    if (other.NetworkId == player.NetworkId && !player.Toggles.ShouldReceiveOwnPosition) return;

                    validBuffer[bufferIndex++] = other;
                });

                if (bufferIndex < 1)
                    return;

                writer.Reset();

                writer.WriteMessageId<FpcPositionMessage>(); // Write the message ID
                writer.WriteUShort((ushort)bufferIndex); // Write the amount of players

                // Loop through the buffer again
                // TODO: Write player data in previous loop, set writer position to player count & set position for even more performance improvements
                for (var y = 0; y < bufferIndex; y++)
                {
                    var other = validBuffer[y];
                    var fpcRole = (IFpcRole)other.Role.Role;
                    var mouseLook = fpcRole.FpcModule.MouseLook;
                    var module = fpcRole.FpcModule;

                    var isInvisible = hasRole &&
                                      !customVisibilityRole.VisibilityController.ValidateVisibility(
                                          other.ReferenceHub);

                    if (!isInvisible && (ghosted || (other.PersonalGhostFlags & (1 << player.PlayerId)) != 0))
                        isInvisible = true;

                    if (!player.SentPositions.TryGetValue(other.NetworkId, out var prevData))
                        player.SentPositions[other.NetworkId] = prevData = new();

                    // The game handles invisibility by putting players in a position where no one can see them ..
                    var position = isInvisible ? FpcMotor.InvisiblePosition : other.Transform.position;
                    var posBit = false;

                    if (player.Position.FakedList.HasGlobalValue)
                    {
                        position = player.Position.FakedList.GlobalValue;
                        posBit = true;
                    }

                    if (player.Position.FakedList.TryGetValue(other, out var fakedPos))
                    {
                        position = fakedPos;
                        posBit = true;
                    }

                    var relative = new RelativePosition(position);

                    // Only send position if the player's position changed OR if the player's role changed.
                    if (prevData.IsReset || prevData.Position != position)
                        posBit = true;

                    mouseLook.GetSyncValues(relative.WaypointId, out var syncH, out var syncV);

                    var lookBit = prevData.IsReset || prevData.SyncH != syncH || prevData.SyncV != syncV;
                    var customBit = module.IsGrounded;

                    prevData.Position = position;
                    prevData.RelativePosition = relative;

                    prevData.SyncH = syncH;
                    prevData.SyncV = syncV;

                    // This is some real compression
                    Misc.ByteToBools((byte)module.SyncMovementState, out var b1, out var b2, out var b3,
                        out var b4, out var b5, out var b6, out var b7, out var b8);

                    writer.WriteRecyclablePlayerId(other.ReferenceHub.Network_playerId);
                    writer.WriteByte(Misc.BoolsToByte(b1, b1, b3, b4, b5, lookBit, posBit, customBit));

                    if (posBit)
                        writer.WriteRelativePosition(relative);

                    if (lookBit)
                    {
                        writer.WriteUShort(syncH);
                        writer.WriteUShort(syncV);
                    }

                    prevData.IsReset = false;
                }

                // Send the compiled data
                player.Send(writer.ToArraySegment());
            });
        }
        catch (Exception ex)
        {
            ApiLog.Error("Position Synchronizer",
                $"Caught an error while synchronizing player positions:\n{ex.ToColoredString()}");
        }
    }

    // Handles role changed
    // Data must be reset when a role changes, otherwise weird shit starts to happen
    private static void OnSpawned(PlayerSpawnedEventArgs args)
    {
        if (args.Player is not ExPlayer player)
            return;

        if (args.Role is not IFpcRole)
            return;

        ExPlayer.AllPlayers.ForEach(ply =>
        {
            if (ply != args.Player &&
                player.SentPositions != null &&
                player.SentPositions.TryGetValue(ply.NetworkId, out var sent))
            {
                sent.IsReset = true;
            }
        });
    }

    // Clears the buffer so we don't encounter exceptions
    private static void OnRoundRestart()
    {
        Array.Clear(validBuffer, 0, validBuffer.Length);
    }

    [LoaderInitialize(3)]
    private static void OnInit()
    {
        typeof(PlayerEvents).InsertFirst<LabEventHandler<PlayerSpawnedEventArgs>>(nameof(PlayerEvents.Spawned),
            OnSpawned);

        InternalEvents.OnRoundRestart += OnRoundRestart;

        if (ApiLoader.ApiConfig.OtherSection.MirrorAsync)
            return;
        
        PlayerLoopHelper.ModifySystem(x =>
        {
            if (!x.InjectAfter<TimeUpdate.WaitForLastPresentationAndUpdateTime>(OnUpdate, typeof(PositionUpdateLoop)))
                return null;

            return x;
        });
    }
}