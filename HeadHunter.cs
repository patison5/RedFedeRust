using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Facepunch;
using Facepunch.Math;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using Rust;
using UnityEngine;
using ProtoBuf;
using System.Timers;
using System.Threading.Tasks;
using Apex;

namespace Oxide.Plugins
{
    [Info("HEADHUNTER", "Lulex.py", "0.0.1")]
    [Description("HUNT, KILL AND GET MONEY!")]
    public class HeadHunter : RustPlugin
    {
    	private int activePlayersCount;
    	private List<BasePlayer> activePlayersList;
    	private BasePlayer RandomPlayer;

		private string startTime;
		private string endTime;
		private Timer mystimer;
        private bool _isEventStart = false;

        [PluginReference]
        private Plugin ZoneManager;


    	// Generate a random number between two numbers  
		private int RandomPlayerNumber(int min, int max)  
		{  
		    System.Random random = new System.Random();  
		    return random.Next(min, max);  
		} 

        private void updateActivePlayersList()
        {
            activePlayersCount = BasePlayer.activePlayerList.Count();
            activePlayersList  = BasePlayer.activePlayerList;
        }

        private BasePlayer getRandomPlayer() {
        	updateActivePlayersList();
			return activePlayersList[RandomPlayerNumber(0, activePlayersCount)];
		}

        private void broadcastMessage(string msg) {
            foreach (var playerElement in BasePlayer.activePlayerList) {
                SendReply(playerElement, $"{msg}");
            }
        }


		[ChatCommand("startHH")]
		private void startHH (BasePlayer player, string command, string[] args) {
            _isEventStart = true;

			updateActivePlayersList();

			RandomPlayer = getRandomPlayer();

			SendReply(RandomPlayer, $"Нам тут поступило сообщение, что ты украл кучу бабла у ученых..... За твою голову назначена награда! Беги, сука, бегиии!!!");			
			// SendReply(RandomPlayer, $"Напиши <color=#ff0>/givemeloot</color> чтобы получить топовый лут для вашей защиты");


			foreach (var playerElement in BasePlayer.activePlayerList)
			{
				SendReply(playerElement, $"<color=#b31212>ВАЖНОЕ СООБЩЕНИЕ!</color>\n\n<color=#128cb3>Господин Залупкин <color=#b433b5>(ученый)</color>: </color> <color=#ff0>{ RandomPlayer.displayName }</color> сбежал из города НПС с очень ценной для меня информацией!!! " 
											+ "Найдите его, убейте и принесите мне его <color=#ff0>голову</color>!!! \n\n" + 
											"</color> <color=#ff0>Взамен на его голову вы получите солидную награду!!!</color>");
			}

			startTimer(player);
		}

		[ChatCommand("givemeloot")]
		private void givemeloot (BasePlayer player, string command, string[] args) {
			if (player == RandomPlayer) {

				SendReply(player, $"<color=#128cb3>Господин Залупкин <color=#b433b5>(ученый)</color> </color> ДА! и правда это ты, держи свой лут!");	
				startTimer(player);

			} else {
				SendReply(player, $"<color=#128cb3>Господин Залупкин <color=#b433b5>(ученый)</color> </color> Пшел нахуй, этот лут не для тебя");
			}
		}

        [ChatCommand("ggtt")]
        private void ggtt (BasePlayer player, string command, string[] args) {
            string testId = UnityEngine.Random.Range(1, 99999999).ToString();

            // (-580.8, 27.1, 171.7)
            object PositionMain = new Vector3(Convert.ToSingle(-580.8), Convert.ToSingle(27.1), Convert.ToSingle(171.7));

            // var test = ZoneManager.Call("CreateOrUpdateZone", "9999999",PositionMain);
            // SendReply(player, testId);
            // SendReply(player, player.transform.position.ToString());

            ZoneManager.Call("createHeadHuntersZone", player); 
            // SendReply(player, isCreated.ToString());
            // SendReply(player, test.ToString());
        }




        

        private void pidor (BasePlayer player) {

            if (_isEventStart) {
                bool test = checkSkullNameInPlayerInventory(player);

                if (test) {
                    SendReply(player, $"<color=#128cb3>Господин Залупкин <color=#b433b5>(ученый)</color> </color> Красааавчик, давай его сюда!");
                    removeSkullAndGiveReward(player);
                    SendReply(player, $"<color=#128cb3>Господин Залупкин <color=#b433b5>(ученый)</color> </color>Вот, держи свою награду!");

                    broadcastMessage($"Игрок <color=#ff0>{player.displayName}</color> отнес голову вора и получил <color=#ff0>награду</color>!");
                    stopEvent();

                } else {
                    SendReply(player, $"<color=#128cb3>Господин Залупкин <color=#b433b5>(ученый)</color> </color> Слыш, у тебя же ничего нет! а ну пшел нафиг отседа!");
                }
            } else {
                SendReply(player, $"<color=#128cb3>Господин Залупкин <color=#b433b5>(ученый)</color> </color> Боюсь у меня пока никаких заданий для тебя!");
            }
            
        }

