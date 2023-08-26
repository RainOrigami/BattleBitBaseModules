using BBRAPIModules;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BattleBitBaseModules;

/// <summary>
/// Author: @RainOrigami
/// Version: 0.4.5.1
/// </summary>

public class Rotation : BattleBitModule
{
    public RotationConfiguration Configuration { get; set; }

    public override Task OnConnected()
    {
        this.Server.GamemodeRotation.SetRotation(this.Configuration.GameModes.ToArray());
        this.Server.MapRotation.SetRotation(this.Configuration.Maps.ToArray());

        return Task.CompletedTask;
    }
}

public class RotationConfiguration : ModuleConfiguration
{
    public List<string> GameModes { get; set; } = new()
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
    public List<string> Maps { get; set; } = new()
    {
        "Azagor",
        "Barsa",
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
