using AdminToys;

using Footprinting;

using LabExtended.API.Toys;
using LabExtended.API.Wrappers;
using LabExtended.Core;
using LabExtended.Patches.Functions;
using LabExtended.Utilities;

using Mirror;

using UnityEngine;

namespace LabExtended.API
{
    public class AdminToy : NetworkWrapper<AdminToyBase>
    {
        static AdminToy()
            => NetworkSpawnPatch.OnSpawned += OnIdentitySpawned;

        public AdminToy(AdminToyBase baseValue) : base(baseValue) { }

        public Transform Transform => Base.transform;
        public GameObject GameObject => Base.gameObject;

        public string Name => Base.CommandName;

        public byte MovementSmoothing
        {
            get => Base.NetworkMovementSmoothing;
            set => Modify(false, false, false, true, () => Base.MovementSmoothing = value);
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
            get => Base.transform.position;
            set => Modify(true, false, false, false, () => Base.transform.position = Base.Position = value);
        }

        public override Vector3 Scale
        {
            get => Base.transform.localScale;
            set => Modify(false, false, true, false, () => Base.transform.localScale = Base.Scale = value);
        }

        public override Quaternion Rotation
        {
            get => Base.transform.rotation;
            set => Modify(false, true, false, false, () =>
            {
                Base.Rotation = new LowPrecisionQuaternion(value);
                Base.transform.rotation = value;
            });
        }

        public override void SetPositionAndRotation(Vector3 position, Quaternion rotation, Vector3? scale = null)
        {
            var writeScale = false;

            Position = position;
            Rotation = rotation;

            if (scale.HasValue && scale.Value != Scale)
            {
                Scale = scale.Value;
                writeScale = true;
            }

            if (IsStatic)
                ResyncStatic(true, true, writeScale, false);
        }

        private void Modify(bool pos, bool rot, bool scale, bool smoothing, Action action)
        {
            action();

            if (IsStatic)
                ResyncStatic(pos, rot, scale, smoothing);
        }

        private void ResyncStatic(bool writePos, bool writeRot, bool writeScale, bool writeSmoothing)
        {
            var msg = NetworkUtils.WriteMessage<EntityStateMessage>(writer =>
            {
                using (var writer2 = NetworkWriterPool.Get())
                {
                    var segment = NetworkUtils.WriteSegment(data =>
                    {
                        var num = 0UL | 1UL << (Identity.NetworkBehaviours.IndexOf(Base) & 31);
                        var mask = 0UL;

                        Compression.CompressVarUInt(data, num);

                        if (writePos)
                            mask |= 1UL;

                        if (writeRot)
                            mask |= 2UL;

                        if (writeScale)
                            mask |= 4UL;

                        if (writeSmoothing)
                            mask |= 8UL;

                        var pos1 = data.Position;

                        data.WriteByte(0);

                        var pos2 = data.Position;

                        data.WriteULong(mask);

                        if (writePos)
                            data.WriteVector3(Base.Position);

                        if (writeRot)
                            data.WriteLowPrecisionQuaternion(Base.Rotation);

                        if (writeScale)
                            data.WriteVector3(Base.Scale);

                        if (writeSmoothing)
                            data.WriteByte(Base.MovementSmoothing);

                        var pos3 = data.Position;

                        data.Position = pos1;

                        var b = (byte)((pos3 - pos2) & 255);

                        data.WriteByte(b);
                        data.Position = pos3;

                        Base.ClearAllDirtyBits();
                    });

                    writer2.WriteBytes(segment.Array, segment.Offset, segment.Count);

                    writer.WriteUInt(NetId);
                    writer.WriteArraySegmentAndSize(writer2.ToArraySegment());
                }
            });

            foreach (var player in ExPlayer.Players)
                player.Connection.Send(msg);
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

        private static void OnIdentitySpawned(NetworkIdentity obj)
        {
            if (obj is null || !obj)
                return;

            if (obj.NetworkBehaviours is null)
                return;

            obj.NetworkBehaviours.ForEach(x =>
            {
                if (x is null || x is not AdminToyBase adminToyBase)
                    return;

                ApiLoader.Debug("Toy API", $"Detected an Admin Toy spawn ({adminToyBase.CommandName} - {adminToyBase.netId})");

                ExMap._toys.Add(Create(adminToyBase));
            });
        }
    }
}