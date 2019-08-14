using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using InfinityScript;

namespace b3helper
{
    public class b3helper : BaseScript
    {
        //HudElem
        private static HudElem[] KillStreakHud = new HudElem[18];
        private static HudElem[] NoKillsHudElem = new HudElem[18];

        //Dictionary for players with alias (custom name)
        private static Dictionary<string, string> clientsAlias = new Dictionary<string, string>();

        //Hud for Information
        private HudElem top;
        private HudElem bottom;
        private HudElem right;
        private HudElem left;

        //Mode
        volatile string MapRotation = "";


        public b3helper()
        {
            Log.Info("b3Extension plugin by Musta#6382 and Pickle Rick#5230 and SunRise#3428.");

            //Making and Settings dvars if they are unused and have value.
            Call("setDvarifUninitialized", "sv_hideCommands", "1"); //Done
            Call("setDvarifUninitialized", "sv_gmotd", "^:Welcome to the server."); //Done
            Call("setDvarifUninitialized", "sv_forceSmoke", "1"); //Done
            Call("setDvarifUninitialized", "sv_objText", "^1This is menu text."); //Done
            Call("setDvarifUninitialized", "sv_clientDvars", "1"); //Done
            Call("setDvarifUninitialized", "sv_rate", "210000");
            Call("setDvarifUninitialized", "sv_serverDvars", "1"); //Done
            Call("setDvarifUninitialized", "sv_killStreakCounter", "1"); //Done
            Call("setDvarifUninitialized", "sv_hudEnable", "1"); //Dome
            Call("setDvarifUninitialized", "sv_hudTop", "^1TOP Message"); //Done
            Call("setDvarifUninitialized", "sv_hudBottom", "^1Bottom Message"); //Done
            Call("setDvarifUninitialized", "sv_hudRight", "^1Right Message"); //Done
            Call("setDvarifUninitialized", "sv_hudLeft", "^1Left Message"); //Done
            Call("setDvarifUninitialized", "sv_scrollingSpeed", "30"); //Done
            Call("setDvarifUninitialized", "sv_scrollingHud", "1"); //Done
            Call("setDvarifUninitialized", "sv_b3Execute", "null"); //Done
            Call("setDvarifUninitialized", "sv_chatAlias", "1"); //Done

            //Loading Server Dvars.
            ServerDvars();

            //HudElem For Information
            InformationHuds();

            //Load players alias
            LoadClientsAlias();

            //Assigning things.
            PlayerConnected += OnPlayerConnect;

            OnInterval(50, () =>
            {
                if (Call<string>("getDvar", "sv_b3Execute") != "null")
                {
                    string content = Call<string>("getDvar", "sv_b3Execute");
                    ProcessCommand(content);
                    Call("setDvar", "sv_b3Execute", "null");
                }
                return true;
            });

            OnInterval(1, () =>
            {
                if (Players.Count > 0)
                {
                    foreach (var entity in Players)
                    {
                        if (entity == null || entity.IsAlive) continue;
                        if (entity.HasField("PlayerUsingNorecoil") && entity.GetField<int>("PlayerUsingNorecoil") == 1)
                            entity.Call("recoilscaleon", 0);
                        if (entity.HasField("PlayerUsingWallhack") && entity.GetField<int>("PlayerUsingWallhack") == 1)
                            entity.Call("thermalvisionfofoverlayon", true);
                        if (entity.HasField("PlayerUsingAimbot") && entity.GetField<int>("PlayerUsingAimbot") == 1)
                            Aimbot(entity);
                    }
                }
                return true;
            });
        }


