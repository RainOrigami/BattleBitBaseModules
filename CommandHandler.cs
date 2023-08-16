using BattleBitAPI.Common;
using BBRAPIModules;
using Permissions;
using PlayerFinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Commands;

[RequireModule(typeof(PlayerPermissions))]
public class CommandHandler : BattleBitModule
{
    private const string COMMAND_PREFIX = "!";

    public CommandHandler(RunnerServer server) : base(server)
    {

    }

    private PlayerPermissions permissions = null!;
    private Dictionary<string, (BattleBitModule Module, MethodInfo Method)> commandCallbacks = new();
    PlayerFinder.PlayerFinder findPlayer;

    public override void OnModulesLoaded()
    {
        this.permissions = this.Server.GetModule<PlayerPermissions>()!;
        this.findPlayer = this.Server.GetModule<PlayerFinder.PlayerFinder>()!;
        this.Register(this);
    }


    public void Register(BattleBitModule module)
    {
        foreach (MethodInfo method in module.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            CommandCallbackAttribute? attribute = method.GetCustomAttribute<CommandCallbackAttribute>();
            if (attribute != null)
            {
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
                    if (this.commandCallbacks[command].Method != method)
                    {
                        throw new Exception($"Command callback method {method.Name} in module {module.GetType().Name} has the same name as another command callback method in the same module.");
                    }

                    // Already added
                    continue;
                }

                this.commandCallbacks.Add(command, (module, method));
            }
        }
    }

    public override Task<bool> OnPlayerTypedMessage(RunnerPlayer player, ChatChannel channel, string message)
    {
        if (!message.StartsWith(COMMAND_PREFIX) || (message.StartsWith(COMMAND_PREFIX) && message.Length <= COMMAND_PREFIX.Length))
        {
            return Task.FromResult(true);
        }

        Task.Run(() => this.handleCommand(player, message));

        return Task.FromResult(false);
    }

    private void handleCommand(RunnerPlayer player, string message)
    {
        string[] fullCommand = parseCommandString(message);
        string command = fullCommand[0].Trim().ToLower()[COMMAND_PREFIX.Length..];

        if (!this.commandCallbacks.ContainsKey(command))
        {
            player.Message("Command not found");
            return;
        }

        (BattleBitModule module, MethodInfo method) = this.commandCallbacks[command];
        CommandCallbackAttribute commandCallbackAttribute = method.GetCustomAttribute<CommandCallbackAttribute>()!;

        // Permissions
        if (commandCallbackAttribute.AllowedRoles != Roles.None && (this.permissions.GetPlayerRoles(player.SteamID) & commandCallbackAttribute.AllowedRoles) == 0)
        {
            player.Message($"You don't have permission to use this command.");
            return;
        }

        ParameterInfo[] parameters = method.GetParameters();

        if (parameters.Length == 0)
        {
            method.Invoke(module, null);
            return;
        }

        if (parameters.Length != fullCommand.Length)
        {
            messagePlayerCommandUsage(player, method, $"Require {parameters.Length - 1} but got {fullCommand.Length - 1} arguments");
            return;
        }

        object?[] args = new object[parameters.Length];
        args[0] = player;

        for (int i = 1; i < parameters.Length; i++)
        {
            string argument = fullCommand[i].Trim();
            ParameterInfo parameter = parameters[i];

            if (parameter.ParameterType == typeof(string))
            {
                args[i] = argument;
            }
            else if (parameter.ParameterType == typeof(RunnerPlayer))
            {
                RunnerPlayer? targetPlayer = null;

                try
                {
                    targetPlayer = findPlayer.ByNamePart(argument);
                }
                catch (ManyPlayersMatchException ex)
                {
                    player.Message($"Multiple players found with name containing {argument}:<br>{string.Join(", ", ex.Players.Select(p => p.Name))}");
                    return;
                }

                if (targetPlayer == null)
                {
                    player.Message($"Could not find player name containing {argument}.");
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

    private static void messagePlayerCommandUsage(RunnerPlayer player, MethodInfo method, string? error = null)
    {
        CommandCallbackAttribute commandCallbackAttribute = method.GetCustomAttribute<CommandCallbackAttribute>()!;

        player.Message($"<color=\"red\">Invalid command usage{(error == null ? "" : $" ({error})")}.<color=\"white\"><br><b>Usage</b>: {COMMAND_PREFIX}{commandCallbackAttribute.Name} {string.Join(' ', method.GetParameters().Skip(1).Select(s => s.Name))}");
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
                parsedValue = Convert.ChangeType(input, parameterInfo.ParameterType);
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
                if (token.StartsWith("\""))
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

    [CommandCallback("help", Description = "Shows this help message")]
    public void HelpCommand(RunnerPlayer player)
    {
        StringBuilder helpOutput = new();

        helpOutput.AppendLine("Available commands:<br>");

        foreach (var (command, (module, method)) in this.commandCallbacks)
        {
            CommandCallbackAttribute commandCallbackAttribute = method.GetCustomAttribute<CommandCallbackAttribute>()!;

            if (commandCallbackAttribute.AllowedRoles != Roles.None && (this.permissions.GetPlayerRoles(player.SteamID) & commandCallbackAttribute.AllowedRoles) == 0)
            {
                continue;
            }

            helpOutput.AppendLine($"<b>{COMMAND_PREFIX}{commandCallbackAttribute.Name}</b>: {commandCallbackAttribute.Description}<br>");
        }

        player.Message(helpOutput.ToString());
    }
}

public class CommandCallbackAttribute : Attribute
{
    public string Name { get; set; }

    public string Description { get; set; } = string.Empty;
    public Roles AllowedRoles { get; set; }

    public CommandCallbackAttribute(string name)
    {
        this.Name = name;
    }
}
