﻿using LabExtended.API.Interfaces;
using LabExtended.Core;
using LabExtended.Utilities;

using Mirror;

using PlayerRoles.FirstPersonControl.NetworkMessages;
using PlayerRoles.PlayableScps;

using RelativePositioning;

using UnityEngine;

namespace LabExtended.API.Containers
{
    /// <summary>
    /// A class used to manage player rotation.
    /// </summary>
    public class RotationContainer
    {
        /// <summary>
        /// Creates a new <see cref="RotationContainer"/> instance.
        /// </summary>
        /// <param name="player">The targeted player.</param>
        public RotationContainer(ExPlayer player)
            => Player = player;

        /// <summary>
        /// Gets the targeted player.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// Gets the player that this player is currently looking at.
        /// </summary>
        public ExPlayer LookingAtPlayer => ExPlayer.Players.FirstOrDefault(x => IsLookingAt(x));

        /// <summary>
        /// Gets a list of players that are currently in the line of sight of this player.
        /// </summary>
        public IEnumerable<ExPlayer> PlayersInSight => ExPlayer.Players.Where(p => p.Rotation.IsInLineOfSight(Player));

        /// <summary>
        /// Gets or sets the player's rotation
        /// </summary>
        public Quaternion Rotation
        {
            get => Player.Transform.rotation;
            set => Set(value);
        }

        /// <summary>
        /// Gets the player's camera rotation.
        /// </summary>
        public Quaternion CameraRotation => Player.CameraTransform.rotation;

        /// <summary>
        /// Gets the player's camera rotation, compressed.
        /// </summary>
        public CompressedRotation Compressed => new CompressedRotation(Rotation);

        /// <summary>
        /// Gets the player's camera rotation's euler angles.
        /// </summary>
        public Vector3 CameraEuler => Player.CameraTransform.rotation.eulerAngles;

        /// <summary>
        /// Gets the player's camera forward direction.
        /// </summary>
        public Vector3 CameraForward => Player.CameraTransform.forward;

        /// <summary>
        /// Gets the player's camera position.
        /// </summary>
        public Vector3 CameraPosition => Player.CameraTransform.position;

        /// <summary>
        /// Gets or sets the euler angles of the player's rotation.
        /// </summary>
        public Vector3 Euler
        {
            get => Player.Transform.rotation.eulerAngles;
            set => SetEulerAngles(value);
        }

        /// <summary>
        /// Gets or sets the player's rotation as a <see cref="LowPrecisionQuaternion"/>.
        /// </summary>
        public LowPrecisionQuaternion LowPrecision
        {
            get => new LowPrecisionQuaternion(Rotation);
            set => Rotation = value.Value;
        }

        /// <summary>
        /// Whether or not a specific player is in this player's line of sight.
        /// </summary>
        /// <param name="player">The player to check for.</param>
        /// <param name="radius">The maximum radius.</param>
        /// <param name="distance">The maximum distance.</param>
        /// <param name="countSpectating">Whether or not to count spectating the target player.</param>
        /// <returns><see langword="true"/> if the player is in line of sight.</returns>
        public bool IsInLineOfSight(ExPlayer player, float radius = 0.12f, float distance = 60f, bool countSpectating = true)
            => player != null && player.Role.IsAlive && ((countSpectating && player.IsSpectatedBy(Player)) || IsInLineOfSight(player.CameraTransform.position, radius, distance));

        /// <summary>
        /// Whether or not a specific <see cref="Transform"/> is in line of sight.
        /// </summary>
        /// <param name="transform">The <see cref="Transform"/> to check for.</param>
        /// <param name="radius">The maximum radius.</param>
        /// <param name="distance">The maximum distance.</param>
        /// <returns><see langword="true"/> if the <see cref="Transform"/> is in line of sight.</returns>
        public bool IsInLineOfSight(Transform transform, float radius = 0.12f, float distance = 60f)
            => transform != null && IsInLineOfSight(transform.position, radius, distance);

        /// <summary>
        /// Whether or not a specific <see cref="GameObject"/> is in line of sight.
        /// </summary>
        /// <param name="gameObject">The <see cref="GameObject"/> to check for.</param>
        /// <param name="radius">The maximum radius.</param>
        /// <param name="distance">The maximum distance.</param>
        /// <returns><see langword="true"/> if the <see cref="GameObject"/> is in line of sight.</returns>
        public bool IsInLineOfSight(GameObject gameObject, float radius = 0.12f, float distance = 60f)
            => gameObject != null && IsInLineOfSight(gameObject.transform.position, radius, distance);

