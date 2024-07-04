using LabExtended.API;
using LabExtended.Core.Events;

using UnityEngine;

namespace LabExtended.Events.Player
{
    public class PlayerKeybindReceivedArgs : HookBooleanCancellableEventBase
    {
        public ExPlayer Player { get; }

        public KeyCode Key { get; }

        internal PlayerKeybindReceivedArgs(ExPlayer player, KeyCode key)
        {
            Player = player;
            Key = key;
        }
    }
}