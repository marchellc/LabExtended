using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

using LabExtended.Utilities;

using MapGeneration;

using UnityEngine;

namespace LabExtended.Commands.Custom.Position;

/// <summary>
/// Provides commands for managing the saving, listing, and removal of custom consistent positions within the system.
/// </summary>
/// <remarks>The PositionCommand class enables users to create, list, and delete named positions, which are
/// associated with specific rooms, shapes, and zones. These commands are typically used in server-side contexts to
/// facilitate navigation, automation, or configuration tasks that require persistent spatial references.</remarks>
[Command("position", "Manages the saving of custom consistent positions.")]
public class PositionCommand : CommandBase, IServerSideCommand
{
    /// <summary>
    /// Lists all saved positions that are not examples and are associated with a named room.
    /// </summary>
    /// <remarks>Positions with keys starting with "example" or associated with unnamed rooms are excluded
    /// from the list. If no valid positions are found, an error message is returned instead of a list.</remarks>
    [CommandOverload("list", "Lists all saved positions.", null)]
    public void List()
    {
        if (MapUtilities.Positions.Count(x => !x.Key.StartsWith("example") && x.Value.RoomName != RoomName.Unnamed) == 0)
        {
            Fail("No valid positions were saved.");
            return;
        }
        
        Ok(x =>
        {
            x.AppendLine($"Showing a list of '{MapUtilities.Positions.Count}' saved position(s):");

            foreach (var pair in MapUtilities.Positions)
            {
                if (pair.Key.ToLower().StartsWith("example"))
                    continue;
                
                if (pair.Value.RoomName is RoomName.Unnamed)
                    continue;
                
                x.AppendLine($"> {pair.Key}");
                x.AppendLine($"  - Position: {pair.Value.Position.Vector.ToPreciseString()}");
                x.AppendLine($"  - Room: {pair.Value.RoomName} (Shape: {pair.Value.RoomShape?.ToString() ?? "(null)"}; Zone: {pair.Value.RoomZone?.ToString() ?? "(null)"})");

                if (RoomUtils.TryFindRoom(pair.Value.RoomName, pair.Value.RoomZone, pair.Value.RoomShape, out var room))
                {
                    x.AppendLine($"  - Active: {room.MainCoords} ({room.GetInstanceID()})");
                }
                else
                {
                    x.AppendLine("  - Active: (null)");
                }
            }
        });
    }

    /// <summary>
    /// Sets and saves a named position within a specified room, shape, and zone.
    /// </summary>
    /// <param name="name">The unique name to assign to the position. Cannot be null or empty.</param>
    /// <param name="position">The coordinates representing the position to save.</param>
    /// <param name="room">The name of the room where the position is located.</param>
    /// <param name="shape">The shape of the room containing the position.</param>
    /// <param name="zone">The facility zone of the room containing the position.</param>
    [CommandOverload("set", "Sets a position.", null)]
    public void Set(
        [CommandParameter("Name", "Name of the position.")] string name,
        [CommandParameter("Position", "The value of the position.")] Vector3 position,
        [CommandParameter("Room", "The name of the room the position is in.")] RoomName room,
        [CommandParameter("Shape", "The shape of the room the position is in.")] RoomShape shape,
        [CommandParameter("Zone", "The zone of the room the position is in.")] FacilityZone zone)
    {
        MapUtilities.SavePosition(name, new()
        {
            Position = new(position),
            
            RoomName = room,
            RoomZone = zone,
            RoomShape = shape
        });
        
        Ok($"Saved position '{name}'!");
    }
    
    /// <summary>
    /// Saves a named position using the sender's current room and the specified coordinates.
    /// </summary>
    /// <param name="name">The name to assign to the saved position. Cannot be null or empty.</param>
    /// <param name="position">The coordinates of the position to save, relative to the current room.</param>
    [CommandOverload("setcur", "Sets a position using the room you are currently in.", null)]
    public void SetCurrent(
        [CommandParameter("Name", "Name of the position.")] string name,
        [CommandParameter("Position", "The value of the position.")] Vector3 position = default)
    {
        var room = Sender.Position.Room;

        if (room == null)
        {
            if (!Sender.Position.Position.TryGetRoom(out room))
            {
                Fail($"Could not get your current room.");
                return;
            }
        }

        if (position == default)
            position = room.transform.TransformPoint(Sender.Position);
        
        MapUtilities.SavePosition(name, new()
        {
            Position = new(position),
            
            RoomName = room.Name,
            RoomZone = room.Zone,
            RoomShape = room.Shape
        });
        
        Ok($"Saved position '{name}'!");
    }

    /// <summary>
    /// Removes a saved position with the specified name.
    /// </summary>
    /// <param name="name">The name of the position to remove. Cannot be null or empty.</param>
    [CommandOverload("remove", "Removes a saved position.", null)]
    public void Remove(
        [CommandParameter("Name", "Name of the position to remove.")] string name)
    {
        if (!MapUtilities.RemovePosition(name))
        {
            Fail($"No such position exists.");
            return;
        }
        
        Ok($"Removed position '{name}'!");
    }
}