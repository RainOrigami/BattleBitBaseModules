# 1 Modules in ModeratorTools.cs

| Description           | Version   |
|:----------------------|:----------|
| Basic moderator tools | 1.0.0     |

## Commands
| Command       | Function Name   | Description                                    | Allowed Roles   | Parameters                                                                                       | Defaults                              |
|:--------------|:----------------|:-----------------------------------------------|:----------------|:-------------------------------------------------------------------------------------------------|:--------------------------------------|
| Say           | void            | Prints a message to all players                | Moderator       | ['RunnerPlayer commandSource', 'string message']                                                 | {}                                    |
| SayToPlayer   | void            | Prints a message to all players                | Moderator       | ['RunnerPlayer commandSource', 'RunnerPlayer target', 'string message']                          | {}                                    |
| AnnounceShort | void            | Prints a short announce to all players         | Moderator       | ['RunnerPlayer commandSource', 'string message']                                                 | {}                                    |
| AnnounceLong  | void            | Prints a long announce to all players          | Moderator       | ['RunnerPlayer commandSource', 'string message']                                                 | {}                                    |
| Message       | void            | Messages a specific player                     | Moderator       | ['RunnerPlayer commandSource', 'RunnerPlayer target', 'string message', 'float? timeout = null'] | {'timeout': 'null'}                   |
| Clear         | void            | Clears the chat                                | Moderator       | ['RunnerPlayer commandSource']                                                                   | {}                                    |
| Kick          | void            | Kicks a player                                 | Moderator       | ['RunnerPlayer commandSource', 'RunnerPlayer target', 'string? reason = null']                   | {'reason': 'null'}                    |
| Ban           | void            | Bans a player                                  | Moderator       | ['RunnerPlayer commandSource', 'RunnerPlayer target']                                            | {}                                    |
| Kill          | void            | Kills a player                                 | Moderator       | ['RunnerPlayer commandSource', 'RunnerPlayer target', 'string? message = null']                  | {'message': 'null'}                   |
| Gag           | void            | Gags a player                                  | Moderator       | ['RunnerPlayer commandSource', 'RunnerPlayer target', 'string? message = null']                  | {'message': 'null'}                   |
| Ungag         | void            | Ungags a player                                | Moderator       | ['RunnerPlayer commandSource', 'RunnerPlayer target', 'string? message = null']                  | {'message': 'null'}                   |
| Mute          | void            | Mutes a player                                 | Moderator       | ['RunnerPlayer commandSource', 'RunnerPlayer target', 'string? message = null']                  | {'message': 'null'}                   |
| Unmute        | void            | Unmutes a player                               | Moderator       | ['RunnerPlayer commandSource', 'RunnerPlayer target', 'string? message = null']                  | {'message': 'null'}                   |
| Silence       | void            | Mutes and gags a player                        | Moderator       | ['RunnerPlayer commandSource', 'RunnerPlayer target', 'string? message = null']                  | {'message': 'null'}                   |
| Unsilence     | void            | Unmutes and ungags a player                    | Moderator       | ['RunnerPlayer commandSource', 'RunnerPlayer target', 'string? message = null']                  | {'message': 'null'}                   |
| LockSpawn     | void            | Prevents a player or all players from spawning | Moderator       | ['RunnerPlayer commandSource', 'RunnerPlayer? target = null', 'string? message = null']          | {'target': 'null', 'message': 'null'} |
| UnlockSpawn   | void            | Allows a player or all players to spawn        | Moderator       | ['RunnerPlayer commandSource', 'RunnerPlayer? target = null', 'string? message = null']          | {'target': 'null', 'message': 'null'} |
| tp2me         | void            | Teleports a player to you                      | Moderator       | ['RunnerPlayer commandSource', 'RunnerPlayer target']                                            | {}                                    |
| tpme2         | void            | Teleports you to a player                      | Moderator       | ['RunnerPlayer commandSource', 'RunnerPlayer target']                                            | {}                                    |
| tp            | void            | Teleports a player to another player           | Moderator       | ['RunnerPlayer commandSource', 'RunnerPlayer target', 'RunnerPlayer destination']                | {}                                    |
| tp2pos        | void            | Teleports a player to a position               | Moderator       | ['RunnerPlayer commandSource', 'RunnerPlayer target', 'int x', 'int y', 'int z']                 | {}                                    |
| tpme2pos      | void            | Teleports you to a position                    | Moderator       | ['RunnerPlayer commandSource', 'int x', 'int y', 'int z']                                        | {}                                    |
| freeze        | void            | Freezes a player                               | Moderator       | ['RunnerPlayer commandSource', 'RunnerPlayer target', 'string? message = null']                  | {'message': 'null'}                   |
| unfreeze      | void            | Unfreezes a player                             | Moderator       | ['RunnerPlayer commandSource', 'RunnerPlayer target', 'string? message = null']                  | {'message': 'null'}                   |
| Inspect       | void            | Inspects a player or stops inspection          | Moderator       | ['RunnerPlayer commandSource', 'RunnerPlayer? target = null']                                    | {'target': 'null'}                    |

