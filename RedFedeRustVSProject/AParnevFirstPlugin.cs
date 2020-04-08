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
    [Info("AParnevFirstPlugin", "AParnev", "0.0.1")]
    internal class AParnevFirstPlugin : RustPlugin
    {
        private string Layer = "AParnevFirstPlugin";

        [PluginReference] private Plugin ImageLibrary;

        private void OnServerInitialized()
        {
            ImageLibrary.Call("AddImage", "https://imgur.com/Mjoc40x", "https://imgur.com/Mjoc40x");
        }


        [ConsoleCommand("aparnevtestclose")]
        private void test(ConsoleSystem.Arg args)
        {
            BasePlayer player = FindBasePlayer(args.GetString(0));
            CuiHelper.DestroyUi(player, Layer);
        }

        [ChatCommand("openWindow")]
        private void openWindowPlease(BasePlayer player, string command, string[] args)
        {
            if (player == null) { return; }

            if (player.IsAdmin)
            {
                CuiHelper.DestroyUi(player, Layer);
                CuiHelper.AddUi(player, openWindow(player));
            }
            else
                SendReply(player, "У вас нет прав для выполнения этой команды");
        }


        [ConsoleCommand("showimg")]
        private void showimg(ConsoleSystem.Arg args)
        {
            BasePlayer player = FindBasePlayer(args.GetString(0));
            int img = args.GetInt(1);

            CuiHelper.DestroyUi(player, $"{Layer}.img");

            List<string> imgages = new List<string>();

            imgages.Add("https://i.imgur.com/Mjoc40x.png");
            imgages.Add("https://i.imgur.com/NnGsZjd.png");
            imgages.Add("https://i.imgur.com/sqUggvg.png");
            imgages.Add("https://i.imgur.com/R48LFRC.png");
            imgages.Add("https://i.imgur.com/sqUggvg.png");
            imgages.Add("https://i.imgur.com/R48LFRC.png");

            CuiHelper.AddUi(player, new CuiElementContainer
            {
                new CuiElement
                {
                    Parent = Layer,
                    Name = $"{Layer}.img",
                    Components =
                    {
                        new CuiRawImageComponent
                        {
                            //Url = imgages[img]
                            Url = (string) ImageLibrary.Call("GetImage", $"https://imgur.com/Mjoc40x")
                        },
                        new CuiRectTransformComponent { AnchorMin = "0.2 0.4", AnchorMax = "0.9 0.89" }
                    }
                },
            });
        }


        private CuiElementContainer openWindow(BasePlayer player)
        {
            var MainContainer = new CuiElementContainer
            {

                // Layer
                {
                    new CuiPanel
                    {
                        Image = { Color = "1 1 1 0" },
                        RectTransform = { 
                            AnchorMin = "0 0",     // лево  низ
                            AnchorMax = "1 1"     // право верх
                        },
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
                            FontSize = 13,
                            Align = TextAnchor.MiddleCenter,
                        },
                        Button = {
                            Command  = $"aparnevtestclose {player.UserIDString}",
                            Color    = "0 0 0 0",
                        },
                        RectTransform = {
                            AnchorMin = "0 0",
                            AnchorMax = "1 1"
                        },
                    },

                    $"{Layer}",
                    $"{Layer}.test"
                },


                {
                    new CuiButton
                    {
                        Text = {
                            Text = "Кнопка 1",
                            FontSize = 19,
                            Align = TextAnchor.MiddleCenter,
                        },
                        Button = {
                            Command  = $"showimg {player.UserIDString} {1}",
                            Color    = "0 0 0 1",
                        },
                        RectTransform = {
                            AnchorMin = "0 0.8",
                            AnchorMax = "0.15 0.89"
                        },
                    },

                    $"{Layer}",
                    $"{Layer}.test"
                },

                {
                    new CuiButton
                    {
                        Text = {
                            Text = "Кнопка 2",
                            FontSize = 19,
                            Align = TextAnchor.MiddleCenter,
                        },
                        Button = {
                            Command  = $"showimg {player.UserIDString} {2}",
                            Color    = "0 0 0 1",
                        },
                        RectTransform = {
                            AnchorMin = "0 0.7",
                            AnchorMax = "0.15 0.79"
                        },
                    },

                    $"{Layer}",
                    $"{Layer}.test"
                },

                {
                    new CuiButton
                    {
                        Text = {
                            Text = "Кнопка 3",
                            FontSize = 19,
                            Align = TextAnchor.MiddleCenter,
                        },
                        Button = {
                            Command  = $"showimg {player.UserIDString} {3}",
                            Color    = "0 0 0 1",
                        },
                        RectTransform = {
                            AnchorMin = "0 0.6",
                            AnchorMax = "0.15 0.69"
                        },
                    },

                    $"{Layer}",
                    $"{Layer}.test"
                },

                {
                    new CuiButton
                    {
                        Text = {
                            Text = "Кнопка 4",
                            FontSize = 19,
                            Align = TextAnchor.MiddleCenter,
                        },
                        Button = {
                            Command  = $"showimg {player.UserIDString} {4}",
                            Color    = "0 0 0 1",
                        },
                        RectTransform = {
                            AnchorMin = "0 0.5",
                            AnchorMax = "0.15 0.59"
                        },
                    },

                    $"{Layer}",
                    $"{Layer}.test"
                },

                {
                    new CuiButton
                    {
                        Text = {
                            Text = "Кнопка 5",
                            FontSize = 19,
                            Align = TextAnchor.MiddleCenter,
                        },
                        Button = {
                            Command  = $"showimg {player.UserIDString} {5}",
                            Color    = "0 0 0 1",
                        },
                        RectTransform = {
                            AnchorMin = "0 0.4",
                            AnchorMax = "0.15 0.49"
                        },
                    },

                    $"{Layer}",
                    $"{Layer}.test"
                },

            };

            return MainContainer;
        }

        #region черный ящик
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
        #endregion

    }
}