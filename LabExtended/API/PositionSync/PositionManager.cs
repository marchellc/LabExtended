using LabExtended.Core;
using LabExtended.Attributes;
using LabExtended.Utilities.Update;

using Mirror;

using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.NetworkMessages;

using PlayerRoles.Visibility;

using RelativePositioning;

using UnityEngine;

namespace LabExtended.API.PositionSync;

/// <summary>
/// Manages and sends player positions.
/// </summary>
public static class PositionManager
{
    private static float time = FpcServerPositionDistributor.SendRate;
    
    private static NetworkWriter writer = new();
    private static List<ExPlayer> players = new(byte.MaxValue);
    
    /// <summary>
    /// Whether or not to force-send next frame.
    /// </summary>
    public static bool SendNextFrame { get; set; }

    /// <summary>
    /// Sets the receiver's position cache IsDirty property.
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <param name="receiver">The receiving player.</param>
    /// <param name="isDirty">The new dirty value.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void SetPositionDirty(this ExPlayer player, ExPlayer receiver, bool isDirty)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        if (receiver is null)
            throw new ArgumentNullException(nameof(receiver));

        if (receiver == player)
            return;
        
        var cache = receiver.SentPositions.GetOrAdd(player.NetworkId, () => new());

        cache.IsDirty = isDirty;
    }

    /// <summary>
    /// Sets the IsDirty property for all receiving players.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="isDirty"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void SetAllPositionDirty(this ExPlayer player, bool isDirty)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));
        
        ExPlayer.AllPlayers.ForEach(receiver =>
        {
            if (receiver?.SentPositions is null || receiver == player)
                return;

            var cache = receiver.SentPositions.GetOrAdd(player.NetworkId, () => new());

            cache.IsDirty = isDirty;
        });
    }

    /// <summary>
    /// Sends all positions.
    /// </summary>
    public static void SendPositions()
    {
        time -= Time.deltaTime;

        if (SendNextFrame || time <= 0f)
        {
            try
            {
                ExPlayer.AllPlayers.ForEach(ProcessPlayer);
            }
            catch (Exception ex)
            {
                ApiLog.Error("Position Manager", $"An error occured while sending positions!\n{ex}");
            }
            
            time = FpcServerPositionDistributor.SendRate;
        }

        SendNextFrame = false;
    }

    private static void ProcessPlayer(ExPlayer? receiver)
    {
        if (receiver?.SentPositions is null || receiver.IsUnverified || !receiver.Toggles.ShouldReceivePositions)
            return;
        
        players.Clear();
        
        ExPlayer.AllPlayers.ForEach(player =>
        {
            if (player?.SentPositions is null || player.IsUnverified)
                return;

            if (!player.Toggles.ShouldSendPosition)
                return;

            if (player.NetworkId == receiver.NetworkId && !receiver.Toggles.ShouldReceiveOwnPosition)
                return;

            if (player.RoleBase is not IFpcRole)
                return;
            
            players.Add(player);
        });

        if (players.Count < 1)
            return;
        
        var isGhosted = (ExPlayer.GhostedFlags & receiver.GhostBit) == receiver.GhostBit;
        var isCustomVisibilityRole = receiver.Role.Is<ICustomVisibilityRole>(out var customVisibilityRole);
        
        writer.Reset();
        
        writer.WriteMessageId<FpcPositionMessage>();
        writer.WriteUShort((ushort)players.Count);
        
        players.ForEach(player =>
        {
            var role = player.RoleBase as IFpcRole;
            var module = role.FpcModule;
            var mouse = module.MouseLook;

            var invisible = (isCustomVisibilityRole &&
                             !customVisibilityRole.VisibilityController.ValidateVisibility(player.ReferenceHub))
                || isGhosted
                || (player.PersonalGhostFlags & receiver.GhostBit) == receiver.GhostBit;

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
        });
        
        receiver.Send(writer.ToArraySegment());
    }

    private static void OnRoleChanged(ReferenceHub userHub, PlayerRoleBase prevRole, PlayerRoleBase newRole)
    {
        if (prevRole is not IFpcRole)
            return;

        if (!ExPlayer.TryGet(userHub, out var player))
            return;
        
        player.SetAllPositionDirty(true);
    }
    
    [LoaderInitialize(1)]
    private static void OnInit()
    {
        PlayerUpdateHelper.OnUpdate += SendPositions;
        PlayerRoleManager.OnRoleChanged += OnRoleChanged;
    }
}