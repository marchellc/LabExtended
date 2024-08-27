using LabExtended.Extensions;

using UnityEngine;

namespace LabExtended.Core.Ticking.Distributors.Unity
{
    public class UnityTickComponent : MonoBehaviour
    {
        private static int _idClock = 0;

        public event Action OnUpdate;
        public event Action OnLateUpdate;
        public event Action OnFixedUpdate;

        public float TickRate;

        void Update()
        {
            OnUpdate.InvokeSafe();
            TickRate = 1f / Time.deltaTime;
        }

        void LateUpdate()
            => OnLateUpdate.InvokeSafe();

        void FixedUpdate()
            => OnFixedUpdate.InvokeSafe();

        void OnDestroy()
        {
            OnUpdate = null;
            OnLateUpdate = null;
            OnFixedUpdate = null;
        }

        public void Destroy()
            => Destroy(this);

        public static UnityTickComponent CreateNew()
        {
            var obj = new GameObject($"Tick_{_idClock++}");
            var comp = obj.AddComponent<UnityTickComponent>();

            DontDestroyOnLoad(comp);
            DontDestroyOnLoad(obj);

            return comp;
        }
    }
}