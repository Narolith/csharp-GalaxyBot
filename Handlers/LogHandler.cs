using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace GalaxyBot.Handlers
{
    /// <summary>
    /// Handler responsible for logging.
    /// </summary>
    public class LogHandler
    {
        public static Task LogConsoleAsync(LogMessage log)
        {
            var logMsg = $"{DateTime.Now,-19} [{log.Severity,8}] {log.Source}: {log.Message} {log.Exception}";

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
            Console.WriteLine(logMsg);
            Console.ForegroundColor = cc;

            return Task.CompletedTask;
        }

        public static Task LogChannelAsync(SocketTextChannel logChannel, LogMessage log)
        {
            Task.Run(async () =>
            {
                var embed = new EmbedBuilder()
                    .WithColor(new Color(0xFF0000))
                    .WithTitle($"{log.Source} [{log.Severity}]")
                    .WithDescription($"{ log.Message}\n{ log.Exception}")
                    .WithTimestamp(DateTime.UtcNow);
                
                switch (log.Severity)
                {
                    case LogSeverity.Critical:
                        embed.WithColor(new Color(0xFF0000));
                        break;
                    case LogSeverity.Error:
                        embed.WithColor(new Color(0xFF0000));
                        break;
                    case LogSeverity.Warning:
                        embed.WithColor(new Color(0xFFA500));
                        break;
                    case LogSeverity.Info:
                        embed.WithColor(new Color(0xFFFFFF));
                        break;
                    case LogSeverity.Verbose:
                        embed.WithColor(new Color(0xC0C0C0));
                        break;
                    case LogSeverity.Debug:
                        embed.WithColor(new Color(0x808080));
                        break;
                }
                await logChannel.SendMessageAsync(embed: embed.Build());
            });

            return Task.CompletedTask;
        }
    }
}