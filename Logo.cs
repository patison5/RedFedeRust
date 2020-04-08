using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using UnityEngine;
using UnityEngine.UI;

namespace Oxide.Plugins
{
    [Info("Logo", "Beorn", "0.0.1")]
    class Logo : RustPlugin
    {
        void Loaded()
        {
            foreach (var pl in ListAllPlayers())
            {
                pl.SendConsoleCommand("logo.undraw");
                pl.SendConsoleCommand("logo.draw");
            }
            Puts("Logo загружена. Спасибо, что используете наш плагин");
        }

        public ListHashSet<BasePlayer> ListAllPlayers()
        {
            //var sleepingPlayers = BasePlayer.sleepingPlayerList;
            var activePlayers = BasePlayer.activePlayerList;
            //var allPlayers = activePlayers.Concat(sleepingPlayers).ToList();
            //return allPlayers;
            return activePlayers;
        }

        public class Configuration
        {
            [JsonProperty(PropertyName = "Версия конфига (не менять)")]
            public int version;

            [JsonProperty(PropertyName = "Логотип")]
            public ConfigurationLogo logo;
        }

        public class ConfigurationLogo
        {
            [JsonProperty(PropertyName = "Включен")]
            public bool allow;

            [JsonProperty(PropertyName = "Ссылка")]
            public string url;

            [JsonProperty(PropertyName = "Размер X")]
            public int x;

            [JsonProperty(PropertyName = "Размер Y")]
            public int y;

            [JsonProperty(PropertyName = "Отступ верх-право X")]
            public int offsetX;

            [JsonProperty(PropertyName = "Отступ верх-право Y")]
            public int offsetY;
        }

        private Configuration config;

        protected override void LoadDefaultConfig()
        {
            config = new Configuration
            {
                version = 1,
                logo = new ConfigurationLogo
                {
                    allow = true,
                    url = "https://i.imgur.com/c2e8gYD.png",
                    x = 180,
                    y = 65,
                    offsetX = 5,
                    offsetY = 5
                }
            };
            SaveConfig();
        }

        protected override void SaveConfig() => Config.WriteObject(config);

        private CuiElementContainer DrawLogo()
        {
            const string uiName = "LogoUILogo";

            return new CuiElementContainer
            {
                {
                    CreateImage()
                }
            };
        }

        private CuiElement CreateImage()
        {
            var element = new CuiElement();
            var image = new CuiRawImageComponent
            {
                Url = "https://i.imgur.com/pJLY6zE.png",
            };

            var rectTransform = new CuiRectTransformComponent
            {
                AnchorMin = "0 0",
                AnchorMax = "1 1"
            };
            element.Components.Add(image);
            element.Components.Add(rectTransform);
            element.Parent = "ImagePanel";

            return element;
        }

        [ConsoleCommand("logo.draw")]
        private void DDrawLogo(ConsoleSystem.Arg arg)
        {
            if (arg.Player() == null)
                return;
            BasePlayer player = arg.Player();
            Puts(player.displayName);
            CuiHelper.AddUi(player, new CuiElementContainer
            {
                {
                    new CuiPanel
                    {
                        Image = { Color = "0 0 0 0" },
                        RectTransform = { AnchorMin = "0.844 0.925", AnchorMax = "0.999 0.995" }
                    },
                    "Hud",
                    "ImagePanel"
                }
            });
            CuiHelper.AddUi(player, DrawLogo());
        }

        [ConsoleCommand("logo.undraw")]
        private void UnDrawLogo(ConsoleSystem.Arg arg)
        {
            if (arg.Player() == null)
                return;
            BasePlayer player = arg.Player();
            CuiHelper.DestroyUi(player, "ImagePanel");
        }

        private void OnPlayerInit(BasePlayer player)
        {
            player.SendConsoleCommand("logo.draw");
        }
    }

}