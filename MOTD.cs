using BattleBitAPI.Common;
using BBRAPIModules;
using Commands;
using System.IO;
using System.Threading.Tasks;

namespace BattleBitBaseModules;

[RequireModule(typeof(CommandHandler))]
public class MOTD : BattleBitModule
{
    private static string? motd = null;

    [ModuleReference]
    public CommandHandler CommandHandler { get; set; }

    public MOTD(RunnerServer server) : base(server)
    {
        if (motd is not null)
        {
            return;
        }

        if (!File.Exists("motd.txt"))
        {
            File.WriteAllText("motd.txt", motd);
        }
        else
        {
            motd = File.ReadAllText("motd.txt");
        }
    }

    public override void OnModulesLoaded()
    {
        this.CommandHandler.Register(this);
    }

    public override Task OnPlayerConnected(RunnerPlayer player)
    {
        player.Message(motd);
        return Task.CompletedTask;
    }

    [CommandCallback("setmotd", Description = "Sets the MOTD", AllowedRoles = Roles.Admin)]
    public void SetMOTD(RunnerPlayer commandSource, string motd)
    {
        MOTD.motd = motd;
        File.WriteAllText("motd.txt", motd);
        commandSource.Message(motd);
    }

    [CommandCallback("motd", Description = "Shows the MOTD")]
    public void ShowMOTD(RunnerPlayer commandSource)
    {
        commandSource.Message(motd);
    }
}
