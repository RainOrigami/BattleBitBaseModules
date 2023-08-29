using BattleBitAPI.Common;
using BBRAPIModules;
using Permissions;
using System.Threading.Tasks;

namespace BattleBitBaseModules;

[RequireModule(typeof(PlayerPermissions))]
[Module("Allow only specific Roles to spectate", "1.0.0")]
public class SpectateControl : BattleBitModule
{
    public static SpectateControlConfiguration Configuration { get; set; }

    [ModuleReference]
    public PlayerPermissions PlayerPermissions { get; set; }

    public override Task OnPlayerConnected(RunnerPlayer player)
    {
        player.Modifications.CanSpectate = Configuration.SpectatorRoles == 0 || (PlayerPermissions.GetPlayerRoles(player.SteamID) & (Roles)Configuration.SpectatorRoles) > 0;

        return Task.CompletedTask;
    }
}

public class SpectateControlConfiguration : ModuleConfiguration
{
    public ulong SpectatorRoles { get; set; } = (ulong)(Roles.Admin | Roles.Moderator);
}
