using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Powerless Turrets", "August", "2.1.6")]
    [Description("Allows turrets to operate without electricity.")]
    
    class PowerlessTurrets : RustPlugin
    {
        #region Initialization
        
        private RelationshipManager rm;
        private List<AutoTurret> onlineTurrets = new List<AutoTurret>();
        
        #region Config
        private Configuration _config;

        protected override void LoadDefaultConfig()
        {
            Config.WriteObject(new Configuration(), true);
        }
        private class Configuration
        {
            [JsonProperty("Can SAM Sites operate without power?")]
            public bool IsEnabled { get; set; } = true;

            [JsonProperty("Maximum distance players can turn on/off turrets")]
            public float MaxDistance { get; set; } = 5f;
        }
        private void SaveConfig()
        {
            Config.WriteObject(_config, true);
        }
        
        #endregion

        private const string Perm = "powerlessturrets.use";
        private const string PermManage = "powerlessturrets.manage";
        
        void Init()
        {
            onlineTurrets.Clear();
            
            _config = Config.ReadObject<Configuration>();
                        
            permission.RegisterPermission(PermManage, this);
            permission.RegisterPermission(Perm, this);
        }

        void OnServerInitialized()
        {
            if (_config.IsEnabled)
            {
                Subscribe(nameof(OnEntitySpawned));
                ChangePower(25);
            }

            foreach (var turret in onlineTurrets)
            {
                UpdateTurret(turret);
            }
        }
        
        protected override void LoadDefaultMessages()
        {
            // English
            lang.RegisterMessages(new Dictionary<string, string> {
                ["Enabled"] = "Sam sites now operate without power",
                ["Disabled"] = "Sam sites now require power to operate.",
                
                ["NoPermission"] = "Error: No Permission",
                ["Syntax"] = "Error: Syntax",
                
                ["InvalidEntity"] = "This is not a valid entity! (Are you too far?)"
            }, this);
        }
        #endregion
        
        #region Turret
        
        [ChatCommand("turret")]
        private void TurretCommand(BasePlayer player, string cmd, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, Perm))
            {
                player.ChatMessage(Lang("NoPermission", player.UserIDString));
                return;
            }

            RaycastHit ray;

            if (!Physics.Raycast(player.eyes.HeadRay(), out ray, _config.MaxDistance))
            {
                player.ChatMessage(Lang("InvalidEntity", player.UserIDString));
                return;
            }

            AutoTurret ent = ray.GetEntity() as AutoTurret;

            if (ent == null)
            {
                player.ChatMessage(Lang("InvalidEntity", player.UserIDString));
                return;
            }
            
            if (!ent.IsAuthed(player))
            {
                player.ChatMessage(Lang("NoPermission", player.UserIDString));
                return;
            }
            UpdateTurret(ent);
        }

        private void UpdateTurret(AutoTurret turret)
        {
            if (onlineTurrets.Contains(turret) || turret.IsOnline())
            {
                turret.SetIsOnline(false);
                onlineTurrets.Remove(turret);
            }
            else
            {
                turret.SetIsOnline(true);
                onlineTurrets.Add(turret);
            }
            turret.SendNetworkUpdateImmediate();
        }
        
        object OnTurretShutdown(AutoTurret turret)
        {
            if (onlineTurrets.Contains(turret)) onlineTurrets.Remove(turret);
            return null;
        }
        #endregion
        
        #region SAMs
        void OnEntitySpawned(SamSite sam)
        {
            sam.UpdateHasPower(25, 1);
        }
        
        [ChatCommand("sams")]
        void PowerlessTurretsCommand(BasePlayer player, string cmd, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, PermManage))
            {
                player.ChatMessage(Lang("NoPermission", player.UserIDString));
                return;
            }

            if (args.Length != 1)
            {
                player.ChatMessage(Lang("Syntax", player.UserIDString));
                return;
            }

            if (args[0].ToLower() == "toggle")
            {
                ToggleSams(player);
            }
            else
            {
                player.ChatMessage(Lang("Syntax", player.UserIDString));
            }
            
        }

        void ChangePower(int amt)
        {
            foreach (var sam in UnityEngine.Object.FindObjectsOfType<SamSite>())
            {
                sam.UpdateHasPower(amt, 1);
            }
        }
        void ToggleSams(BasePlayer player)
        {
            if (_config.IsEnabled == false)
            {
                Subscribe(nameof(OnEntitySpawned));
                
                ChangePower(25);
                
                player.ChatMessage(Lang("Enabled", player.UserIDString));
            }
            else
            {
                Unsubscribe(nameof(OnEntitySpawned));

                ChangePower(0);
                
                player.ChatMessage(Lang("Disabled", player.UserIDString));
            }
            _config.IsEnabled = !_config.IsEnabled;
            SaveConfig();
        }
        
        #endregion
        
        #region Else
        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);

        void Unload()
        {
            ChangePower(0);
            SaveConfig();
        }
        
        #endregion
    }
}