# 1 Modules in CommandHandler.cs

| Description                                | Version   |
|:-------------------------------------------|:----------|
| Basic in-game chat command handler library | 1.0.0     |

## Commands
| Command   | Function Name   | Description                       | Allowed Roles   | Parameters                                | Defaults      |
|:----------|:----------------|:----------------------------------|:----------------|:------------------------------------------|:--------------|
| help      | void            | Shows this help message           |                 | ['RunnerPlayer player', 'int page = 1']   | {'page': '1'} |
| cmdhelp   | void            | Shows help for a specific command |                 | ['RunnerPlayer player', 'string command'] | {}            |
| modules   | void            | Lists all loaded modules          | Admin           | ['RunnerPlayer commandSource']            | {}            |

## Public Methods
| Function Name      | Parameters                                                       | Defaults      |
|:-------------------|:-----------------------------------------------------------------|:--------------|
|                    |                                                                  |               |
|                    |                                                                  |               |
|                    |                                                                  |               |
|                    |                                                                  |               |
|                    |                                                                  |               |
|                    |                                                                  |               |
|                    |                                                                  |               |
| void               | ['']                                                             | {}            |
| Register           | ['BattleBitModule module']                                       | {}            |
| Task               | ['RunnerPlayer player', 'ChatChannel channel', 'string message'] | {}            |
| HelpCommand        | ['RunnerPlayer player', 'int page = 1']                          | {'page': '1'} |
| CommandHelpCommand | ['RunnerPlayer player', 'string command']                        | {}            |
| ListModules        | ['RunnerPlayer commandSource']                                   | {}            |
|                    |                                                                  |               |
|                    |                                                                  |               |
|                    |                                                                  |               |
|                    |                                                                  |               |
|                    |                                                                  |               |