using BattleBitAPI.Common;
using BBRAPIModules;
using System;
using System.IO;
using System.Threading.Tasks;
using static BattleBitAPI.Common.PlayerStats;

namespace BattleBitBaseModules;

[Module("Provide basic persistent progression for players", "1.1.1")]
public class BasicProgression : BattleBitModule {
    public BasicProgressionConfiguration Configuration { get; set; } = null!;

    private string dataDir => this.Configuration.PerServer ? Path.Combine(this.Configuration.DataDirectory, $"{this.Server.GameIP}:{this.Server.GamePort}") : this.Configuration.DataDirectory;

    public override Task OnConnected() {
        if (!Directory.Exists(dataDir)) {
            Directory.CreateDirectory(dataDir);
        }

        return Task.CompletedTask;
    }

    public override async Task OnPlayerJoiningToServer(ulong steamID, PlayerJoiningArguments args) {
        if (this.Configuration.ApplyInitialStatsOnEveryJoin) {
            args.Stats = this.Configuration.InitialStats?.ToPlayerStats() ?? args.Stats;
            return;
        }

        string playerFileName = getPlayerFileName(steamID);
        for (int i = 0; i < 5; i++) {
            try {
                args.Stats = File.Exists(playerFileName) ? new PlayerStats(File.ReadAllBytes(playerFileName)) : (this.Configuration.InitialStats?.ToPlayerStats() ?? args.Stats);
                return;
            } catch (Exception ex) {
                this.Logger.Error($"Tried {i} times to read from file {playerFileName} but failed:{ex}");
            }
            await Task.Delay(250);
        }
        this.Logger.Error("Giving up trying to read.");
    }

    public override async Task OnSavePlayerStats(ulong steamID, PlayerStats stats) {
        for (int i = 0; i < 5; i++) {
            try {
                File.WriteAllBytes(getPlayerFileName(steamID), stats.SerializeToByteArray());
                return;
            } catch (Exception ex) {
                this.Logger.Error($"Tried {i} times to write to file {getPlayerFileName(steamID)} but failed:{ex}");
            }
            await Task.Delay(250);
        }
        this.Logger.Error("Giving up trying to save.");
    }

    private string getPlayerFileName(ulong steamId) {
        return Path.Combine(dataDir, $"{steamId}.bin");
    }
}

public class BasicProgressionConfiguration : ModuleConfiguration {
    public string DataDirectory { get; set; } = "./data/PersistentProgressionFiles";

    public bool PerServer { get; set; } = false;

    public bool ApplyInitialStatsOnEveryJoin { get; set; } = false;

    public BasicPlayerStats? InitialStats { get; set; } = new BasicPlayerStats() {
        Achievements = new byte[0],
        IsBanned = false,
        Progress = new BasicPlayerProgress(),
        Roles = Roles.None,
        Selections = new byte[0],
        ToolProgress = new byte[0]
    };
}

public class BasicPlayerStats {
    public bool IsBanned { get; set; }

    public Roles Roles { get; set; }

    public BasicPlayerProgress Progress { get; set; } = new();

    public byte[] ToolProgress { get; set; } = Array.Empty<byte>();

    public byte[] Achievements { get; set; } = Array.Empty<byte>();

    public byte[] Selections { get; set; } = Array.Empty<byte>();

    public PlayerStats ToPlayerStats() {
        return new() {
            IsBanned = this.IsBanned,
            Roles = this.Roles,
            Progress = this.Progress.ToPlayerProgress(),
            ToolProgress = this.ToolProgress,
            Achievements = this.Achievements,
            Selections = this.Selections
        };
    }
}

public class BasicPlayerProgress {
    public uint KillCount { get; set; }

    public uint LeaderKills { get; set; }

    public uint AssaultKills { get; set; }

    public uint MedicKills { get; set; }

    public uint EngineerKills { get; set; }

    public uint SupportKills { get; set; }

    public uint ReconKills { get; set; }

    public uint DeathCount { get; set; }

    public uint WinCount { get; set; }

    public uint LoseCount { get; set; }

    public uint FriendlyShots { get; set; }

    public uint FriendlyKills { get; set; }

    public uint Revived { get; set; }

    public uint RevivedTeamMates { get; set; }

    public uint Assists { get; set; }

    public uint Prestige { get; set; }

    public uint Rank { get; set; }

    public uint EXP { get; set; }

    public uint ShotsFired { get; set; }

    public uint ShotsHit { get; set; }

    public uint Headshots { get; set; }

    public uint ObjectivesComplated { get; set; }

    public uint HealedHPs { get; set; }

    public uint RoadKills { get; set; }

    public uint Suicides { get; set; }

    public uint VehiclesDestroyed { get; set; }

    public uint VehicleHPRepaired { get; set; }

    public uint LongestKill { get; set; }

    public uint PlayTimeSeconds { get; set; }

    public uint LeaderPlayTime { get; set; }

    public uint AssaultPlayTime { get; set; }

    public uint MedicPlayTime { get; set; }

    public uint EngineerPlayTime { get; set; }

    public uint SupportPlayTime { get; set; }

    public uint ReconPlayTime { get; set; }

    public uint LeaderScore { get; set; }

    public uint AssaultScore { get; set; }

    public uint MedicScore { get; set; }

    public uint EngineerScore { get; set; }

    public uint SupportScore { get; set; }

    public uint ReconScore { get; set; }

    public uint TotalScore { get; set; }

    public PlayerProgess ToPlayerProgress() {
        return new() {
            KillCount = this.KillCount,
            LeaderKills = this.LeaderKills,
            AssaultKills = this.AssaultKills,
            MedicKills = this.MedicKills,
            EngineerKills = this.EngineerKills,
            SupportKills = this.SupportKills,
            ReconKills = this.ReconKills,
            DeathCount = this.DeathCount,
            WinCount = this.WinCount,
            LoseCount = this.LoseCount,
            FriendlyShots = this.FriendlyShots,
            FriendlyKills = this.FriendlyKills,
            Revived = this.Revived,
            RevivedTeamMates = this.RevivedTeamMates,
            Assists = this.Assists,
            Prestige = this.Prestige,
            Rank = this.Rank,
            EXP = this.EXP,
            ShotsFired = this.ShotsFired,
            ShotsHit = this.ShotsHit,
            Headshots = this.Headshots,
            ObjectivesComplated = this.ObjectivesComplated,
            HealedHPs = this.HealedHPs,
            RoadKills = this.RoadKills,
            Suicides = this.Suicides,
            VehiclesDestroyed = this.VehiclesDestroyed,
            VehicleHPRepaired = this.VehicleHPRepaired,
            LongestKill = this.LongestKill,
            PlayTimeSeconds = this.PlayTimeSeconds,
            LeaderPlayTime = this.LeaderPlayTime,
            AssaultPlayTime = this.AssaultPlayTime,
            MedicPlayTime = this.MedicPlayTime,
            EngineerPlayTime = this.EngineerPlayTime,
            SupportPlayTime = this.SupportPlayTime,
            ReconPlayTime = this.ReconPlayTime,
            LeaderScore = this.LeaderScore,
            AssaultScore = this.AssaultScore,
            MedicScore = this.MedicScore,
            EngineerScore = this.EngineerScore,
            SupportScore = this.SupportScore,
            ReconScore = this.ReconScore,
            TotalScore = this.TotalScore
        };
    }
}