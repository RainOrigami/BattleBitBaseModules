using BBRAPIModules;
using Commands;
using Permissions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PermissionsManager;

[RequireModule(typeof(CommandHandler))]
[Module("Provide commands for managing GranularPermissions", "1.0.0")]
public class GranularPermissionsCommands : BattleBitModule
{
    [ModuleReference]
    public GranularPermissions GranularPermissions { get; set; } = null!;
    [ModuleReference]
    public CommandHandler CommandHandler { get; set; } = null!;

    public GranularPermissionsCommandsConfiguration Configuration { get; set; } = null!;

    public override void OnModulesLoaded()
    {
        this.CommandHandler.Register(this);
    }

    [CommandCallback("addplayerperm", Description = "Adds a permission to a player", Permissions = new[] { "GranularPermissions.AddPlayerPerm" })]
    public void AddPermissionCommand(RunnerPlayer commandSource, RunnerPlayer player, string permission)
    {
        this.GranularPermissions.AddPlayerPermission(player.SteamID, permission);

        commandSource.Message($"Added permission {permission} to {player.Name}");
    }

    [CommandCallback("removeplayerperm", Description = "Removes a permission from a player", Permissions = new[] { "GranularPermissions.RemovePlayerPerm" })]
    public void RemovePermissionCommand(RunnerPlayer commandSource, RunnerPlayer player, string permission)
    {

        this.GranularPermissions.RemovePlayerPermission(player.SteamID, permission);

        commandSource.Message($"Removed permission {permission} from {player.Name}");

    }

    [CommandCallback("clearplayerperms", Description = "Clears all permissions and groups from a player", Permissions = new[] { "GranularPermissions.ClearPlayerPerms" })]
    public void ClearPermissionCommand(RunnerPlayer commandSource, RunnerPlayer player)
    {
        foreach (string group in this.GranularPermissions.GetPlayerGroups(player.SteamID))
        {
            this.GranularPermissions.RemovePlayerGroup(player.SteamID, group);
        }

        foreach (string permission in this.GranularPermissions.GetPlayerPermissions(player.SteamID))
        {
            this.GranularPermissions.RemovePlayerPermission(player.SteamID, permission);
        }

        commandSource.Message($"Cleared permissions from {player.Name}");
    }

    [CommandCallback("listplayerperms", Description = "Lists player permissions", Permissions = new[] { "GranularPermissions.ListPlayerPerms" })]
    public void ListPermissionCommand(RunnerPlayer commandSource, RunnerPlayer targetPlayer, int page = 1)
    {
        List<string> permissions = new();

        permissions.AddRange(this.GranularPermissions.GetPlayerPermissions(targetPlayer.SteamID));
        foreach (string group in this.GranularPermissions.GetPlayerGroups(targetPlayer.SteamID))
        {
            permissions.AddRange(((string[])this.GranularPermissions.GetGroupPermissions(group)).Select(p => $"{p} from {group}"));
        }

        int pageCount = (int)Math.Ceiling(permissions.Count / (double)this.Configuration.PermissionsPerPage);

        commandSource.Message($"{targetPlayer.Name}: {string.Join("\n", permissions.Skip(page * this.Configuration.PermissionsPerPage).Take(this.Configuration.PermissionsPerPage))}{(pageCount > 1 ? $"{Environment.NewLine}Page {page} of {pageCount}{(page == pageCount ? "" : $", use listperms \"{targetPlayer.Name}\" {page + 1} to see more")}" : "")}");
    }

    [CommandCallback("addplayergroup", Description = "Adds a group to a player", Permissions = new[] { "GranularPermissions.AddPlayerGroup" })]
    public void AddGroupCommand(RunnerPlayer commandSource, RunnerPlayer player, string group)
    {
        if (this.GranularPermissions.GetPlayerGroups(player.SteamID).Contains(group))
        {
            commandSource.Message($"{player.Name} already has group {group}");
            return;
        }

        this.GranularPermissions.AddPlayerGroup(player.SteamID, group);

        commandSource.Message($"Added group {group} to {player.Name}");
    }

    [CommandCallback("removeplayergroup", Description = "Removes a group from a player", Permissions = new[] { "GranularPermissions.RemovePlayerGroup" })]
    public void RemoveGroupCommand(RunnerPlayer commandSource, RunnerPlayer player, string group)
    {
        if (!this.GranularPermissions.GetPlayerGroups(player.SteamID).Contains(group))
        {
            commandSource.Message($"{player.Name} does not have group {group}");
            return;
        }

        this.GranularPermissions.RemovePlayerGroup(player.SteamID, group);

        commandSource.Message($"Removed group {group} from {player.Name}");
    }

