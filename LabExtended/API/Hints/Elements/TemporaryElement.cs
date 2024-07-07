using LabExtended.API.Messages;

namespace LabExtended.API.Hints.Elements
{
    internal class TemporaryElement : HintElement
    {
        internal DateTime _showUntil;

        public override bool ClearWriter => false;

        public override float VerticalOffset { get; set; } = -5f;
        public override HintAlign Alignment { get; set; } = HintAlign.Center;

        internal void SetHint(HintMessage hintMessage)
        {
            if (string.IsNullOrWhiteSpace(hintMessage.Content) || hintMessage.Duration <= 0)
                return;

            _showUntil = DateTime.Now.AddSeconds(hintMessage.Duration);

            Writer.Write(hintMessage.Content);
            IsActive = true;
        }

        internal void Reset()
        {
            IsActive = false;
            Writer.Clear();
            _showUntil = DateTime.MinValue;
        }

        internal void CheckDuration()
        {
            if (Writer.Size < 1)
                return;

            if (DateTime.Now >= _showUntil)
                Reset();
        }
    }
}
