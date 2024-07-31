using UnityEngine;

namespace LabExtended.Core.Ticking
{
    public class TickComponent : MonoBehaviour
    {
        private Action _action;
        private Func<bool> _validator;

        public bool IsPaused { get; set; } = true;

        void Update()
        {
            if (IsPaused)
                return;

            if (_validator != null && !_validator())
                return;

            _action();
        }

        public void Stop()
        {
            IsPaused = true;

            _action = null;
            _validator = null;

            Destroy(this);
        }

        public static void Create(string id, Action target, Func<bool> validator, out GameObject parent, out TickComponent component)
        {
            parent = new GameObject(id);
            component = parent.AddComponent<TickComponent>();

            DontDestroyOnLoad(parent);
            DontDestroyOnLoad(component);

            component._validator = validator;
            component._action = target;

            component.IsPaused = false;
            component.enabled = true;
        }
    }
}