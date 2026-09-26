using System;

namespace ThatCore.Logging;

public class ConsoleLogger(string name = "ThatCore") : ILogger
{
    public void LogTrace(string message)
    {
        System.Console.WriteLine($"[Trace  : {name}] {message}");
    }

    public void LogDebug(string message)
    {
        System.Console.WriteLine($"[Debug  : {name}] {message}");
    }

    public void LogInfo(string message)
    {
        System.Console.WriteLine($"[Info   : {name}] {message}");
    }

    public void LogWarning(string message)
    {
        System.Console.WriteLine($"[Warning: {name}] {message}");
    }

    public void LogError(string message)
    {
        System.Console.WriteLine($"[Error  : {name}] {message}");
    }

    public void Log(string message, LogLevel level)
    {
        switch (level)
        {
            case LogLevel.Error:
                LogError(message);
                break;
            case LogLevel.Warning:
                LogWarning(message);
                break;
            case LogLevel.Info:
                LogInfo(message);
                break;
            case LogLevel.Debug:
                LogDebug(message);
                break;
            case LogLevel.Trace:
                LogTrace(message);
                break;
            default:
                break;
        }
    }
}
