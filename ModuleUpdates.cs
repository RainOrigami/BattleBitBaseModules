using BBRAPIModules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace BattleBitBaseModules;

[Module("Check for and download module updates from the module repository", "1.0.0")]
public class ModuleUpdates : BattleBitModule
{
    public static ModuleUpdatesConfiguration Configuration { get; set; } = null!;

    PropertyInfo nameProperty = null!;
    PropertyInfo versionProperty = null!;
    PropertyInfo moduleFilePathProperty = null!;

    private static dynamic[] modulesToUpdate = Array.Empty<dynamic>();
    private static bool running = false;

    public override void OnModulesLoaded()
    {
        if (!running)
        {
            running = true;
            Task.Run(versionChecker);
        }
    }

    public override void OnModuleUnloading()
    {
        running = false;
    }

    private static DateTime lastChecked = DateTime.MinValue;

    private async Task versionChecker()
    {
        while (running)
        {
            if (lastChecked.AddMinutes(Configuration.CheckDelay) < DateTime.Now)
            {
                await doVersionCheck();
            }

            await Task.Delay(60000);
        }
    }

    private async Task doVersionCheck()
    {
        lastChecked = DateTime.Now;

        IReadOnlyList<dynamic> collection = null!;

        try
        {
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetTypes().Any(t => t.Namespace == "BattleBitAPIRunner"))!;
            Type moduleType = assembly.GetTypes().FirstOrDefault(t => t.Name == "Module")!;
            PropertyInfo modulesProperty = moduleType.GetProperty("Modules")!;
            collection = (IReadOnlyList<dynamic>)modulesProperty.GetValue(null)!;

            this.nameProperty = moduleType.GetProperty("Name")!;
            this.versionProperty = moduleType.GetProperty("Version")!;
            this.moduleFilePathProperty = moduleType.GetProperty("ModuleFilePath")!;
        }
        catch (Exception ex)
        {
            this.Logger.Error($"Error retrieving loaded modules: {ex.Message}");
            return;
        }

        List<dynamic> modulesToUpdate = new();

        foreach (dynamic item in collection)
        {
            string? moduleName = null;
            string? moduleVersion = null;

            try
            {
                moduleName = this.nameProperty.GetValue(item).ToString();
                moduleVersion = this.versionProperty.GetValue(item).ToString();
            }
            catch (Exception ex)
            {
                this.Logger.Error($"Error retrieving module name and version: {ex.Message}");
                continue;
            }

            if (moduleName == null || moduleVersion == null)
            {
                continue;
            }

            string? latestVersion = null;
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "BattleBitAPIRunner");
                string response = await client.GetStringAsync($"{Configuration.APIEndpoint}/Modules/GetModule/{moduleName}");
                using (JsonDocument responseDocument = JsonDocument.Parse(response))
                {
                    latestVersion = responseDocument.RootElement.GetProperty("versions").EnumerateArray().First().GetProperty("Version_v_number").GetString();
                }
            }
            catch (Exception ex)
            {
                this.Logger.Error($"Error checking for module {moduleName} updates: {ex.Message}");
                continue;
            }

            if (latestVersion == null)
            {
                continue;
            }

            if (moduleVersion != latestVersion)
            {
                this.Logger.Warn($"Module {moduleName} is out of date! Installed version: {moduleVersion}, Latest version: {latestVersion}");

                modulesToUpdate.Add(item);
            }
        }

        if (modulesToUpdate.Count > 0)
        {
            ModuleUpdates.modulesToUpdate = modulesToUpdate.ToArray();
            this.Logger.Warn($"There are {modulesToUpdate.Count} modules out of date. Run 'updateall' to update all modules or 'update modulename' to update an individual module.");
        }
    }

    private async Task doUpdate(string? module = null)
    {
        if (module is null)
        {
            if (modulesToUpdate.Length == 0)
            {
                this.Logger.Info("There are no modules to update. Run 'update' to fetch latest versions.");
                return;
            }

            foreach (dynamic item in modulesToUpdate)
            {
                await doUpdate(this.nameProperty.GetValue(item).ToString());
            }

            return;
        }

        dynamic? moduleToUpdate = modulesToUpdate.FirstOrDefault(m => module.Equals(this.nameProperty.GetValue(m).ToString(), StringComparison.InvariantCultureIgnoreCase));
        if (moduleToUpdate is null)
        {
            this.Logger.Error($"Module {module} is not out of date.");
            return;
        }

        this.Logger.Info($"Updating module {module}...");
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "BattleBitAPIRunner");
        byte[] response = await client.GetByteArrayAsync($"{Configuration.APIEndpoint}/Download/{module}/latest");
        File.WriteAllBytes(this.moduleFilePathProperty.GetValue(moduleToUpdate).ToString(), response);

        this.Logger.Info($"Module {module} updated successfully.");
    }

    public override void OnConsoleCommand(string command)
    {
        if (!command.Trim().ToLower().StartsWith("update"))
        {
            return;
        }

        if (command.Trim().ToLower() == "updateall")
        {
            Task.Run(() => doUpdate()).ContinueWith(t => this.Logger.Error($"Error updating modules: {t.Exception!.Message}"), TaskContinuationOptions.OnlyOnFaulted);
            return;
        }

        string[] args = command.Split(' ');
        if (args.Length > 2)
        {
            this.Logger.Error("Usage: update [module name]");
            return;
        }

        if (args.Length == 1)
        {
            doVersionCheck().ContinueWith(t => this.Logger.Error($"Error checking for module updates: {t.Exception!.Message}"), TaskContinuationOptions.OnlyOnFaulted);
            return;
        }

        Task.Run(() => doUpdate(args[1])).ContinueWith(t => this.Logger.Error($"Error updating module {args[1]}: {t.Exception!.Message}"), TaskContinuationOptions.OnlyOnFaulted);
    }
}

public class ModuleUpdatesConfiguration : ModuleConfiguration
{
    public string APIEndpoint { get; set; } = "https://modules.battlebit.community/api";
    public int CheckDelay { get; set; } = 30;
}
