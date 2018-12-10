namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Logging
{
    /// <summary>
    /// Service interface for logging messages to their corresponding category's log file.
    /// </summary>
    public interface ILogger
    {
        void LogMessage(string msg);
        void LogWarning(string msg);
        void LogError(string msg);
    }
}
