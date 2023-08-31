using BattleBitAPI.Common;
using BBRAPIModules;
using Permissions;
using System.Threading.Tasks;

namespace BattleBitBaseModules;

/// <summary>
/// Author: @RainOrigami
/// Version: 0.4.5.1
/// </summary>

[RequireModule(typeof(PlayerPermissions))]
public class ReservedSlots : BattleBitModule
{
    public ReservedSlotsConfiguration Configuration { get; set; }
    public PlayerPermissions PlayerPermissions { get; set; }

    public override Task OnPlayerJoiningToServer(ulong steamID, PlayerJoiningArguments args)
    {
        if (this.Server.MaxPlayerCount - this.Server.CurrentPlayerCount > this.Configuration.ReservedSlots)
        {
            return Task.CompletedTask;
        }

        if ((this.PlayerPermissions.GetPlayerRoles(steamID) & this.Configuration.AllowedRoles) > 0)
        {
            return Task.CompletedTask;
        }

        // Reject player because server is full and player doesn't have required role
        // TODO: verify if this is the correct way to reject player
        args.Stats = null;

        return Task.CompletedTask;
    }
}

public class ReservedSlotsConfiguration : ModuleConfiguration
{
    public int ReservedSlots { get; set; } = 2;
    public Roles AllowedRoles { get; set; } = Roles.Admin | Roles.Moderator;
}
