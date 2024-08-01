using LabExtended.Utilities.Values;

namespace LabExtended.Core.Performance
{
    public class PerformanceStat<T>
    {
        public long MaxThisRound { get; set; } = -1;
        public long MinThisRound { get; set; } = -1;

        public long MaxOverall { get; set; } = -1;
        public long MinOverall { get; set; } = -1;

        public int MinRoundPlayerCount { get; set; } = -1;
        public int MaxRoundPlayerCount { get; set; } = -1;

        public int MinOverallPlayerCount { get; set; } = -1;
        public int MaxOverallPlayerCount { get; set; } = -1;

        public DateTime MinOverallTime { get; set; } = DateTime.MinValue;
        public DateTime MaxOverallTime { get; set; } = DateTime.MinValue;

        public OptionalValue<T> MaxThisRoundData { get; set; } = OptionalValue<T>.FromNull();
        public OptionalValue<T> MinThisRoundData { get; set; } = OptionalValue<T>.FromNull();

        public OptionalValue<T> MaxOverallData { get; set; } = OptionalValue<T>.FromNull();
        public OptionalValue<T> MinOverallData { get; set; } = OptionalValue<T>.FromNull();

        public void Clear()
        {
            MaxThisRound = -1;
            MinThisRound = -1;

            MinRoundPlayerCount = -1;
            MaxRoundPlayerCount = -1;

            MinOverallPlayerCount = -1;
            MaxOverallPlayerCount = -1;

            MaxOverall = -1;
            MinOverall = -1;

            MinOverallTime = DateTime.MinValue;
            MaxOverallTime = DateTime.MinValue;

            MaxThisRoundData = OptionalValue<T>.FromNull();
            MinThisRoundData = OptionalValue<T>.FromNull();

            MaxOverallData = OptionalValue<T>.FromNull();
            MinOverallData = OptionalValue<T>.FromNull();
        }

        public void Copy(PerformanceStat<T> other)
        {
            MaxThisRound = other.MaxThisRound;
            MinThisRound = other.MinThisRound;

            MinRoundPlayerCount = other.MinRoundPlayerCount;
            MaxRoundPlayerCount = other.MaxRoundPlayerCount;

            MinOverallPlayerCount = other.MinOverallPlayerCount;
            MaxOverallPlayerCount = other.MaxOverallPlayerCount;

            MaxOverall = other.MaxOverall;
            MinOverall = other.MinOverall;

            MinOverallTime = other.MinOverallTime;
            MaxOverallTime = other.MaxOverallTime;

            MaxThisRoundData = other.MaxThisRoundData.HasValue ? OptionalValue<T>.FromValue(other.MaxThisRoundData.Value) : OptionalValue<T>.FromNull();
            MinThisRoundData = other.MinThisRoundData.HasValue ? OptionalValue<T>.FromValue(other.MinThisRoundData.Value) : OptionalValue<T>.FromNull();

            MaxOverallData = other.MaxOverallData.HasValue ? OptionalValue<T>.FromValue(other.MaxOverallData.Value) : OptionalValue<T>.FromNull();
            MinOverallData = other.MinOverallData.HasValue ? OptionalValue<T>.FromValue(other.MinOverallData.Value) : OptionalValue<T>.FromNull();
        }
    }
}