        public void ServerDvars()
        {
            if (Call<int>("getDvarInt", "sv_serverDvars") != 0)
            {
                Function.Call("setdevDvar", "sv_network_fps", 200);
                Function.Call("setDvar", "sv_hugeSnapshotSize", 10000);
                Function.Call("setDvar", "sv_hugeSnapshotDelay", 100);
                Function.Call("setDvar", "sv_pingDegradation", 0);
                Function.Call("setDvar", "sv_pingDegradationLimit", 9999);
                Function.Call("setDvar", "sv_acceptableRateThrottle", 9999);
                Function.Call("setDvar", "sv_newRateThrottling", 2);
                Function.Call("setDvar", "sv_minPingClamp", 50);
                Function.Call("setDvar", "sv_cumulThinkTime", 1000);
                Function.Call("setDvar", "sys_lockThreads", "all");
                Function.Call("setDvar", "com_maxFrameTime", 1000);
                Function.Call("setDvar", "com_maxFps", 0);
                Function.Call("setDvar", "sv_voiceQuality", 9);
                Function.Call("setDvar", "maxVoicePacketsPerSec", 1000);
                Function.Call("setDvar", "maxVoicePacketsPerSecForServer", 200);
                Function.Call("setDvar", "cg_everyoneHearsEveryone", 1);
                Function.Call("makedvarserverinfo", "motd", Call<string>("getDvar", "sv_gmotd"));
                Function.Call("makedvarserverinfo", "didyouknow", Call<string>("getDvar", "sv_gmotd"));
            }
        }

        public void LoadClientsAlias()
        {
            if (Call<int>("getDvarInt", "sv_chatAlias") == 1)
            {
                if (!File.Exists("scripts\\ClientsAlias.txt")) File.WriteAllLines("scripts\\ClientsAlias.txt", new string[0]);
                string[] lines = File.ReadAllLines("scripts\\ClientsAlias.txt");
                foreach(var line in lines)
                {
                    if (line.StartsWith("//") || !line.Contains("=") || string.IsNullOrWhiteSpace(line)) continue;
                    string[] args = line.Split('=');
                    clientsAlias[args[0]] = string.Join("=", args.Skip(1));
                }
            }
        }

        public void SaveClientsAlias()
        {
            if (clientsAlias == null) return;
            File.WriteAllLines("scripts\\ClientsAlias.txt", new string[0]);
            string[] toWrite = new string[255];
            int index = 0;
            foreach (var kvp in clientsAlias)
            {
                toWrite[index] = $"{kvp.Key}={kvp.Value}";
                index++;
            }
            File.WriteAllLines("scripts\\ClientsAlias.txt", toWrite);
        }

        public void OnPlayerConnect(Entity player)
        {
            //Reseting killstreak on player connect
            player.SetField("playerKillStreak", 0);

            // Settings player fields 
            player.SetField("PlayerUsingAimbot", 0);
            player.SetField("PlayerUsingNorecoil", 0);
            player.SetField("PlayerUsingWallhack", 0);
            player.SetField("PlayerHasUnlimiteAmmo", 0);
            player.SetField("PlayerIsInvisible", 0);
            player.SetField("PlayerIsFlying", 0);
            player.SetField("PlayerHasUnlimiteHealth", 0);
            //player.SetField("AimbotWhiteList", ""); // I'll add it later

            //Client Performance dvar
            if (Call<int>("getDvarInt", "sv_clientDvars") != 0)
            {
                player.SetClientDvar("cg_objectiveText", Call<String>("getDvar", "sv_objText"));
                player.SetClientDvar("sys_lockThreads", "all");
                player.SetClientDvar("com_maxFrameTime", "1000");
                player.SetClientDvar("rate ", Call<string>("getDvar", "sv_rate"));
                player.SpawnedPlayer += () =>
                {
                    player.SetClientDvar("cg_objectiveText", Call<String>("getDvar", "sv_objText"));
                };
            }
            if (Call<int>("getDvarInt", "sv_forceSmoke") != 0)
            {
                player.SetClientDvar("fx_draw", "1");
            }

            //Killstreak Related Code
            var killstreakHud = HudElem.CreateFontString(player, "hudsmall", 0.8f);
            killstreakHud?.SetPoint("TOP", "TOP", -9, 2);
            killstreakHud?.SetText("^5Killstreak: ");
            killstreakHud.HideWhenInMenu = true;

            var noKills = HudElem.CreateFontString(player, "hudsmall", 0.8f);
            noKills?.SetPoint("TOP", "TOP", 39, 2);
            noKills?.SetText("^20");
            noKills.HideWhenInMenu = true;

            KillStreakHud[GetEntityNumber(player)] = killstreakHud;
            NoKillsHudElem[GetEntityNumber(player)] = noKills;
            
            player.SpawnedPlayer += () =>
            {
                if (player.HasField("frozen"))
                {
                    if (player.GetField<int>("frozen") == 1)
                    {
                        player.Call("freezecontrols", true);
                    }
                }
            };
            player.OnNotify("giveloadout", delegate (Entity entity)
            {
                if (entity.HasField("frozen"))
                {
                    if (entity.GetField<int>("frozen") == 1)
                    {
                        entity.Call("freezecontrols", true);
                    }
                }
            });
            player.OnNotify("weapon_fired", new Action<Entity, Parameter>((entity, args) =>
            {
                if (entity.HasField("PlayerHasUnlimiteAmmo") && entity.GetField<int>("PlayerHasUnlimiteAmmo") == 1)
                {
                    entity.Call("setweaponammoclip", entity.CurrentWeapon, 999);
                    entity.Call("giveMaxAmmo", entity.CurrentWeapon);
                }
            }));
        }


