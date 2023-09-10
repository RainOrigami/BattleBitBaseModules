using BBRAPIModules;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BattleBitBaseModules;

[Module("Check for and download module updates from the module repository", "1.0.0")]
public class ModuleUpdates : BattleBitModule
{
    public static ModuleUpdatesConfiguration Configuration { get; set; } = null!;

    private static dynamic[] modulesToUpdate = Array.Empty<dynamic>();
    private static int loadedInstances = 0;

    public override void OnModulesLoaded()
    {
        if (loadedInstances == 0)
        {
            Task.Run(versionChecker);
        }

        loadedInstances++;
    }

    public override void OnModuleUnloading()
    {
        loadedInstances--;
    }

    private static DateTime lastChecked = DateTime.MinValue;

    private async Task versionChecker()
    {
        while (loadedInstances > 0)
        {
            if (lastChecked.AddSeconds(Configuration.CheckDelay) < DateTime.Now)
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
            // From internal class of different assembly, reflect to class Module static property IReadOnlyList<Module> Modules
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetTypes().Any(t => t.Namespace == "BattleBitAPIRunner"))!;
            Type moduleType = assembly.GetTypes().FirstOrDefault(t => t.Name == "Module")!;
            PropertyInfo modulesProperty = moduleType.GetProperty("Modules")!;
            collection = (IReadOnlyList<dynamic>)modulesProperty.GetValue(null)!;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error retrieving loaded modules: {ex.Message}");
            Console.ResetColor();
            return;
        }

        List<dynamic> modulesToUpdate = new();

        foreach (dynamic item in collection)
        {
            string? moduleName = null;
            string? moduleVersion = null;

            try
            {
                moduleName = item.Name;
                moduleVersion = item.Version;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error retrieving module name and version: {ex.Message}");
                Console.ResetColor();
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
                latestVersion = JsonConvert.DeserializeObject<dynamic>(response)!.versions[0].ToString();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error checking for module {moduleName} updates: {ex.Message}");
                Console.ResetColor();
                continue;
            }

            if (latestVersion == null)
            {
                continue;
            }

            if (moduleVersion != latestVersion)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Module {moduleName} is out of date! Installed version: {moduleVersion}, Latest version: {latestVersion}");
                Console.ResetColor();

                modulesToUpdate.Add(item);
            }
        }

        if (modulesToUpdate.Count > 0)
        {
            ModuleUpdates.modulesToUpdate = modulesToUpdate.ToArray();
            Console.WriteLine($"There are {modulesToUpdate.Count} modules to update. Run 'update all' to update all modules.");
            Console.ResetColor();
        }
    }

    private async Task doUpdate(string? module = null)
    {
        if (module is null)
        {
            await doVersionCheck();

            if (modulesToUpdate.Length == 0)
            {
                Console.WriteLine("There are no modules to update.");
                return;
            }

            foreach (dynamic item in modulesToUpdate)
            {
                await doUpdate(item.Name);
            }

            return;
        }

        dynamic? moduleToUpdate = modulesToUpdate.FirstOrDefault(m => m.Name == module);
        if (moduleToUpdate is null)
        {
            Console.WriteLine($"Module {module} is not out of date.");
            return;
        }

        Console.WriteLine($"Updating module {module}...");
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "BattleBitAPIRunner");
        byte[] response = await client.GetByteArrayAsync($"{Configuration.APIEndpoint}/Download/{module}/latest");
        File.WriteAllBytes(moduleToUpdate.ModuleFilePath, response);

        Console.WriteLine($"Module {module} updated successfully.");
    }

    public override void OnConsoleCommand(string command)
    {
        if (!command.ToLower().StartsWith("update"))
        {
            return;
        }
    }
}

public class ModuleUpdatesConfiguration : ModuleConfiguration
{
    public string APIEndpoint { get; set; } = "https://apirunner.mevng.net";
    public int CheckDelay { get; set; } = 30;
}
