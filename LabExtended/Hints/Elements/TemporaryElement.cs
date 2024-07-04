using LabExtended.API.Messages;
using LabExtended.Utilities;

using System.Text;

namespace LabExtended.Hints.Elements
{
    internal class TemporaryElement : HintElement
    {
        internal struct TemporaryMessage
        {
            internal readonly int PixelSize;
            internal readonly string Content;

            internal TemporaryMessage(int pixelSize, string content)
            {
                PixelSize = pixelSize;
                Content = content;
            }
        }

        internal readonly List<TemporaryMessage> _messages = new List<TemporaryMessage>();
        internal DateTime _showUntil;

        public override void OnEnabled()
        {
            base.OnEnabled();

            Alignment = HintAlign.Center;
            VerticalOffset = 0.5f;
        }

        internal void SetHint(HintMessage hintMessage)
        {
            if (string.IsNullOrWhiteSpace(hintMessage.Content) || hintMessage.Duration <= 0)
                return;

            _messages.Clear();
            _showUntil = DateTime.Now.AddSeconds(hintMessage.Duration);

            HintUtils.GetMessages(hintMessage.Content, _messages);

            IsActive = true;
        }

        internal void Reset()
        {
            _messages.Clear();
            _showUntil = DateTime.MinValue;

            IsActive = false;
        }

        internal void CheckDuration()
        {
            if (_messages.Count < 1)
                return;

            if (DateTime.Now >= _showUntil)
                Reset();
        }

        public override void Write()
        {
            if (_messages.Count < 1)
                return;

            foreach (var message in _messages)
                Builder.Append(message.Content);
        }
    }
}
