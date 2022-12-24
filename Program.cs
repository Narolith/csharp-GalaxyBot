namespace GalaxyBot;

public class Program
{
    private static async Task Main(string[] args)
    {
        await Services.StartBotServices(args);
    }
}