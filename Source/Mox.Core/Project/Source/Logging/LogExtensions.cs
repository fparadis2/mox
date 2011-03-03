using System;

namespace Mox
{
    public static class LogExtensions
    {
        #region Methods

        public static void Log(this ILog log, LogImportance importance, string msg, params object[] args)
        {
            LogMessage logMessage = new LogMessage
            {
                Text = string.Format(msg, args),
                Importance = importance
            };
            log.Log(logMessage);
        }

        public static void LogWarning(this ILog log, string msg, params object[] args)
        {
            log.Log(LogImportance.Warning, msg, args);
        }

        public static void LogError(this ILog log, string msg, params object[] args)
        {
            log.Log(LogImportance.Error, msg, args);
        }

        #endregion
    }
}