    [CommandCallback("clearplayergroups", Description = "Clears all groups from a player", Permissions = new[] { "GranularPermissions.ClearPlayerGroups" })]
    public void ClearGroupCommand(RunnerPlayer commandSource, RunnerPlayer player)
    {
        foreach (string group in this.GranularPermissions.GetPlayerGroups(player.SteamID))
        {
            this.GranularPermissions.RemovePlayerGroup(player.SteamID, group);
        }

        commandSource.Message($"Cleared groups from {player.Name}");
    }

    [CommandCallback("listplayergroups", Description = "Lists player groups", Permissions = new[] { "GranularPermissions.ListPlayerGroups" })]
    public void ListGroupCommand(RunnerPlayer commandSource, RunnerPlayer targetPlayer, int page = 1)
    {
        List<string> groups = new();

        groups.AddRange(this.GranularPermissions.GetPlayerGroups(targetPlayer.SteamID));

        int pageCount = (int)Math.Ceiling(groups.Count / (double)this.Configuration.PermissionsPerPage);

        commandSource.Message($"{targetPlayer.Name}: {string.Join("\n", groups.Skip(page * this.Configuration.PermissionsPerPage).Take(this.Configuration.PermissionsPerPage))}{(pageCount > 1 ? $"{Environment.NewLine}Page {page} of {pageCount}{(page == pageCount ? "" : $", use listgroups \"{targetPlayer.Name}\" {page + 1} to see more")}" : "")}");
    }

    [CommandCallback("addgroupperm", Description = "Adds a permission to a group", Permissions = new[] { "GranularPermissions.AddGroupPerm" })]
    public void AddGroupPermissionCommand(RunnerPlayer commandSource, string group, string permission)
    {
        if (this.GranularPermissions.GetGroupPermissions(group).Contains(permission))
        {
            commandSource.Message($"{group} already has permission {permission}");
            return;
        }

        this.GranularPermissions.AddGroupPermission(group, permission);

        commandSource.Message($"Added permission {permission} to {group}");
    }

    [CommandCallback("removegroupperm", Description = "Removes a permission from a group", Permissions = new[] { "GranularPermissions.RemoveGroupPerm" })]
    public void RemoveGroupPermissionCommand(RunnerPlayer commandSource, string group, string permission)
    {
        if (!this.GranularPermissions.GetGroupPermissions(group).Contains(permission))
        {
            commandSource.Message($"{group} does not have permission {permission}");
            return;
        }

        this.GranularPermissions.RemoveGroupPermission(group, permission);

        commandSource.Message($"Removed permission {permission} from {group}");
    }

    [CommandCallback("cleargroupperms", Description = "Clears all permissions from a group", Permissions = new[] { "GranularPermissions.ClearGroupPerms" })]
    public void ClearGroupPermissionCommand(RunnerPlayer commandSource, string group)
    {
        foreach (string permission in this.GranularPermissions.GetGroupPermissions(group))
        {
            this.GranularPermissions.RemoveGroupPermission(group, permission);
        }

        commandSource.Message($"Cleared permissions from {group}");
    }

    [CommandCallback("listgroupperms", Description = "Lists group permissions", Permissions = new[] { "GranularPermissions.ListGroupPerms" })]
    public void ListGroupPermissionCommand(RunnerPlayer commandSource, string group, int page = 1)
    {
        List<string> permissions = new();

        permissions.AddRange(this.GranularPermissions.GetGroupPermissions(group));

        int pageCount = (int)Math.Ceiling(permissions.Count / (double)this.Configuration.PermissionsPerPage);

        commandSource.Message($"{group}: {string.Join("\n", permissions.Skip(page * this.Configuration.PermissionsPerPage).Take(this.Configuration.PermissionsPerPage))}{(pageCount > 1 ? $"{Environment.NewLine}Page {page} of {pageCount}{(page == pageCount ? "" : $", use listgroupperms \"{group}\" {page + 1} to see more")}" : "")}");
    }
}

public class GranularPermissionsCommandsConfiguration : ModuleConfiguration
{
    public int PermissionsPerPage { get; set; } = 6;
}
