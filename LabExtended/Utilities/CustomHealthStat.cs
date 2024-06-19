using PlayerStatsSystem;

namespace LabExtended.Utilities
{
    public class CustomHealthStat : HealthStat
    {
        private float? _overridenMax;

        public override float MaxValue => _overridenMax.HasValue ? _overridenMax.Value : base.MaxValue;

        public float MaxHealth
        {
            get => MaxValue;
            set => _overridenMax = value;
        }
    }
}