        public override void OnPlayerKilled(Entity player, Entity inflictor, Entity attacker, int damage, string mod, string weapon, Vector3 dir, string hitLoc)
        {
            if (!player.HasField("playerKillStreak") || !attacker.HasField("playerKillStreak"))
                return;
            try
            {
                if (player != attacker) //Suicide Alert!
                {
                    attacker.SetField("playerKillStreak", attacker.GetField<int>("playerKillStreak") + 1);
                }
                player.SetField("playerKillStreak", 0);
                var attackerNoKills = NoKillsHudElem[GetEntityNumber(attacker)];
                if (attackerNoKills == null)
                {
                    throw new Exception("AttackerNoKills is null. Attacker: " + attacker.Name);
                }
                attackerNoKills.SetText("^2" + attacker.GetField<int>("playerKillStreak"));
                NoKillsHudElem[GetEntityNumber(attacker)] = attackerNoKills;

                var victimNoKills = NoKillsHudElem[GetEntityNumber(player)];
                if (victimNoKills == null)
                {
                    throw new Exception("VictimNoKills is null. Victim: " + player.Name);
                }
                victimNoKills.SetText("0");
                NoKillsHudElem[GetEntityNumber(player)] = victimNoKills;
            }
            catch (Exception ex)
            {
                Log.Error("Error in Killstreak: " + ex.Message + ex.StackTrace);
                return;
            }

        }

        public override void OnPlayerDamage(Entity player, Entity inflictor, Entity attacker, int damage, int dFlags, string mod, string weapon, Vector3 point, Vector3 dir, string hitLoc)
        {
            if (player == null) return;
            if (player.HasField("PlayerHasUnlimiteHealth") && player.GetField<int>("PlayerHasUnlimiteHealth") == 1 && player.IsAlive)
                player.Health += damage;
        }


        public override EventEat OnSay3(Entity player, ChatType type, string name, ref string message)
        {
            try
            {
                message = message.ToLower();
                if ((message.StartsWith("!")) || (message.StartsWith("@")))
                {
                    if (Call<int>("getDvarInt", "sv_hideCommands") != 0)
                        return EventEat.EatGame;

                }
                if (player.HasField("muted") && player.GetField<int>("muted") == 1)
                {
                    return EventEat.EatGame;
                }

                if (clientsAlias.TryGetValue(player.HWID, out string alias))
                {
                    string toSend = $"{alias}^7: {message}";

                    if (player.GetField<string>("sessionteam") == "spectator")
                        toSend = $"^7(Spectator){toSend}";
                    else if (!player.IsAlive)
                        toSend = $"^7(Dead){toSend}";

                    if (type == ChatType.Team)
                        foreach (var teammate in Players.Where(x => x.GetField<string>("sessionteam") == player.GetField<string>("sessionteam")))
                            Utilities.RawSayTo(teammate, toSend);
                    else
                        Utilities.RawSayAll(toSend);

                    return EventEat.EatGame;
                }
            }
            catch (Exception)
            {
            }
            return EventEat.EatNone;
        }


