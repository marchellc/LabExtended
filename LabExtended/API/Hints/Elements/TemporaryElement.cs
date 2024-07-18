using LabExtended.API.Enums;
using LabExtended.API.Messages;

namespace LabExtended.API.Hints.Elements
{
    internal class TemporaryElement : HintElement
    {
        internal DateTime _showUntil;
        internal string _content;

        public override float VerticalOffset { get; set; } = -5f;
        public override HintAlign Alignment { get; set; } = HintAlign.Center;

        public override string GetContent(ExPlayer player)
            => _content;

        internal void SetHint(HintMessage hintMessage)
        {
            if (string.IsNullOrWhiteSpace(hintMessage.Content) || hintMessage.Duration <= 0)
                return;

            _showUntil = DateTime.Now.AddSeconds(hintMessage.Duration);
            _content = hintMessage.Content;

            IsActive = true;
        }

        internal void Reset()
        {
            IsActive = false;

            _showUntil = DateTime.MinValue;
            _content = null;
        }

        internal void CheckDuration()
        {
            if (_content is null || _content.Length < 1)
                return;

            if (DateTime.Now >= _showUntil)
                Reset();
        }
    }
}
