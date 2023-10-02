using BBRAPIModules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Permissions;

[Module("Granular permissions for players and groups.", "1.0.0")]
public class GranularPermissions : BattleBitModule
{
    public const string CatchAll = "*";
    public const string RevokePrefix = "-";
    public const string PermissionSeparator = ".";

    public static PermissionsConfiguration Configuration { get; set; } = null!;
    public PermissionsServerConfiguration ServerConfiguration { get; set; } = null!;

    public void Save()
    {
        this.Logger.Debug("Saving permissions configuration.");
        this.ServerConfiguration.Save();
        Configuration.Save();
    }

    public bool HasPermission(ulong steamId, string permission)
    {
        this.Logger.Debug($"Checking if player {steamId} has permission {permission}.");

        if (permission == CatchAll)
        {
            this.Logger.Debug($"Player {steamId} has permission {permission} because it is a catch-all permission.");
            return true;
        }

        string[] playerGroups = this.ServerConfiguration.PlayerGroups.ContainsKey(steamId) ? this.ServerConfiguration.PlayerGroups[steamId].ToArray() : Array.Empty<string>();
        if (Configuration.Groups.ContainsKey(CatchAll))
        {
            playerGroups = playerGroups.Append(CatchAll).ToArray();
        }

        bool permitted = false;

        // Player permissions
        if (this.ServerConfiguration.PlayerPermissions.ContainsKey(steamId))
        {
            if (this.ServerConfiguration.PlayerPermissions[steamId].Any(p => p.Equals(RevokePrefix + permission, StringComparison.InvariantCultureIgnoreCase)))
            {
                this.Logger.Debug($"Player {steamId} does not have permission {permission} because they have revoked permission {permission}.");
                return false;
            }

            if (this.ServerConfiguration.PlayerPermissions[steamId].Any(p => p.Equals(permission, StringComparison.InvariantCultureIgnoreCase)))
            {
                this.Logger.Debug($"Player {steamId} has permission {permission} because they have permission {permission}.");
                permitted = true;
            }

            if (this.ServerConfiguration.PlayerPermissions[steamId].Any(p => p.Equals(CatchAll, StringComparison.InvariantCultureIgnoreCase)))
            {
                this.Logger.Debug($"Player {steamId} has permission {permission} because they have catch-all permission *.");
                permitted = true;
            }
        }

        // Group permissions
        foreach (string group in playerGroups)
        {
            if (!Configuration.Groups.ContainsKey(group))
            {
                this.Logger.Error($"Group {group} of player {steamId} does not exist.");
                continue;
            }

            if (Configuration.Groups[group].Contains("-*"))
            {
                this.Logger.Debug($"Player {steamId} does not have permission {permission} because group {group} has revoked all permissions.");
                return false;
            }

            if (Configuration.Groups[group].Any(p => p.Equals(RevokePrefix + permission, StringComparison.InvariantCultureIgnoreCase)))
            {
                this.Logger.Debug($"Player {steamId} does not have permission {permission} because group {group} has revoked permission {permission}.");
                return false;
            }

            if (Configuration.Groups[group].Any(p => p.Equals(permission, StringComparison.InvariantCultureIgnoreCase)))
            {
                permitted = true;
            }

            if (Configuration.Groups[group].Any(p => p.Equals(CatchAll, StringComparison.InvariantCultureIgnoreCase)))
            {
                this.Logger.Debug($"Player {steamId} has permission {permission} because group {group} has catch-all permission *.");
                permitted = true;
            }
        }

        // Partial permissions with catch-all
        string[] permissionPath = permission.Split(PermissionSeparator);
        string partialPermissionPath = string.Empty;
        foreach (string permissionPart in permissionPath)
        {
            partialPermissionPath += $"{permissionPart}{PermissionSeparator}";

            // Partial player permissions

            if (this.ServerConfiguration.PlayerPermissions.ContainsKey(steamId))
            {
                if (this.ServerConfiguration.PlayerPermissions[steamId].Any(p => p.Equals(RevokePrefix + partialPermissionPath + CatchAll, StringComparison.InvariantCultureIgnoreCase)))
                {
                    this.Logger.Debug($"Player {steamId} does not have permission {permission} because they have revoked permission {partialPermissionPath + CatchAll}.");
                    return false;
                }

                if (this.ServerConfiguration.PlayerPermissions[steamId].Any(p => p.Equals(partialPermissionPath + CatchAll, StringComparison.InvariantCultureIgnoreCase)))
                {
                    this.Logger.Debug($"Player {steamId} has permission {permission} because they have permission {partialPermissionPath + CatchAll}.");
                    permitted = true;
                }
            }

            // Partial group permissions

            foreach (string group in playerGroups)
            {
                if (!Configuration.Groups.ContainsKey(group))
                {
                    // We already logged this error above.
                    continue;
                }

                if (Configuration.Groups[group].Any(p => p.Equals(RevokePrefix + partialPermissionPath + CatchAll, StringComparison.InvariantCultureIgnoreCase)))
                {
                    this.Logger.Debug($"Player {steamId} does not have permission {permission} because group {group} has revoked permission {partialPermissionPath + CatchAll}.");
                    return false;
                }

                if (Configuration.Groups[group].Any(p => p.Equals(partialPermissionPath + CatchAll, StringComparison.InvariantCultureIgnoreCase)))
                {
                    this.Logger.Debug($"Player {steamId} has permission {permission} because group {group} has permission {partialPermissionPath + CatchAll}.");
                    permitted = true;
                }
            }
        }

        this.Logger.Debug($"Player {steamId} {(permitted ? "has" : "does not have")} permission {permission}.");

        return permitted;
    }

