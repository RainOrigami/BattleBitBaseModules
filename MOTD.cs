using BattleBitAPI.Common;
using BBRAPIModules;
using Commands;
using System.Threading.Tasks;

namespace BattleBitBaseModules;

[RequireModule(typeof(CommandHandler))]
public class MOTD : BattleBitModule
{
    public MOTDConfiguration Configuration { get; set; }

    [ModuleReference]
    public CommandHandler CommandHandler { get; set; }
    
    public override void OnModulesLoaded()
    {
        this.CommandHandler.Register(this);
    }

    public override Task OnPlayerConnected(RunnerPlayer player)
    {
        this.ShowMOTD(player);
        return Task.CompletedTask;
    }

    [CommandCallback("setmotd", Description = "Sets the MOTD", AllowedRoles = Roles.Admin)]
    public void SetMOTD(RunnerPlayer commandSource, string motd)
    {
        this.Configuration.MOTD = motd;
        this.Configuration.Save();
        this.ShowMOTD(commandSource);
    }

    [CommandCallback("motd", Description = "Shows the MOTD")]
    public void ShowMOTD(RunnerPlayer commandSource)
    {
        commandSource.Message(string.Format(this.Configuration.MOTD, commandSource.Name, commandSource.PingMs, this.Server.ServerName, this.Server.Gamemode, this.Server.Map, this.Server.DayNight, this.Server.MapSize.ToString().Trim('_'), this.Server.CurrentPlayerCount, this.Server.InQueuePlayerCount, this.Server.MaxPlayerCount));
    }
}

public class MOTDConfiguration : ModuleConfiguration
{
    public string MOTD { get; set; } = "Welcome!";
}
