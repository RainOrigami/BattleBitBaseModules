using BattleBitAPI.Common;
using BBRAPIModules;
using Commands;
using Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PermissionsManager;

[RequireModule(typeof(PlayerPermissions))]
[RequireModule(typeof(CommandHandler))]
[Module("Provide addperm and removeperm commands for PlayerPermissions", "1.0.0")]
public class PermissionsCommands : BattleBitModule
{
    [ModuleReference]
    public PlayerPermissions PlayerPermissions { get; set; }
    [ModuleReference]
    public CommandHandler CommandHandler { get; set; }

    public override void OnModulesLoaded()
    {
        this.CommandHandler.Register(this);
    }

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
    public void ClearPermissionCommand(RunnerPlayer commandSource, RunnerPlayer player)
    {
        foreach (Roles role in Enum.GetValues<Roles>())
        {
            this.PlayerPermissions.RemovePlayerRoles(player.SteamID, role);
        }

        commandSource.Message($"Cleared permissions from {player.Name}");
    }

    [CommandCallback("listperms", Description = "Lists player permissions", AllowedRoles = Roles.Admin | Roles.Moderator)]
    public void ListPermissionCommand(RunnerPlayer commandSource, RunnerPlayer? targetPlayer = null)
    {
        StringBuilder response = new();
        List<ulong> targetSteamIds = targetPlayer is null ? PlayerPermissions.Configuration.PlayerRoles.Keys.ToList() : new() { targetPlayer.SteamID };

        foreach (ulong playerSteamId in targetSteamIds)
        {
            string playerName = this.Server.AllPlayers.FirstOrDefault(p => p.SteamID == playerSteamId)?.Name ?? playerSteamId.ToString();
            Roles playerRoles = PlayerPermissions.Configuration.PlayerRoles.GetValueOrDefault(playerSteamId);
            Roles[] individualRoles = Enum.GetValues<Roles>().Where(r => (r & playerRoles) > 0).ToArray();

            response.AppendLine($"{playerName}: {string.Join(targetSteamIds.Count == 1 ? "\n" : ", ", individualRoles)}");
        }

        commandSource.Message(response.ToString());
    }
}