using LabExtended.API.Hints.Elements.SelectMenu;

namespace LabExtended.API.Hints.Elements.SelectMenu.Options
{
    public class TimedOption : SelectMenuOption
    {
        private TimeSpan _duration;
        private DateTime _enabledAt;

        private bool _enabled;

        public override bool IsEnabled => _enabled;

        public TimedOption(string label, string id, TimeSpan duration, string description = null, object customValue = null) : base(label, id, description, customValue)
        {
            if (duration <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(duration));

            _duration = duration;
            _enabledAt = DateTime.MinValue;
        }

        public override void OnEnabled()
        {
            base.OnEnabled();

            _enabledAt = DateTime.Now;
            _enabled = true;
        }

        public override void OnDisabled()
        {
            base.OnDisabled();

            _enabled = false;
            _enabledAt = DateTime.MinValue;
        }

        public override void TickOption()
        {
            base.TickOption();

            if (DateTime.Now - _enabledAt >= _duration)
            {
                _enabled = false;
                return;
            }
        }
    }
}