        /// <summary>
        /// Whether or not a specific <see cref="MonoBehaviour"/> is in line of sight.
        /// </summary>
        /// <param name="behaviour">The <see cref="MonoBehaviour"/> to check for.</param>
        /// <param name="radius">The maximum radius.</param>
        /// <param name="distance">The maximum distance.</param>
        /// <returns><see langword="true"/> if the <see cref="MonoBehaviour"/> is in line of sight.</returns>
        public bool IsInLineOfSight(MonoBehaviour behaviour, float radius = 0.12f, float distance = 60f)
            => behaviour != null && IsInLineOfSight(behaviour.transform.position, radius, distance);

        /// <summary>
        /// Whether or not a specific <see cref="IPosition"/> is in line of sight.
        /// </summary>
        /// <param name="position">The <see cref="IPosition"/> to check for.</param>
        /// <param name="radius">The maximum radius.</param>
        /// <param name="distance">The maximum distance.</param>
        /// <returns><see langword="true"/> if the <see cref="IPosition"/> is in line of sight.</returns>
        public bool IsInLineOfSight(IPosition position, float radius = 0.12f, float distance = 60f)
            => position != null && IsInLineOfSight(position.Position, radius, distance);

        /// <summary>
        /// Whether or not a specific position is in line of sight.
        /// </summary>
        /// <param name="position">The position to check for.</param>
        /// <param name="radius">The maximum radius.</param>
        /// <param name="distance">The maximum distance.</param>
        /// <returns><see langword="true"/> if the position is in line of sight.</returns>
        public bool IsInLineOfSight(Vector3 position, float radius = 0.12f, float distance = 60f)
            => VisionInformation.GetVisionInformation(Player.Hub, Player.CameraTransform, position, radius, distance).IsInLineOfSight;

        /// <summary>
        /// Whether or not a specific player is being looked at by this player.
        /// </summary>
        /// <param name="player">The player to check for.</param>
        /// <param name="radius">The maximum radius.</param>
        /// <param name="distance">The maximum distance.</param>
        /// <param name="countSpectating">Whether or not to count spectating the target player.</param>
        /// <returns><see langword="true"/> if the player is being looked at.</returns>
        public bool IsLookingAt(ExPlayer player, float radius = 0.12f, float distance = 60f, bool countSpectating = true)
            => player != null && player.Role.IsAlive && ((player.IsSpectatedBy(Player) && countSpectating) || IsLookingAt(player.Rotation.CameraPosition));

        /// <summary>
        /// Whether or not a specific <see cref="Transform"/> is being looked at by this player.
        /// </summary>
        /// <param name="transform">The <see cref="Transform"/> to check for.</param>
        /// <param name="radius">The maximum radius.</param>
        /// <param name="distance">The maximum distance.</param>
        /// <returns><see langword="true"/> if the <see cref="Transform"/> is being looked at.</returns>
        public bool IsLookingAt(Transform transform, float radius = 0.12f, float distance = 60f)
            => transform != null && IsLookingAt(transform.position, radius, distance);

        /// <summary>
        /// Whether or not a specific <see cref="GameObject"/> is being looked at by this player.
        /// </summary>
        /// <param name="gameObject">The <see cref="GameObject"/> to check for.</param>
        /// <param name="radius">The maximum radius.</param>
        /// <param name="distance">The maximum distance.</param>
        /// <returns><see langword="true"/> if the <see cref="GameObject"/> is being looked at.</returns>
        public bool IsLookingAt(GameObject gameObject, float radius = 0.12f, float distance = 60f)
            => gameObject != null && IsLookingAt(gameObject.transform.position, radius, distance);

        /// <summary>
        /// Whether or not a specific <see cref="MonoBehaviour"/> is being looked at by this player.
        /// </summary>
        /// <param name="behaviour">The <see cref="MonoBehaviour"/> to check for.</param>
        /// <param name="radius">The maximum radius.</param>
        /// <param name="distance">The maximum distance.</param>
        /// <returns><see langword="true"/> if the <see cref="MonoBehaviour"/> is being looked at.</returns>
        public bool IsLookingAt(MonoBehaviour behaviour, float radius = 0.12f, float distance = 60f)
            => behaviour != null && IsLookingAt(behaviour.transform.position, radius, distance);

        /// <summary>
        /// Whether or not a specific <see cref="IPosition"/> is being looked at by this player.
        /// </summary>
        /// <param name="position">The <see cref="IPosition"/> to check for.</param>
        /// <param name="radius">The maximum radius.</param>
        /// <param name="distance">The maximum distance.</param>
        /// <returns><see langword="true"/> if the <see cref="IPosition"/> is being looked at.</returns>
        public bool IsLookingAt(IPosition position, float radius = 0.12f, float distance = 60f)
            => position != null && IsLookingAt(position.Position, radius, distance);

