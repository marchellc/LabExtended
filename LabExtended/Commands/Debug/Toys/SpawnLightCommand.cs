using LabExtended.API;

using LabExtended.Core.Commands;
using LabExtended.Core.Commands.Arguments;
using LabExtended.Core.Commands.Interfaces;

using LabExtended.Utilities;

using UnityEngine;

namespace LabExtended.Commands.Debug.Toys
{
    public class SpawnLightCommand : CommandInfo
    {
        public override string Command => "spawnlight";
        public override string Description => "Spawns a new light.";

        public override ICommandArgument[] Arguments { get; set; } = new ICommandArgument[]
        {
            new GenericArgument<Vector3>("Scale", "The light's scale."),
            new GenericArgument<Color>("Color", "The light's color.", Color.white) { IsOptional = true }
        };

        public object OnCalled(ExPlayer player, Vector3 scale, Color color)
        {
            if (scale == default)
                scale = Vector3.one;

            var light = PrimitiveUtils.SpawnLight(player.Position, scale);
            return $"Light spawned! (Net ID: {light.netId})";
        }
    }
}