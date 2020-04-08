using Newtonsoft.Json;
using Oxide.Core.Libraries.Covalence;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Oxide.Core;
using System;
using Oxide.Core.Configuration;
using System.Linq;
using UnityEngine;


namespace Oxide.Plugins 
{
	[Info("banker", "Lulex.py", "0.0.1")]
	public class banker : RustPlugin 
	{	


        private const string permBankerAdmin = "banker.admin";

        public List<Banker> customers = new List<Banker>();
        const uint priceItemId = 642482233;
        public int price = 5;

        [ChatCommand("repair")]
        void chatCommand_repair(BasePlayer player, string command, string[] args)
        {
            if (player.net.connection.authLevel == 1 || player.net.connection.authLevel == 2 )
            {   
                SendReply(player, $"дарова, {player.displayName}, я сделалъ");
                List<BuildingBlock> allBlocks = UnityEngine.Object.FindObjectsOfType<BuildingBlock>().ToList();
                
                foreach(BuildingBlock block in allBlocks)
                {
                    if (block.OwnerID == player.userID) {
                        block.health = block.MaxHealth();
                    }
                    
                }
            } else {
                SendReply(player, $"дарова, {player.displayName},  <color=#FFEB3B>У тебя неи прав на эту команду!!</color>");
            }
        }


		[ChatCommand("banker")]
		private void bankerTest (BasePlayer player, string command, string[] args) {

            if (!player.IPlayer.HasPermission(permBankerAdmin)) {
                SendReply(player, "У вас нет прав на выполнение этой команды");
                return;
            }

			SendReply(player, $"<color=#3999D5>##########</color>  <color=#FFEB3B>Информация по карьерам:</color>  <color=#3999D5>##########</color>");

            foreach (var item in player.inventory.containerBelt.itemList) {
                SendReply(player, $"{ item.info.shortname }");

                Banker con = (from x in customers where x.UID == Convert.ToString(player.userID) select x).FirstOrDefault();
                con.itemsList.Add(new CustomItem (item.amount, item.info.shortname));
                Saved();
            }
		}

		private void OnServerInitialized()
        {

        }

        // Загружаем если есть дату по игрокам. Создаем нового игрока в дате, если его не существует.
        void Loaded()
        {
            customers = Interface.Oxide.DataFileSystem.ReadObject<List<Banker>>("Banker");
            foreach (var player in BasePlayer.activePlayerList)
            {
                var check = (from x in customers where x.UID == player.UserIDString select x).Count();
                if (check == 0) CreateInfo(player);
            }
        }

         void Init()
        {
            permission.RegisterPermission(permBankerAdmin, this);
        }

        // Сохраняем дату по игрокам
        void Saved()
        {
            Interface.Oxide.DataFileSystem.WriteObject("Banker", customers);
        }

        // Создаем нового игрока в дате, если его не существует
        void CreateInfo(BasePlayer player)
        {
            if (player == null) return;
            customers.Add(new Banker(player.displayName, player.UserIDString));
            Saved();
        }

        public class CustomItem {

            public CustomItem (int amount, string shortname) {
                this.amount = amount;
                this.shortname = shortname;
            }

            public int amount           { get; set; }
            public string shortname     { get; set; }
        }

        public class Banker {
            public Banker (string nickname, string UID) {
                this.nickname = nickname;
                this.UID = UID;
                this.itemsList = new List<CustomItem>();
                this.currentBalance = 0;
            }


            public string nickname              { get; set; }
            public string UID                   { get; set; }
            public List<CustomItem> itemsList   { get; set; }

            public int currentBalance   { get; set; }
        }
	}
}
