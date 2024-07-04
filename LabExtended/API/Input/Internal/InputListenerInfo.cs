using LabExtended.API.Input.Enums;
using LabExtended.API.Input.Interfaces;

using UnityEngine;

namespace LabExtended.API.Input.Internal
{
    public struct InputListenerInfo
    {
        public IInputListener Listener { get; }

        public InputType Type { get; }
        public KeyCode Key { get; }

        public InputListenerInfo(IInputListener listener, InputType type, KeyCode key)
        {
            Listener = listener;
            Type = type;
            Key = key;
        }
    }
}
