using LabExtended.API.Enums;
using LabExtended.API.Input.Interfaces;

using UnityEngine;

namespace LabExtended.API.Internal
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