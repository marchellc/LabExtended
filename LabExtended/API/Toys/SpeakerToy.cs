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

        public SpeakerToy(AdminToys.SpeakerToy baseValue) : base(baseValue)
            => Base = baseValue;

        public new AdminToys.SpeakerToy Base { get; }

        public SpeakerToyPlaybackBase Playback => Base.Playback;

        public bool IsSpatial
        {
            get => Base.IsSpatial;
            set => Base.NetworkIsSpatial = value;
        }

        public byte ControllerId
        {
            get => Base.ControllerId;
            set => Base.NetworkControllerId = value;
        }

        public float MaxDistance
        {
            get => Base.MaxDistance;
            set => Base.NetworkMaxDistance = value;
        }

        public float MinDistance
        {
            get => Base.MinDistance;
            set => Base.NetworkMinDistance = value;
        }

        public float Volume
        {
            get => Base.Volume;
            set
            {
                if (value > 1f)
                    value = value / 100f;

                Base.NetworkVolume = value;
            }
        }

        public static SpeakerToy Spawn(Vector3 position, Action<SpeakerToy> setup = null)
        {
            if (Id.Cache.Count + 1 >= byte.MaxValue)
                throw new Exception("No more speakers can be spawned.");

            var toy = PrefabList.Speaker.Instance.GetComponent<AdminToys.SpeakerToy>();
            var wrapper = new SpeakerToy(toy);

            toy.SpawnerFootprint = ExPlayer.Host.Footprint;
            toy.Playback.ControllerId = toy.ControllerId = (byte)Id.Next();

            toy.transform.position = position;
            toy.transform.localScale = Vector3.zero;

            ExMap._toys.Add(wrapper);

            setup.InvokeSafe(wrapper);

            NetworkServer.Spawn(toy.gameObject);
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

            Id.Free(speakerToy.ControllerId);
        }
    }
}