        public void InformationHuds()
        {
            if (Call<int>("getDvarInt", "sv_hudEnable") != 0)
            {
                if (Call<string>("getDvar", "sv_hudTop") != "null")
                {
                    top = HudElem.CreateServerFontString("hudbig", 0.5f);
                    top.SetPoint("TOPCENTER", "TOPCENTER", 0, 15);
                    top.HideWhenInMenu = true;
                    top.SetText(Call<string>("getDvar", "sv_hudTop"));
                }
                if (Call<string>("getDvar", "sv_hudRight") != "null")
                {
                    right = HudElem.CreateServerFontString("hudbig", 0.5f);
                    right.SetPoint("TOPRIGHT", "TOPRIGHT", -5, 5);
                    right.HideWhenInMenu = true;
                    right.SetText(Call<string>("getDvar", "sv_hudRight"));
                }
                if (Call<string>("getDvar", "sv_hudRight") != "null")
                {
                    left = HudElem.CreateServerFontString("hudbig", 0.5f);
                    left.SetPoint("TOPLEFT", "TOPLEFT", 6, 105);
                    left.HideWhenInMenu = true;
                    left.SetText(Call<string>("getDvar", "sv_hudLeft"));
                }
                if ((Call<string>("getDvar", "sv_hudBottom") != "null") && (Call<int>("getDvarInt", "sv_scrollingHud") != 0) && (Call<int>("getDvarInt", "sv_scrollingSpeed") != 0))
                {
                    bottom = HudElem.CreateServerFontString("hudbig", 0.4f);
                    bottom.SetPoint("CENTER", "BOTTOM", 0, -5);
                    bottom.Foreground = true;
                    bottom.HideWhenInMenu = true;
                    OnInterval(30000, () =>
                    {
                        bottom.SetText(Call<string>("getDvar", "sv_hudBottom"));
                        bottom.SetPoint("CENTER", "BOTTOM", 1100, -5);
                        bottom.Call("moveovertime", Call<int>("getDvarInt", "sv_scrollingSpeed"));
                        bottom.X = -700f;
                        return true;
                    });

                }
                else if (Call<string>("getDvar", "sv_hudBottom") != "null")
                {
                    bottom = HudElem.CreateServerFontString("hudbig", 0.5f);
                    bottom.SetPoint("BOTTOMCENTER", "BOTTOMCENTER", 0, -5);
                    bottom.HideWhenInMenu = true;
                    bottom.SetText(Call<string>("getDvar", "sv_hudBottom"));
                }
            }

        }

