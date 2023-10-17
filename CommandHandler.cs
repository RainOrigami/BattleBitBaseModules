using BattleBitAPI.Common;
using BBRAPIModules;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Commands;

[Module("Basic in-game chat command handler library", "1.1.1")]
public class CommandHandler : BattleBitModule
{
    public static CommandConfiguration CommandConfiguration { get; set; } = null!;
    public CommandPermissions CommandPermissions { get; set; } = null!;

    private Dictionary<string, (BattleBitModule Module, MethodInfo Method)> commandCallbacks = new();

    [ModuleReference]
    public dynamic? PlayerFinder { get; set; }

    [ModuleReference]
    public dynamic? GranularPermissions { get; set; }

    [ModuleReference]
    public dynamic? PlayerPermissions { get; set; }

    public override void OnModulesLoaded()
    {
        if (this.PlayerPermissions is null && this.GranularPermissions is null)
        {
            this.Logger.Warn($"Neither PlayerPermissions nor GranularPermissions is loaded. This module will not be able to check permissions for commands.");
        }

        this.Register(this);
    }

    public void Register(BattleBitModule module)
    {
        foreach (MethodInfo method in module.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            CommandCallbackAttribute? attribute = method.GetCustomAttribute<CommandCallbackAttribute>();
            if (attribute == null)
            {
                continue;
            }

            if (attribute.AllowedRoles != Roles.None)
            {
                this.Logger.Warn($"Command callback method {method.Name} in module {module.GetType().Name} has the deprecated AllowedRoles property set. Use Permissions instead. If you did not make this module, report the issue to the module author.");
            }
            else if (attribute.Permissions.Length == 1 && attribute.Permissions[0] == "*")
            {
                this.Logger.Warn($"Command callback method {method.Name} in module {module.GetType().Name} has no permissions set. This is not recommended as it allows everyone to use the command. Commands should have at least one granular permission or a PlayerPermissions role.");
            }

            // Store command permissions
            if (!this.CommandPermissions.Permissions.ContainsKey(attribute.Name))
            {
                this.CommandPermissions.Permissions.Add(attribute.Name, null);
            }

            // Validate parameter
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length > 0 && parameters[0].ParameterType != typeof(RunnerPlayer))
            {
                throw new Exception($"Command callback method {method.Name} in module {module.GetType().Name} has invalid first parameter. Must be of type RunnerPlayer.");
            }

            string command = attribute.Name.Trim().ToLower();

            // Prevent duplicate command names in different methods or modules
            if (this.commandCallbacks.ContainsKey(command))
            {
                if (this.commandCallbacks[command].Method == method)
                {
                    continue;
                }

                if (this.commandCallbacks[command].Module.GetType().Name == module.GetType().Name)
                {
                    throw new Exception($"Command callback method {method.Name} in module {module.GetType().Name} has the same command name {command} as another command callback method {this.commandCallbacks[command].Method.Name} in the same module.");
                }
                else
                {
                    throw new Exception($"Command callback method {method.Name} in module {module.GetType().Name} has the same command name {command} as command callback method {this.commandCallbacks[command].Method.Name} in module {this.commandCallbacks[command].Module.GetType().Name}.");
                }
            }

            // Prevent parent commands of subcommands (!perm command does not allow !perm add and !perm remove)
            foreach (string subcommand in this.commandCallbacks.Keys.Where(c => c.Contains(' ')))
            {
                if (!subcommand.StartsWith(command))
                {
                    continue;
                }

                throw new Exception($"Command callback {command} in module {module.GetType().Name} conflicts with subcommand {subcommand}.");
            }

            // Prevent subcommands of existing commands (!perm add and !perm remove do not allow !perm)
            if (command.Contains(' '))
            {
                string[] subcommandChain = command.Split(' ');
                string subcommand = "";
                for (int i = 0; i < subcommandChain.Length; i++)
                {
                    subcommand += $"{subcommandChain[i]} ";
                    if (this.commandCallbacks.ContainsKey(subcommand.Trim()))
                    {
                        throw new Exception($"Command callback {command} in module {module.GetType().Name} conflicts with parent command {subcommand.Trim()}.");
                    }
                }
            }

            this.commandCallbacks.Add(command, (module, method));
        }

