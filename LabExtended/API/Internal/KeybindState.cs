using LabExtended.API.Collections.Locked;
using UnityEngine;

namespace LabExtended.API.Internal
{
    public class KeybindState
    {
        public ExPlayer Player { get; set; }

        public LockedHashSet<KeyCode> SyncedBinds { get; } = new LockedHashSet<KeyCode>();
    }
}