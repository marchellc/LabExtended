using AdminToys;

using Footprinting;

using LabExtended.API.Wrappers;

using UnityEngine;

namespace LabExtended.API.Toys
{
    public class AdminToy : NetworkWrapper<AdminToyBase>
    {
        public AdminToy(AdminToyBase baseValue) : base(baseValue) { }

        public Transform Transform => Base.transform;
        public GameObject GameObject => Base.gameObject;

        public string Name => Base.CommandName;

        public byte MovementSmoothing
        {
            get => Base.MovementSmoothing;
            set => Base.NetworkMovementSmoothing = value;
        }

        public bool IsStatic
        {
            get => Base.NetworkIsStatic;
            set => Base.NetworkIsStatic = value;
        }

        public Footprint Spawner
        {
            get => Base.SpawnerFootprint;
            set => Base.SpawnerFootprint = value;
        }

        public override Vector3 Position
        {
            get => Base.Position;
            set => Base.NetworkPosition = value;
        }

        public override Vector3 Scale
        {
            get => Base.Scale;
            set => Base.NetworkScale = value;
        }

        public override Quaternion Rotation
        {
            get => Base.Rotation;
            set => Base.NetworkRotation = value;
        }

        public override void SetPositionAndRotation(Vector3 position, Quaternion rotation, Vector3? scale = null)
        {
            Position = position;
            Rotation = rotation;

            if (scale.HasValue && scale.Value != Scale)
                Scale = scale.Value;
        }

        public static AdminToy Create(AdminToyBase adminToy)
        {
            if (adminToy is null)
                return null;

            return adminToy switch
            {
                LightSourceToy lightSource => new LightToy(lightSource),
                ShootingTarget shootingTarget => new TargetToy(shootingTarget),
                PrimitiveObjectToy primitiveObject => new PrimitiveToy(primitiveObject),

                _ => new AdminToy(adminToy)
            };
        }
    }
}