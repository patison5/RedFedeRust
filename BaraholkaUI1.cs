using System;
using System.Collections.Generic;
using Oxide.Core;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Serialization;

using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using UnityEngine;
using Oxide.Core.Libraries;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace Oxide.Plugins
{
    [Info("MySQL BaraholkaUI", "Lulex.py", "0.0.1")]
    internal class BaraholkaUI : RustPlugin
    {
        private static string Layer => "BaraholkaUIMainLayer";
        private static string Base => "baraholka";

        public string MainColor { get; } = "0.2 0.5 0.39 1";

        public string GreenDarkColor  { get; } = HexToCuiColor("1f5a49");
        public string GreenLightColor { get; } = HexToCuiColor("236956");
        public string PinkDarkColor   { get; } = HexToCuiColor("4d2247");
        public string BlueLightColor  { get; } = HexToCuiColor("1f9e94");

        [PluginReference] private Plugin ImageLibrary;


        private void OnServerInitialized()
        {
            if (!ImageLibrary)
            {
                PrintError("Donwload and install ImageLibrary to work with this plugin...");
            }

            BasePlayer player = FindBasePlayer("76561198077282054");
            closeBaraholkaUI(player);
            CuiHelper.AddUi(player, OpenBaraholka(player));

            /* BasePlayer player1 = FindBasePlayer("76561198428983821");
             closeBaraholkaUI(player1);
             CuiHelper.AddUi(player1, OpenBaraholka(player1));*/

            //player.SendConsoleCommand($"baraholkaui.drawfilter {player.UserIDString} Weapons");   //11111
        }

        private void closeBaraholkaUI(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, Layer);
        }


        [ConsoleCommand("baraholkaui.close")]
        private void closeBaraholkaUI(ConsoleSystem.Arg args)
        {
            Puts("fuck");

            BasePlayer player = args.Player();
            if (player == null) return;
            CuiHelper.DestroyUi(player, Layer);
        }



        private CuiElementContainer OpenBaraholka(BasePlayer player)
        {
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
                            Command  = $"BaraholkaUI.TESTFUCKINGBTN",
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

                //  Кнопка Мои предложения
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

                //  Кнопка Мои предложения
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
                            AnchorMin = "0 0.695",      // лево  низ
                            AnchorMax = "0.997 0.736"       // право верх
                        }
                    },
                    $"{Layer}.BaraholkaUI.leftPanel",
                    $"{Layer}.BaraholkaUI.leftPanel.filterBTN_other"
                },

                //  Кнопка Мои предложения
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
                        Image = { Color = "0 0 0 0.8" },
                        RectTransform = {
                            AnchorMin = "0.2666 0", 
                            AnchorMax = "1 1" 
                        }
                    },
                    Layer,
                    $"{Layer}.BaraholkaUI.leftPanel"
                },
                #endregion
            };

            return MainContainer;
        }

        [ConsoleCommand("baraholkaui.drawfilter")]
        private void drawFilter(ConsoleSystem.Arg args)
        {
            BasePlayer player = FindBasePlayer(args.GetString(0));
            string filterType = args.GetString(1);

            int page = args.GetInt(2);
            string prevOrNext = args.GetString(3);

            int start = page * 30;
            int end   = start + 30;

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
                            Name = $"{Layer}.BaraholkaUI.leftPanel.filter_block_wrap.filter_block_wrap-tmp.filter_block_{counterX}-{counterY}",
                            Components = {
                                new CuiImageComponent {
                                    Color = GreenLightColor,
                                    Sprite = "Assets/Content/UI/UI.Background.Tile.psd",
                                    Material = "assets/content/ui/uibackgroundblur.mat",
                                },
                                new CuiRectTransformComponent {
                                    AnchorMin = $"{ 0.0030 + 0.20273 * counterX } { 0.85 - 0.17 * counterY }",       // лево  низ
                                    AnchorMax = $"{ 0.1765 + 0.20273 * counterX } { 1.00 - 0.17 * counterY }",       // право верх
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
                                    AnchorMin = $"{ 0.0030 + 0.2028 * counterX } { 0.85 - 0.17 * counterY }",       // лево  низ
                                    AnchorMax = $"{ 0.1765 + 0.2028 * counterX } { 1.00 - 0.17 * counterY }",       // право верх
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
                                Command  = $"baraholkaui.filterby {filteredItems[i].ShortName}",
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
                                    AnchorMax = "0.997 0.065"        // право верх
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

        [ConsoleCommand("baraholkaui.filterby")]
        void filterBy (ConsoleSystem.Arg args)
        {
            string filterItemShortname = args.GetString(0);

            Puts(filterItemShortname);
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

        private Dictionary<string, List<BaseItems>> BaseItemsDictionary = new Dictionary<string, List<BaseItems>>
        {
            { "Weapons", 
                new List<BaseItems>() {
                    new BaseItems
                    {
                        ShortName = "rifle.ak",
                        BasePrice = 20225,
                        image = "",
                    },
                    new BaseItems
                    {
                        ShortName = "rifle.bolt",
                        BasePrice = 17662,
                        image = "",
                    },
                    new BaseItems {
                        ShortName = "rifle.lr300",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "pistol.eoka",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "smg.thompson",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "smg.mp5",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "smg.2",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "rocket.launcher",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "longsword",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "pistol.m92",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "lmg.m249",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "rifle.semiauto",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "pistol.semiauto",
                        BasePrice = 0,
                        image = ""
                    },
                    //not unic
                    new BaseItems
                    {
                        ShortName = "rifle.ak",
                        BasePrice = 22700,
                        image = "",
                    },
                    new BaseItems
                    {
                        ShortName = "rifle.bolt",
                        BasePrice = 17662,
                        image = "",
                    },
                    new BaseItems {
                        ShortName = "rifle.lr300",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "pistol.eoka",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "rifle.semiauto",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "pistol.semiauto",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "rifle.semiauto",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "pistol.semiauto",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "smg.thompson",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "smg.mp5",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "smg.2",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "rocket.launcher",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "longsword",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems
                    {
                        ShortName = "rifle.ak",
                        BasePrice = 22700,
                        image = "",
                    },
                    new BaseItems
                    {
                        ShortName = "rifle.bolt",
                        BasePrice = 17662,
                        image = "",
                    },
                    new BaseItems {
                        ShortName = "rifle.lr300",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "pistol.eoka",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "smg.thompson",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "smg.mp5",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "smg.2",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "rocket.launcher",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems
                    {
                        ShortName = "rifle.ak",
                        BasePrice = 22700,
                        image = "",
                    },
                    new BaseItems
                    {
                        ShortName = "rifle.bolt",
                        BasePrice = 17662,
                        image = "",
                    },
                    new BaseItems {
                        ShortName = "rifle.lr300",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "pistol.eoka",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "rifle.semiauto",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "pistol.semiauto",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "rifle.semiauto",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "pistol.semiauto",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "smg.thompson",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "smg.mp5",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "smg.2",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "rocket.launcher",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "longsword",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems
                    {
                        ShortName = "rifle.ak",
                        BasePrice = 22700,
                        image = "",
                    },
                    new BaseItems
                    {
                        ShortName = "rifle.bolt",
                        BasePrice = 17662,
                        image = "",
                    },
                    new BaseItems {
                        ShortName = "rifle.lr300",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "pistol.eoka",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "smg.thompson",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "smg.mp5",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "smg.2",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "rocket.launcher",
                        BasePrice = 0,
                        image = ""
                    },new BaseItems
                    {
                        ShortName = "rifle.ak",
                        BasePrice = 22700,
                        image = "",
                    },
                    new BaseItems
                    {
                        ShortName = "rifle.bolt",
                        BasePrice = 17662,
                        image = "",
                    },
                    new BaseItems {
                        ShortName = "rifle.lr300",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "pistol.eoka",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "rifle.semiauto",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "pistol.semiauto",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "rifle.semiauto",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "pistol.semiauto",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "smg.thompson",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "smg.mp5",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "smg.2",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "rocket.launcher",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "longsword",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems
                    {
                        ShortName = "rifle.ak",
                        BasePrice = 22700,
                        image = "",
                    },
                    new BaseItems
                    {
                        ShortName = "rifle.bolt",
                        BasePrice = 17662,
                        image = "",
                    },
                    new BaseItems {
                        ShortName = "rifle.lr300",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "pistol.eoka",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "smg.thompson",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "smg.mp5",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "smg.2",
                        BasePrice = 0,
                        image = ""
                    },
                    new BaseItems {
                        ShortName = "rocket.launcher",
                        BasePrice = 0,
                        image = ""
                    }
                }
            },
            { "Resources",
                new List<BaseItems>() {
                    new BaseItems
                    {
                        ShortName = "sulfur",
                        BasePrice = 22700,
                        image = "",
                    },
                    new BaseItems
                    {
                        ShortName = "stone",
                        BasePrice = 17662,
                        image = ""
                    },
                }
            },
            { "Components",
                new List<BaseItems>() {
                    new BaseItems
                    {
                        ShortName = "stone",
                        BasePrice = 17662,
                        image = ""
                    },
                }
            },
            { "Other",
                new List<BaseItems>() {
                    new BaseItems
                    {
                        ShortName = "sulfur",
                        BasePrice = 22700,
                        image = "",
                    }
                }
            }
        };

        public class BaseItems
        {
            public string ShortName = "";
            public int BasePrice = 0;
            public string image = "";
        }
    }
}