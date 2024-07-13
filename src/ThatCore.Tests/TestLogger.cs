using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using ThatCore.Logging;

namespace ThatCore;

public sealed class TestLog
{
    public LogLevel LogLevel { get; set; }

    public string Message { get; set; }

    public MethodBase CallingMethod { get; set; }
}

public class TestLogger : ILogger
{
    public List<TestLog> Logs { get; } = new();

    public void Log(
        string message,
        LogLevel logLevel)
    {
        StackTrace stackTrace = new();
        StackFrame[] frames = stackTrace.GetFrames();

        MethodBase method = frames[1].GetMethod();

        if (method.DeclaringType == typeof(TestLog))
        {
            method = frames[2].GetMethod();
        }

        Logs.Add(new TestLog
        {
            LogLevel = logLevel,
            Message = message,
            CallingMethod = method,
        });
    }

    public void LogDebug(string message) =>
        Log(message, LogLevel.Debug);

    public void LogError(string message) =>
        Log(message, LogLevel.Error);

    public void LogInfo(string message) =>
        Log(message, LogLevel.Info);

    public void LogTrace(string message) =>
        Log(message, LogLevel.Trace);

    public void LogWarning(string message) =>
        Log(message, LogLevel.Warning);
}
