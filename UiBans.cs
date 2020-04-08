using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Game.Rust.Cui;
using Rust;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Oxide.Plugins
{
    [Info("UiBans", "RustPlugin.ru", "0.0.1")]
    class UiBans : RustPlugin
    {
        #region FIELDS
        class Players
        {
            public Dictionary<ulong, BansPlayers> PlayersBanned = new Dictionary<ulong, BansPlayers>();
            public Players() { }
        }

        class BansPlayers
        {
            public string Name;
            public string Date;
            public string Reason;
        }

        Players banplayers;
        private const string BAN_PERM = "UiBans.ban";
        #endregion

        #region Commands

        [ConsoleCommand("uiban")]
        void cmdConsoleBan(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null) return;
            if (arg.Args == null || arg.Args.Length < 1)
            {
                Puts("Используйте uiban NAME/STEAMID");
                return;
            }
            var uid = string.Format(arg.Args[0]);
            BasePlayer target = FindBasePlayer(uid);
            if (target == null)
            {
                Puts("Игрок не найден");
                return;
            }
            if (!banplayers.PlayersBanned.ContainsKey(target.userID))
            {
                banplayers.PlayersBanned.Add(target.userID, new BansPlayers()
                {
                    Name = target.displayName,
                    Date = DateTime.UtcNow.Date.ToString("MM.dd.yyyy"),
                    Reason = "Banned by Console"
                });

                target.inventory.containerBelt.Clear();
                target.inventory.containerMain.Clear();
                target.inventory.containerWear.Clear();
                DrawUI(target);
                Puts(Messages["banPermanent"], $"{target.userID}/{target.displayName}", "Banned by Console");
            }
            else
            {
                Puts(Messages["IsBanned"]);
            }

        }

        [ChatCommand("UiBan")]
        void cmdChatBan(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, BAN_PERM))
            {
                SendReply(player, Messages["nPermission"]);
                return;
            }
            if (args.Length < 2 || args.Length == null)
            {
                SendReply(player, Messages["Args"]);
                return;
            }
            var nameOrId = args[0];

            BasePlayer target = FindBasePlayer(nameOrId);

            if (target == null)
            {
                SendReply(player, Messages["NFound"]);
                return;
            }

            if (!banplayers.PlayersBanned.ContainsKey(target.userID))
            {
                string Msg = string.Join(" ", args.Skip(1).ToArray());
                banplayers.PlayersBanned.Add(target.userID, new BansPlayers()
                {
                    Name = target.displayName,
                    Date = DateTime.UtcNow.Date.ToString("MM.dd.yyyy"),
                    Reason = Msg
                });

                target.inventory.containerBelt.Clear();
                target.inventory.containerMain.Clear();
                target.inventory.containerWear.Clear();
                DrawUI(target);
                SendReply(player, Messages["banPermanent"], $"{target.userID}/{target.displayName}", Msg);
            }
            else
            {
                SendReply(player, Messages["IsBanned"]);
            }
        }

        [ConsoleCommand("unuiban")]
        void cmdConsoleUnBan(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null) return;
            if (arg.Args == null || arg.Args.Length < 1)
            {
                Puts("Используйте unuiban NAME/STEAMID");
                return;
            }
            var uid = string.Format(arg.Args[0]);
            BasePlayer target = FindBasePlayer(uid);
            if (target == null)
            {
                Puts("Игрок не найден");
                return;
            }
            if (banplayers.PlayersBanned.ContainsKey(target.userID))
            {
                banplayers.PlayersBanned.Remove(target.userID);
                DestroyUI(target);
                Puts(Messages["UnBanned"], target.displayName);
            }
            else
            {
                Puts(Messages["nFoudsBans"]);
            }
        }

        [ChatCommand("unUiBan")]
        void cmdChatUnban(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, BAN_PERM))
            {
                SendReply(player, Messages["NPermission"]);
                return;
            }

            var nameOrId = args[0];
            BasePlayer target = FindBasePlayer(nameOrId);

            if (target == null)
            {
                SendReply(player, Messages["NFound"]);
                return;
            }
            if (banplayers.PlayersBanned.ContainsKey(target.userID))
            {
                banplayers.PlayersBanned.Remove(target.userID);
                DestroyUI(target);
                SendReply(player, Messages["UnBanned"], target.displayName);
            }
            else
            {
                SendReply(player, Messages["nFoudsBans"]);
            }
        }
        #endregion

        #region OXIDE HOOKS

        private void OnEntityTakeDamage(BaseCombatEntity entity, HitInfo hitinfo)
        {
            if (entity == null || hitinfo == null || entity.IsDestroyed) return;
            var attacker = hitinfo.InitiatorPlayer;
            if (attacker == null) return;

            if (banplayers.PlayersBanned.ContainsKey(attacker.userID))
            {
                hitinfo.damageTypes = new DamageTypeList();
                hitinfo.DoHitEffects = false;
                hitinfo.HitMaterial = 0;
            }
        }
        
        public BasePlayer FindBasePlayer(string nameOrUserId)
        {
            nameOrUserId = nameOrUserId.ToLower();
            foreach (var player in BasePlayer.activePlayerList)
            {
                if (player.displayName.ToLower().Contains(nameOrUserId) || player.UserIDString == nameOrUserId)
                    return player;
            }
            foreach (var player in BasePlayer.sleepingPlayerList)
            {
                if (player.displayName.ToLower().Contains(nameOrUserId) || player.UserIDString == nameOrUserId)
                    return player;
            }
            return default(BasePlayer);
        }

        private Dictionary<ulong, string> players = new Dictionary<ulong, string>();

        public string FindDisplayname(ulong uid)
        {
            string name;
            if (players.TryGetValue(uid, out name)) return name;
            return uid.ToString();
        }

        void OnServerInitialized()
        {
            LoadData();
            lang.RegisterMessages(Messages, this, "en");
            Messages = lang.GetMessages("en", this);
            permission.RegisterPermission(BAN_PERM, this);

            foreach (var player in BasePlayer.activePlayerList)
            {
                OnPlayerInit(player);
            }
        }

        void OnPlayerInit(BasePlayer player)
        {
            if (player.HasPlayerFlag(BasePlayer.PlayerFlags.ReceivingSnapshot))
            {
                timer.In(1f, () => OnPlayerInit(player));
                return;
            }
            if (banplayers.PlayersBanned.ContainsKey(player.userID))
            {
                player.inventory.containerBelt.Clear();
                player.inventory.containerMain.Clear();
                player.inventory.containerWear.Clear();
                DrawUI(player);
            }
        }

        List<ulong> GetBannedList()
        {
            return banplayers.PlayersBanned.Keys.ToList();
        }

        void Unload()
        {
            SaveData();
            foreach (var player in BasePlayer.activePlayerList)
            {
                DestroyUI(player);
            }
        }
        void OnServerSave() => SaveData();
        #endregion

        #region UI

        string BansGUI = "[{\"name\":\"bansui_bp\",\"parent\":\"Overlay\",\"components\":[{\"type\":\"UnityEngine.UI.RawImage\",\"sprite\":\"Assets/Content/UI/UI.Background.Tile.psd\",\"color\":\"0 0 0 0.99\"},{\"type\":\"NeedsCursor\"},{\"type\":\"RectTransform\",\"anchormin\":\"0 0\",\"anchormax\":\"1 1\"}]},{\"name\":\"bansui_text\",\"parent\":\"bansui_bp\",\"components\":[{\"type\":\"UnityEngine.UI.Text\",\"text\":\"ВНИМАНИЕ!\",\"fontSize\":50,\"align\":\"MiddleCenter\",\"color\":\"1 0.5790581 0 1\"},{\"type\":\"RectTransform\",\"anchormin\":\"0 0.3828116\",\"anchormax\":\"1 0.8997387\"}]},{\"name\":\"bansui_text2\",\"parent\":\"bansui_bp\",\"components\":[{\"type\":\"UnityEngine.UI.Text\",\"text\":\"Уважаемый {name}. Вам навсегда запрещен доступ к данному серверу.\nПричина: {reason}\nАдминистрация проекта желает Вам хорошего дня :)\",\"fontSize\":20,\"align\":\"MiddleCenter\",\"color\":\"0.7395598 0.7395598 0.7395598 1\"},{\"type\":\"RectTransform\",\"anchormin\":\"0 0\",\"anchormax\":\"1 1\"}]}]";


        void DrawUI(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, "bansui_bp");
            CuiHelper.AddUi(player,
              BansGUI.Replace("{name}", player.displayName)
              .Replace("{reason}", banplayers.PlayersBanned[player.userID].Reason)
              );
        }

        void DestroyUI(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, "bansui_bp");
        }

        #endregion

        #region DATA

        void LoadData()
        {
            try
            {
                banplayers = Interface.GetMod().DataFileSystem.ReadObject<Players>("UiBans_Players");
            }

            catch
            {
                banplayers = new Players();
            }
        }

        DynamicConfigFile BansData = Interface.Oxide.DataFileSystem.GetFile("UiBans_Players");

        

        void SaveData()
        {
            BansData.WriteObject(banplayers);
        }

        #endregion

        #region LOCALIZATION

        Dictionary<string, string> Messages = new Dictionary<string, string>()
        {
            { "playerNotFound", "Игрок не найден!" },
            { "playerDisconnected", "Игрок не в игре!" },
            { "banPermanent", "Вы забанили игрока {0}\nПричина: {1}" },
             { "NFound", "Извините но данный игрок не найден, или игрок не в сети"},
             { "nPermission", "У Вас нету прав на выполнение этой команды"},
             { "Args", "Вы не верно вводите команду, пример: /uiban NAME/STEAMID Reason"},
             { "IsBanned", "Игрок уже забанен"},
             { "nFoudsBans", "Игрока нету в бан листе"},
             { "UnBanned", "С игрока {0} снят бан"},

        };

        #endregion

    }
}
