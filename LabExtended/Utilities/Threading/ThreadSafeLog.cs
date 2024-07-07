using Common.Extensions;

using LabExtended.Core;

namespace LabExtended.Utilities.Threading
{
    public static class ThreadSafeLog
    {
        public static void Info(string tag, string message)
            => UnityThread.Thread.Run(() => ExLoader.Info(tag, message), null);

        public static void Warn(string tag, string message)
            => UnityThread.Thread.Run(() => ExLoader.Warn(tag, message), null);

        public static void Error(string tag, string message)
            => UnityThread.Thread.Run(() => ExLoader.Error(tag, message), null);

        public static void Debug(string tag, string message)
            => UnityThread.Thread.Run(() => ExLoader.Debug(tag, message), null);
    }
}