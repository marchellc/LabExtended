using LabExtended.API.Interfaces;
using LabExtended.API.Prefabs;

using LabExtended.Core.Networking;

using LabExtended.Events;
using LabExtended.Extensions;
using LabExtended.Utilities.Generation;

using Mirror;

using UnityEngine;

using VoiceChat.Playbacks;

namespace LabExtended.API.Toys
{
    public class SpeakerToy : AdminToy, IWrapper<AdminToys.SpeakerToy>
    {
        static SpeakerToy()
        {
            RoundEvents.OnWaitingForPlayers += OnWaiting;
            MirrorEvents.OnDestroy += OnDestroyed;
        }

        public static UniqueInt32Generator Id { get; } = new UniqueInt32Generator(byte.MinValue, byte.MaxValue);

        private bool wasAssignedId;
        
        public SpeakerToy(AdminToys.SpeakerToy baseValue) : base(baseValue)
            => Base = baseValue;

        public new AdminToys.SpeakerToy Base { get; }

        public SpeakerToyPlaybackBase Playback => Base.Playback;

        public bool IsSpatial
        {
            get => Base.NetworkIsSpatial;
            set => Base.NetworkIsSpatial = value;
        }

        public byte ControllerId
        {
            get => Base.NetworkControllerId;
            set => Base.NetworkControllerId = Base.Playback!.ControllerId = value;
        }

        public float MaxDistance
        {
            get => Base.NetworkMaxDistance;
            set => Base.NetworkMaxDistance = value;
        }

        public float MinDistance
        {
            get => Base.NetworkMinDistance;
            set => Base.NetworkMinDistance = value;
        }

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

        public static SpeakerToy Spawn(Vector3 position, byte? customId, Action<SpeakerToy> setup = null)
        {
            if (Id.Cache.Count + 1 >= byte.MaxValue)
                throw new Exception("No more speakers can be spawned.");

            var toy = PrefabList.Speaker.CreateInstance().GetComponent<AdminToys.SpeakerToy>();
            var wrapper = new SpeakerToy(toy);

            NetworkServer.Spawn(toy.gameObject);
            
            toy.SpawnerFootprint = ExPlayer.Host.Footprint;
            toy.Playback.ControllerId = toy.ControllerId = customId.HasValue ? customId.Value : (byte)Id.Next();

            wrapper.wasAssignedId = !customId.HasValue;
            
            wrapper.Position = position;
            wrapper.Scale = Vector3.zero;

            ExMap.Toys.Add(wrapper);

            setup.InvokeSafe(wrapper);
            return wrapper;
        }

        private static void OnWaiting()
            => Id.FreeAll();

        private static void OnDestroyed(NetworkIdentity identity)
        {
            if (!ExRound.IsRunning)
                return;

            if (identity is null || !identity.gameObject.TryFindComponent<AdminToys.SpeakerToy>(out var speakerToy))
                return;

            var wrapperToy = ExMap.GetToy<SpeakerToy>(speakerToy);

            if (wrapperToy is null || !wrapperToy.wasAssignedId)
                return;
            
            Id.Free(speakerToy.ControllerId);
        }
    }
}
