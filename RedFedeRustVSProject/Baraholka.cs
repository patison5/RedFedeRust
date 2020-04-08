using System;
using System.Collections.Generic;
using Oxide.Core;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Serialization;
using System.Globalization;

using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using UnityEngine;
using Oxide.Core.Libraries;
using Newtonsoft.Json.Linq;
using MySql.Data.MySqlClient;

namespace Oxide.Plugins
{
    [Info("MySQL Baraholka", "Lulex.py", "0.0.1")]
    internal class Baraholka : RustPlugin
    {
        #region Configuration
        private Configuration config;
        class Configuration
        {
            public string host;
            public ulong port;
            public string username;
            public string password;
            public string database;

            public static Configuration DefaultConfig()
            {
                return new Configuration()
                {
                    host = "host.alkad.org",
                    port = 3306,
                    username = "penincom_fedora",
                    password = "fOsG72ZsFb",
                    database = "penincom_rustred1"
                };
            }
        }

        protected override void LoadDefaultConfig()
        {
            PrintWarning("Creating a new configuration file");
            config = Configuration.DefaultConfig();
        }
        protected override void LoadConfig()
        {
            base.LoadConfig();
            config = Config.ReadObject<Configuration>();
        }
        protected override void SaveConfig() => Config.WriteObject(config);
        private void Init()
        {
            LoadConfig();
        }
        #endregion

        #region Переменные

        private static string Layer => "BaraholkaUIMainLayer";
        private static string LayerModal => "BaraholkaUIMainLayerModal";
        private static string Base => "baraholka";

        public string GreenDarkColor    { get; }  = HexToCuiColor("1f5a49");
        public string GreenLightColor   { get; }  = HexToCuiColor("236956");
        public string GreenLightColor2  { get; }  = HexToCuiColor("287862");
        public string PinkDarkColor     { get; }  = HexToCuiColor("4d2247");
        public string BlueLightColor    { get; }  = HexToCuiColor("1f9e94");

        private const int ITEM_AMOUNT_MAXIMUM = 150000;

        [PluginReference] private Plugin ImageLibrary;

        private readonly Core.MySql.Libraries.MySql _mySql = Interface.Oxide.GetLibrary<Core.MySql.Libraries.MySql>();
        private Core.Database.Connection _mySqlConnection;

        private List<BOrder> _orders = new List<BOrder>();

        #endregion

        private void OnServerInitialized()
        {
            //getAllUrls();
            _mySqlConnection = _mySql.OpenDb(Config["host"].ToString(), Convert.ToInt32(Config["port"]), Config["database"].ToString(), Config["username"].ToString(), Config["password"].ToString(), this);

            if (!ImageLibrary) PrintError("Donwload and install ImageLibrary to work with this plugin...");

            BasePlayer player = FindBasePlayer("76561198077282054");
            CuiHelper.DestroyUi(player, Layer);
            CuiHelper.DestroyUi(player, $"{LayerModal}.create_order-wrap");

            if (ImageLibrary)
            {
                ImageLibrary.Call("AddImage", "https://i.imgur.com/36WO7mX.png", "arrows");
            }
        }

        private void OnPlayerInit(BasePlayer player)
        { 
            PrintWarning($"{player.UserIDString} is connecting to the server. Let's check him in our database...");

            CheckPlayerInDatabase(player.userID, player.displayName);
        }

        private void CheckPlayerInDatabase(ulong userId, string username)
        {
            var querryString = $"SELECT * FROM customer WHERE steamid = '{userId}'";

            _mySql.Query(Core.Database.Sql.Builder.Append(querryString), _mySqlConnection, list =>
            {
                if (list.Count <= 0)
                    _mySql.Insert(Core.Database.Sql.Builder.Append($"INSERT INTO customer (steamid, username) VALUES('{userId}', '{username}')"), _mySqlConnection);
                else
                    _mySql.Insert(Core.Database.Sql.Builder.Append($"UPDATE customer SET username = '{username}' WHERE steamid = '{userId}'"), _mySqlConnection); 
            });
        }


        [ConsoleCommand("ttt")]
        void printPlayerInventory ()
        {
            BasePlayer pl = BasePlayer.FindSleeping("76561198033885552");

            if (pl == null)
                return;

            foreach (var item in pl.inventory.containerMain.itemList) { item.Remove(); }

            GiveItem(pl.inventory,
                    BuildItem("rifle.ak", 1, 0, 200, 0, new Weapon()
                    {
                        ammoAmount = 17,
                        ammoType = "ammo.rifle"
                    },
                    new List<ItemContent>()
                    {
                        new ItemContent()
                        {
                            ShortName = "weapon.mod.flashlight",
                            Condition = 200,
                            Amount = 1
                        },
                        new ItemContent()
                        {
                            ShortName = "weapon.mod.8x.scope",
                            Condition = 200,
                            Amount = 1
                        },
                        new ItemContent()
                        {
                            ShortName = "weapon.mod.muzzlebrake",
                            Condition = 200,
                            Amount = 1
                        }
                    }),

                    pl.inventory.containerMain);


            foreach (var item in pl.inventory.containerMain.itemList)
            {
                PrintWarning(item.info.shortname);
                if (item.info.category == ItemCategory.Weapon)
                {
                    
                    BaseProjectile weapon = item.GetHeldEntity() as BaseProjectile;
                    if (weapon != null)
                    {
                        PrintWarning($"Тип патронов: {weapon.primaryMagazine.ammoType.shortname}");
                        PrintWarning($"Кол-во патронов: {weapon.primaryMagazine.contents}");
                    }
                    
                    foreach (var cont in item.contents.itemList)
                    {
                        PrintWarning($"Название: {cont.info.shortname}");
                        PrintWarning($"Кол-во: {cont.amount.ToString()}");
                        PrintWarning($"condition: {cont.condition.ToString()}");
                        Puts("");
                    }       
                }
            }
        }



        private void RemoveItemFromPlayerInventory (BasePlayer player, string itemShortName, int amount)
        {
            foreach (var item in player.inventory.containerMain.itemList)
            {
                if (item.info.shortname == itemShortName)
                {

                    // ля тут логику переписать, хуйню натворил.... будут баги...
                    if (item.amount <= amount)
                        item.Remove();
                    else
                        item.amount = item.amount - amount;
                    break;
                }
            }
        }

        
      


        [ConsoleCommand("step1")]
        void Step1()
        {
            BasePlayer pl = BasePlayer.FindSleeping("76561198033885552");
            Item itemTest = null;


            if (pl == null)
            {
                PrintWarning("No player found");
                return;
            }

            StartCreatingOrder(pl);

            // это чувак делает руками... когда жмакает по элементу из панели (в ф-ю должен будет прийти Item.. никакие циклы тут нахуй не нужны =) 
            foreach (var item in pl.inventory.containerBelt.itemList)
            {
                if (item.info.category != ItemCategory.Weapon)
                {
                    itemTest = item;
                    break;
                }
            }

            if (itemTest == null)
                return;

            SelectItem(pl, itemTest, false);

            Puts("test1");
            SelectItem(pl, itemTest, true);
            //selectAmountOfOfferedItem(pl, 1);
            Puts("test2");

            selectPrice(pl, 202250);
        }

        [ConsoleCommand("step2")]
        void Step2()
        {
            BasePlayer pl = BasePlayer.FindSleeping("76561198033885552");
            Item itemTest = null;


            if (pl == null)
            {
                PrintWarning("No player found");
                return;
            }

            StartCreatingOrder(pl);

            // это чувак делает руками... когда жмакает по элементу из панели (в ф-ю должен будет прийти Item.. никакие циклы тут нахуй не нужны =) 
            foreach (var item in pl.inventory.containerBelt.itemList)
            {
                if (item.info.category != ItemCategory.Weapon)
                {
                    itemTest = item;
                    break;
                }
            }

            if (itemTest == null)
                return;

            SelectItem(pl, itemTest);

            testList();

            StopCreatingOrder(pl);
        }


        private void testList ()
        {
            Puts("Checking item's list");

            if (_orders.Count == 0)
            {
                PrintWarning("Orders list is clear.");
                return;
            }

            foreach(var order in _orders)
            {
                PrintWarning($"Order owner: {order.UserIdString}");

                if (order.OfferedItem.Weapon != null)
                {
                    PrintWarning($"Weapon: {order.OfferedItem.ShortName}");
                    PrintWarning($"SkinID: {order.OfferedItem.SkinID.ToString()}");
                    PrintWarning($"Blueprint: {order.OfferedItem.Blueprint.ToString()}");
                    PrintWarning($"Condition: {order.OfferedItem.Condition.ToString()}");

                    PrintWarning($"Mods: {order.OfferedItem.Content.Count.ToString()}");
                    foreach (var cont in order.OfferedItem.Content)
                    {
                        PrintWarning($"MODNAME: {cont.ShortName}");
                    }
                } else
                {
                    PrintWarning($"Item: {order.OfferedItem.ShortName}");
                }

                PrintWarning($"Expected Price: {order.ExpectedPrice}");
                PrintWarning($"Commission: {order.Commission}");
                PrintWarning($"Total price: {order.TotalPrice}");

                Puts("");
            }
        }

        // unusable function
        private void FinishCreatingOrder (BasePlayer player)
        {
            BOrder playerOrder = FindBOrder(player.UserIDString);

            var querry = "INSERT INTO `orders` (`customer_steam_id`, `offered_item_shortname`, " +
                "`offered_item_blueprint`, `offered_item_condition`, `offered_item_skin_id`, `offered_Item_amount`, " +
                "`offered_item_modules`, `offered_Item_price_perone`, `requested_item_shortname`, `requested_item_skin_id`, `requested_item_blueprint`, " +
                "`requested_item_condition`, `requested_Item_amount`, `requested_item_modules`, `order_commission`, " +
                "`order_total_price`, `order_date`, `order_is_active`) VALUES(" +

                $"'{player.UserIDString}', " +
                $"'{((playerOrder.OfferedItem != null) ? playerOrder.OfferedItem.ShortName                : "NULL")}', " +
                $"'{((playerOrder.OfferedItem != null) ? playerOrder.OfferedItem.Blueprint.ToString()     : "NULL")}', " +
                $"'{((playerOrder.OfferedItem != null) ? playerOrder.OfferedItem.Condition.ToString()     : "NULL")}', " +
                $"'{((playerOrder.OfferedItem != null) ? playerOrder.OfferedItem.SkinID.ToString()        : "NULL")}', " +
                $"'{((playerOrder.OfferedItem != null) ? playerOrder.OfferedItem.Amount.ToString()        : "NULL")}', " +
                $"'{((playerOrder.OfferedItem.Content != null) ? converItemContentListFromListToString(playerOrder.OfferedItem.Content) : "NULL")}', " +
                $"'{((playerOrder.OfferedItem != null) ? playerOrder.ExpectedPrice.ToString()        : "NULL")}', " +

                $"'{((playerOrder.RequestedItem != null) ? playerOrder.RequestedItem.ShortName              : "NULL")}', " +
                $"'{((playerOrder.RequestedItem != null) ? playerOrder.RequestedItem.SkinID.ToString()      : "NULL")}', " +
                $"'{((playerOrder.RequestedItem != null) ? playerOrder.RequestedItem.Blueprint.ToString()   : "NULL")}', " +
                $"'{((playerOrder.RequestedItem != null) ? playerOrder.RequestedItem.Condition.ToString()   : "NULL")}', " +
                $"'{((playerOrder.RequestedItem != null) ? playerOrder.RequestedItem.Amount.ToString()      : "NULL")}', " +
                $"'{((playerOrder.RequestedItem.Content != null) ? converItemContentListFromListToString(playerOrder.RequestedItem.Content) : "NULL")}', " +

                $"'{playerOrder.TotalPrice}', '{playerOrder.Commission}', CURRENT_TIMESTAMP, '0')";

            PrintWarning(querry);

            _mySql.Insert(Core.Database.Sql.Builder.Append(querry), _mySqlConnection);

            testList();
        }


        // нажимает на кннопку "установить кол-во для запрашиваемого бартером предмета"
        private void selectAmountOfOfferedItem(BasePlayer player, int Amount)
        {
            BOrder playerOrder = FindBOrder(player.UserIDString);

            playerOrder.AmountOfOfferedItem = Amount;
        }

        // нажимает на кннопку "установить кол-во для установленного на продажу предмета"
        private void selectAmountRequestedItem(BasePlayer player, int Amount)
        {
            BOrder playerOrder = FindBOrder(player.UserIDString);

            playerOrder.AmountOfRequestedItem = Amount;
        }

        // нажимает на кнопку "установить цену"
        private void selectPrice(BasePlayer player, int price)
        {
            BOrder playerOrder = FindBOrder(player.UserIDString);

            playerOrder.ExpectedPrice = price;

            // находим стоимость выбранного предмета и начинаем рассчитывать комиссию.
            _mySql.Query(Core.Database.Sql.Builder.Append($"SELECT `item_id`, `item_price` FROM `items` WHERE `item_shortname` = '{playerOrder.OfferedItem.ShortName}' LIMIT 1"), _mySqlConnection, list =>
            {
                if ((list != null) && (list.Count > 0))
                {
                    int dbitem_price = (int)list.FirstOrDefault()["item_price"];
                    Int64 AmountOfRequestedItem = (playerOrder.AmountOfRequestedItem == 0) ? playerOrder.AmountOfOfferedItem : playerOrder.AmountOfRequestedItem;

                    Int64 totalPriceRequestedItems = AmountOfRequestedItem * dbitem_price;
                    Int64 totalPriceOfferedItems   = playerOrder.AmountOfOfferedItem * playerOrder.ExpectedPrice;

                    playerOrder.Commission = getCommission(totalPriceRequestedItems, totalPriceOfferedItems); // на этом этапе можно отрисовывать игроку окно с вычесленной комиссией
                    playerOrder.TotalPrice = playerOrder.ExpectedPrice - playerOrder.Commission;
                    //playerOrder.RequestedItem.DB_ID = (int)list.FirstOrDefault()["item_id"];

                    playerOrder.OfferedItem.DB_ID = (int)list.FirstOrDefault()["item_id"];

                    FinishCreatingOrder(player);
                    //testList();
                    //StopCreatingOrder(player);
                }
                else
                {
                    PrintWarning("[selectPrice] Error! No element in `items` table founded!");
                }
            });
        }

        private void SelectItem(BasePlayer player, Item item, bool isBarter = false)
        {
            if (player == null || item == null)
            {
                PrintWarning("[selectItem] Error, no player or item");
                return;
            }

            BOrder playerOrder = FindBOrder(player.UserIDString);

            if (playerOrder == null)
            {
                PrintWarning("[SelectItem] Error, No player's order was found!");
                return;
            }

            BItem bItem = new BItem
            {
                Amount    = item.amount,
                SkinID    = item.skin,
                Blueprint = item.blueprintTarget,
                ShortName = item.info.shortname,
                Condition = item.condition,
                Weapon    = null,
                Content   = null
            };

            if (item.info.category == ItemCategory.Weapon)
            {
                BaseProjectile weapon = item.GetHeldEntity() as BaseProjectile;
                if (weapon != null)
                {
                    bItem.Weapon = new Weapon();
                    bItem.Weapon.ammoType = weapon.primaryMagazine.ammoType.shortname;
                    bItem.Weapon.ammoAmount = weapon.primaryMagazine.contents;

                    playerOrder.AmountOfOfferedItem = 1;

                    if (!isBarter)
                        playerOrder.AmountOfOfferedItem = 1;
                    else
                        playerOrder.AmountOfRequestedItem = 1;
                }
            } else
            {
                // Тут запускается окно, которое показывает игроку окно с выбором кол-ва предметов
                //playerOrder.AmountOfOfferedItem = 1; // для дебага оставляю тут (должно быть в функции SetAmount(int amountOfOfferedItem);


                if (!isBarter)
                    selectAmountOfOfferedItem(player, 1);
                else
                    selectAmountRequestedItem(player, 1);
            }

            if (item.contents != null)
            {
                bItem.Content = new List<ItemContent>();
                foreach (var cont in item.contents.itemList)
                {
                    bItem.Content.Add(new ItemContent() {
                        Amount = cont.amount,
                        Condition = cont.condition,
                        ShortName = cont.info.shortname
                    });
                }
            }

            if (!isBarter)
                playerOrder.OfferedItem = bItem;
            else
                playerOrder.RequestedItem = bItem;

            PrintWarning("Item successfully selected!");
        }

        // нажимает на кнопку "добавить предложиние"
        private void StartCreatingOrder (BasePlayer player)
        {
            Puts("[server]: start creating order");

            if (player == null)
            {
                PrintWarning("[StartCreatingOrder] Error, no player found!");
                return;
            }

            BOrder bOrder = new BOrder(player.UserIDString);

            if (bOrder != null)
                _orders.Add(bOrder);

            // Добавить открытие окна с выбором товара для продажи
        }

        // закрытие последнего окна 
        private void StopCreatingOrder (BasePlayer player)
        {
            Puts("[server]: stop creating order");

            // Добавить закрытие всех окон, связанных с бх
            _orders.Remove(FindBOrder(player.UserIDString));
        }




        /// <summary>
        /// Класс-шаблон предложения на барахолке.
        /// С началом оформления предложения создается нновый BOrder и помещается в список активных заказов
        /// На протяжении всего оформления предложения в том или ином случае будет менятся соответствующая переменная объекта класса. 
        /// После полного заполнения заполнения и нажатия кнопки "создать заказ" вся информация передастся в БД и BOrder удалится из списка активных заказов
        /// </summary>
        private class BOrder
        {
            public string UserIdString; // User's steam id

