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

            if (parameters.Length == 0)
            {
                throw new Exception($"Command callback method {method.Name} in module {module.GetType().Name} has no parameters. Must have at least one parameter of type Context.");
            }

            if (parameters[0].ParameterType != typeof(Context))
            {
                throw new Exception($"Command callback method {method.Name} in module {module.GetType().Name} has invalid first parameter. Must be of type Context.");
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

        Task.Run(() => this.HandleCommand(new ChatSource(player), message));

        return Task.FromResult(false);
    }

    public override void OnConsoleCommand(string command)
    {
        if (!IsCommand(command))
        {
            return;
        }

        Task.Run(() => this.HandleCommand(new ConsoleSource(), command));
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

    public void HandleCommand(Source source, string message)
    {
        string[] fullCommand = parseCommandString(message);
        string command = fullCommand[0].Trim().ToLower()[CommandConfiguration.CommandPrefix.Length..];

        ChatSource? chatSource = source as ChatSource;
        Context errorContext = new Context(source, message, command, Array.Empty<string>(), Array.Empty<object?>(), null, this, null);

        int subCommandSkip;
        for (subCommandSkip = 1; subCommandSkip < fullCommand.Length && !this.commandCallbacks.ContainsKey(command); subCommandSkip++)
        {
            command += $" {fullCommand[subCommandSkip]}";
        }

        if (!this.commandCallbacks.ContainsKey(command))
        {
            if (chatSource is not null)
            {
                errorContext.Reply($"<color=\"red\">Command not found: {command}");
            }
            else
            {
                errorContext.Reply($"Command not found: {command}");
            }
            return;
        }

        fullCommand = new[] { command }.Concat(fullCommand.Skip(subCommandSkip)).ToArray();

        (BattleBitModule module, MethodInfo method) = this.commandCallbacks[command];
        CommandCallbackAttribute commandCallbackAttribute = method.GetCustomAttribute<CommandCallbackAttribute>()!;

        if (source is ConsoleSource && !commandCallbackAttribute.ConsoleCommand)
        {
            this.Logger.Error($"Command {command} is not a console command.");
            return;
        }

        // Permissions
        if (chatSource is not null && !this.HasPermissionForCommand(chatSource.Invoker, commandCallbackAttribute))
        {
            if (CommandConfiguration.HideInaccessibleCommands)
            {
                errorContext.Reply($"<color=\"red\">Command not found: {command}");
                return;
            }

            errorContext.Reply($"<color=\"red\">You don't have permission to use this command.{Environment.NewLine}<color=\"white\">Required permission: {string.Join(" or ", commandCallbackAttribute.Permissions)}");
            return;
        }

        ParameterInfo[] parameters = method.GetParameters();

        bool hasOptional = parameters.Any(p => p.IsOptional);
        if (fullCommand.Length - 1 < parameters.Skip(1).Count(p => !p.IsOptional) || fullCommand.Length - 1 > parameters.Length - 1)
        {
            sendCommandUsageMessage(errorContext, method, $"Require {(hasOptional ? $"between {parameters.Skip(1).Count(p => !p.IsOptional)} and {parameters.Length - 1}" : $"{parameters.Length - 1}")} but got {fullCommand.Length - 1} argument{((fullCommand.Length - 1) == 1 ? "" : "s")}.");
            return;
        }

        object?[] args = new object[parameters.Length];

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
                        if (chatSource is not null)
                        {
                            errorContext.Reply($"<color=\"red\">Error while searching for player name containing {argument}.{Environment.NewLine}<color=\"white\">{ex.Message}");
                        }
                        else
                        {
                            this.Logger.Error($"Error while searching for player name containing {argument}.{Environment.NewLine}{ex.Message}");
                        }
                        return;
                    }

                    if (targetPlayer == null)
                    {
                        errorContext.Reply($"Could not find player name containing {argument}.");
                        return;
                    }
                }
                else
                {
                    targetPlayer = this.Server.AllPlayers.FirstOrDefault(p => p.Name.Equals(argument, StringComparison.OrdinalIgnoreCase));
                }

                if (targetPlayer == null)
                {
                    errorContext.Reply($"Could not find player {argument}.");
                    return;
                }

                args[i] = targetPlayer;
            }
            else
            {
                if (!tryParseParameter(parameter, argument, out object? parsedValue))
                {
                    sendCommandUsageMessage(errorContext, method, $"Couldn't parse value {argument} to type {parameter.ParameterType.Name}");
                    return;
                }

                args[i] = parsedValue;
            }
        }

        args[0] = new Context(source, message, command, fullCommand.Skip(1).ToArray(), args.Skip(1).ToArray(), module, this, commandCallbackAttribute);

        object? result = method.Invoke(module, args);
        if (result is not null)
        {
            source.Reply((Context)args[0]!, result.ToString() ?? "No reply");
        }
    }

    private void sendCommandUsageMessage(Context context, MethodInfo method, string? error = null)
    {
        CommandCallbackAttribute commandCallbackAttribute = method.GetCustomAttribute<CommandCallbackAttribute>()!;
        bool hasOptional = method.GetParameters().Any(p => p.IsOptional);
        if (context.Source is ChatSource chatSource)
        {
            context.Reply($"<color=\"red\">Invalid command usage{(error == null ? "" : $" ({error})")}.<color=\"white\"><br><b>Usage</b>: {CommandConfiguration.CommandPrefix}{commandCallbackAttribute.Name} {string.Join(' ', method.GetParameters().Skip(1).Select(s => $"{s.Name}{(s.IsOptional ? "*" : "")}"))}{(hasOptional ? "<br><size=80%>* Parameter is optional." : "")}");
        }
        else
        {
            context.Reply($"Invalid command usage{(error == null ? "" : $" ({error})")}.{Environment.NewLine}Usage: {CommandConfiguration.CommandPrefix}{commandCallbackAttribute.Name} {string.Join(' ', method.GetParameters().Skip(1).Select(s => $"{s.Name}{(s.IsOptional ? "*" : "")}"))}{(hasOptional ? $"{Environment.NewLine}* Parameter is optional." : "")}");
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

    [CommandCallback("help", Description = "Shows this help message", Permissions = new[] { "CommandHandler.Help" }, ConsoleCommand = true)]
    public string HelpCommand(Context context, int page = 1)
    {
        List<string> helpLines = new();
        foreach (var (commandKey, (module, method)) in this.commandCallbacks)
        {
            CommandCallbackAttribute commandCallbackAttribute = method.GetCustomAttribute<CommandCallbackAttribute>()!;

            if (context.Source is ChatSource chatSource && !this.HasPermissionForCommand(chatSource.Invoker, commandCallbackAttribute))
            {
                continue;
            }

            if (context.Source is ChatSource)
            {
                helpLines.Add($"<b>{CommandConfiguration.CommandPrefix}{commandCallbackAttribute.Name}</b>{(string.IsNullOrEmpty(commandCallbackAttribute.Description) ? "" : $": {commandCallbackAttribute.Description}")}");
            }
            else
            {
                helpLines.Add($"{CommandConfiguration.CommandPrefix}{commandCallbackAttribute.Name}{(string.IsNullOrEmpty(commandCallbackAttribute.Description) ? "" : $": {commandCallbackAttribute.Description}")}");
            }
        }

        int pages = (int)Math.Ceiling((double)helpLines.Count / CommandConfiguration.CommandsPerPage);

        if (page < 1 || page > pages)
        {
            if (context.Source is ChatSource)
            {
                return $"<color=\"red\">Invalid page number. Must be between 1 and {pages}.";
            }
            else
            {
                return $"Invalid page number. Must be between 1 and {pages}.";
            }
        }

        if (context.Source is ChatSource)
        {
            return $"<#FFA500>Available commands<br><color=\"white\">{Environment.NewLine}{string.Join(Environment.NewLine, helpLines.Skip((page - 1) * CommandConfiguration.CommandsPerPage).Take(CommandConfiguration.CommandsPerPage))}{(pages > 1 ? $"{Environment.NewLine}Page {page} of {pages}{(page < pages ? $" - type !help {page + 1} for next page" : "")}" : "")}";
        }
        else
        {
            return $"Available commands{Environment.NewLine}{string.Join(Environment.NewLine, helpLines.Skip((page - 1) * CommandConfiguration.CommandsPerPage).Take(CommandConfiguration.CommandsPerPage))}{(pages > 1 ? $"{Environment.NewLine}Page {page} of {pages}{(page < pages ? $" - type !help {page + 1} for next page" : "")}" : "")}";
        }
    }

    [CommandCallback("cmdhelp", Description = "Shows help for a specific command", Permissions = new[] { "CommandHandler.CommandHelp" }, ConsoleCommand = true)]
    public string CommandHelpCommand(Context context, string command)
    {
        if (!this.commandCallbacks.TryGetValue(command, out var commandCallback))
        {
            if (context.Source is ChatSource)
            {
                return $"<color=\"red\">Command {command} not found.<color=\"white\">";
            }
            else
            {
                return $"Command {command} not found.";
            }
        }

        CommandCallbackAttribute commandCallbackAttribute = commandCallback.Method.GetCustomAttribute<CommandCallbackAttribute>()!;

        if (context.Source is ChatSource chatSource && !this.HasPermissionForCommand(chatSource.Invoker, commandCallbackAttribute))
        {
            if (CommandConfiguration.HideInaccessibleCommands)
            {
                return $"<color=\"red\">Command {command} not found.";
            }

            return $"<color=\"red\">You don't have permission to see help about this command.";
        }

        bool hasOptional = commandCallback.Method.GetParameters().Any(p => p.IsOptional);

        if (context.Source is ChatSource)
        {
            return $"<size=120%>{commandCallback.Module.GetType().Name} {commandCallbackAttribute.Name}<size=100%><br>{commandCallbackAttribute.Description}<br><#F5F5F5>{CommandConfiguration.CommandPrefix}{commandCallbackAttribute.Name} {string.Join(' ', commandCallback.Method.GetParameters().Skip(1).Select(s => $"{s.Name}{(s.IsOptional ? "*" : "")}"))}{(hasOptional ? "<br><color=\"white\"><size=80%>* Parameter is optional." : "")}";
        }
        else
        {
            return $"{commandCallback.Module.GetType().Name} {commandCallbackAttribute.Name}{Environment.NewLine}{commandCallbackAttribute.Description}{Environment.NewLine}{CommandConfiguration.CommandPrefix}{commandCallbackAttribute.Name} {string.Join(' ', commandCallback.Method.GetParameters().Skip(1).Select(s => $"{s.Name}{(s.IsOptional ? "*" : "")}"))}{(hasOptional ? $"{Environment.NewLine}* Parameter is optional." : "")}";
        }
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
    public bool ReplyToChat { get; set; } = false;
}

