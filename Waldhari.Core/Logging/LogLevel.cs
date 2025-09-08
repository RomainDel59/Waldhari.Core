namespace Waldhari.Core.Logging
{
    /// <summary>
    /// Defines the severity levels for log messages in the logging system.
    /// These levels follow standard logging conventions from most verbose to most critical.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Debug level: Detailed diagnostic information, typically only of interest when diagnosing problems.
        /// Used for fine-grained informational events that are most useful to debug an application.
        /// </summary>
        Debug,

        /// <summary>
        /// Information level: General informational messages that highlight the progress of the application.
        /// Used to track general workflow of the application and important events.
        /// </summary>
        Info,

        /// <summary>
        /// Warning level: Potentially harmful situations or unexpected conditions that don't prevent the application from functioning.
        /// Used when something unexpected happened, or indicative of some problem in the near future.
        /// </summary>
        Warn,

        /// <summary>
        /// Error level: Error events that might still allow the application to continue running.
        /// Used when an error occurred that should be logged and investigated, but doesn't necessarily stop the application.
        /// </summary>
        Error
    }
}