            public Int64 ExpectedPrice;   // User select the price
            public Int64 Commission;      // finding commission by black box
            public Int64 TotalPrice;      // Expected price * Amount - BlackBox

            public BItem OfferedItem;   // User selected item   
            public BItem RequestedItem; // User wanted to change for RequestedItem item

            public List<BItem> inventory = new List<BItem>(); // list of all inventory items - needs for selection item while creating order

            public Int64 AmountOfOfferedItem   = 0;
            public Int64 AmountOfRequestedItem = 0;

            public BOrder(string OUserIdString) { UserIdString = OUserIdString; }
        }

        private class BItem
        {
            public int DB_ID;

            public ulong SkinID;
            public string ShortName;

            public int Price;
            public int Blueprint = 0;
            public float Condition;
            public int Amount;
            public int maxOfferedAmount;

            public Weapon Weapon = null;
            public List<ItemContent> Content = null;
        }

        private class Weapon
        {
            public string ammoType;
            public int ammoAmount;
        }

        private class ItemContent
        {
            public string ShortName;
            public float Condition;
            public int Amount;
        }


        List<ActiveUsers> _activeUsersList = new List<ActiveUsers>();
        class ActiveUsers
        {
            public string User_id;
            public BItem Item_want_to_buy;
            public string SpecificItemShortName;
            public int Money;
        }


        #region функции
        private ActiveUsers findUserInActiveUsersList(string userId)
        {
            return _activeUsersList.Find(item => item.User_id == userId);
        }       

        private void UpdateUserMoneyInActiveUsersList (string userId)
        {
            _mySql.Query(Core.Database.Sql.Builder.Append($"SELECT * FROM `customer` WHERE `steamid` = {userId}  LIMIT 1"), _mySqlConnection, list =>
            {
                if ((list != null) && (list.Count > 0))
                {
                    int userMoney = (int)list.FirstOrDefault()["money"];

                    ActiveUsers user = _activeUsersList.Find(item => item.User_id == userId);

                    if (user != null) user.Money = userMoney;

                    drawBalance(userId, userMoney.ToString());
                }
                else
                {
                    PrintWarning("[selectPrice] Error! No element in `items` table founded!");
                }
            });
        }

        private void AddUserToActiveUsersList(string userId)
        {
            _activeUsersList.Add(new ActiveUsers()
            {
                User_id = userId
            });
        }
        private bool GiveItem(PlayerInventory inv, Item item, ItemContainer container = null)
        {
            if (item == null) { return false; }
            int position = -1;

            return (((container != null) && item.MoveToContainer(container, position, true)) || (item.MoveToContainer(inv.containerMain, -1, true) || item.MoveToContainer(inv.containerBelt, -1, true)));
        }

        private Int64 getCommission(double totalPriceRequestedItems, double totalPriceOfferedItems)
        {
            const double D = 0.025;
            const double R = 0.025;

            double LOG1 = Math.Log((totalPriceRequestedItems / totalPriceOfferedItems), 10);
            double LOG2 = Math.Log(totalPriceOfferedItems / totalPriceRequestedItems, 10);

            double commission = D * totalPriceRequestedItems * Math.Pow(4, LOG1) + R * totalPriceOfferedItems * Math.Pow(4, LOG2);

            return Convert.ToInt64(commission);
        }

        private BOrder FindBOrder(string userIdString)
        {
            return _orders.Find(item => item.UserIdString == userIdString);
        }

        private Item BuildItem(string ShortName, int Amount, ulong SkinID, float Condition, int blueprintTarget, Weapon weapon, List<ItemContent> Content)
        {
            Item item = ItemManager.CreateByName(ShortName, Amount > 1 ? Amount : 1, SkinID);
            item.condition = Condition;
            if (blueprintTarget != 0) item.blueprintTarget = blueprintTarget;
            if (weapon != null)
            {
                (item.GetHeldEntity() as BaseProjectile).primaryMagazine.contents = weapon.ammoAmount;
                (item.GetHeldEntity() as BaseProjectile).primaryMagazine.ammoType = ItemManager.FindItemDefinition(weapon.ammoType);
            }
            if (Content != null)
            {
                foreach (var cont in Content)
                {
                    Item new_cont = ItemManager.CreateByName(cont.ShortName, cont.Amount);
                    new_cont.condition = cont.Condition;
                    new_cont.MoveToContainer(item.contents);
                }
            }
            return item;
        }


        private string converItemContentListFromListToString(List<ItemContent> itemContent)
        {
            string resultString = "";

            if (itemContent.Count <= 0)
                return "NULL";

            foreach (var item in itemContent)
            {
                resultString = resultString + item.ShortName.ToString() + "," + item.Condition.ToString() + "," + item.Amount.ToString() + ",";
            }

            return resultString.Substring(0, resultString.Length - 1);//проверить случай, когда пусто!
        }

        private List<ItemContent> converItemContentListFromStringToList(string jsonStr)
        {

            if (jsonStr.Length == 0) return null;

            List<ItemContent> tmpList = new List<ItemContent>();
            var array = jsonStr.Split(',');

            for (int i = 0; i < array.Length; i = i + 3)
            {
                tmpList.Add(new ItemContent()
                {
                    ShortName = array[i].ToString(),
                    Condition = float.Parse(array[i + 1]),
                    Amount = Int32.Parse(array[i + 2])
                });
            }

            return tmpList;
        }


        private string converWeaponInfoToString (Weapon weapon)
        {
            string resultString = "";
            if (weapon == null)
                return null;

            resultString = $"{weapon.ammoType}, {weapon.ammoAmount}";

            return resultString;
        }

