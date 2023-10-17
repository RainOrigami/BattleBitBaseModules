using BattleBitAPI.Common;
using BattleBitAPI.Features;
using BBRAPIModules;
using Commands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BattleBitBaseModules;

[RequireModule(typeof(CommandHandler))]
[Module("Show a message of the day to players who join the server", "1.1.0")]
public class MOTD : BattleBitModule
{
    public MOTDConfiguration Configuration { get; set; } = null!;

    [ModuleReference]
    public CommandHandler CommandHandler { get; set; } = null!;

    [ModuleReference]
    public dynamic? PlaceholderLib { get; set; }

    private List<ulong> greetedPlayers = new();

    public override void OnModulesLoaded()
    {
        if (this.PlaceholderLib is null)
        {
            this.Logger.Info("PlaceholderLib not found. MOTD will only support basic numbered placeholders.");
        }

        this.CommandHandler.Register(this);
    }

    public override Task OnGameStateChanged(GameState oldState, GameState newState)
    {
        if (newState == GameState.EndingGame)
        {
            greetedPlayers.Clear();
            greetedPlayers.AddRange(this.Server.AllPlayers.Select(p => p.SteamID));
        }

        return Task.CompletedTask;
    }

    public override Task OnPlayerConnected(RunnerPlayer player)
    {
        if (this.greetedPlayers.Contains(player.SteamID))
        {
            return Task.CompletedTask;
        }

        this.ShowMOTD(player);

        return Task.CompletedTask;
    }

    [CommandCallback("setmotd", Description = "Sets the MOTD", Permissions = new[] { "MOTD.Set" })]
    public void SetMOTD(RunnerPlayer commandSource, string motd)
    {
        this.Configuration.MOTD = motd;
        this.Configuration.Save();
        this.ShowMOTD(commandSource);
    }

    [CommandCallback("motd", Description = "Shows the MOTD")]
    public void ShowMOTD(RunnerPlayer commandSource)
    {
        string message;
        if (this.PlaceholderLib is not null)
        {
            message = new PlaceholderLib(this.Configuration.MOTD)
            .AddParam("servername", this.Server.ServerName)
            .AddParam("gamemode", this.Server.Gamemode)
            .AddParam("map", this.Server.Map)
            .AddParam("daynight", this.Server.DayNight)
            .AddParam("mapsize", this.Server.MapSize.ToString().Trim('_'))
            .AddParam("currentplayers", this.Server.CurrentPlayerCount)
            .AddParam("inqueueplayers", this.Server.InQueuePlayerCount)
            .AddParam("maxplayers", this.Server.MaxPlayerCount)
            .AddParam("name", commandSource.Name)
            .AddParam("ping", commandSource.PingMs).Run();
        }
        else
        {
            message = string.Format(this.Configuration.MOTD, commandSource.Name, commandSource.PingMs, this.Server.ServerName, this.Server.Gamemode, this.Server.Map, this.Server.DayNight, this.Server.MapSize.ToString().Trim('_'), this.Server.CurrentPlayerCount, this.Server.InQueuePlayerCount, this.Server.MaxPlayerCount);
        }

        commandSource.Message(message, this.Configuration.MessageTimeout);
    }
}

public class MOTDConfiguration : ModuleConfiguration
{
    public string MOTD { get; set; } = "Welcome!";
    public int MessageTimeout { get; set; } = 30;
}
