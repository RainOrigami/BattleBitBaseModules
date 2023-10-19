using BattleBitAPI.Common;
using BBRAPIModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BattleBitBaseModules;

[Module("Allow only specific Roles to spectate", "1.1.1")]
public class SpectateControl : BattleBitModule {
    public static SpectateControlConfiguration Configuration { get; set; } = null!;

    [ModuleReference]
    public dynamic? GranularPermissions { get; set; }

    [ModuleReference]
    public dynamic? PlayerPermissions { get; set; }

    public override void OnModulesLoaded() {
        if (this.GranularPermissions is null) {
            if (this.PlayerPermissions is null) {
                this.Logger.Error("GranularPermissions or PlayerPermissions not found, unloading module");
                this.Unload();
                return;
            }

            this.Logger.Info("PlayerPermissions found, using roles");

            foreach (string permission in Configuration.SpectatorPermissions) {
                if (!Enum.TryParse(permission, out Roles role)) {
                    this.Logger.Error($"Invalid role {permission}");
                }
            }
        }
    }

    public override Task OnPlayerConnected(RunnerPlayer player) {
        if (this.GranularPermissions is null) {
            foreach (string permission in Configuration.SpectatorPermissions) {
                if (!Enum.TryParse(permission, out Roles role)) {
                    this.Logger.Error($"Invalid role {permission}");
                } else {
                    if (this.PlayerPermissions!.HasPlayerRole(player.SteamID, role)) {
                        player.Modifications.CanSpectate = true;
                        this.Logger.Info($"Player {player.Name} ({player.SteamID}) has role {role} and can spectate");
                        return Task.CompletedTask;
                    }
                }
            }
        } else {
            player.Modifications.CanSpectate = Configuration.SpectatorPermissions.Any(p => this.GranularPermissions.HasPermission(player.SteamID, p));
        }

        return Task.CompletedTask;
    }
}

public class SpectateControlConfiguration : ModuleConfiguration {
    public List<string> SpectatorPermissions { get; set; } = new();
}