## Public Methods
| Function Name          | Parameters                                                                                       | Defaults                              |
|:-----------------------|:-------------------------------------------------------------------------------------------------|:--------------------------------------|
|                        |                                                                                                  |                                       |
|                        |                                                                                                  |                                       |
| void                   | ['']                                                                                             | {}                                    |
| Task                   | ['']                                                                                             | {}                                    |
| Say                    | ['RunnerPlayer commandSource', 'string message']                                                 | {}                                    |
| SayToPlayer            | ['RunnerPlayer commandSource', 'RunnerPlayer target', 'string message']                          | {}                                    |
| AnnounceShort          | ['RunnerPlayer commandSource', 'string message']                                                 | {}                                    |
| AnnounceLong           | ['RunnerPlayer commandSource', 'string message']                                                 | {}                                    |
| Message                | ['RunnerPlayer commandSource', 'RunnerPlayer target', 'string message', 'float? timeout = null'] | {'timeout': 'null'}                   |
| Clear                  | ['RunnerPlayer commandSource']                                                                   | {}                                    |
| Kick                   | ['RunnerPlayer commandSource', 'RunnerPlayer target', 'string? reason = null']                   | {'reason': 'null'}                    |
| Ban                    | ['RunnerPlayer commandSource', 'RunnerPlayer target']                                            | {}                                    |
| Kill                   | ['RunnerPlayer commandSource', 'RunnerPlayer target', 'string? message = null']                  | {'message': 'null'}                   |
| Gag                    | ['RunnerPlayer commandSource', 'RunnerPlayer target', 'string? message = null']                  | {'message': 'null'}                   |
| Ungag                  | ['RunnerPlayer commandSource', 'RunnerPlayer target', 'string? message = null']                  | {'message': 'null'}                   |
| Mute                   | ['RunnerPlayer commandSource', 'RunnerPlayer target', 'string? message = null']                  | {'message': 'null'}                   |
| Unmute                 | ['RunnerPlayer commandSource', 'RunnerPlayer target', 'string? message = null']                  | {'message': 'null'}                   |
| Silence                | ['RunnerPlayer commandSource', 'RunnerPlayer target', 'string? message = null']                  | {'message': 'null'}                   |
| Unsilence              | ['RunnerPlayer commandSource', 'RunnerPlayer target', 'string? message = null']                  | {'message': 'null'}                   |
| LockSpawn              | ['RunnerPlayer commandSource', 'RunnerPlayer? target = null', 'string? message = null']          | {'target': 'null', 'message': 'null'} |
| UnlockSpawn            | ['RunnerPlayer commandSource', 'RunnerPlayer? target = null', 'string? message = null']          | {'target': 'null', 'message': 'null'} |
| TeleportPlayerToMe     | ['RunnerPlayer commandSource', 'RunnerPlayer target']                                            | {}                                    |
| TeleportMeToPlayer     | ['RunnerPlayer commandSource', 'RunnerPlayer target']                                            | {}                                    |
| TeleportPlayerToPlayer | ['RunnerPlayer commandSource', 'RunnerPlayer target', 'RunnerPlayer destination']                | {}                                    |
| TeleportPlayerToPos    | ['RunnerPlayer commandSource', 'RunnerPlayer target', 'int x', 'int y', 'int z']                 | {}                                    |
| TeleportMeToPos        | ['RunnerPlayer commandSource', 'int x', 'int y', 'int z']                                        | {}                                    |
| Freeze                 | ['RunnerPlayer commandSource', 'RunnerPlayer target', 'string? message = null']                  | {'message': 'null'}                   |
| Unfreeze               | ['RunnerPlayer commandSource', 'RunnerPlayer target', 'string? message = null']                  | {'message': 'null'}                   |
| Inspect                | ['RunnerPlayer commandSource', 'RunnerPlayer? target = null']                                    | {'target': 'null'}                    |
| Task                   | ['RunnerPlayer player', 'ChatChannel channel', 'string msg']                                     | {}                                    |
| Task                   | ['RunnerPlayer player', 'OnPlayerSpawnArguments request']                                        | {}                                    |