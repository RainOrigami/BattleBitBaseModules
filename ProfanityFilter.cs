using BattleBitAPI.Common;
using BBRAPIModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BattleBitBaseModules;

public class ProfanityFilter : BattleBitModule
{
    public static ProfanityFilterConfiguration Configuration { get; set; }

    public override Task<bool> OnPlayerTypedMessage(RunnerPlayer player, ChatChannel channel, string message)
    {
        if (Configuration.Profanity.Any(x => message.Contains(x, StringComparison.OrdinalIgnoreCase)))
        {
            player.Message(Configuration.Message, Configuration.MessageTimeout);
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
}

public class ProfanityFilterConfiguration : ModuleConfiguration
{
    public List<string> Profanity { get; set; } = new();
    public float MessageTimeout { get; set; } = 10f;
    public string Message { get; set; } = "Please do not use profanity in chat.";
}
