using BattleBitAPI.Common;
using BBRAPIModules;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BattleBitBaseModules;

[Module("Provide basic persistent progression for players", "1.1.0")]
public class BasicProgression : BattleBitModule
{
    public BasicProgressionConfiguration Configuration { get; set; } = null!;

    private string dataDir => this.Configuration.PerServer ? Path.Combine(this.Configuration.DataDirectory, $"{this.Server.GameIP}:{this.Server.GamePort}") : this.Configuration.DataDirectory;

    public override Task OnConnected()
    {
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }

        return Task.CompletedTask;
    }

    public override async Task OnPlayerJoiningToServer(ulong steamID, PlayerJoiningArguments args)
    {
        if (this.Configuration.ApplyInitialStatsOnEveryJoin)
        {
            args.Stats = this.Configuration.InitialStats ?? args.Stats;
            return;
        }

        string playerFileName = getPlayerFileName(steamID);
        for (int i = 0; i < 5; i++)
        {
            try
            {
                args.Stats = File.Exists(playerFileName) ? new PlayerStats(File.ReadAllBytes(playerFileName)) : (this.Configuration.InitialStats ?? args.Stats);
                return;
            }
            catch (Exception ex)
            {
                this.Logger.Error($"Tried {i} times to read from file {playerFileName} but failed:{ex}");
            }
            await Task.Delay(250);
        }
        this.Logger.Error("Giving up trying to read.");
    }

    public override async Task OnSavePlayerStats(ulong steamID, PlayerStats stats)
    {
        for (int i = 0; i < 5; i++)
        {
            try
            {
                File.WriteAllBytes(getPlayerFileName(steamID), stats.SerializeToByteArray());
                return;
            }
            catch (Exception ex)
            {
                this.Logger.Error($"Tried {i} times to write to file {getPlayerFileName(steamID)} but failed:{ex}");
            }
            await Task.Delay(250);
        }
        this.Logger.Error("Giving up trying to save.");
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