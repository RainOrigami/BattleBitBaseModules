using BattleBitAPI.Common;
using BBRAPIModules;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Bluscream;
using System.Text.Json.Serialization;
using System.ComponentModel.Design;

namespace BattleBitBaseModules {

    [RequireModule(typeof(BluscreamLib))]
    [Module("Provide basic persistent progression for players", "1.1.0")]
    public class BasicProgression : BattleBitModule {
        [ModuleReference]
        public BluscreamLib BluscreamLib { get; set; } = null!;

        public Configuration GlobalConfiguration { get; set; } = null!;

        public DirectoryInfo DataDir => new DirectoryInfo(GlobalConfiguration.PerServer ? Path.Combine(GlobalConfiguration.DataDirectory, $"{this.Server.GameIP}:{this.Server.GamePort}") : GlobalConfiguration.DataDirectory);

        public override Task OnConnected() {
            if (!DataDir.Exists) {
                DataDir.Create();
            }

            return Task.CompletedTask;
        }

        public override async Task OnPlayerJoiningToServer(ulong steamID, PlayerJoiningArguments args) {
            Console.WriteLine("OnPlayerJoiningToServer");
            if (GlobalConfiguration.ApplyInitialStatsOnEveryJoin) {
                this.Logger.Info(args.Stats.str());
                this.Logger.Info(args.Stats.ToJson());
                args.Stats = GlobalConfiguration.InitialStats ?? args.Stats;
                this.Logger.Info($"Applied initial player stats for {steamID}");
                this.Logger.Info(args.Stats.str());
                this.Logger.Info(args.Stats.ToJson());
                args.Stats.Progress.Rank = 201;
                this.Logger.Info(args.Stats.str());
                this.Logger.Info(args.Stats.ToJson());
                return;
            }

            var playerFile = getPlayerFile(steamID);
            for (int i = 0; i < 5; i++) {
                try {
                    if (playerFile.Exists) {
                        args.Stats = GlobalConfiguration.PlainText ? JsonUtils.FromJsonFile<PlayerStats>(playerFile) : new PlayerStats(File.ReadAllBytes(playerFile.FullName));
                        this.Logger.Info($"Read player stats for {steamID} from \"{playerFile.FullName}\"");
                    } else {
                        args.Stats = (GlobalConfiguration.InitialStats ?? args.Stats);
                        this.Logger.Info($"Applied initial player stats for {steamID}");
                    }
                    return;
                } catch (Exception ex) {
                    this.Logger.Error($"Tried {i} times to read from file {playerFile.FullName} but failed:{ex}");
                }
                await Task.Delay(250);
            }
            this.Logger.Error($"Giving up trying to read \"{playerFile.FullName}\"");
        }

        public override async Task OnSavePlayerStats(ulong steamID, PlayerStats stats) {
            Console.WriteLine("OnSavePlayerStats");
            this.Logger.Info(stats.str());
            this.Logger.Info(stats.ToJson());
            for (int i = 0; i < 5; i++) {
                try {
                    var playerFile = getPlayerFile(steamID);
                    if (GlobalConfiguration.PlainText) {
                        stats.ToJsonFile(playerFile, indented: true);
                    } else {
                        playerFile.WriteAllBytes(stats.SerializeToByteArray());
                    }
                    this.Logger.Info($"Wrote player stats for {steamID} to \"{playerFile}\"");
                    return;
                } catch (Exception ex) {
                    this.Logger.Error($"Tried {i} times to write to file {getPlayerFile(steamID)} but failed:{ex}");
                }
                await Task.Delay(250);
            }
            this.Logger.Error("Giving up trying to save.");
        }

        private FileInfo getPlayerFile(ulong steamId) {
            return new FileInfo(Path.Combine(DataDir.FullName, $"{steamId}.bin"));
        }

        public class Configuration : ModuleConfiguration {
            public string DataDirectory { get; set; } = "./data/PersistentProgressionFiles";

            public bool PlainText { get; set; } = false;

            public bool PerServer { get; set; } = false;

            public bool ApplyInitialStatsOnEveryJoin { get; set; } = false;

            public PlayerStats InitialStats = new PlayerStats() {
                Achievements = new byte[0],
                IsBanned = false,
                Progress = new PlayerStats.PlayerProgess(),
                Roles = Roles.None,
                Selections = new byte[0],
                ToolProgress = new byte[0]
            };
        }
    }
}