        public void ProcessCommand(string message)
        {
            try
            {
                string[] msg = message.Split(' ');
                msg[0] = msg[0].ToLowerInvariant();
                if (msg[0].StartsWith("!afk"))
                {
                    Entity player = GetPlayer(msg[1]);
                    ChangeTeam(player, "spectator");
                }
                if (msg[0].StartsWith("!setafk"))
                {
                    Entity target = GetPlayer(msg[1]);
                    ChangeTeam(target, "spectator");
                }
                if (msg[0].StartsWith("!kill"))
                {
                    Entity target = GetPlayer(msg[1]);
                    AfterDelay(50, () => target.Call("suicide"));
                }
                if (msg[0].StartsWith("!suicide"))
                {
                    Entity player = GetPlayer(msg[1]);
                    AfterDelay(50, () =>
                    {
                        if (player.IsAlive) player.Call("suicide");
                    });
                }
                if (msg[0].StartsWith("!teleport"))
                {
                    Entity teleporter = GetPlayer(msg[1]);
                    Entity reciever = GetPlayer(msg[2]);

                    teleporter.Call("setOrigin", reciever.Origin);
                }
                if (msg[0].StartsWith("!mode"))
                {
                    if (!System.IO.File.Exists($@"admin\{msg[1]}.dsr") && !System.IO.File.Exists($@"players2\{msg[1]}.dsr"))
                    {
                        Utilities.RawSayAll("^1DSR not found.");
                        return;
                    }
                    Mode(msg[1]);
                }
                if (msg[0].StartsWith("!gametype"))
                {
                    if (!System.IO.File.Exists($@"admin\{msg[1]}.dsr") && !System.IO.File.Exists($@"players2\{msg[1]}.dsr"))
                    {
                        Utilities.RawSayAll("^1DSR not found.");
                        return;
                    }
                    string newMap = msg[2];
                    Mode(msg[1], newMap);

                }
                if (msg[0].StartsWith("!ac130"))
                {
                    if (msg[1].StartsWith("*all*"))
                    {
                        AC130All();
                    }
                    else
                    {
                        Entity player = GetPlayer(msg[1]);
                        AfterDelay(500, () =>
                        {
                            player.TakeAllWeapons();
                            player.GiveWeapon("ac130_105mm_mp");
                            player.GiveWeapon("ac130_40mm_mp");
                            player.GiveWeapon("ac130_25mm_mp");
                            player.SwitchToWeaponImmediate("ac130_25mm_mp");
                        });
                    }

                }
                if (msg[0].StartsWith("!blockchat"))
                {
                    Entity player = GetPlayer(msg[1]);
                    if (!player.HasField("muted"))
                    {
                        player.SetField("muted", 0);
                    }
                    if (player.GetField<int>("muted") == 1)
                    {
                        player.SetField("muted", 0);
                        Utilities.RawSayAll($"^1{player.Name} chat has been unblocked.");
                    }
                    else if (player.GetField<int>("muted") == 0)
                    {
                        player.SetField("muted", 1);
                        Utilities.RawSayAll($"^1{player.Name} chat has been blocked.");
                    }
                }
                if (msg[0].StartsWith("!freeze"))
                {
                    Entity player = GetPlayer(msg[1]);
                    if (!player.HasField("frozen"))
                    {
                        player.SetField("frozen", 0);
                    }
                    if (player.GetField<int>("frozen") == 1)
                    {
                        player.Call("freezecontrols", false);
                        player.SetField("frozen", 0);
                        Utilities.RawSayAll($"^1{player.Name} has been unfrozen.");
                    }
                    else if (player.GetField<int>("frozen") == 0)
                    {
                        player.Call("freezecontrols", true);
                        player.SetField("frozen", 1);
                        Utilities.RawSayAll($"^1{player.Name} has been frozen.");
                    }
                }
                if (msg[0].StartsWith("!changeteam"))
                {
                    Entity player = GetPlayer(msg[1]);
                    string playerteam = player.GetField<string>("sessionteam");

                    switch (playerteam)
                    {
                        case "axis":
                            ChangeTeam(player, "allies");
                            break;
                        case "allies":
                            ChangeTeam(player, "axis");
                            break;
                        case "spectator":
                            Utilities.RawSayAll($"^1{player.Name} team can't be changed because he is already spectator.");
                            break;
                    }
                }
                if (msg[0].StartsWith("!fly"))
                {
                    Entity player = GetPlayer(msg[1]);
                    if (player.GetField<int>("PlayerIsFlying") == 0)
                    {
                        player.SetField("PlayerIsFlying", 1);
                        player.Call("allowspectateteam", "freelook", true);
                        player.SetField("sessionstate", "spectator");
                        player.Call("setcontents", 0);
                        //Utilities.RawSayAll($"^1{player.Name} ^7has ^2started ^3flying.");
                    }
                    else
                    {
                        player.SetField("PlayerIsFlying", 0);
                        player.Call("allowspectateteam", "freelook", false);
                        player.SetField("sessionstate", "playing");
                        player.Call("setcontents", 1000);
                        //Utilities.RawSayAll($"^1{player.Name} ^7has ^1stopped ^3flying.");
                    }
                }
                if (msg[0].StartsWith("!invisible"))
                {
                    if (msg[1] != "*all*")
                    {
                        Entity player = GetPlayer(msg[1]);
                        //Utilities.RawSayAll($"^1{player.Name} ^7is now {(status == 1 ? "^1invisible" : "^2visible")} ^7to others.");
                        if (player.GetField<int>("PlayerIsInvisible") == 0)
                        {
                            player.SetField("PlayerIsInvisible", 1);
                            player.Call("hide");
                        }
                        else
                        {
                            player.SetField("PlayerIsInvisible", 0);
                            player.Call("show");
                        }
                    }
                    else
                    {
                        bool status = BooleanFromString(msg[2]);
                        foreach (var player in Players)
                        {
                            if (status)
                            {
                                player.SetField("PlayerIsInvisible", 1);
                                player.Call("hide");
                            }
                            else
                            {
                                player.SetField("PlayerIsInvisible", 0);
                                player.Call("show");
                            }
                        }
                        //Utilities.RawSayAll($"^1everyone ^7are now {(status ? "^1invisible" : "^2visible")}");
                    }
                }
                if (msg[0].StartsWith("!unlimiteammo"))
                {
                    if (msg[1] != "*all*")
                    {
                        Entity player = GetPlayer(msg[1]);
                        if (player.GetField<int>("PlayerHasUnlimiteAmmo") == 0)
                            player.SetField("PlayerHasUnlimiteAmmo", 1);
                        else
                            player.SetField("PlayerHasUnlimiteAmmo", 0);
                        //Utilities.RawSayAll($"^3Unlimite Ammo ^7has been {(status == 1 ? "^1disabled" : "^2enabled")} ^7for ^1{player.Name}");
                    }
                    else
                    {
                        bool status = BooleanFromString(msg[2]);
                        foreach (var player in Players)
                        {
                            if (status)
                                player.SetField("PlayerHasUnlimiteAmmo", 1);
                            else
                                player.SetField("PlayerHasUnlimiteAmmo", 0);
                        }
                        //Utilities.RawSayAll($"^3Unlimite Ammo ^7has been {(status ? "^1disabled" : "^2enabled")} ^7for ^3everyone");
                    }
                }
                if (msg[0].StartsWith("!norecoil"))
                {
                    if (msg[1] != "*all*")
                    {
                        Entity player = GetPlayer(msg[1]);
                        //Utilities.RawSayAll($"^3NoRecoil ^7has been {(status == 1 ? "^1disabled" : "^2enabled")} ^7for ^1{player.Name}");
                        if (player.GetField<int>("PlayerUsingNorecoil") == 0)
                            player.SetField("PlayerUsingNorecoil", 1);
                        else
                            player.SetField("PlayerUsingNorecoil", 0);
                    }
                    else
                    {
                        bool status = BooleanFromString(msg[2]);
                        foreach (var player in Players)
                        {
                            if (status)
                                player.SetField("PlayerUsingNorecoil", 1);
                            else
                                player.SetField("PlayerUsingNorecoil", 0);
                        }
                        //Utilities.RawSayAll($"^3NoRecoil ^7has been {(status ? "^1disabled" : "^2enabled")} ^7for ^3everyone");
                    }
                }
                if (msg[0].StartsWith("!wallhack"))
                {
                    if (msg[1] != "*all*")
                    {
                        Entity player = GetPlayer(msg[1]);
                        //Utilities.RawSayAll($"^3Wallhack ^7has been {(status == 1 ? "^1disabled" : "^2enabled")} ^7for ^1{player.Name}");
                        if (player.GetField<int>("PlayerUsingWallhack") == 0)
                        {
                            player.SetField("PlayerUsingNorecoil", 1);
                            player.Call("thermalvisionfofoverlayon", true);
                        }
                        else
                        {
                            player.SetField("PlayerUsingNorecoil", 0);
                            player.Call("thermalvisionfofoverlayoff", true);
                        }                           
                    }
                    else
                    {
                        bool status = BooleanFromString(msg[2]);
                        foreach (var player in Players)
                        {
                            if (status)
                            {
                                player.SetField("PlayerUsingNorecoil", 1);
                                player.Call("thermalvisionfofoverlayon", true);
                            }
                            else
                            {
                                player.SetField("PlayerUsingNorecoil", 0);
                                player.Call("thermalvisionfofoverlayoff", true);
                            }
                        }
                        //Utilities.RawSayAll($"^3Wallhack ^7has been {(status ? "^1disabled" : "^2enabled")} ^7for ^3everyone");
                    }
                }

                if (msg[0].StartsWith("!aimbot"))
                {
                    if (msg[1] != "*all*")
                    {
                        Entity player = GetPlayer(msg[1]);
                        //Utilities.RawSayAll($"^3Aimbot ^7has been {(status == 1 ? "^1disabled" : "^2enabled")} ^7for ^1{player.Name}");
                        if (player.GetField<int>("PlayerUsingAimbot") == 0)
                            player.SetField("PlayerUsingAimbot", 1);
                        else
                            player.SetField("PlayerUsingAimbot", 0);
                    }
                    else
                    {
                        bool status = BooleanFromString(msg[2]);
                        foreach (var player in Players)
                        {
                            if (status)
                                player.SetField("PlayerUsingAimbot", 1);
                            else
                                player.SetField("PlayerUsingAimbot", 0);
                        }
                        //Utilities.RawSayAll($"^3Aimbot ^7has been {(status ? "^1disabled" : "^2enabled")} ^7for ^3everyone");

                    }
                }
                if (msg[0].StartsWith("!godmode"))
                {
                    if (msg[1] != "*all*")
                    {
                        Entity player = GetPlayer(msg[1]);
                        //Utilities.RawSayAll($"^3God Mode ^7has been {(status == 1 ? "^1disabled" : "^2enabled")} ^7for ^1{player.Name}");
                        if (player.GetField<int>("PlayerHasUnlimiteHealth") == 0)
                            player.SetField("PlayerHasUnlimiteHealth", 1);
                        else
                            player.SetField("PlayerHasUnlimiteHealth", 0);
                    }
                    else
                    {
                        bool status = BooleanFromString(msg[2]);
                        foreach (var player in Players)
                        {
                            if (status)
                                player.SetField("PlayerHasUnlimiteHealth", 1);
                            else
                                player.SetField("PlayerHasUnlimiteHealth", 0);
                        }
                        //Utilities.RawSayAll($"^3God Mode ^7has been {(status ? "^1disabled" : "^2enabled")} ^7for ^3everyone");
                    }
                }
                if (msg[0].StartsWith("!balance"))
                {
                    if(Call<string>("getdvar", "g_gametype") == "infect" || Call<string>("getdvar", "g_gametype") == "inf")
                    {
                        //Utilities.RawSayAll("^1Can't balance teams in infected games.");
                        return;
                    }
                    BalanceTeams();
                }
                if (msg[0].StartsWith("!setalias"))
                {
                    Entity player = GetPlayer(msg[1]);
                    if (msg.Length > 2 && msg[2] != "<!DEF>" && !string.IsNullOrWhiteSpace(msg[2]))
                    {
                        string newAlias = string.Join(" ", msg.Skip(2));
                        clientsAlias[player.HWID] = newAlias;
                        //Utilities.RawSayAll($"^1{player.Name}^7's alias has been set to {newAlias}");
                    }
                    else
                    {
                        clientsAlias.Remove(player.HWID);
                        //Utilities.RawSayAll($"^1{player.Name}^7's alias has been ^2reseted");
                    }
                    SaveClientsAlias();
                }
            }
            catch (Exception e)
            {
                Log.Error("Error in Command Processing. Error:" + e.Message + e.StackTrace);
            }
        }

