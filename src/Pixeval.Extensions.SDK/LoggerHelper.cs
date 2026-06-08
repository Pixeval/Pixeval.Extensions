using System;
using System.Runtime.CompilerServices;
using Pixeval.Extensions.Common;

namespace Pixeval.Extensions.SDK;

public static class LoggerHelper
{
    extension(ILogger logger)
    {
        public void Log(LogLevel logLevel, string message, Exception? exception,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) =>
            logger.Log(logLevel, message, exception?.ToIException(), memberName, filePath, lineNumber);

        public void LogTrace(string message, Exception? exception,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) =>
            logger.Log(LogLevel.Trace, message, exception, memberName, filePath, lineNumber);

        public void LogDebug(string message, Exception? exception,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) =>
            logger.Log(LogLevel.Debug, message, exception, memberName, filePath, lineNumber);

        public void LogInformation(string message, Exception? exception,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) =>
            logger.Log(LogLevel.Information, message, exception, memberName, filePath, lineNumber);

        public void LogWarning(string message, Exception? exception,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) =>
            logger.Log(LogLevel.Warning, message, exception, memberName, filePath, lineNumber);

        public void LogError(string message, Exception? exception,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) =>
            logger.Log(LogLevel.Error, message, exception, memberName, filePath, lineNumber);

        public void LogCritical(string message, Exception? exception,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) =>
            logger.Log(LogLevel.Critical, message, exception, memberName, filePath, lineNumber);
    }
}
