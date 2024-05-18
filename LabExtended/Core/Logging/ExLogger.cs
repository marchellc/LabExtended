using Common.Logging;

namespace LabExtended.Core.Logging
{
    public class ExLogger : ILogger
    {
        private LogMessage _last;

        public DateTime Started { get; } = DateTime.Now;

        public LogMessage Latest => _last;

        public void Emit(LogMessage message)
        {
            var source = message.Source.GetString();
            var msg = message.Message.GetString();

            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(msg))
                return;

            _last = message;

            if (source.StartsWith("["))
                source = source.Remove(0, 1);

            if (source.EndsWith("]"))
                source = source.Remove(source.Length - 1, 1);

            switch (message.Level)
            {
                case LogLevel.Error:
                case LogLevel.Fatal:
                    ExLoader.Error(source, msg);
                    break;

                case LogLevel.Trace:
                case LogLevel.Debug:
                case LogLevel.Verbose:
                    ExLoader.Debug(source, msg);
                    break;

                case LogLevel.Warning:
                    ExLoader.Warn(source, msg);
                    break;

                case LogLevel.Information:
                    ExLoader.Info(source, msg);
                    break;
            }
        }
    }
}