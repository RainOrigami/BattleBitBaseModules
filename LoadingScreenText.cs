using BattleBitAPI.Common;
using BBRAPIModules;
using Bluscream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BattleBitBaseModules;

[RequireModule(typeof(Bluscream.BluscreamLib))]
[RequireModule(typeof(BattleBitBaseModules.RichText))]
[Module("Configure the loading screen text of your server", "1.0.0")]
public class LoadingScreenText : BattleBitModule
{
    [ModuleReference]
    public LoadingScreenTextConfiguration Config { get; set; } = null!;

    public override Task OnConnected() {
        UpdateLoadingScreenText(); return Task.CompletedTask;
    }

    public override Task OnPlayerJoiningToServer(ulong steamID, PlayerJoiningArguments args) {
        UpdateLoadingScreenText(); return Task.CompletedTask;
    }

    public override Task OnGameStateChanged(GameState oldState, GameState newState) {
        UpdateLoadingScreenText(); return Task.CompletedTask;
    }

    public void UpdateLoadingScreenText() {
        var newText = this.Config.LoadingScreenText
            .Replace("{server}", this.Server.ServerName)
            .Replace("{gamemode}", this.Server.GetCurrentGameMode().DisplayName)
            .Replace("{map}", this.Server.GetCurrentMap().DisplayName)
            .Replace("{maptime}", this.Server.DayNight.ToString().ToTitleCase())
            .Replace("{players}", this.Server.AllPlayers.Count().ToString())
            .Replace("{slots}", this.Server.MaxPlayerCount.ToString());

        foreach (var replacement in Config.randomReplacements) {
            newText = newText.Replace($"{{random.{replacement.Key}}}", replacement.Value[Random.Shared.Next(replacement.Value.Length)]);
        }

        this.Server.LoadingScreenText = newText;
    }

    public class LoadingScreenTextConfiguration : ModuleConfiguration {
        public Dictionary<string, string[]> randomReplacements = new Dictionary<string, string[]>() {
            { "welcome", new string[] { "Enjoy your stay!", "Have a good one!", "Get Ready for battle!" } },
        };
        public string LoadingScreenText { get; set; } =
@$"{Colors.SkyBlue}Welcome to {Colors.None}{{server}}{Colors.SkyBlue}!
We are currently playing {Colors.None}{{gamemode}}{Colors.SkyBlue} on {Colors.None}{{map}}{Colors.SkyBlue} at {Colors.None}{{maptime}}{Colors.SkyBlue} with {Colors.None}{{players}}{Colors.SkyBlue}/{Colors.None}{{slots}}{Colors.SkyBlue} players!
{{random.welcome}}";
    }
}
