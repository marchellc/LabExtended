using LabExtended.API;
using LabExtended.Core.Events;

using UnityEngine;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Gets called when a player executes the .input command.
    /// </summary>
    public class PlayerKeybindReceivedArgs : HookBooleanCancellableEventBase
    {
        /// <summary>
        /// The player sending the keybind.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// The key that was sent.
        /// </summary>
        public KeyCode Key { get; }

        internal PlayerKeybindReceivedArgs(ExPlayer player, KeyCode key)
        {
            Player = player;
            Key = key;
        }
    }
}