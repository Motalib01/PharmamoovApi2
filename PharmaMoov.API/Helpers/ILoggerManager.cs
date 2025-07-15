namespace PharmaMoov.API.Helpers
{
    public interface ILoggerManager
    {
        void LogDebugObject(object obj);
        void LogInfo(string message);
        void LogWarn(string message);
        void LogDebug(string message);
        void LogError(string message);
    }
}
