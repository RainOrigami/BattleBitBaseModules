# BattleBitBaseModules
All the basic modules for the modular BattleBit API https://github.com/RainOrigami/BattleBitAPIRunner

# Modules
- PlayerFinder - Library module for finding players
- PlayerPermissions - Library module for basic persistent player permissions (Roles)
- CommandHandler - Library module for easy command handling
- PermissionsCommands - Module for adding and removing permissions using in-game chat
- DiscordWebhooks - Module for sending messages to discord webhooks

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
This module stores player roles (Admin, Moderator, VIP, Special) to a json file and will load and apply previously assigned roles to players when they join.
It can also be used by other modules to get, set and check for roles of a player.

Roles are stored and shared globally across all connected servers. Every time a user role is changed, the json file is written to.

### Dependencies
- [Newtonsoft JSON](https://github.com/JamesNK/Newtonsoft.Json/releases) - `Bin\net6.0\Newtonsoft.Json.dll`

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

### Commands
- `help` - Lists all available and accessible commands.

### Dependencies
- [PlayerPermissions](https://github.com/RainOrigami/BattleBitBaseModules/blob/main/PlayerPermissions.cs)

## DiscordWebhooks
### Description
This module sends player connect and disconnect, server connect and disconnect, and player chat messages to a discord webhook which is specified in `DiscordWebhooks.json`.
It can also be used by other modules to send raw webhook messages.

### Dependencies
- [Newtonsoft JSON](https://github.com/JamesNK/Newtonsoft.Json/releases) - `Bin\net6.0\Newtonsoft.Json.dll`
- System.Net.Http - Copy from your dotnet6 installation, for example `C:\Program Files\dotnet\shared\Microsoft.NETCore.App\6.0.*\System.Net.Http.dll`

### Available methods and properties
- `void SendMessage(string message)` - Send the message to the webhook
