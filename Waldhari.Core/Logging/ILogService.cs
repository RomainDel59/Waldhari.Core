using System;

namespace Waldhari.Core.Logging
{
    public interface ILogService
    {
        LogLevel Level { get; set; }

        void Debug(string message);
        void Info(string message);
        void Warn(string message);
        void Error(string message, Exception ex = null);
    }
}