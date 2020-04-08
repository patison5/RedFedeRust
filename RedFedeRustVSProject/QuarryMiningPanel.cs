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
	[Info("QuarryMiningPanel", "Lulex.py", "0.0.1")]
	public class QuarryMiningPanel : RustPlugin 
	{	

		private List<MiningQuarry> quarries = new List<MiningQuarry>();


		[ChatCommand("qstatus")]
		private void CheckQuarryStatus (BasePlayer player, string command, string[] args) {
			SendReply(player, $"<color=#3999D5>##########</color>  <color=#FFEB3B>Информация по карьерам</color>  <color=#3999D5>##########</color>");

			bool haveQuarries = false;

			foreach (var quarry in quarries)
            {
                if (quarry.IsDestroyed) continue;

                if (player != null)
                {	
                	haveQuarries = true;

                	Item fuel 	= quarry.fuelStoragePrefab.instance.GetComponent<StorageContainer>().inventory.FindItemsByItemName("lowgradefuel");

                	ItemContainer hopper = (quarry.hopperPrefab.instance as StorageContainer).inventory;
					Item Stones = hopper.FindItemsByItemName("Stones");
					Item Sulfur = hopper.FindItemsByItemName("sulfur.ore");
                	Item hqm 	= hopper.FindItemsByItemName("hq.metal.ore");
                	Item metal 	= hopper.FindItemsByItemName("metal.ore");

                	int stonesAmoun = 0;
                	int SulfurAmoun = 0;
                	int hqmAmoun = 0;
                	int metalAmoun = 0;

                	if (Stones != null)
            			stonesAmoun += Stones.amount;
            		if (Sulfur != null)
            			SulfurAmoun += Sulfur.amount;
            		if (hqm != null)
            			hqmAmoun += hqm.amount;
            		if (metal != null)
            			metalAmoun += metal.amount;

                    if (quarry.OwnerID == player.userID) {

                    	if (fuel != null){
							
							SendReply(player, $"В Вашем карьере: <color=#FFEB3B>{ fuel.amount.ToString() } топлива</color>. \nДобыто: <color=#FFEB3B>{stonesAmoun}</color> камня, <color=#FFEB3B>{SulfurAmoun}</color> серы, <color=#FFEB3B>{hqmAmoun}</color> МВК, <color=#FFEB3B>{metalAmoun}</color> металла");

                    	}
                    	else
                    		SendReply(player, $"В Вашем карьере: <color=#FFEB3B> Нет топлива</color>. \nДобыто: <color=#FFEB3B>{stonesAmoun}</color> камня, <color=#FFEB3B>{SulfurAmoun}</color> серы, <color=#FFEB3B>{hqmAmoun}</color>, МВК <color=#FFEB3B>{metalAmoun}</color> металла");
                    }


                    continue;
                }
            }


            if (!haveQuarries)
            	SendReply(player, $"У вас <color=#FFEB3B>нет</color> карьеров!");

            SendReply(player, $"<color=#3999D5>##########</color>  <color=#FFEB3B>Информация по карьерам</color>  <color=#3999D5>##########</color>");
		}



		private void OnServerInitialized()
        {
            quarries.Clear();
            quarries = UnityEngine.Object.FindObjectsOfType<MiningQuarry>().Where(x => x.OwnerID != 0).ToList();
        }

        private void OnEntitySpawned(MiningQuarry quarry)
        {
            if (quarry != null && !quarries.Contains(quarry) && quarry.OwnerID != 0) {
                quarries.Add(quarry);
            }
        }

	}
}


// rh start - rad housew
// chat
// info
