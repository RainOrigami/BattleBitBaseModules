using BBRAPIModules;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BattleBitBaseModules;

[Module("Configure the map and game mode rotation of the server", "1.0.0")]
public class Rotation : BattleBitModule
{
    public RotationConfiguration Configuration { get; set; }

    public override Task OnConnected()
    {
        this.Logger.Info($"Setting up game mode rotation to {string.Join(", ", this.Configuration.GameModes)}");
        this.Server.GamemodeRotation.SetRotation(this.Configuration.GameModes);
        this.Logger.Debug($"New game mode rotation: {string.Join(", ", this.Server.GamemodeRotation.GetGamemodeRotation())}");
        this.Logger.Info($"Setting up map rotation to {string.Join(", ", this.Configuration.Maps)}");
        this.Server.MapRotation.SetRotation(this.Configuration.Maps);
        this.Logger.Debug($"New map rotation: {string.Join(", ", this.Server.MapRotation.GetMapRotation())}");

        return Task.CompletedTask;
    }
}

public class RotationConfiguration : ModuleConfiguration
{
    public string[] GameModes { get; set; } = new[]
    {
        "TDM",
        "AAS",
        "RUSH",
        "CONQ",
        "DOMI",
        "ELI",
        "INFCONQ",
        "FRONTLINE",
        "GunGameFFA",
        "FFA",
        "GunGameTeam",
        "SuicideRush",
        "CatchGame",
        "Infected",
        "CashRun",
        "VoxelFortify",
        "VoxelTrench",
        "CaptureTheFlag"
    };
    public string[] Maps { get; set; } = new[]
    {
        "Azagor",
        "Basra",
        "Construction",
        "District",
        "Dustydew",
        "Eduardovo",
        "Frugis",
        "Isle",
        "Lonovo",
        "MultuIslands",
        "Namak",
        "OilDunes",
        "River",
        "Salhan",
        "SandySunset",
        "TensaTown",
        "Valley",
        "Wakistan",
        "WineParadise"
    };
}
