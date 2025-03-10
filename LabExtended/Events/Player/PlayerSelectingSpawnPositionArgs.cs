using LabExtended.API;
using LabExtended.Core.Events;
using UnityEngine;

namespace LabExtended.Events.Player;

public class PlayerSelectingSpawnPositionArgs : BoolCancellableEvent
{
    public ExPlayer? Player { get; }
    
    public Vector3 Position { get; set; }
    
    public float Rotation { get; set; }

    public PlayerSelectingSpawnPositionArgs(ExPlayer? player, Vector3 position, float rotation)
    {
        Player = player;
        Position = position;
        Rotation = rotation;
    }
}