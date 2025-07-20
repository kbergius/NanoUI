using NanoUI.Common;
using NanoUI.Svg.Data;
using NanoUI.Utils;
using System;
using System.Numerics;
using System.Xml;

namespace NanoUI.Svg
{
    // attributes handling
    static partial class SvgManager
    {
        // collect all xml attributes here
        static ArrayBuffer<SvgXmlAttribute> _xmlAttributes = new();

        static void ParseXmlAttributes(XmlReader reader)
        {
            string[] split;
            string attribName;

            // clear previous
            _xmlAttributes.Clear();

            for (int i = 0; i < reader.AttributeCount; i++)
            {
                reader.MoveToAttribute(i);

                // ignore namespaces
                if (reader.Name.StartsWith("xmlns"))
                    continue;

                // remove namespace from attribute name
                split = reader.Name.Split(":");
                attribName = split[split.Length - 1];

                // store the value & type (how we cast)
                StoreXmlAttributeValue(attribName, StringUtils.SanitizeString(reader.Value));
            }
        }

        // we support both combined "style" and individual "fill", "stroke", "stroke-width" etc attributes
        static void StoreXmlAttributeValue(ReadOnlySpan<char> attribName, ReadOnlySpan<char> attrValue)
        {
            // first attributes that we know exactly how to parse
            switch (attribName.ToString().ToLower())
            {
                case "style":
                    AddXmlAttribute(attribName, SvgXmlUtils.ParseStyle(attrValue), typeof(SvgStyle));
                    return;
                case "transform":
                    if (SvgXmlUtils.TryParseTransform(attrValue, out Matrix3x2 transform))
                    {
                        AddXmlAttribute(attribName, transform, typeof(Matrix3x2));
                        return;
                    }
                    break;
                case "d":
                    AddXmlAttribute(attribName, SvgXmlUtils.ParsePath(attrValue), typeof(SvgXmlPath));
                    return;
                case "points":
                    // polyline, polygon
                    // points="0,0 50,150 100,75 150,50 200,140 250,140"
                    string[] split = attrValue.ToString().Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    float[] values = new float[split.Length];

                    for (int i = 0; i < split.Length; i++)
                    {
                        if (StringUtils.TryParseFloat(split[i], out float point))
                        {
                            values[i] = point;
                        }
                    }

                    AddXmlAttribute(attribName, new SvgXmlPoints { Points = values }, typeof(SvgXmlPoints));
                    return;
                // separate styles - same as in <style>
                // todo: not all are implemented in drawing
                case "opacity":
                case "fill":
                case "fill-opacity":
                case "stroke":
                case "stroke-width":
                case "stroke-linecap":
                case "stroke-linejoin":
                case "stroke-opacity":
                case "stroke-miterlimit":
                case "stroke-dasharray":
                case "stroke-dashoffset":
                    // we create style & combine separate styles to one later
                    SvgStyle style = new();

                    SvgXmlUtils.ParseStyleAttribute(ref style, attribName.ToString().ToLower(), attrValue);

                    AddXmlAttribute("style", style, typeof(SvgStyle));
                    return;
            }

            // we don't know exactly how to parse - so test different methods
            if (StringUtils.TryParseNumberUnit(attrValue, out float result, out bool isPercent))
            {
                // number + unit
                // note: we parse all units out
                AddXmlAttribute(attribName, result, typeof(float), isPercent);
            }
            else if (StringUtils.TryParseFloat(attrValue, out float floatResult))
            {
                // float
                AddXmlAttribute(attribName, floatResult, typeof(float));
            }
            else if (StringUtils.TryParseColor(attrValue, out Color? color))
            {
                // color
                AddXmlAttribute(attribName, color!.Value, typeof(Color));
            }
            else
            {
                // string
                AddXmlAttribute(attribName, attrValue.ToString(), typeof(string));
            }
        }

        static void AddXmlAttribute(ReadOnlySpan<char> attribName, object attribValue, Type valueType, bool isPercent = false)
        {
            _xmlAttributes.Add(new SvgXmlAttribute
            {
                Name = attribName.ToString(),
                Value = attribValue,
                AttributeType = valueType,
                IsPercent = isPercent
            });
        }
    }
}