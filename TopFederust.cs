using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Oxide.Core;
using Oxide.Core.Libraries;
using Newtonsoft.Json.Linq;
using System.Globalization;
using Oxide.Core.Libraries.Covalence;

namespace Oxide.Plugins
{
    [Info("TopFederust", "Beorn", "1.0.0")]
    public class TopFederust : RustPlugin
    {
        [PluginReference]
        private Plugin TopCustom;

        [PluginReference]
        private Plugin Rep;

        private static string Layer => "topfederust";
        public string MainColor { get; } = "0.2 0.5 0.39 1";
        public string SecondColor { get; } = "0.5 0.2 0.39 1";
        public string DarkGreenColor { get; } = "0.25 0.55 0.39 1";
        public string BlackColor { get; } = "0 0 0 0.85";
        public string GreyColor { get; } = "0 0 0 0.65";

        public bool _isTopFedeRustGuiUsed = false;

        object OnPlayerCommand(ConsoleSystem.Arg arg)
        {

            BasePlayer player = arg.Player();
            if (player == null) return null;

            if (ProfileUsers == null) return null;

            var pl = ProfileUsers.Contains(player);

            if (pl)
            {
                return true;
            }

            return null;

        }


        void OnServerInitialized()
        {
            ProfileUsers = new List<BasePlayer>();
            foreach (BasePlayer player in BasePlayer.activePlayerList)
            {
                CloseProfile(player);
                ProfileUsers = new List<BasePlayer>() { };
                //if (player.displayName == "Beorn")
                //{
                //    player.SendConsoleCommand($"top");
                //}
            }

        }
        private List<BasePlayer> ProfileUsers { get; set; }


        [ConsoleCommand("top")]
        private void CmdOpenProfile(ConsoleSystem.Arg args)
        {
            BasePlayer player = args.Player();
            if (player == null) return;
            Puts($"{player.displayName} воспользовался плагином TopFederust");
            if (ProfileUsers.Contains(player))
            {
                CloseProfile(player);
                ProfileUsers.Remove(player);
                return;
            }
            FreezePlayer(player);
            ProfileUsers.Add(player);
            player.SendConsoleCommand("repgui.close.profile.atall");
            player.SendConsoleCommand("repgui.exit");
            player.SendConsoleCommand("playerprofile.close");
            CuiHelper.AddUi(player, OpenProfile(player));

            if (TopCustom != null)
                ListPlayerInTop(player, (List<BasePlayer>)TopCustom.Call("getPlayersListForTopSorted", "PVP_KD", BasePlayer.activePlayerList.ToList().Concat(BasePlayer.sleepingPlayerList).ToList()));
            else
            {
                ListPlayerInTop(player, BasePlayer.activePlayerList.ToList().Concat(BasePlayer.sleepingPlayerList).ToList());
                Puts("TopCustom not found...");
            }
        }


