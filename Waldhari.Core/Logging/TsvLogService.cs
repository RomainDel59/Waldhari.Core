using System;
using System.IO;

namespace Waldhari.Core.Logging
{
    /// <summary>
    /// A file-based logging service that writes log entries to a TSV (Tab-Separated Values) file.
    /// Each log entry contains timestamp, level, message, and optional exception information.
    /// The service is thread-safe and automatically creates the necessary directory structure.
    /// </summary>
    public class TsvLogService : ILogService
    {
        private readonly string _logFilePath;
        private readonly object _lock = new object();

        /// <summary>
        /// Gets or sets the minimum log level that will be written to the file.
        /// Messages below this level will be ignored to reduce log file size.
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        /// Initializes a new TSV log service with automatic file path generation.
        /// Creates a "Waldhari" subdirectory in the GTAV scripts folder for log storage.
        /// </summary>
        /// <param name="modName">Name of the module/component for the log file name</param>
        /// <param name="level">Initial minimum log level to capture (default: Debug)</param>
        public TsvLogService(string modName, LogLevel level = LogLevel.Debug)
        {
            Level = level;

            // Create dedicated logs directory in the GTAV scripts folder to keep it organized
            var logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Waldhari");

            if (!Directory.Exists(logsDir))
                Directory.CreateDirectory(logsDir);

            _logFilePath = Path.Combine(logsDir, $"{modName}.log");
        }

        /// <summary>
        /// Core logging method that formats and writes log entries to the TSV file.
        /// Respects the current log level threshold and ensures thread-safe file access.
        /// </summary>
        /// <param name="logLevel">The severity level of this log entry</param>
        /// <param name="message">The main log message</param>
        /// <param name="ex">Optional exception to include in the log entry</param>
        private void Write(LogLevel logLevel, string message, Exception ex = null)
        {
            // Filter out messages below the configured threshold to control log verbosity
            if (logLevel < Level)
                return;

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string cleanMessage = CleanString(message);
            string cleanException = ex != null ? CleanString(ex.ToString()) : string.Empty;

            // Format as TSV: timestamp, level, message, exception (each field properly escaped)
            string line = $"{timestamp}\t{logLevel}\t{cleanMessage}\t{cleanException}";

            // Ensure thread-safe file writing to prevent corruption in multi-threaded scenarios
            lock (_lock)
            {
                File.AppendAllText(_logFilePath, line + Environment.NewLine);
            }
        }

        /// <summary>
        /// Sanitizes strings for TSV format by escaping special characters.
        /// Handles tab characters and double quotes to maintain proper TSV structure.
        /// </summary>
        /// <param name="input">The raw string to clean</param>
        /// <returns>A properly escaped string safe for TSV format</returns>
        private string CleanString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "\"\"";

            // Replace tabs with spaces since we use tabs as field separators
            string clean = input.Replace("\t", " ");

            // Escape double quotes using CSV/TSV standard (double the quotes)
            clean = clean.Replace("\"", "\"\"");

            // Wrap in quotes to handle any remaining special characters
            return $"\"{clean}\"";
        }

        /// <summary>
        /// Logs a debug message for detailed troubleshooting information.
        /// </summary>
        /// <param name="message">The debug message to log</param>
        public void Debug(string message) => Write(LogLevel.Debug, message);

        /// <summary>
        /// Logs an informational message for general application flow tracking.
        /// </summary>
        /// <param name="message">The informational message to log</param>
        public void Info(string message) => Write(LogLevel.Info, message);

        /// <summary>
        /// Logs a warning message for potentially problematic situations.
        /// </summary>
        /// <param name="message">The warning message to log</param>
        public void Warn(string message) => Write(LogLevel.Warn, message);

        /// <summary>
        /// Logs an error message for serious problems and exceptions.
        /// </summary>
        /// <param name="message">The error message to log</param>
        /// <param name="ex">Optional exception that caused the error</param>
        public void Error(string message, Exception ex = null) => Write(LogLevel.Error, message, ex);
    }
}