using BBRAPIModules;
using System;
using System.Linq;

namespace PlayerFinder;

public class PlayerFinder : BattleBitModule
{
    public RunnerPlayer? ByExactName(string exactName, bool caseSensitive)
    {
        return this.Server.AllPlayers.FirstOrDefault(p => p.Name.Equals(exactName, caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase));
    }

    public RunnerPlayer? ByNamePart(string namePart)
    {
        RunnerPlayer? exactMatch = this.ByExactName(namePart, true);
        if (exactMatch != null)
        {
            return exactMatch;
        }

        exactMatch = this.ByExactName(namePart, false);
        if (exactMatch != null)
        {
            return exactMatch;
        }

        RunnerPlayer[] playerList = this.AllByNamePart(namePart);

        if (playerList.Length > 1)
        {
            throw new ManyPlayersMatchException(playerList);
        }

        if (playerList.Length == 0)
        {
            return null;
        }

        return playerList[0];
    }

    public RunnerPlayer? BySteamId(ulong steamId)
    {
        return this.Server.AllPlayers.FirstOrDefault(p => p.SteamID == steamId);
    }

    public RunnerPlayer[] AllByNamePart(string namePart)
    {
        return this.Server.AllPlayers.Where(p => p.Name.ToLower().Contains(namePart.ToLower())).ToArray();
    }
}

public class ManyPlayersMatchException : Exception
{
    public RunnerPlayer[] Players { get; }

    public ManyPlayersMatchException(RunnerPlayer[] players)
    {
        this.Players = players;
    }

    public override string ToString()
    {
        return $"Multiple players match: {string.Join(", ", this.Players.Select(p => p.Name))}";
    }
}