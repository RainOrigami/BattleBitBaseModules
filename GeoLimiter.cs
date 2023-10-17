using BBRAPIModules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GeoLimiter;

[Module("Kick users based on their country of origin", "1.2.0")]
public class GeoLimiter : BattleBitModule
{
    public static GeoLimiterConfiguration Configuration { get; set; } = null!;
    public GeoLimiterServerConfiguration ServerConfiguration { get; set; } = null!;

    [ModuleReference]
    public dynamic? GranularPermissions { get; set; }

    private static Geolocation[] database = null!;

    public override void OnModulesLoaded()
    {
        if (!File.Exists(Path.Combine(Configuration.DataDirectory, Configuration.DatabaseFileName)))
        {
            this.Logger.Error($"Could not find geolocation database at {Path.GetFullPath(Path.Combine(Configuration.DataDirectory, Configuration.DatabaseFileName))}");
            this.Unload();
            return;
        }

        if (database != null)
        {
            return;
        }

        this.Logger.Info("Loading geolocation database...");
        Stopwatch stopwatch = Stopwatch.StartNew();
        database = File.ReadAllLines(Path.Combine(Configuration.DataDirectory, Configuration.DatabaseFileName))
            .Select(x => x.Split(','))
            .Select(x => new Geolocation() { FromIP = uint.Parse(x[0].Trim('"')), ToIP = uint.Parse(x[1].Trim('"')), CountryCode = x[2].Trim('"'), CountryName = x[3].Trim('"') })
            .ToArray();
        this.Logger.Info($"Loaded {database.Length} geolocation entries, took {stopwatch.ElapsedMilliseconds}ms");
    }

    public override Task OnPlayerConnected(RunnerPlayer player)
    {
        if (this.GranularPermissions is not null && ServerConfiguration.IgnoredPermissions?.Any(p => this.GranularPermissions.HasPermission(player.SteamID, p)) == true)
        {
            this.Logger.Info($"Player {player.SteamID} has an ignored permission, skipping...");
            return Task.CompletedTask;
        }

        byte[] ipBytes = player.IP.GetAddressBytes();
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(ipBytes);
        }

        int ipNumber = BitConverter.ToInt32(ipBytes, 0);
        this.Logger.Debug($"IP address {player.IP} as integer: {ipNumber}");

        Geolocation? geolocation = database.FirstOrDefault(x => x.FromIP <= ipNumber && x.ToIP >= ipNumber);
        if (geolocation == null)
        {
            this.Logger.Warn($"Could not find geolocation for IP {player.IP}");
            return Task.CompletedTask;
        }

        this.Logger.Info($"Found geolocation for IP {player.IP}: {geolocation.CountryName} ({geolocation.CountryCode})");

        if (this.ServerConfiguration.AllowCountryCodes && this.ServerConfiguration.CountryCodes.Contains(geolocation.CountryCode))
        {
            this.Logger.Info($"Country {geolocation.CountryCode} is allowed, skipping...");
            return Task.CompletedTask;
        }

        if (!this.ServerConfiguration.AllowCountryCodes && !this.ServerConfiguration.CountryCodes.Contains(geolocation.CountryCode))
        {
            this.Logger.Info($"Country {geolocation.CountryCode} is not denied, skipping...");
            return Task.CompletedTask;
        }

        this.Logger.Info($"Kicking player {player.SteamID} because country {geolocation.CountryCode} is disallowed");
        player.Kick(string.Format(ServerConfiguration.KickMessage, geolocation.CountryName));

        return Task.CompletedTask;
    }
}

public class GeoLimiterConfiguration : ModuleConfiguration
{
    public string DataDirectory { get; set; } = "./data/GeoLimiter";
    public string DatabaseFileName { get; set; } = "IP2LOCATION-LITE-DB1.CSV";
}

public class GeoLimiterServerConfiguration : ModuleConfiguration
{
    public string[] CountryCodes { get; set; } = new string[] { "DE", "AT", "CH" };
    public bool AllowCountryCodes { get; set; } = true;

    public string KickMessage { get; set; } = "Your country {0} is not allowed on this server.";

    public string[] IgnoredPermissions { get; set; } = Array.Empty<string>();
}

public class Geolocation
{
    public uint FromIP { get; set; }
    public uint ToIP { get; set; }
    public string CountryCode { get; set; } = null!;
    public string CountryName { get; set; } = null!;
}