		public void checkForRandomPlayerSkull(BasePlayer player, string command, string[] args) {

            // bool test = checkSkullNameInPlayerInventory(player);
            // SendReply(player, $"У тебя есть череп? - {(test) ? "конечно есть!" : "заебал, дай за так.."}");


            // 1217701633
		}
        
        public bool checkSkullNameInPlayerInventory (BasePlayer player) {
            var name = "";
            bool flag = false;

            foreach (var item in player.inventory.containerBelt.itemList) { 

                if (item.info.shortname == "skull.human") {
                    name = item.name.Replace("Skull of ", "");
                    name = name.Replace("\"", "");
                }

                if (name == RandomPlayer.displayName.ToString()) {
                    flag = true;
                    break;
                }
            }

            foreach (var item in player.inventory.containerMain.itemList) { 

                if (item.info.shortname == "skull.human") {
                    name = item.name.Replace("Skull of ", "");
                    name = name.Replace("\"", "");
                }

                if (name == RandomPlayer.displayName.ToString()) {
                    flag = true;
                    break;
                }
            }

            return flag;
        }

        private void removeSkullAndGiveReward (BasePlayer player) {
            foreach (var item in player.inventory.containerBelt.itemList) { 

                if (item.info.shortname == "skull.human") {
                    var name = item.name.Replace("Skull of ", "");
                        name = name.Replace("\"", "");

                    if (name == RandomPlayer.displayName.ToString()) {
                        item.Remove();
                        break;
                    }
                }
            }

            foreach (var item in player.inventory.containerMain.itemList) { 

                if (item.info.shortname == "skull.human") {
                    var name = item.name.Replace("Skull of ", "");
                        name = name.Replace("\"", "");

                    if (name == RandomPlayer.displayName.ToString()) {
                        item.Remove();
                        break;
                    }
                }
            }

            GiveItem(player.inventory, ItemManager.CreateByName("sulfur", 10000), player.inventory.containerMain);
            stopEvent();
        }

		private void startTimer (BasePlayer player) {

			int periodTime = 40;

			SendReply(player, $"Отсчет пошел!");		

			int cooldown2 = Convert.ToInt32((periodTime * 60));
            mystimer = timer.Repeat(1f, cooldown2, () =>
            {
                foreach (var playerElement in BasePlayer.activePlayerList)
                    CloseUI(playerElement);

                if (cooldown2 == 1)
                {
                    SendReply(player, $"Время вышло... Молодец чувак! Ты победил!");
                }


                if (1 < cooldown2 && cooldown2 <= periodTime * 60)
                    foreach (var playerElement in BasePlayer.activePlayerList)
                        DrawUI(playerElement, FormatTime(TimeSpan.FromSeconds(cooldown2)));

                if (cooldown2 != 0)
                    cooldown2--;
                
            });
		}

        private void stopEvent () {
            foreach (var playerElement in BasePlayer.activePlayerList)
                CloseUI(playerElement);

            StopTimer();
            _isEventStart = false;
        }

        [ChatCommand("dstop")]
        private void dstop (BasePlayer player, string command, string[] args)
        {
            foreach (var playerElement in BasePlayer.activePlayerList)
                CloseUI(playerElement);

            SendReply(player, $"Таймер остановлен!");	
        	StopTimer();
            _isEventStart = false;
		}




        private static CuiElementContainer CreatePanel(string msg, string playerName)
        {
            return new CuiElementContainer
            {
                {
                    new CuiLabel
                    {
                        Text = { Text = $"<color=#ff0>{ playerName }</color> сможет сбежать через:", FontSize = 15, Align = TextAnchor.MiddleLeft },
                        RectTransform = { AnchorMin = "0.008 0.883", AnchorMax = "0.444 0.924"  },
                    },
                    "Hud",
                    "Label1"
                },
                {
                    new CuiLabel
                    {
                        Text = { Text = msg, FontSize = 15, Align = TextAnchor.MiddleLeft },
                        RectTransform = { AnchorMin = "0.008 0.85", AnchorMax = "0.444 0.889"  },
                    },
                    "Hud",
                    "Label2"
                },
            };
        }

        [ChatCommand("d")]
        void DrawUI(BasePlayer player, string msg)
        {

            CuiElementContainer container;
            container = CreatePanel(msg, RandomPlayer.displayName);
            CuiHelper.AddUi(player, container);

        }

        [ChatCommand("dc")]
        void CloseUI(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, "Label1");
            CuiHelper.DestroyUi(player, "Label2");

        }

        private void StopTimer()
        {
            if (mystimer != null)
                mystimer?.Destroy();
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

        bool GiveItem(PlayerInventory inv, Item item, ItemContainer container = null)
        {
            if (item == null) { return false; }
            int position = -1;
            return (((container != null) && item.MoveToContainer(container, position, true)) || (item.MoveToContainer(inv.containerMain, -1, true) || item.MoveToContainer(inv.containerBelt, -1, true)));
        }
    }
}