        public bool BooleanFromString(string str)
        {
            str = str.Trim().ToLower();
            return str == "on" || str == "1" || str == "enable" || str == "true";
        }

        public void Aimbot(Entity player)
        {
            if (player == null || !player.IsAlive)
                return;

            Entity targetEnt = null;

            foreach (Entity entity in Players)
            {
                if (!entity.IsAlive)
                    continue;

                if (player.EntRef == entity.EntRef)
                    continue;

                if (player.GetField<string>("sessionteam") == entity.GetField<string>("sessionteam"))
                    if (Call<string>("getdvar", "g_gametype") != "dm")
                        continue;

                if (Call<int>("sighttracepassedint", player.Call<Vector3>("gettagorigin", "j_head"), entity.Call<Vector3>("gettagorigin", "j_head")) != 1
                || entity.GetField<int>("PlayerHasUnlimiteHealth") == 1 || entity.GetField<int>("PlayerIsFlying") == 1)
                    continue;

                if (targetEnt != null)
                {
                    if (Call<bool>("closer",
                        player.Call<Vector3>("gettagorigin", "j_head"), entity.Call<Vector3>("gettagorigin", "j_head"), targetEnt.Call<Vector3>("gettagorigin", "j_head")))
                        targetEnt = entity;
                }
                else
                {
                    targetEnt = entity;
                }
            }

            if (targetEnt != null && targetEnt.IsAlive)
                player.Call("setplayerangles", Call<Vector3>("vectortoangles", (targetEnt.Call<Vector3>("gettagorigin", "j_mainroot") - player.Call<Vector3>("gettagorigin", "tag_weapon_right"))));
        }

