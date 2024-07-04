namespace LabExtended.Hints.Elements.SelectMenu
{
    public class SelectMenuOption
    {
        private bool _isEnabled;

        public byte Position { get; internal set; }

        public string Description { get; }
        public string Label { get; }
        public string Id { get; }

        public object Value { get; }

        public virtual bool IsEnabled => _isEnabled;

        public SelectMenuOption(string label, string id, string description = null, object customValue = null)
        {
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentNullException(nameof(label));

            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));

            Label = label;
            Id = id;

            Description = description ?? "No description.";
            Value = customValue;
        }

        public virtual void TickOption() { }

        public virtual void OnEnabled() => _isEnabled = true;
        public virtual void OnDisabled() => _isEnabled = false;
    }
}