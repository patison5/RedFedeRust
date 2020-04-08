using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Oxide.Core;
using System.Globalization;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Libraries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Oxide.Plugins
{
    [Info("Give Tools", "Beorn", "1.0.0")]
    [Description("Give tools hatchet pickaxe")]

    public class GiveTools : RustPlugin
    {
        private const string permGiveTools = "givetools.perm";

        void Init()
        {
            permission.RegisterPermission(permGiveTools, this);
        }

        void Loaded()
        {
            PlayersData = Interface.Oxide.DataFileSystem.ReadObject<List<GiveToolsData>>("GiveToolsData");
            Puts("Плагин GiveTools загружен");
        }

        void Saved()
        {
            Interface.Oxide.DataFileSystem.WriteObject("GiveToolsData", PlayersData);
        }


        [ConsoleCommand("reloadfiretools")]
        private void ReloadData(ConsoleSystem.Arg arg)
        {
            foreach (var playerData in PlayersData)
            {
                playerData.Reset();
            }
            Saved();
            Puts("FireTools Data Reloaded");
        }

        public List<GiveToolsData> PlayersData = new List<GiveToolsData>();

        public class GiveToolsData
        {
            public string Nickname { get; set; }
            public string UID { get; set; }
            public DateTime LastTimeChecked { get; set; }

            public GiveToolsData(string Nickname, string UID)
            {
                this.Nickname = Nickname;
                this.UID = UID;
                this.LastTimeChecked = DateTime.Now.AddDays(-2); //7
            }
            public void Reset()
            {
                this.LastTimeChecked = DateTime.Now.AddDays(-2); //7
            }
        }

        void CreateInfo(BasePlayer player)
        {
            if (player == null) return;
            PlayersData.Add(new GiveToolsData(player.displayName, player.UserIDString));
            Saved();
        }

        [ChatCommand("firetools")]
        private void GiveToolsMethod(BasePlayer player, string command, string[] args)
        {
            if (!player.IPlayer.HasPermission(permGiveTools))
            {
                SendReply(player, "У вас нет прав на выполнение этой команды");
                return;
            }
                var check = (from x in PlayersData where x.UID == player.UserIDString select x).Count();
            if (check == 0)
            {
                CreateInfo(player);
            }

            var playerInstance = (from x in PlayersData where x.UID == player.UserIDString select x).FirstOrDefault();
            var now = DateTime.Now;
            var days = (now - playerInstance.LastTimeChecked).Days;
            var message = FormatTime(playerInstance.LastTimeChecked.AddDays(2) - now); //7
            Puts(days.ToString());

            // 7.0
            if (days >= 2.0)
            {
                playerInstance.LastTimeChecked = DateTime.Now;
                rust.RunServerCommand($"givetool {player.UserIDString} hatchet icepick");
            } else
            {
                SendReply(player, $"До повторного использования {message}");
            }
        }

        public static string FormatTime(TimeSpan time)
        {
            string result = string.Empty;
            if (time.Days != 0)
                result += $"{Format(time.Days, "дней", "дня", "день")} ";

            if (time.Hours != 0)
                result += $"{Format(time.Hours, "часов", "часа", "час")} ";

            if (time.Minutes != 0)
                result += $"{Format(time.Minutes, "минут", "минуты", "минута")} ";

            if (time.Seconds != 0)
                result += $"{Format(time.Seconds, "секунд", "секунды", "секунда")} ";

            return result;
        }

        private static string Format(int units, string form1, string form2, string form3)
        {
            var tmp = units % 10;

            if (units >= 5 && units <= 20 || tmp >= 5 && tmp <= 9)
                return $"{units} {form1}";

            if (tmp >= 2 && tmp <= 4)
                return $"{units} {form2}";

            return $"{units} {form3}";
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
    }
}