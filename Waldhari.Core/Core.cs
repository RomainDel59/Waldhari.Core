using System;

namespace Waldhari.Core
{
    using Logging;

    /// <summary>
    /// Central static class providing global access to logging services for the Waldhari.Core library.
    /// This class allows mods using the core library to configure their own logger so that
    /// core library logs are written to the mod's log file instead of a separate core log.
    /// </summary>
    public static class Core
    {
        /// <summary>
        /// Private instance of the logging service used by the core library.
        /// </summary>
        private static ILogService _logger;

        /// <summary>
        /// Gets the current logging service instance.
        /// If no custom logger has been set, defaults to a TsvLogService with "Core" as the mod name.
        /// Mods should call SetLogger() to redirect core logs to their own log file.
        /// </summary>
        /// <value>The currently configured logging service instance.</value>
        public static ILogService Logger
        {
            get
            {
                // Lazy initialization with default logger if none has been set
                if (_logger == null)
                    _logger = new TsvLogService(modName: "Core");
                return _logger;
            }
        }

        /// <summary>
        /// Sets a custom logging service for the core library to use.
        /// Mods should call this method during initialization to ensure that all core library
        /// log messages are written to the mod's log file rather than a separate core log.
        /// </summary>
        /// <param name="logger">The logging service to use. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when the logger parameter is null.</exception>
        public static void SetLogger(ILogService logger)
        {
            // Validate parameter and assign the custom logger
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
    }
}