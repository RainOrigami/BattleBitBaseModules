using BattleBitAPI.Common;
using BBRAPIModules;
using Commands;
using Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PermissionsManager;

/// <summary>
/// Author: @RainOrigami
/// Version: 0.4.5.2
/// </summary>

[RequireModule(typeof(PlayerPermissions))]
[RequireModule(typeof(CommandHandler))]
public class PermissionsCommands : BattleBitModule
{
    [ModuleReference]
    public PlayerPermissions PlayerPermissions { get; set; }
    [ModuleReference]
    public CommandHandler CommandHandler { get; set; }

    public override void OnModulesLoaded() => this.CommandHandler.Register(this);

    [CommandCallback("addperm", Description = "Adds a permission to a player", AllowedRoles = Roles.Admin)]
    public void AddPermissionCommand(RunnerPlayer commandSource, RunnerPlayer player, Roles permission)
    {
        this.PlayerPermissions.AddPlayerRoles(player.SteamID, permission);
        commandSource.Message($"Added permission {permission} to {player.Name}");
    }

    [CommandCallback("removeperm", Description = "Removes a permission from a player", AllowedRoles = Roles.Admin)]
    public void RemovePermissionCommand(RunnerPlayer commandSource, RunnerPlayer player, Roles permission)
    {
        this.PlayerPermissions.RemovePlayerRoles(player.SteamID, permission);
        commandSource.Message($"Removed permission {permission} from {player.Name}");
    }

    [CommandCallback("clearperms", Description = "Removes all permission from a player", AllowedRoles = Roles.Admin)]
    public void ClearPermissionCommand(RunnerPlayer commandSource, RunnerPlayer player) => RemovePermissionCommand(commandSource, player, (Roles)15);

    [CommandCallback("listperms", Description = "Lists player permissions", AllowedRoles = Roles.Admin | Roles.Moderator)]
    public void ListPermissionCommand(RunnerPlayer commandSource, RunnerPlayer? targetPlayer = null) {
        var sb = new StringBuilder();
        if (targetPlayer is null) {
            foreach (var permissionsPlayer in PlayerPermissions.Configuration.PlayerRoles) {
                var roles = GetIndividualRoleStrings(permissionsPlayer.Value);
                sb.AppendLine($"{GetPlayerNameOrId(permissionsPlayer.Key)}: {string.Join(", ", roles)}");
            }
        } else {
            var roles = GetIndividualRoleStrings(PlayerPermissions.GetPlayerRoles(targetPlayer.SteamID));
            sb.AppendLine($"{GetPlayerNameOrId(targetPlayer.SteamID)}: {string.Join('\n', roles)}");
        }
        commandSource.Message(sb.ToString());
    }

    public Roles[] GetIndividualRoles(Roles roles) {
        List<Roles> permStrs = new();
        foreach (var role in Enum.GetValues<Roles>()) {
            if ((role & roles) > 0) permStrs.Add(role);
        }
        return permStrs.ToArray();
    }
    public string[] GetIndividualRoleStrings(Roles roles) => GetIndividualRoles(roles).Select(r => r.ToString()).ToArray();
    public string GetPlayerNameOrId(ulong steamId64) => this.Server.AllPlayers.FirstOrDefault(p => p.SteamID == steamId64)?.Name ?? steamId64.ToString();

}
