using System;
using BepInEx.Logging;

namespace ThatCore.Logging;

public class BepInExLogger : ILogger
{
    private readonly ManualLogSource _logger;

    public BepInExLogger(ManualLogSource logger)
    {
        _logger = logger;
    }

    public void LogTrace(string message) => _logger.LogDebug(message);
    public void LogDebug(string message) => _logger.LogInfo(message);
    public void LogInfo(string message) => _logger.LogMessage(message); 
    public void LogWarning(string message) => _logger.LogWarning(message);
    public void LogError(string message) => _logger.LogError(message);

    public void Log(string message, LogLevel level)
    {
        switch (level)
        {
            case LogLevel.Error:
                _logger.LogError(message);
                break;
            case LogLevel.Warning:
                _logger.LogWarning(message);
                break;
            case LogLevel.Info:
                _logger.LogMessage(message);
                break;
            case LogLevel.Debug:
                _logger.LogInfo(message);
                break;
            case LogLevel.Trace:
                _logger.LogDebug(message);
                break;
            default:
                break;
        }
    }
}
