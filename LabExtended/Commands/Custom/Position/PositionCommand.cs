using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

using LabExtended.Utilities;

using MapGeneration;

using UnityEngine;

namespace LabExtended.Commands.Custom.Position;

[Command("position", "Manages the saving of custom consistent positions.")]
public class PositionCommand : CommandBase, IServerSideCommand
{
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
    
    [CommandOverload("setcur", "Sets a position using the room you are currently in.", null)]
    public void SetCurrent(
        [CommandParameter("Name", "Name of the position.")] string name,
        [CommandParameter("Position", "The value of the position.")] Vector3 position)
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
        
        MapUtilities.SavePosition(name, new()
        {
            Position = new(position),
            
            RoomName = room.Name,
            RoomZone = room.Zone,
            RoomShape = room.Shape
        });
        
        Ok($"Saved position '{name}'!");
    }

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