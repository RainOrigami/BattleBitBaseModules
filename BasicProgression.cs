using BattleBitAPI.Common;
using BBRAPIModules;
using System.IO;
using System.Threading.Tasks;

namespace BattleBitBaseModules;

/// <summary>
/// Author: @RainOrigami
/// Version: 0.4.5.1
/// </summary>

public class BasicProgression : BattleBitModule
{
    private const string DATA_DIR = "./data/PersistentProgressionFiles";

    public BasicProgressionConfiguration Configuration { get; set; }

    public override void OnModulesLoaded()
    {
        if (!Directory.Exists(DATA_DIR))
        {
            Directory.CreateDirectory(DATA_DIR);
        }
    }

    public override Task OnPlayerJoiningToServer(ulong steamID, PlayerJoiningArguments args)
    {
        if (this.Configuration.ApplyInitialStatsOnEveryJoin)
        {
            args.Stats = this.Configuration.InitialStats ?? args.Stats;
            return Task.CompletedTask;
        }

        string playerFileName = getPlayerFileName(steamID);
        args.Stats = File.Exists(playerFileName) ? new PlayerStats(File.ReadAllBytes(playerFileName)) : (this.Configuration.InitialStats ?? args.Stats);

        return Task.CompletedTask;
    }

    public override Task OnSavePlayerStats(ulong steamID, PlayerStats stats)
    {
        File.WriteAllBytes(getPlayerFileName(steamID), stats.SerializeToByteArray());

        return Task.CompletedTask;
    }

    private static string getPlayerFileName(ulong steamId)
    {
        return Path.Combine(DATA_DIR, $"{steamId}.bin");
    }
}

public class BasicProgressionConfiguration : ModuleConfiguration
{
    public bool ApplyInitialStatsOnEveryJoin { get; set; } = false;

    public PlayerStats? InitialStats { get; set; } = new PlayerStats()
    {
        Achievements = new byte[0],
        IsBanned = false,
        Progress = new PlayerStats.PlayerProgess(),
        Roles = Roles.None,
        Selections = new byte[0],
        ToolProgress = new byte[0]
    };
}