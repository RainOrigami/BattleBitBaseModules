using BBRAPIModules;
using System.Threading.Tasks;

namespace BattleBitBaseModules;

[Module("Configure the loading screen text of your server", "1.0.0")]
public class LoadingScreenText : BattleBitModule
{
    public LoadingScreenTextConfiguration Configuration { get; set; } = null!;

    public override Task OnConnected()
    {
        this.Server.LoadingScreenText = this.Configuration.LoadingScreenText;

        return Task.CompletedTask;
    }
}

public class LoadingScreenTextConfiguration : ModuleConfiguration
{
    public string LoadingScreenText { get; set; } = "This is a community server!";
}