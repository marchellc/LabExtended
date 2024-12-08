using AdminToys;

using LabExtended.API.Interfaces;
using LabExtended.API.Prefabs;
using LabExtended.Extensions;
using Mirror;

using UnityEngine;

namespace LabExtended.API.Toys
{
    public class LightToy : AdminToy, IWrapper<LightSourceToy>
    {
        public LightToy(LightSourceToy baseValue) : base(baseValue)
            => Base = baseValue;

        public new LightSourceToy Base { get; }

        public float Intensity
        {
            get => Base.LightIntensity;
            set => Base.NetworkLightIntensity = value;
        }

        public float Range
        {
            get => Base.LightRange;
            set => Base.NetworkLightRange = value;
        }

        public float SpotAngle
        {
            get => Base.SpotAngle;
            set => Base.NetworkSpotAngle = value;
        }

        public float InnerSpotAngle
        {
            get => Base.InnerSpotAngle;
            set => Base.NetworkInnerSpotAngle = value;
        }

        public float ShadowStrength
        {
            get => Base.ShadowStrength;
            set => Base.NetworkShadowStrength = value;
        }

        public LightShadows ShadowType
        {
            get => Base.ShadowType;
            set => Base.NetworkShadowType = value;
        }

        public LightShape Shape
        {
            get => Base.LightShape;
            set => Base.NetworkLightShape = value;
        }

        public Color Color
        {
            get => Base.LightColor;
            set => Base.NetworkLightColor = value;
        }

        public static LightToy Spawn(Vector3 position, Action<LightToy> configure = null)
        {
            var lightSource = UnityEngine.Object.Instantiate(PrefabList.LightSource.GameObject).GetComponent<LightSourceToy>();

            lightSource.SpawnerFootprint = ExPlayer.Host.Footprint;
            lightSource.transform.position = position;

            var toy = new LightToy(lightSource);

            configure.InvokeSafe(toy);

            ExMap._toys.Add(toy);

            NetworkServer.Spawn(lightSource.gameObject);
            return toy;
        }
    }
}