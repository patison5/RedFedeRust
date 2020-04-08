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

        public string GreenDarkColor   { get; } = HexToCuiColor("1f5a49");
        public string GreenLightColor  { get; } = HexToCuiColor("236956");
        public string GreenLightColor2 { get; } = HexToCuiColor("287862");
        public string PinkDarkColor    { get; } = HexToCuiColor("4d2247");
        public string BlueLightColor   { get; } = HexToCuiColor("1f9e94");

        [PluginReference] private Plugin ImageLibrary;

        public string MainColor { get; } = "0.2 0.5 0.39 1";


        private void OnServerInitialized()
        {
            if (!ImageLibrary)
            {
                PrintError("Donwload and install ImageLibrary to work with this plugin...");
            }
            /*
            BasePlayer player = FindBasePlayer("76561198077282054");
            closeBaraholkaUI(player);
            CuiHelper.AddUi(player, OpenBaraholka(player));

            player.SendConsoleCommand($"baraholkaui.drawfilter {player.UserIDString} Weapons");   //11111
            player.SendConsoleCommand($"baraholkaui.draworders {player.UserIDString}");   //11111
            //player.SendConsoleCommand($"baraholkaui.createorder {player.UserIDString}");   //11111

            */
            //BasePlayer player1 = FindBasePlayer("76561198428983821");
            //closeBaraholkaUI(player1);
            //CuiHelper.AddUi(player1, OpenBaraholka(player1));
            //player.SendConsoleCommand($"baraholkaui.draworders {player1.UserIDString}");
        }

        private void closeBaraholkaUI(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, Layer);
        }


        
        [ChatCommand("torg")]
        void openBaraholkaPlease(BasePlayer player)
        {
            if (player.IsAdmin)
                CuiHelper.AddUi(player, OpenBaraholka(player));
            else 
                SendReply(player, "У вас нет прав для выполнения этой команды");
        }

        [ConsoleCommand("baraholkaui.close")]
        private void closeBaraholkaUI(ConsoleSystem.Arg args)
        {
            Puts("fuck");

            BasePlayer player = args.Player();
            if (player == null) return;
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
                            Command  = $"BaraholkaUI.createorder {player.UserIDString}",
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


                //  лейбл с тем, сколько у пацана лаве
                {
                    new CuiLabel
                    {
                       Text = {
                            Text = "132.145.147.144.746",
                            FontSize = 13,
                            Align = TextAnchor.MiddleRight,
                        },
                        RectTransform = {
                            AnchorMin = "0.75 0.955",      // лево  низ
                            AnchorMax = "1 0.997",       // право верх
                            OffsetMax = "-30 0"
                        }
                    },
                    $"{Layer}.BaraholkaUI.rightPanel",
                    $"{Layer}.BaraholkaUI.rightPanel.myBalance"
                },



                
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
                            Command  =  $"baraholkaui.drawfilter {player.UserIDString} Resources 0 standing",
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
                            Command  =  $"baraholkaui.drawfilter {player.UserIDString} Resources 0 standing",
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
                            Command  =  $"baraholkaui.drawfilter {player.UserIDString} Resources 0 standing",
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

        [ConsoleCommand("baraholkaui.draworders")]
        private void draworders(ConsoleSystem.Arg args)
        {
            BasePlayer player = FindBasePlayer(args.GetString(0));

            List<int> counterList = new List<int>();
            counterList.Add(1);counterList.Add(2);counterList.Add(3);counterList.Add(4);counterList.Add(1);counterList.Add(2);counterList.Add(3);counterList.Add(4);

            // создание новой обвертки для всех маленьких блоков
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

            int start = 0;
            int stop = start+8;

            for (int i = start; i < stop; i++) {
            
                PrintWarning(i.ToString());

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
                                    Png = (string) ImageLibrary.Call("GetImage", "rifle.ak")
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
                                Text = "1000000000",
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
                                Text = "1000",
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
                                Command  = $"BaraholkaUI.TESTFUCKINGBTN",
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
            
            }

            
            
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
                    new BaseItems {
                        ShortName = "bow.hunting",
                        BasePrice = 0,
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
                        BasePrice = 0,
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
                        BasePrice = 22700,
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
                    new BaseItems
                    {
                        ShortName = "stones",
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