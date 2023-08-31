using BattleBitAPI.Common;
using BBRAPIModules;
using Commands;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BattleBitBaseModules;

/// <summary>
/// Author: @RainOrigami
/// Version: 0.4.5.1
/// </summary>

[RequireModule(typeof(CommandHandler))]
public class Allowlist : BattleBitModule
{
    public CommandHandler CommandHandler { get; set; }
    public AllowlistConfiguration AllowedPlayers { get; set; }

    public override Task OnPlayerJoiningToServer(ulong steamID, PlayerJoiningArguments args)
    {
        //if (!this.AllowedPlayers.AllowedPlayers.Contains(steamID))
        //{
        //    this.Server.Kick(steamID, "You are not allowed to join this server.");
        //}

        // TODO: requires testing if it works like this
        args.Stats.IsBanned = !this.AllowedPlayers.AllowedPlayers.Contains(steamID);

        return Task.CompletedTask;
    }

    //public override Task OnPlayerConnected(RunnerPlayer player)
    //{
    //    if (!this.AllowedPlayers.AllowedPlayers.Contains(player.SteamID))
    //    {
    //        player.Kick("You are not allowed to join this server.");
    //    }

    //    return Task.CompletedTask;
    //}

    [CommandCallback("allow add", Description = "Adds a player to the allowlist", AllowedRoles = Roles.Moderator)]
    public void AllowAdd(RunnerPlayer commandSource, ulong steamID)
    {
        if (this.AllowedPlayers.AllowedPlayers.Contains(steamID))
        {
            commandSource.Message("This player is already allowed to join this server.", 10);
            return;
        }

        this.AllowedPlayers.AllowedPlayers.Add(steamID);
        this.AllowedPlayers.Save();

        commandSource.Message("Player added to the allowlist.", 10);
    }

    [CommandCallback("allow remove", Description = "Removes a player from the allowlist", AllowedRoles = Roles.Moderator)]
    public void AllowRemove(RunnerPlayer commandSource, ulong steamID)
    {
        if (!this.AllowedPlayers.AllowedPlayers.Contains(steamID))
        {
            commandSource.Message("This player is already not allowed to join this server.", 10);
            return;
        }

        this.AllowedPlayers.AllowedPlayers.Remove(steamID);
        this.AllowedPlayers.Save();

        commandSource.Message("Player removed from the allowlist.", 10);
    }
}

public class AllowlistConfiguration : ModuleConfiguration
{
    public List<ulong> AllowedPlayers { get; set; } = new();
}
