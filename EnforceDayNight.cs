using BBRAPIModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleBitBaseModules;

/// <summary>
/// Author: @RainOrigami
/// Version: 0.4.5.1
/// </summary>

public class EnforceDayNight : BattleBitModule
{
    public EnforceDayNightConfiguration Configuration { get; set; }
    
    public override Task OnConnected()
    {
        // NOTE: This doesn't work in 0.4.5.1 because CanVoteDay and CanVoteNight have not been implemented yet.
        this.Server.ServerSettings.CanVoteDay = this.Configuration.AllowDayVotes;
        this.Server.ServerSettings.CanVoteNight = this.Configuration.AllowNightVotes;

        return Task.CompletedTask;
    }
}

public class EnforceDayNightConfiguration : ModuleConfiguration
{
    public bool AllowDayVotes { get; set; } = true;
    public bool AllowNightVotes { get; set; } = true;
}
