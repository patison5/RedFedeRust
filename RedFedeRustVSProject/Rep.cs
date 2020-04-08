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
    [Info("Rep", "Beorn", "1.0.0")]
    class Rep : RustPlugin
    {
        [PluginReference]
        private Plugin TopCustom, StoreBonus;

        private int Duration = 60;

        private int AmountOfPlayersOnPage = 9;

        private const string permSetRep = "rep.setrep";
        public string MainColor { get; } = "0.2 0.5 0.39 1";
        public string SecondColor { get; } = "0.5 0.2 0.39 1";
        public string RedColor { get; } = "0.7 0.2 0.29 1";
        public string GreyColor { get; } = "0.35 0.35 0.35 1";
        void Init()
        {
            permission.RegisterPermission(permSetRep, this);
        }

        void Loaded()
        {
            Tops = Interface.Oxide.DataFileSystem.ReadObject<List<RepData>>("RepData");
            if (TopCustom != null)
            foreach (var item in Tops)
            {
                    TopCustom.Call("PutRepInTop", item.UID, GetRep(item.Rep));
            }
            Puts("Плагин репутации загружен.");
        }

        public ListHashSet<BasePlayer> AllPlayers { get; set; }
        public CuiElementContainer Elements { get; set; }

        #region Получение Данных из RepData

        public string GetRepByPlayerWithColor(BasePlayer player)
        {
            var isExists = (from x in Tops where x.UID == player.UserIDString select x).ToList().Count;
            if (isExists.ToString() != "0")
            {
                var rep = (from x in Tops where x.UID == player.UserIDString select x).FirstOrDefault().Rep;
                return GetRepWithColor(rep);
            }
            else
                return $"<color=#989898>0</color>";
        }

        private string GetCategory(Dictionary<string, Dictionary<string, int>> rep, string playerName)
        {
            return (from x in rep where x.Key == playerName select x).FirstOrDefault().Value.FirstOrDefault().Key;
        }

        private int GetRep(Dictionary<string, Dictionary<string, int>> rep)
        {
            var sum = 0;
            foreach (var a in rep)
            {
                foreach (var c in a.Value)
                sum += c.Value;
            }
            return sum;
        }
        private string GetRepPos(Dictionary<string, Dictionary<string, int>> rep)
        {
            var pos = 0;
            foreach (var a in rep)
            {
                foreach (var c in a.Value)
                {
                    if (c.Value > 0)
                    {
                        pos += 1;
                    }
                }
            }
            return pos.ToString();
        }
        private string GetRepNeg(Dictionary<string, Dictionary<string, int>> rep)
        {
            var neg = 0;
            foreach (var a in rep)
            {
                foreach (var c in a.Value)
                {
                    if (c.Value < 0)
                    {
                        neg += 1;
                    }
                }
            }
            return neg.ToString();
        }
        private string GetRepByPlayerPos(BasePlayer player)
        {
            var isExists = (from x in Tops where x.UID == player.UserIDString select x).ToList().Count;
            if (isExists.ToString() != "0")
            {
                var rep = (from x in Tops where x.UID == player.UserIDString select x).FirstOrDefault().Rep;
                return GetRepPos(rep);
            }
            else
                return "0";
        }
        private string GetRepByPlayerNeg(BasePlayer player)
        {
            var isExists = (from x in Tops where x.UID == player.UserIDString select x).ToList().Count;
            if (isExists.ToString() != "0")
            {
                var rep = (from x in Tops where x.UID == player.UserIDString select x).FirstOrDefault().Rep;
                return GetRepNeg(rep);
            }
            else
                return "0";
        }
        private int GetRepByPlayer(BasePlayer player)
        {
            var isExists = (from x in Tops where x.UID == player.UserIDString select x).ToList().Count;
            if (isExists.ToString() != "0")
            {
                var rep = (from x in Tops where x.UID == player.UserIDString select x).FirstOrDefault().Rep;
                return GetRep(rep);
            }
            else
                return 0;
        }
        private string GetRepTop(BasePlayer player)
        {
            var isExists = (from x in Tops where x.UID == player.UserIDString select x).ToList().Count;
            if (isExists.ToString() == "0")
            {
                CreateInfo(player);
            }
            var rep = (from x in Tops where x.UID == player.UserIDString select x).FirstOrDefault().Rep;
            var val = Tops.OrderByDescending(x => GetRep(x.Rep));

            int a = 1;

            foreach (var item in val)
            {
                if (item.UID == player.UserIDString) return $"{a}    из    {val.Count()}";
                a++;
            }

            return "N/A";

        }

        private string GetRepWithColor(Dictionary<string, Dictionary<string, int>> rep)
        {
            var sum = GetRep(rep);
            if (sum < 0)
                return $"<color=#fd584b>{sum}</color>";
            if (sum == 0)
                return $"<color=#989898>{sum}</color>";
            if (sum > 0)
                return $"<color=#43b67f>{sum}</color>";
            return sum.ToString();
        }


        private string GetPaintedTag(string name, string color)
        {
            var newName = $"{name.Substring(0, name.Length > 20 ? 20 : name.Length)}";
            if (name.StartsWith("["))
            {
                int closeTagIndex = name.IndexOf("]");
                var tag = newName.Substring(0, closeTagIndex + 1);
                tag = $"<color={color}>{tag}</color>";
                var oname = newName.Substring(closeTagIndex + 1);
                newName = tag + oname;
            }
            return newName;
        }

        #endregion

        #region Махинации с RepData

        void CreateInfo(BasePlayer player)
        {
            if (player == null) return;
            Tops.Add(new RepData(player.displayName, player.UserIDString));
            Saved();
        }

        void Saved()
        {
            Interface.Oxide.DataFileSystem.WriteObject("RepData", Tops);
        }
        public List<RepData> Tops = new List<RepData>();
        public class RepData
        {
            public string Nickname { get; set; }
            public string UID { get; set; }
            public Dictionary<string, Dictionary<string, int>> Rep { get; set; }
            public DateTime LastTimeChecked { get; set; }

            public RepData(string Nickname, string UID)
            {
                this.Nickname = Nickname;
                this.UID = UID;
                this.Rep = new Dictionary<string, Dictionary<string, int>> { };
                this.LastTimeChecked = DateTime.Now.AddHours(-2.1);
            }
            public void Reset()
            {
                this.Rep = new Dictionary<string, Dictionary<string, int>> { };
                this.LastTimeChecked = DateTime.Now.AddHours(-2.1);
            }
        }
        #endregion

        #region Основная Команда Rep

        [ChatCommand("rep")]
        private void GetRepMenu(BasePlayer player, string command, string[] args)
        {
            CuiHelper.DestroyUi(player, "Panel");
            if (args.Length == 0)
            {
                CuiElementContainer container;
                container = CreatePanel(player);
                CuiHelper.AddUi(player, container);

                player.SendConsoleCommand($"rep.listpages");
                player.SendConsoleCommand($"rep.page 1");
                return;
            }

            if ((args.Length == 1) && (args[0] == "help"))
            {
                SendReply(player, "<color=#3999D5>/rep [ like / dis / normal ] [ NAME ]</color>");
                SendReply(player, "<color=#3999D5>like</color> - увеличивает репутацию игрока");
                SendReply(player, "<color=#3999D5>dis</color> - понижает репутацию игрока");
                SendReply(player, "<color=#3999D5>normal</color> - сохраняет нейтралитет в отношении игрока");
                SendReply(player, "Помните! Ваша репутация влияет на Рэйты в добыче ресурсов, " +
                                  "а также на отношение других игроков к вам. Будьте справедливы. " +
                                  "Также репутацию игрока можно увидеть в чате после его сообщения");
                return;
            }

            if ((args.Length == 2) && (args[0] == "get"))
            {
                BasePlayer playerRequested = FindBasePlayer(args[1]);
                if (playerRequested != null)
                    SendReply(player, $"Карма {playerRequested.displayName}: {GetRepByPlayerWithColor(playerRequested)}");
                else SendReply(player, "Такого игрока не существует");

                return;
            }

            if ((args.Length == 1) && (args[0] == "get"))
            {
                SendReply(player, $"Ваша карма: {GetRepByPlayerWithColor(player)}");

                SendReply(player, "Напишите <color=#3999D5>/rep help</color> - для получения подробной информации о данной команды");
                return;
            }

            if (args.Length != 2)
            {
                SendReply(player, "Arguments Error");
                return;
            }
        }

        #endregion

        #region Выставление Репутации

        [ConsoleCommand("rep.setrep")]
        void SetRepAdmin(ConsoleSystem.Arg arg)
        {
            if (arg.Player() == null)
                return;
            BasePlayer player = arg.Player();
            if (!player.IPlayer.HasPermission(permSetRep))
            {
                SendReply(player, "У вас нет прав на выполнение этой команды");
                return;
            }
            if (arg.Args.Count() != 2)
            {
                SendReply(player, "rep.setrep [ИМЯ] [Кол-во очков]");
                return;
            }

            var name = arg.GetString(0);
            var repAmount = arg.GetInt(1);

            var target = FindBasePlayer(name);
            if (target == null)
            {
                SendReply(player, $"Игрок {name} не найден");
                return;
            }



            var TargetInstance = (from x in Tops where x.UID == target.UserIDString select x).FirstOrDefault();
            var res = TargetInstance.Rep.ContainsKey(player.UserIDString);
            var colorDecision = "";
            var checkedWord = "";
            var responseWord = "";

            if (repAmount > 0)
            {
                colorDecision = "<color=#43b67f>";
                checkedWord = "положительную";
                responseWord = "увеличена";
            }
            if (repAmount == 0)
            {
                colorDecision = "<color=#fd584b>";
                checkedWord = "отрицательную";
                responseWord = "понижена";
            }
            if (repAmount < 0)
            {
                colorDecision = "<color=#989898>";
                checkedWord = "нейтральную";
                responseWord = "изменена";
            }

            if (res)
            {
                TargetInstance.Rep["Federuster"] = new Dictionary<string, int>() { { "Так решил Федерастер", repAmount } };
                SendReply(player, $"Вы изменили свое мнение о {colorDecision}{target.displayName}</color> в {checkedWord} сторону. Причина \"Так решил Федерастер\"");

            }
            else
            {
                TargetInstance.Rep.Add("Federuster", new Dictionary<string, int>() { { "Так решил Федерастер", repAmount } });
                SendReply(player, $"Вы откликнулись о {colorDecision}{target.displayName}</color> в {colorDecision}{checkedWord}</color> сторону. Причина \"Так решил Федерастер\"");
            }
            SendReply(target, $"Ваша карма была {responseWord} {colorDecision}Federuster</color> и составляет - {GetRepWithColor(TargetInstance.Rep)}. Причина \"Так решил Федерастер\"");
            SendReply(player, $"Карма {colorDecision}{target.displayName}</color> {responseWord} и составляет {GetRepWithColor(TargetInstance.Rep)}");
            if (TopCustom != null)
                TopCustom.Call("PutRepInTop", target, GetRep(TargetInstance.Rep));
            Effect.server.Run("assets/prefabs/misc/easter/painted eggs/effects/gold_open.prefab", target, 30, Vector3.zero, Vector3.forward);

            Saved();
        }

        [ConsoleCommand("rep.set")]
        private void SetRepCommand(ConsoleSystem.Arg arg)
        {
            if (arg.Player() == null)
                return;
            BasePlayer player = arg.Player();
            var timeFromFirstLogin = (TimeSpan)StoreBonus.Call("GetPlayerTimeOnServerInSeconds", player);
            if (timeFromFirstLogin.TotalSeconds <= 7200.0)
            {
                SendReply(player, $"Вы не можете оценивать других игроков в первые 2 часа игры");
                SendReply(player, $"Осталось времени: {FormatTime(new TimeSpan(2, 0, 0)- timeFromFirstLogin)}");
                return;
            }

            var name = "";
            var text = arg.GetString(2);
            var decision = arg.GetString(0);
            var ind = arg.GetInt(1);

            var a = 0;
            foreach (var item in arg.Args)
            {

                if (a >= 3)
                {
                    name = name + item + " ";
                }
                a += 1;
            }


            name = name.Substring(0, name.Length - 1);
            var target = FindBasePlayer(name);
            if (target != null)
            {
                var check = (from x in Tops where x.UID == target.UserIDString select x).Count();
                if (check == 0)
                {
                    CreateInfo(target);
                }

                check = (from x in Tops where x.UID == player.UserIDString select x).Count();
                if (check == 0)
                {
                    CreateInfo(player);
                }

                var TargetInstance = (from x in Tops where x.UID == target.UserIDString select x).FirstOrDefault();
                var res = TargetInstance.Rep.ContainsKey(player.UserIDString);

                var PlayerInstance = (from x in Tops where x.UID == player.UserIDString select x).FirstOrDefault();
                TimeSpan duration = DateTime.Now - PlayerInstance.LastTimeChecked;


                var st = "";
                var time = Math.Round(this.Duration - duration.TotalSeconds);
                if (time % 10 == 1) st = "а";
                if ((time % 10 == 2) || (time % 10 == 3) || (time % 10 == 4)) st = "ы";

                if (duration.TotalSeconds <= this.Duration)
                {
                    SendReply(player, $"Время до повторной возможности оценить игрока {target.displayName}: {Math.Round(this.Duration - duration.TotalSeconds)} секунд{st}");
                    return;
                }
                var repDecision = 2;
                var colorDecision = "";
                var checkedWord = "";
                var responseWord = "";

                var ifNegativePN = " " + player.displayName;

                if (decision == "like")
                {
                    repDecision = 1;
                    colorDecision = "<color=#43b67f>";
                    checkedWord = "положительную";
                    responseWord = "увеличена";
                }
                if (decision == "dis")
                {
                    repDecision = -1;
                    colorDecision = "<color=#fd584b>";
                    checkedWord = "отрицательную";
                    responseWord = "понижена";
                    ifNegativePN = "";
                }
                if (decision == "normal")
                {
                    repDecision = 0;
                    colorDecision = "<color=#989898>";
                    checkedWord = "нейтральную";
                    responseWord = "изменена";
                }

                if (repDecision != 2)
                {
                    PlayerInstance.LastTimeChecked = DateTime.Now;
                    if (res)
                    {
                        TargetInstance.Rep[player.UserIDString] = new Dictionary<string, int>() { { text, repDecision } };
                        SendReply(player, $"Вы изменили свое мнение о {colorDecision}{target.displayName}</color> в {checkedWord} сторону. Причина \"{text}\"");

                    }
                    else
                    {
                        TargetInstance.Rep.Add(player.UserIDString, new Dictionary<string, int>() { { text, repDecision } });
                        SendReply(player, $"Вы откликнулись о {colorDecision}{target.displayName}</color> в {colorDecision}{checkedWord}</color> сторону. Причина \"{text}\"");
                    } // 
                    SendReply(target, $"Ваша карма была {responseWord}{colorDecision}{ifNegativePN}</color> и составляет - {GetRepWithColor(TargetInstance.Rep)}. Причина \"{text}\"");
                    SendReply(player, $"Карма {colorDecision}{target.displayName}</color> {responseWord} и составляет {GetRepWithColor(TargetInstance.Rep)}");
                    if (TopCustom != null)
                        TopCustom.Call("PutRepInTop", target, GetRep(TargetInstance.Rep));
                    Effect.server.Run("assets/prefabs/misc/easter/painted eggs/effects/gold_open.prefab", target, 30, Vector3.zero, Vector3.forward);
                }
                else
                {
                    SendReply(player, "Неправильная оценка");
                }


                Saved();

                CuiHelper.DestroyUi(player, $"TargetInstance.RepIndex{ind}");
                CuiHelper.AddUi(player, new CuiElementContainer
                {
                    {
                        new CuiLabel
                        {
                            Text = { Text = GetRepByPlayerWithColor(target), FontSize = 20, Align = TextAnchor.MiddleLeft, Color = "1 1 1 1" },
                            RectTransform = { AnchorMin = "0.3 0", AnchorMax = "0.4 1" },
                        },
                        $"PlayerPanel{ind}",
                        $"TargetInstance.RepIndex{ind}"
                    }
                });

            }
            else
            {
                SendReply(player, "Игрок не найден. Ошибка");
            }

        }

        #endregion

        #region Панель Репутации

        [ConsoleCommand("repgui.close.category")]
        private void ClosePanelCategory(ConsoleSystem.Arg arg)
        {
            if (arg.Player() == null)
                return;
            BasePlayer player = arg.Player();
            var index = arg.GetString(0);
            var name = arg.GetString(1);

            CuiHelper.DestroyUi(player, $"PlayerPanelCategory{index}");

            CuiHelper.AddUi(player, new CuiElementContainer {
            {
                new CuiPanel
                {
                    Image = { Color = "0 0 0 0" },
                    RectTransform = { AnchorMin = "0.762 0.066", AnchorMax = "0.996 0.945" },
                    CursorEnabled = true
                },
                    $"PlayerPanel{index}",
                    $"PlayerPanelDecision{index}"
                },
                {
                new CuiButton
                {
                    Button = { Command = $"repgui.createpanel.category like {index} {name}", Color = MainColor,  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                    Text = { Text = ":)", FontSize = 20, Align = TextAnchor.MiddleCenter },
                    RectTransform = { AnchorMin = "0 0", AnchorMax = "0.318 0.93" }
                },
                    $"PlayerPanelDecision{index}"
                },
                {
                new CuiButton
                {
                    Text = { Text = "-_-", FontSize = 20, Align = TextAnchor.MiddleCenter },
                    Button = { Command = $"rep.set normal {index} normal {name}", Color = "0.35 0.35 0.35 1",  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                    RectTransform = { AnchorMin = "0.336 0", AnchorMax = "0.651 0.93" }
                },
                    $"PlayerPanelDecision{index}"
                },
                {
                new CuiButton
                {
                    Text = { Text = ":(", FontSize = 20, Align = TextAnchor.MiddleCenter },
                    Button = { Command = $"repgui.createpanel.category dis {index} {name}", Color = RedColor,  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                    RectTransform = { AnchorMin = "0.669 0", AnchorMax = "0.995 0.93" }
                },
                    $"PlayerPanelDecision{index}"
                }
            });
        }

        [ConsoleCommand("repgui.createpanel.category")]
        private void CreatePanelCategory(ConsoleSystem.Arg arg)
        {
            if (arg.Player() == null)
                return;
            BasePlayer player = arg.Player();

            var name = arg.GetString(2);

            var decision = arg.GetString(0);
            var index = arg.GetString(1);

            CuiHelper.DestroyUi(player, $"PlayerPanelDecision{index}");

            CuiHelper.AddUi(player, new CuiElementContainer {
                {
                    new CuiPanel
                    {
                        Image = { Color = "0 0 0 0" },
                        RectTransform = { AnchorMin = "0.604 0.066", AnchorMax = "0.996 0.945" },
                        CursorEnabled = true
                    },
                    $"PlayerPanel{index}",
                    $"PlayerPanelCategory{index}"
                }
            });
            var text = "";
            var color = "";
            if (decision == "like") { color = MainColor; } else { color = RedColor; }
            for (int i = 0; i < 5; i++)
            {

                if (i == 0) { if (decision == "like") { text = "Адекватный"; } else { text = "Неадекватный"; } }
                if (i == 1) { if (decision == "like") { text = "Помощь"; } else { text = "Вред"; } }
                if (i == 2) { if (decision == "like") { text = "Анти-Рейд"; } else { text = "Рейд"; } }
                if (i == 3) { if (decision == "like") { text = "Честность"; } else { text = "Обман"; } }
                var command = $"rep.set {decision} {index} {text} {name}";
                if (i == 4) { text = "Закрыть"; color = "0.35 0.35 0.35 1"; command = $"repgui.close.category {index} {name}"; }
                CuiHelper.AddUi(player, new CuiElementContainer {
                {
                    new CuiButton
                    {
                        Button = { Command = command, Color = $"{color}" , Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat"},
                        Text = { Text = $"{text}", FontSize = 13, Align = TextAnchor.MiddleCenter },
                        RectTransform = { AnchorMin = $"{0.005 + i*0.2} 0", AnchorMax = $"{0.200 + i*0.2 - 0.005} 0.95" }
                    },
                    $"PlayerPanelCategory{index}",
                    $"PlayerPanelCategoryButton{index}"
                }
            });

            }
        }



        private CuiElementContainer CreatePanel(BasePlayer player)
        {
            return new CuiElementContainer
            {
                {
                    new CuiPanel
                    {
                        Image = { Color = "0 0 0 0.8" /* , Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" */ },
                        //FadeOut = 0.4f,
                        RectTransform = { AnchorMin = "0.06 0.15", AnchorMax = "0.94 0.93" },
                        CursorEnabled = true
                    },
                    "Overlay",
                    "Panel"
                },
                {
                    new CuiButton
                    {
                        Text = { Text = "", FontSize = 20, Align = TextAnchor.MiddleCenter },
                        Button = { Command = $"repgui.exit", Color = "0 0 0 0" },
                        RectTransform = { AnchorMin = "-100 -100", AnchorMax = "100 100" }
                    },
                    "Panel"
                },
                {
                    new CuiPanel
                    {
                        Image = { Color = MainColor, Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat"},
                        RectTransform = { AnchorMin = "0 0.905", AnchorMax = "0.999 1" }
                    },
                    "Panel",
                    "Panel.label"
                },
                {
                    new CuiLabel
                    {
                        Text = { Text = "<color=#fff>Репутация игроков</color>", FontSize = 25, Align = TextAnchor.MiddleLeft },
                        RectTransform = { AnchorMin = "0.035 0", AnchorMax = "0.244 1" },
                    },
                    "Panel.label"
                },
                //{
                //    new CuiPanel
                //    {
                //        Image = { Color = "0 0 0 0.70" },
                //        RectTransform = { AnchorMin = "0.25 0.905", AnchorMax = "0.979 0.97" },
                //        CursorEnabled = true
                //    },
                //    "Panel",
                //    "ButtonPanel"
                //},
                //{
                //    new CuiButton
                //    {
                //        Text = { Text = "<color=#898989>Закрыть</color>", FontSize = 25, Align = TextAnchor.MiddleRight },
                //        Button = { Command = "repgui.exit", Color = "0 0 0 0" },
                //        RectTransform = { AnchorMin = "0 0", AnchorMax = "0.99 1" }
                //    },
                //    "ButtonPanel"
                //},
                {
                    new CuiPanel
                    {
                        Image = { Color = "0 0 0 0" },
                        RectTransform = { AnchorMin = "0.021 0.105", AnchorMax = "0.979 0.879" },
                        CursorEnabled = true
                    },
                    "Panel",
                    "List"
                },
                {
                    new CuiPanel
                    {
                        Image = { Color = "0 0 0 0" },
                        RectTransform = { AnchorMin = "0.021 0.016", AnchorMax = "0.979 0.097" },
                    },
                    "Panel",
                    "Pages"
                }

            };

        }


        [ConsoleCommand("rep.listpages")]
        void ListAllPages(ConsoleSystem.Arg arg)
        {
            if (arg.Player() == null)
                return;
            BasePlayer player = arg.Player();

            CuiHelper.DestroyUi(player, "Pages");
            CuiHelper.AddUi(player, new CuiElementContainer {{
                new CuiPanel
                {
                    Image = { Color = "0 0 0 0" },
                    RectTransform = { AnchorMin = "0.021 0.016", AnchorMax = "0.979 0.097" },

                },
                "Panel",
                "Pages"
            } });

            AllPlayers = ListAllPlayers();
            var countOfPages = Math.Ceiling((AllPlayers.Count / (float)AmountOfPlayersOnPage));
            var a = 0;
            for (int i = 1; i <= countOfPages; i++)
            {
                CuiHelper.AddUi(player, new CuiElementContainer {{
                    new CuiButton
                    {
                        Text = { Text = $"{i}", FontSize = 20, Align = TextAnchor.MiddleCenter },
                        Button = { Command = $"rep.page {i}", Color = MainColor },
                        RectTransform = { AnchorMin = $"{0.04 * a} 0.001", AnchorMax = $"{0.035 + 0.040 * a} 1" },
                    }, "Pages"
                } });
                a++;
            }

        }

        [ConsoleCommand("rep.page")]
        void ListPlayersInPanelByPage(ConsoleSystem.Arg arg)
        {
            if (arg.Player() == null)
                return;
            BasePlayer player = arg.Player();
            CuiHelper.DestroyUi(player, "List");
            CuiHelper.AddUi(player, new CuiElementContainer {
                {
                    new CuiPanel
                    {
                        Image = { Color = "0 0 0 0" },
                        RectTransform = { AnchorMin = "0.021 0.105", AnchorMax = "0.979 0.879" },
                        CursorEnabled = true
                    },
                    "Panel",
                    "List"
                }
            });

            int page = arg.GetInt(0);

            var index = 0;
            foreach (var basePlayer in GetPlayersByPage(page))
            {
                var name = GetPaintedTag(basePlayer.displayName, "#43b67f");


                CuiHelper.AddUi(player, new CuiElementContainer {
                    {
                    new CuiPanel
                    {
                        Image = { Color = "0 0 0 0.75" },
                        RectTransform = { AnchorMin = $"0 {0.91 - index * 0.1}", AnchorMax = $"1 {1 - index * 0.1}" },
                    },
                    "List",
                    $"PlayerPanel{index}"
                },
                {
                    new CuiButton
                    {
                        Text = { Text = name, FontSize = 20, Align = TextAnchor.MiddleLeft
                        , Color = "1 1 1 1" },
                        RectTransform = { AnchorMin = "0.015 0", AnchorMax = "0.4 1" },
                        Button = { Command = $"repgui.open.profile {page} {basePlayer.UserIDString}", Color = "0 0 0 0" }
                    },
                    $"PlayerPanel{index}"
                },
                {
                    new CuiLabel
                    {
                        Text = { Text = GetRepByPlayerWithColor(basePlayer), FontSize = 20, Align = TextAnchor.MiddleLeft, Color = "1 1 1 1" },
                        RectTransform = { AnchorMin = "0.3 0", AnchorMax = "0.4 1" },
                    },
                    $"PlayerPanel{index}",
                    $"TargetInstance.RepIndex{index}"

                },
                {
                    new CuiPanel
                    {
                        Image = { Color = "0 0 0 0" },
                        RectTransform = { AnchorMin = "0.762 0.066", AnchorMax = "0.996 0.945" },
                        CursorEnabled = true
                    },
                    $"PlayerPanel{index}",
                    $"PlayerPanelDecision{index}"
                },
                {
                    new CuiButton
                    {
                        Button = { Command = $"repgui.createpanel.category like {index} {basePlayer.displayName.ToString()}", Color = MainColor,  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                        Text = { Text = ":)", FontSize = 20, Align = TextAnchor.MiddleCenter },
                        RectTransform = { AnchorMin = "0 0", AnchorMax = "0.318 0.93" }
                    },
                    $"PlayerPanelDecision{index}"
                },
                {
                    new CuiButton
                    {
                        Text = { Text = "-_-", FontSize = 20, Align = TextAnchor.MiddleCenter },
                        Button = { Command = $"rep.set normal {index} normal {basePlayer.displayName.ToString()}", Color = "0.35 0.35 0.35 1",  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                        RectTransform = { AnchorMin = "0.336 0", AnchorMax = "0.651 0.93" }
                    },
                    $"PlayerPanelDecision{index}"
                },
                {
                    new CuiButton
                    {
                        Text = { Text = ":(", FontSize = 20, Align = TextAnchor.MiddleCenter },
                        Button = { Command = $"repgui.createpanel.category dis {index} {basePlayer.displayName.ToString()}", Color = RedColor, Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                        RectTransform = { AnchorMin = "0.669 0", AnchorMax = "0.995 0.93" }
                    },
                    $"PlayerPanelDecision{index}"
                }
                });
                index++;
            }
        }

        // Выход с панельки
        [ConsoleCommand("repgui.exit")]
        private void RepGUIExit(ConsoleSystem.Arg arg)
        {
            if (arg.Player() == null)
                return;
            BasePlayer player = arg.Player();
            CuiHelper.DestroyUi(player, "Panel");
        }

        #endregion

        #region Профиль

        [ConsoleCommand("repgui.open.profile")]
        private void OpenProfile(ConsoleSystem.Arg arg)
        {
            if (arg.Player() == null)
                return;
            BasePlayer player = arg.Player();

            Puts(arg.FullString);
            var idNameStarts = 1;
            var days = "";
            var suffixOfDays = "";
            var hours = "";
            var suffixOfHours = "";
            var minutes = "";
            var suffixOfMinutes = "";
            var suffixOfYears = "";


            var target = FindBasePlayer(arg.GetString(idNameStarts));

            string time;

            if (StoreBonus != null)
            {
                time = (string)StoreBonus.Call("GetPlayerTimeOnServer", target);
            }
            else
                time = "Неизвестно";

            var page = arg.GetString(0);

            if (target != null)
            {
                var check = (from x in Tops where x.UID == target.UserIDString select x).Count();
                if (check == 0)
                {
                    CreateInfo(target);
                }

                player.SendConsoleCommand($"repgui.exit");

                var rep = (from x in Tops where x.UID == target.UserIDString select x).FirstOrDefault().Rep;
                var Adequate = 0;
                var Help = 0;
                var AntiRaid = 0;
                var Honesty = 0;
                var Inadequate = 0;
                var Harm = 0;
                var Raid = 0;
                var Lie = 0;
                var Decision = 0;

                foreach (var item in rep.Keys)
                {
                    foreach (var cat in rep[item])
                    {
                        if (cat.Key == "Адекватный")
                            Adequate += cat.Value;
                        if (cat.Key == "Помощь")
                            Help += cat.Value;
                        if (cat.Key == "Анти-Рейд")
                            AntiRaid += cat.Value;
                        if (cat.Key == "Честность")
                            Honesty += cat.Value;
                        if (cat.Key == "Неадекватный")
                            Inadequate += cat.Value;
                        if (cat.Key == "Вред")
                            Harm += cat.Value;
                        if (cat.Key == "Рейд")
                            Raid += cat.Value;
                        if (cat.Key == "Обман")
                            Lie += cat.Value;
                        if (cat.Key == "Так решил Федерастер")
                            Decision += cat.Value;
                    }
                }

                var colorOne = "0 0 0 0.75";
                var colorTwo = MainColor;


                var number = GetRepByPlayer(target);
                if (number < 0)
                {
                    colorTwo = RedColor;
                }
                if (number == 0)
                {
                    colorTwo = GreyColor;

                }

                CuiHelper.AddUi(player, CreateProfile(target, page, $"{time}", target.displayName, colorOne, colorTwo));

                CuiHelper.AddUi(player, new CuiElementContainer
                {

                    {
                        new CuiLabel
                        {
                            Text = { Text = $"Адекватный: {Adequate}", FontSize = 18, Align = TextAnchor.MiddleLeft },
                            RectTransform = { AnchorMin = "0.063 0.705", AnchorMax = "1 0.742" },
                        },
                        "PanelSecond",
                        "Adequate"
                    },
                    {
                        new CuiLabel
                        {
                            Text = { Text = $"Помощь: {Help}", FontSize = 18, Align = TextAnchor.MiddleLeft },
                            RectTransform = { AnchorMin = "0.063 0.64", AnchorMax = "1 0.677" },
                        },
                        "PanelSecond",
                        "Help"
                    },
                    {
                        new CuiLabel
                        {
                            Text = { Text = $"Анти-рейд: {AntiRaid}", FontSize = 18, Align = TextAnchor.MiddleLeft },
                            RectTransform = { AnchorMin = "0.063 0.576", AnchorMax = "1 0.613" },
                        },
                        "PanelSecond",
                        "AntiRaid"
                    },
                    {
                        new CuiLabel
                        {
                            Text = { Text = $"Честность: {Honesty}", FontSize = 18, Align = TextAnchor.MiddleLeft },
                            RectTransform = { AnchorMin = "0.063 0.511", AnchorMax = "1 0.548" },
                        },
                        "PanelSecond",
                        "Honesty"
                    },


                    {
                        new CuiLabel
                        {
                            Text = { Text = $"Неадекватный: {Inadequate}", FontSize = 18, Align = TextAnchor.MiddleLeft },
                            RectTransform = { AnchorMin = "0.063 0.321", AnchorMax = "1 0.360" },
                        },
                        "PanelSecond",
                        "Inadequate"
                    },
                    {
                        new CuiLabel
                        {
                            Text = { Text = $"Вред: {Harm}", FontSize = 18, Align = TextAnchor.MiddleLeft },
                            RectTransform = { AnchorMin = "0.063 0.256", AnchorMax = "1 0.295" },
                        },
                        "PanelSecond",
                        "Harm"
                    },
                    {
                        new CuiLabel
                        {
                            Text = { Text = $"Рейд: {Raid}", FontSize = 18, Align = TextAnchor.MiddleLeft },
                            RectTransform = { AnchorMin = "0.063 0.191", AnchorMax = "1 0.230" },
                        },
                        "PanelSecond",
                        "Raid"
                    },
                    {
                        new CuiLabel
                        {
                            Text = { Text = $"Обман: {Lie}", FontSize = 18, Align = TextAnchor.MiddleLeft },
                            RectTransform = { AnchorMin = "0.063 0.126", AnchorMax = "1 0.165" },
                        },
                        "PanelSecond",
                        "Lie"
                    },
                    {
                        new CuiLabel
                        {
                            Text = { Text = $"<color=#fffff4>Решение Бога: {Decision}</color>", FontSize = 18, Align = TextAnchor.MiddleLeft },
                            RectTransform = { AnchorMin = "0.063 0", AnchorMax = "1 0.1" },
                        },
                        "PanelSecond",
                        "Decision"
                    }
                });

                webrequest.Enqueue($"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key=4D07BD7B441615DF886DF708D7458274&steamids={target.UserIDString}", null, (code, response) =>
                    GetCallback(code, response, player), this, RequestMethod.GET);
            }
            else
            {
                SendReply(player, "Player Not Found");
            }

        }

        [ConsoleCommand("repgui.close.profile")]
        private void CloseProfile(ConsoleSystem.Arg arg)
        {
            if (arg.Player() == null)
                return;
            BasePlayer player = arg.Player();
            var page = arg.GetString(0);

            CuiHelper.DestroyUi(player, "Profile");

            CuiElementContainer container;
            container = CreatePanel(player);
            CuiHelper.AddUi(player, container);

            player.SendConsoleCommand($"rep.listpages");
            player.SendConsoleCommand($"rep.page {page}");

        }

        [ConsoleCommand("repgui.close.profile.atall")]
        private void CloseProfileAtAll(ConsoleSystem.Arg arg)
        {
            if (arg.Player() == null)
                return;
            BasePlayer player = arg.Player();
            var page = arg.GetString(0);

            CuiHelper.DestroyUi(player, "Profile");

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

            CuiHelper.AddUi(player, new CuiElementContainer { CreateImage("PanelOne", cur.ToString())});
        }



        private CuiElementContainer CreateProfile(BasePlayer player, string page, string time, string name, string PanelColor, string InterPanelColor)
        {
            
            return new CuiElementContainer
            {
                {
                    new CuiPanel
                    {
                        Image = { Color = "0 0 0 0" },
                        RectTransform = { AnchorMin = "0.06 0.15", AnchorMax = "0.94 0.931" },
                        CursorEnabled = true
                    },
                    "Overlay",
                    "Profile"
                },
                {
                    new CuiButton
                    {
                        Text = { Text = "", FontSize = 20, Align = TextAnchor.MiddleCenter },
                        Button = { Command = $"repgui.close.profile {page}", Color = "0 0 0 0" },
                        RectTransform = { AnchorMin = "-100 -100", AnchorMax = "100 100" }
                    },
                    "Profile"
                },
                {//
                    new CuiPanel
                    {
                        //Image = { Color = "0.64 0.75 0.66 0.35" },
                        Image = { Color = PanelColor, /* Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" */ },
                        RectTransform = { AnchorMin = "0 0", AnchorMax = "0.25 0.996" }
                    },
                    "Profile",
                    "PanelOne"
                },
                {
                    new CuiPanel
                    {
                        Image = { Color = InterPanelColor,  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                        RectTransform = { AnchorMin = "0 0.9", AnchorMax = "0.996 1" }
                    },
                    "PanelOne",
                    "InterPanelOne"
                },
                {
                    new CuiLabel
                    {
                        Text = { Text = "РЕПУТАЦИЯ ИГРОКОВ", FontSize = 20, Align = TextAnchor.MiddleCenter },
                        RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
                    },
                    "InterPanelOne"
                },
                {
                    new CuiPanel
                    {
                        Image = { Color = InterPanelColor,  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                        RectTransform = { AnchorMin = "0 0.18", AnchorMax = "0.996 0.27" }
                    },
                    "PanelOne",
                    "InterPanelSecond"
                },
                {
                    new CuiLabel
                    {
                        Text = { Text = $"{GetPaintedTag(name, "orange")}", FontSize = 20, Align = TextAnchor.MiddleCenter },
                        RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
                    },
                    "InterPanelSecond"
                },
                {
                    new CuiPanel
                    {
                        Image = { Color = InterPanelColor ,  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat"},
                        RectTransform = { AnchorMin = "0 0.03", AnchorMax = "0.996 0.16" }
                    },
                    "PanelOne",
                    "InterPanelThird"
                },
                {
                    new CuiLabel
                    {
                        Text = { Text = "На сервере:", FontSize = 20, Align = TextAnchor.MiddleCenter },
                        RectTransform = { AnchorMin = "0 0.5", AnchorMax = "1 1" },
                    },
                    "InterPanelThird"
                },
                {
                    new CuiLabel
                    {
                        Text = { Text = $"{time}", FontSize = 18, Align = TextAnchor.MiddleCenter },
                        RectTransform = { AnchorMin = "0 0", AnchorMax = "1 0.5" },
                    },
                    "InterPanelThird"
                },

                // Вторая панель
                {
                    new CuiPanel
                    {
                        Image = { Color = PanelColor, /* Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat"*/ },
                        RectTransform = { AnchorMin = "0.27 0", AnchorMax = "1 0.996" }
                    },
                    "Profile",
                    "PanelSecond"
                },
                {
                    new CuiPanel
                    {
                        Image = { Color = InterPanelColor,  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                        RectTransform = { AnchorMin = "0 0.9", AnchorMax = "0.999 1" }
                    },
                    "PanelSecond",
                    "InterPanelStatOne"
                },
                {
                    new CuiLabel
                    {
                        Text = { Text = "СТАТИТИСТИКА", FontSize = 20, Align = TextAnchor.MiddleCenter },
                        RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
                    },
                    "InterPanelStatOne"
                },
                {
                    new CuiPanel
                    {
                        Image = { Color = MainColor,  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                        RectTransform = { AnchorMin = "0 0.774", AnchorMax = "0.999 0.855" }
                    },
                    "PanelSecond",
                    "InterPanelStatSecond"
                },
                {
                    new CuiLabel
                    {
                        Text = { Text = "Положительная репутация", FontSize = 20, Align = TextAnchor.MiddleCenter },
                        RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
                    },
                    "InterPanelStatSecond"
                },
                {
                    new CuiPanel
                    {
                        Image = { Color = RedColor,  Sprite = "Assets/Content/UI/UI.Background.Tile.psd", Material = "assets/content/ui/uibackgroundblur.mat" },
                        RectTransform = { AnchorMin = "0 0.387", AnchorMax = "0.999 0.468" }
                    },
                    "PanelSecond",
                    "InterPanelStatThird"
                },
                {
                    new CuiLabel
                    {
                        Text = { Text = "Отрицательная репутация", FontSize = 20, Align = TextAnchor.MiddleCenter },
                        RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
                    },
                    "InterPanelStatThird"
                },

                //{
                //    new CuiButton
                //    {
                //        Text = { Text = "<color=#fff>Закрыть</color>", FontSize = 20, Align = TextAnchor.MiddleCenter },
                //        Button = { Command = $"repgui.close.profile {page}", Color = "0 0 0 0" },
                //        RectTransform = { AnchorMin = "0.847 0.91", AnchorMax = "0.946 1" }
                //    },
                //    "Profile"
                //},

            };

        }

        private static CuiElement CreateImage(string panelName, string url)
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
                AnchorMin = "0.217 0.455",
                AnchorMax = "0.763 0.758"
            };
            element.Components.Add(image);
            element.Components.Add(rectTransform);
            element.Name = CuiHelper.GetGuid();
            element.Parent = panelName;

            return element;
        }

        #endregion

        #region Вспомогательные Функции
        public static string FormatTime(TimeSpan time)
        {
            string result = string.Empty;
            if (time.Days != 0)
                result += $"{Format(time.Days, "дней", "дня", "день")} ";

            if (time.Hours != 0)
                result += $"{Format(time.Hours, "часов", "часа", "час")} ";

            if (time.Minutes != 0)
                result += $"{Format(time.Minutes, "минут", "минуты", "минута")} ";

            if (time.Seconds != 0)
                result += $"{Format(time.Seconds, "секунд", "секунды", "секунда")} ";

            return result;
        }

        private static string Format(int units, string form1, string form2, string form3)
        {
            var tmp = units % 10;

            if (units >= 5 && units <= 20 || tmp >= 5 && tmp <= 9)
                return $"{units} {form1}";

            if (tmp >= 2 && tmp <= 4)
                return $"{units} {form2}";

            return $"{units} {form3}";
        }

        public List<BasePlayer> GetPlayersByPage(int page)
        {
            var pageZero = page - 1;
            var allPlayer = ListAllPlayers();
            int countOfPlayers;
            if (pageZero * AmountOfPlayersOnPage + AmountOfPlayersOnPage <= allPlayer.Count)
                countOfPlayers = AmountOfPlayersOnPage;
            else
                countOfPlayers = allPlayer.Count % AmountOfPlayersOnPage;

            var lst = allPlayer.ToList().GetRange(0 + pageZero * AmountOfPlayersOnPage, countOfPlayers);

            return lst;

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

        public ListHashSet<BasePlayer> ListAllPlayers()
        {
            //var sleepingPlayers = BasePlayer.sleepingPlayerList;
            ListHashSet<BasePlayer> activePlayers = BasePlayer.activePlayerList;
            foreach (var pl in activePlayers)
            {
                if (pl.UserIDString == "")
                {
                    activePlayers.Remove(pl);
                }
            }
            //var allPlayers = activePlayers.Concat(sleepingPlayers).ToList();
            //return allPlayers;
            return activePlayers;
        }

        [ChatCommand("getplayerrep")]
        void SetRepAdmin(BasePlayer player, string command, string[] args)
        {
            if (!player.IPlayer.HasPermission(permSetRep))
            {
                SendReply(player, "У вас нет прав на выполнение этой команды");
                return;
            }
            if (args.Count() != 1)
            {
                SendReply(player, "Введите всего один параметр - SteamID или имя игрока");
                return;
            }
            var name = args.ElementAt(0);
            var target = FindBasePlayer(name);
            if (target == null)
            {
                SendReply(player, $"Игрок {name} не найден");
                return;
            }
            var TargetInstance = (from x in Tops where x.UID == target.UserIDString select x).FirstOrDefault();

            SendReply(player, $"У игрока {target.displayName} следующая репутация:");
            foreach (var item in TargetInstance.Rep)
            {
                var value = TargetInstance.Rep[item.Key].FirstOrDefault();
                SendReply(player, $"{(FindBasePlayer(item.Key)).displayName} - {value.Value}");
            }
        }

        #endregion

    }
}
