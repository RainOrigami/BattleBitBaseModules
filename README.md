# BattleBitBaseModules
All the basic modules for the modular BattleBit API https://github.com/RainOrigami/BattleBitAPIRunner

# Modules
- MOTD - Module for displaying a message to every player who joins the server
- PlayerFinder - Library module for finding players
- PlayerPermissions - Library module for basic persistent player permissions (Roles)
- CommandHandler - Library module for easy command handling
- PermissionsCommands - Module for adding and removing permissions using in-game chat
- DiscordWebhooks - Module for sending messages to discord webhooks

## MOTD
### Description
This module shows a configurable message to each player when they join a server.
The message can be changed in the `motd.txt`.
The MOTD is stored and shared globally across all connected servers. Every time the MOTD is set, the text file is written to.

### Commands
- `setmotd "message of the day"` - Sets a new MOTD. Requires Admin role to execute.
- `motd` - Shows the current MOTD again.

### Dependencies
- [CommandHandler](https://github.com/RainOrigami/BattleBitBaseModules/blob/main/CommandHandler.cs)

## PlayerFinder
### Description
This module can be used by other modules to find players by different means. It doesn't do anything by itself.

### Dependencies
- None

### Available methods and properties
- `RunnerPlayer? ByExactName(string exactName, bool caseSensitive)` - Find a player by their exact name, case sensitive or insensitive. Returns a `RunnerPlayer` instance or null, if no player was found.
- `public RunnerPlayer? ByNamePart(string namePart)` - Find a player by part of their name, case insensitive. First tries exact match case sensitive, then insensitive, then partial search. If multiple players match the `namePart` a `ManyPlayersMatchException` exception is thrown containing a list of matching players. Returns a `RunnerPlayer` instance or null, if no player was found.
- `RunnerPlayer? BySteamId(ulong steamId)` - Find a player by their steam64id. Returns a `RunnerPlayer` instance or null, if no player was found.
- `public RunnerPlayer[] AllByNamePart(string namePart)` - Find all players matching part of their name.

## PlayerPermissions
### Description
This module stores player roles (Admin, Moderator, Vip, Special) to a json file and will load and apply previously assigned roles to players when they join.
It can also be used by other modules to get, set and check for roles of a player.

Roles are stored and shared globally across all connected servers. Every time a user role is changed, the json file is written to.

### Dependencies
- None

### Available methods and properties
- `bool HasPlayerRole(ulong steamID, Roles role)` - Returns true if the roles of a player include the specified role or set of roles.
- `Roles GetPlayerRoles(ulong steamID)` - Returns the players assigned roles.
- `void SetPlayerRoles(ulong steamID, Roles roles)` - Set the set of roles of a player.
- `void AddPlayerRoles(ulong steamID, Roles role)` - Adds the set of roles to the existing roles of a player.
- `void RemovePlayerRoles(ulong steamID, Roles role)` - Removes the set of roles from the existing roles of a player.

## PermissionsCommands
### Description
This module provides in-game chat commands to add and remove roles of players.

### Commands
- `addperm player role` - Adds the role to the player. Requires Admin role to execute.
- `removeperm` - Removes the role from the player. Requires Admin role to execute.

### Dependencies
- [PlayerPermissions](https://github.com/RainOrigami/BattleBitBaseModules/blob/main/PlayerPermissions.cs)
- [CommandHandler](https://github.com/RainOrigami/BattleBitBaseModules/blob/main/CommandHandler.cs)

## CommandHandler
### Description
This module can be used by other modules to create easy to use chat commands and automatically parse and provide the required parameters.
The hardcoded command prefix is `!`.

To create a new command in your module, register your module with CommandHandler in `OnModulesLoaded`:
```cs
public override void OnModulesLoaded()
{
    this.Server.GetModule<CommandHandler>()!.Register(this);
}
```

Commands are public void methods that take zero or many.
They require the `[CommandCallback("name")]` attribute:
- `name` - The name of the command that the player has to enter in chat (prefixed by the command prefix).
- `Description` - A short description of the command for use in the `help` command.
- `AllowedRoles` - A set of roles that are allowed to see and execute the command. Only works if PlayerPermissions is loaded.

The first method parameter must always be of type `RunnerPlayer` and contains the player who has called this command.
Other parameters are automatically parsed. Parameters of type `RunnerPlayer` will try to find a player using the `PlayerFinder.ByNamePart` if available, otherwise uses exact case-insensitive matching. Enums, such as `Roles` are parsed. Simple types, such as string, int, float, double, bool, are parsed.
Optional parameters are not supported at the moment.

**Example**
```cs
[CommandCallback("ping", Description = "Ping Pong", AllowedRoles = Roles.Vip)]
public void PingCommand(RunnerPlayer commandSource, int time)
{
  Thread.Sleep(time);
  commandSource.Message("Pong!");
}
```

### Commands
- `help` - Lists all available and accessible commands.

### Dependencies
- (Optional) [PlayerPermissions](https://github.com/RainOrigami/BattleBitBaseModules/blob/main/PlayerPermissions.cs)
- (Optional) [PlayerFinder](https://github.com/RainOrigami/BattleBitBaseModules/blob/main/PlayerFinder.cs)

### Available methods and properties
- `void Register(BattleBitModule module)` - Registers the commands of the specified module

## DiscordWebhooks
### Description
This module sends player connect and disconnect, server connect and disconnect, and player chat messages to a discord webhook which is specified in `DiscordWebhooks.json`.
It can also be used by other modules to send raw webhook messages.

### Dependencies
- [Newtonsoft JSON](https://github.com/JamesNK/Newtonsoft.Json/releases) - `Bin\net6.0\Newtonsoft.Json.dll`
- System.Net.Http - Copy from your dotnet6 installation, for example `C:\Program Files\dotnet\shared\Microsoft.NETCore.App\6.0.*\System.Net.Http.dll`

### Available methods and properties
- `void SendMessage(string message)` - Send the message to the webhook