    public void AddGroup(string group)
    {
        this.Logger.Debug($"Adding group {group}.");

        if (Configuration.Groups.ContainsKey(group))
        {
            this.Logger.Error($"Group {group} already exists.");
            return;
        }

        Configuration.Groups.Add(group, new());
    }

    public void RemoveGroup(string group)
    {
        this.Logger.Debug($"Removing group {group}.");

        if (!Configuration.Groups.ContainsKey(group))
        {
            this.Logger.Error($"Group {group} does not exist.");
            return;
        }

        Configuration.Groups.Remove(group);
    }

    public string[] GetGroups()
    {
        return Configuration.Groups.Keys.ToArray();
    }

    public string[] GetPlayerGroups(ulong steamId)
    {
        if (!ServerConfiguration.PlayerGroups.ContainsKey(steamId))
        {
            if (Configuration.Groups.ContainsKey(CatchAll))
            {
                return new[] { CatchAll };
            }
            else
            {
                return Array.Empty<string>();
            }
        }

        return ServerConfiguration.PlayerGroups[steamId].ToArray();
    }

    public void AddGroupPermission(string group, string permission)
    {
        this.Logger.Debug($"Adding permission {permission} to group {group}.");

        if (!Configuration.Groups.ContainsKey(group))
        {
            this.Logger.Error($"Group {group} does not exist.");
            return;
        }

        if (Configuration.Groups[group].Contains(permission))
        {
            this.Logger.Error($"Group {group} already has permission {permission}.");
            return;
        }

        Configuration.Groups[group].Add(permission);
    }

    public void RemoveGroupPermission(string group, string permission)
    {
        this.Logger.Debug($"Removing permission {permission} from group {group}.");

        if (!Configuration.Groups.ContainsKey(group))
        {
            this.Logger.Error($"Group {group} does not exist.");
            return;
        }

        if (!Configuration.Groups[group].Contains(permission))
        {
            this.Logger.Error($"Group {group} does not have permission {permission}.");
            return;
        }

        Configuration.Groups[group].Remove(permission);
    }

    public void AddRevokedGroupPermission(string group, string permission)
    {
        this.Logger.Debug($"Adding revoked permission {permission} to group {group}.");

        this.AddGroupPermission(group, $"{RevokePrefix}{permission}");
    }

    public void RemoveRevokedGroupPermission(string group, string permission)
    {
        this.Logger.Debug($"Removing revoked permission {permission} from group {group}.");

        this.RemoveGroupPermission(group, $"{RevokePrefix}{permission}");
    }

    public string[] GetGroupPermissions(string group)
    {
        if (!Configuration.Groups.ContainsKey(group))
        {
            this.Logger.Error($"Group {group} does not exist.");
            return Array.Empty<string>();
        }

        return Configuration.Groups[group].ToArray();
    }

