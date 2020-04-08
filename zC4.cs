using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using System;
using UnityEngine.Assertions;
using System.Linq;

using UnityEngine;



namespace Oxide.Plugins
{
    [Info("Triggered Explosive Charges", "Lulex.py", "0.0.1")]

    public class zC4 : RustPlugin
    {

        List<Item> customItems = new List<Item>();

    	[ChatCommand("testC")]
    	private void checkC4 (BasePlayer player) {
          
            foreach (var item in player.inventory.containerBelt.itemList) { 

				if (item.info.shortname == "rf.detonator") {
                    SendReply(player, $"Во {item.info.shortname}");
                    // SendReply(player, $"Во {item.frequency.ToString()}");
                    
                    Puts(item.GetHeldEntity().GetType().ToString());
                    
                    if (item.GetHeldEntity().GetType().ToString() == "Detonator") {
                         
                        int frequency = ((Detonator)item.GetHeldEntity()).GetFrequency();
                    }
                }

        				
            }
        }
        [ChatCommand("callHelli")]
        private void callHelli(BasePlayer player)
        {
            //assets/prefabs/clothes/suit.heavyscientist/scientistsuitheavy.item.prefab
            //var item = GameManager.server?.CreateEntity("assets/prefabs/tools/detonator/detonator.entity.prefab", player.transform.position) as Detonator;

            Item item = ItemManager.CreateByName("rf.detonator", 1);

            item.name = "Helicopter Transmitter";
            customItems.Add(item);

            Item item2 = ItemManager.CreateByItemID(-1772746857, 1, 0);
            item2.OnBroken();
//            (Item as BaseEntity)item2.

            GiveItem(player.inventory, item, player.inventory.containerMain);
            GiveItem(player.inventory, item2, player.inventory.containerMain); 
        }
        
        private void OnEntitySpawned(Detonator entity)
        {
            if (entity == null || !(entity is Detonator)) return;

            entity.frequency = 0000;
            Puts(entity.name);
        }

        private void OnItemAddedToContainer(ItemContainer container, Item item)
        {
            if (item.name == "Helicopter Transmitter" && container.playerOwner != null)
            {

                Item itest = container.playerOwner.inventory.containerBelt.FindItemByUID(item.uid);

                if (itest != null)
                {
                    SendReply(container.playerOwner, "Вы взяли <color=#FFEB3B>трансмиттер вражеского пилота</color>. Если Вы нажмете на кнопку к Вам тут же вылетит вражеская авиация! <color=#FFEB3B>Будте осторожны!</color>");
                }
            }

            if (item.name == "Broken Helicopter Transmitter" && container.playerOwner != null)
            {

                Item itest = container.playerOwner.inventory.containerBelt.FindItemByUID(item.uid);

                if (itest != null)
                {
                    SendReply(container.playerOwner, "Вы взяли <color=#FFEB3B>поломанный</color> трансмиттер вражеского пилота");
                }
            }
        }


        private void OnPlayerInput(BasePlayer player, InputState input)
        {
            if (input.WasJustPressed(BUTTON.FIRE_PRIMARY))
            {
                //HeldEntity weapon = player.GetHeldEntity();

                var item = player.GetActiveItem();
                if (item == null) return;

                Item con = (from x in customItems where x.uid == item.uid select x).FirstOrDefault();
                if (con == null) return;

                int frequency = ((Detonator)item.GetHeldEntity()).GetFrequency();
                //SendReply(player, $"{frequency}");

                var playerPos = player.transform.position;
                float mapWidth = (TerrainMeta.Size.x / 2) - 50f;

                var heliPos = new Vector3(
                    playerPos.x < 0 ? -mapWidth : mapWidth,
                    30,
                    playerPos.z < 0 ? -mapWidth : mapWidth
                );

                BaseHelicopter heli = GameManager.server.CreateEntity("assets/prefabs/npc/patrol helicopter/patrolhelicopter.prefab", new Vector3(), new Quaternion(), true) as BaseHelicopter;
                if (!heli) return;



                heli.name = "1231";

                PatrolHelicopterAI heliAI = heli.GetComponent<PatrolHelicopterAI>();
                heli.Spawn();
                heli.transform.position = heliPos;
                heliAI._targetList.Add(new PatrolHelicopterAI.targetinfo(player, player));

                SendReply(player, $"Поздравляю! Вы вызвали <color=#FFEB3B> вражеский вертолет</color> на свою позицию! <color=#FFEB3B>Вам пизда!</color>");

                foreach (var pl in BasePlayer.activePlayerList)
                    if (pl != player)
                        SendReply(pl, $"<color=#FFEB3B>Кто-то вызвал вражеский вертолет!</color>");

                item.name = "Broken Helicopter Transmitter";
                customItems.Remove(con);
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