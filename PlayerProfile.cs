using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Oxide.Core;
using Oxide.Core.Libraries;
using Newtonsoft.Json.Linq;

namespace Oxide.Plugins
{
    [Info("PlayerProfile", "Beorn", "1.0.0")]
    public class PlayerProfile : RustPlugin
    {

        [PluginReference]
        private Plugin StoreBonus;

        [PluginReference]
        private Plugin Rep;

        [PluginReference]
        private Plugin BannerSystem;

        [PluginReference]
        private Plugin TopCustom;

        private const string playerprofilePerm = "playerprofile.perm";
        private static string Layer => "layer";
        private static string Base => "playerprofile";
        private List<BasePlayer> ProfileUsers { get; set; }

        public string MainColor { get; } = "0.2 0.5 0.39 1";
        public string SecondColor { get; } = "0.5 0.2 0.39 1";
        public List<string> AdminsList { get; } = new List<string>() { "76561198077282054", "76561198033885552" };

        void Init()
        {
            permission.RegisterPermission(playerprofilePerm, this);
        }

        #region Профиль
        void OnServerInitialized()
        {
            ProfileUsers = new List<BasePlayer>();
            foreach (BasePlayer player in BasePlayer.activePlayerList)
            {
                CloseProfile(player);
                //if (player.displayName == "Beorn")
                //{
                //    player.SendConsoleCommand($"profile {player.UserIDString}");
                //}
            }

        }

        [ConsoleCommand("profile")]
        private void CmdOpenProfile(ConsoleSystem.Arg args)
        {
            if (args.Player() == null)
                return;
            BasePlayer player = args.Player();
            var target = player;
            var backToTopFederust = "";
            Puts(args.Args.Length.ToString());
            if (args.Args.Count() == 1)
            {
                target = FindBasePlayer(args.GetString(0));

                if (target == null)
                {
                    target = player;
                }
            }

            if (args.Args.Count() == 2)
            {
                target = FindBasePlayer(args.GetString(0));

                if (target == null)
                {
                    target = player;
                }

                if (args.GetString(1) == "back")
                {
                    backToTopFederust = "back";
                }
            }

            if (!player.IPlayer.HasPermission(playerprofilePerm))
            {
                SendReply(player, "У вас нет прав на выполнение этой команды");
                return;
            }

            player.SendConsoleCommand("topfederust.close");
            player.SendConsoleCommand("repgui.close.profile.atall");
            player.SendConsoleCommand("repgui.exit");

            if (ProfileUsers.Contains(player))
            {
                ProfileUsers.Remove(player);
                CloseProfile(player);
                return;
            }
            ProfileUsers.Add(player);
            CuiHelper.AddUi(player, OpenProfile(player, target, backToTopFederust));
        }

        [ChatCommand("profile")]
        private void CmdOpenProfile(BasePlayer player, string command, string[] args)
        {
            BasePlayer target = player;
            if (args.Length == 1)
            {
                target = FindBasePlayer(args[0]);
                if (target == null)
                {
                    SendReply(player, "Player Not Found");
                    return;
                }
            }

            if (!player.IPlayer.HasPermission(playerprofilePerm))
            {
                SendReply(player, "У вас нет прав на выполнение этой команды");
                return;
            }

            player.SendConsoleCommand("topfederust.close");
            player.SendConsoleCommand("repgui.close.profile.atall");
            player.SendConsoleCommand("repgui.exit");

            if (ProfileUsers.Contains(player))
            {
                ProfileUsers.Remove(player);
                CloseProfile(player);
                return;
            }
            ProfileUsers.Add(player);
            CuiHelper.AddUi(player, OpenProfile(player, target, ""));
        }

        private void CloseProfile(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, Layer);
            ProfileUsers.Remove(player);
        }
        [ConsoleCommand("playerprofile.close")]
        private void CloseProfile(ConsoleSystem.Arg args)
        {
            BasePlayer player = args.Player();
            if (player == null) return;
            CuiHelper.DestroyUi(player, Layer);
            ProfileUsers.Remove(player);
            Puts(args.FullString);
            if (args.FullString == "back")
            {
                player.SendConsoleCommand("ftop");
            }
        }

        void GetCallback(int code, string response, BasePlayer player)
        {
            if (response == null || code != 200)
            {
                Puts($"Error: {code} - Couldn't get an answer from Steam for {player.displayName}");
                return;
            }
            JObject res = JObject.Parse(response);
            var cur = res.SelectToken("response.players")[0].SelectToken("avatarfull");

            CuiHelper.AddUi(player, new CuiElementContainer { CreateImage($"{Layer}.profile.panelone.photo", cur.ToString()) });
        }



