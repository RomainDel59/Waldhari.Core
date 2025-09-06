using System;
using Waldhari.Core.Logging;

namespace Waldhari.Core.Tests.Logging
{
    public class StubLogService : ILogService
    {
        public LogLevel Level { get; set; } = LogLevel.Debug;

        public void Debug(string message) { /* noop */ }
        public void Info(string message) { /* noop */ }
        public void Warn(string message) { /* noop */ }
        public void Error(string message, Exception ex = null) { /* noop */ }
    }
}