using Discord;
using Discord.WebSocket;

namespace GalaxyBot.Handlers;

/// <summary>
///     Handler responsible for logging.
/// </summary>
public static class LogHandler
{
    public static Task LogConsoleAsync(LogMessage log)
    {
        var logMsg = $"{DateTime.Now,-19} [{log.Severity,8}] {log.Source}: {log.Message} {log.Exception}";

        var cc = Console.ForegroundColor;

        Console.ForegroundColor = log.Severity switch
        {
            LogSeverity.Critical => ConsoleColor.Red,
            LogSeverity.Error => ConsoleColor.DarkRed,
            LogSeverity.Warning => ConsoleColor.Yellow,
            LogSeverity.Info => ConsoleColor.White,
            LogSeverity.Verbose => ConsoleColor.Gray,
            LogSeverity.Debug => ConsoleColor.DarkGray,
            _ => Console.ForegroundColor
        };
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
                .WithDescription($"{log.Message}\n{log.Exception}")
                .WithTimestamp(DateTime.UtcNow);

            switch (log.Severity)
            {
                case LogSeverity.Critical:
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