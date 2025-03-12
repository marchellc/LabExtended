using AdminToys;

using LabExtended.API.Interfaces;
using LabExtended.API.Prefabs;

using Mirror;

using VoiceChat.Playbacks;

namespace LabExtended.API.Toys
{
    /// <summary>
    /// Represents a speaker toy.
    /// </summary>
    public class SpeakerToy : AdminToy, IWrapper<AdminToys.SpeakerToy>
    {
        /// <summary>
        /// Spawns a new speaker toy.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public SpeakerToy() : base(PrefabList.Speaker.CreateInstance().GetComponent<AdminToyBase>())
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            Base = base.Base as AdminToys.SpeakerToy;
#pragma warning restore CS8601 // Possible null reference assignment.

            if (Base is null)
                throw new Exception($"Failed to spawn SpeakerToy.");
            
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            Base.SpawnerFootprint = ExPlayer.Host.Footprint;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            
            NetworkServer.Spawn(Base.gameObject);
        }
        
        internal SpeakerToy(AdminToys.SpeakerToy baseValue) : base(baseValue)
            => Base = baseValue;

        /// <summary>
        /// Speaker toy base.
        /// </summary>
        public new AdminToys.SpeakerToy Base { get; }

        /// <summary>
        /// Gets the speaker's playback.
        /// </summary>
        public SpeakerToyPlaybackBase Playback => Base.Playback;

        /// <summary>
        /// Whether or not to use spatial 3D audio.
        /// </summary>
        public bool IsSpatial
        {
            get => Base.IsSpatial;
            set => Base.NetworkIsSpatial = value;
        }

        /// <summary>
        /// Gets or sets the speaker's controller ID.
        /// </summary>
        public byte ControllerId
        {
            get => Base.ControllerId;
            set => Base.NetworkControllerId = Base.Playback!.ControllerId = value;
        }

        /// <summary>
        /// Gets or sets the maximum distance from the speaker.
        /// </summary>
        public float MaxDistance
        {
            get => Base.NetworkMaxDistance;
            set => Base.NetworkMaxDistance = value;
        }

        /// <summary>
        /// Gets or sets the minimum distance from the speaker.
        /// </summary>
        public float MinDistance
        {
            get => Base.NetworkMinDistance;
            set => Base.NetworkMinDistance = value;
        }

        /// <summary>
        /// Gets or sets the speaker's volume (values between 0 - 1).
        /// </summary>
        public float Volume
        {
            get => Base.NetworkVolume;
            set
            {
                if (value > 1f)
                    value = value / 100f;

                Base.NetworkVolume = value;
            }
        }
    }
}
