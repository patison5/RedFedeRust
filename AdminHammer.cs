using System.Collections.Generic;
using Oxide.Core.Configuration;
using UnityEngine;
using Oxide.Core;
using System;
using System.Linq;

namespace Oxide.Plugins
{
    [Info("AdminHammer", "mvrb", "1.8.1")]
    class AdminHammer : RustPlugin
    {
        public static AdminHammer plugin;

        private const string permAllow = "adminhammer.allow";
        private bool logToConsole = true;
        private float toolDistance = 200f;
        private string toolUsed = "hammer";
        private string chatCommand = "b";
        private bool showSphere = false;
        private bool performanceMode = false;

        private int layerMask = LayerMask.GetMask("Construction", "Deployed", "Default");

        private readonly DynamicConfigFile dataFile = Interface.Oxide.DataFileSystem.GetFile("AdminHammer");

        private List<ulong> Users = new List<ulong>();

        protected override void LoadDefaultConfig()
        {
            Config["LogToConsole"] = logToConsole = GetConfig("LogToFile", true);
            Config["ShowSphere"] = showSphere = GetConfig("ShowSphere", false);
            Config["ToolDistance"] = toolDistance = GetConfig("ToolDistance", 200f);
            Config["ToolUsed"] = toolUsed = GetConfig("ToolUsed", "hammer");
            Config["ChatCommand"] = chatCommand = GetConfig("ChatCommand", "b");
            Config["PerformanceMode"] = performanceMode = GetConfig("PerformanceMode", false);

            SaveConfig();
        }

