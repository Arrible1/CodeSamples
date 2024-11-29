using PortalHunter.GameTools.I18n;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using PortalHunter.I18n;
using System;

namespace PortalHunter.GameTools
{
    public class ProducerOfRich
    {
        public const string GDTypePattern = @"\$[A-Za-z0-9]+";

        public static readonly IReadOnlyDictionary<string, string> IconKeyToColor = new Dictionary<string, string>
        {
            { HpKey, "84ff49" },
            { ImmuneShieldKey, "a5a5a5" },
            { FrostKey, "239eff" },
            { FireKey, "ff2a00" },
            { ThunderKey, "00fff1" },
            { HotKey, "ff2a00" },
            { ColdKey, "239eff" },
            { PhysKey, "ffffff" },
            { WeakPointKey, "ffb500" },
            { PiercingKey, "fdff83" },
            { FrostBurnKey, "ffb500" },
        };

        public static string HpKey => "hp";
        public static string ImmuneShieldKey => "shield";
        public static string FrostKey => "ice";
        public static string FireKey => "fire";
        public static string ThunderKey => "electr";
        public static string HotKey => "hot";
        public static string ColdKey => "cold";
        public static string PhysKey => "phys";
        public static string WeakPointKey => "weakpoint";
        public static string PiercingKey => "piercing";
        public static string FrostBurnKey => "frostburn";

        public static string GetSpriteForKey(string key) => $"<sprite name={key}>";
        public static string GetColorForKey(string key) => $"<color=#${IconKeyToColor[key]}>";

        public static string MakeRich(string source)
        {
            if (string.IsNullOrEmpty(source))
                return string.Empty;

            source = NewCoolPatternReplacer(source, GDTypePattern, GetTranslationForType);
            source = NewCoolPatternReplacer(source, PluralService.KeyPattern, MakeTextRich);

            return source;
        }

        static string NewCoolPatternReplacer(string source, string pattern, Func<string, string> richFunc)
        {
            var mathces = Regex.Matches(source, pattern);
            if (mathces.Count == 0) return source;

            var res = new System.Text.StringBuilder();

            var startIndex = 0;
            var count = 0;
            foreach (Match match in mathces)
            {
                var key = match.Value;
                count = match.Index - startIndex;

                if (match.Index != 0 && source[match.Index - 1] == '+')
                {
                    key = "+" + key;
                    count--;
                }
                if (match.Index + match.Value.Length < source.Length && source[match.Index + match.Value.Length] == '%')
                {
                    key = key + "%";
                }
                res.Append(source.Substring(startIndex, count));
                res.Append(richFunc(key));

                startIndex += count + key.Length;
            }

            if (startIndex < source.Length)
            {
                res.Append(source.Substring(startIndex));
            }

            return res.ToString();
        }

        const string defaultFormatPlaceholder = "<color=#{0}>{1}</color>";
        const string placeholder = "<color=#{0}>{1}</color>{2}";

        static string MakeTextRich(string key)
        {
            var fkey = key.Replace("{", "").Replace("}", "").Replace("+", "").Replace("%", "");
            if (!IconKeyToColor.ContainsKey(fkey))
                return string.Format(defaultFormatPlaceholder, "ffffff", key);
            else
                return string.Format(placeholder, IconKeyToColor[fkey], key, GetSpriteForKey(fkey));
        }

        static string GetTranslationForType(string key)
        {
            var rich = MakeTextRich(key);

            return rich.Replace(key, I18nService.Instance.GetText($"{key}_Name"));
        }

        public static Color ToColor(string hex)
        {
            if (hex.Length < 6 || hex.Length > 8)
                return Color.white;

            hex = hex.Replace("0x", "").Replace("#", "");
            float a = 1f;
            float r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
            float g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
            float b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
            }
            return new Color(r, g, b, a);
        }

        static string ToRGBHex(Color c)
        {
            return string.Format("{0:X2}{1:X2}{2:X2}", ToByte(c.r), ToByte(c.g), ToByte(c.b));
        }

        static byte ToByte(float f)
        {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }
    }
}