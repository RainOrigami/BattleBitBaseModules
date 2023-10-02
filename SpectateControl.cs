using BattleBitAPI.Common;
using BBRAPIModules;
using Permissions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BattleBitBaseModules;

[RequireModule(typeof(GranularPermissions))]
[Module("Allow only specific Roles to spectate", "1.1.0")]
public class SpectateControl : BattleBitModule
{
    public static SpectateControlConfiguration Configuration { get; set; } = null!;

    [ModuleReference]
    public GranularPermissions GranularPermissions { get; set; } = null!;

    public override Task OnPlayerConnected(RunnerPlayer player)
    {
        player.Modifications.CanSpectate = Configuration.SpectatorPermissions.Any(p => this.GranularPermissions.HasPermission(player.SteamID, p));

        return Task.CompletedTask;
    }
}

public class SpectateControlConfiguration : ModuleConfiguration
{
    public List<string> SpectatorPermissions { get; set; } = new();
}
