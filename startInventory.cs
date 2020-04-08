using Newtonsoft.Json;
using Oxide.Core.Libraries.Covalence;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Oxide.Core;
using System;
using Oxide.Core.Configuration;
using System.Linq;


namespace Oxide.Plugins 
{
	[Info("startInventory", "Lulex.py", "0.0.1")]
	public class startInventory : RustPlugin 
	{	

		private class StartInventoryItem {
			public string 	title 		{ get; set; }
			public string 	location 	{ get; set; }
			public int 		amount 		{ get; set; }
		}

		private class StartInventory {
			public List<StartInventoryItem> iventoryList = new List<StartInventoryItem>();
		}

        private StartInventory 		config;
        private StartInventory 		customInventory;
        private DynamicConfigFile 	inventoryData;

        private void Init ()
        {
        	if (Interface.Oxide.DataFileSystem.ExistsDatafile("startInventory"))
			{
				inventoryData = Interface.Oxide.DataFileSystem.GetDatafile("startInventory");
			}
			else
			{
				inventoryData = Interface.Oxide.DataFileSystem.GetDatafile("startInventory");
				inventoryData.WriteObject(GetDefaultConfig(), true);
			}
        	
        	config = Config.ReadObject<StartInventory>();
        }

        void Loaded() {
        	Puts("Testing Loaded function...");
        	customInventory = inventoryData.ReadObject<StartInventory>();
        }

        protected override void LoadDefaultConfig()
		{
		    Config.WriteObject(GetDefaultConfig(), true);
		}

		private StartInventory GetDefaultConfig()
		{
			StartInventory defaultInventory = new StartInventory() {};
			defaultInventory.iventoryList = new List<StartInventoryItem> {
				new StartInventoryItem() { title = "stonehatchet",  		location = "belt", amount = 1 },
				new StartInventoryItem() { title = "stone.pickaxe", 		location = "belt", amount = 1 },
				new StartInventoryItem() { title = "bearmeat.cooked", 		location = "belt", amount = 3 },
				new StartInventoryItem() { title = "bandage", 				location = "belt", amount = 10 },
				new StartInventoryItem() { title = "tshirt",  				location = "wear", amount = 1 },
				new StartInventoryItem() { title = "attire.hide.boots",  	location = "wear", amount = 1 },
				new StartInventoryItem() { title = "attire.hide.poncho",  	location = "wear", amount = 1 },
				new StartInventoryItem() { title = "hat.beenie",  			location = "wear", amount = 1 },
				new StartInventoryItem() { title = "burlap.gloves",  		location = "wear", amount = 1 },
				new StartInventoryItem() { title = "pants.shorts",  		location = "wear", amount = 1 }
			};

		    return defaultInventory;
		}
       
		void OnPlayerRespawned(BasePlayer player) 
		{
		    // cleaning inventory
		    foreach (var item in player.inventory.containerBelt.itemList) { item.Remove(); }
            foreach (var item in player.inventory.containerMain.itemList) { item.Remove(); }
            foreach (var item in player.inventory.containerWear.itemList) { item.Remove(); }

            foreach (var item in customInventory.iventoryList) {
            	GiveItem(player.inventory,
                    ItemManager.CreateByName(item.title, item.amount),
                    item.location == "belt"
                        ? player.inventory.containerBelt
                        : item.location == "wear"
                            ? player.inventory.containerWear
                            : player.inventory.containerMain);
            }
		}

		bool GiveItem(PlayerInventory inv, Item item, ItemContainer container = null)
        {
            if (item == null) { return false; }
            int position = -1;
            return (((container != null) && item.MoveToContainer(container, position, true)) || (item.MoveToContainer(inv.containerMain, -1, true) || item.MoveToContainer(inv.containerBelt, -1, true)));
        }
	}
}