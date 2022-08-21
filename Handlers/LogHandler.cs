using Discord;

namespace GalaxyBot.Handlers
{
    /// <summary>
    /// Handler responsible for logging.
    /// </summary>
    public class LogHandler
    {
        public static Task LogAsync(LogMessage log)
        {
            var cc = Console.ForegroundColor;

            switch (log.Severity)
            {
                case LogSeverity.Critical:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }

            Console.WriteLine($"{DateTime.Now,-19} [{log.Severity,8}] {log.Source}: {log.Message} {log.Exception}");

            Console.ForegroundColor = cc;

            return Task.CompletedTask;
        }
    }
}