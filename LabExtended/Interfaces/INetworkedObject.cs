using LabExtended.API;

using Mirror;

using UnityEngine;

namespace LabExtended.Interfaces
{
    /// <summary>
    /// Represents an object that has it's properties synchronized with all players on the server.
    /// </summary>
    public interface INetworkedObject : IMapObject, INetworkedPosition, INetworkedRotation
    {
        /// <summary>
        /// Gets or sets a value indicating whether or not the object is currently spawned.
        /// </summary>
        bool IsSpawned { get; set; }

        /// <summary>
        /// Gets or sets the object's network ID.
        /// </summary>
        uint NetId { get; set; }

        /// <summary>
        /// Gets the object's network identity.
        /// </summary>
        NetworkIdentity Identity { get; }

        /// <summary>
        /// Gets or sets the object's scale.
        /// </summary>
        Vector3 Scale { get; set; }

        /// <summary>
        /// Gets the object's scale for a specific player.
        /// </summary>
        /// <param name="player">The player to get a scale for.</param>
        /// <returns>The scale specific to this player.</returns>
        Vector3 ScaleFor(ExPlayer player);

        /// <summary>
        /// Spawns this object, if not spawned.
        /// </summary>
        void Spawn();

        /// <summary>
        /// Despawns this object, if spawned.
        /// </summary>
        void Despawn();

        /// <summary>
        /// Teleports this object to the specified position.
        /// </summary>
        /// <param name="destination">The position to teleport this object to.</param>
        void Teleport(Vector3 destination);

        /// <summary>
        /// Teleports this object to the specified position.
        /// </summary>
        /// <param name="destination">The position to teleport this object to.</param>
        void Teleport(IPosition destination);

        /// <summary>
        /// Despawns this object for the specified players.
        /// </summary>
        /// <param name="players">A list of players to hide this object for.</param>
        void DespawnFor(params ExPlayer[] players);

        /// <summary>
        /// Spawns this object for the specified players.
        /// </summary>
        /// <param name="players">A list of players to show this object for.</param>
        void SpawnFor(params ExPlayer[] players);

        /// <summary>
        /// Sets a scale for the specified players.
        /// </summary>
        /// <param name="scale">The scale to send to these players.</param>
        /// <param name="args">The list of players to send this fake scale to.</param>
        void SetScaleFor(Vector3 scale, params ExPlayer[] args);

        /// <summary>
        /// Removes a fake scale for a list of players.
        /// </summary>
        /// <param name="args">The list of players to remove a fake scale for.</param>
        void RemoveScaleFor(params ExPlayer[] args);

        /// <summary>
        /// Gets a value indicating whether or not this object is hidden for a specific player.
        /// </summary>
        /// <param name="player">The player to get the information for.</param>
        /// <returns>A value indicating whether or not this object is hidden for a specific player.</returns>
        bool IsSpawnedFor(ExPlayer player);
    }
}