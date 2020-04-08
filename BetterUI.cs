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
using System.Text.RegularExpressions;

namespace Oxide.Plugins
{
    [Info("BetterUI", "Lulex.py", "0.0.1")]
    internal class BetterUI : RustPlugin
    {
        public class CUIBetterUI
        {
            public string Parent;
            string Name;

            string Color;
            string Sprite;
            string Material;

            string LineColor;

            int top     = 0;
            int right   = 0;
            int bottom  = 0;
            int left    = 0;

            public string nickname { get; set; }
        }

        private const string defaultParentLayer = "BetterUILayer";
        public string DefaultGreenDarkColor { get; } = HexToCuiColor("1f5a49");





        private void setProperty(string propery, string value)
        {
            /*switch (propery)
            {
                case "color":
                    color = value;
                    break;
            }*/
        }


        private CUIBetterUI CreateRect()
        {
            return new CUIBetterUI();
        }


        private void setproperty(CUIBetterUI element, string param)
        {
            element.Parent = "Test";

            PrintWarning(param);

            string pattern = @"(\b\w+)='([a-zA-Z0-9 ]+)'";
            MatchCollection matches = Regex.Matches(param, pattern);

            foreach (Match match in matches)
            {
                PrintWarning(match.Value);

                PrintWarning($"property: {match.Groups[1].Value}");
                PrintWarning($"value:    {match.Groups[2].Value}");
                Puts(".");
            }


        }


        private void getProperty(CUIBetterUI element)
        {
            PrintWarning(element.Parent);
        }











        public CuiElement getRect(string layerTitle = "modal", string parentLayer = defaultParentLayer)
        {
            return new CuiElement
            {
                Parent = parentLayer,
                Name = layerTitle,
                Components = {
                    new CuiImageComponent {
                        Color = DefaultGreenDarkColor,
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
            };
        }

        

        private void destroyRect(BasePlayer player, string rectTitle)
        {
            CuiHelper.DestroyUi(player, rectTitle);
        }

        private void drawRect(BasePlayer player, CuiElement rect)
        {
            CuiHelper.AddUi(player, new CuiElementContainer
            {
                {
                    rect
                }
            });
        }

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
    }
}
