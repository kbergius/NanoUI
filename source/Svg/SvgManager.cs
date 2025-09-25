using NanoUI.Common;
using NanoUI.Nvg;
using NanoUI.Svg.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Xml;

namespace NanoUI.Svg
{
    /// <summary>
    /// SvgManager parses & creates svg objects.
    /// </summary>
    internal static partial class SvgManager
    {
        // todo: <svg> & <g> can have nested main elements and have also common attributes
        // that has effect in nested elements
        static string[] _xmlElements = {
            "svg", "defs","linearGradient", "radialGradient", "stop", "g", "path",
            "rect", "circle", "ellipse", "line", "polyline", "polygon" };

        // here we collect parent elements (<g> etc)
        static ArrayBuffer<SvgXmlElement> _parentElements = new();

        // this is the result, where we collect all path info & common params from <svg> & <defs> elements
        static SvgShape _svgShape;

        // this is the result where we collect all paths
        static ArrayBuffer<SvgPath> _svgPaths = new();

        // we collect all resources that can be used in SvgPaths <id, (resource, resourceType)>
        // this is needed for example with gradients
        static Dictionary<string, (object, Type)> _svgResources = new();

        public static SvgShape CreateSvg(NvgContext ctx, Stream stream)
        {
            // clear parents & result
            _svgResources.Clear();
            _parentElements.Clear();
            _svgPaths.Clear();
            _svgShape.Reset();

            // parse & create svg paths with commands
            ParseXML(ctx, stream);

            // we got all paths collected
            _svgShape.Paths = _svgPaths.AsReadOnlySpan().ToArray();

            (Vector2 min, Vector2 max) bounds = CalculatePathsBounds(_svgShape.Paths);
            _svgShape.PathsMin = bounds.min;
            _svgShape.PathsMax = bounds.max;

            return _svgShape;
        }

        static void ParseXML(NvgContext ctx, Stream stream)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            // we don't allow any external dtd processing
            settings.DtdProcessing = DtdProcessing.Ignore;

            // remove namespaces
            string[] split;
            string nodeName;

            // we need to get clean state for all paths
            ctx.SaveState();

            using (XmlReader reader = XmlReader.Create(stream, settings))
            {
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            // remove namespace from element name
                            split = reader.Name.Split(":");
                            nodeName = split[split.Length - 1];

                            // ignore not valid elements
                            if (_xmlElements.Contains(nodeName))
                            {
                                // we use same NvgState struct & reset it for the "clean" state
                                // before creating paths
                                // note: not all elements create path, but it is easier to have it here
                                ctx.ResetState();

                                // we read all valid elements attributes
                                ParseXmlAttributes(reader);

                                switch (nodeName)
                                {
                                    case "svg":
                                        // this is not implemented
                                        CreateSvg();
                                        break;
                                    case "defs":
                                        // this is not implemented
                                        CreateDefs();
                                        break;
                                    case "linearGradient":
                                        // this is not fully implemented
                                        StoreLinearGradient();
                                        break;
                                    case "radialGradient":
                                        // this is not fully implemented
                                        StoreRadialGradient();
                                        break;
                                    case "stop":
                                        // we are inside gradient (linear, radial)
                                        // this is partly implemented
                                        CreateGradientStop();
                                        break;
                                    case "g":
                                        StoreG();
                                        break;
                                    case "path":
                                        CreatePath(ctx);
                                        break;
                                    case "rect":
                                        CreateRect(ctx);
                                        break;
                                    case "circle":
                                        CreateCircle(ctx);
                                        break;
                                    case "ellipse":
                                        CreateEllipse(ctx);
                                        break;
                                    case "line":
                                        CreateLine(ctx);
                                        break;
                                    case "polyline":
                                        CreatePolyline(ctx);
                                        break;
                                    case "polygon":
                                        CreatePolygon(ctx);
                                        break;
                                }
                            }
                            break;
                        case XmlNodeType.Text:
                            // note: we don't support any inner text currently (no support for <text>!)
                            break;
                        case XmlNodeType.EndElement:
                            // remove namespace from element name
                            split = reader.Name.Split(":");
                            nodeName = split[split.Length - 1];

                            // ignore not valid elements
                            if (_xmlElements.Contains(nodeName))
                            {
                                switch (nodeName)
                                {
                                    case "svg":
                                        // no need to do anything
                                        break;
                                    case "defs":
                                        // no need to do anything
                                        break;
                                    case "linearGradient":
                                        if (_linearGradient != null && !string.IsNullOrEmpty(_linearGradient.Value.id))
                                        {
                                            // store for later use
                                            var gradient = _linearGradient.Value;
                                            _svgResources[gradient.id] = (gradient, typeof(SvgLinearGradient));

                                            // don't need anymore
                                            _linearGradient = null;
                                        }
                                        break;
                                    case "radialGradient":
                                        if (_radialGradient != null && !string.IsNullOrEmpty(_radialGradient.Value.id))
                                        {
                                            // store for later use
                                            var gradient = _radialGradient.Value;
                                            _svgResources[gradient.id] = (gradient, typeof(SvgRadialGradient));

                                            // don't need anymore
                                            _radialGradient = null;
                                        }
                                        break;
                                    case "g":
                                        // remove last stored element
                                        _parentElements.RemoveLast();
                                        break;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            // restore state
            ctx.RestoreState();
        }

        // in case svg xml does not define size correctly, we calculate paths size
        internal static (Vector2, Vector2) CalculatePathsBounds(ReadOnlySpan<SvgPath> paths)
        {
            Vector2 min = new Vector2(float.MaxValue);
            Vector2 max = new Vector2(float.MinValue);

            foreach (SvgPath path in paths)
            {
                min = Vector2.Min(min, path.Bounds.Min);
                max = Vector2.Max(max, path.Bounds.Max);
            }

            return (min, max);
        }
    }
}
