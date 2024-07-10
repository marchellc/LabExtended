namespace LabExtended.API.Hints.Elements
{
    public abstract class BufferedElement : HintElement
    {
        private string _buffer;

        public virtual bool ClearBuffer { get; set; } = true;

        public abstract void WriteContent();

        public void Append(string content)
        {
            if (_buffer is null)
                _buffer = "";

            _buffer += content;
        }

        public void AppendLine(string content)
        {
            if (_buffer is null)
                _buffer = "";

            _buffer += $"{content}\n";
        }

        public void Clear()
            => _buffer = "";

        public override string GetContent()
        {
            if (ClearBuffer || _buffer is null)
                _buffer = "";

            WriteContent();
            return _buffer;
        }
    }
}