        this.CommandPermissions.Save();
    }

    public override Task<bool> OnPlayerTypedMessage(RunnerPlayer player, ChatChannel channel, string message)
    {
        if (!IsCommand(message))
        {
            return Task.FromResult(true);
        }

        Task.Run(() => this.handleCommand(player, message));

        return Task.FromResult(false);
    }

    public override void OnConsoleCommand(string command)
    {
        if (!IsCommand(command))
        {
            return;
        }

        Task.Run(() => this.handleCommand(null, command));
    }

    public bool IsCommand(string message)
    {
        return message.StartsWith(CommandConfiguration.CommandPrefix) && message.Length > CommandConfiguration.CommandPrefix.Length;
    }

    public bool HasPermissionForCommand(RunnerPlayer player, CommandCallbackAttribute attribute)
    {
        if (attribute.AllowedRoles != Roles.None)
        {
            this.Logger.Warn($"Command {attribute.Name} has the deprecated AllowedRoles property set. Use Permissions instead. If you did not make this module, report the issue to the module author.");

            if (attribute.Permissions.Length == 1 && attribute.Permissions[0] == "*")
            {
                List<Roles> roles = new();
                foreach (Roles role in Enum.GetValues(typeof(Roles)))
                {
                    if (role == Roles.None)
                    {
                        continue;
                    }

                    if ((attribute.AllowedRoles & role) == role)
                    {
                        roles.Add(role);
                    }
                }

                this.Logger.Info($"Overwriting Permissions property of command {attribute.Name} with roles {string.Join(", ", roles.Select(r => r.ToString()))} from AllowedRoles property.");
                attribute.Permissions = roles.Select(r => r.ToString()).ToArray();
            }
        }

        if (attribute.Permissions.Length == 0 || attribute.Permissions[0] == "*")
        {
            return true;
        }

        if (this.PlayerPermissions is null && this.GranularPermissions is null)
        {
            this.Logger.Warn($"Command {attribute.Name} requires permissions but neither PlayerPermissions nor GranularPermissions is loaded.");
            return false;
        }

        // Permission overwrites from configuration file
        string[] requiredPermissions = attribute.Permissions;
        if (!this.CommandPermissions.Permissions.ContainsKey(attribute.Name))
        {
            this.Logger.Error($"Command {attribute.Name} has no permissions stored in the CommandPermissions configuration file. This should not happen, report the bug.");
        }

        if (this.CommandPermissions.Permissions.ContainsKey(attribute.Name) && this.CommandPermissions.Permissions[attribute.Name] is not null)
        {
            requiredPermissions = this.CommandPermissions.Permissions[attribute.Name]!;
        }

        // PlayerPermissions module
        if (this.PlayerPermissions is not null)
        {
            foreach (string requiredPermission in requiredPermissions)
            {
                if (!Enum.TryParse(requiredPermission, true, out Roles role))
                {
                    this.Logger.Warn($"Command {attribute.Name} could not resolve {requiredPermission} to a Role for PlayerPermissions.");
                    this.Logger.Info($"This warning can be ignored if you are also using the GranularPermissions module as the permission may be defined there.");
                    continue;
                }

                if (this.PlayerPermissions?.HasPlayerRole(player.SteamID, role))
                {
                    return true;
                }
            }
        }

        // GranularPermissions module
        if (this.GranularPermissions is not null)
        {
            foreach (string requiredPermission in requiredPermissions)
            {
                if (this.GranularPermissions.HasPermission(player.SteamID, requiredPermission))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void handleCommand(RunnerPlayer? player, string message)
    {
        string[] fullCommand = parseCommandString(message);
        string command = fullCommand[0].Trim().ToLower()[CommandConfiguration.CommandPrefix.Length..];

        int subCommandSkip;
        for (subCommandSkip = 1; subCommandSkip < fullCommand.Length && !this.commandCallbacks.ContainsKey(command); subCommandSkip++)
        {
            command += $" {fullCommand[subCommandSkip]}";
        }

        if (!this.commandCallbacks.ContainsKey(command))
        {
            if (player is null)
            {
                this.Logger.Error($"Command not found: {command}");
            }
            else
            {
                player.Message("<color=\"red\">Command not found", CommandConfiguration.MessageTimeout);
            }
            return;
        }

        fullCommand = new[] { command }.Concat(fullCommand.Skip(subCommandSkip)).ToArray();

        (BattleBitModule module, MethodInfo method) = this.commandCallbacks[command];
        CommandCallbackAttribute commandCallbackAttribute = method.GetCustomAttribute<CommandCallbackAttribute>()!;

        if (player is null && !commandCallbackAttribute.ConsoleCommand)
        {
            this.Logger.Error($"Command {command} is not a console command.");
            return;
        }

        // Permissions
        if (player is not null && !this.HasPermissionForCommand(player, commandCallbackAttribute))
        {
            if (CommandConfiguration.HideInaccessibleCommands)
            {
                if (player is null)
                {
                    this.Logger.Error($"Command not found: {command}");
                }
                else
                {
                    player.Message("<color=\"red\">Command not found", CommandConfiguration.MessageTimeout);
                }
                return;
            }

            player.Message($"<color=\"red\">You don't have permission to use this command.{Environment.NewLine}<color=\"white\">Required permission: {string.Join(" or ", commandCallbackAttribute.Permissions)}", CommandConfiguration.MessageTimeout);
            return;
        }

        ParameterInfo[] parameters = method.GetParameters();

        if (parameters.Length == 0)
        {
            method.Invoke(module, null);
            return;
        }

        bool hasOptional = parameters.Any(p => p.IsOptional);
        if (fullCommand.Length - 1 < parameters.Skip(1).Count(p => !p.IsOptional) || fullCommand.Length - 1 > parameters.Length - 1)
        {
            if (player is not null)
            {
                messagePlayerCommandUsage(player, method, $"Require {(hasOptional ? $"between {parameters.Skip(1).Count(p => !p.IsOptional)} and {parameters.Length - 1}" : $"{parameters.Length - 1}")} but got {fullCommand.Length - 1} argument{((fullCommand.Length - 1) == 1 ? "" : "s")}.");
            }
            else
            {
                this.Logger.Error($"Command {command} requires {(hasOptional ? $"between {parameters.Skip(1).Count(p => !p.IsOptional)} and {parameters.Length - 1}" : $"{parameters.Length - 1}")} but got {fullCommand.Length - 1} argument{((fullCommand.Length - 1) == 1 ? "" : "s")}.");
            }
            return;
        }

        object?[] args = new object[parameters.Length];
        args[0] = player;

        for (int i = 1; i < parameters.Length; i++)
        {
            ParameterInfo parameter = parameters[i];

            if (parameter.IsOptional && i >= fullCommand.Length)
            {
                args[i] = parameter.DefaultValue;
                continue;
            }

            string argument = fullCommand[i].Trim();

            if (parameter.ParameterType == typeof(string))
            {
                args[i] = argument;
            }
            else if (parameter.ParameterType == typeof(RunnerPlayer))
            {
                RunnerPlayer? targetPlayer = null;

                if (ulong.TryParse(argument, out ulong steamId) && this.Server.AllPlayers.FirstOrDefault(p => p.SteamID == steamId) is RunnerPlayer playerBySteamId)
                {
                    args[i] = targetPlayer;
                    continue;
                }

                if (this.PlayerFinder is not null)
                {
                    try
                    {
                        targetPlayer = this.PlayerFinder.ByNamePart(argument);
                    }
                    catch (Exception ex)
                    {
                        if (player is not null)
                        {
                            player.Message($"<color=\"red\">Error while searching for player name containing {argument}.{Environment.NewLine}<color=\"white\">{ex.Message}", CommandConfiguration.MessageTimeout);
                        }
                        else
                        {
                            this.Logger.Error($"Error while searching for player name containing {argument}.{Environment.NewLine}{ex.Message}");
                        }
                        return;
                    }

                    if (targetPlayer == null)
                    {
                        if (player is not null)
                        {
                            player.Message($"Could not find player name containing {argument}.", CommandConfiguration.MessageTimeout);
                        }
                        else
                        {
                            this.Logger.Error($"Could not find player name containing {argument}.");
                        }
                        return;
                    }
                }
                else
                {
                    targetPlayer = this.Server.AllPlayers.FirstOrDefault(p => p.Name.Equals(argument, StringComparison.OrdinalIgnoreCase));
                }

                if (targetPlayer == null)
                {
                    if (player is not null)
                    {
                        player.Message($"Could not find player {argument}.", CommandConfiguration.MessageTimeout);
                    }
                    else
                    {
                        this.Logger.Error($"Could not find player {argument}.");
                    }
                    return;
                }

                args[i] = targetPlayer;
            }
            else
            {
                if (!tryParseParameter(parameter, argument, out object? parsedValue))
                {
                    messagePlayerCommandUsage(player, method, $"Couldn't parse value {argument} to type {parameter.ParameterType.Name}");
                    return;
                }

                args[i] = parsedValue;
            }
        }

        method.Invoke(module, args);
    }

    private void messagePlayerCommandUsage(RunnerPlayer? player, MethodInfo method, string? error = null)
    {
        CommandCallbackAttribute commandCallbackAttribute = method.GetCustomAttribute<CommandCallbackAttribute>()!;
        bool hasOptional = method.GetParameters().Any(p => p.IsOptional);
        if (player is not null)
        {
            player.Message($"<color=\"red\">Invalid command usage{(error == null ? "" : $" ({error})")}.<color=\"white\"><br><b>Usage</b>: {CommandConfiguration.CommandPrefix}{commandCallbackAttribute.Name} {string.Join(' ', method.GetParameters().Skip(1).Select(s => $"{s.Name}{(s.IsOptional ? "*" : "")}"))}{(hasOptional ? "<br><size=80%>* Parameter is optional." : "")}", CommandConfiguration.MessageTimeout);
        }
        else
        {
            this.Logger.Error($"Invalid command usage{(error == null ? "" : $" ({error})")}.{Environment.NewLine}Usage: {CommandConfiguration.CommandPrefix}{commandCallbackAttribute.Name} {string.Join(' ', method.GetParameters().Skip(1).Select(s => $"{s.Name}{(s.IsOptional ? "*" : "")}"))}{(hasOptional ? $"{Environment.NewLine}* Parameter is optional." : "")}");
        }
    }

    private static bool tryParseParameter(ParameterInfo parameterInfo, string input, out object? parsedValue)
    {
        parsedValue = null;

        try
        {
            if (parameterInfo.ParameterType.IsEnum)
            {
                parsedValue = Enum.Parse(parameterInfo.ParameterType, input, true);
            }
            else
            {
                Type? targetType = targetType = Nullable.GetUnderlyingType(parameterInfo.ParameterType);
                if (targetType is null)
                {
                    targetType = parameterInfo.ParameterType;
                }
                parsedValue = Convert.ChangeType(input, targetType);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string[] parseCommandString(string command)
    {
        List<string> parameterValues = new();
        string[] tokens = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        bool insideQuotes = false;
        StringBuilder currentValue = new();

        foreach (var token in tokens)
        {
            if (!insideQuotes)
            {
                if (token.StartsWith("\"") && token.EndsWith("\""))
                {
                    insideQuotes = false;
                    currentValue.Clear();
                    parameterValues.Add(token.Substring(1, token.Length - 2));
                }
                else if (token.StartsWith("\""))
                {
                    insideQuotes = true;
                    currentValue.Append(token.Substring(1));
                }
                else
                {
                    parameterValues.Add(token);
                }
            }
            else
            {
                if (token.EndsWith("\""))
                {
                    insideQuotes = false;
                    currentValue.Append(" ").Append(token.Substring(0, token.Length - 1));
                    parameterValues.Add(currentValue.ToString());
                    currentValue.Clear();
                }
                else
                {
                    currentValue.Append(" ").Append(token);
                }
            }
        }

        return parameterValues.Select(unescapeQuotes).ToArray();
    }

    private static string unescapeQuotes(string input)
    {
        return input.Replace("\\\"", "\"");
    }

    [CommandCallback("help", Description = "Shows this help message", Permissions = new[] { "CommandHandler.Help" })]
    public void HelpCommand(RunnerPlayer player, int page = 1)
    {
        List<string> helpLines = new();
        foreach (var (commandKey, (module, method)) in this.commandCallbacks)
        {
            CommandCallbackAttribute commandCallbackAttribute = method.GetCustomAttribute<CommandCallbackAttribute>()!;

            if (!this.HasPermissionForCommand(player, commandCallbackAttribute))
            {
                continue;
            }

            helpLines.Add($"<b>{CommandConfiguration.CommandPrefix}{commandCallbackAttribute.Name}</b>{(string.IsNullOrEmpty(commandCallbackAttribute.Description) ? "" : $": {commandCallbackAttribute.Description}")}");
        }

        int pages = (int)Math.Ceiling((double)helpLines.Count / CommandConfiguration.CommandsPerPage);

        if (page < 1 || page > pages)
        {
            player.Message($"<color=\"red\">Invalid page number. Must be between 1 and {pages}.", CommandConfiguration.MessageTimeout);
            return;
        }

        player.Message($"<#FFA500>Available commands<br><color=\"white\">{Environment.NewLine}{string.Join(Environment.NewLine, helpLines.Skip((page - 1) * CommandConfiguration.CommandsPerPage).Take(CommandConfiguration.CommandsPerPage))}{(pages > 1 ? $"{Environment.NewLine}Page {page} of {pages}{(page < pages ? $" - type !help {page + 1} for next page" : "")}" : "")}", CommandConfiguration.MessageTimeout);
    }

    [CommandCallback("cmdhelp", Description = "Shows help for a specific command", Permissions = new[] { "CommandHandler.CommandHelp" })]
    public void CommandHelpCommand(RunnerPlayer player, string command)
    {
        if (!this.commandCallbacks.TryGetValue(command, out var commandCallback))
        {
            player.Message($"<color=\"red\">Command {command} not found.<color=\"white\">", CommandConfiguration.MessageTimeout);
            return;
        }

        CommandCallbackAttribute commandCallbackAttribute = commandCallback.Method.GetCustomAttribute<CommandCallbackAttribute>()!;

        if (!this.HasPermissionForCommand(player, commandCallbackAttribute))
        {
            if (CommandConfiguration.HideInaccessibleCommands)
            {
                player.Message($"<color=\"red\">Command {command} not found.<color=\"white\">", CommandConfiguration.MessageTimeout);
                return;
            }

            player.Message($"<color=\"red\">You don't have permission to see help about this command.", CommandConfiguration.MessageTimeout);
            return;
        }

        bool hasOptional = commandCallback.Method.GetParameters().Any(p => p.IsOptional);
        player.Message($"<size=120%>{commandCallback.Module.GetType().Name} {commandCallbackAttribute.Name}<size=100%><br>{commandCallbackAttribute.Description}<br><#F5F5F5>{CommandConfiguration.CommandPrefix}{commandCallbackAttribute.Name} {string.Join(' ', commandCallback.Method.GetParameters().Skip(1).Select(s => $"{s.Name}{(s.IsOptional ? "*" : "")}"))}{(hasOptional ? "<br><color=\"white\"><size=80%>* Parameter is optional." : "")}", CommandConfiguration.MessageTimeout);
    }
}

public class CommandCallbackAttribute : Attribute
{
    public string Name { get; set; }

    public string Description { get; set; } = string.Empty;
    public Roles AllowedRoles { get; set; } = Roles.None;
    public string[] Permissions { get; set; } = new[] { "*" };
    public bool ConsoleCommand { get; set; } = false;

    public CommandCallbackAttribute(string name)
    {
        this.Name = name;
    }
}

public class CommandConfiguration : ModuleConfiguration
{
    public string CommandPrefix { get; set; } = "!";
    public int CommandsPerPage { get; set; } = 6;
    public int MessageTimeout { get; set; } = 15;
    public bool HideInaccessibleCommands { get; set; } = false;
}

public class CommandPermissions : ModuleConfiguration
{
    public Dictionary<string, string[]?> Permissions { get; set; } = new();
}
