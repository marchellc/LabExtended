using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.Utilities.Update;

using Mirror;

using PlayerRoles;
using PlayerRoles.Visibility;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.NetworkMessages;

using RelativePositioning;

using UnityEngine;

namespace LabExtended.Utilities;

/// <summary>
/// An optimized position synchronizer.
/// </summary>
public static class PositionSync
{
    /// <summary>
    /// Describes a position previously sent to a player.
    /// </summary>
    public class SentPosition
    {
        /// <summary>
        /// Gets or sets the horizontal mouse axis.
        /// </summary>
        public ushort LookHorizontal { get; set; } = 0;
    
        /// <summary>
        /// Gets or sets the vertical mouse axis.
        /// </summary>
        public ushort LookVertical { get; set; } = 0;

        /// <summary>
        /// Whether or not the cached position is invalid.
        /// </summary>
        public bool IsDirty { get; set; } = true;
    
        /// <summary>
        /// Gets or sets the previous previous.
        /// </summary>
        public RelativePosition Position { get; set; }
    }
    
    private static NetworkWriter writer = new();
    private static float time = FpcServerPositionDistributor.SendRate;

    /// <summary>
    /// Whether or not the custom position sync system should be enabled.
    /// </summary>
    public static bool IsEnabled { get; set; } = true;

    private static void Internal_Update()
    {
        time -= Time.deltaTime;

        if (time > 0f)
            return;
        
        time = FpcServerPositionDistributor.SendRate;
        
        ExPlayer.AllPlayers.ForEach(receiver =>
        {
            if (receiver.ReferenceHub == null)
                return;

            if (!receiver.Toggles.ShouldReceivePositions)
                return;
            
            var controller = receiver.Role.Is<ICustomVisibilityRole>(out var customVisibilityRole)
                             && customVisibilityRole.VisibilityController != null;

            var count = 0;
            
            writer.Reset();
            writer.Position = 4; // 2x UShort for Message ID and player count
            
            ExPlayer.AllPlayers.ForEach(player =>
            {
                if (player.ReferenceHub != null
                    && player.Role.Is<IFpcRole>(out var fpcRole)
                    && player.Toggles.ShouldSendPosition
                    && (player != receiver || player.Toggles.ShouldReceiveOwnPosition))
                {
                    var module = fpcRole.FpcModule;
                    var mouse = module.MouseLook;

                    var invisible = (controller &&
                                     !customVisibilityRole.VisibilityController.ValidateVisibility(player.ReferenceHub))
                                    || (ExPlayer.GhostedFlags & player.GhostBit) == player.GhostBit
                                    || (receiver.PersonalGhostFlags & player.GhostBit) == player.GhostBit;
                    
                    var validatedVisibilityArgs =
                        new PlayerValidatedVisibilityEventArgs(receiver.ReferenceHub, player.ReferenceHub, !invisible);

                    PlayerEvents.OnValidatedVisibility(validatedVisibilityArgs);

                    invisible = !validatedVisibilityArgs.IsVisible;

                    var cache = receiver.SentPositions.GetOrAdd(player.NetworkId, () => new());

                    var writePosition = false;
                    var writeLook = false;

                    var position = invisible
                        ? FpcMotor.InvisiblePosition
                        : player.Transform.position;

                    if (player.Position.FakedList.HasGlobalValue)
                        position = player.Position.FakedList.GlobalValue;
                    else if (player.Position.FakedList.TryGetValue(player, out var fakePosition))
                        position = fakePosition;

                    var relative = new RelativePosition(position);

                    if (cache.IsDirty || cache.Position != relative)
                        writePosition = true;

                    mouse.GetSyncValues(relative.WaypointId, out var syncH, out var syncV);

                    if (cache.IsDirty || cache.LookHorizontal != syncH || cache.LookVertical != syncV)
                        writeLook = true;

                    cache.LookHorizontal = syncH;
                    cache.LookVertical = syncV;

                    cache.Position = relative;

                    cache.IsDirty = false;

                    Misc.ByteToBools((byte)module.SyncMovementState, out var b1, out var b2, out var b3,
                        out var b4, out var b5, out var b6, out var b7, out var b8);

                    writer.WriteRecyclablePlayerId(player.ReferenceHub.Network_playerId);
                    writer.WriteByte(Misc.BoolsToByte(b1, b1, b3, b4, b5, writeLook, writePosition, module.IsGrounded));

                    if (writePosition)
                    {
                        writer.WriteRelativePosition(relative);
                    }

                    if (writeLook)
                    {
                        writer.WriteUShort(syncH);
                        writer.WriteUShort(syncV);
                    }

                    count++;
                }

                if (RoleSync.IsEnabled)
                    RoleSync.Internal_CheckDirtyRole(player, receiver);
            });

            if (count > 0)
            {
                var curPosition = writer.Position;
                
                writer.Position = 0;
                
                writer.WriteMessageId<FpcPositionMessage>();
                writer.WriteUShort((ushort)count);
                
                writer.Position = curPosition;
                
                receiver.Send(writer.ToArraySegment());
            }
        });
    }

    internal static void Internal_Reset(ExPlayer player)
    {
        ExPlayer.AllPlayers.ForEach(p =>
        {
            if (p.SentPositions.TryGetValue(player.NetworkId, out var sentPosition))
            {
                sentPosition.IsDirty = true;
            }
            
            if (player.SentPositions.TryGetValue(p.NetworkId, out sentPosition))
            {
                sentPosition.IsDirty = true;
            }
        });
    }
    
    internal static void Internal_Init()
    {
        if (!IsEnabled)
            return;
        
        PlayerUpdateHelper.Component.OnUpdate += Internal_Update;
        PlayerRoleManager.OnRoleChanged -= FpcServerPositionDistributor.ResetPlayer;
    }
}