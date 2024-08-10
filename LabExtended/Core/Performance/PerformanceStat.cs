using LabExtended.API;

namespace LabExtended.Core.Performance
{
    public class PerformanceStat
    {
        public PerformanceStat(string name, string id, bool isDecreaseGood, Func<long, string> asString)
        {
            IsDecreaseGood = isDecreaseGood;
            AsString = asString;
            Name = name;
            Id = id;
        }

        public string Name { get; }
        public string Id { get; }

        public bool IsDecreaseGood { get; }

        public Func<long, string> AsString { get; }

        public PerformanceValue RoundValue { get; } = new PerformanceValue();
        public PerformanceValue OverallValue { get; } = new PerformanceValue();

        public void Update(long curValue)
        {
            var strValue = AsString(curValue);

            RoundValue.LastValue = curValue;
            RoundValue.LastValueStr = strValue;

            OverallValue.LastValue = curValue;
            OverallValue.LastValueStr = strValue;

            if (RoundValue.MinValue == -1 || curValue < RoundValue.MinValue)
            {
                RoundValue.MinValue = curValue;
                RoundValue.MinValueStr = strValue;

                RoundValue.TimeMin = DateTime.Now;
                RoundValue.PlayerCountMin = ExPlayer.Count;
            }

            if (RoundValue.MaxValue == -1 || curValue > RoundValue.MaxValue)
            {
                RoundValue.MaxValue = curValue;
                RoundValue.MaxValueStr = strValue;

                RoundValue.TimeMax = DateTime.Now;
                RoundValue.PlayerCountMax = ExPlayer.Count;
            }

            if (OverallValue.MinValue == -1 || curValue < OverallValue.MinValue)
            {
                if (OverallValue.MinValue != -1 && OverallValue.MinValueStr != strValue && ApiLoader.Config.ApiOptions.PerformanceOptions.EnablePerformanceWatcher && !ApiLoader.Config.ApiOptions.PerformanceOptions.NoLogPerformance.Contains(Id))
                {
                    if (IsDecreaseGood)
                        ApiLoader.Info("Performance Statistics", $"Overall value of &4{Name}&r &2decreased&r to &6{strValue}&r");
                    else
                        ApiLoader.Warn("Performance Statistics", $"Overall value of &4{Name}&r &1decreased&r to &6{strValue}&r");
                }

                OverallValue.MinValue = curValue;
                OverallValue.MinValueStr = strValue;

                OverallValue.TimeMin = DateTime.Now;
                OverallValue.PlayerCountMin = ExPlayer.Count;
            }

            if (OverallValue.MaxValue == -1 || curValue > OverallValue.MaxValue)
            {
                if (OverallValue.MaxValue != -1 && OverallValue.MaxValueStr != strValue && ApiLoader.Config.ApiOptions.PerformanceOptions.EnablePerformanceWatcher && !ApiLoader.Config.ApiOptions.PerformanceOptions.NoLogPerformance.Contains(Id))
                {
                    if (IsDecreaseGood)
                        ApiLoader.Warn("Performance Statistics", $"Overall value of &4{Name}&r &1increased&r to &6{strValue}&r");
                    else
                        ApiLoader.Info("Performance Statistics", $"Overall value of &4{Name}&r &2increased&r to &6{strValue}&r");
                }

                OverallValue.MaxValue = curValue;
                OverallValue.MaxValueStr = strValue;

                OverallValue.TimeMax = DateTime.Now;
                OverallValue.PlayerCountMax = ExPlayer.Count;
            }
        }

        public void Clear()
        {
            RoundValue.Clear();
            OverallValue.Clear();
        }
    }
}