namespace LabExtended.API.Hints
{
    public abstract class HintElement
    {
        internal readonly SortedSet<HintData> _messages = new SortedSet<HintData>(new HintSorter());
        internal string _prev = null;

        public virtual bool IsActive { get; set; }
        public virtual bool IsRawDisplay { get; set; }

        public int Id { get; internal set; }

        public ExPlayer Player { get; internal set; }

        public virtual float VerticalOffset { get; set; } = 0f;
        public virtual int MaxCharactersPerLine { get; set; } = 60;
        public virtual bool SkipPreviousLine { get; set; }

        public virtual HintAlign Alignment { get; set; } = HintAlign.Center;

        public virtual void OnDisabled() { }
        public virtual void OnEnabled() { }

        public virtual void UpdateElement() { }

        public abstract string GetContent();
    }
}