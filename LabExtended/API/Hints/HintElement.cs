using LabExtended.API.Enums;
using LabExtended.API.Collections;
using LabExtended.API.Collections.Locked;

namespace LabExtended.API.Hints
{
    public abstract class HintElement
    {
        public const float DefaultVerticalOffset = 0f;
        public const int DefaultPixelLineSpacing = 3;

        internal long _tickNum = 0;
        internal string _prevCompiled = null;

        public int Id { get; internal set; }
        public string CustomId { get; internal set; }

        public long TickNumber => _tickNum;

        public PlayerCollection Whitelist { get; } = new PlayerCollection();
        public LockedHashSet<HintData> Data { get; } = new LockedHashSet<HintData>();

        public bool IsActive { get; internal set; }

        public virtual HintAlign Align { get; } = HintAlign.Center;

        public virtual bool IsGlobal { get; } = true;
        public virtual bool IsRaw { get; } = false;

        public virtual bool ShouldWrap { get; } = true;

        public virtual void TickElement() { }

        public virtual void OnEnabled() { }
        public virtual void OnDisabled() { }

        public virtual void OnWhitelistAdded(ExPlayer player) { }
        public virtual void OnWhitelistRemoved(ExPlayer player) { }

        public virtual bool ValidateWhitelist(ExPlayer player) => false;

        public virtual int GetPixelSpacing(ExPlayer player) => DefaultPixelLineSpacing;
        public virtual float GetVerticalOffset(ExPlayer player) => DefaultVerticalOffset;

        public abstract string BuildContent(ExPlayer player);

        internal bool CompareId(string customId)
            => !string.IsNullOrWhiteSpace(customId) && !string.IsNullOrWhiteSpace(CustomId) && customId == CustomId;

        public void SetCustomId(string customId) => CustomId = customId;
    }
}