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
    public class ObjectLists : ScriptableObject
    {
        public UnityEngine.Object[] objects;

        public ObjectLists()
        {
        }
    }

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

        private void OnServerInitialized()
        {
            generateItems();
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

        void generateItems()
        {
            List<ItemDefinition> list = ItemManager.GetItemDefinitions();

            foreach (var item in list)
            {
                var newItem = new ItemDefinitionExtended(item);
                newItem.price = 0;

                Items.Add(newItem);
            }
        }

        private List<ItemDefinitionExtended> Items = new List<ItemDefinitionExtended> { };

        public class ItemDefinitionExtended : ItemDefinition
        {
            public ulong price { get; set; }

            public ItemDefinitionExtended(ItemDefinition parent)
            {
                foreach (FieldInfo prop in parent.GetType().GetFields())
                    GetType().GetField(prop.Name).SetValue(this, prop.GetValue(parent));
            }
        }






        [ConsoleCommand("gt")]
        void getItemDef(ConsoleSystem.Arg args)
        {
            foreach (var item in Items)
            {
                PrintWarning($"{item.category} : {item.shortname} : {item.price}");
            }
        }
    }
}