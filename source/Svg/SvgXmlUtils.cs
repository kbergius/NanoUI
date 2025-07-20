using NanoUI.Common;
using NanoUI.Svg.Data;
using NanoUI.Utils;
using System;
using System.Linq;
using System.Numerics;
using System.Text;

namespace NanoUI.Svg
{
    // this is for path (<d>), style (<style>) & matrix (<transform>) processing
    // todo: we could sanitize all strings to be sure, before converting strings to objects?

    // Valid path commands:
    // M = moveto (move from one point to another point)
    // L = lineto (create a line)
    // H = horizontal lineto (create a horizontal line)
    // V = vertical lineto (create a vertical line)
    // C = curveto (create a curve)
    // S = smooth curveto (create a smooth curve)
    // Q = quadratic Bézier curve (create a quadratic Bézier curve)
    // T = smooth quadratic Bézier curveto (create a smooth quadratic Bézier curve)
    // A = elliptical Arc (create a elliptical arc)
    // Z = closepath (close the path)

    // Note: All of the commands above can also be expressed in lower case.
    // Upper case means absolutely positioned, lower case means relatively positioned.

    // d="m 131.73911,422.01626 c 0,146.85769 43.82213,215.39128 201.5818,141.96244 157.75968,-73.42885 188.43518,-107.69564 354.95926,78.32409 166.5241,186.01973 210.34624,244.76282 162.1419,-122.3814 -48.20435,-367.1442 -4.38221,34.26679 -131.46641,-24.47627 C 591.87149,436.70204 732.10231,191.93923 543.66715,187.04398 355.23198,182.14871 574.34264,265.36807 534.90271,368.16845 495.4628,470.96883 355.23198,627.61702 311.40985,475.8641 267.58772,324.11115 193.09009,333.90166 131.73911,422.01626 z"
    public static class SvgXmlUtils
    {
        // this is needed to wipe out  invalid letters
        static char[] _validPathCommandChars = { 'm', 'l', 'h', 'v', 'c', 's', 'q', 't', 'a', 'z' };

        static SvgXmlPathCommandType _currentCommandType = SvgXmlPathCommandType.NONE;
        static ArrayBuffer<float> _currentPathValues = new();
        // this is collection of all commands that we return after all done
        static ArrayBuffer<SvgXmlPathCommand> _pathCommands = new();

        #region Path

        public static SvgXmlPath ParsePath(ReadOnlySpan<char> val)
        {
            // clear values
            _currentCommandType = SvgXmlPathCommandType.NONE;
            _currentPathValues.Clear();
            _pathCommands.Clear();

            // parse input ready to be splitted
            val = ParseInputPath(val);

            // split with spaces & commas - to get commands & values
            string[] splitted = val.ToString().Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

            // absolute flag for last command
            bool lastAbsolute = false;

            // loop values
            for (int i = 0; i < splitted.Length; i++)
            {
                // try get command type
                if (TryGetCommandType(splitted[i], out SvgXmlPathCommandType commandType, out bool absolute))
                {
                    // we got new command!

                    // create & store previous command
                    // note: there could be no values
                    if (_currentCommandType != SvgXmlPathCommandType.NONE)
                    {
                        // create & store previous command with values
                        _pathCommands.Add(new SvgXmlPathCommand
                        {
                            CommandType = _currentCommandType,
                            Values = _currentPathValues.AsReadOnlySpan().ToArray(),
                            Absolute = lastAbsolute
                        });
                    }

                    // store absolute flag
                    lastAbsolute = absolute;

                    // clear values - set new commandtype
                    _currentCommandType = commandType;
                    _currentPathValues.Clear();
                }
                else if (StringUtils.TryParseFloat(splitted[i], out float floatResult))
                {
                    // add to values
                    _currentPathValues.Add(floatResult);
                }
                else
                {
                    // invalid string format!!
                }
            }

            // create & store last command
            // note: there could be no values, if the last command is close command
            if (_currentCommandType != SvgXmlPathCommandType.NONE)
            {
                // create & store last command with values
                _pathCommands.Add(new SvgXmlPathCommand
                {
                    CommandType = _currentCommandType,
                    Values = _currentPathValues.AsReadOnlySpan().ToArray(),
                    Absolute = lastAbsolute
                });
            }

            // return result
            return new SvgXmlPath { Commands = _pathCommands.AsReadOnlySpan().ToArray() };
        }

        #region Helpers

