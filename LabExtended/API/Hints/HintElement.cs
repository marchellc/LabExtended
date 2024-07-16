using LabExtended.API.Enums;

namespace LabExtended.API.Hints
{
    public abstract class HintElement
    {
        internal readonly List<HintData> _messages = new List<HintData>();
        internal string _prev = null;

        public virtual bool IsActive { get; set; }
        public virtual bool IsRawDisplay { get; set; }

        public int Id { get; internal set; }
        public virtual string CustomId { get; set; }

        public ExPlayer Player { get; internal set; }

        public virtual float VerticalOffset { get; set; } = 0f;
        public virtual int MaxCharactersPerLine { get; set; } = 60;

        public virtual HintAlign Alignment { get; set; } = HintAlign.Center;

        public virtual void OnDisabled() { }
        public virtual void OnEnabled() { }

        public virtual void UpdateElement() { }

        public abstract string GetContent();

        internal bool CompareId(string customId)
            => !string.IsNullOrWhiteSpace(customId) && !string.IsNullOrWhiteSpace(CustomId) && customId == CustomId;
    }
}