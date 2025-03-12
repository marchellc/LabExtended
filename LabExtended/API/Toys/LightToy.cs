using AdminToys;

using LabExtended.API.Interfaces;
using LabExtended.API.Prefabs;

using Mirror;

using UnityEngine;

namespace LabExtended.API.Toys
{
    /// <summary>
    /// Represents a spawnable light source.
    /// </summary>
    public class LightToy : AdminToy, IWrapper<LightSourceToy>
    {
        /// <summary>
        /// Spawns a new light source toy.
        /// </summary>
        public LightToy() : base(PrefabList.LightSource.CreateInstance().GetComponent<AdminToyBase>())
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            Base = base.Base as LightSourceToy;

            if (Base is null)
                throw new Exception($"Could not spawn light source.");

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            Base.SpawnerFootprint = ExPlayer.Host.Footprint;

            NetworkServer.Spawn(Base.gameObject);
        }
        
        internal LightToy(LightSourceToy baseValue) : base(baseValue)
            => Base = baseValue;

        /// <summary>
        /// Gets the light toy.
        /// </summary>
        public new LightSourceToy Base { get; }

        /// <summary>
        /// Gets or sets the light's intensity.
        /// </summary>
        public float Intensity
        {
            get => Base.LightIntensity;
            set => Base.NetworkLightIntensity = value;
        }

        /// <summary>
        /// Gets or set's the light's range.
        /// </summary>
        public float Range
        {
            get => Base.LightRange;
            set => Base.NetworkLightRange = value;
        }

        /// <summary>
        /// Gets or sets the light's spot angle.
        /// </summary>
        public float SpotAngle
        {
            get => Base.SpotAngle;
            set => Base.NetworkSpotAngle = value;
        }

        /// <summary>
        /// Gets or sets the light's inner spot angle.
        /// </summary>
        public float InnerSpotAngle
        {
            get => Base.InnerSpotAngle;
            set => Base.NetworkInnerSpotAngle = value;
        }

        /// <summary>
        /// Gets or sets the light's shadow strength.
        /// </summary>
        public float ShadowStrength
        {
            get => Base.ShadowStrength;
            set => Base.NetworkShadowStrength = value;
        }

        /// <summary>
        /// Gets or sets the light's shadow type.
        /// </summary>
        public LightShadows ShadowType
        {
            get => Base.ShadowType;
            set => Base.NetworkShadowType = value;
        }

        /// <summary>
        /// Gets or sets the light's shape.
        /// </summary>
        public LightShape Shape
        {
            get => Base.LightShape;
            set => Base.NetworkLightShape = value;
        }

        /// <summary>
        /// Gets or sets the light's color.
        /// </summary>
        public Color Color
        {
            get => Base.LightColor;
            set => Base.NetworkLightColor = value;
        }
    }
}