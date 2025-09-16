using NanoUI.Common;
using Color = NanoUI.Common.Color;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Net;
using System.Text;

namespace NanoUI.Utils
{
    // note: parse methods assumes that your have first "sanitized" your string (no control chars & trimmed)

    /// <summary>
    /// StringUtils.
    /// </summary>
    public static class StringUtils
    {
        static StringBuilder _stringBuilder = new();

        // these are units we try to parse out (also '%')
        // note: we currently support only 2 char units
        static string[] _units = { "px", "pt", "em" };

        #region Named colors

        internal static Dictionary<string, Color> _namedColors = new()
        {
            { "transparent", Color.Transparent },
            { "aliceblue", Color.AliceBlue },
            { "antiquewhite", Color.AntiqueWhite },
            { "aqua", Color.Aqua },
            { "aquamarine", Color.Aquamarine },
            { "azure", Color.Azure },
            { "beige", Color.Beige },
            { "bisque", Color.Bisque },
            { "black", Color.Black },
            { "blanchedalmond", Color.BlanchedAlmond },
            { "blue", Color.Blue },
            { "blueviolet", Color.BlueViolet },
            { "brown", Color.Brown },
            { "burlyWood", Color.BurlyWood },
            { "cadetblue", Color.CadetBlue },
            { "chartreuse", Color.Chartreuse },
            { "chocolate", Color.Chocolate },
            { "coral", Color.Coral },
            { "cornflowerblue", Color.CornflowerBlue },
            { "cornsilk", Color.Cornsilk },
            { "crimson", Color.Crimson  },
            { "cyan", Color.Cyan },
            { "darkblue", Color.DarkBlue },
            { "darkcyan", Color.DarkCyan },
            { "darkgoldenrod", Color.DarkGoldenrod },
            { "darkgray", Color.DarkGray },
            { "darkgreen", Color.DarkGreen },
            { "darkkhaki", Color.DarkKhaki },
            { "darkmagenta", Color.DarkMagenta },
            { "darkolivegreen", Color.DarkOliveGreen },
            { "darkorange", Color.DarkOrange },
            { "darkorchid", Color.DarkOrchid },
            { "darkred", Color.DarkRed },
            { "darksalmon", Color.DarkSalmon },
            { "darkseagreen", Color.DarkSeaGreen },
            { "darkslateblue", Color.DarkSlateBlue },
            { "darkslategray", Color.DarkSlateGray },
            { "darkturquoise", Color.DarkTurquoise },
            { "darkviolet", Color.DarkViolet },
            { "deeppink", Color.DeepPink },
            { "deepskyblue", Color.DeepSkyBlue },
            { "dimgray", Color.DimGray },
            { "dodgerblue", Color.DodgerBlue },
            { "firebrick", Color.Firebrick },
            { "floralwhite", Color.FloralWhite },
            { "forestgreen", Color.ForestGreen },
            { "fuchsia", Color.Fuchsia },
            { "gainsboro", Color.Gainsboro },
            { "ghostwhite", Color.GhostWhite },
            { "gold", Color.Gold },
            { "goldenrod", Color.Goldenrod },
            { "gray", Color.Gray },
            { "green", Color.Green },
            { "greenyellow", Color.GreenYellow },
            { "honeydew", Color.Honeydew },
            { "hotpink", Color.HotPink },
            { "indianred", Color.IndianRed },
            { "indigo", Color.Indigo },
            { "ivory", Color.Ivory },
            { "khaki", Color.Khaki },
            { "lavender", Color.Lavender },
            { "lavenderblush", Color.LavenderBlush },
            { "lawngreen", Color.LawnGreen },
            { "lemonchiffon", Color.LemonChiffon },
            { "lightblue", Color.LightBlue },
            { "lightcoral", Color.LightCoral },
            { "lightcyan", Color.LightCyan },
            { "lightgoldenrodyellow", Color.LightGoldenrodYellow },
            { "lightgray", Color.LightGray },
            { "lightgreen", Color.LightGreen },
            { "lightpink", Color.LightPink },
            { "lightsalmon", Color.LightSalmon },
            { "lightseagreen", Color.LightSeaGreen },
            { "lightskyblue", Color.LightSkyBlue },
            { "lightslategray", Color.LightSlateGray },
            { "lightsteelblue", Color.LightSteelBlue },
            { "lightyellow", Color.LightYellow },
            { "lime", Color.Lime },
            { "limegreen", Color.LimeGreen },
            { "linen", Color.Linen },
            { "magenta", Color.Magenta },
            { "maroon", Color.Maroon },
            { "mediumaquamarine", Color.MediumAquamarine },
            { "mediumblue", Color.MediumBlue },
            { "mediumorchid", Color.MediumOrchid },
            { "mediumpurple", Color.MediumPurple },
            { "mediumseagreen", Color.MediumSeaGreen },
            { "mediumslateblue", Color.MediumSlateBlue },
            { "mediumspringgreen", Color.MediumSpringGreen },
            { "mediumturquoise", Color.MediumTurquoise },
            { "mediumvioletred", Color.MediumVioletRed },
            { "midnightblue", Color.MidnightBlue },
            { "mintcream", Color.MintCream },
            { "mistyrose", Color.MistyRose },
            { "moccasin", Color.Moccasin },
            { "navajowhite", Color.NavajoWhite },
            { "navy", Color.Navy },
            { "oldlace", Color.OldLace },
            { "olive", Color.Olive },
            { "olivedrab", Color.OliveDrab },
            { "orange", Color.Orange },
            { "orangered", Color.OrangeRed },
            { "orchid", Color.Orchid },
            { "palegoldenrod", Color.PaleGoldenrod },
            { "palegreen", Color.PaleGreen },
            { "paleturquoise", Color.PaleTurquoise },
            { "palevioletred", Color.PaleVioletRed },
            { "papayawhip", Color.PapayaWhip },
            { "peachpuff", Color.PeachPuff },
            { "peru", Color.Peru },
            { "pink", Color.Pink },
            { "plum", Color.Plum },
            { "powderblue", Color.PowderBlue },
            { "purple", Color.Purple },
            { "red", Color.Red },
            { "rosybrown", Color.RosyBrown },
            { "royalblue", Color.RoyalBlue },
            { "saddlebrown", Color.SaddleBrown },
            { "salmon", Color.Salmon },
            { "sandybrown", Color.SandyBrown },
            { "seagreen", Color.SeaGreen },
            { "seashell", Color.SeaShell },
            { "sienna", Color.Sienna },
            { "silver", Color.Silver },
            { "skyblue", Color.SkyBlue },
            { "slateblue", Color.SlateBlue },
            { "slategray", Color.SlateGray },
            { "snow", Color.Snow },
            { "springgreen", Color.SpringGreen },
            { "steelblue", Color.SteelBlue },
            { "tan", Color.Tan },
            { "teal", Color.Teal },
            { "thistle", Color.Thistle },
            { "tomato", Color.Tomato },
            { "turquoise", Color.Turquoise },
            { "violet", Color.Violet },
            { "wheat", Color.Wheat },
            { "white", Color.White },
            { "whitesmoke", Color.WhiteSmoke },
            { "yellow", Color.Yellow },
            { "yellowgreen", Color.YellowGreen }
        };

