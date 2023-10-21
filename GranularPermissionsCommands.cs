using BBRAPIModules;
using Commands;
using Permissions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PermissionsManager;

[RequireModule(typeof(CommandHandler))]
[RequireModule(typeof(GranularPermissions))]
[Module("Provide commands for managing GranularPermissions", "1.0.1")]
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

    [CommandCallback("addplayerperm", Description = "Adds a permission to a player", Permissions = new[] { "GranularPermissions.AddPlayerPerm" }, ConsoleCommand = true)]
    public string AddPermissionCommand(Context context, RunnerPlayer player, string permission)
    {
        this.GranularPermissions.AddPlayerPermission(player.SteamID, permission);
        this.GranularPermissions.Save();

        this.Logger.Debug($"Added permission {permission} to {player.Name}");

        return $"Added permission {permission} to {player.Name}";
    }

    [CommandCallback("removeplayerperm", Description = "Removes a permission from a player", Permissions = new[] { "GranularPermissions.RemovePlayerPerm" }, ConsoleCommand = true)]
    public string RemovePermissionCommand(Context context, RunnerPlayer player, string permission)
    {
        this.GranularPermissions.RemovePlayerPermission(player.SteamID, permission);
        this.GranularPermissions.Save();

        this.Logger.Debug($"Removed permission {permission} from {player.Name}");

        return $"Removed permission {permission} from {player.Name}";
    }

    [CommandCallback("clearplayerperms", Description = "Clears all permissions and groups from a player", Permissions = new[] { "GranularPermissions.ClearPlayerPerms" }, ConsoleCommand = true)]
    public string ClearPermissionCommand(Context context, RunnerPlayer player)
    {
        foreach (string group in this.GranularPermissions.GetPlayerGroups(player.SteamID))
        {
            this.GranularPermissions.RemovePlayerGroup(player.SteamID, group);
        }

        foreach (string permission in this.GranularPermissions.GetPlayerPermissions(player.SteamID))
        {
            this.GranularPermissions.RemovePlayerPermission(player.SteamID, permission);
        }

        this.GranularPermissions.Save();

        this.Logger.Debug($"Cleared permissions from {player.Name}");

        return $"Cleared permissions from {player.Name}";
    }

    [CommandCallback("listplayerperms", Description = "Lists player permissions", Permissions = new[] { "GranularPermissions.ListPlayerPerms" }, ConsoleCommand = true)]
    public string ListPermissionCommand(Context context, RunnerPlayer targetPlayer, int page = 1)
    {
        if (page < 1)
        {
            page = 1;
        }

        string[] permissions = this.GranularPermissions.GetAllPlayerPermissions(targetPlayer.SteamID);

        int pageCount = (int)Math.Ceiling(permissions.Length / (double)this.Configuration.PermissionsPerPage);

        this.Logger.Debug($"Listing permissions for {targetPlayer.Name}");

        return $"{targetPlayer.Name}:{Environment.NewLine}{string.Join("\n", permissions.Skip((page - 1) * this.Configuration.PermissionsPerPage).Take(this.Configuration.PermissionsPerPage))}{(pageCount > 1 ? $"{Environment.NewLine}Page {page} of {pageCount}{(page == pageCount ? "" : $", use listperms \"{targetPlayer.Name}\" {page + 1} to see more")}" : "")}";
    }

    [CommandCallback("addplayergroup", Description = "Adds a group to a player", Permissions = new[] { "GranularPermissions.AddPlayerGroup" }, ConsoleCommand = true)]
    public string AddGroupCommand(Context context, RunnerPlayer player, string group)
    {
        if (this.GranularPermissions.GetPlayerGroups(player.SteamID).Contains(group))
        {
            return $"{player.Name} already has group {group}";
        }

        if (!this.GranularPermissions.GetGroups().Contains(group))
        {
            return $"Group {group} does not exist";
        }

        this.GranularPermissions.AddPlayerGroup(player.SteamID, group);
        this.GranularPermissions.Save();

        this.Logger.Debug($"Added group {group} to {player.Name}");

        return $"Added group {group} to {player.Name}";
    }

    [CommandCallback("removeplayergroup", Description = "Removes a group from a player", Permissions = new[] { "GranularPermissions.RemovePlayerGroup" }, ConsoleCommand = true)]
    public string RemoveGroupCommand(Context context, RunnerPlayer player, string group)
    {
        if (!this.GranularPermissions.GetPlayerGroups(player.SteamID).Contains(group))
        {
            return $"{player.Name} does not have group {group}";
        }

        this.GranularPermissions.RemovePlayerGroup(player.SteamID, group);
        this.GranularPermissions.Save();

        this.Logger.Debug($"Removed group {group} from {player.Name}");

        return $"Removed group {group} from {player.Name}";
    }

    [CommandCallback("clearplayergroups", Description = "Clears all groups from a player", Permissions = new[] { "GranularPermissions.ClearPlayerGroups" }, ConsoleCommand = true)]
    public string ClearGroupCommand(Context context, RunnerPlayer player)
    {
        foreach (string group in this.GranularPermissions.GetPlayerGroups(player.SteamID))
        {
            this.GranularPermissions.RemovePlayerGroup(player.SteamID, group);
        }

        this.GranularPermissions.Save();

        this.Logger.Debug($"Cleared groups from {player.Name}");

        return $"Cleared groups from {player.Name}";
    }

    [CommandCallback("listplayergroups", Description = "Lists player groups", Permissions = new[] { "GranularPermissions.ListPlayerGroups" }, ConsoleCommand = true)]
    public string ListGroupCommand(Context context, RunnerPlayer targetPlayer, int page = 1)
    {
        if (page < 1)
        {
            page = 1;
        }

        List<string> groups = new();

        groups.AddRange(this.GranularPermissions.GetPlayerGroups(targetPlayer.SteamID));

        int pageCount = (int)Math.Ceiling(groups.Count / (double)this.Configuration.PermissionsPerPage);

        this.Logger.Debug($"Listing groups for {targetPlayer.Name}");

        return $"{targetPlayer.Name}:{Environment.NewLine}{string.Join("\n", groups.Skip((page - 1) * this.Configuration.PermissionsPerPage).Take(this.Configuration.PermissionsPerPage))}{(pageCount > 1 ? $"{Environment.NewLine}Page {page} of {pageCount}{(page == pageCount ? "" : $", use listgroups \"{targetPlayer.Name}\" {page + 1} to see more")}" : "")}";
    }

    [CommandCallback("addgroup", Description = "Adds a group", Permissions = new[] { "GranularPermissions.AddGroup" }, ConsoleCommand = true)]
    public string AddGroupCommand(Context context, string group)
    {
        if (this.GranularPermissions.GetGroups().Contains(group))
        {
            return $"Group {group} already exists";
        }

        this.GranularPermissions.AddGroup(group);
        this.GranularPermissions.Save();

        this.Logger.Debug($"Added group {group}");

        return $"Added group {group}";
    }

    [CommandCallback("removegroup", Description = "Removes a group", Permissions = new[] { "GranularPermissions.RemoveGroup" }, ConsoleCommand = true)]
    public string RemoveGroupCommand(Context context, string group)
    {
        if (!this.GranularPermissions.GetGroups().Contains(group))
        {
            return $"Group {group} does not exist";
        }

        this.GranularPermissions.RemoveGroup(group);
        this.GranularPermissions.Save();

        this.Logger.Debug($"Removed group {group}");

        return $"Removed group {group}";
    }

    [CommandCallback("listgroups", Description = "Lists groups", Permissions = new[] { "GranularPermissions.ListGroups" }, ConsoleCommand = true)]
    public string ListGroupCommand(Context context, int page = 1)
    {
        if (page < 1)
        {
            page = 1;
        }

        List<string> groups = new();

        groups.AddRange(this.GranularPermissions.GetGroups());

        int pageCount = (int)Math.Ceiling(groups.Count / (double)this.Configuration.PermissionsPerPage);

        this.Logger.Debug($"Listing groups");

        return $"{Environment.NewLine}{string.Join("\n", groups.Skip((page - 1) * this.Configuration.PermissionsPerPage).Take(this.Configuration.PermissionsPerPage))}{(pageCount > 1 ? $"{Environment.NewLine}Page {page} of {pageCount}{(page == pageCount ? "" : $", use listgroups {page + 1} to see more")}" : "")}";
    }

    [CommandCallback("addgroupperm", Description = "Adds a permission to a group", Permissions = new[] { "GranularPermissions.AddGroupPerm" }, ConsoleCommand = true)]
    public string AddGroupPermissionCommand(Context context, string group, string permission)
    {
        if (!this.GranularPermissions.GetGroups().Contains(group))
        {
            return $"Group {group} does not exist";
        }

        if (this.GranularPermissions.GetGroupPermissions(group).Contains(permission))
        {
            return $"{group} already has permission {permission}";
        }

        this.GranularPermissions.AddGroupPermission(group, permission);
        this.GranularPermissions.Save();

        this.Logger.Debug($"Added permission {permission} to {group}");

        return $"Added permission {permission} to {group}";
    }

    [CommandCallback("removegroupperm", Description = "Removes a permission from a group", Permissions = new[] { "GranularPermissions.RemoveGroupPerm" }, ConsoleCommand = true)]
    public string RemoveGroupPermissionCommand(Context context, string group, string permission)
    {
        if (!this.GranularPermissions.GetGroups().Contains(group))
        {
            return $"Group {group} does not exist";
        }

        if (!this.GranularPermissions.GetGroupPermissions(group).Contains(permission))
        {
            return $"{group} does not have permission {permission}";
        }

        this.GranularPermissions.RemoveGroupPermission(group, permission);
        this.GranularPermissions.Save();

        this.Logger.Debug($"Removed permission {permission} from {group}");

        return $"Removed permission {permission} from {group}";
    }

    [CommandCallback("cleargroupperms", Description = "Clears all permissions from a group", Permissions = new[] { "GranularPermissions.ClearGroupPerms" }, ConsoleCommand = true)]
    public string ClearGroupPermissionCommand(Context context, string group)
    {
        if (!this.GranularPermissions.GetGroups().Contains(group))
        {
            return $"Group {group} does not exist";
        }

        foreach (string permission in this.GranularPermissions.GetGroupPermissions(group))
        {
            this.GranularPermissions.RemoveGroupPermission(group, permission);
        }

        this.GranularPermissions.Save();

        this.Logger.Debug($"Cleared permissions from {group}");

        return $"Cleared permissions from {group}";
    }

    [CommandCallback("listgroupperms", Description = "Lists group permissions", Permissions = new[] { "GranularPermissions.ListGroupPerms" }, ConsoleCommand = true)]
    public string ListGroupPermissionCommand(Context context, string group, int page = 1)
    {
        if (!this.GranularPermissions.GetGroups().Contains(group))
        {
            return $"Group {group} does not exist";
        }

        if (page < 1)
        {
            page = 1;
        }

        List<string> permissions = new();

        permissions.AddRange(this.GranularPermissions.GetGroupPermissions(group));

        int pageCount = (int)Math.Ceiling(permissions.Count / (double)this.Configuration.PermissionsPerPage);

        return $"{group}:{Environment.NewLine}{string.Join("\n", permissions.Skip((page - 1) * this.Configuration.PermissionsPerPage).Take(this.Configuration.PermissionsPerPage))}{(pageCount > 1 ? $"{Environment.NewLine}Page {page} of {pageCount}{(page == pageCount ? "" : $", use listgroupperms \"{group}\" {page + 1} to see more")}" : "")}";
    }
}

public class GranularPermissionsCommandsConfiguration : ModuleConfiguration
{
    public int PermissionsPerPage { get; set; } = 6;
}
