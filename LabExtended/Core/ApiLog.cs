using PluginAPI.Core;

using System.Diagnostics;

namespace LabExtended.Core
{
    public static class ApiLog
    {
        public static void Info(object msg) => Info(null, msg);
        public static void Warn(object msg) => Warn(null, msg);
        public static void Error(object msg) => Error(null, msg);
        public static void Debug(object msg) => Debug(null, msg);

        public static void Info(string source, object msg)
        {
            if (source is null || source.Length < 1 || source[0] == ' ')
                source = GetSourceType();

            Log.Info(msg.ToString(), source);
        }

        public static void Warn(string source, object msg)
        {
            if (source is null || source.Length < 1 || source[0] == ' ')
                source = GetSourceType();

            Log.Warning(msg.ToString(), source);
        }

        public static void Error(string source, object msg)
        {
            if (source is null || source.Length < 1 || source[0] == ' ')
                source = GetSourceType();

            Log.Error(msg.ToString(), source);
        }

        public static void Debug(string source, object msg)
        { 
            if (ApiLoader.BaseConfig != null && !ApiLoader.BaseConfig.DebugEnabled)
                return;

            if (source is null || source.Length < 1 || source[0] == ' ')
                source = GetSourceType();

            if (!CheckDebug(source))
                return;

            Log.Debug(msg.ToString(), source);
        }

        public static bool CheckDebug(string sourceName)
        {
            if (ApiLoader.BaseConfig is null)
                return true;

            return !string.IsNullOrWhiteSpace(sourceName) && !ApiLoader.BaseConfig.DisabledDebugSources.Contains(sourceName);
        }

        private static string GetSourceType()
        {
            var trace = new StackTrace();
            var frames = trace.GetFrames();

            foreach (var frame in frames)
            {
                var method = frame.GetMethod();

                if (method is null)
                    continue;

                if (method.DeclaringType is null)
                    continue;

                if (method.DeclaringType == typeof(ApiLog))
                    continue;

                return method.DeclaringType.Name;
            }

            return "Unknown";
        }
    }
}