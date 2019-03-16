using System;

namespace BestPractices.Correlation.Contracts
{
    public interface ILogger
    {
        void LogEvent(string eventName);
        void LogException(Exception ex);
    }
}
