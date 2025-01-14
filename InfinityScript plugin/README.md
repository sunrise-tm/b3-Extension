# b3 Helper

## Purpose

b3 Helper is a plugin based on InfinityScript version 1.1 for TeknoMW3. The plugin serves the purpose of providing more options to people using b3 only for their servers as their administration script. It has some simple commands like !afk that can not be executed with b3 usually to TeknoMW3 server. It also has some other features for users as well. It is coded in the way that people can expand the possibilities of this plugin in future.

## Setup

1. put `b3helper.dll` in your server's script folder.
2. navigate to your server's `server.cfg` file and open it.
3. At the bottom of configuration file, add `loadscript "b3helper.dll"`
4. Add the following dvar to the configuration file below:

```
//////////////////////////////////////////////////
//Script configuration for b3helper.

//Hides text with prefix "!" and "@". default value: 1, 1 would hide the commands. 0 to disable.
seta sv_hideCommands "1"

//Server motd. Message shown on loading screen.
seta sv_gmotd "^1Welcome to the server. Edit me."

//Force users to have smoke. Does not allow users to remove smoke in game. 1 to enable, 0 to disable.
seta sv_forceSmoke "1"

//Ojective text in menu. Objective text shown in the menu, edit to your liking.
seta sv_objText "^1Welcome to the server. Edit me."

//Client performance dvar. Enable to edit client dvar. 1 to enable, 0 to disable.
seta sv_clientDvars "1"

//Server performance dvar. Enable to allow server dvar editing. 1 to enable, 0 to disable.
seta sv_serverDvars "1"

//Enables Killstreak counter on top of screen. 1 to enable, 0 to disable.
seta sv_killStreakCounter "1"

//Enables HudElem. 1 to enable HudElem, 0 to disable.
seta sv_hudEnable "1"

//Text of Top HudElem. Insert a message.
seta sv_hudTop "^1TOP MESSAGE. CHANGE ME"

//Text of Bottom HudElem. Insert a message.
seta sv_hudBottom "^1BOTTOM MESSAGE. CHANGE ME"

//Text of Right HudElem. Insert a message.
seta sv_hudRight "^1RIGHT MESSAGE. CHANGE ME"

//Text of Left HudElem. Insert a message.
seta sv_hudLeft "^1LEFT MESSAGE. CHANGE ME"

//Enables Scrolling of Bottom Hud. 1 to enable, 0 to disable (Static position).
seta sv_scrollingHud "1"

//Scrolling Speed of Bottom Hud. Default value: 30
seta sv_scrollingSpeed "30"

//Enable clients alias in chat. Default: 1, Eanble: 1, Disable: 0
seta sv_chatAlias "1"

///////////////////////////////////////////
```

## b3 Plugin Setup
Coming soon. You can ask me about setup on discord while this is added.

## Changelogs 

v1.0.0.x
- Initial relase
- Some fixes and features addition

v2.0.0.x
- Connected b3 with IS.
- Command execution based on b3 permission by IS. (Information on discord. setup coming soon.)
- Bug fixes from v1.
- Commands
  1. !afk
  2. !setafk (playername)
  3. !teleport (teleporter) (reciever)
  4. !mode (dsr name)
  5. !gametype (dsr name) (map name)(Unformatted)
  6. !kill (playername)
  7. !suicide
 
v2.0.0.2
- Changed latency to 1200ms from 1000ms to have commands execute everytime.
- Fixed OnSay2 Bug and Errors.
- Added check for player rank before execution of command and prevent execution on higher rank.
- Using Entref instead of Name of entity sent by b3 to InfinityScript.
- Fixed issue in execution of 2 arguments commands due to player name having spaces.
- Player finding function changed for InfinityScript. Now based on Entity Number.
- Commands added:
  1. !blockchat (playername) [Toggle command]*
  2. !ac130 (playername) || all
  3. !freeze (playername) [Toggle command]*
  4. !changeteam (playername)
  
v2.0.0.3
- Instead of using latency, the IS will check for executed b3 command on interval of 50ms.
- Demo recording issue has been removed.
- rate increased from 60000 (6kbps upload) to 210000 (21kbps upload).
- Freeze command fixed where user would get unfreezed after respawning causing script execution issue.

v2.0.0.4
- OnSay2 has been changed to OnSay3 so clients still can team message even if they are using alias (custom name)
- Clients alias will be stored in 'scripts\ClientsAlias.txt', format of stored data is '[Player HWID]=[Player Alias]'
- Commands added:
  1. !wallhack (playername) || all [Enabled / Disable]*
  2. !aimbot (playername) || all [Enabled / Disable]*
  3. !norecoil (playername) || all [Enabled / Disable]*
  4. !godmode (playername) || all [Enabled / Disable]*
  5. !invisible (playername) || all [Enabled / Disable]*
  6. !unlimiteammo (playername) || all [Enabled / Disable]*
  7. !fly (playername)
  8. !setalias (playername) (alias)
  9. !balance
  
*Toggle command = Enables on first execution and same command is used for disabling.
*Enabled / Disable = specified value to set command status, can be (1, on, true, enable)

## Contributing to the project

You can contribute to this project, feel free to fork the project and push your code and I will have a review and commit to master.

## Contact

You can contact us and get more information about this plugin on Discord [HERE](https://discord.gg/hS3Drg6). 
Discord: Musta#6382 & SunRise#3428