        // parse path to create SPACEs in correct places
        // note. it doesn't matter that there will be extra spaces,
        // since they are stripped out in string split method
        static string ParseInputPath(ReadOnlySpan<char> val)
        {
            StringBuilder sb = new();

            for (int i = 0; i < val.Length; i++)
            {
                char c = val[i];

                if (char.IsControl(c))
                {
                    // \r, \t, \n
                    sb.Append(' ');
                }
                else if(c == '-')
                {
                    // set space at start
                    sb.Append(' ').Append(c);
                }
                else if (char.IsLetter(c))
                {
                    if (_validPathCommandChars.Contains(char.ToLower(c)))
                    {
                        // add spaces around - add char as-is
                        sb.Append(' ').Append(c).Append(' ');
                    }
                    else
                    {
                        // shouldn't happen
                        sb.Append(' ');
                    }
                }
                else
                {
                    // just append
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        // todo: we may not need to trim & convert to lower since it is done in initial parse!
        static bool TryGetCommandType(string val, out SvgXmlPathCommandType commandType, out bool absolute)
        {
            commandType = SvgXmlPathCommandType.NONE;
            absolute = false;

            if (string.IsNullOrEmpty(val))
                return false;

            val = val.Trim();

            if (val.Length != 1)
                return false;

            // set absolute flag - upper case command letter?
            absolute = char.IsUpper(val[0]);

            switch (val)
            {
                case "M": // M = moveto (move from one point to another point)
                case "m":
                    commandType = SvgXmlPathCommandType.MoveTo;
                    return true;
                case "L": // L = lineto (create a line)
                case "l":
                    commandType = SvgXmlPathCommandType.LineTo;
                    return true;
                case "H": // H = horizontal lineto (create a horizontal line)
                case "h":
                    commandType = SvgXmlPathCommandType.HorizontalLineTo;
                    return true;
                case "V": // V = vertical lineto (create a vertical line)
                case "v":
                    commandType = SvgXmlPathCommandType.VerticalLineTo;
                    return true;
                case "C": // C = curveto (create a curve)
                case "c":
                    commandType = SvgXmlPathCommandType.CurveTo;
                    return true;
                case "S": // S = smooth curveto (create a smooth curve)
                case "s":
                    commandType = SvgXmlPathCommandType.SmoothCurveTo;
                    return true;
                case "Q": // Q = quadratic Bézier curve (create a quadratic Bézier curve)
                case "q":
                    commandType = SvgXmlPathCommandType.QuadraticBezier;
                    return true;
                case "T": // T = smooth quadratic Bézier curveto (create a smooth quadratic Bézier curve)
                case "t":
                    commandType = SvgXmlPathCommandType.SmoothQuadraticBezier;
                    return true;
                case "A": // A = elliptical Arc (create a elliptical arc)
                case "a":
                    commandType = SvgXmlPathCommandType.EllipticalArc;
                    return true;
                case "Z": // Z = closepath (close the path)
                case "z":
                    commandType = SvgXmlPathCommandType.ClosePath;
                    return true;
            }

            return false;
        }

        #endregion

        #endregion

        #region Style

        // style
        // todo: opacities, stroke-dasharray etc
        public static SvgStyle ParseStyle(ReadOnlySpan<char> val)
        {
            SvgStyle style = new();

            // first split attributes
            string[] attributes = val.ToString().Split(';');

            // loop attributes
            for (int i = 0; i < attributes.Length; i++)
            {
                // split name & value
                string[] nameValue = attributes[i].Split(':');

                // bad format!
                if (nameValue.Length != 2)
                    continue;

                // parse attribute name & value
                ParseStyleAttribute(ref style, nameValue[0].Trim().ToLower(), nameValue[1].Trim());
            }

            return style;
        }

        // note: we can come here either from style parsing or from individual style attribute
        // todo: handle "url( ...)" in fill & stroke (only relative)
        public static void ParseStyleAttribute(ref SvgStyle style, string attrName, ReadOnlySpan<char> attrValue)
        {
            switch (attrName)
            {
                case "opacity":
                    // can be percentage or absolute value
                    if (StringUtils.TryParseNumberUnit(attrValue, out float opacityResult, out bool _))
                    {
                        // we treat all values as percentage
                        style.Opacity = Math.Clamp(opacityResult / 100, 0, 1);
                    }
                    else if (StringUtils.TryParseFloat(attrValue, out float opacity))
                    {
                        style.Opacity = Math.Clamp(opacity, 0, 1);
                    }
                    break;
                case "fill":
                    // gradient
                    if (TryParseUrl(attrValue, out ReadOnlySpan<char> fillGradientId))
                    {
                        style.FillPaintId = fillGradientId.ToString();
                    }
                    // color
                    else if (StringUtils.TryParseColor(attrValue, out Color? fillColor))
                    {
                        style.FillColor = fillColor;
                    }
                    else
                    {
                        // "none", "transparent" etc
                        style.FillColor = Color.Transparent;
                    }
                    break;
                case "fill-opacity":
                    // can be percentage or absolute value
                    if (StringUtils.TryParseNumberUnit(attrValue, out float fillOpacityResult, out bool _))
                    {
                        // we treat all values as percentage
                        style.FillOpacity = Math.Clamp(fillOpacityResult / 100, 0, 1);
                    }
                    else if (StringUtils.TryParseFloat(attrValue, out float fillOpacity))
                    {
                        style.FillOpacity = Math.Clamp(fillOpacity, 0, 1);
                    }
                    break;
                case "stroke":
                    // gradient
                    if (TryParseUrl(attrValue, out ReadOnlySpan<char> strokeGradientId))
                    {
                        style.StrokePaintId = strokeGradientId.ToString();
                    }
                    // color
                    else if (StringUtils.TryParseColor(attrValue, out Color? strokeColor))
                    {
                        style.StrokeColor = strokeColor;
                    }
                    else
                    {
                        // "none", "transparent" etc
                        style.StrokeColor = Color.Transparent;
                    }
                    break;
                case "stroke-width":
                    // number-unit or float
                    // todo: should we store number unit in SvgStyle?
                    if (StringUtils.TryParseNumberUnit(attrValue, out float result, out bool isPercent))
                    {
                        style.StrokeWidth = result;
                    }
                    else if (StringUtils.TryParseFloat(attrValue, out float strokeWidth))
                    {
                        style.StrokeWidth = strokeWidth;
                    }
                    else
                    {
                        style.StrokeWidth = 1;
                    }
                    break;
                case "stroke-linecap":
                    if(StringUtils.TryParseLineCap(attrValue, out LineCap? strokeLineCap))
                    {
                        style.StrokeLineCap = strokeLineCap;
                    }
                    break;
                case "stroke-linejoin":
                    if (StringUtils.TryParseLineCap(attrValue, out LineCap? strokeLineJoin))
                    {
                        style.StrokeLineJoin = strokeLineJoin;
                    }
                    break;
                case "stroke-opacity":
                    // can be percentage or absolute value
                    if (StringUtils.TryParseNumberUnit(attrValue, out float strokeOpacityResult, out bool _))
                    {
                        // we treat all values as percentage
                        style.StrokeOpacity = Math.Clamp(strokeOpacityResult / 100, 0, 1);
                    }
                    else if (StringUtils.TryParseFloat(attrValue, out float strokeOpacity))
                    {
                        style.StrokeOpacity = Math.Clamp(strokeOpacity, 0, 1);
                    }
                    break;
                case "stroke-miterlimit":
                    if (StringUtils.TryParseFloat(attrValue, out float strokeMiterLimit))
                    {
                        style.StrokeMiterLimit = strokeMiterLimit;
                    }
                    break;
                case "stroke-dasharray":
                    if (StringUtils.TryParseFloatArray(attrValue, [' '], out ReadOnlySpan<float> strokeDashArray))
                    {
                        style.StrokeDashArray = strokeDashArray.ToArray();
                    }
                    break;
                case "stroke-dashoffset":
                    if (StringUtils.TryParseFloat(attrValue, out float strokeDashOffset))
                    {
                        style.StrokeDashOffset = strokeDashOffset;
                    }
                    break;
            }
        }

        // we don't support any url's that points to some external resource (http, https etc),
        // so url is just relative link (id) to any resource that is already defined & stored in
        // SvgManager's id, object dictionary
        // example: "url(#grad1)"
        internal static bool TryParseUrl(ReadOnlySpan<char> attrValue, out ReadOnlySpan<char> id)
        {
            if (!attrValue.StartsWith("url(#"))
            {
                id = string.Empty;
                return false;
            }

            id = attrValue.Slice(5, attrValue.Length - 6).Trim();
            return true;
        }

        #endregion

        #region Transform

        // todo: can there bw many transform functions?
        public static bool TryParseTransform(ReadOnlySpan<char> val, out Matrix3x2 transform)
        {
            string[] splitted = val.ToString().Split([ '(', ')', ',', ' ' ], StringSplitOptions.RemoveEmptyEntries);

            // todo: check splitted length > 1

            // parse to float array - don't take first
            float[] floats = new float[splitted.Length - 1];

            for (int i = 1; i < splitted.Length; i++)
            {
                if (StringUtils.TryParseFloat(splitted[i], out float floatResult))
                {
                    floats[i - 1] = floatResult;
                }
            }

            switch (splitted[0].ToLower())
            {
                case "translate":
                    if (floats.Length == 2)
                    {
                        transform = Matrix3x2.CreateTranslation(floats[0], floats[1]);
                        return true;
                    }
                    break;
                case "scale":
                    if (floats.Length == 1)
                    {
                        transform = Matrix3x2.CreateScale(floats[0]);
                        return true;
                    }
                    else if (floats.Length == 2)
                    {
                        transform = Matrix3x2.CreateScale(floats[0], floats[1]);
                        return true;
                    }
                    break;
                case "rotate":
                    if (floats.Length == 1)
                    {
                        // note: value is degrees
                        transform = Matrix3x2.CreateRotation(float.DegreesToRadians(floats[0]));
                        return true;
                    }
                    break;
                case "skewx":
                    if (floats.Length == 1)
                    {
                        // note: value is degrees
                        transform = Matrix3x2.CreateSkew(float.DegreesToRadians(floats[0]), 0);
                        return true;
                    }
                    break;
                case "skewy":
                    if (floats.Length == 1)
                    {
                        // note: value is degrees
                        transform = Matrix3x2.CreateSkew(0, float.DegreesToRadians(floats[0]));
                        return true;
                    }
                    break;
                case "matrix":
                    if (floats.Length == 6)
                    {
                        transform = new Matrix3x2(floats[0], floats[1], floats[2], floats[3], floats[4], floats[5]);
                        return true;
                    }
                    break;
            }

            transform = Matrix3x2.Identity;
            return false;
        }

        #endregion
    }
}