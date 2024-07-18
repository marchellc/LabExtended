using AdminToys;
using LabExtended.API;
using LabExtended.Core.Commands;
using LabExtended.Core.Commands.Arguments;
using LabExtended.Core.Commands.Interfaces;
using LabExtended.Utilities;

using UnityEngine;

namespace LabExtended.API.CustomCommands.Debug.Toys
{
    public class SpawnPrimitiveCommand : CommandInfo
    {
        public override string Command => "spawnprimitive";
        public override string Description => "Spawns a primitive object.";

        public override ICommandArgument[] Arguments { get; set; } = new ICommandArgument[]
        {
            new GenericArgument<PrimitiveType>("Type", "The type of the primitive object (Cube, Capsule, Plane, Quad, Sphere, Cylinder)"),
            new GenericArgument<PrimitiveFlags>("Flags", "Flags for the primitive object (Collidable, Visible, None)", PrimitiveFlags.Collidable | PrimitiveFlags.Visible) { IsOptional = true },
            new GenericArgument<Vector3>("Scale", "The object's scale.", Vector3.one) { IsOptional = true }
        };

        public object OnCalled(ExPlayer sender, PrimitiveType type, PrimitiveFlags flags, Vector3 scale)
        {
            var primitive = PrimitiveUtils.SpawnPrimitive(sender.Position, sender.Rotation, scale == default ? Vector3.one : scale, type, flags);
            return $"Primitive object spawned! (Net ID: {primitive.netId})";
        }
    }
}