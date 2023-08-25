using BattleBitAPI.Common;
using BBRAPIModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BattleBitBaseModules;

public class ProfanityFilter : BattleBitModule
{
    public ProfanityFilterConfiguration Configuration { get; set; }

    public override Task<bool> OnPlayerTypedMessage(RunnerPlayer player, ChatChannel channel, string message)
    {
        if (this.Configuration.Profanity.Any(x => message.Contains(x, StringComparison.OrdinalIgnoreCase)))
        {
            player.Message("Please do not use profanity in chat.", this.Configuration.MessageTimeout);
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
}

public class ProfanityFilterConfiguration : BattleBitModule
{
    public List<string> Profanity { get; set; } = new();
    public float MessageTimeout { get; set; } = 10f;
}