public class CommandPermissions : ModuleConfiguration
{
    public Dictionary<string, string[]?> Permissions { get; set; } = new();
}

public class Context
{
    public Source Source { get; set; }
    public string Message { get; set; }
    public string Command { get; set; }
    public string[] RawParameters { get; set; }
    public object?[] Parameters { get; set; }
    public BattleBitModule? Module { get; set; }
    public CommandHandler CommandHandler { get; set; }
    public CommandCallbackAttribute? CommandCallbackAttribute { get; set; }

    public Context(Source source, string message, string command, string[] rawParameters, object?[] parameters, BattleBitModule? module, CommandHandler commandHandler, CommandCallbackAttribute? commandCallbackAttribute)
    {
        this.Source = source;
        this.Message = message;
        this.Command = command;
        this.RawParameters = rawParameters;
        this.Parameters = parameters;
        this.Module = module;
        this.CommandHandler = commandHandler;
        this.CommandCallbackAttribute = commandCallbackAttribute;
    }

    public virtual void Reply(string message)
    {
        this.Source.Reply(this, message);
    }
}

public abstract class Source
{
    public abstract void Reply(Context context, string message);
}

public class ChatSource : Source
{
    public ChatSource(RunnerPlayer invoker)
    {
        this.Invoker = invoker;
    }

    public RunnerPlayer Invoker { get; }

    public override void Reply(Context context, string message)
    {
        if (CommandHandler.CommandConfiguration.ReplyToChat)
        {
            this.Invoker.SayToChat(message);
        }
        else
        {
            this.Invoker.Message(message, CommandHandler.CommandConfiguration.MessageTimeout);
        }
    }
}

public class ConsoleSource : Source
{
    public override void Reply(Context context, string message)
    {
        context.CommandHandler.Logger.Info(message);
    }
}