    public void AddPlayerGroup(ulong steamId, string group)
    {
        this.Logger.Debug($"Adding player {steamId} to group {group}.");

        if (!ServerConfiguration.PlayerGroups.ContainsKey(steamId))
        {
            this.Logger.Debug($"Player {steamId} does not have any groups.");
            ServerConfiguration.PlayerGroups.Add(steamId, new());
        }

        if (ServerConfiguration.PlayerGroups[steamId].Contains(group))
        {
            this.Logger.Error($"Player {steamId} already has group {group}.");
            return;
        }

        ServerConfiguration.PlayerGroups[steamId].Add(group);
    }

    public void RemovePlayerGroup(ulong steamId, string group)
    {
        this.Logger.Debug($"Removing player {steamId} from group {group}.");

        if (!ServerConfiguration.PlayerGroups.ContainsKey(steamId))
        {
            this.Logger.Error($"Player {steamId} does not have any groups.");
            return;
        }

        if (!ServerConfiguration.PlayerGroups[steamId].Contains(group))
        {
            this.Logger.Error($"Player {steamId} does not have group {group}.");
            return;
        }

        ServerConfiguration.PlayerGroups[steamId].Remove(group);
    }

    public void AddPlayerPermission(ulong steamId, string permission)
    {
        this.Logger.Debug($"Adding permission {permission} to player {steamId}.");

        if (!this.ServerConfiguration.PlayerPermissions.ContainsKey(steamId))
        {
            this.Logger.Debug($"Player {steamId} does not have any permissions.");
            this.ServerConfiguration.PlayerPermissions.Add(steamId, new());
        }

        if (this.ServerConfiguration.PlayerPermissions[steamId].Contains(permission))
        {
            this.Logger.Error($"Player {steamId} already has permission {permission}.");
            return;
        }

        this.ServerConfiguration.PlayerPermissions[steamId].Add(permission);
    }

    public void RemovePlayerPermission(ulong steamId, string permission)
    {
        this.Logger.Debug($"Removing permission {permission} from player {steamId}.");

        if (!this.ServerConfiguration.PlayerPermissions.ContainsKey(steamId))
        {
            this.Logger.Error($"Player {steamId} does not have any permissions.");
            return;
        }

        if (!this.ServerConfiguration.PlayerPermissions[steamId].Contains(permission))
        {
            this.Logger.Error($"Player {steamId} does not have permission {permission}.");
            return;
        }

        this.ServerConfiguration.PlayerPermissions[steamId].Remove(permission);
    }

    public void AddRevokedPlayerPermission(ulong steamId, string permission)
    {
        this.Logger.Debug($"Adding revoked permission {permission} to player {steamId}.");

        this.AddPlayerPermission(steamId, $"{RevokePrefix}{permission}");
    }

    public void RemoveRevokedPlayerPermission(ulong steamId, string permission)
    {
        this.Logger.Debug($"Removing revoked permission {permission} from player {steamId}.");

        this.RemovePlayerPermission(steamId, $"{RevokePrefix}{permission}");
    }

    public string[] GetPlayerPermissions(ulong steamId)
    {
        if (!this.ServerConfiguration.PlayerPermissions.ContainsKey(steamId))
        {
            this.Logger.Debug($"Player {steamId} does not have any player-specific permissions.");
            return Array.Empty<string>();
        }

        return this.ServerConfiguration.PlayerPermissions[steamId].ToArray();
    }

    public string[] GetAllPlayerPermissions(ulong steamId)
    {
        List<string> permissions = new();

        if (this.ServerConfiguration.PlayerPermissions.ContainsKey(steamId))
        {
            permissions.AddRange(this.ServerConfiguration.PlayerPermissions[steamId]);
        }

        foreach (string group in this.GetPlayerGroups(steamId))
        {
            permissions.AddRange(this.GetGroupPermissions(group));
        }

        return permissions.Distinct().ToArray();
    }
}

public class PermissionsConfiguration : ModuleConfiguration
{
    public Dictionary<string, List<string>> Groups { get; set; } = new() {
        { GranularPermissions.CatchAll, new() }
    };
}

public class PermissionsServerConfiguration : ModuleConfiguration
{
    public Dictionary<ulong, List<string>> PlayerGroups { get; set; } = new();
    public Dictionary<ulong, List<string>> PlayerPermissions { get; set; } = new();
}
