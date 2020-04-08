using System.Collections;
using System.Collections.Generic;
using System.IO;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Game.Rust.Cui;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("AspectRatio", "ApiGUI", "1.2")]
    public class AspectRatio : RustPlugin
    {
        public static string PngID;
        Dictionary<ulong, string> aspectratioData = new Dictionary<ulong, string>();

		DynamicConfigFile aspectratioDataFile = Interface.Oxide.DataFileSystem.GetFile("AspectRatio");

        void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                { "CHANGED.ASPECT.RATIO", "Соотношение сторон экрана изменено на \"<color=#DCFF66>{0}</color>\".\nИспользуйте <color=#DCFF66>/ar</color> для повторной калибровки интерфейса." },
                { "UI.HELP", "<color=#FFFFFF>Выберите соотношение сторон вашего монитора. \nEсли не нашли его выберите <color=#D3442E>НАИБОЛЕЕ РОВНЫЙ КРУГ</color>.</color>" },
                { "UI.RES.CHOOSED", "Выбрано" }
            }, this);
        }

        void OnServerInitialized()
        {
            aspectratioData = aspectratioDataFile.ReadObject<Dictionary<ulong, string>>();
            new GameObject("WebObject").AddComponent<Images>();
        }

        class Images : MonoBehaviour
        {
            private MemoryStream stream = new MemoryStream();

            void Awake()
            {
                var www = new WWW("http://s011.radikal.ru/i315/1612/f9/f6864d81a6db.png");
                StartCoroutine(WaitForRequest(www));
            }

            IEnumerator WaitForRequest(WWW www)
            {
                yield return www;
                
                    PngID = FileStorage.server.Store( www.bytes, FileStorage.Type.png, CommunityEntity.ServerInstance.net.ID).ToString();
            }
        }

        void Loaded() => LoadDefaultMessages();

        protected override void LoadDefaultConfig()
        {
            Config["Показывать калибровку интерфейса при первом входе на сервер"] = true;
        }

        void OnServerSave() => aspectratioDataFile.WriteObject(aspectratioData);

        void Unload() => aspectratioDataFile.WriteObject(aspectratioData);

        void OnPlayerInit(BasePlayer player)
        {
            if (!Config.Get<bool>("Показывать калибровку интерфейса при первом входе на сервер")) return;

            if (player == null)
                return;

            if (aspectratioData.ContainsKey(player.userID))
                return;

            if (player.IsReceivingSnapshot)
            {
                timer.Once(2, () => OnPlayerInit(player));
                return;
            }

            //player.SendConsoleCommand("uiscale 1"); // перенес в отд плагин
            ShowAspectRatioMenu(player);
        }

		void ShowAspectRatioMenu(BasePlayer player)
		{
			CuiHelper.DestroyUi(player, "AspectRatioMain");

			CuiElementContainer container = new CuiElementContainer();

            container.Add(new CuiElement
            {
                Name = "AspectRatioMain",
				Parent = "Hud",
                Components =
                {
                    new CuiRawImageComponent { Color = "0 0 0 0" },
					new CuiNeedsCursorComponent(),
                    new CuiRectTransformComponent()
                }
            });

            string TitlePanelName = CuiHelper.GetGuid();

            container.Add(new CuiElement
            {
                Name = TitlePanelName,
                Parent = "AspectRatioMain",
                Components =
                {
                    new CuiRawImageComponent { Color = "0.5 0.5 0.5 0.1" },
                    new CuiRectTransformComponent
                    {
                        AnchorMin = "0.2 0.85",
                        AnchorMax = "0.8 0.95"
                    }
                }
            });

            container.Add(new CuiElement
            {
                Name = CuiHelper.GetGuid(),
                Parent = TitlePanelName,
                Components =
                {
                    new CuiTextComponent
                    {
                        Text = GetLangMessage("UI.HELP"),
                        FontSize = 25,
                        Align = TextAnchor.MiddleCenter,
                        Font = "robotocondensed-regular.ttf"
                    },
                    new CuiRectTransformComponent(),
                    new CuiOutlineComponent() { Color = "0 0 0 1" }
                }
            });

            //16/9: 

            //9 / 16 = X;
            //(xMax - xMin) / X = B
            // Ymax - B = Ymin

            string UserAspectRatio = (string)(GetUserAspectRatio(player.userID) ?? string.Empty);

            CreateCircle(container, "0.2 0.4444", "0.4 0.8", "16x9", UserAspectRatio == "16x9");
            CreateCircle(container, "0.6 0.48", "0.8 0.8", "16x10", UserAspectRatio == "16x10");
            CreateCircle(container, "0.2 0.1333", "0.4 0.4", "4x3", UserAspectRatio == "4x3");
            CreateCircle(container, "0.6 0.15", "0.8 0.4", "5x4", UserAspectRatio == "5x4");
            											
			CuiHelper.AddUi(player, container); 
		}
				
        void CreateCircle(CuiElementContainer container, string AnchorMin, string AnchorMax, string AspectRatio, bool Active = false)
        {
            string BoxName = CuiHelper.GetGuid();

            container.Add(new CuiElement
            {
                Name = BoxName,
                Parent = "AspectRatioMain",
                Components =
                {
                    new CuiRawImageComponent
                    {
                        Png = PngID
                    },
                    new CuiRectTransformComponent
                    {
                        AnchorMin = AnchorMin,
                        AnchorMax = AnchorMax
                    },
                    new CuiOutlineComponent()
                    {
                        Color = "0 0 0 1"
                    }
                }
            });

            container.Add(new CuiElement
            {
                Name = CuiHelper.GetGuid(),
                Parent = BoxName,
                Components =
                {
                    new CuiTextComponent
                    {
                        Text = AspectRatio + (Active ? "\n\n" + GetLangMessage("UI.RES.CHOOSED") : ""),
                        FontSize = 30,
                        Align = TextAnchor.MiddleCenter,
                        Font = "robotocondensed-regular.ttf"
                    },
                    new CuiRectTransformComponent(),
                    new CuiOutlineComponent() { Color = "0 0 0 1" }
                }
            });

            container.Add(new CuiElement
            {
                Name = BoxName,
                Parent = "AspectRatioMain",
                Components =
                {
                    new CuiButtonComponent
                    {
                        Command = "aspect.select " + AspectRatio, // NOTE! Text will put as CMD
                        Close = "AspectRatioMain",
                        Color = "1 1 1 0"
                    },
                    new CuiRectTransformComponent
                    {
                        AnchorMin = AnchorMin,
                        AnchorMax = AnchorMax
                    }
                }
            });
        }
		
		[ConsoleCommand("aspect.select")]
		void ConsoleCmd_Select(ConsoleSystem.Arg arg)
		{
			if (arg.Connection != null)
            {
				string SelectedAspectRatio = arg.Args[0];

                switch(SelectedAspectRatio)
                {
                    case "16x9":
                    case "16x10":
                    case "5x4":
                    case "4x3": break;

                    default: return;
                }

                BasePlayer player = arg.Player();

				aspectratioData[player.userID] = SelectedAspectRatio;

                Interface.Oxide.CallHook("OnUserAspectRatio", player, SelectedAspectRatio);

				SendReply(player, $"<size=16>{string.Format(GetLangMessage("CHANGED.ASPECT.RATIO"), SelectedAspectRatio)}</size>");
			}
		}
		
		[ChatCommand("ar")]
        void ChatCmd_Ratio(BasePlayer player, string command, string[] args) => ShowAspectRatioMenu(player);

		// API - Плагина нужно для добавления в другие плагины, что бы отображать корректно gui у пользователей.
        object GetUserAspectRatio(ulong userId)
        {
            string AspectRatioState;

            if (aspectratioData.TryGetValue(userId, out AspectRatioState))
                return AspectRatioState;

            return null;
        }

        string GetLangMessage(string key, string steamID = null) => lang.GetMessage(key, this, steamID);
    }
}