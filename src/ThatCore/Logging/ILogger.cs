using System;

namespace ThatCore.Logging;

public interface ILogger
{
    void LogTrace(string message);
    void LogDebug(string message);
    void LogInfo(string message);
    void LogWarning(string message);
    void LogError(string message);

    void Log(string message, LogLevel logLevel);
}