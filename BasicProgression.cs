using BattleBitAPI.Common;
using BBRAPIModules;
using System.IO;
using System.Threading.Tasks;

namespace BattleBitBaseModules;

/// <summary>
/// Author: @RainOrigami
/// Version: 0.4.7
/// </summary>

public class BasicProgression : BattleBitModule
{
    public BasicProgressionConfiguration Configuration { get; set; }

    private string dataDir => this.Configuration.PerServer ? Path.Combine(this.Configuration.DataDirectory, $"{this.Server.GameIP}:{this.Server.GamePort}") : this.Configuration.DataDirectory;

    public override Task OnConnected()
    {
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }

        return Task.CompletedTask;
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

    private string getPlayerFileName(ulong steamId)
    {
        return Path.Combine(dataDir, $"{steamId}.bin");
    }
}

public class BasicProgressionConfiguration : ModuleConfiguration
{
    public string DataDirectory { get; set; } = "./data/PersistentProgressionFiles";

    public bool PerServer { get; set; } = false;

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