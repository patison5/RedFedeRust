using Facepunch;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using Rust;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Oxide.Plugins
{
    [Info("TestMarker", "RustPlugin.ru", "0.0.1")]

    class TestMarker : RustPlugin
    {
        [PluginReference]
        Plugin WorldMap, ImageLibrary;

        private const string Overlay = "TestMarkerShopOverlay";

        bool debug = false;

        const string prefab = "assets/content/vehicles/minicopter/minicopter.entity.prefab";

        string npcMessage = "<color=#dc539c>Бот Василий:</color> <color=#ffe>{0}</color>";

        const string chinukPrefab       = "assets/prefabs/npc/ch47/ch47.entity.prefab";
        const string miniCopterPrefab   = "assets/content/vehicles/minicopter/minicopter.entity.prefab";
        const string scrapCopterPrefab  = "assets/content/vehicles/scrap heli carrier/scraptransporthelicopter.prefab";
        const string ridableHorsePrefab = "assets/rust.ai/nextai/testridablehorse.prefab";
        const string sedanPrefab        = "assets/content/vehicles/sedan_a/sedantest.entity.prefab";

        const string MBKHouseShortCut           = "horse.shoes.advanced";
        const string basicHouseShortCut         = "horse.shoes.basic";
        const string horseArmorRoadsignShortCut = "horse.armor.roadsign";
        const string horseArmorRoodShortCut     = "horse.armor.wood";

        private static int COPTER_PRICE = 250;
        private static int SEDAN_PRICE = 1000;
        private static int SCRAP_COPTER_PRICE = 500;
        private static int HORSE_PRICE = 150;
        private static int HQHS_PRICE = 125;
        private static int OHS_PRICE = 100;
        private static int RSHA_PRICE = 125;
        private static int OHA_PRICE = 50;


        //npc id... 1453672018
        [ConsoleCommand("testmarker.buyvehicle")]
        private void buyBotsVenicle(ConsoleSystem.Arg arg)
        {
            if (arg.Player()== null)
                return;

            BasePlayer player = arg.Player();

            string choice = arg.GetString(0);



            //тут отрисовываем гуи....
            string msg = " Желаешь купить что-нибудь? - Это у нас моожно!";
            string msg1 = " Коптер, Лошадь, Седан, броня? Чего вашей душе угодно!?";


            // SendReply(player, $"{npcMessage}", msg1);


            if (choice == "mini")
            {
                var flag = getScrapFromPlayerInventory(player, COPTER_PRICE);

                if (flag){
                    SpawnOneHelicopterPlease("mini", player);
                    SendReply(player, $"{npcMessage}", "Вот твой миникоптер... Не забудь пополнить бак!");
                }
            }
            else if (choice == "chinuck")
            {
                var flag = getScrapFromPlayerInventory(player, SEDAN_PRICE);

                if (flag)
                    SpawnOneHelicopterPlease("chinuck", player);
            }
            else if (choice == "sedan")
            {
                var flag = getScrapFromPlayerInventory(player, SEDAN_PRICE);

                if (flag){
                    SpawnOneHelicopterPlease("sedan", player);
                    SendReply(player, $"{npcMessage}", "Полуучите и распишитесь! Только это.. надеюсь ты права не забыл?!");
                }
            }
            else if (choice == "scrapCopter")
            {
                var flag = getScrapFromPlayerInventory(player, SCRAP_COPTER_PRICE);

                if (flag) {
                    SpawnOneHelicopterPlease("scrapCopter", player);
                    SendReply(player, $"{npcMessage}", "Воот он! Красавчик какой, а!? \nДержи ключи!");                    
                }
            }
            else if (choice == "horse")
            {
                var flag = getScrapFromPlayerInventory(player, HORSE_PRICE);

                if (flag) {
                    SpawnOneHorsePlease(player);
                    SendReply(player, $"{npcMessage}", "Только покормить не забудь!!");

                }

                // rust.RunServerCommand($"entity.spawn horse {player.transform.position}"); (можно передать вектор - куда смотрит бот...)
            }
            else if (choice == "MBKHouseShortCut")
            {
                var flag = getScrapFromPlayerInventory(player, HQHS_PRICE);

                if (flag) {
                    GiveItem(player.inventory, ItemManager.CreateByName(MBKHouseShortCut, 1), player.inventory.containerMain);
                    SendReply(player, $"{npcMessage}", "Вот, держи!");
                }
                else 
                    SendReply(player, $"{npcMessage}", "Может лучше посмотришь серию по проще?))");
            }
            else if (choice == "basicHouseShortCut")
            {
                var flag = getScrapFromPlayerInventory(player, OHS_PRICE);

                if (flag) {
                    GiveItem(player.inventory, ItemManager.CreateByName(basicHouseShortCut, 1), player.inventory.containerMain);
                    SendReply(player, $"{npcMessage}", "Вот, держи!");
                }
            }
            else if (choice == "horseArmorRoadsignShortCut")
            {
                var flag = getScrapFromPlayerInventory(player, RSHA_PRICE);

                if (flag) {
                    GiveItem(player.inventory, ItemManager.CreateByName(horseArmorRoadsignShortCut, 1), player.inventory.containerMain);
                    SendReply(player, $"{npcMessage}", "Армированная броня на Лошадь.. тяжелая штукенция однако!");
                }
            }
            else if (choice == "horseArmorRoodShortCut")
            {
                var flag = getScrapFromPlayerInventory(player, OHA_PRICE);

                if (flag) {
                    GiveItem(player.inventory, ItemManager.CreateByName(horseArmorRoodShortCut, 1), player.inventory.containerMain);
                    SendReply(player, $"{npcMessage}", "Вот она, сам смастерил! Держи!");
                }
            }
        }


        [ChatCommand("giveCopter")]
        void tgiveCopter(BasePlayer player, string cmd, string[] Args)
        {
            // entity.spawn horse $player.id (or player.position)

            Puts(player.transform.position.ToString());
            rust.RunServerCommand($"entity.spawn horse {player.transform.position}");

            // SpawnOneHelicopterPlease("scrapCopter", player);

            SpawnOneHorsePlease(player);

            Puts("Giving u to suck...");
        }

        void SpawnOneHelicopterPlease(string what, BasePlayer player)
        {

            //отдебажить на все карты...
            Vector3 spot = player.transform.position;
            spot = player.transform.position - (player.transform.forward * 12);
            

            if (spot == null)
                return;

            string prefab = chinukPrefab;

            if (what == "mini")
                prefab = miniCopterPrefab;

            if (what == "scrapCopter")
                prefab = scrapCopterPrefab;

            if (what == "sedan")
                prefab = sedanPrefab;

            BaseVehicle vehicle = (BaseVehicle)GameManager.server.CreateEntity(prefab, spot, new Quaternion());

            if (vehicle == null)
                return;

            BaseEntity entity = vehicle as BaseEntity;

            entity.OwnerID = 998877665544;

            vehicle.Spawn();

            uint copteruint = entity.net.ID;
            // storedData.SpawnerCH47.Add(copteruint);

            if (debug)
                Puts($"SPAWNED MINICOPTER {copteruint.ToString()} - OWNER {entity.OwnerID}");
        }


        void SpawnOneHorsePlease(BasePlayer player)
        {

            //отдебажить на все карты...
            Vector3 spot = player.transform.position;
            spot = player.transform.position - (player.transform.forward * 12);
            

            if (spot == null)
                return;

            BaseVehicle ridebleHorse = (BaseVehicle)GameManager.server.CreateEntity(ridableHorsePrefab, spot, new Quaternion());

            if (ridebleHorse == null)
                return;

            BaseEntity entity = ridebleHorse as BaseEntity;

            entity.OwnerID = 998877665544;

            ridebleHorse.Spawn();

            uint copteruint = entity.net.ID;
            // storedData.SpawnerCH47.Add(copteruint);

            if (debug)
                Puts($"SPAWNED HORSE {copteruint.ToString()} - OWNER {entity.OwnerID}");
        }



        bool getScrapFromPlayerInventory (BasePlayer player, int price) {
            // var player = arg.Player();

            int randomNumber = RandomNumber(0, 100);

            if (randomNumber < 5) {
                SendReply(player, $"Тааак уж и быть, только для тебя и только сегодня! скидка в 15%!");
                price = Convert.ToInt32((price * 0.85));;
            }

            int amount = player.inventory.GetAmount(-932201673);
            // int result = player.inventory.GetAmount(-932201673) * config.obmen;

            if (amount <= 0) 
            {
                SendReply(player, $"Что-то мне подсказывает, что ты не взял с собой скрапа...");
                return false;

            } else if (amount < price) 
            {
                SendReply(player, $"Боюсь тебе не хватает! ");
                return false;
            
            } else 
            {
                player.inventory.Take(null, -932201673, price);
                return true;
            }

            
            // Item Give = ItemManager.CreateByItemID(Convert.ToInt32(642482233), result);
            // player.GiveItem(Give);
            // PrintToChat(player, $"Вы успешно обменяли <color=#00FFFF>Кристаллы Ниберия</color> на {result} <color=#8A2BE2>Кристаллов Илизиума</color>!");
        }

        // Generate a random number between two numbers  
        private int RandomNumber(int min, int max)  
        {  
            System.Random random = new System.Random();  
            return random.Next(min, max);  
        } 


        bool GiveItem(PlayerInventory inv, Item item, ItemContainer container = null)
        {
            if (item == null) { return false; }
            int position = -1;
            return (((container != null) && item.MoveToContainer(container, position, true)) || (item.MoveToContainer(inv.containerMain, -1, true) || item.MoveToContainer(inv.containerBelt, -1, true)));
        }


        [ChatCommand("ggtsh")]
        void getshortName(BasePlayer player, string cmd, string[] Args)
        {
            foreach (var item in player.inventory.containerBelt.itemList)
            {
                SendReply(player, $"название { item.info.shortname }");
            }
        }


        [ChatCommand("marker")]
        void mmarker(BasePlayer player, string cmd, string[] Args)
        {
            WorldMap?.Call("AddTemporaryMarker", "bag", false, 0.04f, 0.99f, player.transform, "TestMarker", "test");
            Puts("create");
        }


        [ChatCommand("remmarker")]
        void mremmarker(BasePlayer player, string cmd, string[] Args)
        {
            WorldMap?.Call("RemoveTemporaryMarkerByName", "radHouseMarker");
            Puts("delete");
        }

        private void DrawStoreOverlayGUI(BasePlayer player)
        {
            CuiHelper.AddUi(player, new CuiElementContainer
            {
                {
                    new CuiPanel
                    {
                        Image = { Color = "0 0 0 0.39" },
                        RectTransform = { AnchorMin = "0.086 0.15", AnchorMax = "0.914 0.917" },
                        CursorEnabled = true
                    },
                    "Hud",
                    Overlay
                }
            });
        }

        [ConsoleCommand("testmarker.close")]
        private void DestroyOverlay(ConsoleSystem.Arg arg)
        {
            if (arg.Player() == null)
                return;
            BasePlayer player = arg.Player();
            CuiHelper.DestroyUi(player, Overlay);
            CuiHelper.DestroyUi(player, "CloseOverlay");
            Puts("Removed");
        }

        private void DrawStoreGUI(BasePlayer player)
        {
            DrawStoreOverlayGUI(player);
            CuiHelper.AddUi(player, HorsePanel());
            CuiHelper.AddUi(player, Model("Car", "0.255 0", "0.495 1", "0.147 0.723", "0.882 0.97", "Седан", "CarURL", "sedan", SEDAN_PRICE));
            CuiHelper.AddUi(player, Model("Copter", "0.505 0", "0.745 1", "0.264 0.723", "0.755 0.97", "Коптер", "CopterURL", "mini", COPTER_PRICE));
            CuiHelper.AddUi(player, Model("ScrapCopter", "0.755 0", "1 1", "0.264 0.723", "0.755 0.97", "Большой Коптер", "ScrapCopterURL", "scrapCopter", SCRAP_COPTER_PRICE));
            CuiHelper.AddUi(player, CloseButton());
        }

        public static string HorseURL => "https://i.imgur.com/1b6XxUN.png"; 
        public static string CarURL => "https://i.imgur.com/rEXFfMI.png";
        public static string CopterURL => "https://i.imgur.com/AzCV23W.png";
        public static string ScrapCopterURL => "https://psv4.userapi.com/c848024/u170877706/docs/d17/16887452b19d/copter.png?extra=J5SOmyma-NbLFSKpTpA5S2IoMmuF0C0WEBxa3WKJ5-qMJn4jalskeY1SOIAwEWPKOfGr-aeQ0-KrXSrWpyimKgu410pZRQsHBYQoXfzCGRRP3XJPPqyO7GDT02djGerCUlx3fkeSGGV5ChajegZzdNgFWv8";


        public static string SCRAP => "https://i.imgur.com/h2eqKOY.png";

        public static string CLOSE => "https://i.imgur.com/iupaeZv.png";


        private void OnServerInitialized()
        {
            if (ImageLibrary != null)
            {
                ImageLibrary.Call("AddImage", HorseURL, "HorseURL");
                ImageLibrary.Call("AddImage", CarURL, "CarURL");
                ImageLibrary.Call("AddImage", CopterURL, "CopterURL");
                ImageLibrary.Call("AddImage", ScrapCopterURL, "ScrapCopterURL");
                ImageLibrary.Call("AddImage", SCRAP, "SCRAP");
                ImageLibrary.Call("AddImage", CLOSE, "CLOSE");
            }
        }

        private CuiElementContainer HorsePanel()
        {
            return new CuiElementContainer()
            {
                {
                    new CuiPanel
                    {
                        Image = { Color = "0 0 0 0" },
                        RectTransform = { AnchorMin = "0 0", AnchorMax = "0.245 1" }
                    },
                    Overlay,
                    "HorsePanel"
                },
                {
                    TitlePanel(),
                    "HorsePanel",
                    "HorsePanelTitlePanel"
                },
                {
                    TitleForPanel("Лошадь"),
                    "HorsePanelTitlePanel"
                },
                {
                    PanelMain(),
                    "HorsePanel",
                    "HorsePanelMain"
                },
                {
                    CreateImage("HorsePanelMain", "HorseURL", "0.264 0.723", "0.755 0.97")
                },
                {
                    BuyButton("horse"),
                    "HorsePanelMain"
                },
                {
                    PriceTitle(HORSE_PRICE),
                    "HorsePanelMain"
                },
                {
                    CreateImage("HorsePanelMain", "SCRAP", "0.65 0.2", "0.93 0.35")
                },
                {
                    BuyButton("horseArmorRoodShortCut", Text: " Деревянная броня", anchorMin:  "0.1 0.37", anchorMax:"0.8 0.42", color: "1 0.5 0 0.7", anchor: TextAnchor.MiddleLeft),
                    $"HorsePanelMain"
                },
                {
                    BuyButton("horseArmorRoadsignShortCut", Text: " Броня из знаков", anchorMin: "0.1 0.44", anchorMax: "0.8 0.49", color: "1 0.5 0 0.7", anchor: TextAnchor.MiddleLeft),
                    $"HorsePanelMain"
                },
                {
                    BuyButton("basicHouseShortCut", Text: " Обычная подкова",  anchorMin: "0.1 0.51", anchorMax: "0.8 0.56", color: "1 0.5 0 0.7", anchor: TextAnchor.MiddleLeft),
                    $"HorsePanelMain"
                },
                {
                    BuyButton("MBKHouseShortCut", Text: " HQ подкова",  anchorMin: "0.1 0.58", anchorMax:"0.8 0.63", color: "1 0.5 0 0.7", anchor: TextAnchor.MiddleLeft),
                    $"HorsePanelMain"
                },
                {
                    PriceTitle(OHA_PRICE, PositionMin:"0.85 0.37", PositionMax: "1 0.42", Font: 20),
                    "HorsePanelMain"
                },
                {
                    PriceTitle(RSHA_PRICE, PositionMin:"0.85 0.44", PositionMax: "1 0.49", Font: 20),
                    "HorsePanelMain"
                },
                {
                    PriceTitle(OHS_PRICE, PositionMin:"0.85 0.51", PositionMax: "1 0.56", Font: 20),
                    "HorsePanelMain"
                },
                {
                    PriceTitle(HQHS_PRICE, PositionMin:"0.85 0.58", PositionMax: "1 0.63", Font: 20),
                    "HorsePanelMain"
                }
            };
        }

        private CuiElementContainer Model(string PanelName,
                                                 string anchorMin,
                                                 string anchorMax,
                                                 string anchorImageMin,
                                                 string anchorImageMax,
                                                 string Name,
                                                 string URL,
                                                 string ItemConsoleName,
                                                 int Price)
        {
            return new CuiElementContainer()
            {
                {
                    new CuiPanel
                    {
                        Image = { Color = "0 0 0 0" },
                        RectTransform = { AnchorMin = anchorMin, AnchorMax = anchorMax }
                    },
                    Overlay,
                    $"{PanelName}Panel"
                },
                {
                    TitlePanel(),
                    $"{PanelName}Panel",
                    $"{PanelName}PanelTitlePanel"
                },
                {
                    TitleForPanel(Name),
                    $"{PanelName}PanelTitlePanel"
                },
                {
                    PanelMain(),
                    $"{PanelName}Panel",
                    $"{PanelName}PanelMain"
                },
                {
                    CreateImage($"{PanelName}PanelMain", URL, anchorImageMin, anchorImageMax)
                },
                {
                    BuyButton(ItemConsoleName),
                    $"{PanelName}PanelMain"
                },
                {
                    PriceTitle(Price),
                    $"{PanelName}PanelMain"
                },
                {
                    CreateImage($"{PanelName}PanelMain", "SCRAP", "0.65 0.2", "0.93 0.35")
                }

            };
        }

        private CuiPanel TitlePanel()
        {
            return new CuiPanel()
            {
                Image = { Color = "0 0 0 0.9", Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                RectTransform = { AnchorMin = "0 0.9", AnchorMax = "1 1" }
            };
        }

        private CuiPanel PanelMain()
        {
            return new CuiPanel()
            {
                Image = { Color = "0 0 0 0.9", Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                RectTransform = { AnchorMin = "0 0", AnchorMax = "1 0.888" }
            };
        }

        private CuiLabel TitleForPanel(string Title)
        {
            return new CuiLabel
            {
                Text = { Text = $"{Title}", FontSize = 20, Align = TextAnchor.MiddleCenter },
                RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
            };
        }

        #region Цена и покупка
        private CuiButton BuyButton(string Vehicle, string Text = "Купить", string anchorMin = "0 0", string anchorMax = "1 0.13", string color = "0 0.59 0 0.7", TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            return new CuiButton
            {
                Button = { Command = $"testmarker.buyvehicle {Vehicle}", Color = color, Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                Text = { Text = Text, FontSize = 20, Align = anchor },
                RectTransform = { AnchorMin = anchorMin, AnchorMax = anchorMax }
            };
        }

        private CuiLabel PriceTitle(int Price, string PositionMin = "0.2 0.2", string PositionMax = "1 0.33", int Font = 40)
        {
            return new CuiLabel
            {
                Text = { Text = $"{Price}", FontSize = Font, Align = TextAnchor.MiddleLeft },
                RectTransform = { AnchorMin = PositionMin, AnchorMax = PositionMax },
            };
        }

        #endregion

        private CuiElementContainer CloseButton()
        {
            return new CuiElementContainer()
            {
                { new CuiPanel()
                    {
                        Image = { Color = "0 0 0 0" },
                        RectTransform = { AnchorMin = "0.95 0.5", AnchorMax = "0.95 0.5", OffsetMin = "-45 -255", OffsetMax = "45 303" },
                        CursorEnabled = true
                    },
                    "Hud",
                    "CloseOverlay"
                },
                {
                    new CuiButton
                    {
                        Button = { Command = "testmarker.close", Color = "0 0 0 0" },
                        Text = { Text = "" },
                        RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1"}
                    },
                    "CloseOverlay"
                },
                {
                    CreateImageWithOffset(Overlay, "CLOSE", "1.05 0.5", "1.05 0.5", "-45 -45", "45 45")
                }
            };
        }

        private CuiElement CreateImageWithOffset(string panelName, string url, string anchorMin, string anchorMax, string offsetMin, string offsetMax)
        {
            var element = new CuiElement();
            var image = new CuiRawImageComponent
            {
                Png = (string)ImageLibrary.Call("GetImage", url),
                Color = "1 1 1 1"
            };

            var rectTransform = new CuiRectTransformComponent
            {
                AnchorMin = anchorMin,
                AnchorMax = anchorMax,
                OffsetMin = offsetMin,
                OffsetMax = offsetMax
            };
            element.Components.Add(image);
            element.Components.Add(rectTransform);
            element.Name = CuiHelper.GetGuid();
            element.Parent = panelName;

            return element;
        }

        private CuiElement CreateImage(string panelName, string url, string anchorMin, string anchorMax)
        {
            var element = new CuiElement();
            var image = new CuiRawImageComponent
            {
                Png = (string)ImageLibrary.Call("GetImage", url),
                Color = "1 1 1 1"
            };

            var rectTransform = new CuiRectTransformComponent
            {
                AnchorMin = anchorMin,
                AnchorMax = anchorMax
            };
            element.Components.Add(image);
            element.Components.Add(rectTransform);
            element.Name = CuiHelper.GetGuid();
            element.Parent = panelName;

            return element;
        }
    }
}
