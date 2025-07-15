using Newtonsoft.Json;
using NLog;

namespace PharmaMoov.API.Helpers
{
    public class LoggerManager : ILoggerManager
    {
        private ILogger logger = LogManager.GetCurrentClassLogger();

        public LoggerManager()
        {
        }

        public void LogDebugObject(object obj)
        {
            if (obj != null) logger.Info("\n----- Object Inspect: -----\n" + JsonConvert.SerializeObject(obj, Formatting.Indented).ToString() + "\n---------------------------");
        }

        public void LogDebug(string message)
        {
            logger.Debug(message);
        }

        public void LogError(string message)
        {
            logger.Error(message);
        }

        public void LogInfo(string message)
        {
            logger.Info(message);
        }

        public void LogWarn(string message)
        {
            logger.Warn(message);
        }
    }
}