        public void BalanceTeams()
        {
            List<Entity> axis = new List<Entity>();
            List<Entity> allies = new List<Entity>();

            foreach (var client in Players)
            {
                switch (client.GetField<string>("sessionteam"))
                {
                    case "allies":
                        allies.Add(client);
                        break;

                    case "axis":
                        axis.Add(client);
                        break;
                    default:
                        //nothing
                        break;
                }
            }

            int difference = (int)Math.Floor(Math.Abs(axis.Count - allies.Count) / 2d);

            if (difference > 0)
            {
                IEnumerable<Entity> tobebalanced;

                if (axis.Count > allies.Count)
                    tobebalanced = axis.OrderBy(ent => ent.IsAlive ? 1 : 0).Take(difference).ToList();
                else
                    tobebalanced = allies.OrderBy(ent => ent.IsAlive ? 1 : 0).Take(difference).ToList();

                foreach (var player in tobebalanced)
                {
                    string team = player.GetField<string>("sessionteam");
                    if (team == "axis")
                        ChangeTeam(player, "allies");
                    else if (team == "allies")
                        ChangeTeam(player, "axis");
                    else
                        continue;
                    //Utilities.RawSayAll($"^1{player.Name} ^7has been ^2balanced^7.");
                }
            }
        }

        public void AC130All()
        {
            foreach (Entity player in Players)
            {
                player.TakeAllWeapons();
                player.GiveWeapon("ac130_105mm_mp");
                player.GiveWeapon("ac130_40mm_mp");
                player.GiveWeapon("ac130_25mm_mp");
                player.SwitchToWeaponImmediate("ac130_25mm_mp");
            }
        }

