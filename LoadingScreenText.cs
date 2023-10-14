using BattleBitAPI.Common;
using BBRAPIModules;
using Bluscream;
using System.Linq;
using System.Threading.Tasks;

namespace BattleBitBaseModules;

[RequireModule(typeof(Bluscream.BluscreamLib))]
[RequireModule(typeof(BattleBitBaseModules.RichText))]
[Module("Configure the loading screen text of your server", "1.0.0")]
public class LoadingScreenText : BattleBitModule
{
    [ModuleReference]
    public LoadingScreenTextConfiguration Configuration { get; set; } = null!;

    public override Task OnConnected() {
        UpdateLoadingScreenText(); return Task.CompletedTask;
    }

    public override Task OnPlayerJoiningToServer(ulong steamID, PlayerJoiningArguments args) {
        UpdateLoadingScreenText(); return Task.CompletedTask;
    }

    public void UpdateLoadingScreenText() {
        this.Server.LoadingScreenText = this.Configuration.LoadingScreenText
            .Replace("{server}", this.Server.ServerName)
            .Replace("{gamemode}", this.Server.Gamemode.ToTitleCase())
            .Replace("{map}", this.Server.Map.ToTitleCase())
            .Replace("{players}", this.Server.AllPlayers.Count().ToString())
            .Replace("{slots}", this.Server.MaxPlayerCount.ToString());
    }

    public class LoadingScreenTextConfiguration : ModuleConfiguration {
        public string LoadingScreenText { get; set; } = "<#87CEEB>Welcome to <color=\"white\">{server}<#87CEEB>!<br>We are currently playing <color=\"white\">{gamemode}<#87CEEB> on <color=\"white\">{map}<#87CEEB> with <color=\"white\">{players}<#87CEEB>/<color=\"white\">{slots}<#87CEEB> players!<br>Enjoy your stay!";
    }
}