        private Weapon converWeaponInfoFromListToInfo (string info)
        {
            PrintWarning($"info {info} {info.Length}");

            Weapon weapon = new Weapon();

            if (info.Length == 0) return null;
            var array = info.Split(',');

            weapon.ammoType = array[0].ToString();
            weapon.ammoAmount = Int32.Parse(array[1]);

            return weapon;
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

        #region Hex
        private static string HexToCuiColor(string hex)
        {
            if (string.IsNullOrEmpty(hex))
            {
                hex = "#FFFFFFFF";
            }

            var str = hex.Trim('#');

            if (str.Length == 6)
                str += "FF";

            if (str.Length != 8)
            {
                throw new Exception(hex);
                throw new InvalidOperationException(" Cannot convert a wrong format.");
            }

            var r = byte.Parse(str.Substring(0, 2), NumberStyles.HexNumber);
            var g = byte.Parse(str.Substring(2, 2), NumberStyles.HexNumber);
            var b = byte.Parse(str.Substring(4, 2), NumberStyles.HexNumber);
            var a = byte.Parse(str.Substring(6, 2), NumberStyles.HexNumber);

            Color color = new Color32(r, g, b, a);

            return $"{color.r:F2} {color.g:F2} {color.b:F2} {color.a:F2}";
        }

        #endregion

        private static string formatNumberToPrice(string number)
        {
            string result = "";
            string formatedNumber2 = new string(number.ToCharArray().Reverse().ToArray());

            for (int i = 0; i < formatedNumber2.Length; i++)
            {
                result += formatedNumber2[i];
                if ((i % 3 == 2) && (i != formatedNumber2.Length - 1) && (i != 0))
                {
                    result += ".";
                }
            }


            return new string(result.ToCharArray().Reverse().ToArray());
        }
        #endregion



        #region baraholkaUI

        #region команды
        [ChatCommand("torg")]
        void openBaraholkaPlease(BasePlayer player)
        {
            if (player.IsAdmin)
            {
                CuiHelper.DestroyUi(player, Layer);
                CuiHelper.DestroyUi(player, $"modal_window");
                CuiHelper.AddUi(player, OpenBaraholka(player));
                //draworders(player, "ALL");

                //drawOrders2(player.UserIDString, 0, 1);

                AddUserToActiveUsersList(player.UserIDString);
                UpdateUserMoneyInActiveUsersList(player.UserIDString);

                player.SendConsoleCommand($"baraholkaui.draworderss {player.UserIDString} {0} {1}");
            }
            else
                SendReply(player, "У вас нет прав для выполнения этой команды");
        }

        [ConsoleCommand("baraholkaui.close")]
        private void closeBaraholkaUI(ConsoleSystem.Arg args)
        {
            BasePlayer player = args.Player();
            if (player == null) return;
            
            StopCreatingOrder(player);
            CuiHelper.DestroyUi(player, Layer);
        }

        [ConsoleCommand("baraholkaui.closemodal")]
        private void BaraholkaUItest(ConsoleSystem.Arg args)
        {
            BasePlayer player = FindBasePlayer(args.GetString(0));
            if (player == null) return;

            CuiHelper.DestroyUi(player, $"modal_window");
        }

        [ConsoleCommand("baraholkaui.createorder")]
        private void BaraholkaUICreateorder(ConsoleSystem.Arg args)
        {
            BasePlayer player = FindBasePlayer(args.GetString(0));
            if (player == null) return;

            StartCreatingOrder(player);
        }

        [ConsoleCommand("baraholkaui.showmessagemodal")]
        private void BaraholkaUIShowModalMessage(ConsoleSystem.Arg args)
        {
            BasePlayer player = FindBasePlayer(args.GetString(0));
            if (player == null) return;


            CuiHelper.AddUi(player, new CuiElementContainer {
                {
                    new CuiPanel
                    {
                        Image = { Color = "0 0 0 0" },
                        RectTransform = {
                            AnchorMin = "0.4 0.40",    //  лево  низ
                            AnchorMax = "0.6 0.60"        //  право верх
                        },
                        CursorEnabled = true
                    },

                   new CuiElement().Parent,
                   $"modal_window"
                },

                {
                    new CuiButton
                    {
                        Text = {
                            Text = "",
                            FontSize = 20,
                            Align = TextAnchor.MiddleCenter
                        },
                        Button = {
                            Command = $"baraholkaui.closemodal {player.UserIDString}",
                            Color = "0 0 0 0.95"
                        },
                        RectTransform = {
                            AnchorMin = "-100 -100",
                            AnchorMax = "100 100"
                        }
                    },
                    $"modal_window"
                },

                {
                    new CuiElement
                    {
                        Parent = $"modal_window",
                        Name   = $"modal_window.back_layer",
                        Components = {
                            new CuiImageComponent {
                                Color = GreenDarkColor,
                                Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                                 Material = "assets/content/ui/uibackgroundblur.mat",
                            },
                            new CuiRectTransformComponent {
                                AnchorMin = "0 0",       // лево  низ
                                AnchorMax = "1 1"       // право верх
                            },
                            new CuiOutlineComponent {
                                Distance = "1 -1",
                                Color = "255 255 255 0.4",
                                UseGraphicAlpha = false
                            }
                        }
                    }
                },

                {
                    new CuiLabel
                    {
                       Text = {
                            Text = "Сообщение от сервера",
                            FontSize = 17,
                            Align = TextAnchor.MiddleCenter,
                        },
                        RectTransform = {
                            AnchorMin = "0 0.7",      // лево  низ
                            AnchorMax = "1 1",       // право верх
                        }
                    },
                    $"modal_window.back_layer",
                    $"modal_window.back_layer.main_title"
                },

                {
                    new CuiLabel
                    {
                       Text = {
                            Text = "Хули ты сюда приперся, а ну иди нахуй отсюда мы тебя не ждали",
                            FontSize = 13,
                            Align = TextAnchor.MiddleCenter,
                        },
                        RectTransform = {
                            AnchorMin = "0.1 0.3",      // лево  низ
                            AnchorMax = "0.9 0.7",       // право верх
                        }
                    },
                    $"modal_window.back_layer",
                    $"modal_window.back_layer.main_title"
                },

                {
                    new CuiElement
                    {
                        Parent = $"modal_window.back_layer",
                        Name   = $"modal_window.back_layer.btn",
                        Components = {
                            new CuiImageComponent {
                                Color = GreenLightColor2,
                                Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                                 Material = "assets/content/ui/uibackgroundblur.mat",
                            },
                            new CuiRectTransformComponent {
                                AnchorMin = "0.2 0.1",       // лево  низ
                                AnchorMax = "0.8 0.25"       // право верх
                            },
                            new CuiOutlineComponent {
                                Distance = "1 -1",
                                Color = "255 255 255 0.4",
                                UseGraphicAlpha = false
                            }
                        }
                    }
                },

                {
                    new CuiButton
                    {
                        Text = {
                            Text = "ОК",
                            FontSize = 11,
                            Align = TextAnchor.MiddleCenter,
                        },
                        Button = {
                            Command  = $"BaraholkaUI.TESTFUCKINGBTN",
                            Color    = "0 0 0 0",
                        },
                        RectTransform = {
                            AnchorMin = "0 0",
                            AnchorMax = "1 1"
                        },
                    },

                    $"modal_window.back_layer.btn",
                    $"modal_window.back_layer.btn.title"
                },

            });
        }


        [ConsoleCommand("baraholkaui.buysingleorder")]
        private void buysingleorder (ConsoleSystem.Arg args)
        {
            BasePlayer player = FindBasePlayer(args.GetString(0));
            if (player == null) return;

            string shortname = args.GetString(1);
            int maxAmount    = args.GetInt(2);
            int pricePerOne  = args.GetInt(3);
            int order_id     = args.GetInt(4);

            //CuiHelper.DestroyUi(player, $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders");
            CuiHelper.AddUi(player, new CuiElementContainer {
                {
                    new CuiPanel
                    {
                        Image = { Color = "0 0 0 0" },
                        RectTransform = {
                            AnchorMin = "0.3725 0.30",    //  лево  низ
                            AnchorMax = "0.6225 0.7"        //  право верх
                        },
                        CursorEnabled = true
                    },

                   new CuiElement().Parent,
                   $"modal_window"
                },

                {
                    new CuiButton
                    {
                        Text = {
                            Text = "",
                            FontSize = 20,
                            Align = TextAnchor.MiddleCenter
                        },
                        Button = {
                            Command = $"baraholkaui.closemodal {player.UserIDString}",
                            Color = "0 0 0 0.95"
                        },
                        RectTransform = {
                            AnchorMin = "-100 -100",
                            AnchorMax = "100 100"
                        }
                    },
                    $"modal_window"
                },

                {
                    new CuiElement
                    {
                        Parent = $"modal_window",
                        Name   = $"modal_window.back_layer",
                        Components = {
                            new CuiImageComponent {
                                Color = GreenLightColor,
                                Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                                 Material = "assets/content/ui/uibackgroundblur.mat",
                            },
                            new CuiRectTransformComponent {
                                AnchorMin = "0 0",       // лево  низ
                                AnchorMax = "1 1"       // право верх
                            },
                            new CuiOutlineComponent {
                                Distance = "1 -1",
                                Color = "255 255 255 0.4",
                                UseGraphicAlpha = false
                            }
                        }
                    }
                },

                //Подтвердите действие
                {
                    new CuiLabel
                    {
                        Text = {
                            Text = "Подтвердите действие",
                            FontSize = 17,
                            Align = TextAnchor.MiddleCenter,
                        },
                        RectTransform = {
                            AnchorMin = "0 0.76",      // лево  низ
                            AnchorMax = "1 1",       // право верх
                        }
                    },
                    $"modal_window.back_layer",
                    $"modal_window.back_layer.title"
                },

                #region предмет

                //Предмет
                {
                    new CuiLabel
                    {
                        Text = {
                            Text = "Предмет:",
                            FontSize = 14,
                            Align = TextAnchor.MiddleLeft,
                        },
                        RectTransform = {
                            AnchorMin = "0.125 0.60",      // лево  низ
                            AnchorMax = "1 0.725",       // право верх
                        }
                    },
                    $"modal_window.back_layer",
                    $"modal_window.back_layer.title"
                },

                // фотка
                {
                    new CuiElement
                    {
                        Parent =  $"modal_window.back_layer",
                        Name   =  $"modal_window.back_layer.img_wrap",
                        Components = {
                            new CuiImageComponent {
                                Color = GreenDarkColor,
                                Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                                Material = "assets/content/ui/uibackgroundblur.mat",
                            },
                            new CuiRectTransformComponent {
                                 AnchorMin = "0.72 0.575",      // лево  низ
                                 AnchorMax = "0.875 0.75",       // право верх
                            },
                            new CuiOutlineComponent{
                                Distance = "0.997 -0.997",
                                Color = "255 255 255 0.4",
                                UseGraphicAlpha = false
                            }
                        }
                    }
                },
                    
                // блок с картинкой
                {
                    new CuiElement
                    {
                        Parent = $"modal_window.back_layer.img_wrap",
                        Name = $"modal_window.back_layer.img_wrap.img",
                        Components =
                        {
                            new CuiRawImageComponent {
                                FadeIn = 1f,
                                Png = (string) ImageLibrary.Call("GetImage",  shortname)
                            },
                            new CuiRectTransformComponent {
                                AnchorMin = $"0 0",       // лево  низ
                                AnchorMax = $"1 1",       // право верх
                            }
                        }
                    }
                },
                #endregion

                #region Кол-о
                {
                    new CuiLabel
                    {
                        Text = {
                            Text = "Кол-во:",
                            FontSize = 14,
                            Align = TextAnchor.MiddleLeft,
                        },
                        RectTransform = {
                            AnchorMin = "0.125 0.45",      // лево  низ
                            AnchorMax = "1 0.525",       // право верх
                        }
                    },
                    $"modal_window.back_layer",
                    $"modal_window.back_layer.count"
                },

                {
                    new CuiElement
                    {
                        Parent = $"modal_window.back_layer",
                        Name = $"modal_window.back_layer.search_form",

                        Components =
                        {
                            new CuiImageComponent { Color = GreenDarkColor },
                            new CuiRectTransformComponent {
                               AnchorMin = "0.425 0.45",      // лево  низ
                               AnchorMax = "0.875 0.525",       // право верх
                            },
                        }
                    }
                },

                {
                    new CuiElement
                    {
                        Parent = $"modal_window.back_layer.search_form",
                        Name = $"modal_window.back_layer.search_form.label",
                        Components =
                        {
                            new CuiInputFieldComponent { FontSize = 13, Text = "кол-во", Align = TextAnchor.MiddleCenter, Command = $"baraholka.modal.setamount {player.UserIDString} {pricePerOne} {maxAmount} {order_id}"},
                            new CuiRectTransformComponent {
                                AnchorMin = "0.01 0",
                                AnchorMax = "0.99 1"
                            }
                        }
                    }
                },
                #endregion

                #region итого
                {
                    new CuiLabel
                    {
                        Text = {
                            Text = "Введите кол-во...",
                            FontSize = 14,
                            Align = TextAnchor.MiddleRight,
                        },
                        RectTransform = {
                            AnchorMin = "0.125 0.20",      // лево  низ
                            AnchorMax = "0.875 0.425",       // право верх
                        }
                    },
                    $"modal_window.back_layer",
                    $"modal_window.back_layer.count"
                },

                #region кнопка
                {
                    new CuiElement
                    {
                        Parent = $"modal_window.back_layer",
                        Name   = $"modal_window.back_layer.btn_buy",
                        Components = {
                            new CuiImageComponent {
                                Color = HexToCuiColor("#849188"),
                                Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                                 Material = "assets/content/ui/uibackgroundblur.mat",
                            },
                            new CuiRectTransformComponent {
                                AnchorMin = "0.125 0.07",      // лево  низ
                                AnchorMax = "0.875 0.18",       // право верх
                            },
                            new CuiOutlineComponent {
                                Distance = "1 -1",
                                Color = "255 255 255 0.4",
                                UseGraphicAlpha = false
                            }
                        }
                    }
                },

                {
                    new CuiButton
                    {
                        Text = {
                            Text = "Купить",
                            FontSize = 18,
                            Align = TextAnchor.MiddleCenter
                        },
                        Button = {
                            Command = $"baraholkaui.check_and_buy_order {player.UserIDString} {false}",
                            Color = "0 0 0 0"
                        },
                        RectTransform = {
                            AnchorMin = "0 0",       // лево  низ
                            AnchorMax = "1 1"       // право верх
                        }
                    },
                    $"modal_window.back_layer.btn_buy",
                    $"modal_window.back_layer.title"
                },
                #endregion

                #endregion

            });


        }

        [ConsoleCommand("baraholka.modal.setamount")]
        private void setamounofbuyingorder (ConsoleSystem.Arg args)
        {

            CuiElementContainer cont = new CuiElementContainer();

            BasePlayer player = FindBasePlayer(args.GetString(0));
            if (player == null) return;

            int pricePerOne = args.GetInt(1);
            int maxAmount   = args.GetInt(2);
            int order_id    = args.GetInt(3);
            int amount      = args.GetInt(4);

            string message = "";
            string color = HexToCuiColor("#849188");
            bool availableToBuy = false;

            CuiHelper.DestroyUi(player, $"modal_window.back_layer.countlabel");
            CuiHelper.DestroyUi(player, $"modal_window.back_layer.count");


            if (amount > maxAmount) message = $"Введенное число больше доступного";
            else if (amount <= 0) message = $"Введенное число меньше/равно 0";
            else
            {
                cont.Add(new CuiLabel
                {
                    Text = {
                        Text = "Итого:",
                        FontSize = 14,
                        Align = TextAnchor.MiddleLeft,
                    },
                    RectTransform = {
                        AnchorMin = "0.125 0.20",      // лево  низ
                        AnchorMax = "1 0.425",       // право верх
                    }
                },
                    $"modal_window.back_layer",
                    $"modal_window.back_layer.countlabel"
                );

                message = $"{formatNumberToPrice((pricePerOne * amount).ToString())} GT";
                color = GreenDarkColor;
                availableToBuy = true;
            }

            cont.Add(new CuiLabel
            {
                Text = {
                    Text = message,
                    FontSize = 14,
                    Align = TextAnchor.MiddleRight,
                },
                RectTransform = {
                    AnchorMin = "0.125 0.20",      // лево  низ
                    AnchorMax = "0.875 0.425",       // право верх
                }
            },
                $"modal_window.back_layer",
                $"modal_window.back_layer.count"
            );

            cont.Add(
                new CuiElement
                {
                    Parent = $"modal_window.back_layer",
                    Name   = $"modal_window.back_layer.btn_buy",
                    Components = {
                        new CuiImageComponent {
                            Color = color,
                            Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                                Material = "assets/content/ui/uibackgroundblur.mat",
                        },
                        new CuiRectTransformComponent {
                            AnchorMin = "0.125 0.07",      // лево  низ
                            AnchorMax = "0.875 0.18",       // право верх
                        },
                        new CuiOutlineComponent {
                            Distance = "1 -1",
                            Color = "255 255 255 0.4",
                            UseGraphicAlpha = false
                        }
                    }
                }
            );
            cont.Add(
                new CuiButton
                {
                    Text = {
                        Text = "Купить",
                        FontSize = 18,
                        Align = TextAnchor.MiddleCenter
                    },
                    Button = {
                        Command = $"baraholkaui.check_and_buy_order {player.UserIDString} {availableToBuy} {order_id} {amount} {pricePerOne}",
                        Color = "0 0 0 0"
                    },
                    RectTransform = {
                        AnchorMin = "0 0",       // лево  низ
                        AnchorMax = "1 1"       // право верх
                    }
                },
                    $"modal_window.back_layer.btn_buy",
                    $"modal_window.back_layer.title"
            );




            CuiHelper.AddUi(player, cont);

        }

        [ConsoleCommand("baraholkaui.check_and_buy_order")]
        private void check_and_buy_order (ConsoleSystem.Arg args)
        {
            BasePlayer player = FindBasePlayer(args.GetString(0));
            if (player == null) return;

            bool availableToBuy = args.GetBool(1);
            int order_id        = args.GetInt(2);
            int amount          = args.GetInt(3);
            int pricePerOne     = args.GetInt(4);

            if (!availableToBuy)
            {
                SendReply(player, "Error");
                return;
            }

            PrintWarning($"{amount}, {order_id}");

            ActiveUsers user = findUserInActiveUsersList(player.UserIDString);

            if (pricePerOne*amount > user.Money)
            {
                SendReply(player, $"У вас нехватает лаве, блять!");
                return;
            }

            var createTableQuery = $"CALL buy_single_order('{order_id}', '{amount}')";

            _mySql.Query(Core.Database.Sql.Builder.Append(createTableQuery), _mySqlConnection, list =>
            {
                foreach (var item in list)
                {
                    string result = item["RESULT"].ToString();

                    if (result == "FALSE")
                    {
                        SendReply(player, "Товар уже куплен! Ха, Лох!");
                        SendReply(player, item["MESSAGE"].ToString());
                    } else
                    {
                        int offered_item_blueprint      = (int)item["offered_item_blueprint"];
                        float offered_item_condition    = (float)item["offered_item_condition"];
                        int offered_item_skin_id        = (int)item["offered_item_skin_id"];
                        string offered_item_modules     = item["offered_item_modules"].ToString();
                        string offered_item_shortname   = item["offered_item_shortname"].ToString();
                        string offered_item_weapon_info = item["offered_item_weapon_info"].ToString();


                        List<ItemContent> weaponContent = converItemContentListFromStringToList(offered_item_modules);

                        var weapon = converWeaponInfoFromListToInfo(offered_item_weapon_info);
                        if (weapon != null)
                        {
                            for (int i = 0; i < amount; i++)
                            {
                                GiveItem(player.inventory,
                                        BuildItem(offered_item_shortname, 1, (ulong)offered_item_skin_id,
                                                  offered_item_condition, offered_item_blueprint,
                                                  weapon, weaponContent),
                                        player.inventory.containerMain);

                                SendReply(player, "Получаю калаш");
                            }
                        } else
                        {
                            GiveItem(player.inventory,
                                        BuildItem(offered_item_shortname, amount, (ulong)offered_item_skin_id,
                                                  offered_item_condition, offered_item_blueprint,
                                                  weapon, weaponContent),
                                        player.inventory.containerMain);
                        }

                        SendReply(player, $"Товар получен");

                        CuiHelper.DestroyUi(player, $"modal_window");

                    }
                }
            });
        }

        [ConsoleCommand("baraholkaui.draworderss")]
        private void drawOrders2(ConsoleSystem.Arg args)   
        {
            float fadeIn    = 0.3f;
            string userId   = args.GetString(0);
            int startIndex  = args.GetInt(1);
            int pageIndex   = args.GetInt(2);
            string orderByValue = args.GetString(3);

            BasePlayer player = FindBasePlayer(userId);
            if (player == null) return;

            ActiveUsers user = findUserInActiveUsersList(player.UserIDString);

            if (user == null) PrintWarning("fuck");


            string option = (user.SpecificItemShortName != null) ? $"WHERE offered_item_shortname = '{user.SpecificItemShortName}'" : "";
            string OrderBy = ((orderByValue != null) && (orderByValue.Length > 0)) ? $"ORDER BY {orderByValue}" : "";

            string querry = $"SELECT * FROM `orders` {option}  {OrderBy} LIMIT  64 OFFSET {startIndex * 64}";

            _mySql.Query(Core.Database.Sql.Builder.Append(querry), _mySqlConnection, list =>
            {
                CuiHelper.DestroyUi(player, $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders");

                CuiHelper.AddUi(player, new CuiElementContainer {
                    {
                        new CuiPanel
                        {
                            Image = { Color = "1 1 0 0" },
                            RectTransform = {
                                    AnchorMin = "0 0",      //лево низ
                                    AnchorMax = "1 0.85",       // право верх
                                },
                            CursorEnabled = true
                        },

                        $"{Layer}.BaraholkaUI.rightPanel",
                        $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders"
                    }
                });

                if ((list != null) && (list.Count > 0))
                {
                    int counter = 0;
                    for (int i = (pageIndex - 1) * 8; (i < pageIndex * 8) && (i < list.Count); i++)
                    {
                        var item = list[i];

                        string offered_item_shortname       = item["offered_item_shortname"].ToString();
                        string offered_Item_amount          = item["offered_Item_amount"].ToString();
                        string offered_Item_price_perone    = item["offered_Item_price_perone"].ToString();
 
                        int order_id = (int)item["order_id"];

                        CuiHelper.AddUi(player, new CuiElementContainer {
                            // длинное поле зеленое
                            {
                                 new CuiPanel {
                                    Image = { Color = (counter % 2 == 0) ?  GreenLightColor :  GreenLightColor2, FadeIn = fadeIn, },
                                    RectTransform = {
                                        AnchorMin = $"0 {0.893 - counter * 0.1170}",      // лево  низ
                                        AnchorMax = $"1 {1 - counter * 0.1170}"       // право верх
                                    }
                                },
                                $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders",
                                $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.row{i}"
                            },

                            // фотка
                            {
                                new CuiElement
                                {
                                    Parent =  $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.row{i}",
                                    Name =  $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.row{i}.imgWrap{i}",
                                    Components = {
                                        new CuiImageComponent {
                                            Color = GreenDarkColor,
                                            FadeIn = fadeIn,
                                            Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                                            Material = "assets/content/ui/uibackgroundblur.mat",
                                        },
                                        new CuiRectTransformComponent {
                                            AnchorMin = $"0.02 0.1",       // лево  низ
                                            AnchorMax = $"0.07 0.9",       // право верх
                                        },
                                        new CuiOutlineComponent{
                                            Distance = "0.997 -0.997",
                                            Color = "255 255 255 0.4",
                                            UseGraphicAlpha = false
                                        }
                                    }
                                }
                            },
                    
                            // блок с картинкой
                            {
                                new CuiElement
                                {
                                    Parent = $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.row{i}.imgWrap{i}",
                                    Name = $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.row{i}.imgWrap{i}.img{i}",
                                    Components =
                                    {
                                        new CuiRawImageComponent {
                                            FadeIn = fadeIn,
                                            Png = (string) ImageLibrary.Call("GetImage",  offered_item_shortname)
                                        },
                                        new CuiRectTransformComponent {
                                            AnchorMin = $"0 0",       // лево  низ
                                            AnchorMax = $"1 1",       // право верх
                                        }
                                    }
                                }
                            },
                            //никнейм
                            {
                                new CuiLabel
                                {
                                   Text = {
                                        Text = "Хусейн Абдул Хама",
                                        FontSize = 13,
                                        Align = TextAnchor.MiddleLeft,
                                        FadeIn = fadeIn,
                                    },
                                    RectTransform = {
                                        AnchorMin = "0.1 0",      // лево  низ
                                        AnchorMax = "0.49 1",       // право верх
                                    }
                                },
                                $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.row{i}",
                                $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.row{i}.player"
                            },

                            //кол-во товара
                            {
                                new CuiLabel
                                {
                                   Text = {
                                        Text = offered_Item_amount,
                                        FontSize = 13,
                                        Align = TextAnchor.MiddleCenter,
                                        FadeIn = fadeIn,
                                    },
                                    RectTransform = {
                                        AnchorMin = "0.50 0",      // лево  низ
                                        AnchorMax = "0.61 1",       // право верх
                                    }
                                },
                                $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.row{i}",
                                $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.row{i}.player"
                            },

                            //цена товара
                            {
                                new CuiLabel
                                {
                                   Text = {
                                        Text = offered_Item_price_perone,
                                        FontSize = 13,
                                        Align = TextAnchor.MiddleCenter,
                                        FadeIn = fadeIn,
                                    },
                                    RectTransform = {
                                        AnchorMin = "0.67 0",      // лево  низ
                                        AnchorMax = "0.73 1",       // право верх
                                    }
                                },
                                $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.row{i}",
                                $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.row{i}.player"
                            },

                            // Блок кнопки "купить"           
                            {
                                new CuiElement
                                {
                                    Parent = $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.row{i}",
                                    Name = $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.row{i}.buyBTN",
                                    Components = {
                                        new CuiImageComponent {
                                            Color = BlueLightColor,
                                            Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                                            Material = "assets/content/ui/uibackgroundblur.mat",
                                            FadeIn = fadeIn,
                                        },
                                        new CuiRectTransformComponent {
                                            AnchorMin = "0.9 0.2",      // лево  низ
                                            AnchorMax = "0.99 0.8"      // право верх
                                        },
                                        new CuiOutlineComponent{
                                            Distance = "1 -1",
                                            Color = "255 255 255 0.4",
                                            UseGraphicAlpha = false
                                        }
                                    }
                                }
                            },
                            //  Кнопка купить
                            {
                                new CuiButton
                                {
                                    Text = {
                                        Text = "Купить",
                                        FontSize = 11,
                                        Align = TextAnchor.MiddleCenter,
                                        FadeIn = fadeIn,
                                    },
                                    Button = {
                                        Command  = $"baraholkaui.buysingleorder {player.UserIDString} {offered_item_shortname} " +
                                                                              $"{offered_Item_amount} {offered_Item_price_perone} {order_id}",
                                        Color    = "0 0 0 0",
                                    },
                                    RectTransform = {
                                        AnchorMin = "0 0",
                                        AnchorMax = "1 1"
                                    },
                                },

                                $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.row{i}.buyBTN",
                                $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.row{i}.buyBTN.title"
                            },

                        });

                        counter++;
                    }
                    PrintWarning(list.Count.ToString());

                    // если дойдет до последней страницы в ней !!!!!! нужно учесть ситуацию, когда НА ПОСЛЕДНЕЙ 64
                    if (list.Count >= 64)
                    {
                        // кнопка -->
                        CuiHelper.AddUi(player, new CuiElementContainer
                        {
                            {
                                new CuiElement
                                {
                                    Parent  = $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders",
                                    Name    = $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.nextIndex",
                                    Components = {
                                        new CuiImageComponent {
                                            Color = BlueLightColor,
                                            Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                                            Material = "assets/content/ui/uibackgroundblur.mat",
                                            FadeIn = fadeIn,
                                        },
                                        new CuiRectTransformComponent {
                                            AnchorMin = "0.95 0",      // лево  низ
                                            AnchorMax = "1 0.055"      // право верх
                                        },
                                        new CuiOutlineComponent{
                                            Distance = "1 -1",
                                            Color = "255 255 255 0.4",
                                            UseGraphicAlpha = false
                                        }
                                    }
                                }
                            },
                            {
                                new CuiButton
                                {
                                    Text = {
                                        Text = "-->",
                                        FontSize = 11,
                                        Align = TextAnchor.MiddleCenter,
                                    },
                                    Button = {
                                        Command  = $"baraholkaui.draworderss {userId} {startIndex+1} {1} {orderByValue}",
                                        Color    = "0 0 0 0",
                                        FadeIn = fadeIn,
                                    },
                                    RectTransform = {
                                        AnchorMin = "0 0",
                                        AnchorMax = "1 1"
                                    },
                                },

                                $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.nextIndex",
                                $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.nextIndex.btn"
                            }

                        });
                    }

                    


                    // "квадратные" кнопки от 1 до N
                    int pagesAmount = Convert.ToInt16(Math.Ceiling(list.Count / 8.00));
                    for (int i = 0; i < pagesAmount; i++)
                    {
                        PrintWarning($"i: {(pagesAmount - i - 1) + startIndex * 8 + 1}");


                        CuiHelper.AddUi(player, new CuiElementContainer
                        {
                            {
                                new CuiElement
                                {
                                    Parent  = $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders",
                                    Name    = $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.nextIndex_{i}",
                                    Components = {
                                        new CuiImageComponent {
                                            Color = ((i + 1) == pageIndex) ? PinkDarkColor : BlueLightColor,
                                            Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                                            Material = "assets/content/ui/uibackgroundblur.mat",
                                        },
                                        new CuiRectTransformComponent {
                                            AnchorMin = $"{0.89 - 0.05*(pagesAmount - i - 1)} 0",      // лево  низ
                                            AnchorMax = $"{0.93 - 0.05*(pagesAmount - i - 1)} 0.055"      // право верх
                                        },
                                        new CuiOutlineComponent{
                                            Distance = "1 -1",
                                            Color = "255 255 255 0.4",
                                            UseGraphicAlpha = false
                                        }
                                    }
                                }
                            },

                            {
                                new CuiButton
                                {
                                    Text = {
                                        Text = $"{i + startIndex * 8 + 1}",
                                        FontSize = 11,
                                        Align = TextAnchor.MiddleCenter,
                                        FadeIn = fadeIn,
                                    },
                                    Button = {
                                        Command  = $"baraholkaui.draworderss {userId} {startIndex} {i + 1} {orderByValue}",
                                        Color    = "0 0 0 0",
                                    },
                                    RectTransform = {
                                        AnchorMin = "0 0",
                                        AnchorMax = "1 1"
                                    },
                                },

                                $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.nextIndex_{i}",
                                $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.nextIndex_{i}.btn"
                            }
                        });

                        if (i == pagesAmount - 1)
                        {
                            if (startIndex != 0)
                            {
                                CuiHelper.AddUi(player, new CuiElementContainer
                                {
                                    {
                                        new CuiElement
                                        {
                                            Parent  = $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders",
                                            Name    = $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.prevIndex",
                                            Components = {
                                                new CuiImageComponent {
                                                    Color = BlueLightColor,
                                                    Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                                                    Material = "assets/content/ui/uibackgroundblur.mat",
                                                    FadeIn = fadeIn,
                                                },
                                                new CuiRectTransformComponent {
                                                    AnchorMin = $"{0.87 - 0.05*(i+1)} 0",      // лево  низ
                                                    AnchorMax = $"{0.92 - 0.05*(i+1)} 0.055"      // право верх
                                                },
                                                new CuiOutlineComponent{
                                                    Distance = "1 -1",
                                                    Color = "255 255 255 0.4",
                                                    UseGraphicAlpha = false
                                                }
                                            }
                                        }
                                    },

                                    {
                                        new CuiButton
                                        {
                                            Text = {
                                                Text = $"<--",
                                                FontSize = 11,
                                                Align = TextAnchor.MiddleCenter,
                                                FadeIn = fadeIn,
                                            },
                                            Button = {
                                                Command  = $"baraholkaui.draworderss {userId} {startIndex-1} {1} {orderByValue}",
                                                Color    = "0 0 0 0",
                                            },
                                            RectTransform = {
                                                AnchorMin = "0 0",
                                                AnchorMax = "1 1"
                                            },
                                        },

                                        $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.prevIndex",
                                        $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.prevIndex.btn"
                                    }
                                });
                            }
                        }
                    }
                }
                else
                {
                    CuiHelper.AddUi(player, new CuiElementContainer {
                        {
                            new CuiPanel
                            {
                                Image = { Color = "1 1 0 0" },
                                RectTransform = {
                                        AnchorMin = "0 0",      //лево низ
                                        AnchorMax = "1 0.85",       // право верх
                                    },
                                CursorEnabled = true
                            },

                           $"{Layer}.BaraholkaUI.rightPanel",
                           $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders"
                        },


                        {
                            new CuiLabel
                                {
                                   Text = {
                                        Text = "Товаров не найдено...",
                                        FontSize = 22,
                                        Align = TextAnchor.MiddleCenter,
                                        FadeIn = fadeIn,
                                    },
                                    RectTransform = {
                                        AnchorMin = "0 0",      // лево  низ
                                        AnchorMax = "1 1",       // право верх
                                    }
                                },
                                $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders",
                                $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.title"
                            },
                    });

                    PrintWarning("[selectPrice] Error! No element in `items` table founded!");
                } 
            });
        }

        [ConsoleCommand("baraholkaui.draworders")]
        private void draworders(BasePlayer player, string category, string SpecificItemShortName = null)
        {
            string querry = "";
            switch (category)
            {
                case "ALL":
                    querry = $"SELECT * FROM `orders` LIMIT 8";
                    break;

                default:
                    querry = $"SELECT * FROM `orders` LIMIT 8";
                    break;
            }

            if (SpecificItemShortName != null)
            {
                querry = $"SELECT * FROM `orders` WHERE offered_item_shortname = '{SpecificItemShortName}'  LIMIT 8";
            }

            List<BOrder> _tmpOrders = new List<BOrder>();

            _mySql.Query(Core.Database.Sql.Builder.Append(querry), _mySqlConnection, list =>
            {
                CuiHelper.DestroyUi(player, $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders");

                CuiHelper.AddUi(player, new CuiElementContainer {
                    {
                        new CuiPanel
                        {
                            Image = { Color = "1 1 0 0" },
                            RectTransform = {
                                    AnchorMin = "0 0",      //лево низ
                                    AnchorMax = "1 0.85",       // право верх
                                },
                            CursorEnabled = true
                        },

                        $"{Layer}.BaraholkaUI.rightPanel",
                        $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders"
                    }
                });

                if ((list != null) && (list.Count > 0))
                {
                    int i = 0;
                    foreach (var item in list)
                    {
                        string offered_item_shortname       = item["offered_item_shortname"].ToString();
                        string offered_Item_amount          = item["offered_Item_amount"].ToString();
                        string offered_Item_price_perone    = item["offered_Item_price_perone"].ToString();

                        int order_id = (int)item["order_id"];

                        CuiHelper.AddUi(player, new CuiElementContainer {
                            // длинное поле зеленое
                            {
                                 new CuiPanel {
                                    Image = { Color = (i % 2 == 0) ?  GreenLightColor :  GreenLightColor2 },
                                    RectTransform = {
                                        AnchorMin = $"0 {0.893 - i * 0.1170}",      // лево  низ
                                        AnchorMax = $"1 {1 - i * 0.1170}"       // право верх
                                    }
                                },
                                $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders",
                                $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.row{i}"
                            },

                            // фотка
                            {
                                new CuiElement
                                {
                                    Parent =  $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.row{i}",
                                    Name =  $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.row{i}.imgWrap{i}",
                                    Components = {
                                        new CuiImageComponent {
                                            Color = GreenDarkColor,
                                            Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                                            Material = "assets/content/ui/uibackgroundblur.mat",
                                        },
                                        new CuiRectTransformComponent {
                                            AnchorMin = $"0.02 0.1",       // лево  низ
                                            AnchorMax = $"0.07 0.9",       // право верх
                                        },
                                        new CuiOutlineComponent{
                                            Distance = "0.997 -0.997",
                                            Color = "255 255 255 0.4",
                                            UseGraphicAlpha = false
                                        }
                                    }
                                }
                            },
                    
                            // блок с картинкой
                            {
                                new CuiElement
                                {
                                    Parent = $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.row{i}.imgWrap{i}",
                                    Name = $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.row{i}.imgWrap{i}.img{i}",
                                    Components =
                                    {
                                        new CuiRawImageComponent {
                                            FadeIn = 1f,
                                            Png = (string) ImageLibrary.Call("GetImage",  offered_item_shortname)
                                        },
                                        new CuiRectTransformComponent {
                                            AnchorMin = $"0 0",       // лево  низ
                                            AnchorMax = $"1 1",       // право верх
                                        }
                                    }
                                }
                            },
                            //никнейм
                            {
                                new CuiLabel
                                {
                                   Text = {
                                        Text = "Хусейн Абдул Хама",
                                        FontSize = 13,
                                        Align = TextAnchor.MiddleLeft,
                                    },
                                    RectTransform = {
                                        AnchorMin = "0.1 0",      // лево  низ
                                        AnchorMax = "0.49 1",       // право верх
                                    }
                                },
                                $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.row{i}",
                                $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.row{i}.player"
                            },

                            //кол-во товара
                            {
                                new CuiLabel
                                {
                                   Text = {
                                        Text = offered_Item_amount,
                                        FontSize = 13,
                                        Align = TextAnchor.MiddleCenter,
                                    },
                                    RectTransform = {
                                        AnchorMin = "0.50 0",      // лево  низ
                                        AnchorMax = "0.61 1",       // право верх
                                    }
                                },
                                $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.row{i}",
                                $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.row{i}.player"
                            },

                            //цена товара
                            {
                                new CuiLabel
                                {
                                   Text = {
                                        Text = offered_Item_price_perone,
                                        FontSize = 13,
                                        Align = TextAnchor.MiddleCenter,
                                    },
                                    RectTransform = {
                                        AnchorMin = "0.67 0",      // лево  низ
                                        AnchorMax = "0.73 1",       // право верх
                                    }
                                },
                                $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.row{i}",
                                $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.row{i}.player"
                            },

                            // Блок кнопки "купить"           
                            {
                                new CuiElement
                                {
                                    Parent = $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.row{i}",
                                    Name = $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.row{i}.buyBTN",
                                    Components = {
                                        new CuiImageComponent {
                                            Color = BlueLightColor,
                                            Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                                            Material = "assets/content/ui/uibackgroundblur.mat",
                                        },
                                        new CuiRectTransformComponent {
                                            AnchorMin = "0.9 0.2",      // лево  низ
                                            AnchorMax = "0.99 0.8"      // право верх
                                        },
                                        new CuiOutlineComponent{
                                            Distance = "1 -1",
                                            Color = "255 255 255 0.4",
                                            UseGraphicAlpha = false
                                        }
                                    }
                                }
                            },
                            //  Кнопка купить
                            {
                                new CuiButton
                                {
                                    Text = {
                                        Text = "Купить",
                                        FontSize = 11,
                                        Align = TextAnchor.MiddleCenter,
                                    },
                                    Button = {
                                        Command  = $"baraholkaui.buysingleorder {player.UserIDString} {offered_item_shortname} " +
                                                                              $"{offered_Item_amount} {offered_Item_price_perone} {order_id}",
                                        Color    = "0 0 0 0",
                                    },
                                    RectTransform = {
                                        AnchorMin = "0 0",
                                        AnchorMax = "1 1"
                                    },
                                },

                                $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.row{i}.buyBTN",
                                $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.row{i}.buyBTN.title"
                            },

                        });

                        i = i + 1;
                    }
                }
                else
                {
                    CuiHelper.AddUi(player, new CuiElementContainer {
                        {
                            new CuiPanel
                            {
                                Image = { Color = "1 1 0 0" },
                                RectTransform = {
                                        AnchorMin = "0 0",      //лево низ
                                        AnchorMax = "1 0.85",       // право верх
                                    },
                                CursorEnabled = true
                            },

                           $"{Layer}.BaraholkaUI.rightPanel",
                           $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders"
                        },

                        
                        {
                            new CuiLabel
                                {
                                   Text = {
                                        Text = "Нихуя не найдено, идите нахуй",
                                        FontSize = 22,
                                        Align = TextAnchor.MiddleCenter,
                                        FadeIn = 0.5f
                                    },
                                    RectTransform = {
                                        AnchorMin = "0 0",      // лево  низ
                                        AnchorMax = "1 1",       // право верх
                                    }
                                },
                                $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders",
                                $"{Layer}.BaraholkaUI.rightPanel.tableWithOrders.title"
                            },
                    });

                    PrintWarning("[selectPrice] Error! No element in `items` table founded!");
                }
            });
        }

        [ConsoleCommand("baraholkaui.drawfilter")]
        private void drawFilter(ConsoleSystem.Arg args)
        {
            BasePlayer player = FindBasePlayer(args.GetString(0));
            string filterType = args.GetString(1);

            int page = args.GetInt(2);
            string prevOrNext = args.GetString(3);

            int start = page * 30;
            int end = start + 30;

            List<BaseItems> filteredItems = BaseItemsDictionary[filterType];

            if (player == null)
                return;

            // удаление обвертки для всех маленьких блоков
            CuiHelper.DestroyUi(player, $"{Layer}.BaraholkaUI.leftPanel.filter_block_wrap.filter_block_wrap-tmp");

            CuiHelper.DestroyUi(player, $"{Layer}.BaraholkaUI.leftPanel.filter_block_wrap.nextBtn");
            CuiHelper.DestroyUi(player, $"{Layer}.BaraholkaUI.leftPanel.filter_block_wrap.prevBtn");

            // создание новой обвертки для всех маленьких блоков
            CuiHelper.AddUi(player, new CuiElementContainer {
                {
                    new CuiPanel
                    {
                        Image = { Color = "0 0 0 0" },
                        RectTransform = {
                                AnchorMin = "0 0.094",
                                AnchorMax = "1 1",       // право верх
                            },
                        CursorEnabled = true
                    },

                   $"{Layer}.BaraholkaUI.leftPanel.filter_block_wrap",
                   $"{Layer}.BaraholkaUI.leftPanel.filter_block_wrap.filter_block_wrap-tmp"
                }
            });

            int counterX = 0;
            int counterY = 0;

            ActiveUsers user = findUserInActiveUsersList(player.UserIDString);

            for (int i = start; i < end; i++)
            {
                if (i >= filteredItems.Count)
                    break;

                CuiHelper.AddUi(player, new CuiElementContainer {
                    {
                    // обвертка маленького блока
                        new CuiElement
                        {
                            Parent = $"{Layer}.BaraholkaUI.leftPanel.filter_block_wrap.filter_block_wrap-tmp",
                            Name = $"{Layer}.BaraholkaUI.leftPanel.filter_block_wrap.filter_block_wrap-tmp.filter_block_{counterX}-{counterY}-wrap",
                            Components = {
                                new CuiImageComponent {
                                    Color = ((user != null) && (user.SpecificItemShortName == filteredItems[i].ShortName) ? PinkDarkColor : GreenLightColor),
                                    Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                                    Material = "assets/content/ui/uibackgroundblur.mat",
                                },
                                new CuiRectTransformComponent {
                                    AnchorMin = $"{ 0.0060 + 0.20278 * counterX } { 0.85 - 0.17 * counterY }",       // лево  низ
                                    AnchorMax = $"{ 0.1785 + 0.20278 * counterX } { 1.00 - 0.17 * counterY }",       // право верх
                                },
                                new CuiOutlineComponent{
                                    Distance = "0.997 -0.997",
                                    Color = "255 255 255 0.4",
                                    UseGraphicAlpha = false
                                }
                            }
                        }
                    },
                    {
                        // блок с картинкой
                        new CuiElement
                        {
                            Parent = $"{Layer}.BaraholkaUI.leftPanel.filter_block_wrap.filter_block_wrap-tmp",
                            Name = $"{Layer}.BaraholkaUI.leftPanel.filter_block_wrap.filter_block_wrap-tmp.filter_block_{counterX}-{counterY}",
                            Components =
                            {
                                new CuiRawImageComponent {
                                    FadeIn = 1f,
                                    Png = (string) ImageLibrary.Call("GetImage", filteredItems[i].ShortName)
                                },
                                new CuiRectTransformComponent {
                                    AnchorMin = $"{ 0.0060 + 0.20278 * counterX } { 0.85 - 0.17 * counterY }",       // лево  низ
                                    AnchorMax = $"{ 0.1785 + 0.20278 * counterX } { 1.00 - 0.17 * counterY }",       // право верх
                                }
                            }
                        }
                    },

                    {
                        new CuiButton
                        {
                            Text = {
                                Text = "",
                                FontSize = 20,
                                Align = TextAnchor.MiddleCenter
                            },
                            Button = {
                                Command  = $"baraholkaui.filterby {player.UserIDString} {filteredItems[i].ShortName} {counterX} {counterY} ",
                                Color    = "0 0 0 0",
                            },
                            RectTransform = {
                                AnchorMin = "0 0",
                                AnchorMax = "0.997 1"
                            },
                        },

                        $"{Layer}.BaraholkaUI.leftPanel.filter_block_wrap.filter_block_wrap-tmp.filter_block_{counterX}-{counterY}",
                        $"{Layer}.BaraholkaUI.leftPanel.filter_block_wrap.filter_block_wrap-tmp.filter_block_{counterX}-{counterY}_btn"
                    },
                });

                counterX = counterX + 1;

                if (counterX == 5)
                {
                    counterY = counterY + 1;
                    counterX = 0;
                }
            }


            if (start > 0)
                CuiHelper.AddUi(player, new CuiElementContainer {
                    #region Кнопка Назад
                    // Блок кнопки "Назад"           
                    {
                        new CuiElement
                        {
                            Parent = $"{Layer}.BaraholkaUI.leftPanel.filter_block_wrap",
                            Name   = $"{Layer}.BaraholkaUI.leftPanel.filter_block_wrap.prevBtn",
                            Components = {
                                new CuiImageComponent {
                                    Color = GreenDarkColor,
                                    Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                                    Material = "assets/content/ui/uibackgroundblur.mat",
                                    FadeIn = 0.5f,
                                },
                                new CuiRectTransformComponent {
                                    AnchorMin = "0 0",      // лево  низ
                                    AnchorMax = "0.26 0.065"        // право верх
                                },
                                new CuiOutlineComponent{
                                    Distance = "0.955 -0.997",
                                    Color = "255 255 255 0.4",
                                    UseGraphicAlpha = false
                                }
                            }
                        }
                    },
                    //  Кнопка Назад
                    {
                        new CuiButton
                        {
                            Text = {
                                Text = "Назад",
                                FontSize = 11,
                                Align = TextAnchor.MiddleCenter,
                            },
                            Button = {
                                Command  = $"baraholkaui.drawfilter {player.UserIDString} {filterType} {page - 1} prev",
                                Color    = "0 0 0 0",
                            },
                            RectTransform = {
                                AnchorMin = "0 0",
                                AnchorMax = "1 1"
                            },
                        },

                        $"{Layer}.BaraholkaUI.leftPanel.filter_block_wrap.prevBtn",
                        $"{Layer}.BaraholkaUI.leftPanel.filter_block_wrap.prevBtn.title"
                    },
                    #endregion
                });


            PrintWarning((start + 30).ToString());
            PrintWarning(filteredItems.Count.ToString());

            if ((start + 30) < filteredItems.Count)
                CuiHelper.AddUi(player, new CuiElementContainer {      
                    #region Кнопка Вперед
                    // Блок кнопки "Вперед"           
                    {
                        new CuiElement
                        {
                            Parent = $"{Layer}.BaraholkaUI.leftPanel.filter_block_wrap",
                            Name   = $"{Layer}.BaraholkaUI.leftPanel.filter_block_wrap.nextBtn",
                            Components = {
                                new CuiImageComponent {
                                    Color = GreenDarkColor,
                                    Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                                    Material = "assets/content/ui/uibackgroundblur.mat",
                                    FadeIn = 0.5f,
                                },
                                new CuiRectTransformComponent {
                                    AnchorMin = "0.74 0",      // лево  низ
                                    AnchorMax = "0.994 0.065"        // право верх
                                },
                                new CuiOutlineComponent{
                                    Distance = "0.955 -0.997",
                                    Color = "255 255 255 0.4",
                                    UseGraphicAlpha = false
                                }
                            }
                        }
                    },
                    //  Кнопка Назад
                    {
                        new CuiButton
                        {
                            Text = {
                                Text = "Вперед",
                                FontSize = 11,
                                Align = TextAnchor.MiddleCenter,
                            },
                            Button = {
                                Command  = $"baraholkaui.drawfilter {player.UserIDString} {filterType} {page + 1} next",
                                Color    = "0 0 0 0",
                            },
                            RectTransform = {
                                AnchorMin = "0 0",
                                AnchorMax = "1 1"
                            },
                        },

                        $"{Layer}.BaraholkaUI.leftPanel.filter_block_wrap.nextBtn",
                        $"{Layer}.BaraholkaUI.leftPanel.filter_block_wrap.nextBtn.title"
                    },
                    #endregion
                });

        }


        #region отрисовывает блок фильтра
        private void drawFilterBlock(string userid, string filterItemShortname, int counterX, int counterY)
        {
            BasePlayer player = FindBasePlayer(userid);
            ActiveUsers user = findUserInActiveUsersList(userid);

            CuiHelper.AddUi(player, new CuiElementContainer {
                {
                // обвертка маленького блока
                    new CuiElement
                    {
                        Parent = $"{Layer}.BaraholkaUI.leftPanel.filter_block_wrap.filter_block_wrap-tmp",
                        Name = $"{Layer}.BaraholkaUI.leftPanel.filter_block_wrap.filter_block_wrap-tmp.filter_block_{counterX}-{counterY}-wrap",
                        Components = {
                            new CuiImageComponent {
                                Color = ((user != null) && (user.SpecificItemShortName == filterItemShortname) ? PinkDarkColor : GreenLightColor),
                                Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                                Material = "assets/content/ui/uibackgroundblur.mat",
                            },
                            new CuiRectTransformComponent {
                                AnchorMin = $"{ 0.0060 + 0.20278 * counterX } { 0.85 - 0.17 * counterY }",       // лево  низ
                                AnchorMax = $"{ 0.1785 + 0.20278 * counterX } { 1.00 - 0.17 * counterY }",       // право верх
                            },
                            new CuiOutlineComponent{
                                Distance = "0.997 -0.997",
                                Color = "255 255 255 0.4",
                                UseGraphicAlpha = false
                            }
                        }
                    }
                },
                {
                    // блок с картинкой
                    new CuiElement
                    {
                        Parent = $"{Layer}.BaraholkaUI.leftPanel.filter_block_wrap.filter_block_wrap-tmp",
                        Name = $"{Layer}.BaraholkaUI.leftPanel.filter_block_wrap.filter_block_wrap-tmp.filter_block_{counterX}-{counterY}",
                        Components =
                        {
                            new CuiRawImageComponent {
                                FadeIn = 1f,
                                Png = (string) ImageLibrary.Call("GetImage",filterItemShortname)
                            },
                            new CuiRectTransformComponent {
                                AnchorMin = $"{ 0.0060 + 0.20278 * counterX } { 0.85 - 0.17 * counterY }",       // лево  низ
                                AnchorMax = $"{ 0.1785 + 0.20278 * counterX } { 1.00 - 0.17 * counterY }",       // право верх
                            }
                        }
                    }
                },

                {
                    new CuiButton
                    {
                        Text = {
                            Text = "",
                            FontSize = 20,
                            Align = TextAnchor.MiddleCenter
                        },
                        Button = {
                            Command  = $"baraholkaui.filterby {player.UserIDString} {filterItemShortname} {counterX} {counterY} ",
                            Color    = "0 0 0 0",
                        },
                        RectTransform = {
                            AnchorMin = "0 0",
                            AnchorMax = "0.997 1"
                        },
                    },

                    $"{Layer}.BaraholkaUI.leftPanel.filter_block_wrap.filter_block_wrap-tmp.filter_block_{counterX}-{counterY}",
                    $"{Layer}.BaraholkaUI.leftPanel.filter_block_wrap.filter_block_wrap-tmp.filter_block_{counterX}-{counterY}_btn"
                },
            });
        }
        #endregion

        [ConsoleCommand("baraholkaui.filterby")]
        void filterBy(ConsoleSystem.Arg args)
        {
            BasePlayer player = FindBasePlayer(args.GetString(0));
            string filterItemShortname = args.GetString(1);

            int counterX = args.GetInt(2);
            int counterY = args.GetInt(3);

            ActiveUsers user = findUserInActiveUsersList(player.UserIDString);
            user.SpecificItemShortName = filterItemShortname;

            //эту хуйню я делал для красивого вида
            /*CuiHelper.DestroyUi(player, $"{Layer}.BaraholkaUI.leftPanel.filter_block_wrap.filter_block_wrap-tmp.filter_block_{counterX}-{counterY}-wrap");
            CuiHelper.DestroyUi(player, $"{Layer}.BaraholkaUI.leftPanel.filter_block_wrap.filter_block_wrap-tmp.filter_block_{counterX}-{counterY}");
            drawFilterBlock(player.UserIDString, filterItemShortname, counterX, counterY);*/

            //draworders(player, "ALL", filterItemShortname);

            //drawOrders2(player.UserIDString, 0, 1, filterItemShortname);


            double width = 0.01f * ((filterItemShortname.Length < 15) ? 15 : filterItemShortname.Length);

            #region Кнопка +предложение
            // Блок кнопки "+предложение"     
            CuiHelper.DestroyUi(player, $"{Layer}.BaraholkaUI.rightPanel.filter");
            CuiHelper.AddUi(player, new CuiElementContainer {
                {
                    new CuiElement
                    {
                        Parent = $"{Layer}.BaraholkaUI.rightPanel",
                        Name = $"{Layer}.BaraholkaUI.rightPanel.filter",
                        Components = {
                            new CuiImageComponent {
                                Color = BlueLightColor,
                                Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                                Material = "assets/content/ui/uibackgroundblur.mat",
                            },
                            new CuiRectTransformComponent {
                                AnchorMin = "0.29 0.955",      // лево  низ
                                AnchorMax = $"{0.29 + width} 0.996"        // право верх
                            },
                            new CuiOutlineComponent{
                                Distance = "1 -1",
                                Color = "255 255 255 0.4",
                                UseGraphicAlpha = false
                            }
                        }
                    }
                },
                //  кнопка фильтрации
                {
                    new CuiButton
                    {
                        Text = {
                            Text = $"{filterItemShortname}",
                            FontSize = 13,
                            Align = TextAnchor.MiddleCenter,
                        },
                        Button = {
                            Command  = $"baraholkaui.removefilteroption {player.UserIDString}",
                            Color    = "0 0 0 0",
                        },
                        RectTransform = {
                            AnchorMin = "0 0",
                            AnchorMax = "1 1"
                        },
                    },

                    $"{Layer}.BaraholkaUI.rightPanel.filter",
                    $"{Layer}.BaraholkaUI.rightPanel.filter.title"
                },
                //  кнопка удалить фильтрацию
                {
                    new CuiButton
                    {
                        Text = {
                            Text = $"х",
                            FontSize = 14,
                            Align = TextAnchor.MiddleRight,
                        },
                        Button = {
                            Command  = $"baraholkaui.removefilteroption {player.UserIDString}",
                            Color    = "0 0 0 0",
                        },
                        RectTransform = {
                            AnchorMin = "0 0",
                            AnchorMax = "1 1",
                            OffsetMax = "-10 0"
                        },
                    },

                    $"{Layer}.BaraholkaUI.rightPanel.filter",
                    $"{Layer}.BaraholkaUI.rightPanel.filter.title"
                },
            });
                #endregion


            player.SendConsoleCommand($"baraholkaui.draworderss {player.UserIDString} {0} {1}");
        }

        #endregion

        [ConsoleCommand("baraholkaui.removefilteroption")]
        private void removefilteroption(ConsoleSystem.Arg args)
        {
            BasePlayer player = FindBasePlayer(args.GetString(0));
            ActiveUsers user = findUserInActiveUsersList(player.UserIDString);

            user.SpecificItemShortName = null;

            CuiHelper.DestroyUi(player, $"{Layer}.BaraholkaUI.rightPanel.filter");
            player.SendConsoleCommand($"baraholkaui.draworderss {player.UserIDString} {0} {1}");
        }

        private void drawBalance (string userid, string money)
        {
            BasePlayer player = FindBasePlayer(userid);

            CuiHelper.DestroyUi(player, $"{Layer}.BaraholkaUI.rightPanel.myBalance");
            CuiHelper.AddUi(player, new CuiElementContainer {
                {
                    new CuiLabel
                    {
                       Text = {
                            Text = $"{formatNumberToPrice(money)} GT",
                            FontSize = 13,
                            Align = TextAnchor.MiddleRight,
                        },
                        RectTransform = {
                            AnchorMin = "0.75 0.955",      // лево  низ
                            AnchorMax = "1 0.997",       // право верх
                            //OffsetMax = "-30 0"
                        }
                    },
                    $"{Layer}.BaraholkaUI.rightPanel",
                    $"{Layer}.BaraholkaUI.rightPanel.myBalance"
                },
            });
        }

        
        [ConsoleCommand("baraholkaui.setamount_offereditem")]
        private void setamount_offereditem(ConsoleSystem.Arg args) {
            PrintWarning($"setamount_offereditem: {args.GetInt(1).ToString()}");

            BasePlayer player = FindBasePlayer(args.GetString(0));
            if (player == null) return;

            int amount = args.GetInt(1);

            BOrder playerOrder = FindBOrder(player.UserIDString);

            if (amount > 0)
            {
                playerOrder.AmountOfOfferedItem = amount;
            }
            
            calculateCreatingOrder(player.UserIDString);
        }

        [ConsoleCommand("baraholkaui.set_price_offereditem")]
        private void set_price_offereditem(ConsoleSystem.Arg args)
        {
            PrintWarning($"set_price_offereditem: {args.GetInt(1).ToString()}");

            BasePlayer player = FindBasePlayer(args.GetString(0));
            if (player == null) return;

            int price = args.GetInt(1);

            BOrder playerOrder = FindBOrder(player.UserIDString);

            if (price > 0)
            {
                playerOrder.ExpectedPrice = price;
            }

            calculateCreatingOrder(player.UserIDString);
        }

        [ConsoleCommand("baraholkaui.select_item")]
        private void select_item(ConsoleSystem.Arg args)
        {
            BasePlayer player = FindBasePlayer(args.GetString(0));
            if (player == null) return;

            int id = args.GetInt(1);

            BOrder bOrder = FindBOrder(player.UserIDString);


            string selectedItemShortname    = bOrder.inventory[id].ShortName;
            int selectedItemAmount          = bOrder.inventory[id].Amount;
            ulong selectedItemSkinId        = bOrder.inventory[id].SkinID;
            float selectedItemCondition     = bOrder.inventory[id].Condition;
            int selectedItemBluePrint       = bOrder.inventory[id].Blueprint;

            Weapon selectedItemWeapon             = bOrder.inventory[id].Weapon;
            List<ItemContent> selectedItemContent = bOrder.inventory[id].Content;


            Item selectedItem = BuildItem(selectedItemShortname, selectedItemAmount, selectedItemSkinId,
                                                  selectedItemCondition, selectedItemBluePrint,
                                                  selectedItemWeapon, selectedItemContent);

            
            int selectedItemAmounTotal = 0;
            int counter = 0;
            foreach (var item in player.inventory.containerMain.itemList)
            {
                CuiHelper.DestroyUi(player, $"{LayerModal}.create_order-wrap.main_left.smallblock_{counter}.border");

                if (isItemsEquals(item, selectedItem))
                {
                    selectedItemAmounTotal += item.amount;
                    CuiHelper.AddUi(player, new CuiElementContainer {
                        {
                            new CuiElement
                            {
                                Parent = $"{LayerModal}.create_order-wrap.main_left.smallblock_{counter}",
                                Name   = $"{LayerModal}.create_order-wrap.main_left.smallblock_{counter}.border",
                                Components = {
                                    new CuiImageComponent {
                                        Color = "0 0 0 0.6",
                                    },
                                    new CuiRectTransformComponent {
                                        AnchorMin = $"0 0",       // лево  низ
                                        AnchorMax = $"1 1"       // право верх
                                    },
                                    new CuiOutlineComponent {
                                        Distance = "1 -1",
                                        Color = "0 0 0 0",
                                        UseGraphicAlpha = false
                                    }
                                }
                            }
                        },
                    });
                }

                counter++;
            }


            CuiHelper.DestroyUi(player, $"{LayerModal}.create_order-wrap.main_left.choosenItem.smallImg");
            CuiHelper.DestroyUi(player, $"{LayerModal}.create_order-wrap.main_left.choosenItem.amount");

            CuiHelper.AddUi(player, new CuiElementContainer {
                {
                    // блок с картинкой
                    new CuiElement
                    {
                        Parent = $"{LayerModal}.create_order-wrap.main_left.choosenItem",
                        Name   = $"{LayerModal}.create_order-wrap.main_left.choosenItem.smallImg",
                        Components =
                        {
                            new CuiRawImageComponent {
                                Png = (string) ImageLibrary.Call("GetImage", selectedItemShortname)
                            },
                            new CuiRectTransformComponent {
                                AnchorMin = $"0 0",       // лево  низ
                                AnchorMax = $"1 1",       // право верх
                            }
                        }
                    }
                },

                // кол-во
                {
                    new CuiLabel
                    {
                        Text = {
                            Text = $"{formatNumberToPrice(((selectedItemAmounTotal <= ITEM_AMOUNT_MAXIMUM) ? selectedItemAmounTotal : ITEM_AMOUNT_MAXIMUM).ToString())}",
                            FontSize = 12,
                            Align = TextAnchor.LowerRight,
                        },
                        RectTransform = {
                            AnchorMin = "0 0",      // лево  низ
                            AnchorMax = "0.95 1",       // право верх
                            //OffsetMax = "-30 0"
                        }
                    },
                    $"{LayerModal}.create_order-wrap.main_left.choosenItem",
                    $"{LayerModal}.create_order-wrap.main_left.choosenItem.amount"
                },
            });

            bOrder.inventory[id].maxOfferedAmount = selectedItemAmounTotal;  // сохраняем максимально доступное к продаже число предметов
            bOrder.AmountOfOfferedItem = (selectedItemAmounTotal <= ITEM_AMOUNT_MAXIMUM) ? selectedItemAmounTotal : ITEM_AMOUNT_MAXIMUM;             // Сохраняем по дефолту макс доступное число...
            bOrder.OfferedItem = bOrder.inventory[id];                       // сохраняем предмет, который хотим продать

            calculateCreatingOrder(player.UserIDString);
        }

        private bool isItemsEquals(Item item1, Item item2)
        {

            PrintWarning($"maxConditionNormalized: {item1.maxConditionNormalized}");
            PrintWarning($"conditionNormalized: {item1.conditionNormalized}");
            PrintWarning($"maxCondition: {item1.maxCondition}");

            PrintWarning($"item1.condition  {item1.condition }");
            PrintWarning($"item2.condition  {item2.condition }");

            if (item1.info.shortname != item2.info.shortname)
                return false;

            if (item1.condition != item2.condition)
            {
                return false;
            }
                

            if (item1.blueprintTarget != item2.blueprintTarget)
            {
                return false;
            }
                

            if ((item1.info.category == ItemCategory.Weapon) && (item2.info.category == ItemCategory.Weapon))
            {
                BaseProjectile weapon1 = item1.GetHeldEntity() as BaseProjectile;
                BaseProjectile weapon2 = item1.GetHeldEntity() as BaseProjectile;

                if ((weapon1 != null) && (weapon2 != null))
                {
                    if (weapon1.primaryMagazine.ammoType.shortname != weapon2.primaryMagazine.ammoType.shortname)
                        return false;

                    if (weapon1.primaryMagazine.contents != weapon2.primaryMagazine.contents)
                        return false;
                }
            }

            if ((item1.contents != null) && (item2.contents != null))
            {
                if (item1.contents.itemList.Count != item2.contents.itemList.Count)
                    return false;




                for (int i = 0; i < item1.contents.itemList.Count; i++)
                {
                    var cont1 = item1.contents.itemList[i];

                    bool flag = false;

                    for (int j = 0; j < item2.contents.itemList.Count; j++)
                    {
                        var cont2 = item2.contents.itemList[j];

                        if ((cont1.amount == cont2.amount) && (cont1.condition == cont2.condition) && (cont1.info.shortname == cont2.info.shortname))
                            flag = true;
                    }
                    return flag;
                }
            }

            return true;
        }

        [ConsoleCommand("baraholkaui.close_creating_order")]
        private void close_creating_order(ConsoleSystem.Arg args)
        {
            BasePlayer player = FindBasePlayer(args.GetString(0));
            if (player == null) return;

            CuiHelper.DestroyUi(player, $"{LayerModal}.create_order-wrap");
            //CuiHelper.DestroyUi(player, $"create_order-black");

            StopCreatingOrder(player);
        }

        [ConsoleCommand("baraholkaui.create_order")]
        private void create_order(ConsoleSystem.Arg args)
        {
            BasePlayer player = FindBasePlayer(args.GetString(0));
            if (player == null) return;

            StartCreatingOrder(player);

            BOrder playerOrder = FindBOrder(player.UserIDString);

            double offset4 = 0.15;

            CuiHelper.AddUi(player, new CuiElementContainer {
                {
                    new CuiPanel
                    {
                        Image = { Color = "0 0 0 0" },
                        RectTransform = {
                            AnchorMin = "0 0",    //  лево  низ
                            AnchorMax = "1 1"        //  право верх
                        },
                        CursorEnabled = true
                    },
                    new CuiElement().Parent,
                    $"{LayerModal}.create_order-wrap"
                },

                {
                    new CuiButton
                    {
                        Text = {
                            Text = "",
                            FontSize = 20,
                            Align = TextAnchor.MiddleCenter
                        },
                        Button = {
                            Command = $"baraholkaui.close_creating_order {player.UserIDString}",
                            Color = "0 0 0 0.95"
                        },
                        RectTransform = {
                            AnchorMin = "-100 -100",
                            AnchorMax = "100 100"
                        }
                    },
                    $"{LayerModal}.create_order-wrap"
                },

                {
                    new CuiElement
                    {
                        Parent = $"{LayerModal}.create_order-wrap",
                        Name   = $"{LayerModal}.create_order-wrap.main_left",
                        Components = {
                            new CuiImageComponent {
                                Color = GreenDarkColor,
                                Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                                 Material = "assets/content/ui/uibackgroundblur.mat",
                            },
                            new CuiRectTransformComponent {
                                AnchorMin = "0.1 0.2",       // лево  низ
                                AnchorMax = "0.5 0.85"       // право верх
                            },
                            new CuiOutlineComponent {
                                Distance = "1 -1",
                                Color = "255 255 255 0.4",
                                UseGraphicAlpha = false
                            }
                        }
                    }
                },

                // заголовок "Выберите товар"
                {
                new CuiLabel
                {
                    Text = {
                        Text = $"Выберите товар",
                        FontSize = 19,
                        Align = TextAnchor.MiddleLeft,
                    },
                    RectTransform = {
                        AnchorMin = "0.085 0.86",      // лево  низ
                        AnchorMax = "1 0.997",       // право верх
                        //OffsetMax = "-30 0"
                    }
                },
                $"{LayerModal}.create_order-wrap.main_left",
                $"{LayerModal}.create_order-wrap.main_left.label"
                },


                 // стрелки вверх вниз
                {
                    new CuiElement
                    {
                        Parent = $"{LayerModal}.create_order-wrap.main_left",
                        Name   = $"{LayerModal}.create_order-wrap.main_left.smallblock_arrows",
                        Components = {
                            new CuiImageComponent {
                                Png = (string)ImageLibrary.Call("GetImage", "arrows"),
                                Color = "255 255 255 0.7"
                            },
                            new CuiRectTransformComponent {
                                AnchorMin = $"0.46 0.19",       // лево  низ
                                AnchorMax = $"0.54 0.27"       // право верх
                            }
                        }
                    }
                },

                // квадрат с выбранным товаром
                {
                    new CuiElement
                    {
                        Parent = $"{LayerModal}.create_order-wrap.main_left",
                        Name   = $"{LayerModal}.create_order-wrap.main_left.choosenItem",
                        Components = {
                            new CuiImageComponent {
                                Color = GreenLightColor2,
                                Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                                Material = "assets/content/ui/uibackgroundblur.mat",
                            },
                            new CuiRectTransformComponent {
                                AnchorMin = $"{0.087 + 0.352} {0.04}",       // лево  низ
                                AnchorMax = $"{0.207 + 0.352} {0.165}"       // право верх
                            },
                            new CuiOutlineComponent {
                                Distance = "1 -1",
                                Color = "255 255 255 0.4",
                                UseGraphicAlpha = false
                            }
                        }
                    }
                },

                // основное поле [правое]
                {
                    new CuiElement
                    {
                        Parent = $"{LayerModal}.create_order-wrap",
                        Name   = $"{LayerModal}.create_order-wrap.main_right",
                        Components = {
                            new CuiImageComponent {
                                Color = GreenDarkColor,
                                Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                                Material = "assets/content/ui/uibackgroundblur.mat",
                            },
                            new CuiRectTransformComponent {
                                AnchorMin = "0.55 0.45",       // лево  низ
                                AnchorMax = "0.9 0.85"       // право верх
                            },
                            new CuiOutlineComponent {
                                Distance = "1 -1",
                                Color = "255 255 255 0.4",
                                UseGraphicAlpha = false
                            }
                        }
                    }
                },

                // заголовок "Детали предложения"
                {
                    new CuiLabel
                    {
                        Text = {
                            Text = $"Детали предложения",
                            FontSize = 19,
                            Align = TextAnchor.MiddleLeft,
                        },
                        RectTransform = {
                            AnchorMin = "0.085 0.77",      // лево  низ
                            AnchorMax = "1 0.997",       // право верх
                            //OffsetMax = "-30 0"
                        }
                    },
                    $"{LayerModal}.create_order-wrap.main_right",
                    $"{LayerModal}.create_order-wrap.main_right.label"
                },

                #region Стоимость (за один предмет)
                {
                    new CuiLabel
                    {
                        Text = {
                            Text = $"Стоимость (за один предмет)",
                            FontSize = 12,
                            Align = TextAnchor.MiddleLeft,
                        },
                        RectTransform = {
                            AnchorMin = "0.085 0.65",      // лево  низ
                            AnchorMax = "0.7 0.8",       // право верх
                            //OffsetMax = "-30 0"
                        }
                    },
                    $"{LayerModal}.create_order-wrap.main_right",
                    $"{LayerModal}.create_order-wrap.main_right.price_per_one"
                },

                
                {
                    new CuiElement
                    {
                        Parent = $"{LayerModal}.create_order-wrap.main_right",
                        Name   = $"{LayerModal}.create_order-wrap.main_right.price_per_one-block",

                        Components =
                        {
                            new CuiImageComponent { Color = GreenDarkColor,
                                Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                                Material = "assets/content/ui/uibackgroundblur.mat", },
                            new CuiRectTransformComponent {
                                AnchorMin = "0.5 0.68",      // лево  низ
                                AnchorMax = "0.915 0.77",          // право верх
                            },
                            new CuiOutlineComponent {
                                Distance = "1 -1",
                                Color = "255 255 255 0.3",
                                UseGraphicAlpha = false
                            }
                        }
                    }
                },
                
                {
                    new CuiElement
                    {
                        Parent = $"{LayerModal}.create_order-wrap.main_right.price_per_one-block",
                        Name   = $"{LayerModal}.create_order-wrap.main_right.price_per_one-block.label",
                        Components =
                        {
                            new CuiInputFieldComponent { FontSize = 14, Align = TextAnchor.MiddleCenter, 
                                                         Command = $"baraholkaui.set_price_offereditem {player.UserIDString}", 
                                                         Text = "12345678"},
                            new CuiRectTransformComponent {
                                AnchorMin = "0.01 0",
                                AnchorMax = "0.99 1"
                            }
                        }
                    }
                },
                #endregion

                #region кол-во предметов
                {
                    new CuiLabel
                    {
                        Text = {
                            Text = $"Кол-во предметов",
                            FontSize = 12,
                            Align = TextAnchor.MiddleLeft,
                        },
                        RectTransform = {
                            AnchorMin = $"0.085 {0.65 - offset4}",      // лево  низ
                            AnchorMax = $"0.7 {0.8 - offset4}",       // право верх
                        }
                    },
                    $"{LayerModal}.create_order-wrap.main_right",
                    $"{LayerModal}.create_order-wrap.main_right.timeofTitle"
                },


                {
                    new CuiElement
                    {
                        Parent = $"{LayerModal}.create_order-wrap.main_right",
                        Name   = $"{LayerModal}.create_order-wrap.main_right.amount",

                        Components =
                        {
                            new CuiImageComponent { Color = GreenDarkColor,
                                Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                                Material = "assets/content/ui/uibackgroundblur.mat", },
                            new CuiRectTransformComponent {
                                AnchorMin = $"0.5 {0.68 - offset4}",      // лево  низ
                                AnchorMax = $"0.915 {0.77 - offset4}",          // право верх
                            },
                            new CuiOutlineComponent {
                                Distance = "1 -1",
                                Color = "255 255 255 0.3",
                                UseGraphicAlpha = false
                            }
                        }
                    }
                },

                {
                    new CuiElement
                    {
                        Parent = $"{LayerModal}.create_order-wrap.main_right.amount",
                        Name   = $"{LayerModal}.create_order-wrap.main_right.input",
                        Components =
                        {
                            new CuiInputFieldComponent { FontSize = 14, Align = TextAnchor.MiddleCenter, 
                                                         Command = $"baraholkaui.setamount_offereditem {player.UserIDString}", 
                                                         Text = "12345678"},
                            new CuiRectTransformComponent {
                                AnchorMin = "0.01 0",
                                AnchorMax = "0.99 1"
                            }
                        }
                    }
                },

               
                #endregion

                #region Комиссия
                {
                    new CuiLabel
                    {
                        Text = {
                            Text = $"Комиссия",
                            FontSize = 12,
                            Align = TextAnchor.MiddleLeft,
                        },
                        RectTransform = {
                            AnchorMin = $"0.085 {0.65 - offset4*2}",      // лево  низ
                            AnchorMax = $"0.7 {0.8 - offset4*2}",       // право верх
                        }
                    },
                    $"{LayerModal}.create_order-wrap.main_right",
                    $"{LayerModal}.create_order-wrap.main_right.commission-label"
                },


                {
                    new CuiElement
                    {
                        Parent = $"{LayerModal}.create_order-wrap.main_right",
                        Name   = $"{LayerModal}.create_order-wrap.main_right.commission-element",

                        Components =
                        {
                            new CuiImageComponent { Color = GreenDarkColor,
                                Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                                Material = "assets/content/ui/uibackgroundblur.mat", },
                            new CuiRectTransformComponent {
                                AnchorMin = $"0.5 {0.68 - offset4*2}",      // лево  низ
                                AnchorMax = $"0.915 {0.77 - offset4*2}",          // право верх
                            },
                            new CuiOutlineComponent {
                                Distance = "1 -1",
                                Color = "255 255 255 0.3",
                                UseGraphicAlpha = false
                            }
                        }
                    }
                },

                {
                    new CuiLabel
                    {
                        Text = {
                            Text = $"Выберите предмет...",
                            FontSize = 12,
                            Align = TextAnchor.MiddleCenter,
                        },
                        RectTransform = {
                            AnchorMin = "0.01 0",
                                AnchorMax = "0.99 1"
                        }
                    },
                    $"{LayerModal}.create_order-wrap.main_right.commission-element",
                    $"{LayerModal}.create_order-wrap.main_right.commission-element.input"
                },
                #endregion

                //кнопка подтвердить
                {
                    new CuiElement
                    {
                        Parent = $"{LayerModal}.create_order-wrap.main_right",
                        Name   = $"{LayerModal}.create_order-wrap.main_right.btn-wrap",

                        Components =
                        {
                            new CuiImageComponent { Color = GreenLightColor,
                            },
                            new CuiRectTransformComponent {
                                AnchorMin = $"0.085 0.1",      // лево  низ
                                AnchorMax = $"0.915 0.25",          // право верх
                            },
                            new CuiOutlineComponent {
                                Distance = "1 -1",
                                Color = "255 255 255 0.5",
                                UseGraphicAlpha = false
                            }
                        }
                    }
                },
                {
                    new CuiButton
                    {
                        Text = {
                            Text = "Подтвердить",
                            FontSize = 17,
                            Align = TextAnchor.MiddleCenter
                        },
                        Button = {
                            Command = $"baraholkaui.close",
                            Color = "0 0 0 0.85"
                        },
                        RectTransform = {
                            AnchorMin = "0 0 ",
                            AnchorMax = "1 1"
                        }
                    },
                    $"{LayerModal}.create_order-wrap.main_right.btn-wrap",
                    $"{LayerModal}.create_order-wrap.main_right.btn-wrap.btn"
                }
            });

            int row = 0;
            int cols = 1;

            for (int i = 0; i < 24; i++)
            {
                double width = 0.12;
                double offset = 0.02;

                double height = 0.125;
                double offset2 = 0.015;

                // отрисовка пустых блоков...
                if (i >= player.inventory.containerMain.itemList.Count)
                {
                    PrintWarning($"i : {i}");
                    CuiHelper.AddUi(player, new CuiElementContainer
                    {
                        {
                            new CuiElement
                            {
                                Parent = $"{LayerModal}.create_order-wrap.main_left",
                                Name   = $"{LayerModal}.create_order-wrap.main_left.smallblock_{i}",
                                Components = {
                                    new CuiImageComponent {
                                        Color = GreenLightColor,
                                        Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                                        Material = "assets/content/ui/uibackgroundblur.mat",
                                    },
                                    new CuiRectTransformComponent {
                                        AnchorMin = $"{0.087 + (width + offset) * (cols - 1)} {0.85 - height - (height + offset2) * row}",       // лево  низ
                                        AnchorMax = $"{0.087 + width + (width + offset) * (cols - 1)} {0.85 - (height + offset2) * row}"       // право верх
                                    },
                                    new CuiOutlineComponent {
                                        Distance = "1 -1",
                                        Color = "255 255 255 0.4",
                                        UseGraphicAlpha = false
                                    }
                                }
                            }
                        },
                    });

                    if (cols % 6 == 0)
                    {
                        row += 1;
                        cols = 1;
                    }
                    else
                        cols += 1;
                    continue;
                }
                var item = player.inventory.containerMain.itemList[i];

                BItem bItem = new BItem
                {
                    Amount = item.amount,
                    SkinID = item.skin,
                    Blueprint = item.blueprintTarget,
                    ShortName = item.info.shortname,
                    Condition = item.condition,
                    Weapon = null,
                    Content = null
                };

                if (item.info.category == ItemCategory.Weapon)
                {
                    BaseProjectile weapon = item.GetHeldEntity() as BaseProjectile;
                    if (weapon != null)
                    {
                        bItem.Weapon = new Weapon();
                        bItem.Weapon.ammoType = weapon.primaryMagazine.ammoType.shortname;
                        bItem.Weapon.ammoAmount = weapon.primaryMagazine.contents;
                    }
                }

                if (item.contents != null)
                {
                    bItem.Content = new List<ItemContent>();
                    foreach (var cont in item.contents.itemList)
                    {
                        bItem.Content.Add(new ItemContent()
                        {
                            Amount = cont.amount,
                            Condition = cont.condition,
                            ShortName = cont.info.shortname
                        });
                    }
                }

                playerOrder.inventory.Add(bItem);

                CuiHelper.AddUi(player, new CuiElementContainer
                {
                    {
                        new CuiElement
                        {
                            Parent = $"{LayerModal}.create_order-wrap.main_left",
                            Name   = $"{LayerModal}.create_order-wrap.main_left.smallblock_{i}",
                            Components = {
                                new CuiImageComponent {
                                    Color = GreenLightColor,
                                    Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                                    Material = "assets/content/ui/uibackgroundblur.mat",
                                },
                                new CuiRectTransformComponent {
                                    AnchorMin = $"{0.087 + (width + offset) * (cols - 1)} {0.85 - height - (height + offset2) * row}",       // лево  низ
                                    AnchorMax = $"{0.087 + width + (width + offset) * (cols - 1)} {0.85 - (height + offset2) * row}"       // право верх
                                },
                                new CuiOutlineComponent {
                                    Distance = "1 -1",
                                    Color = "255 255 255 0.4",
                                    UseGraphicAlpha = false
                                }
                            }
                        }
                    },

                    {
                        // блок с картинкой
                        new CuiElement
                        {
                            Parent = $"{LayerModal}.create_order-wrap.main_left.smallblock_{i}",
                            Name   = $"{LayerModal}.create_order-wrap.main_left.smallblock_{i}.img",
                            Components =
                            {
                                new CuiRawImageComponent {
                                    FadeIn = 1f,
                                    Png = (string) ImageLibrary.Call("GetImage", item.info.shortname)
                                },
                                new CuiRectTransformComponent {
                                    AnchorMin = $"0 0",       // лево  низ
                                    AnchorMax = $"1 1",       // право верх
                                }
                            }
                        }
                    },

                    // кол-во
                    {
                        new CuiLabel
                        {
                            Text = {
                                Text = $"{((item.amount > 1) ? formatNumberToPrice(item.amount.ToString()) : "")}",
                                FontSize = 12,
                                Align = TextAnchor.LowerRight,
                            },
                            RectTransform = {
                                AnchorMin = "0 0",      // лево  низ
                                AnchorMax = "0.95 1",       // право верх
                                //OffsetMax = "-30 0"
                            }
                        },
                        $"{LayerModal}.create_order-wrap.main_left.smallblock_{i}",
                        $"{LayerModal}.create_order-wrap.main_left.smallblock_{i}.amount"
                    },

                });
               

                if (item.info.category == ItemCategory.Weapon)
                {
                    BaseProjectile weapon = item.GetHeldEntity() as BaseProjectile;
                    if (weapon != null)
                    {
                        if (item.contents != null)
                        {
                            int k = 0;

                            foreach (var cont in item.contents.itemList)
                            {
                                CuiHelper.AddUi(player, new CuiElementContainer
                                {
                                    {
                                        // блок с картинкой
                                        new CuiElement
                                        {
                                            Parent = $"{LayerModal}.create_order-wrap.main_left.smallblock_{i}",
                                            Name   = $"{LayerModal}.create_order-wrap.main_left.smallblock_{i}.content_{k}",
                                            Components =
                                            {
                                                new CuiRawImageComponent {
                                                    FadeIn = 1f,
                                                    Png = (string) ImageLibrary.Call("GetImage", cont.info.shortname)
                                                },
                                                new CuiRectTransformComponent {
                                                    AnchorMin = $"{0 + 0.19 * k} 0.77",       // лево  низ
                                                    AnchorMax = $"{0.18 + 0.19 * k} 0.97",       // право верх
                                                }
                                            }
                                        }
                                    }
                                });

                                k++;
                            }

                            if (weapon.primaryMagazine.contents > 0)
                            {
                                CuiHelper.AddUi(player, new CuiElementContainer
                                {
                                    {
                                        new CuiLabel
                                        {
                                            Text = {
                                                Text = $"{weapon.primaryMagazine.contents}",
                                                FontSize = 12,
                                                Align = TextAnchor.LowerRight,
                                            },
                                            RectTransform = {
                                                AnchorMin = "0 0",      // лево  низ
                                                AnchorMax = "0.7 1",       // право верх
                                                //OffsetMax = "-30 0"
                                            }
                                        },
                                        $"{LayerModal}.create_order-wrap.main_left.smallblock_{i}",
                                        $"{LayerModal}.create_order-wrap.main_left.smallblock_{i}.ammoAmount"
                                    },

                                    {
                                        // блок с картинкой
                                        new CuiElement
                                        {
                                            Parent = $"{LayerModal}.create_order-wrap.main_left.smallblock_{i}",
                                            Name   = $"{LayerModal}.create_order-wrap.main_left.smallblock_{i}.content_{k}",
                                            Components =
                                            {
                                                new CuiRawImageComponent {
                                                    FadeIn = 1f,
                                                    Png = (string) ImageLibrary.Call("GetImage", weapon.primaryMagazine.ammoType.shortname)
                                                },
                                                new CuiRectTransformComponent {
                                                    AnchorMin = "0.75 0.02",      // лево  низ
                                                    AnchorMax = "0.96 0.22",       // право верх
                                                }
                                            }
                                        }
                                    }
                                });
                            }
                            
                        }
                    }
                }

                CuiHelper.AddUi(player, new CuiElementContainer
                {
                    {
                        new CuiButton
                        {
                            Text = {
                                Text = "",
                                FontSize = 20,
                                Align = TextAnchor.MiddleCenter
                            },
                            Button = {
                                Command = $"baraholkaui.select_item {player.UserIDString} {i}",
                                Color = "0 0 0 0"
                            },
                            RectTransform = {
                                AnchorMin = "0 0",
                                AnchorMax = "1 1"
                            }
                        },
                         $"{LayerModal}.create_order-wrap.main_left.smallblock_{i}",
                         $"{LayerModal}.create_order-wrap.main_left.smallblock_{i}.link"
                    },
                });

                if (cols % 6 == 0)
                {
                    row += 1;
                    cols = 1;
                }
                else
                    cols += 1;
            }

            calculateCreatingOrder(player.UserIDString);
        }


        private void drawCommission (BasePlayer player, string value)
        {

            Int64 num;
            bool isNum = Int64.TryParse(value, out num);
            if (isNum)
                value = $"{formatNumberToPrice(value)} GT";


            CuiHelper.DestroyUi(player, $"{LayerModal}.create_order-wrap.main_right.commission-element.input");
            CuiHelper.AddUi(player, new CuiElementContainer {
                {
                    new CuiLabel
                    {
                        Text = {
                            Text = value,
                            FontSize = 12,
                            Align = TextAnchor.MiddleCenter,
                        },
                        RectTransform = {
                            AnchorMin = "0.01 0",
                                AnchorMax = "0.99 1"
                        }
                    },
                    $"{LayerModal}.create_order-wrap.main_right.commission-element",
                    $"{LayerModal}.create_order-wrap.main_right.commission-element.input"
                },
            });
        }

        private void calculateCreatingOrder (string userId)
        {
            BasePlayer player = FindBasePlayer(userId);
            BOrder playerOrder = FindBOrder(userId);

            if (playerOrder.OfferedItem == null)
            {
                PrintWarning("Выберите предмет...");
                drawCommission(player, "Выберите предмет...");
                return;
            }

            if (playerOrder.AmountOfOfferedItem <= 0)
            {
                PrintWarning("Введите кол-во предметов...");
                drawCommission(player, "Введите кол-во предметов...");
                return;
            }

            if (playerOrder.ExpectedPrice <= 0)
            {
                PrintWarning("Введите стоимость предмета...");
                drawCommission(player, "Введите стоимость предмета...");
                return;
            }

            int basePrice = findBasePrice(playerOrder.OfferedItem.ShortName);

            PrintWarning($"basePrice: {basePrice}");

            if (basePrice <= 0)
            {
                drawCommission(player, $"Этот предмет НЕЛЬЗЯ продавать!");
                return;
            }



            Int64 totalPriceRequestedItems = playerOrder.AmountOfOfferedItem * basePrice;
            Int64 totalPriceOfferedItems   = playerOrder.AmountOfOfferedItem * playerOrder.ExpectedPrice;

            Puts("");
            
            playerOrder.Commission = getCommission(totalPriceRequestedItems, totalPriceOfferedItems);
            playerOrder.TotalPrice = playerOrder.ExpectedPrice - playerOrder.Commission;

            PrintWarning($"totalPriceRequestedItems: {playerOrder.AmountOfOfferedItem} * {basePrice} = {formatNumberToPrice(totalPriceRequestedItems.ToString())}");
            PrintWarning($"totalPriceOfferedItems: {playerOrder.AmountOfOfferedItem} * {playerOrder.ExpectedPrice} = {formatNumberToPrice(totalPriceOfferedItems.ToString())}");
            PrintWarning($"Commission: {playerOrder.Commission}");
          
            drawCommission(player, $"{playerOrder.Commission}"); 
        }

        #region Интерфейс UI
        private CuiElementContainer OpenBaraholka(BasePlayer player)
        {
            PrintWarning("OpenBaraholka");

            //player.SendConsoleCommand($"baraholkaui.draworders {player.UserIDString} ALL"); //человек первый раз открыл барахолку - отрисовываем предметы инвентаря

            var MainContainer = new CuiElementContainer
            {

                // Layer
                {
                    new CuiPanel
                    {
                        Image = { Color = "1 1 1 0" },
                        RectTransform = { AnchorMin = "0.06 0.15", AnchorMax = "0.94 0.931" },
                        CursorEnabled = true
                    },
                    new CuiElement().Parent,
                    Layer
                },
                {
                    new CuiButton
                    {
                        Text = {
                            Text = "",
                            FontSize = 20,
                            Align = TextAnchor.MiddleCenter
                        },
                        Button = {
                            Command = $"baraholkaui.close",
                            Color = "0 0 0 0.85"
                        },
                        RectTransform = {
                            AnchorMin = "-100 -100",
                            AnchorMax = "100 100"
                        }
                    },
                    Layer
                },

                #region Первая панель
                // Первая панель
                {
                    new CuiPanel
                    {
                        Image = { Color = "0 0 0 0" },
                        RectTransform = { AnchorMin = "0 0", AnchorMax = "0.25 1" }
                    },
                    Layer,
                    $"{Layer}.BaraholkaUI.leftPanel"
                },


                #region Верхние три кнопки

                #region Кнопка купить
                // Блок кнопки "купить"           
                {
                    new CuiElement
                    {
                        Parent = $"{Layer}.BaraholkaUI.leftPanel",
                        Name = $"{Layer}.BaraholkaUI.leftPanel.buyButton",
                        Components = {
                            new CuiImageComponent {
                                Color = GreenDarkColor,
                                Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                                Material = "assets/content/ui/uibackgroundblur.mat",
                            },
                            new CuiRectTransformComponent {
                                AnchorMin = "0.005 0.955",      // лево  низ
                                AnchorMax = "0.26 0.996"        // право верх
                            },
                            new CuiOutlineComponent{
                                Distance = "1 -1",
                                Color = "255 255 255 0.4",
                                UseGraphicAlpha = false
                            }
                        }
                    }
                },
                //  Кнопка купить
                {
                    new CuiButton
                    {
                        Text = {
                            Text = "Купить",
                            FontSize = 11,
                            Align = TextAnchor.MiddleCenter,
                        },
                        Button = {
                            Command  = $"BaraholkaUI.TESTFUCKINGBTN",
                            Color    = "0 0 0 0",
                        },
                        RectTransform = {
                            AnchorMin = "0 0",
                            AnchorMax = "1 1"
                        },
                    },

                    $"{Layer}.BaraholkaUI.leftPanel.buyButton",
                    $"{Layer}.BaraholkaUI.leftPanel.buyButton.title"
                },
                #endregion

                #region Кнопка Обменять
                // Блок кнопки "Обменять"           
                {
                    new CuiElement
                    {
                        Parent = $"{Layer}.BaraholkaUI.leftPanel",
                        Name = $"{Layer}.BaraholkaUI.leftPanel.ChangeBTN",
                        Components = {
                            new CuiImageComponent {
                                Color = GreenDarkColor,
                                Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                                 Material = "assets/content/ui/uibackgroundblur.mat",
                            },
                            new CuiRectTransformComponent {
                                AnchorMin = "0.28 0.955",
                                AnchorMax = "0.63 0.996"
                            },
                            new CuiOutlineComponent{
                                Distance = "1 -1",
                                Color = "255 255 255 0.4",
                                UseGraphicAlpha = false
                            }
                        }
                    }
                },
                //  Кнопка Обменять
                {
                    new CuiButton
                    {
                        Text = {
                            Text = "Обменять",
                            FontSize = 11,
                            Align = TextAnchor.MiddleCenter,
                        },
                        Button = {
                            Command  = $"BaraholkaUI.TESTFUCKINGBTN",
                            Color    = "0 0 0 0",
                        },
                        RectTransform = {
                            AnchorMin = "0 0",
                            AnchorMax = "1 1"
                        },
                    },

                    $"{Layer}.BaraholkaUI.leftPanel.ChangeBTN",
                    $"{Layer}.BaraholkaUI.leftPanel.ChangeBTN.title"
                },
                #endregion

                #region Кнопка Мои предложения
                // Блок кнопки "Мои предложения"           
                {
                    new CuiElement
                    {
                        Parent = $"{Layer}.BaraholkaUI.leftPanel",
                        Name = $"{Layer}.BaraholkaUI.leftPanel.myOffer",
                        Components = {
                            new CuiImageComponent {
                                Color = GreenDarkColor,
                                Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                                 Material = "assets/content/ui/uibackgroundblur.mat",
                            },
                            new CuiRectTransformComponent {
                                AnchorMin = "0.65 0.955",       // лево  низ
                                AnchorMax = "0.996 0.996"       // право верх
                            },
                            new CuiOutlineComponent {
                                Distance = "1 -1",
                                Color = "255 255 255 0.4",
                                UseGraphicAlpha = false
                            }
                        }
                    }
                },
                //  Кнопка Мои предложения
                {
                    new CuiButton
                    {
                        Text = {
                            Text = "Мои предл.",
                            FontSize = 11,
                            Align = TextAnchor.MiddleCenter,
                        },
                        Button = {
                            Command  = $"baraholkaui.create_order {player.UserIDString}",
                            Color    = "0 0 0 0",
                        },
                        RectTransform = {
                            AnchorMin = "0 0",
                            AnchorMax = "1 1"
                        },
                    },

                    $"{Layer}.BaraholkaUI.leftPanel.myOffer",
                    $"{Layer}.BaraholkaUI.leftPanel.myOffer.title"
                },
                #endregion

                #endregion

                #region Поиск
                {
                    new CuiElement
                    {
                        Parent = $"{Layer}.BaraholkaUI.leftPanel",
                        Name = $"{Layer}.BaraholkaUI.leftPanel.searchForm",

                        Components =
                        {
                            new CuiImageComponent { Color = "255 255 255 0.85" },
                            new CuiRectTransformComponent {
                                AnchorMin = "0 0.894",
                                AnchorMax = "1 0.935"
                            }
                        }
                    }
                },

                {
                    new CuiElement
                    {
                        Parent = $"{Layer}.BaraholkaUI.leftPanel.searchForm",
                        Name = $"{Layer}.BaraholkaUI.leftPanel.searchForm.input",
                        Components =
                        {
                            new CuiInputFieldComponent { FontSize = 16, Align = TextAnchor.MiddleLeft, Command = "topfederust.put ", Text = "12345678"},
                            new CuiRectTransformComponent {
                                AnchorMin = "0.01 0",
                                AnchorMax = "0.99 1"
                            }
                        }
                    }
                },
                #endregion

                #region кнопка "ресурсы"
                // кнопка "ресурсы"
                {
                    new CuiPanel
                    {
                        Image = {
                            Color = GreenLightColor,
                            Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                            Material = "assets/content/ui/uibackgroundblur.mat",
                        },
                        RectTransform = {
                            AnchorMin = "0 0.833",      // лево  низ
                            AnchorMax = "0.997 0.874"       // право верх
                        }
                    },
                    $"{Layer}.BaraholkaUI.leftPanel",
                    $"{Layer}.BaraholkaUI.leftPanel.filterBTN_resouces"
                },

                //  Кнопка Ресурсы
                {
                    new CuiButton
                    {
                        Text = {
                            Text = "Ресурсы",
                            FontSize = 11,
                            Align = TextAnchor.MiddleLeft,
                        },
                        Button = {
                            Command  =  $"baraholkaui.drawfilter {player.UserIDString} Resources 0 standing",
                            Color    = "0 0 0 0",
                        },
                        RectTransform = {
                            AnchorMin = "0 0",
                            AnchorMax = "1 1",
                            OffsetMin = "10 0"
                        },
                    },

                    $"{Layer}.BaraholkaUI.leftPanel.filterBTN_resouces",
                    $"{Layer}.BaraholkaUI.leftPanel.filterBTN_resouces.title"
                },
                #endregion

                #region кнопка "Компоненты"
                // кнопка "ресурсы"
                {
                    new CuiPanel
                    {
                        Image = {
                            Color = GreenLightColor,
                            Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                            Material = "assets/content/ui/uibackgroundblur.mat",
                        },
                        RectTransform = {
                            AnchorMin = "0 0.787",      // лево  низ
                            AnchorMax = "0.997 0.828"       // право верх
                        }
                    },
                    $"{Layer}.BaraholkaUI.leftPanel",
                    $"{Layer}.BaraholkaUI.leftPanel.filterBTN_components"
                },

                //  Кнопка Компоненты
                {
                    new CuiButton
                    {
                        Text = {
                            Text = "Компоненты",
                            FontSize = 11,
                            Align = TextAnchor.MiddleLeft,
                        },
                        Button = {
                            Command  =  $"baraholkaui.drawfilter {player.UserIDString} Components 0 standing",
                            Color    = "0 0 0 0",
                        },
                        RectTransform = {
                            AnchorMin = "0 0",
                            AnchorMax = "1 1",
                            OffsetMin = "10 0"
                        },
                    },

                    $"{Layer}.BaraholkaUI.leftPanel.filterBTN_components",
                    $"{Layer}.BaraholkaUI.leftPanel.filterBTN_components.title"
                },
                #endregion

                #region кнопка "Оружие"
                {
                    new CuiPanel
                    {
                        Image = {
                            Color = GreenLightColor,
                            Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                            Material = "assets/content/ui/uibackgroundblur.mat",
                        },
                        RectTransform = {
                            AnchorMin = "0 0.741",      // лево  низ
                            AnchorMax = "0.997 0.782"   // право верх
                        }
                    },
                    $"{Layer}.BaraholkaUI.leftPanel",
                    $"{Layer}.BaraholkaUI.leftPanel.filterBTN_weapons"
                },
                {
                    new CuiButton
                    {
                        Text = {
                            Text = "Оружие",
                            FontSize = 11,
                            Align = TextAnchor.MiddleLeft,
                        },
                        Button = {
                            Command  = $"baraholkaui.drawfilter {player.UserIDString} Weapons 0 standing",
                            Color    = "0 0 0 0",
                        },
                        RectTransform = {
                            AnchorMin = "0 0",
                            AnchorMax = "1 1",
                            OffsetMin = "10 0"
                        },
                    },

                    $"{Layer}.BaraholkaUI.leftPanel.filterBTN_weapons",
                    $"{Layer}.BaraholkaUI.leftPanel.filterBTN_weapons.title"
                },
                #endregion

                #region кнопка "Разное"
                // кнопка "ресурсы"
                {
                    new CuiPanel
                    {
                        Image = {
                            Color = GreenLightColor,
                            Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                            Material = "assets/content/ui/uibackgroundblur.mat",
                        },
                        RectTransform = {
                            AnchorMin = "0 0.694",      // лево  низ
                            AnchorMax = "0.997 0.735"       // право верх
                        }
                    },
                    $"{Layer}.BaraholkaUI.leftPanel",
                    $"{Layer}.BaraholkaUI.leftPanel.filterBTN_other"
                },

                //  Кнопка Разное
                {
                    new CuiButton
                    {
                        Text = {
                            Text = "Разное",
                            FontSize = 11,
                            Align = TextAnchor.MiddleLeft,
                        },
                        Button = {
                            Command  =  $"baraholkaui.drawfilter {player.UserIDString} Other 0 standing",
                            Color    = "0 0 0 0",
                        },
                        RectTransform = {
                            AnchorMin = "0 0",
                            AnchorMax = "0.997 1",
                            OffsetMin = "10 0"
                        },
                    },

                    $"{Layer}.BaraholkaUI.leftPanel.filterBTN_other",
                    $"{Layer}.BaraholkaUI.leftPanel.filterBTN_other.title"
                },
                #endregion




                #region квадратные блоки фильтра

                // Основной слой
                {
                    new CuiPanel
                    {
                        Image = { Color = "255 255 255 0" },
                        RectTransform = {
                            AnchorMin = "0 0",              // лево  низ
                            AnchorMax = "0.997 0.675"       // право верх
                        },
                        CursorEnabled = true
                    },

                    $"{Layer}.BaraholkaUI.leftPanel",
                    $"{Layer}.BaraholkaUI.leftPanel.filter_block_wrap"
                },         

                #endregion


                #endregion



                #region Полоска
                // Полоса
                {
                    new CuiPanel
                    {
                        Image = { Color = "255 255 255 0.7" },
                        RectTransform = {
                            AnchorMin = "0.258 0",
                            AnchorMax = "0.25801 1"
                        }
                    },
                    Layer,
                    $"{Layer}.BaraholkaUI.horizontalLine"
                },
                #endregion

                #region Вторая панель
                // Вторая панель контейнер
                {
                    new CuiPanel
                    {
                        Image = { Color = "0 0 0 0" },
                        RectTransform = {
                            AnchorMin = "0.2666 0",
                            AnchorMax = "1 1"
                        }
                    },
                    Layer,
                    $"{Layer}.BaraholkaUI.rightPanel"
                },
                #endregion


                #region Кнопка +предложение
                // Блок кнопки "+предложение"           
                {
                    new CuiElement
                    {
                        Parent = $"{Layer}.BaraholkaUI.rightPanel",
                        Name = $"{Layer}.BaraholkaUI.rightPanel.makeAnOffer",
                        Components = {
                            new CuiImageComponent {
                                Color = PinkDarkColor,
                                Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                                Material = "assets/content/ui/uibackgroundblur.mat",
                            },
                            new CuiRectTransformComponent {
                                AnchorMin = "0 0.955",      // лево  низ
                                AnchorMax = "0.26 0.996"        // право верх
                            },
                            new CuiOutlineComponent{
                                Distance = "1 -1",
                                Color = "255 255 255 0.4",
                                UseGraphicAlpha = false
                            }
                        }
                    }
                },
                //  Кнопка + предложение
                {
                    new CuiButton
                    {
                        Text = {
                            Text = "+ Предложение",
                            FontSize = 13,
                            Align = TextAnchor.MiddleCenter,
                        },
                        Button = {
                            Command  = $"baraholkaui.create_order {player.UserIDString}",
                            Color    = "0 0 0 0",
                        },
                        RectTransform = {
                            AnchorMin = "0 0",
                            AnchorMax = "1 1"
                        },
                    },

                    $"{Layer}.BaraholkaUI.rightPanel.makeAnOffer",
                    $"{Layer}.BaraholkaUI.rightPanel.makeAnOffer.title"
                },
                #endregion
                
                #region таблица с товарами
                // таблица с товарами
                {
                    new CuiPanel
                    {
                        Image = { Color = GreenDarkColor },
                        RectTransform = {
                            AnchorMin = "0 0.874",      // лево  низ
                            AnchorMax = "1 0.935"       // право верх
                        }
                    },
                    $"{Layer}.BaraholkaUI.rightPanel",
                    $"{Layer}.BaraholkaUI.rightPanel.table_header"
                },


                // заголовок "предложение"
                {
                    new CuiButton
                    {
                        Text = {
                            Text = "Предложение",
                            FontSize = 12,
                            Align = TextAnchor.MiddleLeft,
                        },
                        Button = {
                            Command  =  $"baraholkaui.draworderss {player.UserIDString} {0} {1} {"offered_item_shortname"}",
                            Color    = "0 0 0 0",
                        },
                        RectTransform = {
                            AnchorMin = "0 0",
                            AnchorMax = "0.3 1",
                            OffsetMin = "20 0"
                        },
                    },

                    $"{Layer}.BaraholkaUI.rightPanel.table_header",
                    $"{Layer}.BaraholkaUI.rightPanel.table_header.first_header"
                },

                // заголовок "Кол-во"
                {
                    new CuiButton
                    {
                        Text = {
                            Text = "Кол-во",
                            FontSize = 12,
                            Align = TextAnchor.MiddleCenter,
                           
                        },
                        Button = {
                            Command  =  $"baraholkaui.draworderss {player.UserIDString} {0} {1} {"`orders`.`offered_Item_amount` ASC"}",
                            Color    = "0 0 0 0",
                        },
                        RectTransform = {
                            AnchorMin = "0.50 0",
                            AnchorMax = "0.6 1",
                        },
                    },

                    $"{Layer}.BaraholkaUI.rightPanel.table_header",
                    $"{Layer}.BaraholkaUI.rightPanel.table_header.second_header"
                },

                // заголовок "Цена"
                {
                    new CuiButton
                    {
                        Text = {
                            Text = "Цена",
                            FontSize = 12,
                            Align = TextAnchor.MiddleCenter,
                        },
                        Button = {
                            Command  =  $"baraholkaui.draworderss {player.UserIDString} {0} {1} {"offered_Item_price_perone"}",
                            Color    = "0 0 0 0",
                        },
                        RectTransform = {
                            AnchorMin = "0.65 0",
                            AnchorMax = "0.75 1",
                        },
                    },

                    $"{Layer}.BaraholkaUI.rightPanel.table_header",
                    $"{Layer}.BaraholkaUI.rightPanel.table_header.first_header"
                },
                // заголовок "Действие"
                {
                    new CuiButton
                    {
                        Text = {
                            Text = "Действие",
                            FontSize = 12,
                            Align = TextAnchor.MiddleCenter,
                        },
                        Button = {
                            Command  =  $"baraholkaui.drawfilter {player.UserIDString} Resources 0 standing",
                            Color    = "0 0 0 0",
                        },
                        RectTransform = {
                            AnchorMin = "0.85 0",
                            AnchorMax = "1 1",
                        },
                    },

                    $"{Layer}.BaraholkaUI.rightPanel.table_header",
                    $"{Layer}.BaraholkaUI.rightPanel.table_header.first_header"
                }
                #endregion

            };

            return MainContainer;
        }

        #endregion


        private int findBasePrice (string shortname)
        {
            /*foreach (var filteredItems1 in BaseItemsDictionary)
                return filteredItems1.Value.Where(item => item.ShortName.Equals(shortname)).FirstOrDefault().BasePrice;
*/
            foreach (var filteredItems in BaseItemsDictionary)
                foreach(var item in filteredItems.Value)
                    if (shortname == item.ShortName) return item.BasePrice;

            return 0;
        }

        public static int baseGTCoin = 10;

        #region Классы
        private Dictionary<string, List<BaseItems>> BaseItemsDictionary = new Dictionary<string, List<BaseItems>>
            {
                { "Weapons",
                    new List<BaseItems>() {
                        new BaseItems {
                            ShortName = "bow.hunting",
                            BasePrice = 0 * baseGTCoin,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "crossbow",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "surveycharge",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "mace",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "machete",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "longsword",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "salvaged.sword",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "guntrap",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "pistol.nailgun",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "bow.compound",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "shotgun.waterpipe",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems
                        {
                            ShortName = "pistol.eoka",
                            BasePrice = 0,
                            image = "",
                        },
                        new BaseItems
                        {
                            ShortName = "pistol.revolver",
                            BasePrice = 0,
                            image = "",
                        },
                        new BaseItems {
                            ShortName = "pistol.python",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "pistol.semiauto",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "rifle.ak",
                            BasePrice = 20225,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "rifle.bolt",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "rifle.lr300",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "rifle.semiauto",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "shotgun.double",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "shotgun.pump",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "smg.2",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "smg.mp5",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "smg.thompson",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "pistol.m92",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "grenade.f1",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "explosive.satchel",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "explosive.timed",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "flamethrower",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "trap.landmine",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "rocket.launcher",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "rifle.l96",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "rifle.m39",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "multiplegrenadelauncher",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "lmg.m249",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "weapon.mod.flashlight",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "weapon.mod.holosight",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "weapon.mod.lasersight",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "weapon.mod.muzzleboost",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "weapon.mod.muzzlebrake",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "weapon.mod.silencer",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "weapon.mod.small.scope",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "weapon.mod.simplesight",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "weapon.mod.8x.scope",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "ammo.handmade.shell",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "ammo.nailgun.nails",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "arrow.bone",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "arrow.fire",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "arrow.wooden",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "ammo.pistol",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "ammo.pistol.fire",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "ammo.pistol.hv",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "ammo.rifle",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "ammo.rifle.explosive",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "ammo.rifle.hv",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "ammo.rifle.incendiary",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "ammo.rocket.basic",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "ammo.rocket.fire",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "ammo.rocket.hv",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "ammo.rocket.smoke",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "ammo.shotgun",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "ammo.shotgun.slug",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "arrow.hv",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "ammo.grenadelauncher.buckshot",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "ammo.grenadelauncher.he",
                            BasePrice = 0,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "ammo.grenadelauncher.smoke",
                            BasePrice = 0,
                            image = ""
                        }
                    }
                },
                { "Resources",
                    new List<BaseItems>() {
                        new BaseItems
                        {
                            ShortName = "wood",
                            BasePrice = 10,
                            image = "",
                        },
                        new BaseItems
                        {
                            ShortName = "stones",
                            BasePrice = 17662,
                            image = ""
                        },
                        new BaseItems
                        {
                            ShortName = "bone.fragments",
                            BasePrice = 22700,
                            image = "",
                        },
                        new BaseItems
                        {
                            ShortName = "charcoal",
                            BasePrice = 17662,
                            image = ""
                        },
                        new BaseItems
                        {
                            ShortName = "cloth",
                            BasePrice = 22700,
                            image = "",
                        },
                        new BaseItems
                        {
                            ShortName = "crude.oil",
                            BasePrice = 17662,
                            image = ""
                        },
                        new BaseItems
                        {
                            ShortName = "leather",
                            BasePrice = 22700,
                            image = "",
                        },
                        new BaseItems
                        {
                            ShortName = "lowgradefuel",
                            BasePrice = 17662,
                            image = ""
                        },
                        new BaseItems
                        {
                            ShortName = "metal.fragments",
                            BasePrice = 22700,
                            image = "",
                        },
                        new BaseItems
                        {
                            ShortName = "metal.ore",
                            BasePrice = 17662,
                            image = ""
                        },
                        new BaseItems
                        {
                            ShortName = "metal.refined",
                            BasePrice = 22700,
                            image = "",
                        },
                        new BaseItems
                        {
                            ShortName = "gunpowder",
                            BasePrice = 17662,
                            image = ""
                        },
                        new BaseItems
                        {
                            ShortName = "explosives",
                            BasePrice = 22700,
                            image = "",
                        },
                        new BaseItems
                        {
                            ShortName = "fat.animal",
                            BasePrice = 17662,
                            image = ""
                        },
                        new BaseItems
                        {
                            ShortName = "hq.metal.ore",
                            BasePrice = 22700,
                            image = "",
                        },
                        new BaseItems
                        {
                            ShortName = "sulfur.ore",
                            BasePrice = 17662,
                            image = ""
                        },
                        new BaseItems
                        {
                            ShortName = "sulfur",
                            BasePrice = 22700,
                            image = "",
                        },
                        new BaseItems
                        {
                            ShortName = "diesel_barrel",
                            BasePrice = 17662,
                            image = ""
                        },
                    }
                },
                { "Components",
                    new List<BaseItems>() {
                        new BaseItems {
                            ShortName = "propanetank",
                            BasePrice = 17662,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "gears",
                            BasePrice = 17662,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "metalblade",
                            BasePrice = 17662,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "metalpipe",
                            BasePrice = 17662,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "metalspring",
                            BasePrice = 17662,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "riflebody",
                            BasePrice = 17662,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "roadsigns",
                            BasePrice = 17662,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "rope",
                            BasePrice = 17662,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "semibody",
                            BasePrice = 17662,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "sewingkit",
                            BasePrice = 17662,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "sheetmetal",
                            BasePrice = 17662,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "smgbody",
                            BasePrice = 17662,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "techparts",
                            BasePrice = 17662,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "tarp",
                            BasePrice = 17662,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "targeting.computer",
                            BasePrice = 17662,
                            image = ""
                        },
                        new BaseItems {
                            ShortName = "tool.camera",
                            BasePrice = 17662,
                            image = ""
                        },
                    }
                },
                { "Other",
                    new List<BaseItems>() {
                        new BaseItems {
                            ShortName = "barricade.concrete",
                            BasePrice = 22700,
                            image = "",
                        },
                        new BaseItems {
                            ShortName = "barricade.metal",
                            BasePrice = 22700,
                            image = "",
                        },
                        new BaseItems {
                            ShortName = "barricade.sandbags",
                            BasePrice = 22700,
                            image = "",
                        },
                        new BaseItems {
                            ShortName = "barricade.stone",
                            BasePrice = 22700,
                            image = "",
                        },
                        new BaseItems {
                            ShortName = "barricade.wood",
                            BasePrice = 22700,
                            image = "",
                        },
                        new BaseItems {
                            ShortName = "barricade.woodwire",
                            BasePrice = 22700,
                            image = "",
                        },
                        new BaseItems {
                            ShortName = "battery.small",
                            BasePrice = 22700,
                            image = "",
                        },
                        new BaseItems {
                            ShortName = "bucket.water",
                            BasePrice = 22700,
                            image = "",
                        },
                        new BaseItems {
                            ShortName = "pookie.bear",
                            BasePrice = 22700,
                            image = "",
                        },
                        new BaseItems {
                            ShortName = "stash.small",
                            BasePrice = 22700,
                            image = "",
                        },
                        new BaseItems {
                            ShortName = "waterjug",
                            BasePrice = 22700,
                            image = "",
                        },
                        new BaseItems {
                            ShortName = "research.table",
                            BasePrice = 22700,
                            image = "",
                        },
                        new BaseItems {
                            ShortName = "door.double.hinged.metal",
                            BasePrice = 22700,
                            image = "",
                        },
                        new BaseItems {
                            ShortName = "door.double.hinged.toptier",
                            BasePrice = 22700,
                            image = "",
                        },
                        new BaseItems {
                            ShortName = "door.double.hinged.wood",
                            BasePrice = 22700,
                            image = "",
                        },
                        new BaseItems {
                            ShortName = "door.hinged.metal",
                            BasePrice = 22700,
                            image = "",
                        },
                        new BaseItems {
                            ShortName = "door.hinged.toptier",
                            BasePrice = 22700,
                            image = "",
                        },
                        new BaseItems {
                            ShortName = "door.hinged.wood",
                            BasePrice = 22700,
                            image = "",
                        },
                        new BaseItems {
                            ShortName = "floor.grill",
                            BasePrice = 22700,
                            image = "",
                        },
                        new BaseItems {
                            ShortName = "floor.ladder.hatch",
                            BasePrice = 22700,
                            image = "",
                        },
                        new BaseItems {
                            ShortName = "ladder.wooden.wall",
                            BasePrice = 22700,
                            image = "",
                        },
                        new BaseItems {
                            ShortName = "wall.frame.garagedoor",
                            BasePrice = 22700,
                            image = "",
                        },
                    }
                }
            };

            public class BaseItems
            {
                public string ShortName = "";
                public int BasePrice = 0;
                public string image = "";
            }
            #endregion

        #endregion

    }
}








/*DROP PROCEDURE IF EXISTS buy_single_order;

DELIMITER $
CREATE PROCEDURE buy_single_order(id INT(11))
BEGIN
    IF EXISTS(SELECT* FROM orders WHERE order_id = id) THEN
         DELETE FROM orders WHERE order_id = id;
SELECT 'TRUE' AS RESULT;
ELSE
   SELECT 'FALSE' AS RESULT;
END IF;
END $
DELIMITER ;

CALL buy_single_order(22);
DROP PROCEDURE IF EXISTS buy_single_order;*/


/*
BEGIN
IF EXISTS(SELECT* FROM orders WHERE order_id = id) THEN
        SELECT*, 'TRUE' as result FROM `orders` WHERE `order_id` = id;
        DELETE FROM orders WHERE order_id = id;
    ELSE
       SELECT 'FALSE' AS RESULT;
END IF;
END*/
