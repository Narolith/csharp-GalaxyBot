using Discord;
using Discord.Interactions;

namespace GalaxyBot.Modules.Lfg.Models
{
    /// <summary>
    /// Lfg modal that asks the user what roles they need for their group.
    /// </summary>
    public class LfgCreate : IModal
    {
        public string Title => "LFG";
        [InputLabel("What content are you running?")]
        [ModalTextInput("content", TextInputStyle.Short, placeholder: "", minLength: 1, maxLength: 80)]
        public string? Content { get; set; }
        [InputLabel("How many healers needed?")]
        [ModalTextInput("healers", TextInputStyle.Short, placeholder: "0", minLength: 1, maxLength: 1)]
        public string? Healers { get; set; }
        [InputLabel("How many tanks needed?")]
        [ModalTextInput("tanks", TextInputStyle.Short, placeholder: "0", minLength: 1, maxLength: 1)]
        public string? Tanks { get; set; }
        [InputLabel("How many dps needed?")]
        [ModalTextInput("dps", TextInputStyle.Short, placeholder: "0", minLength: 1, maxLength: 1)]
        public string? Dps { get; set; }
    }
}
