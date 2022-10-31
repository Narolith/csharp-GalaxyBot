using Discord.Interactions;
using GalaxyBot.Data;

namespace GalaxyBot.Modules.Birthdays;

[Group("birthday", "Birthday commands")]
public class SlashCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly GalaxyBotContext _db;

    public SlashCommand(GalaxyBotContext db)
    {
        _db = db;
    }

    [SlashCommand("add", "Adds or updates user's birthday.")]
    public async Task AddBirthdayCommand([Summary(description: "Digit month of your birthday")] int month,
        [Summary(description: "Digit day of your birthday")]
        int day)
    {
        await DeferAsync(true);
        var user = Context.User;
        var birthday = _db.Birthdays.FirstOrDefault(b => b.UserId == user.Id);
        if (birthday == null)
        {
            _db.Birthdays.Add(new Birthday { UserId = user.Id, Month = month, Day = day });
        }
        else
        {
            birthday.Month = month;
            birthday.Day = day;
        }

        await _db.SaveChangesAsync();
        await FollowupAsync($"Your birthday has been set to {month}/{day}.", ephemeral: true);
    }

    [SlashCommand("remove", "Removes user's birthday.")]
    public async Task RemoveBirthdayCommand()
    {
        // Checks if the user has a birthday in the database and removes it if they do
        await DeferAsync(true);
        var user = Context.User;
        var birthday = _db.Birthdays.FirstOrDefault(b => b.UserId == user.Id);
        if (birthday != null)
        {
            _db.Birthdays.Remove(birthday);
            await _db.SaveChangesAsync();
            await FollowupAsync("Your birthday has been removed.", ephemeral: true);
        }
        else
        {
            await FollowupAsync("You have no birthday to remove.", ephemeral: true);
        }
    }

    [SlashCommand("check", "Checks for and returns user's birthday.")]
    public async Task CheckBirthdayCommand()
    {
        // Checks if the user has a birthday and if so, returns the date
        await DeferAsync(true);
        var user = Context.User;
        var birthday = _db.Birthdays.FirstOrDefault(b => b.UserId == user.Id);
        if (birthday != null)
            await FollowupAsync($"Your birthday is {birthday.Month}/{birthday.Day}", ephemeral: true);
        else
            await FollowupAsync("You have no birthday set.", ephemeral: true);
    }
}