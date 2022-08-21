//using Discord;
//using Discord.Interactions;
//using GalaxyBot.Data;
//using GalaxyBot.Modules.Lfg.Models;

//namespace GalaxyBot.Modules.Lfg
//{
//    public class ModalCommands : InteractionModuleBase<SocketInteractionContext>
//    {
//        private readonly GalaxyBotContext _db;

//        public ModalCommands(GalaxyBotContext db)
//        {
//            _db = db;
//        }
//        /// <summary>
//        /// Processes submitted looking for group modal.
//        /// </summary>
//        /// <param name="modal">Submitted modal</param>
//        /// <returns></returns>
//        [ModalInteraction("lfg_create")]
//        public async Task Create(LfgCreate modal)
//        {
//            await DeferAsync();

//            if (modal.Healers == null || modal.Tanks == null || modal.Dps == null)
//            {
//                await FollowupAsync("You must enter a number for each role.");
//                return;
//            }
//            if (!int.TryParse(modal.Healers, out int numOfHealers) || !int.TryParse(modal.Tanks, out int numOfTanks) || !int.TryParse(modal.Dps, out int numOfDps))
//            {
//                await FollowupAsync("You must enter a number for each role.");
//                return;
//            }
//            else if (numOfHealers + numOfTanks + numOfDps > 8)
//            {
//                await FollowupAsync("You can't have more than 8 people in a group.");
//                return;
//            }

//            // Create a new group and add it to the database
//            var newGroup = new Group
//            {
//                GroupLeaderId = Context.User.Id,
//                NumOfHealers = numOfHealers,
//                NumOfTanks = numOfTanks,
//                NumOfDps = numOfDps
//            };
//            _db.Groups.Add(newGroup);
//            _db.SaveChanges();

//            var embed = new EmbedBuilder()
//                .WithTitle($"Group Id: {newGroup.GroupId.ToString()}")
//                .AddField("Content", modal.Content)
//                .AddField("Group Leader", Context.User.Mention)
//                .WithDescription($"Healers: {modal.Healers}\nTanks: {modal.Tanks}\nDPS: {modal.Dps}")
//                .WithColor(Color.Blue)
//                .Build();

//            await FollowupAsync(embed: embed);
//        }
//    }
//}
