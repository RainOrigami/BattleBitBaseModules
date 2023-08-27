using BattleBitAPI.Common;
using BBRAPIModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleBitBaseModules;

/// <summary>
/// Author: @RainOrigami
/// Version: 0.4.7
/// </summary>
/// 
public class BasicServerSettings : BattleBitModule
{
    public BasicServerSettingsConfiguration Configuration { get; set; }

    public override Task OnConnected()
    {
        this.applyServerSettings();

        return Task.CompletedTask;
    }

    public override Task OnGameStateChanged(GameState oldState, GameState newState)
    {
        if (oldState == newState)
        {
            return Task.CompletedTask;
        }

        this.applyRoundSettings(newState);

        return Task.CompletedTask;
    }

    private void applyServerSettings()
    {
        this.Server.ServerSettings.APCSpawnDelayMultipler = this.Configuration.APCSpawnDelayMultipler ?? this.Server.ServerSettings.APCSpawnDelayMultipler;
        this.Server.ServerSettings.CanVoteDay = this.Configuration.CanVoteDay ?? this.Server.ServerSettings.CanVoteDay;
        this.Server.ServerSettings.CanVoteNight = this.Configuration.CanVoteNight ?? this.Server.ServerSettings.CanVoteNight;
        this.Server.ServerSettings.DamageMultiplier = this.Configuration.DamageMultiplier ?? this.Server.ServerSettings.DamageMultiplier;
        this.Server.ServerSettings.EngineerLimitPerSquad = this.Configuration.EngineerLimitPerSquad ?? this.Server.ServerSettings.EngineerLimitPerSquad;
        this.Server.ServerSettings.FriendlyFireEnabled = this.Configuration.FriendlyFireEnabled ?? this.Server.ServerSettings.FriendlyFireEnabled;
        this.Server.ServerSettings.HelicopterSpawnDelayMultipler = this.Configuration.HelicopterSpawnDelayMultipler ?? this.Server.ServerSettings.HelicopterSpawnDelayMultipler;
        this.Server.ServerSettings.MedicLimitPerSquad = this.Configuration.MedicLimitPerSquad ?? this.Server.ServerSettings.MedicLimitPerSquad;
        this.Server.ServerSettings.OnlyWinnerTeamCanVote = this.Configuration.OnlyWinnerTeamCanVote ?? this.Server.ServerSettings.OnlyWinnerTeamCanVote;
        this.Server.ServerSettings.PlayerCollision = this.Configuration.PlayerCollision ?? this.Server.ServerSettings.PlayerCollision;
        this.Server.ServerSettings.ReconLimitPerSquad = this.Configuration.ReconLimitPerSquad ?? this.Server.ServerSettings.ReconLimitPerSquad;
        this.Server.ServerSettings.SeaVehicleSpawnDelayMultipler = this.Configuration.SeaVehicleSpawnDelayMultipler ?? this.Server.ServerSettings.SeaVehicleSpawnDelayMultipler;
        this.Server.ServerSettings.SupportLimitPerSquad = this.Configuration.SupportLimitPerSquad ?? this.Server.ServerSettings.SupportLimitPerSquad;
        this.Server.ServerSettings.TankSpawnDelayMultipler = this.Configuration.TankSpawnDelayMultipler ?? this.Server.ServerSettings.TankSpawnDelayMultipler;
        this.Server.ServerSettings.TransportSpawnDelayMultipler = this.Configuration.TransportSpawnDelayMultipler ?? this.Server.ServerSettings.TransportSpawnDelayMultipler;
        this.Server.ServerSettings.UnlockAllAttachments = this.Configuration.UnlockAllAttachments ?? this.Server.ServerSettings.UnlockAllAttachments;
    }

    private void applyRoundSettings(GameState gameState)
    {
        if (!this.Configuration.RoundSettings.ContainsKey(gameState))
        {
            return;
        }

        var roundSettings = this.Configuration.RoundSettings[gameState];

        this.Server.RoundSettings.MaxTickets = roundSettings.MaxTickets ?? this.Server.RoundSettings.MaxTickets;
        this.Server.RoundSettings.PlayersToStart = roundSettings.PlayersToStart ?? this.Server.RoundSettings.PlayersToStart;
        this.Server.RoundSettings.SecondsLeft = roundSettings.SecondsLeft ?? this.Server.RoundSettings.SecondsLeft;
        this.Server.RoundSettings.TeamATickets = roundSettings.TeamATickets ?? this.Server.RoundSettings.TeamATickets;
        this.Server.RoundSettings.TeamBTickets = roundSettings.TeamBTickets ?? this.Server.RoundSettings.TeamBTickets;
    }
}
public class BasicServerSettingsConfiguration : ModuleConfiguration
{
    public float? APCSpawnDelayMultipler { get; set; } = null;
    public float? HelicopterSpawnDelayMultipler { get; set; } = null;
    public float? SeaVehicleSpawnDelayMultipler { get; set; } = null;
    public float? TankSpawnDelayMultipler { get; set; } = null;
    public float? TransportSpawnDelayMultipler { get; set; } = null;
    public bool? CanVoteDay { get; set; } = null;
    public bool? CanVoteNight { get; set; } = null;
    public float? DamageMultiplier { get; set; } = null;
    public byte? EngineerLimitPerSquad { get; set; } = null;
    public byte? MedicLimitPerSquad { get; set; } = null;
    public byte? ReconLimitPerSquad { get; set; } = null;
    public byte? SupportLimitPerSquad { get; set; } = null;
    public bool? FriendlyFireEnabled { get; set; } = null;
    public bool? OnlyWinnerTeamCanVote { get; set; } = null;
    public bool? PlayerCollision { get; set; } = null;
    public bool? UnlockAllAttachments { get; set; } = null;
    public Dictionary<GameState, RoundSettingsConfiguration> RoundSettings = new()
    {
        { GameState.WaitingForPlayers, new(){ PlayersToStart = 1} },
        { GameState.CountingDown, new(){ SecondsLeft = 5 } },
        { GameState.Playing, new(){ MaxTickets= 5000, SecondsLeft= 1800, TeamATickets= 3000, TeamBTickets= 3000 } },
        { GameState.EndingGame, new() }
    };
}

public class RoundSettingsConfiguration
{
    public double? MaxTickets { get; set; } = null;
    public int? PlayersToStart { get; set; } = null;
    public int? SecondsLeft { get; set; } = null;
    public double? TeamATickets { get; set; } = null;
    public double? TeamBTickets { get; set; } = null;
}