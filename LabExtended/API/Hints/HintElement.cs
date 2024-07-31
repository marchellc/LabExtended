using LabExtended.API.Collections.Locked;
using LabExtended.API.Enums;

namespace LabExtended.API.Hints
{
    public abstract class HintElement
    {
        internal readonly LockedHashSet<HintData> _messages = new LockedHashSet<HintData>();
        internal string _prev = null;

        public virtual bool IsActive { get; set; }
        public virtual bool IsRawDisplay { get; set; }

        public int Id { get; internal set; }
        public virtual string CustomId { get; set; }

        public virtual float VerticalOffset { get; set; } = 0f;

        /// <summary>
        /// For <see langword="value"/> == <see langword="true"/>, the Automatic Algorithm chooses where to wrap the Line.<br/>
        /// For <see langword="value"/> == <see langword="false"/>, no Newlines are inserted.
        /// </summary>
        public virtual bool AutoLineWrap { get; set; } = true;

        public virtual HintAlign Alignment { get; set; } = HintAlign.Center;

        public virtual void OnDisabled() { }
        public virtual void OnEnabled() { }

        public virtual void UpdateElement() { }

        public abstract string GetContent(ExPlayer player);

        internal bool CompareId(string customId)
            => !string.IsNullOrWhiteSpace(customId) && !string.IsNullOrWhiteSpace(CustomId) && customId == CustomId;
    }
}