        #endregion

        public static string GetNumberFormat<T>(T value, NumericFormat numericFormat) where T : INumber<T>
        {
            switch (numericFormat)
            {
                case NumericFormat.Decimal0: return $"{value:0}";
                case NumericFormat.Decimal1: return $"{value:0.0}";
                case NumericFormat.Decimal2: return $"{value:0.00}";
                case NumericFormat.Decimal3: return $"{value:0.000}";
                case NumericFormat.Decimal4: return $"{value:0.0000}";
                case NumericFormat.Decimal5: return $"{value:0.00000}";
                case NumericFormat.Decimal6: return $"{value:0.000000}";
                default: return $"{value}";
            }
        }


        // if there is html encoding - decode
        public static ReadOnlySpan<char> HtmlDecode(string value)
        {
            return SanitizeString(WebUtility.HtmlDecode(value));
        }

        public static ReadOnlySpan<char> SanitizeString(ReadOnlySpan<char> val)
        {
            // reinit
            _stringBuilder.Clear();

            // prevent duplicate spaces
            bool lastSpace = false;

            for (int i = 0; i < val.Length; i++)
            {
                char c = val[i];

                // \t, \r, \n, SPACE etc
                if (char.IsControl(c) || c == ' ')
                {
                    if (!lastSpace)
                    {
                        _stringBuilder.Append(' ');
                        lastSpace = true;
                    }

                    continue;
                }

                lastSpace = false;

                _stringBuilder.Append(c);
            }

            return _stringBuilder.ToString().Trim();
        }

        #region Parsing

        // float - use invariant culture (uses '.' as decimal separator)
        public static bool TryParseFloat(ReadOnlySpan<char> val, out float floatResult)
        {
            return float.TryParse(val, CultureInfo.InvariantCulture, out floatResult);
        }

