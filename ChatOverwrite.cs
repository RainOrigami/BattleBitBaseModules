using BattleBitAPI.Common;
using BBRAPIModules;
using Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatOverwrite;

[RequireModule(typeof(GranularPermissions))]
[Module("Overwrite chat messages", "1.0.0")]
public class ChatOverwrite : BattleBitModule
{
    public ChatOverwriteConfiguration Configuration { get; set; } = null!;

    [ModuleReference]
    public dynamic? CommandHandler { get; set; }

    [ModuleReference]
    public GranularPermissions GranularPermissions { get; set; } = null!;

    public override Task<bool> OnPlayerTypedMessage(RunnerPlayer player, ChatChannel channel, string msg)
    {
        if (this.CommandHandler is not null && this.CommandHandler.IsCommand(msg))
        {
            this.Logger.Debug($"Ignoring message {msg} from {player.Name} because it is a command");
            return Task.FromResult(true);
        }

        if (this.GranularPermissions.HasPermission(player.SteamID, "ChatOverwrite.Bypass"))
        {
            this.Logger.Debug($"Ignoring message {msg} from {player.Name} because they have the ChatOverwrite.Bypass permission");
            return Task.FromResult(true);
        }

        string? permission = this.Configuration.Overwrites.Keys.FirstOrDefault(k => this.GranularPermissions.HasPermission(player.SteamID, k));

        if (String.IsNullOrEmpty(permission))
        {
            this.Logger.Debug($"Ignoring message {msg} from {player.Name} because they do not have any ChatOverwrite permissions");
            return Task.FromResult(true);
        }

        OverwriteMessage overwriteMessage = this.Configuration.Overwrites[permission];

        this.Logger.Debug($"Overwriting message {msg} from {player.Name} with permission {permission}");

        string gradientName = FormatTextWithGradient(player.Name, overwriteMessage.GradientColors ?? Array.Empty<string>());

        string teamName = player.Team == Team.TeamA ? "US" : "RU";
        char? squadLetter = player.InSquad ? player.SquadName.ToString()[0] : null;

        foreach (RunnerPlayer chatTarget in this.Server.AllPlayers)
        {
            if (channel == ChatChannel.SquadChat && (chatTarget.Team != player.Team || !chatTarget.InSquad || !player.InSquad || chatTarget.SquadName != player.SquadName))
            {
                continue;
            }

            if (channel == ChatChannel.TeamChat && chatTarget.Team != player.Team)
            {
                continue;
            }

            string nameColor = player.Team == chatTarget.Team && player.InSquad && chatTarget.InSquad && player.SquadName == chatTarget.SquadName ? "green" : (player.Team == chatTarget.Team ? "blue" : "red");
            string teamAndSquadIndicator = teamName;
            if (squadLetter != null)
            {
                teamAndSquadIndicator += $"-{squadLetter}";
            }
            teamAndSquadIndicator = $"[{teamAndSquadIndicator}]";

            string textColor = channel == ChatChannel.TeamChat ? "blue" : (channel == ChatChannel.SquadChat ? "green" : "white");

            chatTarget.SayToChat(string.Format(overwriteMessage.Text, nameColor, player.Name, teamAndSquadIndicator, textColor, msg, gradientName));
        }

        return Task.FromResult(false);
    }

    private static string FormatTextWithGradient(string text, string[] gradientColors)
    {
        if (string.IsNullOrEmpty(text) || gradientColors.Length == 0)
        {
            return text;
        }

        int segmentCount = gradientColors.Length;
        int segmentLength = text.Length / segmentCount;
        int remainder = text.Length % segmentCount;

        StringBuilder formattedName = new StringBuilder();
        int currentIndex = 0;

        for (int i = 0; i < segmentCount; i++)
        {
            int currentSegmentLength = segmentLength + (i < remainder ? 1 : 0);
            string currentColor = gradientColors[i];
            string segmentText = text.Substring(currentIndex, currentSegmentLength);

            formattedName.Append($"<color=\"{currentColor}\">{segmentText}</color>");
            currentIndex += currentSegmentLength;
        }

        return formattedName.ToString();
    }
}

public class ChatOverwriteConfiguration : ModuleConfiguration
{
    public Dictionary<string, OverwriteMessage> Overwrites { get; set; } = new()
    {
        { "ChatOverwrite.Rainbow", new("{5}{2} : <color=\"{3}\">{4}", new string[] { "red", "orange", "yellow", "green", "blue", "purple" }) },
        { "ChatOverwrite.Large", new("<size=150%><color=\"{0}\">{1}</color>{2} : <color=\"{3}\">{4}") },
        { "ChatOverwrite.Sus", new("<color=\"{0}\">{1}</color>{2} : <color=\"{3}\">I am an idiot and cheat in online games. Please go to my Steam profile and report me!") },
        { "ChatOverwrite.Admin", new("<size=125%><color=\"{0}\">{1}</color>{2}<color=\"orange\">[Server Admin]</color> : <color=\"{3}\">{4}") }
    };
}

public record OverwriteMessage(string Text, string[]? GradientColors = null);