        public static int GetEntityNumber(Entity player)
        {
            return player.Call<int>("getentitynumber");
        }

        public void Mode(string dsrname, string map = "")
        {
            if (string.IsNullOrWhiteSpace(map))
                map = Call<string>("getDvar", "mapname");

            if (!string.IsNullOrWhiteSpace(MapRotation))
            {
                Log.Error("ERROR: Modechange already in progress.");
                return;
            }

            map = map.Replace("default:", "");
            using (System.IO.StreamWriter DSPLStream = new System.IO.StreamWriter("players2\\EX.dspl"))
            {
                DSPLStream.WriteLine(map + "," + dsrname + ",1000");
            }
            MapRotation = Call<string>("getDvar", "sv_maprotation");
            OnExitLevel();
            Utilities.ExecuteCommand("sv_maprotation EX");
            Utilities.ExecuteCommand("map_rotate");
            Utilities.ExecuteCommand("sv_maprotation " + MapRotation);
            MapRotation = "";
        }

        public Entity GetPlayer(string entref)
        {
            foreach (Entity player in Players)
            {
                if (player.EntRef.ToString() == entref)
                {
                    return player;
                }
            }
            return null;
        }

        public void ChangeTeam(Entity player, string team)
        {
            player.SetField("sessionteam", team);
            player.Notify("menuresponse", "team_marinesopfor", team);
        }
    }
}
