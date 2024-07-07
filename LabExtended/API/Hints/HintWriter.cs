using LabExtended.Core;

namespace LabExtended.API.Hints
{
    public class HintWriter
    {
        internal readonly SortedSet<HintData> _messages = new SortedSet<HintData>(new HintSorter());

        internal float _vOffset;
        private string _prevLine = null;

        internal HintWriter(float vOffset)
        {
            _vOffset = vOffset;
        }

        public int Size => _messages.Count;

        public void Write(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                throw new ArgumentNullException(nameof(line));

            if (_prevLine != null && line == _prevLine && _messages.Count > 0)
                return;

            _prevLine = line;
            _messages.Clear();

            if (HintModule.ShowDebug)
                ExLoader.Debug("Hint API - Write()", $"[START] Writing line: {line}");

            line = line.Replace("\r\n", "\n").Replace("\\n", "\n").Replace("<br>", "\n").TrimEnd();

            if (HintModule.ShowDebug)
                ExLoader.Debug("Hint API - Write()", $"[AFTER REPLACE] {line}");

            HintUtils.TrimStartNewLines(ref line, out var count);

            if (_vOffset == 0f)
                _vOffset = -count;

            if (HintModule.ShowDebug)
                ExLoader.Debug("Hint API - Write()", $"[AFTER OFFSET] Offset: {_vOffset}");

            HintUtils.GetMessages(_vOffset, line, _messages);
        }

        public void Clear()
        {
            _messages.Clear();
            _prevLine = null;
        }
    }
}