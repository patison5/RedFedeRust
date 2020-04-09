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
using ProtoBuf;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.Assertions;
using System.Reflection;

namespace Oxide.Plugins
{
    [Info("MySQL Baraholka", "Beorn", "0.0.1")]
    internal class BaraholkaDBInit : RustPlugin
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

        private readonly Core.MySql.Libraries.MySql _mySql = Interface.Oxide.GetLibrary<Core.MySql.Libraries.MySql>();
        private Core.Database.Connection _mySqlConnection;

        private void OnServerInitialized()
        {
        }
        private void OnPluginLoaded()
        {
            _mySqlConnection = _mySql.OpenDb(Config["host"].ToString(), Convert.ToInt32(Config["port"]), Config["database"].ToString(), Config["username"].ToString(), Config["password"].ToString(), this);
            generateItems();
            generatePrices();
            createTableWithPrices();

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


        private List<ItemDefinitionExtended> Items = new List<ItemDefinitionExtended> { };

        public static Dictionary<string, ulong> DefaultBasePrices = new Dictionary<string, ulong>()
        {
            { "diving.fins", 10 },
            { "diving.mask", 10 },
            { "diving.tank", 10 },
            { "diving.wetsuit", 10 },
            { "barrelcostume", 10 },
            { "cratecostume", 10 },
            { "halloween.mummysuit", 10 },
            { "scarecrow.suit", 10 },
            { "partyhat", 10 },
            { "bleach", 10 },
            { "glue", 10 },
            { "mining.pumpjack", 10 },
            { "mining.quarry", 10 },
            { "apple", 10 },
            { "apple.spoiled", 10 },
            { "black.raspberries", 10 },
            { "blueberries", 10 },
            { "cactusflesh", 10 },
            { "can.beans", 10 },
            { "can.tuna", 10 },
            { "chocholate", 10 },
            { "fish.cooked", 10 },
            { "fish.raw", 10 },
            { "fish.minnows", 10 },
            { "fish.troutsmall", 10 },
            { "chicken.burned", 10 },
            { "chicken.cooked", 10 },
            { "granolabar", 10 },
            { "chicken.raw", 10 },
            { "chicken.spoiled", 10 },
            { "deermeat.burned", 10 },
            { "deermeat.cooked", 10 },
            { "deermeat.raw", 10 },
            { "horsemeat.burned", 10 },
            { "horsemeat.cooked", 10 },
            { "horsemeat.raw", 10 },
            { "humanmeat.burned", 10 },
            { "humanmeat.cooked", 10 },
            { "humanmeat.raw", 10 },
            { "humanmeat.spoiled", 10 },
            { "bearmeat.burned", 10 },
            { "bearmeat.cooked", 10 },
            { "bearmeat", 10 },
            { "wolfmeat.burned", 10 },
            { "wolfmeat.cooked", 10 },
            { "wolfmeat.raw", 10 },
            { "wolfmeat.spoiled", 10 },
            { "meat.pork.burned", 10 },
            { "meat.pork.cooked", 10 },
            { "meat.boar", 10 },
            { "mushroom", 10 },
            { "jar.pickle", 10 },
            { "blueprintbase", 10 },
            { "easter.bronzeegg", 10 },
            { "smallwaterbottle", 10 },
            { "easter.goldegg", 10 },
            { "easter.paintedeggs", 10 },
            { "easter.silveregg", 10 },
            { "halloween.lootbag.large", 10 },
            { "halloween.candy", 10 },
            { "halloween.lootbag.medium", 10 },
            { "halloween.lootbag.small", 10 },
            { "spiderweb", 10 },
            { "skull.human", 10 },
            { "candycaneclub", 10 },
            { "candycane", 10 },
            { "pookie.bear", 10 },
            { "xmas.present.large", 10 },
            { "xmas.present.medium", 10 },
            { "xmas.present.small", 10 },
            { "snowball", 10 },
            { "stocking.large", 10 },
            { "stocking.small", 10 },
            { "santahat", 10 },
            { "wrappedgift", 10 },
            { "wrappingpaper", 10 },
            { "gloweyes", 10 },
            { "corn", 10 },
            { "clone.corn", 10 },
            { "seed.corn", 10 },
            { "clone.hemp", 10 },
            { "seed.hemp", 10 },
            { "potato", 10 },
            { "clone.potato", 10 },
            { "seed.potato", 10 },
            { "pumpkin", 10 },
            { "clone.pumpkin", 10 },
            { "seed.pumpkin", 10 },
            { "fat.animal", 10 },
            { "battery.small", 10 },
            { "blood", 10 },
            { "bone.fragments", 10 },
            { "charcoal", 10 },
            { "cloth", 10 },
            { "crude.oil", 10 },
            { "diesel_barrel", 10 },
            { "fertilizer", 10 },
            { "horsedung", 10 },
            { "hq.metal.ore", 10 },
            { "metal.refined", 10 },
            { "leather", 10 },
            { "metal.fragments", 10 },
            { "metal.ore", 10 },
            { "plantfiber", 10 },
            { "researchpaper", 10 },
            { "water.salt", 10 },
            { "scrap", 10 },
            { "stones", 10 },
            { "sulfur.ore", 10 },
            { "sulfur", 10 },
            { "water", 10 },
            { "skull.wolf", 10 },
            { "wood", 10 },
            { "antiradpills", 10 },
            { "tool.camera", 10 },
            { "fishingrod.handmade", 10 },
            { "keycard_blue", 10 },
            { "keycard_green", 10 },
            { "keycard_red", 10 },
            { "grenade.smoke", 10 },
            { "supply.signal", 10 },
            { "cakefiveyear", 10 },
        };

        public class ItemDefinitionExtended
        {
            public ItemDefinition itemDefinition { get; set; }
            public ulong price { get; set; } = 0;

            public ItemDefinitionExtended(ItemDefinition itemDefinition) 
            {
                this.itemDefinition = itemDefinition;
            }
        }

        void generateItems()
        {
            List<ItemDefinition> list = ItemManager.GetItemDefinitions();

            foreach (var item in list)
            {
                var newItem = new ItemDefinitionExtended(item);

                if (DefaultBasePrices.ContainsKey(item.shortname))
                {
                    newItem.price = DefaultBasePrices[item.shortname];
                }

                Items.Add(newItem);
            }

        }

        void generatePrices()
        {
            foreach (var item in Items)
            {
                item.price = generatePrice(item);
            }

        }

        ulong generatePrice(ItemDefinitionExtended item)
        {
            if (item.price != 0)
                return item.price;

            ulong price = 0;

            if (item.itemDefinition.Blueprint)
            {
                foreach (var ingredient in item.itemDefinition.Blueprint.ingredients)
                {
                    var oItem = Items.Where(o => o.itemDefinition.shortname == ingredient.itemDef.shortname).FirstOrDefault();
                    if (oItem.price == 0);
                    {
                        oItem.price = generatePrice(oItem);
                    }
                    price += (ulong)ingredient.amount * oItem.price;
                }
            }
            return price;
        }

        void createTableWithPrices()
        {
            var querryString = $"DROP TABLE IF EXISTS `items`; " +
                               $"CREATE TABLE `items` " +
                                    $"(`item_id` int(11) NOT NULL," +
                                    $" `item_category` varchar(255) NOT NULL," +
                                    $" `item_shortname` varchar(255) NOT NULL," +
                                    $" `item_price` int(11) NOT NULL ) " +
                                        $"ENGINE = InnoDB DEFAULT CHARSET = utf8;";
            _mySql.Query(Core.Database.Sql.Builder.Append(querryString), _mySqlConnection, list =>
            {
                foreach (var item in Items)
                {
                    _mySql.Insert(Core.Database.Sql.Builder.Append($"INSERT INTO items (item_category, item_shortname, item_price) VALUES('{item.itemDefinition.category}', '{item.itemDefinition.shortname}', '{item.price}')"), _mySqlConnection);
                }
            });
        }
    }
}