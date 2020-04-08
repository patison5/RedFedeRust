using System.Collections.Generic;
using System.Linq;
using Oxide.Core;
using Oxide.Game.Rust.Cui;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Sound Browser", "Hougan", "0.0.1")]
    public class SoundBrowser : RustPlugin
    {
        #region Variables

        private HashSet<string> Prefabs = new HashSet<string>();
        private GameManifest.PooledString[] Manifest;

        #endregion

        #region OnServerInitialized

        private void OnServerInitialized()
        {
            if (!Interface.Oxide.DataFileSystem.ExistsDatafile(Name))
            {
                Manifest = GameManifest.Current.pooledStrings;

                foreach (GameManifest.PooledString asset in Manifest)
                {
                    if ((!asset.str.StartsWith("assets/content/")
                         && !asset.str.StartsWith("assets/bundled/")
                         && !asset.str.StartsWith("assets/prefabs/"))
                        || !asset.str.EndsWith(".prefab")) continue;

                    if (asset.str.Contains("/fx/")) Prefabs.Add(asset.str);
                }

                Interface.Oxide.DataFileSystem.WriteObject(Name, Prefabs);
            }
            else Prefabs = Interface.Oxide.DataFileSystem.ReadObject<HashSet<string>>(Name);
            
            PrintWarning($"Parsed {Prefabs.Count} fxes");
        }

        #endregion

        #region Commands

        [ConsoleCommand("s.browser")]
        private void CmdChatBrowser(ConsoleSystem.Arg args)
        {
            var player = args.Player();
            if (player == null) return;

            if (!args.HasArgs(1))
            {
                InitializeInterface(player, 0);
                return;
            }

            int parse = 0;
            if (!int.TryParse(args.Args[0], out parse) && args.Args[0].ToLower() != "remove" && args.Args[0].ToLower() != "save")
            {
                Effect effect = new Effect(args.Args[0], player, 0, new Vector3(), new Vector3());
                EffectNetwork.Send(effect, player.Connection);
                return;
            }

            if (args.Args[0].ToLower() == "remove")
            {
                Prefabs.Remove(args.Args[1]);
                Interface.Oxide.DataFileSystem.WriteObject(Name, Prefabs);
                
                InitializeInterface(player, int.Parse(args.Args[2]));
                return;
            }

            if (args.Args[0].ToLower() == "save")
            {
                PrintError("Saved prefab: " + args.Args[1]); 
                return;
            }
            
            InitializeInterface(player, int.Parse(args.Args[0]));
        }

        #endregion
        
        #region Interfaces

        private static string Layer = "UI_JopaHougana";

        private void InitializeInterface(BasePlayer player, int page)
        {
            CuiElementContainer container = new CuiElementContainer();

            if (page == 0)
            {
                CuiHelper.DestroyUi(player, "SoundBrowser");
                container.Add(new CuiPanel
                {
                    CursorEnabled = true,
                    RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1", OffsetMax = "0 0"},
                    Image = {Color = "0 0 0 0.95"}
                }, "Overlay", "SoundBrowser");
            }
            
            CuiHelper.DestroyUi(player, Layer);
            container.Add(new CuiPanel
            {
                CursorEnabled = false,
                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1", OffsetMax = "0 0"},
                Image = {Color = "0 0 0 0"}
            }, "SoundBrowser", Layer);

            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1", OffsetMax = "0 0"},
                Button = {Color = "0 0 0 0", Close = "SoundBrowser"},
                Text = {Text = ""}
            }, Layer);

            int currentMargin = 50;
            var list = Prefabs.Skip(page * 30).Take(18);
            foreach (var check in list)
            {
                container.Add(new CuiPanel
                {
                    RectTransform = { AnchorMin = "0 1", AnchorMax = "1 1", OffsetMin = $"150 {0 - currentMargin - 30}", OffsetMax = $"-150 {0 - currentMargin}" },
                    Image = { Color = "1 1 1 0.2" }
                }, Layer, Layer + currentMargin);
                
                container.Add(new CuiButton
                {
                    RectTransform = { AnchorMin = "0 0", AnchorMax = "0 1", OffsetMax = "100 0" },
                    Button = { Color = "0.4 0.7 0.4 1", Command = $"s.browser {check}" },
                    Text = { Text = "PLAY", Align = TextAnchor.MiddleCenter, Font = "robotocondensed-bold.ttf", FontSize = 18 }
                }, Layer + currentMargin);

                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1", OffsetMax = "0 0", OffsetMin = "120 0"},
                    Text = {Text = check.ToUpper(), Align = TextAnchor.MiddleLeft, Font = "robotocondensed-regular.ttf"}
                }, Layer + currentMargin);
                
                container.Add(new CuiButton
                {
                    RectTransform = { AnchorMin = "1 0", AnchorMax = "1 1", OffsetMin = "-25 0", OffsetMax = "0 0" },
                    Button = { Color = "0.7 0.4 0.4 1", Command = $"s.browser remove {check} {page}" },
                    Text = { Text = "X", Align = TextAnchor.MiddleCenter, Font = "robotocondensed-bold.ttf", FontSize = 18 }
                }, Layer + currentMargin);
                
                container.Add(new CuiButton
                {
                    RectTransform = { AnchorMin = "1 0", AnchorMax = "1 1", OffsetMin = "-100 0", OffsetMax = "-25 0" },
                    Button = { Color = "0.4 0.4 0.7 1", Command = $"s.browser save {check}" },
                    Text = { Text = "SAVE", Align = TextAnchor.MiddleCenter, Font = "robotocondensed-regular.ttf", FontSize = 18 }
                }, Layer + currentMargin);

                currentMargin += 35;
            }
            
            container.Add(new CuiButton
            {
                RectTransform = { AnchorMin = "0.5 0", AnchorMax = "0.5 0", OffsetMin = "-100 10", OffsetMax = "100 35"},
                Button = { Color = "1 1 1 0.2" },
                Text = { Text = page.ToString(), Align = TextAnchor.MiddleCenter, Font = "robotocondensed-regular.ttf" }
            }, Layer, Layer + ".BTN");
            
            container.Add(new CuiButton
            {
                RectTransform = { AnchorMin = "0 0", AnchorMax = "0.25 1", OffsetMin = "0 0", OffsetMax = "0 0"},
                Button = { Color = "1 1 1 0.2", Command = $"s.browser {Mathf.Max(page - 1, 0)}" },
                Text = { Text = "<", Align = TextAnchor.MiddleCenter, Font = "robotocondensed-regular.ttf" }
            }, Layer + ".BTN");
            
            container.Add(new CuiButton
            {
                RectTransform = { AnchorMin = "0.75 0", AnchorMax = "1 1", OffsetMin = "0 0", OffsetMax = "0 0"},
                Button = { Color = "1 1 1 0.2", Command = $"s.browser {Mathf.Max(page + 1, 0)}" },
                Text = { Text = ">", Align = TextAnchor.MiddleCenter, Font = "robotocondensed-regular.ttf" }
            }, Layer + ".BTN");

            CuiHelper.AddUi(player, container);
        }

        #endregion
    }
}