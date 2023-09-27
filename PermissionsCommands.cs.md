# 1 Modules in PermissionsCommands.cs

| Description                                                   | Version   |
|:--------------------------------------------------------------|:----------|
| Provide addperm and removeperm commands for PlayerPermissions | 1.0.0     |

## Commands
| Command    | Function Name   | Description                          | Allowed Roles    | Parameters                                                                | Defaults                 |
|:-----------|:----------------|:-------------------------------------|:-----------------|:--------------------------------------------------------------------------|:-------------------------|
| addperm    | void            | Adds a permission to a player        | Admin            | ['RunnerPlayer commandSource', 'RunnerPlayer player', 'Roles permission'] | {}                       |
| removeperm | void            | Removes a permission from a player   | Admin            | ['RunnerPlayer commandSource', 'RunnerPlayer player', 'Roles permission'] | {}                       |
| clearperms | void            | Removes all permission from a player | Admin            | ['RunnerPlayer commandSource', 'RunnerPlayer player']                     | {}                       |
| listperms  | void            | Lists player permissions             | Admin, Moderator | ['RunnerPlayer commandSource', 'RunnerPlayer? targetPlayer = null']       | {'targetPlayer': 'null'} |

## Public Methods
| Function Name           | Parameters                                                                | Defaults                 |
|:------------------------|:--------------------------------------------------------------------------|:-------------------------|
|                         |                                                                           |                          |
|                         |                                                                           |                          |
|                         |                                                                           |                          |
| void                    | ['']                                                                      | {}                       |
| AddPermissionCommand    | ['RunnerPlayer commandSource', 'RunnerPlayer player', 'Roles permission'] | {}                       |
| RemovePermissionCommand | ['RunnerPlayer commandSource', 'RunnerPlayer player', 'Roles permission'] | {}                       |
| ClearPermissionCommand  | ['RunnerPlayer commandSource', 'RunnerPlayer player']                     | {}                       |
| ListPermissionCommand   | ['RunnerPlayer commandSource', 'RunnerPlayer? targetPlayer = null']       | {'targetPlayer': 'null'} |