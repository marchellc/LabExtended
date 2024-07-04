using Common.IO.Collections;

using UnityEngine;

namespace LabExtended.API.Input.Internal
{
    public class KeybindState
    {
        public ExPlayer Player { get; set; }
        public LockedList<KeyCode> SyncedBinds { get; } = new LockedList<KeyCode>();
    }
}