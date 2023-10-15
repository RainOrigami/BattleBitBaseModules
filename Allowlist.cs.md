# 1 Modules in Allowlist.cs

| Description                                                        | Version   |
|:-------------------------------------------------------------------|:----------|
| Block players who are not on the allowlist from joining the server | 1.0.0     |

## Commands
| Command      | Function Name   | Description                         | Allowed Roles   | Parameters                                      | Defaults   |
|:-------------|:----------------|:------------------------------------|:----------------|:------------------------------------------------|:-----------|
| allow add    | void            | Adds a player to the allowlist      | Moderator       | ['RunnerPlayer commandSource', 'ulong steamID'] | {}         |
| allow remove | void            | Removes a player from the allowlist | Moderator       | ['RunnerPlayer commandSource', 'ulong steamID'] | {}         |

## Public Methods
| Function Name   | Parameters                                       | Defaults   |
|:----------------|:-------------------------------------------------|:-----------|
|                 |                                                  |            |
|                 |                                                  |            |
|                 |                                                  |            |
| Task            | ['ulong steamID', 'PlayerJoiningArguments args'] | {}         |
| AllowAdd        | ['RunnerPlayer commandSource', 'ulong steamID']  | {}         |
| AllowRemove     | ['RunnerPlayer commandSource', 'ulong steamID']  | {}         |
|                 |                                                  |            |
|                 |                                                  |            |