using Discord.Interactions;
using Discord.WebSocket;
using GalaxyBot.Data;
using GalaxyBot.Handlers;
using GalaxyBot.Modules.Games;
using Microsoft.Extensions.DependencyInjection;

namespace GalaxyBot.Extensions
{
    internal static class IServiceCollectionExt
    {
        /// <summary>
        /// Configures bot services.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="isProd"></param>
        internal static void AddBotServices(this IServiceCollection services, bool isProd)
        {
            services.AddSingleton(Configuration.GetDiscordConfiguration())
            .AddSingleton(Configuration.GetEnvironmentConfiguration(isProd))
            .AddSingleton(_ => new DiscordSocketClient(_.GetRequiredService<DiscordSocketConfig>()))
            .AddSingleton(_ => new Firestore(_.GetRequiredService<Secrets>()))
            .AddSingleton(_ => new GameJobs(_.GetRequiredService<Firestore>(), _.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton(_ => new InteractionService(_.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<InteractionHandler>();
        }
    }
}
