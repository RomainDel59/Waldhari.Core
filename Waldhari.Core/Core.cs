using System;

namespace Waldhari.Core
{
    using Logging;

    public static class Core
    {
        private static ILogService _logger;

        public static ILogService Logger
        {
            get
            {
                if (_logger == null)
                    _logger = new TsvLogService();
                return _logger;
            }
        }

        public static void SetLogger(ILogService logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
    }
}