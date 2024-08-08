namespace LabExtended.Core.Performance
{
    public class PerformanceValue
    {
        public long LastValue { get; set; } = -1;

        public long MinValue { get; set; } = -1;
        public long MaxValue { get; set; } = -1;

        public string LastValueStr { get; set; } = string.Empty;

        public string MinValueStr { get; set; } = string.Empty;
        public string MaxValueStr { get; set; } = string.Empty;

        public long PlayerCountMin { get; set; } = -1;
        public long PlayerCountMax { get; set; } = -1;

        public DateTime TimeMin { get; set; } = DateTime.MinValue;
        public DateTime TimeMax { get; set; } = DateTime.MaxValue;

        public void Clear()
        {
            LastValue = -1;

            MinValue = -1;
            MaxValue = -1;

            PlayerCountMin = -1;
            PlayerCountMax = -1;

            LastValueStr = string.Empty;

            MinValueStr = string.Empty;
            MaxValueStr = string.Empty;

            TimeMax = DateTime.MinValue;
            TimeMin = DateTime.MinValue;
        }
    }
}