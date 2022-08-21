using System.ComponentModel;

namespace GalaxyBot.Data.Models
{
    public enum JobType
    {
        [Description("Healer")]
        Healer,
        [Description("Tank")]
        Tank,
        [Description("Dps")]
        Dps,
        [Description("Any")]
        Any
    }
}

