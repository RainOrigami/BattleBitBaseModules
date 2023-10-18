using BattleBitAPI.Common;
using BBRAPIModules;
using Commands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PermissionsManager;

[RequireModule(typeof(CommandHandler))]
[Module("Provide addperm and removeperm commands for PlayerPermissions", "1.1.0")]
public class PermissionsCommands : BattleBitModule
{
    [ModuleReference]
    public dynamic? PlayerPermissions { get; set; }
    [ModuleReference]
    public dynamic? GranularPermissions { get; set; }
    [ModuleReference]
    public CommandHandler CommandHandler { get; set; } = null!;

    public PermissionsCommandsConfiguration Configuration { get; set; } = null!;

    public override void OnModulesLoaded()
    {
        this.CommandHandler.Register(this);
    }

    [CommandCallback("addperm", Description = "Adds a permission to a player", Permissions = new[] { "Permissions.Add" })]
    public string AddPermissionCommand(Context context, RunnerPlayer player, string permission)
    {
        bool success = false;

        if (this.PlayerPermissions is not null)
        {
            if (!Enum.TryParse(permission, out Roles roles))
            {
                this.Logger.Error($"Could not parse {permission} to a role");
            }
            else
            {
                this.PlayerPermissions.AddPlayerRoles(player.SteamID, roles);
                success = true;
            }
        }

        if (this.GranularPermissions is not null)
        {
            this.GranularPermissions.AddPlayerPermission(player.SteamID, permission);
            success = true;
        }

        if (success)
        {
            this.Logger.Info($"Added permission {permission} to {player.Name}");
            return $"Added permission {permission} to {player.Name}";
        }
        else
        {
            this.Logger.Error($"Could not add permission {permission} to {player.Name}");
            return $"Could not add permission {permission} to {player.Name}";
        }
    }

    [CommandCallback("removeperm", Description = "Removes a permission from a player", Permissions = new[] { "Permissions.Remove" })]
    public string RemovePermissionCommand(Context context, RunnerPlayer player, string permission)
    {
        bool success = false;

        if (this.PlayerPermissions is not null)
        {
            if (!Enum.TryParse(permission, out Roles roles))
            {
                this.Logger.Error($"Colud not parse {permission} to a role");
            }
            else
            {
                this.PlayerPermissions.RemovePlayerRoles(player.SteamID, roles);
                success = true;
            }
        }

        if (this.GranularPermissions is not null)
        {
            this.GranularPermissions.RemovePlayerPermission(player.SteamID, permission);
            success = true;
        }

        if (success)
        {
            this.Logger.Info($"Removed permission {permission} from {player.Name}");
            return $"Removed permission {permission} from {player.Name}";
        }
        else
        {
            this.Logger.Error($"Could not remove permission {permission} from {player.Name}");
            return $"Could not remove permission {permission} from {player.Name}";
        }
    }

    [CommandCallback("clearperms", Description = "Clears all permissions and groups from a player", Permissions = new[] { "Permissions.Clear" })]
    public string ClearPermissionCommand(Context context, RunnerPlayer player)
    {
        if (this.GranularPermissions is not null)
        {
            foreach (string group in this.GranularPermissions.GetPlayerGroups(player.SteamID))
            {
                this.GranularPermissions.RemovePlayerGroup(player.SteamID, group);
            }

            foreach (string permission in this.GranularPermissions.GetPlayerPermissions(player.SteamID))
            {
                this.GranularPermissions.RemovePlayerPermission(player.SteamID, permission);
            }
        }

        if (this.PlayerPermissions is not null)
        {
            foreach (Roles role in Enum.GetValues<Roles>())
            {
                this.PlayerPermissions.RemovePlayerRoles(player.SteamID, role);
            }
        }

        this.Logger.Info($"Cleared permissions from {player.Name}");
        return $"Cleared permissions from {player.Name}";
    }

    [CommandCallback("listperms", Description = "Lists player permissions", Permissions = new[] { "Permissions.List" })]
    public string ListPermissionCommand(Context context, RunnerPlayer targetPlayer, int page = 1)
    {
        List<string> permissions = new();

        if (this.GranularPermissions is not null)
        {
            permissions.AddRange(this.GranularPermissions.GetPlayerPermissions(targetPlayer.SteamID));
            foreach (string group in this.GranularPermissions.GetPlayerGroups(targetPlayer.SteamID))
            {
                permissions.AddRange(((string[])this.GranularPermissions.GetGroupPermissions(group)).Select(p => $"{p} from {group}"));
            }
        }

        if (this.PlayerPermissions is not null)
        {
            Roles playerRoles = PlayerPermissions.Configuration.PlayerRoles.GetValueOrDefault(targetPlayer.SteamID);
            permissions.AddRange(Enum.GetValues<Roles>().Where(r => (r & playerRoles) > 0).Select(r => r.ToString()));
        }

        int pageCount = (int)Math.Ceiling(permissions.Count / (double)this.Configuration.PermissionsPerPage);

        return $"{targetPlayer.Name}: {string.Join("\n", permissions.Skip(page * this.Configuration.PermissionsPerPage).Take(this.Configuration.PermissionsPerPage))}{(pageCount > 1 ? $"{Environment.NewLine}Page {page} of {pageCount}{(page == pageCount ? "" : $", use listperms \"{targetPlayer.Name}\" {page + 1} to see more")}" : "")}";
    }
}

public class PermissionsCommandsConfiguration : ModuleConfiguration
{
    public int PermissionsPerPage { get; set; } = 6;
    public int MessageTimeout { get; set; } = 15;
}
