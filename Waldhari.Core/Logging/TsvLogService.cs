using System;
using System.IO;

namespace Waldhari.Core.Logging
{
    public class TsvLogService : ILogService
    {
        private readonly string _logFilePath;
        private readonly object _lock = new object();

        public LogLevel Level { get; set; }

        public TsvLogService(string modName = "Waldhari.Core", LogLevel level = LogLevel.Debug)
        {
            Level = level;

            var logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Waldhari");

            if (!Directory.Exists(logsDir))
                Directory.CreateDirectory(logsDir);

            _logFilePath = Path.Combine(logsDir, $"{modName}.log");
        }

        private void Write(LogLevel logLevel, string message, Exception ex = null)
        {
            if (logLevel < Level)
                return;

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string cleanMessage = CleanString(message);
            string cleanException = ex != null ? CleanString(ex.ToString()) : string.Empty;

            string line = $"{timestamp}\t{logLevel}\t{cleanMessage}\t{cleanException}";

            lock (_lock)
            {
                File.AppendAllText(_logFilePath, line + Environment.NewLine);
            }
        }

        private string CleanString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "\"\"";

            string clean = input.Replace("\t", " ");

            clean = clean.Replace("\"", "\"\"");

            return $"\"{clean}\"";
        }

        public void Debug(string message) => Write(LogLevel.Debug, message);
        public void Info(string message) => Write(LogLevel.Info, message);
        public void Warn(string message) => Write(LogLevel.Warn, message);
        public void Error(string message, Exception ex = null) => Write(LogLevel.Error, message, ex);
    }
}