        /// <summary>
        /// Whether or not a specific position is being looked at by this player.
        /// </summary>
        /// <param name="position">The position to check for.</param>
        /// <param name="radius">The maximum radius.</param>
        /// <param name="distance">The maximum distance.</param>
        /// <returns><see langword="true"/> if the position is being looked at.</returns>
        public bool IsLookingAt(Vector3 position, float radius = 0.12f, float distance = 60f)
            => VisionInformation.GetVisionInformation(Player.Hub, Player.CameraTransform, position, radius, distance).IsLooking;

        /// <summary>
        /// Sets the player's rotation.
        /// </summary>
        /// <param name="angles">The rotation's euler angles to set.</param>
        public void SetEulerAngles(Vector3 angles)
            => Set(new CompressedRotation(Quaternion.Euler(angles)));

        /// <summary>
        /// Sets the player's rotation.
        /// </summary>
        /// <param name="rotation">The rotation to set.</param>
        public void Set(Vector3 rotation)
            => Set(new CompressedRotation(rotation));

        /// <summary>
        /// Sets the player's rotation.
        /// </summary>
        /// <param name="rotation">The rotation to set.</param>
        public void Set(Quaternion rotation)
            => Set(new CompressedRotation(rotation));

        /// <summary>
        /// Sets the player's rotation.
        /// </summary>
        /// <param name="rotation">The rotation to set.</param>
        public void Set(CompressedRotation rotation)
            => Set(rotation.HorizontalAxis, rotation.VerticalAxis);

        /// <summary>
        /// Sets the player's rotation.
        /// </summary>
        /// <param name="compressedHorizontal">The horizontal axis to set.</param>
        /// <param name="compressedVertical">The vertical axis to set.</param>
        /// <param name="sendPosition">Whether or not to send a position.</param>
        /// <param name="positionToSend">The position to send.</param>
        public void Set(ushort compressedHorizontal, ushort compressedVertical, bool sendPosition = false, RelativePosition? positionToSend = null)
            => Player.Connection.WriteAndSend<FpcPositionMessage>(writer => InternalWrite(writer, compressedHorizontal, compressedVertical, sendPosition, positionToSend));

        private void InternalWrite(NetworkWriter writer, ushort horizontal, ushort vertical, bool writePosition, RelativePosition? positionToSend)
        {
            var module = Player.Role.MovementModule;

            if (module is null)
                throw new Exception($"The player's movement module is null!");

            Misc.ByteToBools((byte)module.SyncMovementState, out var b1, out var b2, out var b3, out var b4, out var b5, out var b6, out var b7, out var b8);

            var b = Misc.BoolsToByte(b1, b2, b3, b4, b5, true, writePosition, module.IsGrounded);
            var pos = positionToSend.HasValue ? positionToSend.Value : Player.Position.Relative;
            var id = Player.Hub.Network_playerId;

            writer.WriteUShort(2); // Data count

            writer.WriteRecyclablePlayerId(id);
            writer.WriteByte(b); // Included data

            if (writePosition)
                writer.WriteRelativePosition(pos);

            writer.WriteUShort(horizontal++);
            writer.WriteUShort(vertical++);

            writer.WriteRecyclablePlayerId(id);
            writer.WriteByte(b);

            if (writePosition)
                writer.WriteRelativePosition(pos);

            writer.WriteUShort(--horizontal);
            writer.WriteUShort(--vertical);
        }

        /// <summary>
        /// Converts the specified <see cref="RotationContainer"/> to a <see cref="Quaternion"/>.
        /// </summary>
        /// <param name="container">The instance to convert.</param>
        public static implicit operator Quaternion(RotationContainer container)
            => container?.Rotation ?? Quaternion.identity;

        /// <summary>
        /// Converts the specified <see cref="RotationContainer"/> to a <see cref="Vector3"/>.
        /// </summary>
        /// <param name="container">The instance to convert.</param>
        public static implicit operator Vector3(RotationContainer container)
            => container?.Euler ?? Vector3.zero;

        /// <summary>
        /// Converts the specified <see cref="RotationContainer"/> to a <see cref="CompressedRotation"/>.
        /// </summary>
        /// <param name="container">The instance to convert.</param>
        public static implicit operator CompressedRotation(RotationContainer container)
            => container?.Compressed ?? new CompressedRotation(0, 0);
    }
}