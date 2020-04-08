using System.Collections.Generic;
using Newtonsoft.Json;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Random = Oxide.Core.Random;

namespace Oxide.Plugins
{
    [Info("Advert Messages", "LaserHydra", "3.0.2", ResourceId = 1510)]
    [Description("Allows to set up messages which are broadcasted in a configured interval")]
    class AdvertMessages : RustPlugin
    {
        private Configuration _config;
        private int _previousAdvert = -1;

        #region Hooks

        [PluginReference("Clans")]
        private Plugin Clans;

        private void Loaded()
        {
            LoadConfig();
            
            Puts($"{Title} is showing adverts every {_config.AdvertInterval} minutes.");
            timer.Every(_config.AdvertInterval * 60, BroadcastNextAdvert);
            timer.Every(10 * 60, BroadcastClanAlert);
        }

        #endregion

        #region Helper Methods

        private void BroadcastClanAlert()
        {
            if (Clans != null)
            {
                foreach (BasePlayer player in BasePlayer.activePlayerList)
                    if (player.IsConnected)
                    {
                        string clanTag = (string)Clans.Call("HookFindClanByUser", player.UserIDString);
                        if (clanTag == "NO_CLAN_TAG")
                        {

                            SendReply(player, "<color=#00FFFF><size=16>[RedFedeRust] </color><color=#FFEFCA>Играть без клан-тэга на сервере одному - нежелательно\nЕсли играете 2+ вам обязательно нужно создать или войти в клан. Используйте <color=#5bb95b>/clan</color></size></color>");
                        }
                    }
            }
        }
        private void BroadcastNextAdvert()
        {
            if (_config.Messages.Count == 0)
                return;

            int advert = GetNextAdvertIndex();

            foreach (BasePlayer player in BasePlayer.activePlayerList)
                if (player.IsConnected)
                {
                    SendReply(player, _config.Messages[advert]);
                }
           
            if (_config.BroadcastToConsole)
                Puts(Formatter.ToPlaintext(_config.Messages[advert]));

            _previousAdvert = advert;
        }

        private int GetNextAdvertIndex()
        {
            if (!_config.ChooseMessageAtRandom)
                return (_previousAdvert + 1) % _config.Messages.Count;

            int advert;
            if (_config.Messages.Count > 1)
            {
                do advert = Random.Range(0, _config.Messages.Count);
                while (advert == _previousAdvert);
            }
            else
                advert = 0;

            return advert;
        }

        #endregion

        #region Configuration

        protected override void LoadConfig()
        {
            base.LoadConfig();
            _config = Config.ReadObject<Configuration>();
            SaveConfig();
        }

        protected override void LoadDefaultConfig()
        {
            _config = Configuration.CreateDefault();
        }

        protected override void SaveConfig() => Config.WriteObject(_config);

        private class Configuration
        {
            [JsonProperty("Messages")]
            public List<string> Messages { get; private set; }

            [JsonProperty("Advert Interval (in Minutes)")]
            public float AdvertInterval { get; private set; }  = 10;

            [JsonProperty("Broadcast to Console (true/false)")]
            public bool BroadcastToConsole { get; private set; } = true;

            [JsonProperty("Choose Message at Random (true/false)")]
            public bool ChooseMessageAtRandom { get; private set; } = false;

            public static Configuration CreateDefault()
            {
                return new Configuration
                {
                    Messages = new List<string>
                    {
                        "Welcome to our server, have fun!",
                        "Please treat everybody respectfully.",
                        "Cheating will result in a [#red]permanent[/#] ban."
                    }
                };
            }
        }

        #endregion
    }
}