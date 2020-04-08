﻿using System;using Oxide.Core.Configuration;using UnityEngine;namespace Oxide.Plugins{    [Info("Metabolism", "A0001", "1.0.0")]    [Description("Изменяет показатели метаболизма игрока при возрождении")]    class Metabolism : RustPlugin    {        #region Fields        const string permAllow = "metabolism.allow";				#endregion				#region Configuration        float caloriesSpawnValue = 500f;        float healthSpawnValue = 100f;        float hydrationSpawnValue = 250f;				protected override void LoadDefaultConfig()        {            PrintWarning("Создание нового файла конфигурации...");        }		private void LoadConfigValues()        {            GetConfig("Количество калорий при возрождении (0.0 - 500.0)", ref caloriesSpawnValue);			GetConfig("Количество здоровья при возрождении (0.0 - 100.0)", ref healthSpawnValue);			GetConfig("Количество жидкости при возрождении (0.0 - 250.0)", ref hydrationSpawnValue);						SaveConfig();		}	        private void GetConfig<T>(string Key, ref T var)        {            if (Config[Key] != null)            {                var = (T)Convert.ChangeType(Config[Key], typeof(T));            }            Config[Key] = var;        }						#endregion				#region Oxide Hooks				void Loaded()        {            LoadConfigValues();        }				        void Init()        {            permission.RegisterPermission(permAllow, this);        }        #endregion        #region Modify Metabolism        private void OnPlayerRespawned(BasePlayer player)        {            if (permission.UserHasPermission(player.UserIDString, permAllow))			{				player.health = healthSpawnValue;				player.metabolism.calories.value = caloriesSpawnValue;				player.metabolism.hydration.value = hydrationSpawnValue;			}		}	        #endregion    }}