        private void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NoAuthorizedPlayers"] = "No authorized players.",
                ["AuthorizedPlayers"] = "Authorized players in the {0} owned by {1}:",
                ["NoEntityFound"] = "No entity found. Look at an entity and right-click while holding a {0}.",
                ["NoOwner"] = "No owner found for this entity.",
                ["ChatEntityOwnedBy"] = "This {0} is owned by {1}",
                ["DoorCode"] = "Door Code: <color=yellow>{0}</color>",
                ["ConsoleEntityOwnedBy"] = "This {0} is owned by www.steamcommunity.com/profiles/{1}",
                ["ToolActivated"] = "You have enabled AdminHammer.",
                ["ToolDeactivated"] = "You have disabled AdminHammer.",
                ["PerformanceMode"] = "Performance mode is enabled, so you have to use the chat command <color=yellow>/{0}</color> instead of right-clicking"
            }, this);
        }

        private void Init()
        {
            plugin = this;

            Users = dataFile.ReadObject<List<ulong>>();

            LoadDefaultConfig();
            permission.RegisterPermission(permAllow, this);

            cmd.AddChatCommand("ah", this, "CmdAdminHammer");
            cmd.AddChatCommand("adminhammer", this, "CmdAdminHammer");
            cmd.AddChatCommand(chatCommand, this, "CmdCheckEntity");
        }

        private void OnServerInitialized()
        {
            if (performanceMode) return;

            foreach (var player in BasePlayer.activePlayerList)
            {
                if (Users.Contains(player.userID))
                {
                    if (player.gameObject.GetComponent<AH>() == null)
                        player.gameObject.AddComponent<AH>();
                }
            }
        }

        private void Unload()
        {
            foreach (var ah in UnityEngine.Object.FindObjectsOfType<AH>().ToList())
                GameObject.Destroy(ah);
        }

        private void OnPlayerInit(BasePlayer player)
        {
            if (performanceMode) return;

            if (Users.Contains(player.userID))
            {
                if (player.gameObject.GetComponent<AH>() == null)
                    player.gameObject.AddComponent<AH>();
            }
        }

        private void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            if (player.gameObject.GetComponent<AH>() != null)
                player.gameObject.GetComponent<AH>().Destroy();
        }

        private void CmdAdminHammer(BasePlayer player)
        {
            if (!permission.UserHasPermission(player.UserIDString, permAllow)) return;

            if (performanceMode)
            {
                player.ChatMessage(Lang("PerformanceMode", player.UserIDString, chatCommand));
                return;
            }

            if (Users.Contains(player.userID))
            {
                Users.Remove(player.userID);

                if (player.gameObject.GetComponent<AH>() != null)
                    player.gameObject.GetComponent<AH>().Destroy();

                player.ChatMessage(Lang("ToolDeactivated", player.UserIDString));
            }
            else
            {
                Users.Add(player.userID);

                if (player.gameObject.GetComponent<AH>() == null)
                    player.gameObject.AddComponent<AH>();

                player.ChatMessage(Lang("ToolActivated", player.UserIDString));
            }

            dataFile.WriteObject(Users);
        }

        private void CmdCheckEntity(BasePlayer player)
        {
            if (!permission.UserHasPermission(player.UserIDString, permAllow)) return;

            CheckEntity(player);
        }

        private void CheckEntity(BasePlayer player)
        {
            RaycastHit hit;
            var raycast = Physics.Raycast(player.eyes.HeadRay(), out hit, toolDistance, layerMask);
            BaseEntity entity = raycast ? hit.GetEntity() : null;

            if (!entity)
            {
                player.ChatMessage(Lang("NoEntityFound", player.UserIDString, toolUsed));
                return;
            }

            if (entity is Door)
            {
                var door = entity as Door;
                var lockSlot = door.GetSlot(BaseEntity.Slot.Lock);

                if (lockSlot is CodeLock)
                {
                    var codeLock = (CodeLock)lockSlot;
                    string msg = Lang("AuthorizedPlayers", player.UserIDString, door.ShortPrefabName, GetName(entity.OwnerID.ToString())) + "\n";

                    int authed = 0;

                    foreach (var user in codeLock.whitelistPlayers)
                    {
                        authed++;
                        msg += $"{authed}. {GetName(user.ToString())}\n";
                    }

                    player.ChatMessage(authed == 0 ? Lang("NoAuthorizedPlayers", player.UserIDString) : msg);
                    player.ChatMessage(Lang("DoorCode", player.UserIDString, (door.GetSlot(BaseEntity.Slot.Lock) as CodeLock)?.code));
                }
                else if (lockSlot is BaseLock)
                {
                    player.ChatMessage(entity.OwnerID == 0 ? Lang("NoOwner", player.UserIDString, entity.ShortPrefabName) : Lang("ChatEntityOwnedBy", player.UserIDString, entity.ShortPrefabName, GetName(entity.OwnerID.ToString())));
                    Puts(entity.OwnerID == 0 ? Lang("NoOwner", player.UserIDString, entity.ShortPrefabName) : Lang("ConsoleEntityOwnedBy", player.UserIDString, entity.ShortPrefabName, entity.OwnerID.ToString()));
                }

            }
            else if (entity is SleepingBag)
            {
                SleepingBag sleepingBag = entity as SleepingBag;

                player.ChatMessage($"This SleepingBag has been assigned to {GetName(sleepingBag.deployerUserID.ToString())} by {GetName(sleepingBag.OwnerID.ToString())}");
            }
            else if (entity is AutoTurret)
            {
                player.ChatMessage(GetAuthorized(entity, player));

                string msg = $"Items in the AutoTurret owned by {GetName(entity.OwnerID.ToString())}:\n";
                foreach (var item in entity.GetComponent<StorageContainer>().inventory.itemList)
                {
                    msg += $"{item.amount}x {item.info.displayName.english}\n";
                }

                player.ChatMessage(msg);
            }
            else if (entity is BuildingPrivlidge)
            {
                player.ChatMessage(GetAuthorized(entity, player));

                BuildingPrivlidge priv = entity as BuildingPrivlidge;
                if (priv != null)
                {
                    float protectedMinutes = priv.GetProtectedMinutes();

                    TimeSpan t = TimeSpan.FromMinutes(protectedMinutes);

                    string formattedTime = string.Format("{0:D2} days {1:D2} hours {2:D2} minutes {3:D2} seconds",
								t.Days,
								t.Hours,
								t.Minutes,
								t.Seconds,
								t.Milliseconds);

                    player.ChatMessage($"The base is protected for {formattedTime}");
                }
            }
            else if (entity is StorageContainer)
            {
                var storageContainer = entity as StorageContainer;
                string msg = $"Items in the {storageContainer.ShortPrefabName} owned by {GetName(storageContainer.OwnerID.ToString())}:\n";
                foreach (var item in storageContainer.inventory.itemList)
                    msg += $"{item.amount}x {item.info.displayName.english}\n";
                player.ChatMessage(msg);
            }
            else
            {
                player.ChatMessage(entity.OwnerID == 0 ? Lang("NoOwner", player.UserIDString, entity.ShortPrefabName) : Lang("ChatEntityOwnedBy", player.UserIDString, entity.ShortPrefabName, GetName(entity.OwnerID.ToString())));
                Puts(entity.OwnerID == 0 ? Lang("NoOwner", player.UserIDString, entity.ShortPrefabName) : Lang("ConsoleEntityOwnedBy", player.UserIDString, entity.ShortPrefabName, entity.OwnerID.ToString()));
            }

            if (showSphere) player.SendConsoleCommand("ddraw.sphere", 2f, Color.blue, entity.CenterPoint(), 1f);
        }

        private class AH : MonoBehaviour
        {
            public BasePlayer player;
            private float lastCheck;

            private void Awake()
            {
                player = GetComponent<BasePlayer>();
                lastCheck = Time.realtimeSinceStartup;
            }

            private void FixedUpdate()
            {
                if (player == null || !player.IsConnected)
                {
                    Destroy();
                    return;
                }

                float currentTime = Time.realtimeSinceStartup;

                if (!player.serverInput.WasJustPressed(BUTTON.FIRE_SECONDARY) || (player.GetActiveItem() as Item)?.info.shortname != plugin.toolUsed) return;

                if (currentTime - lastCheck >= 0.25f)
                {
                    plugin.CheckEntity(player);
                    lastCheck = currentTime;
                }
            }

            public void Destroy()
            {
                Destroy(this);
            }
        }

        private string GetAuthorized(BaseEntity entity, BasePlayer player)
        {
            string msg = Lang("AuthorizedPlayers", player.UserIDString, entity.ShortPrefabName, GetName(entity.OwnerID.ToString())) + "\n";
            var turret = entity as AutoTurret;
            var priv = entity as BuildingPrivlidge;
            int authed = 0;

            foreach (var user in (turret ? turret.authorizedPlayers : priv.authorizedPlayers))
            {
                authed++;
                msg += $"{authed}. {GetName(user.userid.ToString())}\n";
                Puts($"{authed}. {user.userid} {GetName(user.userid.ToString())}\n");
            }

            return authed == 0 ? Lang("NoAuthorizedPlayers", player.UserIDString) : msg;
        }

        private string GetPlayerColor(ulong id) => BasePlayer.FindByID(id) != null ? "green" : "red";

        private string GetName(string id)
        {
            if (id == "0") return "[SERVERSPAWN]";

            string color = GetPlayerColor(ulong.Parse(id));

            return $"<color={color}> {covalence.Players.FindPlayerById(id)?.Name} </color> ({id})";
        }

        private T GetConfig<T>(string name, T value) => Config[name] == null ? value : (T)Convert.ChangeType(Config[name], typeof(T));

        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);
    }
}