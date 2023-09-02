using BattleBitAPI.Common;
using BBRAPIModules;
using Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleBitBaseModules;

/// <summary>
/// Author: @RainOrigami
/// Version: 0.4.11
/// </summary>
/// 

[RequireModule(typeof(CommandHandler))]
public class Voting : BattleBitModule
{
    private bool activeVote = false;
    private string voteText = "";
    private string[] voteOptions = new string[0];
    private Dictionary<ulong, int> votes = new();
    private DateTime endOfVote = DateTime.MinValue;

    public VoteConfiguration Configuration { get; set; }

    [ModuleReference]
    public dynamic? RichText { get; set; }

    [ModuleReference]
    public CommandHandler CommandHandler { get; set; }

    public override void OnModulesLoaded()
    {
        this.CommandHandler.Register(this);
    }

    [CommandCallback("vote", Description = "Votes for an option", AllowedRoles = Roles.Moderator)]
    public void StartVoteCommand(RunnerPlayer commandSource, string text, string options)
    {
        if (this.activeVote)
        {
            commandSource.Message("There is already an active vote.");
            return;
        }

        this.activeVote = true;
        this.voteText = text;
        this.voteOptions = options.Split('|');

        if (this.voteOptions.Length >= 10)
        {
            commandSource.Message("You can only have up to 9 options.");
            this.activeVote = false;
            return;
        }

        this.votes.Clear();
        this.endOfVote = DateTime.Now.AddSeconds(this.Configuration.VoteDuration);

        this.Server.SayToAllChat($"{this.RichText?.Size(125)}A vote has been started!");

        StringBuilder messageText = new($"{this.RichText?.Size(125)}{this.voteText}{this.RichText?.Size(100)}{Environment.NewLine}");
        for (int i = 0; i < this.voteOptions.Length; i++)
        {
            messageText.AppendLine($"Type {i + 1} in chat for {this.RichText?.FromColorName("yellow")}{this.voteOptions[i]}{this.RichText?.Color()}");
        }

        messageText.AppendLine($"{this.RichText?.Size(125)}You have {this.Configuration.VoteDuration} seconds to vote.");

        foreach (RunnerPlayer player in this.Server.AllPlayers)
        {
            player.Message($"{this.RichText?.Size(125)}{messageText}", this.Configuration.VoteDuration);
        }

        this.Server.SayToAllChat(messageText.ToString());

        Task.Run(voteHandler);
    }

    private void voteHandler()
    {
        while (this.IsLoaded && this.Server.IsConnected && this.activeVote)
        {
            if (DateTime.Now > this.endOfVote)
            {
                break;
            }

            Task.Delay(1000).Wait();
        }

        if (!this.IsLoaded || !this.Server.IsConnected || !this.activeVote)
        {
            return;
        }

        this.activeVote = false;

        if (this.votes.Count == 0)
        {
            this.Server.SayToAllChat($"{this.RichText?.Size(125)}The vote has ended!{Environment.NewLine}Nobody voted.");
            return;
        }

        int[] voteCounts = new int[this.voteOptions.Length];
        foreach (int vote in this.votes.Values)
        {
            voteCounts[vote - 1]++;
        }

        int maxVotes = voteCounts.Max();
        int maxVoteIndex = voteCounts.ToList().IndexOf(maxVotes);

        this.Server.SayToAllChat($"{this.RichText?.Size(125)}The vote has ended!{Environment.NewLine}{this.RichText?.FromColorName("yellow")}{this.voteOptions[maxVoteIndex]}{this.RichText?.Color()} won with {maxVotes} votes.");
    }

    public override async Task<bool> OnPlayerTypedMessage(RunnerPlayer player, ChatChannel channel, string msg)
    {
        if (!this.activeVote)
        {
            return true;
        }

        msg = new string(msg.Where(c => char.IsDigit(c)).Distinct().ToArray());
        if (msg.Length == 0)
        {
            return true;
        }

        if (msg.Length > 1)
        {
            player.SayToChat("Could not find a unique vote option in your message.");
            return true;
        }

        if (!int.TryParse(msg, out int vote))
        {
            return true;
        }

        if (vote < 1 || vote > this.voteOptions.Length)
        {
            return true;
        }

        if (this.votes.ContainsKey(player.SteamID))
        {
            this.votes.Remove(player.SteamID);
        }

        this.votes.Add(player.SteamID, vote);

        player.SayToChat($"You voted for {this.RichText?.FromColorName("yellow")}{this.voteOptions[vote - 1]}{this.RichText?.Color()}. You can change your vote any time.");

        await Task.CompletedTask;

        return true;
    }
}

public class VoteConfiguration : ModuleConfiguration
{
    public int VoteDuration { get; set; } = 60;
}