        private CuiElementContainer OpenProfile(
            BasePlayer player,
            BasePlayer target,
            string back,
            string nickname = "",
            string userIdString = "",
            Dictionary<string, string> statistics = null,
            Dictionary<string, string> gathering = null,
            Dictionary<string, string> other = null,
            Dictionary<string, string> top = null)
        {
            nickname = target.displayName;
            if (nickname.Length > 20)
                nickname = nickname.Substring(0, 20);

            string time = "";

            if (StoreBonus != null)
            {
                time = (string)StoreBonus.Call("GetPlayerTimeOnServer", target);
            }

            if (TopCustom != null)
            {
                statistics = (Dictionary<string, string>)TopCustom.Call("ReturnPlayersStatistics", target);
                gathering = (Dictionary<string, string>)TopCustom.Call("ReturnPlayersGathering", target);
                other = (Dictionary<string, string>)TopCustom.Call("ReturnPlayersOther", target);
                top = (Dictionary<string, string>)TopCustom.Call("ReturnPlayersTop", target);
            }

            if (statistics == null)
                statistics = new Dictionary<string, string>() {
                { "Kills", "failed" },
                { "Deaths", "failed"},
                { "GatheredByHands", "failed"},
                { "GatheredByCareer", "failed"},
                { "shootDistance", "failed"},
                { "radHouseSingle", "failed"},
                { "RepPlus", (string)Rep.Call("GetRepByPlayerPos", target) ?? "failed"},
                { "RepMinus", (string)Rep.Call("GetRepByPlayerNeg", target) ?? "failed"}
            };

            if (gathering == null)
                gathering = new Dictionary<string, string>() {
                { "Sulfure", "failed" },
                { "MetalOre", "failed"},
                { "Stone", "failed"},
                { "Tree", "failed"}
            };

            if (other == null)
                other = new Dictionary<string, string>() {
                { "Shots", "failed" },
                { "Explosions", "failed"},
                { "HeliCrashed", "failed"},
                { "PanzerDestroyed", "failed"},
                { "NPCKilled", "failed"}
            };

            if (top == null)
                top = new Dictionary<string, string>() {
                { "Gathering", "failed" },
                { "KD", "failed"},
                { "Explosion", "failed"},
                { "Rep", (string)Rep.Call("GetRepByPlayerPos", target) ?? "failed"},
                { "radHouse", "failed" }
            };

            float repPlus = Convert.ToInt32(statistics["RepPlus"]);
            float repMinus = Convert.ToInt32(statistics["RepMinus"]);
            float Sum = repPlus + repMinus;

            float repPlusPercents = repPlus / Sum;
            float repMinusPercents = repMinus / Sum;

            List<string> privilegiesMember = new List<string>();
            List<string> allPrivilegies = new List<string>();
            allPrivilegies.Add("Бомж");
            allPrivilegies.Add("Барон");
            allPrivilegies.Add("Граф");
            allPrivilegies.Add("Король");

            List<string> groupsMember = new List<string>();
            List<string> allGroups = new List<string>();
            allGroups.Add("Модератор");
            allGroups.Add("Игрок");
            allGroups.Add("Администратор");

            if (target.IPlayer != null)
            {
                if (target.IPlayer.BelongsToGroup("bomg")) privilegiesMember.Add("Бомж");
                if (target.IPlayer.BelongsToGroup("baron")) privilegiesMember.Add("Барон");
                if (target.IPlayer.BelongsToGroup("graf")) privilegiesMember.Add("Граф");
                if (target.IPlayer.BelongsToGroup("king")) privilegiesMember.Add("Король");
                if (target.IPlayer.BelongsToGroup("moderator")) groupsMember.Add("Модератор");
                if (target.IPlayer.BelongsToGroup("default")) groupsMember.Add("Игрок");
                if ((target.IPlayer.BelongsToGroup("admin")) && (AdminsList.Contains(target.UserIDString))) groupsMember.Add("Администратор");
            }

            //CuiHelper.AddUi(player, container);

            //webrequest.Enqueue($"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key=4D07BD7B441615DF886DF708D7458274&steamids={target.UserIDString}", null, (code, response) =>
            //        GetCallback(code, response, player), this, RequestMethod.GET);


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
                        Button = { Command = $"playerprofile.close {back}", Color = "0 0 0 0" },
                        RectTransform = { AnchorMin = "-100 -100", AnchorMax = "100 100" }
                    },
                    Layer
                },

                #region Первая панель
                    // Первая панель
                    {
                        new CuiPanel
                        {
                            Image = { Color = "0 0 0 0.8" },
                            RectTransform = { AnchorMin = "0 0", AnchorMax = "0.25 1" }
                        },
                        Layer,
                        $"{Layer}.profile.panelone"
                    },

                    #region Ник-нейм
                        // Блок с именем
                        {
                            new CuiPanel
                            {
                                Image = { Color = MainColor,  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat"},
                                RectTransform = { AnchorMin = "0 0.9", AnchorMax = "0.996 1" }
                            },
                            $"{Layer}.profile.panelone",
                            $"{Layer}.profile.panelone.nametitle"
                        },

                        // Ник-нейм
                        {
                            new CuiLabel
                            {
                                Text = { Text = $"{nickname}", FontSize = 20, Align = TextAnchor.MiddleCenter },
                                RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
                            },
                            $"{Layer}.profile.panelone.nametitle",
                            $"{Layer}.profile.panelone.nametitle.title"
                        },
                        #endregion

                    #region Фотография
                        // Блок с фотографией
                        //{
                        //    new CuiPanel
                        //    {
                        //        Image = { Color = "0 0 0 0",  /* Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat"*/ },
                        //        RectTransform = { AnchorMin = "0 0.55", AnchorMax = "0.996 0.87" }
                        //    },
                        //    $"{Layer}.profile.panelone",
                        //    $"{Layer}.profile.panelone.photo"
                        //},
                        #endregion
                        
                    #region Баннер

                    // Блок "Баннер"
                    {
                        new CuiPanel
                        {
                            Image = { Color = "0 0 0 0",  /* Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat"*/ },
                            RectTransform = { AnchorMin = "0 0.78", AnchorMax = "0.993 0.89" }
                        },
                        $"{Layer}.profile.panelone",
                        $"{Layer}.profile.panelone.banner"
                    },

                    #endregion

                    #region Статистика
                        // Блок "Статистика"
                        {
                            new CuiPanel
                            {
                                Image = { Color = "0 0 0 0",  /* Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat"*/ },
                                RectTransform = { AnchorMin = "0 0.28", AnchorMax = "0.996 0.78" }
                            },
                            $"{Layer}.profile.panelone",
                            $"{Layer}.profile.panelone.stats"
                        },

                        // Title Статистика
                        {
                            new CuiLabel
                            {
                                Text = { Text = "Статистика", FontSize = 18, Align = TextAnchor.MiddleCenter },
                                RectTransform = { AnchorMin = "0 0.8", AnchorMax = "1 1" },
                            },
                            $"{Layer}.profile.panelone.stats",
                            $"{Layer}.profile.panelone.stats.title"
                        },

                            #region Левый блок "Тайтлы"

                            // Левый блок "Тайтлы"
                            {
                                new CuiPanel
                                {
                                    Image = { Color = "0 0 0 0",  /* Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat"*/ },
                                    RectTransform = { AnchorMin = "0 0", AnchorMax = "0.8 0.75" }
                                },
                                $"{Layer}.profile.panelone.stats",
                                $"{Layer}.profile.panelone.stats.left"
                            },
                
                            // Убийств
                            {
                                new CuiLabel
                                {
                                    Text = { Text = "Убийств:", FontSize = 15, Align = TextAnchor.MiddleLeft },
                                    RectTransform = { AnchorMin = "0.12 0.86", AnchorMax = "1 0.99" },
                                },
                                $"{Layer}.profile.panelone.stats.left"
                            },

                            // Смертей
                            {
                                new CuiLabel
                                {
                                    Text = { Text = "Смертей:", FontSize = 15, Align = TextAnchor.MiddleLeft },
                                    RectTransform = { AnchorMin = "0.12 0.71", AnchorMax = "1 0.84" },
                                },
                                $"{Layer}.profile.panelone.stats.left"
                            },

                            // Самый дальний выстрел
                            {
                                new CuiLabel
                                {
                                    Text = { Text = "Самый дальний выстрел:", FontSize = 15, Align = TextAnchor.MiddleLeft },
                                    RectTransform = { AnchorMin = "0.12 0.56", AnchorMax = "1 0.69" },
                                },
                                $"{Layer}.profile.panelone.stats.left"
                            },

                            // Добыто Руками
                            {
                                new CuiLabel
                                {
                                    Text = { Text = "Добыто Руками:", FontSize = 15, Align = TextAnchor.MiddleLeft },
                                    RectTransform = { AnchorMin = "0.12 0.41", AnchorMax = "1 0.54" },
                                },
                                $"{Layer}.profile.panelone.stats.left"
                            },

                            // Добыто Карьером
                            {
                                new CuiLabel
                                {
                                    Text = { Text = "Добыто Карьером:", FontSize = 15, Align = TextAnchor.MiddleLeft },
                                    RectTransform = { AnchorMin = "0.12 0.26", AnchorMax = "1 0.39" },
                                },
                                $"{Layer}.profile.panelone.stats.left"
                            },

                            // Радиоактивных домов залутано
                            {
                                new CuiLabel
                                {
                                    Text = { Text = "Залутано рад. домов:", FontSize = 15, Align = TextAnchor.MiddleLeft },
                                    RectTransform = { AnchorMin = "0.12 0.11", AnchorMax = "1 0.24" },
                                },
                                $"{Layer}.profile.panelone.stats.left"
                            },

                            // Репутация
                            {
                                new CuiLabel
                                {
                                    Text = { Text = "Репутация:", FontSize = 15, Align = TextAnchor.MiddleLeft },
                                    RectTransform = { AnchorMin = "0.12 -0.03", AnchorMax = "0.65 0.09" },
                                },
                                $"{Layer}.profile.panelone.stats.left"
                            },

                            #endregion

                            #region Правый блок "Значения"

                        // Правый блок "Значения"
                        {
                            new CuiPanel
                            {
                                Image = { Color = "0 0 0 0",  /* Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat"*/ },
                                RectTransform = { AnchorMin = "0.5 0", AnchorMax = "0.996 0.75" }
                            },
                            $"{Layer}.profile.panelone.stats",
                            $"{Layer}.profile.panelone.stats.right"
                        },
                
                        // Убийств
                        {
                            new CuiLabel
                            {
                                Text = { Text =  $"{statistics["Kills"]}", FontSize = 15, Align = TextAnchor.MiddleRight },
                                RectTransform = { AnchorMin = "0 0.86", AnchorMax = "0.9 0.99" },
                            },
                            $"{Layer}.profile.panelone.stats.right"
                        },

                        // Смертей
                        {
                            new CuiLabel
                            {
                                Text = { Text = $"{statistics["Deaths"]}", FontSize = 15, Align = TextAnchor.MiddleRight },
                                RectTransform = { AnchorMin = "0 0.71", AnchorMax = "0.9 0.84" },
                            },
                            $"{Layer}.profile.panelone.stats.right"
                        },

                        // Самый дальний выстрел
                        {
                            new CuiLabel
                            {
                                Text = { Text = $"{statistics["shootDistance"]}", FontSize = 15, Align = TextAnchor.MiddleRight },
                                RectTransform = { AnchorMin = "0 0.56", AnchorMax = "0.9 0.69" },
                            },
                            $"{Layer}.profile.panelone.stats.right"
                        },

                        // Добыто Руками
                        {
                            new CuiLabel
                            {
                                Text = { Text = $"{statistics["GatheredByHands"]}", FontSize = 15, Align = TextAnchor.MiddleRight },
                                RectTransform = { AnchorMin = "0 0.41", AnchorMax = "0.9 0.54" },
                            },
                            $"{Layer}.profile.panelone.stats.right"
                        },

                        // Добыто Карьером
                        {
                            new CuiLabel
                            {
                                Text = { Text = $"{statistics["GatheredByCareer"]}", FontSize = 15, Align = TextAnchor.MiddleRight },
                                RectTransform = { AnchorMin = "0 0.26", AnchorMax = "0.9 0.39" },
                            },
                            $"{Layer}.profile.panelone.stats.right"
                        },

                        // Радиоактивных домов залутано
                        {
                            new CuiLabel
                            {
                                Text = { Text = $"{statistics["radHouseSingle"]}", FontSize = 15, Align = TextAnchor.MiddleRight },
                                RectTransform = { AnchorMin = "0 0.11", AnchorMax = "0.9 0.24" },
                            },
                            $"{Layer}.profile.panelone.stats.right"
                        },

                            #region Репутация
                
                            // Прогресс-бар "Репутация" (Вся шкала)
                            {
                                new CuiPanel
                                {
                                    Image = { Color = "0 0 0 0",  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                                    RectTransform = { AnchorMin = "0 -0.02", AnchorMax = "0.9 0.08" }
                                },
                                $"{Layer}.profile.panelone.stats.right",
                                $"{Layer}.profile.panelone.stats.right.rep"
                            },

                            // Прогресс-бар "Репутация" (Плюс)
                            {
                                new CuiPanel
                                {
                                    Image = { Color = MainColor,  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                                    RectTransform = { AnchorMin = "0 0", AnchorMax = $"{repPlusPercents} 1" }
                                },
                                $"{Layer}.profile.panelone.stats.right.rep",
                                $"{Layer}.profile.panelone.stats.right.rep.plus"
                            },
                            // Прогресс-бар "Репутация" (Плюс) Тайтл
                            {
                                new CuiLabel
                                {
                                    Text = { Text = $"{Math.Round(repPlusPercents * 100)}%", FontSize = 12, Align = TextAnchor.MiddleCenter },
                                    RectTransform = { AnchorMin = "0.2 0", AnchorMax = "0.8 1" },
                                },
                                $"{Layer}.profile.panelone.stats.right.rep.plus"
                            },

                            // Прогресс-бар "Репутация" (Минус)
                            {
                                new CuiPanel
                                {
                                    Image = { Color = SecondColor,  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                                    RectTransform = { AnchorMin = $"{repPlusPercents} 0", AnchorMax = "1 1" }
                                },
                                $"{Layer}.profile.panelone.stats.right.rep",
                                $"{Layer}.profile.panelone.stats.right.rep.minus"
                            },
                            // Прогресс-бар "Репутация" (Минус) Тайтл
                            {
                                new CuiLabel
                                {
                                    Text = { Text = $"{Math.Round(repMinusPercents * 100)}%", FontSize = 12, Align = TextAnchor.MiddleCenter },
                                    RectTransform = { AnchorMin = "0.1 0", AnchorMax = "0.9 1" },
                                },
                                $"{Layer}.profile.panelone.stats.right.rep.minus"
                            },

                            #endregion

                        #endregion

                        #endregion

                    #region Репутация
                    // Блок "Репутация"
                    {
                        new CuiPanel
                        {
                            Image = { Color = "0 0 0 0.4",  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                            RectTransform = { AnchorMin = "0.05 0.02", AnchorMax = "0.95 0.11" }
                        },
                        $"{Layer}.profile.panelone",
                        $"{Layer}.profile.panelone.rep"
                    },
                    {
                        new CuiButton
                        {
                            Text = { Text = "Репутация", FontSize = 20, Align = TextAnchor.MiddleCenter },
                            Button = { Command = $"playerprofile.open.reputaion.profile {target.UserIDString}", Color = MainColor,  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                            RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" }
                        },
                        $"{Layer}.profile.panelone.rep",
                        $"{Layer}.profile.panelone.rep.button"
                    },
                    #endregion
                
                #endregion
                
                #region Вторая панель

                    // Вторая панель
                    {
                        new CuiPanel
                        {
                            Image = { Color = "0 0 0 0.8" },
                            RectTransform = { AnchorMin = "0.27 0", AnchorMax = "0.52 1" }
                        },
                        Layer,
                        $"{Layer}.profile.paneltwo"
                    },

                    #region Добыча

                    // Блок "Добыча"
                    {
                        new CuiPanel
                        {
                            Image = { Color = MainColor,  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat"},
                            RectTransform = { AnchorMin = "0 0.9", AnchorMax = "0.997 1" }
                        },
                        $"{Layer}.profile.paneltwo",
                        $"{Layer}.profile.paneltwo.gatheringtitle"
                    },
                    // Ник-нейм
                    {
                        new CuiLabel
                        {
                            Text = { Text = "Добыча", FontSize = 20, Align = TextAnchor.MiddleCenter },
                            RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
                        },
                        $"{Layer}.profile.paneltwo.gatheringtitle",
                        $"{Layer}.profile.paneltwo.gatheringtitle.title"
                    },

                #endregion
                    
                    #region Добыча Ресурсов

                        // Блок "Добыча ресурсов"
                        {
                            new CuiPanel
                            {
                                Image = { Color = "0 0 0 0",  /* Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat"*/ },
                                RectTransform = { AnchorMin = "0 0.5", AnchorMax = "0.997 0.87" }
                            },
                            $"{Layer}.profile.paneltwo",
                            $"{Layer}.profile.paneltwo.resourcegathering"
                        },

                        // Title "Добыча ресурсов"
                        {
                            new CuiLabel
                            {
                                Text = { Text = "Добыча ресурсов", FontSize = 18, Align = TextAnchor.MiddleCenter },
                                RectTransform = { AnchorMin = "0 0.8", AnchorMax = "1 1" },
                            },
                            $"{Layer}.profile.paneltwo.resourcegathering",
                            $"{Layer}.profile.paneltwo.resourcegathering.title"
                        },

                            #region Левый блок "Тайтлы"

                            // Левый блок "Тайтлы"
                            {
                                new CuiPanel
                                {
                                    Image = { Color = "0 0 0 0",  /* Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat"*/ },
                                    RectTransform = { AnchorMin = "0 0", AnchorMax = "0.6 0.75" }
                                },
                                $"{Layer}.profile.paneltwo.resourcegathering",
                                $"{Layer}.profile.paneltwo.resourcegathering.left"
                            },
                
                            // Серная руда
                            {
                                new CuiLabel
                                {
                                    Text = { Text = "Серная руда:", FontSize = 15, Align = TextAnchor.MiddleLeft },
                                    RectTransform = { AnchorMin = "0.1 0.81", AnchorMax = "1 0.99" },
                                },
                                $"{Layer}.profile.paneltwo.resourcegathering.left"
                            },

                            // Металлическая руда
                            {
                                new CuiLabel
                                {
                                    Text = { Text = "Металлическая руда:", FontSize = 15, Align = TextAnchor.MiddleLeft },
                                    RectTransform = { AnchorMin = "0.1 0.61", AnchorMax = "1 0.79" },
                                },
                                $"{Layer}.profile.paneltwo.resourcegathering.left"
                            },

                            // Камень
                            {
                                new CuiLabel
                                {
                                    Text = { Text = "Камень:", FontSize = 15, Align = TextAnchor.MiddleLeft },
                                    RectTransform = { AnchorMin = "0.1 0.41", AnchorMax = "1 0.59" },
                                },
                                $"{Layer}.profile.paneltwo.resourcegathering.left"
                            },

                            // Дерево
                            {
                                new CuiLabel
                                {
                                    Text = { Text = "Дерево:", FontSize = 15, Align = TextAnchor.MiddleLeft },
                                    RectTransform = { AnchorMin = "0.1 0.21", AnchorMax = "1 0.39" },
                                },
                                $"{Layer}.profile.paneltwo.resourcegathering.left"
                            },

                                #endregion

                            #region Правый блок "Значения"

                            // Правый блок "Значения"
                            {
                                new CuiPanel
                                {
                                    Image = { Color = "0 0 0 0",  /* Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat"*/ },
                                    RectTransform = { AnchorMin = "0.63 0", AnchorMax = "0.996 0.75" }
                                },
                                $"{Layer}.profile.paneltwo.resourcegathering",
                                $"{Layer}.profile.paneltwo.resourcegathering.right"
                            },
                
                            // Серная руда
                            {
                                new CuiLabel
                                {
                                    Text = { Text =  $"{gathering["Sulfure"]}", FontSize = 15, Align = TextAnchor.MiddleRight },
                                    RectTransform = { AnchorMin = "0 0.81", AnchorMax = "0.8 0.99" },
                                },
                                $"{Layer}.profile.paneltwo.resourcegathering.right"
                            },

                            // Металлическая руда
                            {
                                new CuiLabel
                                {
                                    Text = { Text = $"{gathering["MetalOre"]}", FontSize = 15, Align = TextAnchor.MiddleRight },
                                    RectTransform = { AnchorMin = "0 0.61", AnchorMax = "0.8 0.79" },
                                },
                                $"{Layer}.profile.paneltwo.resourcegathering.right"
                            },

                            // Камень
                            {
                                new CuiLabel
                                {
                                    Text = { Text = $"{gathering["Stone"]}", FontSize = 15, Align = TextAnchor.MiddleRight },
                                    RectTransform = { AnchorMin = "0 0.41", AnchorMax = "0.8 0.59" },
                                },
                                $"{Layer}.profile.paneltwo.resourcegathering.right"
                            },

                            // Дерево
                            {
                                new CuiLabel
                                {
                                    Text = { Text = $"{gathering["Tree"]}", FontSize = 15, Align = TextAnchor.MiddleRight },
                                    RectTransform = { AnchorMin = "0 0.21", AnchorMax = "0.8 0.39" },
                                },
                                $"{Layer}.profile.paneltwo.resourcegathering.right"
                            },

                            #endregion

	                #endregion
                    
                    #region Остальное
                    
                    // Блок "Остальное"
                    {
                        new CuiPanel
                        {
                            Image = { Color = "0 0 0 0",  /* Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat"*/ },
                            RectTransform = { AnchorMin = "0 0.14", AnchorMax = "0.997 0.48" }
                        },
                        $"{Layer}.profile.paneltwo",
                        $"{Layer}.profile.paneltwo.other"
                    },
                    
                    // Title "Остальное"
                    {
                        new CuiLabel
                        {
                            Text = { Text = "Остальное", FontSize = 18, Align = TextAnchor.MiddleCenter },
                            RectTransform = { AnchorMin = "0 0.8", AnchorMax = "1 1" },
                        },
                        $"{Layer}.profile.paneltwo.other",
                        $"{Layer}.profile.paneltwo.other.title"
                    },

                        #region Левый блок "Тайтлы"

                        // Левый блок "Тайтлы"
                        {
                            new CuiPanel
                            {
                                Image = { Color = "0 0 0 0",  /* Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat"*/ },
                                RectTransform = { AnchorMin = "0 0", AnchorMax = "0.6 0.75" }
                            },
                            $"{Layer}.profile.paneltwo.other",
                            $"{Layer}.profile.paneltwo.other.left"
                        },
                
                        // Выстрелов совершено
                        {
                            new CuiLabel
                            {
                                Text = { Text = "Выстрелов совершено:", FontSize = 15, Align = TextAnchor.MiddleLeft },
                                RectTransform = { AnchorMin = "0.1 0.81", AnchorMax = "1 0.99" },
                            },
                            $"{Layer}.profile.paneltwo.other.left"
                        },

                        // Взрывов сделано
                        {
                            new CuiLabel
                            {
                                Text = { Text = "Взрывов сделано:", FontSize = 15, Align = TextAnchor.MiddleLeft },
                                RectTransform = { AnchorMin = "0.1 0.61", AnchorMax = "1 0.79" },
                            },
                            $"{Layer}.profile.paneltwo.other.left"
                        },
                        
                        // Сбито вертолетов
                        {
                            new CuiLabel
                            {
                                Text = { Text = "Сбито вертолетов:", FontSize = 15, Align = TextAnchor.MiddleLeft },
                                RectTransform = { AnchorMin = "0.1 0.41", AnchorMax = "1 0.59" },
                            },
                            $"{Layer}.profile.paneltwo.other.left"
                        },

                        // Танков уничтожено
                        {
                            new CuiLabel
                            {
                                Text = { Text = "Танков уничтожено:", FontSize = 15, Align = TextAnchor.MiddleLeft },
                                RectTransform = { AnchorMin = "0.1 0.21", AnchorMax = "1 0.39" },
                            },
                            $"{Layer}.profile.paneltwo.other.left"
                        },

                        // NPC убито
                        {
                            new CuiLabel
                            {
                                Text = { Text = "NPC убито:", FontSize = 15, Align = TextAnchor.MiddleLeft },
                                RectTransform = { AnchorMin = "0.1 0.01", AnchorMax = "1 0.19" },
                            },
                            $"{Layer}.profile.paneltwo.other.left"
                        },

                        #endregion

                        #region Правый блок "Значения"

                            // Правый блок "Значения"
                            {
                                new CuiPanel
                                {
                                    Image = { Color = "0 0 0 0",  /* Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat"*/ },
                                    RectTransform = { AnchorMin = "0.63 0", AnchorMax = "0.996 0.75" }
                                },
                                $"{Layer}.profile.paneltwo.other",
                                $"{Layer}.profile.paneltwo.other.right"
                            },
                
                            // Выстрелов совершено
                            {
                                new CuiLabel
                                {
                                    Text = { Text =  $"{other["Shots"]}", FontSize = 15, Align = TextAnchor.MiddleRight },
                                    RectTransform = { AnchorMin = "0 0.81", AnchorMax = "0.8 0.99" },
                                },
                                $"{Layer}.profile.paneltwo.other.right"
                            },

                            // Взрывов сделано
                            {
                                new CuiLabel
                                {
                                    Text = { Text = $"{other["Explosions"]}", FontSize = 15, Align = TextAnchor.MiddleRight },
                                    RectTransform = { AnchorMin = "0 0.61", AnchorMax = "0.8 0.79" },
                                },
                                $"{Layer}.profile.paneltwo.other.right"
                            },

                            // Сбито вертолетов
                            {
                                new CuiLabel
                                {
                                    Text = { Text = $"{other["HeliCrashed"]}", FontSize = 15, Align = TextAnchor.MiddleRight },
                                    RectTransform = { AnchorMin = "0 0.41", AnchorMax = "0.8 0.59" },
                                },
                                $"{Layer}.profile.paneltwo.other.right"
                            },

                            // Танков уничтожено
                            {
                                new CuiLabel
                                {
                                    Text = { Text = $"{other["PanzerDestroyed"]}", FontSize = 15, Align = TextAnchor.MiddleRight },
                                    RectTransform = { AnchorMin = "0 0.21", AnchorMax = "0.8 0.39" },
                                },
                                $"{Layer}.profile.paneltwo.other.right"
                            },

                            // NPC убито
                            {
                                new CuiLabel
                                {
                                    Text = { Text = $"{other["NPCKilled"]}", FontSize = 15, Align = TextAnchor.MiddleRight },
                                    RectTransform = { AnchorMin = "0 0.01", AnchorMax = "0.8 0.19" },
                                },
                                $"{Layer}.profile.paneltwo.other.right"
                            },

                            #endregion

	                #endregion
                    
                    #region Время на сервере

                    // Блок "Время на сервере"
                    {
                        new CuiPanel
                        {
                            Image = { Color = MainColor,  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                            RectTransform = { AnchorMin = "0.05 0.02", AnchorMax = "0.95 0.11" }
                        },
                        $"{Layer}.profile.paneltwo",
                        $"{Layer}.profile.paneltwo.time"
                    },
                    
                    // Title "Вермя на сервере"
                    {
                        new CuiLabel
                        {
                            Text = { Text = $"Время на сервере:\n {time}", FontSize = 15, Align = TextAnchor.MiddleCenter },
                            RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
                        },
                        $"{Layer}.profile.paneltwo.time",
                        $"{Layer}.profile.paneltwo.time.title"
                    },

                #endregion

                #endregion

                #region Третья панель

                // Третья панель
                {
                    new CuiPanel
                    {
                        Image = { Color = "0 0 0 0.8",  /* Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat"*/ },
                        RectTransform = { AnchorMin = "0.54 0", AnchorMax = "0.997 1" }
                    },
                    Layer,
                    $"{Layer}.profile.panelthree"
                },
                
                #region Разное

                // Блок "Разное"
                {
                    new CuiPanel
                    {
                        Image = { Color = MainColor,  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat"},
                        RectTransform = { AnchorMin = "0 0.9", AnchorMax = "0.998 1" }
                    },
                    $"{Layer}.profile.panelthree",
                    $"{Layer}.profile.panelthree.name"
                },
                    
                // Title "Разное"
                {
                    new CuiLabel
                    {
                        Text = { Text = "Разное", FontSize = 18, Align = TextAnchor.MiddleCenter },
                        RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
                    },
                    $"{Layer}.profile.panelthree.name"
                },

                #endregion
                
                
                #region Занимаемый топ

                // Блок "Занимаемый топ"
                {
                    new CuiPanel
                    {
                        Image = { Color = "0 0 0 0",  /* Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat"*/ },
                        RectTransform = { AnchorMin = "0 0.5", AnchorMax = "0.997 0.87" }
                    },
                    $"{Layer}.profile.panelthree",
                    $"{Layer}.profile.panelthree.top"
                },

                // Title "Занимаемый топ"
                {
                    new CuiLabel
                    {
                        Text = { Text = "Занимаемый топ", FontSize = 18, Align = TextAnchor.MiddleCenter },
                        RectTransform = { AnchorMin = "0 0.8", AnchorMax = "1 1" },
                    },
                    $"{Layer}.profile.panelthree.top",
                    $"{Layer}.profile.panelthree.top.title"
                },

                    #region Левый блок "Тайтлы"
                
                    // Левый блок "Тайтлы"
                    {
                        new CuiPanel
                        {
                            Image = { Color = "0 0 0 0",  /* Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat"*/ },
                            RectTransform = { AnchorMin = "0 0", AnchorMax = "0.6 0.75" }
                        },
                        $"{Layer}.profile.panelthree.top",
                        $"{Layer}.profile.panelthree.top.left"
                    },
                
                    // Топ по добыче
                    {
                        new CuiLabel
                        {
                            Text = { Text = "Топ по добыче:", FontSize = 15, Align = TextAnchor.MiddleLeft },
                            RectTransform = { AnchorMin = "0.25 0.81", AnchorMax = "1 0.99" },
                        },
                        $"{Layer}.profile.panelthree.top.left"
                    },

                    // Топ по К/Д
                    {
                        new CuiLabel
                        {
                            Text = { Text = "Топ по К/Д:", FontSize = 15, Align = TextAnchor.MiddleLeft },
                            RectTransform = { AnchorMin = "0.25 0.61", AnchorMax = "1 0.79" },
                        },
                        $"{Layer}.profile.panelthree.top.left"
                    },

                    // Топ по взрывам
                    {
                        new CuiLabel
                        {
                            Text = { Text = "Топ по взрывам:", FontSize = 15, Align = TextAnchor.MiddleLeft },
                            RectTransform = { AnchorMin = "0.25 0.41", AnchorMax = "1 0.59" },
                        },
                        $"{Layer}.profile.panelthree.top.left"
                    },

                    // Топ по репутации
                    {
                        new CuiLabel
                        {
                            Text = { Text = "Топ по репутации:", FontSize = 15, Align = TextAnchor.MiddleLeft },
                            RectTransform = { AnchorMin = "0.25 0.21", AnchorMax = "1 0.39" },
                        },
                        $"{Layer}.profile.panelthree.top.left"
                    },

                    // Топ по радиоактивным домам
                    {
                        new CuiLabel
                        {
                            Text = { Text = "Топ по радиоактивным домам:", FontSize = 15, Align = TextAnchor.MiddleLeft },
                            RectTransform = { AnchorMin = "0.25 0.01", AnchorMax = "1 0.19" },
                        },
                        $"{Layer}.profile.panelthree.top.left"
                    },

                        #endregion

                    #region Правый блок "Значения"

                    // Правый блок "Значения"
                    {
                        new CuiPanel
                        {
                            Image = { Color = "0 0 0 0",  /* Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat"*/ },
                            RectTransform = { AnchorMin = "0.63 0", AnchorMax = "0.996 0.75" }
                        },
                        $"{Layer}.profile.panelthree.top",
                        $"{Layer}.profile.panelthree.top.right"
                    },
                
                    // Топ по добыче
                    {
                        new CuiLabel
                        {
                            Text = { Text =  $"{top["Gathering"]}", FontSize = 15, Align = TextAnchor.MiddleRight },
                            RectTransform = { AnchorMin = "0 0.81", AnchorMax = "0.6 0.99" },
                        },
                        $"{Layer}.profile.panelthree.top.right"
                    },

                    // Топ по К/Д
                    {
                        new CuiLabel
                        {
                            Text = { Text = $"{top["KD"]}", FontSize = 15, Align = TextAnchor.MiddleRight },
                            RectTransform = { AnchorMin = "0 0.61", AnchorMax = "0.6 0.79" },
                        },
                        $"{Layer}.profile.panelthree.top.right"
                    },

                    // Топ по взрывам
                    {
                        new CuiLabel
                        {
                            Text = { Text = $"{top["Explosion"]}", FontSize = 15, Align = TextAnchor.MiddleRight },
                            RectTransform = { AnchorMin = "0 0.41", AnchorMax = "0.6 0.59" },
                        },
                        $"{Layer}.profile.panelthree.top.right"
                    },

                    // Топ по репутации
                    {
                        new CuiLabel
                        {
                            Text = { Text = $"{top["Rep"]}", FontSize = 15, Align = TextAnchor.MiddleRight },
                            RectTransform = { AnchorMin = "0 0.21", AnchorMax = "0.6 0.39" },
                        },
                        $"{Layer}.profile.panelthree.top.right"
                    },

                    // Топ по радиоактивным домам
                    {
                        new CuiLabel
                        {
                            Text = { Text = $"{top["radHouse"]}", FontSize = 15, Align = TextAnchor.MiddleRight },
                            RectTransform = { AnchorMin = "0 0.01", AnchorMax = "0.6 0.19" },
                        },
                        $"{Layer}.profile.panelthree.top.right"
                    },

                    #endregion

	                #endregion

                #region Группы

                // Блок "Группы"
                {
                    new CuiPanel
                    {
                        Image = { Color = "0 0 0 0",  /* Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat"*/ },
                        RectTransform = { AnchorMin = "0.012 0.37", AnchorMax = "0.986 0.49" }
                    },
                    $"{Layer}.profile.panelthree",
                    $"{Layer}.profile.panelthree.groups"
                },
                // Тайтл "Группы'
                {
                    new CuiLabel
                    {
                        Text = { Text = "Группы", FontSize = 18, Align = TextAnchor.MiddleCenter },
                        RectTransform = { AnchorMin = "0 0.5", AnchorMax = "1 1" },
                    },
                    $"{Layer}.profile.panelthree.groups",
                    $"{Layer}.profile.panelthree.groups.title"
                },

                #endregion 
                
                #region Привилегии

                // Блок "Привелегии"
                {
                    new CuiPanel
                    {
                        Image = { Color = "0 0 0 0",  /* Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat"*/ },
                        RectTransform = { AnchorMin = "0.012 0.22", AnchorMax = "0.986 0.35" }
                    },
                    $"{Layer}.profile.panelthree",
                    $"{Layer}.profile.panelthree.privileges"
                },
                // Тайтл "Привилегии'
                {
                    new CuiLabel
                    {
                        Text = { Text = "Привилегии", FontSize = 18, Align = TextAnchor.MiddleCenter },
                        RectTransform = { AnchorMin = "0 0.5", AnchorMax = "1 1" },
                    },
                    $"{Layer}.profile.panelthree.privileges",
                    $"{Layer}.profile.panelthree.privileges.title"
                },

                #endregion 

                #region Опции

                // Блок "Опции"
                {
                    new CuiPanel
                    {
                        Image = { Color = "0 0 0 0",  /* Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat"*/ },
                        RectTransform = { AnchorMin = "0.014 0.015", AnchorMax = "0.98 0.16" }
                    },
                    $"{Layer}.profile.panelthree",
                    $"{Layer}.profile.panelthree.options"
                },
                
                #region Отправить сообщение

                // Блок "Отправить сообщение"
                {
                    new CuiPanel
                    {
                        Image = { Color = "0 0 0 0.4",  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                        RectTransform = { AnchorMin = "0.01 0.52", AnchorMax = "0.495 0.97" }
                    },
                    $"{Layer}.profile.panelthree.options",
                    $"{Layer}.profile.panelthree.options.ls"
                },
                {
                    new CuiButton
                    {
                        Text = { Text = "Отправить сообщение", FontSize = 16, Align = TextAnchor.MiddleCenter },
                        Button = { Command = $"playerprofile.sendmessage {player.UserIDString} {target.UserIDString}", Color = MainColor,  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                        RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" }
                    },
                    $"{Layer}.profile.panelthree.options.ls",
                    $"{Layer}.profile.panelthree.options.ls.button"
                },

                #endregion

                #region Напугать

                // Блок "Напугать"
                {
                    new CuiPanel
                    {
                        Image = { Color = "0 0 0 0.4",  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                        RectTransform = { AnchorMin = "0.505 0.52", AnchorMax = "0.99 0.97" }
                    },
                    $"{Layer}.profile.panelthree.options",
                    $"{Layer}.profile.panelthree.options.fear"
                },
                {
                    new CuiButton
                    {
                        Text = { Text = "Напугать", FontSize = 16, Align = TextAnchor.MiddleCenter },
                        Button = { Command = $"playerprofile.fear {player.UserIDString} {target.UserIDString}", Color = MainColor,  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                        RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" }
                    },
                    $"{Layer}.profile.panelthree.options.fear",
                    $"{Layer}.profile.panelthree.options.fear.button"
                },

                #endregion

                #region Добавить в друзья

                // Блок "Добавить в друзья"
                {
                    new CuiPanel
                    {
                        Image = { Color = "0 0 0 0.4",  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                        RectTransform = { AnchorMin = "0.01 0.03", AnchorMax = "0.495 0.47" }
                    },
                    $"{Layer}.profile.panelthree.options",
                    $"{Layer}.profile.panelthree.options.friend"
                },
                {
                    new CuiButton
                    {
                        Text = { Text = "Добавить в друзья", FontSize = 16, Align = TextAnchor.MiddleCenter },
                        Button = { Command = $"playerprofile.addfriend {player.UserIDString} {target.UserIDString}", Color = MainColor,  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                        RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" }
                    },
                    $"{Layer}.profile.panelthree.options.friend",
                    $"{Layer}.profile.panelthree.options.friend.button"
                },

                #endregion

                #region Отправить трейд

                // Блок "Отправить трейд"
                {
                    new CuiPanel
                    {
                        Image = { Color = "0 0 0 0.4",  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                        RectTransform = { AnchorMin = "0.505 0.03", AnchorMax = "0.99 0.47" }
                    },
                    $"{Layer}.profile.panelthree.options",
                    $"{Layer}.profile.panelthree.options.trade"
                },
                {
                    new CuiButton
                    {
                        Text = { Text = "Отправить трейд", FontSize = 16, Align = TextAnchor.MiddleCenter },
                        Button = { Command = $"playerprofile.trade {player.UserIDString} {target.UserIDString}", Color = MainColor,  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                        RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" }
                    },
                    $"{Layer}.profile.panelthree.options.trade",
                    $"{Layer}.profile.panelthree.options.trade.button"
                },

                #endregion

                #endregion

                #endregion
            };

            #region Отрисовка Баннера

            if (BannerSystem != null)
            {
                List<CuiElement> ban;
                ban = (CuiElementContainer)BannerSystem.Call("DrawGUIInCustomContainer", target, $"{Layer}.profile.panelone.banner");


                foreach (CuiElement el in ban)
                {
                    MainContainer.Add(el);
                }
            }

            #endregion

            #region Отрисовка Групп
            var enabledColor = "1 1 1 0.3";
            int groupCounter = 0;

            int entireGroupsCounter = allGroups.Count();

            foreach (var group in allGroups)
            {
                enabledColor = "0 0 0 0.75";
                if (groupsMember.Contains(group)) enabledColor = MainColor;
                MainContainer.Add(
                    new CuiPanel
                    {
                        Image = { Color = $"{enabledColor}",  /* Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" */},
                        RectTransform = {
                        AnchorMin = $"" +
                        $"{0.015 + (groupCounter % entireGroupsCounter)* (1.0 / entireGroupsCounter - 0.005)} " +
                        $"{0.015}",
                        AnchorMax =
                        $"{(1.0 / entireGroupsCounter - 0.005) + (groupCounter % entireGroupsCounter) * (1.0 / entireGroupsCounter - 0.005)} " +
                        $"{0.45}" }
                    },
                    $"{Layer}.profile.panelthree.groups",
                    $"{Layer}.profile.panelthree.groups.{groupCounter}"
                );

                MainContainer.Add(
                    new CuiLabel
                    {
                        Text = { Text = group, FontSize = 10, Align = TextAnchor.MiddleCenter },
                        RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
                    },
                    $"{Layer}.profile.panelthree.groups.{groupCounter}"
                );
                groupCounter += 1;
            }

            #endregion

            #region Отрисовка Привилегий

            int privilegiesCounter = 0;

            int entirePrivilegiesCounter = allPrivilegies.Count();

            foreach (var privilege in allPrivilegies)
            {
                enabledColor = "0 0 0 0.75";
                if (privilegiesMember.Contains(privilege)) enabledColor = MainColor;
                MainContainer.Add(
                    new CuiPanel
                    {
                        Image = { Color = $"{enabledColor}",/*  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" */},
                        RectTransform = {
                        AnchorMin = $"" +
                        $"{0.015 + (privilegiesCounter % entirePrivilegiesCounter)* (1.0 / entirePrivilegiesCounter - 0.005)} " +
                        $"{0.015}",
                        AnchorMax =
                        $"{(1.0 / entirePrivilegiesCounter - 0.005) + (privilegiesCounter % entirePrivilegiesCounter) * (1.0 / entirePrivilegiesCounter - 0.005)} " +
                        $"{0.45}" }
                    },
                    $"{Layer}.profile.panelthree.privileges",
                    $"{Layer}.profile.panelthree.privileges.{privilegiesCounter}"
                );

                MainContainer.Add(
                    new CuiLabel
                    {
                        Text = { Text = privilege, FontSize = 10, Align = TextAnchor.MiddleCenter },
                        RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
                    },
                    $"{Layer}.profile.panelthree.privileges.{privilegiesCounter}"
                );
                privilegiesCounter += 1;
            }

            #endregion

            return MainContainer;


        }

        [ConsoleCommand("playerprofile.open.reputaion.profile")]
        private void CmdOpenReputaionProfile(ConsoleSystem.Arg args)
        {
            BasePlayer player = args.Player();
            if (player == null)
            {
                return;
            }

            BasePlayer target = FindBasePlayer(args.GetString(0));

            string time = "";

            if (StoreBonus != null)
            {
                time = (string)StoreBonus.Call("GetPlayerTimeOnServer", target);
            }

            if (Rep == null)
            {
                CuiHelper.DestroyUi(player, $"{Layer}.profile.panelone.rep.button");
                CuiHelper.AddUi(player, new CuiElementContainer {
                {
                    new CuiButton
                    {
                        Text = { Text = "Недоступно", FontSize = 20, Align = TextAnchor.MiddleCenter },
                        Button = { Command = $"playerprofile.open.reputaion.profile {target.UserIDString}", Color = SecondColor,  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                        RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" }
                    },
                    $"{Layer}.profile.panelone.rep",
                    $"{Layer}.profile.panelone.rep.button"
                }});
            }
            else
            {
                CloseProfile(player);
                player.SendConsoleCommand($"repgui.open.profile 1 {target.UserIDString}");
            }
        }
        private CuiElement CreateImage(string panelName, string url)
        {
            var element = new CuiElement();
            var image = new CuiRawImageComponent
            {
                Url = $"{url}",
                Color = "1 1 1 1",
                FadeIn = 1f
            };

            var rectTransform = new CuiRectTransformComponent
            {
                AnchorMin = "0.18 0",
                AnchorMax = "0.82 1"
            };
            element.Components.Add(image);
            element.Components.Add(rectTransform);
            element.Name = CuiHelper.GetGuid();
            element.Parent = panelName;

            return element;
        }

        #endregion

        [ConsoleCommand("playerprofile.sendmessage")]
        private void CmdSendMessage(ConsoleSystem.Arg args)
        {
            BasePlayer player = FindBasePlayer(args.GetString(0));
            BasePlayer target = FindBasePlayer(args.GetString(1));
            if (player.UserIDString == target.UserIDString)
            {
                CuiHelper.DestroyUi(player, $"{Layer}.profile.panelthree.options.sendmessage.ls");
                CuiHelper.AddUi(player, new CuiElementContainer
                {
                    {
                        new CuiButton
                        {
                            Text = { Text = "Недоступно", FontSize = 20, Align = TextAnchor.MiddleCenter },
                            Button = { Command = $"playerprofile.sendmessage {player.UserIDString} {target.UserIDString}", Color = SecondColor, Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                            RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" }
                        },
                        $"{Layer}.profile.panelthree.options.ls",
                        $"{Layer}.profile.panelthree.options.ls.button"
                    }
                });
            }
            else
            {
                player.SendConsoleCommand($"chat.say \"/pm {target.UserIDString} Привет\"");
            }
        }

        [ConsoleCommand("playerprofile.trade")]
        private void CmdTrade(ConsoleSystem.Arg args)
        {
            BasePlayer player = FindBasePlayer(args.GetString(0));
            BasePlayer target = FindBasePlayer(args.GetString(1));
            if (player.UserIDString == target.UserIDString)
            {
                CuiHelper.DestroyUi(player, $"{Layer}.profile.panelthree.options.trade.button");
                CuiHelper.AddUi(player, new CuiElementContainer
                {
                    {
                        new CuiButton
                        {
                            Text = { Text = "Недоступно", FontSize = 20, Align = TextAnchor.MiddleCenter },
                            Button = { Command = $"playerprofile.trade {player.UserIDString} {target.UserIDString}", Color = SecondColor, Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                            RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" }
                        },
                        $"{Layer}.profile.panelthree.options.trade",
                        $"{Layer}.profile.panelthree.options.trade.button"
                    }
                });
            }
            else
            {
                player.SendConsoleCommand($"chat.say \"/trade {target.UserIDString}\"");
            }
        }

        [ConsoleCommand("playerprofile.addfriend")]
        private void CmdAddFriend(ConsoleSystem.Arg args)
        {
            BasePlayer player = FindBasePlayer(args.GetString(0));
            BasePlayer target = FindBasePlayer(args.GetString(1));
            if (player.UserIDString == target.UserIDString)
            {
                CuiHelper.DestroyUi(player, $"{Layer}.profile.panelthree.options.friend.button");
                CuiHelper.AddUi(player, new CuiElementContainer
                {
                    {
                        new CuiButton
                        {
                            Text = { Text = "Недоступно", FontSize = 20, Align = TextAnchor.MiddleCenter },
                            Button = { Command = $"playerprofile.addfriend {player.UserIDString} {target.UserIDString}", Color = SecondColor, Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                            RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" }
                        },
                        $"{Layer}.profile.panelthree.options.friend",
                        $"{Layer}.profile.panelthree.options.friend.button"
                    }
                });
            }
            else
            {
                player.SendConsoleCommand($"chat.say \"/addfriend {target.UserIDString}\"");
            }

        }

        [ConsoleCommand("playerprofile.fear")]
        private void CmdFear(ConsoleSystem.Arg args)
        {
            BasePlayer player = FindBasePlayer(args.GetString(0));
            BasePlayer target = FindBasePlayer(args.GetString(1));
            //// if (player.UserIDString == target.UserIDString)
            //if (true)
            //{
            CuiHelper.DestroyUi(player, $"{Layer}.profile.panelthree.options.fear.button");
            CuiHelper.AddUi(player, new CuiElementContainer
            {
                {
                    new CuiButton
                    {
                        Text = { Text = "Недоступно", FontSize = 20, Align = TextAnchor.MiddleCenter },
                        Button = { Command = $"playerprofile.fear {player.UserIDString} {target.UserIDString}", Color = SecondColor, Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                        RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" }
                    },
                    $"{Layer}.profile.panelthree.options.fear",
                    $"{Layer}.profile.panelthree.options.fear.button"
                }
            });
            //}
            //else
            //{
            //    if (target != null)
            //    {
            //        Effect.server.Run("assets/prefabs/misc/easter/painted eggs/effects/gold_open.prefab", target, 30, Vector3.zero, Vector3.forward);
            //var count = (from x in Tops where x.UID == target.UserIDString select x).Count();
            //var pl = (from x in Tops where x.UID == target.UserIDString select x).FirstOrDefault();
            //if (count == 0) CreateInfo(player);
            //if (pl.Chains.Contains(target.UserIDString))
            //{
            //    SendReply(player, "Низя");
            //    return;
            //}
            //else
            //{
            //    Effect.server.Run("assets/prefabs/misc/easter/painted eggs/effects/gold_open.prefab", target, 30, Vector3.zero, Vector3.forward);
            //    pl.Chains.Add(target.UserIDString);
            //    Saved();
            //    SendReply(player, "Ok");
            //}
            //}
            //else
            //{
            //    SendReply(player, "Игрок не найден или не в сети");
            //}
            //}
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
    }
}
