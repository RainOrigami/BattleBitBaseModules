using BattleBitAPI.Common;
using BBRAPIModules;
using Commands;
using Permissions;

namespace PermissionsManager
{
    [RequireModule(typeof(PlayerPermissions))]
    [RequireModule(typeof(CommandHandler))]
    public class PermissionsCommands : BattleBitModule
    {
        private PlayerPermissions playerPermissions = null!;

        public PermissionsCommands(RunnerServer server) : base(server)
        {
        }

        public override void OnModulesLoaded()
        {
            this.playerPermissions = this.Server.GetModule<PlayerPermissions>()!;
            Server.GetModule<CommandHandler>()!.Register(this);
        }

        [CommandCallback("addperm", Description = "Adds a permission to a player", AllowedRoles = Roles.Admin)]
        public void AddPermissionCommand(RunnerPlayer commandSource, RunnerPlayer player, Roles permission)
        {
            this.playerPermissions.AddPlayerRoles(player.SteamID, permission);
            commandSource.Message($"Added permission {permission} to {player.Name}");
        }

        [CommandCallback("removeperm", Description = "Removes a permission from a player", AllowedRoles = Roles.Admin)]
        public void RemovePermissionCommand(RunnerPlayer commandSource, RunnerPlayer player, Roles permission)
        {
            this.playerPermissions.RemovePlayerRoles(player.SteamID, permission);
            commandSource.Message($"Removed permission {permission} from {player.Name}");
        }
    }
}