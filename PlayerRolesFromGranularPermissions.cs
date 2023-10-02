using BattleBitAPI.Common;
using BBRAPIModules;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Permissions;

[RequireModule(typeof(GranularPermissions))]
[Module("Provides player roles based on granular permissions", "1.0.0")]
public class PlayerRolesFromGranularPermissions : BattleBitModule
{
	[ModuleReference]
	public GranularPermissions GranularPermissions { get; set; } = null!;

	public PlayerRolesConfiguration Configuration { get; set; } = null!;

	public override Task OnPlayerJoiningToServer(ulong steamID, PlayerJoiningArguments args)
	{
		foreach (string permission in Configuration.PermissionRoles.Keys)
		{
			if (!this.GranularPermissions.HasPermission(steamID, permission))
			{
				continue;
			}
			if (Configuration.AppendRoles)
			{
				args.Stats.Roles |= Configuration.PermissionRoles[permission];
			}
			else
			{
				args.Stats.Roles = Configuration.PermissionRoles[permission];
			}
		}

		return Task.CompletedTask;
	}
}

public class PlayerRolesConfiguration : ModuleConfiguration
{
	public Dictionary<string, Roles> PermissionRoles { get; set; } = new()
	{
		{ "Role.Admin", Roles.Admin },
		{ "Role.Moderator", Roles.Moderator },
		{ "Role.Modmin", Roles.Admin | Roles.Moderator }
	};

	public bool AppendRoles { get; set; } = false;
}