        // float array - separated with separator (normally spaces)
        public static bool TryParseFloatArray(ReadOnlySpan<char> val, char[] separators, out ReadOnlySpan<float> floatArray)
        {
            string[] split = val.ToString().Split(separators, StringSplitOptions.RemoveEmptyEntries);

            if(split.Length > 0)
            {
                float[] res = new float[split.Length];

                for (int i = 0; i < split.Length; i++)
                {
                    if (TryParseFloat(split[i], out float floatResult))
                    {
                        res[i] = floatResult;
                    }
                }

                floatArray = res;
                return true;
            }

            floatArray = default;
            return false;
        }

        // number + unit
        public static bool TryParseNumberUnit(ReadOnlySpan<char> val, out float result, out bool isPercent)
        {
            // default
            isPercent = false;

            if (val.EndsWith("%"))
            {
                if (TryParseFloat(val.Slice(0, val.Length - 1), out float floatResult))
                {
                    isPercent = true;
                    result = floatResult;
                    return true;
                }
            }
            else if (val.Length <= 2)
            {
                // too short
                result = default;
                return false;
            }

            // try units - get last 2 chars
            Span<char> unitTest = new char[2];
            int res = val.Slice(val.Length - 2, 2).ToLowerInvariant(unitTest);

            for (int i = 0; i < _units.Length; i++)
            {
                if (Equals(unitTest, _units[i]))
                {
                    // found match
                    if (TryParseFloat(val.Slice(0, val.Length - 2), out float floatResult))
                    {
                        result = floatResult;
                        return true;
                    }
                    // no need to test more
                    break;
                }
            }

            result = default;
            return false;
        }

        public static bool TryParseLineCap(ReadOnlySpan<char> val, out LineCap? lineCap)
        {
            // ignore case
            if (Enum.TryParse(val, true, out LineCap svgLineCap))
            {
                lineCap = svgLineCap;
                return true;
            }

            lineCap = null;
            return false;
        }

        // color: hexstring, named color, rgb(...), rgba(...)
        // todo: hsl(), hsla()
        public static bool TryParseColor(ReadOnlySpan<char> val, out Color? color)
        {
            // format: "#ff00AA"
            if (IsHexString(val))
            {
                byte[] bytes = Convert.FromHexString(val.Slice(1));

                color = new Color(bytes[0], bytes[1], bytes[2]);
                return true;
            }
            else if(_namedColors.TryGetValue(val.ToString().ToLower(), out Color nameColor))
            {
                color = nameColor;
                return true;
            }
            else
            {
                // try rbg(...), rgba(...)
                string[] split = val.ToString().ToLower().Split(new char[] {',', ' ', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                
                // check length & first string
                if (split.Length == 4 && split[0] == "rgb")
                {
                    byte[] bytes = new byte[3];

                    bool success = true;

                    for (int i = 0; i < bytes.Length; i++)
                    {
                        if(!byte.TryParse(split[i + 1], out bytes[i]))
                        {
                            success = false;
                            break;
                        }
                    }

                    if (success)
                    {
                        color = new Color(bytes[0], bytes[1], bytes[2]);
                        return true;
                    }
                }
                else if (split.Length == 5 && split[0] == "rgba")
                {
                    byte[] bytes = new byte[3];

                    bool success = true;

                    for (int i = 0; i < bytes.Length; i++)
                    {
                        if (!byte.TryParse(split[i + 1], out bytes[i]))
                        {
                            success = false;
                            break;
                        }
                    }

                    // note: alpha is float
                    if (success && TryParseFloat(split[4], out float alpha))
                    {
                        color = new Color(bytes[0], bytes[1], bytes[2], alpha);
                        return true;
                    }
                }
            }

            // not color
            color = null;
            return false;
        }


        #endregion

        public static bool Equals(ReadOnlySpan<char> a, ReadOnlySpan<char> b)
        {
            if (a.Length != b.Length)
                return false;

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    return false;
            }
            return true;
        }

        // "#ff0000"
        public static bool IsHexString(ReadOnlySpan<char> val)
        {
            if(val.Length != 7 || !val.StartsWith("#"))
            {
                return false;
            }

            // loop
            for (int i = 1; i < val.Length; i++)
            {
                char c = val[i];

                if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))
                {
                    // valid char
                    continue;
                }
                else
                {
                    // invalid char
                    return false;
                }
            }

            return true;
        }
    }
}
