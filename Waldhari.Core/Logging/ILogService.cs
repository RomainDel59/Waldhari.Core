using System;

namespace Waldhari.Core.Logging
{
    /// <summary>
    /// Provides a unified logging interface.
    /// Supports different log levels and handles exceptions appropriately.
    /// </summary>
    public interface ILogService
    {
        /// <summary>
        /// Gets or sets the minimum log level that will be written to the output.
        /// Messages below this level will be filtered out.
        /// </summary>
        /// <value>The current logging threshold level</value>
        LogLevel Level { get; set; }

        /// <summary>
        /// Logs a debug message for detailed troubleshooting information.
        /// Use for verbose information that helps during development and debugging.
        /// </summary>
        /// <param name="message">The debug message to log</param>
        void Debug(string message);

        /// <summary>
        /// Logs an informational message for general application flow tracking.
        /// Use for important events and milestones in normal operation.
        /// </summary>
        /// <param name="message">The informational message to log</param>
        void Info(string message);

        /// <summary>
        /// Logs a warning message for potentially problematic situations.
        /// Use when something unexpected happened but the application can continue.
        /// </summary>
        /// <param name="message">The warning message to log</param>
        void Warn(string message);

        /// <summary>
        /// Logs an error message for serious problems and exceptions.
        /// Use when something went wrong that affects application functionality.
        /// </summary>
        /// <param name="message">The error message to log</param>
        /// <param name="ex">Optional exception that caused the error</param>
        void Error(string message, Exception ex = null);
    }
}