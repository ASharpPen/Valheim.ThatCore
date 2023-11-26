using System;

namespace ThatCore.Logging;

public static class Log
{
    private static ILogger _logger;

    private static LogWriter _traceLogger;
    private static LogWriter _debugLogger;
    private static LogWriter _infoLogger;
    private static LogWriter _warningLogger;
    private static LogWriter _errorLogger;

    public static void SetLogger(ILogger logger)
    {
        _logger = logger;

        _traceLogger = new(logger, LogLevel.Trace);
        _debugLogger = new(logger, LogLevel.Debug);
        _infoLogger = new(logger, LogLevel.Info);
        _warningLogger = new(logger, LogLevel.Warning);
        _errorLogger = new(logger, LogLevel.Error);
    }

    public static bool DevelopmentEnabled { get; set; }
    public static bool TraceEnabled { get; set; }
    public static bool DebugEnabled { get; set; }

    public static LogWriter Development => DevelopmentEnabled ? _debugLogger : null;
    public static LogWriter Trace => TraceEnabled ? _traceLogger : null;
    public static LogWriter Debug => DebugEnabled ? _debugLogger : null;
    public static LogWriter Info => _infoLogger;
    public static LogWriter Warning => _warningLogger;
    public static LogWriter Error => _errorLogger;
}

public class LogWriter
{
    private ILogger _logger;
    private LogLevel _logLevel;

    public LogWriter(ILogger logger, LogLevel logLevel)
    {
        _logger = logger;
        _logLevel = logLevel;
    }

    public void Log(string message) => _logger.Log(message, _logLevel);

    public void Log(string message, Exception e) => 
        _logger.Log($"{message}\n{e?.Message ?? ""}\n{e?.StackTrace ?? ""}", _logLevel);
}
