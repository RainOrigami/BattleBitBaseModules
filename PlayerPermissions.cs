﻿using BattleBitAPI.Common;
using BBRAPIModules;
using Newtonsoft.Json;
using PlayerFinder;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Permissions
{
    [RequireModule(typeof(PlayerFinder.PlayerFinder))]
    public class PlayerPermissions : BattleBitModule
    {
        private const string PERMISSIONS_FILE = "PlayerPermissions.json";

        private static Dictionary<ulong, Roles> playerRoles = new();

        private PlayerFinder.PlayerFinder playerFinder = null!;

        public PlayerPermissions(RunnerServer server) : base(server)
        {
            if (playerRoles.Count == 0)
            {
                this.loadPermissions();
            }
        }

        public override void OnModulesLoaded()
        {
            this.playerFinder = this.Server.GetModule<PlayerFinder.PlayerFinder>()!;
        }

        private void loadPermissions()
        {
            lock (playerRoles)
            {
                playerRoles.Clear();
                try
                {
                    foreach (KeyValuePair<ulong, Roles> kvp in JsonConvert.DeserializeObject<Dictionary<ulong, Roles>>(File.ReadAllText(PERMISSIONS_FILE)))
                    {
                        playerRoles.Add(kvp.Key, kvp.Value);
                    }
                }
                catch
                {

                }

                if (playerRoles.Count == 0)
                {
                    // Possible missing file, create a new one
                    savePermissions();
                }
            }
        }

        private void savePermissions()
        {
            lock (playerRoles)
            {
                File.WriteAllText(PERMISSIONS_FILE, JsonConvert.SerializeObject(playerRoles));
            }
        }

        public override Task OnConnected()
        {
            //this.Server.GetModule<CommandHandler>()?.Register(this);
            return Task.CompletedTask;
        }

        public override Task OnPlayerConnected(RunnerPlayer player)
        {
            lock (playerRoles)
            {
                if (!playerRoles.ContainsKey(player.SteamID))
                {
                    playerRoles.Add(player.SteamID, Roles.None);
                }
            }

            return Task.CompletedTask;
        }

        public bool HasPlayerRole(ulong steamID, Roles role)
        {
            return (this.GetPlayerRoles(steamID) & role) == role;
        }

        public Roles GetPlayerRoles(ulong steamID)
        {
            lock (playerRoles)
            {
                if (playerRoles.ContainsKey(steamID))
                {
                    return playerRoles[steamID];
                }
            }

            return Roles.None;
        }

        public void SetPlayerRoles(ulong steamID, Roles roles)
        {
            lock (playerRoles)
            {
                if (playerRoles.ContainsKey(steamID))
                {
                    playerRoles[steamID] = roles;
                }
                else
                {
                    playerRoles.Add(steamID, roles);
                }
            }

            this.savePermissions();
        }

        public void AddPlayerRoles(ulong steamID, Roles role)
        {
            this.SetPlayerRoles(steamID, this.GetPlayerRoles(steamID) | role);
        }

        public void RemovePlayerRoles(ulong steamID, Roles role)
        {
            this.SetPlayerRoles(steamID, this.GetPlayerRoles(steamID) & ~role);
        }
    }
}