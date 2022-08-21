//using Discord.Interactions;
//using GalaxyBot.Data;
//using GalaxyBot.Modules.Lfg.Models;

//namespace GalaxyBot.modules.Lfg
//{

//    /// <summary>
//    /// Lfg command module.
//    /// </summary>
//    [Group("lfg", "Create, Join, or Leave groups!")]
//    public class SlashCommand : InteractionModuleBase<SocketInteractionContext>
//    {
//        private readonly GalaxyBotContext _db;

//        public SlashCommand(GalaxyBotContext db)
//        {
//            _db = db;
//        }
//        /// <summary>
//        /// Lfg slash command that creates an Lfg request.
//        /// </summary>
//        [SlashCommand("create", "Find a group to play with")]
//        public async Task CreateLfgCommand()
//        {
//            // Ask the user what roles they need for their group
//            await RespondWithModalAsync<LfgCreate>("lfg_create");
//        }

//        [SlashCommand("join", "Join a group")]
//        public async Task JoinLfgCommand(int groupId, PlayerClass playerClass)
//        {
//            await DeferAsync();
//            bool inGroup = _db.GroupUsers.FirstOrDefault(gu => gu.GroupId == groupId && gu.UserId == Context.User.Id) != null;

//            if (inGroup)
//            {
//                await FollowupAsync("You are already in this group!");
//            }
//            else
//            {
//                _db.GroupUsers.Add(new GroupUser { GroupId = groupId, UserId = Context.User.Id, Class = playerClass });
//                _db.SaveChanges();
//                await FollowupAsync($"{Context.User.Username} has joined group {groupId}");
//            }
//        }

//        [SlashCommand("leave", "Leave a group")]
//        public async Task LeaveLfgCommand(int groupId)
//        {
//            await DeferAsync();
//            var group = _db.GroupUsers.FirstOrDefault(gu => gu.GroupId == groupId && gu.UserId == Context.User.Id);

//            if (group != null)
//            {
//                _db.GroupUsers.Remove(group);
//                _db.SaveChanges();
//                await FollowupAsync($"{Context.User.Username} has left group {groupId}");
//            }
//            else
//            {
//                await FollowupAsync("You are not in this group!");
//            }
//        }
//    }
//}
