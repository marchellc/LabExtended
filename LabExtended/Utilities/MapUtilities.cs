using System.ComponentModel;

using LabExtended.Core;
using LabExtended.Core.Configs.Objects;

using MapGeneration;

using UnityEngine;

namespace LabExtended.Utilities;

/// <summary>
/// Utilities targeting the in-game map.
/// </summary>
public static class MapUtilities
{
    /// <summary>
    /// Contains information about a named position.
    /// </summary>
    public class NamedPosition
    {
        /// <summary>
        /// Gets or sets the YAML-serializable position.
        /// </summary>
        [Description("The local position of the object inside the room.")]
        public YamlVector3 Position { get; set; } = new(Vector3.zero);

        /// <summary>
        /// Gets or sets the name of the room.
        /// </summary>
        [Description("The name of the target room.")]
        public RoomName RoomName { get; set; } = RoomName.Unnamed;

        /// <summary>
        /// Gets or sets the shape of the room.
        /// </summary>
        [Description("The shapee of the target room.")]
        public RoomShape? RoomShape { get; set; } = null;

        /// <summary>
        /// Gets or sets the zone of the room.
        /// </summary>
        [Description("The zone of the target room.")]
        public FacilityZone? RoomZone { get; set; } = null;
    }

    /// <summary>
    /// Gets a list of all named positions loaded from the config.
    /// </summary>
    public static Dictionary<string, NamedPosition> Positions => ApiLoader.BaseConfig.Positions;

    /// <summary>
    /// Saves a position.
    /// </summary>
    /// <param name="name">The name of the position.</param>
    /// <param name="position">The value of the position.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void SavePosition(string name, NamedPosition position)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        if (position is null)
            throw new ArgumentNullException(nameof(position));
        
        Positions[name] = position;
        
        ApiLoader.SaveConfig();
    }

    /// <summary>
    /// Removes a position.
    /// </summary>
    /// <param name="name">The name of the position.</param>
    /// <returns>true if the position was removed</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool RemovePosition(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        if (!Positions.Remove(name))
            return false;
        
        ApiLoader.SaveConfig();
        return true;
    }

    /// <summary>
    /// Attempts to find a position by it's name.
    /// </summary>
    /// <param name="positionName">The name of the position.</param>
    /// <param name="namedPosition">The found position.</param>
    /// <returns>true if the position was found</returns>
    public static bool TryGet(string positionName, out NamedPosition namedPosition) 
        => Positions.TryGetValue(positionName, out namedPosition);

    /// <summary>
    /// Attempts to find a world-space position by it's name.
    /// </summary>
    /// <param name="positionName">The name of the position.</param>
    /// <param name="worldPosition">The converted world-space position.</param>
    /// <returns>true if the position and it's corresponding room were found</returns>
    public static bool TryGet(string positionName, out Vector3 worldPosition)
    {
        if (!TryGet(positionName, out NamedPosition position))
        {
            worldPosition = default;
            return false;
        }

        if (!RoomUtils.TryFindRoom(position.RoomName, position.RoomZone, position.RoomShape, out var room))
        {
            worldPosition = default;
            return false;
        }

        worldPosition = room.transform.TransformPoint(position.Position.Vector);
        return true;
    }
}