using LabExtended.API.Enums;
using LabExtended.API.Input.Interfaces;

using UnityEngine;

namespace LabExtended.API.Input.Inputs
{
    public struct KeybindInputInfo : IInputInfo
    {
        public InputType Type => InputType.Keybind;

        public ExPlayer Player { get; }

        public KeyCode Key { get; }

        internal KeybindInputInfo(ExPlayer player, KeyCode key)
        {
            Player = player;
            Key = key;
        }
    }
}
