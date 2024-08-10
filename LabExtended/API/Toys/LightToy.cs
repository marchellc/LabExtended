using AdminToys;

using LabExtended.API.Interfaces;

using Mirror;

using UnityEngine;

namespace LabExtended.API.Toys
{
    public class LightToy : AdminToy, IWrapper<LightSourceToy>
    {
        public LightToy(LightSourceToy baseValue) : base(baseValue)
            => Base = baseValue;

        public new LightSourceToy Base { get; }

        public Color Color
        {
            get => Base.NetworkLightColor;
            set => Base.NetworkLightColor = value;
        }

        public float Intensity
        {
            get => Base.NetworkLightIntensity;
            set => Base.NetworkLightIntensity = value;
        }

        public float Range
        {
            get => Base.NetworkLightRange;
            set => Base.NetworkLightRange = value;
        }

        public bool Shadows
        {
            get => Base.NetworkLightShadows;
            set => Base.NetworkLightShadows = value;
        }

        public static LightToy Spawn(Vector3 position, Vector3? scale = null)
        {
            var lightSource = UnityEngine.Object.Instantiate(Prefabs.LightSourceToy).GetComponent<LightSourceToy>();

            lightSource.SpawnerFootprint = ExPlayer.Host.Footprint;

            lightSource.transform.position = position;
            lightSource.transform.localScale = scale.HasValue ? scale.Value : Vector3.one;

            var toy = new LightToy(lightSource);

            ExMap._toys.Add(toy);

            NetworkServer.Spawn(lightSource.gameObject);
            return toy;
        }
    }
}