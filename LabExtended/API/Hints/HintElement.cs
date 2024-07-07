namespace LabExtended.API.Hints
{
    public class HintElement
    {
        internal HintWriter _writer;

        internal float _prevOffset;
        internal HintAlign _prevAlign;

        public bool IsActive { get; internal set; }
        public int Id { get; internal set; }

        public ExPlayer Player { get; internal set; }

        public virtual HintWriter Writer => _writer;

        public virtual bool ClearWriter { get; }

        public virtual float VerticalOffset { get; set; } = 0f;
        public virtual HintAlign Alignment { get; set; } = HintAlign.Center;

        public virtual void OnDisabled() { }
        public virtual void OnEnabled() { }

        public virtual void Write() { }
    }
}