        [ChatCommand("top")]
        private void CmdOpenProfile(BasePlayer player, string command, string[] args)
        {
            player.SendConsoleCommand("top");
        }
        private CuiElementContainer OpenProfile(BasePlayer player)
        {
            var MainContainer = new CuiElementContainer
            {
                // Layer
                {
                    new CuiPanel
                    {
                        Image = { Color = "1 1 1 0"  },
                        RectTransform = { AnchorMin = "0.06 0.15", AnchorMax = "0.94 0.931" },
                        CursorEnabled = true
                    },
                    "Overlay",
                    Layer
                },
                {
                    new CuiButton
                    {
                        Text = { Text = "", FontSize = 20, Align = TextAnchor.MiddleCenter },
                        Button = { Command = $"topfederust.close", Color = "0 0 0 0" },
                        RectTransform = { AnchorMin = "-100 -100", AnchorMax = "100 100" }
                    },
                    Layer
                },

                #region Основная панель
                    // Основная панель
                    {
                        new CuiPanel
                        {
                            Image = { Color = "0 0 0 0.8" },
                            RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" }
                        },
                        Layer,
                        $"{Layer}.top"
                    },
                    
                    #region Хэдер
                        // Блок с таблицей игроков
                        {
                            new CuiPanel
                            {
                                Image = { Color = MainColor,  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat"},
                                RectTransform = { AnchorMin = "0 0.9", AnchorMax = "0.999 1" }
                            },
                            $"{Layer}.top",
                            $"{Layer}.header"
                        },

                        #region Тайтл

                        // Панель тайтл
                        {
                            new CuiPanel
                            {
                                Image = { Color = MainColor,  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat"},
                                RectTransform = { AnchorMin = "0.3 0", AnchorMax = "0.7 0.98" }
                            },
                            $"{Layer}.header",
                            $"{Layer}.header.nametitle"
                        },

                        // Таблица игроков
                        {
                            new CuiLabel
                            {
                                Text = { Text = $"Таблица игроков", FontSize = 20, Align = TextAnchor.MiddleCenter },
                                RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
                            },
                            $"{Layer}.header.nametitle",
                            $"{Layer}.header.nametitle.title"
                        },
                        #endregion
                        
                        #region Поле ввода
                        
                        // Блок с таблицей игроков
                        {
                            new CuiPanel
                            {
                                Image = { Color = MainColor,  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat"},
                                RectTransform = { AnchorMin = "0 0", AnchorMax = "0.3 0.9" }
                            },
                            $"{Layer}.header",
                            $"{Layer}.header.io"
                        },
                        {
                            new CuiElement
                            {
                                Parent = $"{Layer}.header.io",
                                Name = $"{Layer}.header.io" + ".Input",
                                Components =
                                {
                                    new CuiImageComponent { Color = HexToCuiColor("#0000007C")},
                                    new CuiRectTransformComponent { AnchorMin = "0.05 0.25", AnchorMax = "1 0.75" }
                                }
                            }
                        },

                        {
                            new CuiElement
                            {
                                Parent = $"{Layer}.header.io" + ".Input",
                                Name = $"{Layer}.header.io" + ".Input.Current",
                                Components =
                                {
                                    new CuiInputFieldComponent { FontSize = 16, Align = TextAnchor.MiddleCenter, Command = "topfederust.put", Text = "12345678"},
                                    new CuiRectTransformComponent { AnchorMin = "0 0", AnchorMax = "1 1" }
                                }
                            }
                        },
                     #endregion

                        #region Кнопки
                        
                        // Блок с таблицей игроков
                        //{
                        //    new CuiPanel
                        //    {
                        //        Image = { Color = "0 0 0 0" },
                        //        RectTransform = { AnchorMin = "0.9 0", AnchorMax = "1 0.98" }
                        //    },
                        //    $"{Layer}.header",
                        //    $"{Layer}.header.buttons"
                        //},
                        //{
                        //    new CuiButton
                        //    {
                        //        Text = { Text = "+", FontSize = 20, Align = TextAnchor.MiddleCenter },
                        //        Button = { Command = $"topfederust.online", Color = DarkGreenColor,  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                        //        RectTransform = { AnchorMin = "0.04 0.1", AnchorMax = "0.45 0.9" }
                        //    },
                        //    $"{Layer}.header.buttons",
                        //    $"{Layer}.header.buttons.online"
                        //},

                        //{
                        //    new CuiButton
                        //    {
                        //        Text = { Text = "-", FontSize = 20, Align = TextAnchor.MiddleCenter },
                        //        Button = { Command = $"topfederust.offline", Color = DarkGreenColor,  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                        //        RectTransform = { AnchorMin = "0.52 0.1", AnchorMax = "0.93 0.9" }
                        //    },
                        //    $"{Layer}.header.buttons",
                        //    $"{Layer}.header.buttons.offline"
                        //},  
                        #endregion


                    #endregion

                    
                    #region Таблица игроков
                        // Блок с таблицей игроков
                        {
                            new CuiPanel
                            {
                                Image = { Color = "0 0 0 0" },
                                RectTransform = { AnchorMin = "0 0", AnchorMax = "0.999 0.9" }
                            },
                            $"{Layer}.top",
                            $"{Layer}.body"
                        },
                        
                        #region Тайтл

                        // Панель тайтл
                        {
                            new CuiPanel
                            {
                                Image = { Color = BlackColor },
                                RectTransform = { AnchorMin = "0 0.90", AnchorMax = "0.999 0.996" }
                            },
                            $"{Layer}.body",
                            $"{Layer}.body.header"
                        },

                        // Имя
                        {
                            new CuiLabel
                            {
                                Text = { Text = $"Игрок (открыть профиль)", FontSize = 20, Align = TextAnchor.MiddleCenter },
                                RectTransform = { AnchorMin = "0 0", AnchorMax = "0.3 1" },
                            },
                            $"{Layer}.body.header",
                            $"{Layer}.body.header.name"
                        },

                        // Убийств
                        {
                            new CuiLabel
                            {
                                Text = { Text = $"Убийств", FontSize = 20, Align = TextAnchor.MiddleCenter },
                                RectTransform = { AnchorMin = "0.3 0", AnchorMax = "0.45 1" },
                            },
                            $"{Layer}.body.header",
                            $"{Layer}.body.header.kills"
                        },

                        // Смертей
                        {
                            new CuiLabel
                            {
                                Text = { Text = $"Смертей", FontSize = 20, Align = TextAnchor.MiddleCenter },
                                RectTransform = { AnchorMin = "0.45 0", AnchorMax = "0.6 1" },
                            },
                            $"{Layer}.body.header",
                            $"{Layer}.body.header.deaths"
                        },

                        // KD
                        {
                            new CuiLabel
                            {
                                Text = { Text = $"K/D", FontSize = 20, Align = TextAnchor.MiddleCenter },
                                RectTransform = { AnchorMin = "0.6 0", AnchorMax = "0.8 1" },
                            },
                            $"{Layer}.body.header",
                            $"{Layer}.body.header.kills"
                        },

                        // Репутация
                        {
                            new CuiLabel
                            {
                                Text = { Text = $"Репутация", FontSize = 20, Align = TextAnchor.MiddleCenter },
                                RectTransform = { AnchorMin = "0.8 0", AnchorMax = "1 1" },
                            },
                            $"{Layer}.body.header",
                            $"{Layer}.body.header.rep"
                        },

                        #endregion

                    #endregion

                #endregion
            };

            if (TopCustom != null)
                ListPlayerInTop(player, (List<BasePlayer>)TopCustom.Call("getPlayersListForTopSorted", "PVP_KD", BasePlayer.activePlayerList.ToList().Concat(BasePlayer.sleepingPlayerList).ToList()));
            else
            {
                ListPlayerInTop(player, BasePlayer.activePlayerList.ToList().Concat(BasePlayer.sleepingPlayerList).ToList());
                Puts("TopCustom not found...");
            }

            return MainContainer;
        }
        private void ClearPlayerList(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, $"{Layer}.body.list");
            CuiHelper.DestroyUi(player, $"{Layer}.body.none");

        }

        [ConsoleCommand("topfederust.put")]
        private void InputCmd(ConsoleSystem.Arg arg)
        {
            BasePlayer player = arg.Player();
            if (player == null) return;
            ClearPlayerList(player);
            var lst = BasePlayer.activePlayerList.ToList().Concat(BasePlayer.sleepingPlayerList).ToList();
            //Puts($"Count {(from x in lst where x.displayName.StartsWith(arg.FullString) select x).ToList().Count}");
            //Puts(player.displayName);

            if (TopCustom != null)
                ListPlayerInTop(player, (List<BasePlayer>)TopCustom.Call("getPlayersListForTopSorted", "PVP_KD", (from x in lst where x.displayName.ToLower().Contains(arg.FullString.ToLower()) select x).ToList()));
            else
            {
                ListPlayerInTop(player, BasePlayer.activePlayerList.ToList().Concat(BasePlayer.sleepingPlayerList).ToList());
                Puts("TopCustom not found...");
            }
        }

        private string GetPaintedTag(string name)
        {
            var newName = $"{name.Substring(0, name.Length > 20 ? 20 : name.Length)}";
            if (name.StartsWith("["))
            {
                int closeTagIndex = name.IndexOf("]");
                var tag = newName.Substring(0, closeTagIndex + 1);
                tag = $"<color=#43b67f>{tag}</color>";
                var oname = newName.Substring(closeTagIndex + 1);
                newName = tag + oname;

            }
            return newName;
        }

        private void ListPlayerInTop(BasePlayer player, List<BasePlayer> players)
        {
            var top = new Dictionary<string, string>() {
                        { "Gathering", "failed" },
                        { "KD", "failed"},
                        { "Explosion", "failed"},
                        { "Rep", "failed"}
                    };

            var firstIndex = 0;
            var lastIndex = 0;
            var a = 0;
            var playersOnline = players.Count;
            // Puts($"{playersOnline}");
            if (playersOnline > 0 && playersOnline <= 10)
            {
                lastIndex = playersOnline;
            }
            else if (playersOnline > 10)
            {
                lastIndex = 10;
            }
            // Если нет игроков
            if (lastIndex == 0)
            {
                CuiHelper.AddUi(player, new CuiElementContainer()
                {
                    {
                        // Панель тайтл
                        new CuiPanel
                        {
                            Image = { Color = a % 2 == 0 ? GreyColor : BlackColor },
                            RectTransform = { AnchorMin = "0 0", AnchorMax = "0.999 0.996" }
                        },
                        $"{Layer}.body",
                        $"{Layer}.body.none"
                    },
                    {
                        new CuiLabel
                        {
                            Text = { Text = $"Нет игроков", FontSize = 30, Align = TextAnchor.MiddleCenter },
                            RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
                        },
                        $"{Layer}.body.none",
                        $"{Layer}.body.none.title"
                    }
                });

            }
            else
            {
                CuiHelper.AddUi(player, new CuiElementContainer()
                {
                        {
                            // Панель тайтл
                            new CuiPanel
                            {
                                Image = { Color = "0 0 0 0" },
                                RectTransform = { AnchorMin = $"0 0", AnchorMax = $"0.999 1" }
                            },
                            $"{Layer}.body",
                            $"{Layer}.body.list"
                        }
                });

                //1         //10
                foreach (var item in players.GetRange(firstIndex, lastIndex))
                {
                    float repPlus = Convert.ToInt32((string)Rep.CallHook("GetRepByPlayerPos", item));
                    float repMinus = Convert.ToInt32((string)Rep.CallHook("GetRepByPlayerNeg", item));
                    float Sum = repPlus + repMinus;

                    float repPlusPercents = repPlus / Sum;
                    float repMinusPercents = repMinus / Sum;

                    var name = GetPaintedTag(item.displayName);

                    if (TopCustom != null) top = (Dictionary<string, string>)TopCustom.Call("ReturnPlayersStatisticsForTop", item);
                    CuiHelper.AddUi(player, new CuiElementContainer()
                    {
                        {
                            // Панель тайтл
                            new CuiPanel
                            {
                                Image = { Color = a % 2 == 0 ? GreyColor : BlackColor },
                                RectTransform = { AnchorMin = $"0 { 0.81 - 0.09 * a}", AnchorMax = $"0.999 { 0.898 - 0.09 * a}" }
                            },
                            $"{Layer}.body.list",
                            $"{Layer}.body.list.{a}"
                        },
                        {
                            new CuiButton
                            {
                                Text = { Text = name, FontSize = 20, Align = TextAnchor.MiddleLeft },
                                RectTransform = { AnchorMin = "0.05 0", AnchorMax = "0.3 1" },
                                Button = { Command = $"profile {item.UserIDString} back", Color = "0 0 0 0" }
                            },
                            $"{Layer}.body.list.{a}",
                            $"{Layer}.body.list.{a}.name"
                        },
                        {
                            new CuiLabel
                            {
                                Text = { Text = $"{top["Kills"]}", FontSize = 20, Align = TextAnchor.MiddleCenter },
                                RectTransform = { AnchorMin = "0.3 0", AnchorMax = "0.45 1" },
                            },
                            $"{Layer}.body.list.{a}",
                            $"{Layer}.body.list.{a}.kills"
                        },
                        {
                            new CuiLabel
                            {
                                Text = { Text = $"{top["Deaths"]}", FontSize = 20, Align = TextAnchor.MiddleCenter },
                                RectTransform = { AnchorMin = "0.45 0", AnchorMax = "0.6 1" },
                            },
                            $"{Layer}.body.list.{a}",
                            $"{Layer}.body.list.{a}.deaths"
                        },
                        {
                            new CuiLabel
                            {
                                Text = { Text = $"{top["K/D"]}", FontSize = 20, Align = TextAnchor.MiddleCenter },
                                RectTransform = { AnchorMin = "0.6 0", AnchorMax = "0.8 1" },
                            },
                            $"{Layer}.body.list.{a}",
                            $"{Layer}.body.list.{a}.deaths"
                        },
                        
                        #region Репутация
                

                        #endregion

                    });

                    if ((repPlus == 0) && (repMinus == 0))
                    {
                        CuiHelper.AddUi(player, new CuiElementContainer()
                        {
                            // Прогресс-бар "Репутация" (Вся шкала)
                            {
                                new CuiPanel
                                {
                                    Image = { Color = "0 0 0 0",  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                                    RectTransform = { AnchorMin = "0.8 0.3", AnchorMax = "0.99 0.7" }
                                },
                                $"{Layer}.body.list.{a}",
                                $"{Layer}.body.list.{a}.rep"
                            },
                            // Прогресс-бар "Репутация" (Плюс)
                            {
                                new CuiPanel
                                {
                                    Image = { Color = GreyColor,  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                                    RectTransform = { AnchorMin = "0 0", AnchorMax = $"1 1" }
                                },
                                $"{Layer}.body.list.{a}.rep",
                                $"{Layer}.body.list.{a}.rep.none"
                            },
                            // Прогресс-бар "Репутация" (Плюс) Тайтл
                            {
                                new CuiLabel
                                {
                                    Text = { Text = $"0%", FontSize = 12, Align = TextAnchor.MiddleCenter },
                                    RectTransform = { AnchorMin = "0.2 0", AnchorMax = "0.8 1" },
                                },
                                $"{Layer}.body.list.{a}.rep.none"
                            },
                        });
                    }
                    else
                    {
                        CuiHelper.AddUi(player, new CuiElementContainer()
                        {
                            
                            // Прогресс-бар "Репутация" (Вся шкала)
                            {
                                new CuiPanel
                                {
                                    Image = { Color = "0 0 0 0",  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                                    RectTransform = { AnchorMin = "0.8 0.3", AnchorMax = "0.99 0.7" }
                                },
                                $"{Layer}.body.list.{a}",
                                $"{Layer}.body.list.{a}.rep"
                            },
                            // Прогресс-бар "Репутация" (Плюс)
                            {
                                new CuiPanel
                                {
                                    Image = { Color = MainColor,  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                                    RectTransform = { AnchorMin = "0 0", AnchorMax = $"{repPlusPercents} 1" }
                                },
                                $"{Layer}.body.list.{a}.rep",
                                $"{Layer}.body.list.{a}.rep.plus"
                            },
                            // Прогресс-бар "Репутация" (Плюс) Тайтл
                            {
                                new CuiLabel
                                {
                                    Text = { Text = $"{Math.Round(repPlusPercents * 100)}%", FontSize = 12, Align = TextAnchor.MiddleCenter },
                                    RectTransform = { AnchorMin = "0.2 0", AnchorMax = "0.8 1" },
                                },
                                $"{Layer}.body.list.{a}.rep.plus"
                            },

                            // Прогресс-бар "Репутация" (Минус)
                            {
                                new CuiPanel
                                {
                                    Image = { Color = SecondColor,  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                                    RectTransform = { AnchorMin = $"{repPlusPercents} 0", AnchorMax = "1 1" }
                                },
                                $"{Layer}.body.list.{a}.rep",
                                $"{Layer}.body.list.{a}.rep.minus"
                            },
                            // Прогресс-бар "Репутация" (Минус) Тайтл
                            {
                                new CuiLabel
                                {
                                    Text = { Text = $"{Math.Round(repMinusPercents * 100)}%", FontSize = 12, Align = TextAnchor.MiddleCenter },
                                    RectTransform = { AnchorMin = "0.1 0", AnchorMax = "0.9 1" },
                                },
                                $"{Layer}.body.list.{a}.rep.minus"
                            },

                        });
                    }

                    a += 1;
                }
            }
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

        private void CloseProfile(BasePlayer player)
        {
            UnfreezePlayer(player.IPlayer);
            CuiHelper.DestroyUi(player, Layer);
            if (ProfileUsers.Contains(player))
            {
                ProfileUsers.Remove(player);
            }
        }

        [ConsoleCommand("topfederust.close")]
        private void CloseProfile(ConsoleSystem.Arg args)
        {
            BasePlayer player = args.Player();
            UnfreezePlayer(player.IPlayer);
            if (ProfileUsers.Contains(player))
            {
                ProfileUsers.Remove(player);
            }
            if (player == null) return;
            CuiHelper.DestroyUi(player, Layer);
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


        private readonly Dictionary<string, Timer> timers = new Dictionary<string, Timer>();

        private const string permFrozen = "TopFederust.frozen";

        void Init()
        {
            permission.RegisterPermission(permFrozen, this);
        }


        private void FreezePlayer(BasePlayer playerBase)
        {
            var player = playerBase.IPlayer;
            player.GrantPermission(permFrozen);

            GenericPosition pos = player.Position();
            timers[player.Id] = timer.Every(0.001f, () =>
            {
                if (!player.IsConnected)
                {
                    timers[player.Id].Destroy();
                    return;
                }

                if (!player.HasPermission(permFrozen))
                {
                    UnfreezePlayer(player);
                }
                else
                {
                    player.Teleport(pos.X, pos.Y, pos.Z);
                }
            });
        }

        private void UnfreezePlayer(IPlayer player)
        {
            player.RevokePermission(permFrozen);

            if (timers.ContainsKey(player.Id))
            {
                timers[player.Id].Destroy();
            }
        }

    }
}
