namespace LabExtended.Utilities
{
    public static class MathEx
    {
        public static long TicksToMilliseconds(long ticks)
            => ticks / TimeSpan.TicksPerMillisecond;
    }
}