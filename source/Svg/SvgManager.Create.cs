using NanoUI.Common;
using Color = NanoUI.Common.Color;
using NanoUI.Nvg;
using NanoUI.Svg.Data;
using System;
using System.Numerics;
using NanoUI.Nvg.Data;

namespace NanoUI.Svg
{
    // xml parsing is done & attributes collected, so create svg structs
    static partial class SvgManager
    {
        // we store temporarily, since we wait stops & end tag
        static SvgLinearGradient? _linearGradient;
        static SvgRadialGradient? _radialGradient;

        #region Svg

        static void CreateSvg()
        {
            bool isPercent = false;
            float width = 0;
            float height = 0;
            string? docname = null;

            // collect values
            foreach (var attr in _xmlAttributes.AsReadOnlySpan())
            {
                switch (attr.Name)
                {
                    case "width":
                        width = attr.GetNumber(out isPercent);
                        break;
                    case "height":
                        height = attr.GetNumber(out isPercent);
                        break;
                    case "docname":
                        docname = attr.GetValue<string>();
                        break;
                }
            }

            // store
            _svgShape.Size = new Vector2(width, height);
        }

        #endregion

        #region Defs

        static void CreateDefs()
        {
            // todo
        }

        #endregion

        #region LinearGradient

        // note: we treat all values as percentages
        static void StoreLinearGradient()
        {
            // default values
            bool isPercent = false;
            string? id = null; // this is required
            float x1 = 0;
            float x2 = 100;
            float y1 = 0;
            float y2 = 0;

            // collect linear gradient values
            foreach (var attr in _xmlAttributes.AsReadOnlySpan())
            {
                switch (attr.Name)
                {
                    case "id":
                        id = attr.GetValue<string>();
                        break;
                    case "x1":
                        x1 = attr.GetNumber(out isPercent);
                        break;
                    case "y1":
                        y1 = attr.GetNumber(out isPercent);
                        break;
                    case "x2":
                        x2 = attr.GetNumber(out isPercent);
                        break;
                    case "y2":
                        y2 = attr.GetNumber(out isPercent);
                        break;
                }
            }

            if(id != null)
            {
                _linearGradient = new SvgLinearGradient
                {
                    id = id,
                    x1 = x1,
                    y1 = y1,
                    x2 = x2,
                    y2 = y2,
                };
            }
        }

        #endregion

        #region RadialGradient

        // note: we treat all values as percentages
        static void StoreRadialGradient()
        {
            // default values
            bool isPercent = false;
            string? id = null;
            float cx = 50;
            float cy = 50;
            float fr = 0;
            float fx = 50;
            float fy = 50;
            float r = 50;

            // collect linear gradient values
            foreach (var attr in _xmlAttributes.AsReadOnlySpan())
            {
                switch (attr.Name)
                {
                    case "id":
                        id = attr.GetValue<string>();
                        break;
                    case "cx":
                        cx = attr.GetNumber(out isPercent);
                        break;
                    case "cy":
                        cy = attr.GetNumber(out isPercent);
                        break;
                    case "fx":
                        fx = attr.GetNumber(out isPercent);
                        break;
                    case "fy":
                        fy = attr.GetNumber(out isPercent);
                        break;
                    case "fr":
                        fr = attr.GetNumber(out isPercent);
                        break;
                    case "r":
                        r = attr.GetNumber(out isPercent);
                        break;
                }
            }

            if (id != null)
            {
                _radialGradient = new SvgRadialGradient
                {
                    id = id,
                    cx = cx,
                    cy = cy,
                    fx = fx,
                    fy = fy,
                    r = r,
                };
            }
        }

        #endregion

        #region GradientStop

        static void CreateGradientStop()
        {
            // defaults
            bool isPercent = false;
            float offset = 0;
            Color stopColor = Color.Black;
            float stopOpacity = 1;

            // collect stop values
            foreach (var attr in _xmlAttributes.AsReadOnlySpan())
            {
                switch (attr.Name)
                {
                    case "offset":
                        offset = attr.GetNumber(out isPercent);
                        break;
                    case "stop-color":
                        stopColor = attr.GetValue<Color>();
                        break;
                    case "stop-opacity":
                        stopOpacity = attr.GetNumber(out isPercent);
                        break;
                }
            }

            // create stop
            var stop = new SvgGradientStop
            {
                Offset = offset,
                StopColor = stopColor,
                StopOpacity = stopOpacity,
                IsPercent = isPercent
            };

            // now store to linear/radial gradient
            if (_linearGradient != null)
            {
                var gradient = _linearGradient.Value;

                if(gradient.Stops == null)
                {
                    gradient.Stops = new SvgGradientStop[1];
                }
                else
                {
                    Array.Resize(ref gradient.Stops, gradient.Stops.Length + 1);
                }

                gradient.Stops[gradient.Stops.Length - 1] = stop;

                _linearGradient = gradient;
            }
            else if (_radialGradient != null)
            {
                var gradient = _radialGradient.Value;

                if (gradient.Stops == null)
                {
                    gradient.Stops = new SvgGradientStop[1];
                }
                else
                {
                    Array.Resize(ref gradient.Stops, gradient.Stops.Length + 1);
                }

                gradient.Stops[gradient.Stops.Length - 1] = stop;

                _radialGradient = gradient;
            }
        }

        #endregion

        #region G

        static void StoreG()
        {
            SvgStyle? style = null;
            Matrix3x2? transform = null;

            string? id = null;

            // collect values
            foreach (var attr in _xmlAttributes.AsReadOnlySpan())
            {
                switch (attr.Name)
                {
                    case "style":
                        // combine with current style
                        style = attr.GetValue<SvgStyle>().Combine(style);
                        break;
                    case "transform":
                        transform = attr.GetValue<Matrix3x2>();
                        break;
                    case "id":
                        id = attr.GetValue<string>();
                        break;
                }
            }

            // id is optional
            // store
            _parentElements.Add(new SvgXmlElement
            {
                Id = id,
                Style = style,
                Transform = transform
            });
        }

        #endregion

        #region Path

        static void CreatePath(NvgContext ctx)
        {
            SvgStyle? style = null;
            Matrix3x2? transform = null;

            SvgXmlPathCommand[] commands = Array.Empty<SvgXmlPathCommand>();

            // collect values
            foreach (var attr in _xmlAttributes.AsReadOnlySpan())
            {
                switch (attr.Name)
                {
                    case "style":
                        // combine with current style
                        style = attr.GetValue<SvgStyle>().Combine(style);
                        break;
                    case "transform":
                        transform = attr.GetValue<Matrix3x2>();
                        break;
                    case "d":
                        var xmlPath = attr.GetValue<SvgXmlPath>();
                        commands = xmlPath.Commands;
                        break;
                }
            }

            // first must be move to
            if(commands.Length > 0 && commands[0].CommandType == SvgXmlPathCommandType.MoveTo)
            {
                // we must set final values (take into account parent elements) before really processing
                CreateFinalStyleTransform(ref style, ref transform);

                ctx.BeginPath();

                // set transform to state
                SetTransform(ctx, transform);

                for (int i = 0; i < commands.Length; i++)
                {
                    SvgXmlPathCommand command = commands[i];

                    switch (command.CommandType)
                    {
                        case SvgXmlPathCommandType.MoveTo:
                            // (x y)+
                            if (command.Values.Length > 0 && command.Values.Length % 2 == 0)
                            {
                                for (int j = 0; j < command.Values.Length; j += 2)
                                {
                                    AddMoveTo(ctx, command.Values[j], command.Values[j + 1], command.Absolute);
                                }
                            }
                            break;
                        case SvgXmlPathCommandType.LineTo:
                            // (x y)+
                            if (command.Values.Length > 0 && command.Values.Length % 2 == 0)
                            {
                                for (int j = 0; j < command.Values.Length; j += 2)
                                {
                                    AddLineTo(ctx, command.Values[j], command.Values[j + 1], command.Absolute);
                                }
                            }
                            break;
                        case SvgXmlPathCommandType.HorizontalLineTo:
                            // x+
                            if (command.Values.Length > 0)
                            {
                                // 1 param x
                                for (int j = 0; j < command.Values.Length; j++)
                                {
                                    AddHorizontalLineTo(ctx, command.Values[j], command.Absolute);
                                }
                            }
                            break;
                        case SvgXmlPathCommandType.VerticalLineTo:
                            // y+
                            if (command.Values.Length > 0)
                            {
                                // 1 param y
                                for (int j = 0; j < command.Values.Length; j++)
                                {
                                    AddVerticalLineTo(ctx, command.Values[j], command.Absolute);
                                }
                            }
                            break;
                        case SvgXmlPathCommandType.CurveTo:
                            // (x1 y1 x2 y2 x y)+
                            if (command.Values.Length > 0 && command.Values.Length % 6 == 0)
                            {
                                for(int j = 0; j < command.Values.Length; j += 6)
                                {
                                    AddCurveTo(ctx,
                                        command.Values[j], command.Values[j + 1],
                                        command.Values[j + 2], command.Values[j + 3],
                                        command.Values[j + 4], command.Values[j + 5],
                                        command.Absolute);
                                }
                            }
                            break;
                        case SvgXmlPathCommandType.SmoothCurveTo:
                            // (x2 y2 x y)+
                            if (command.Values.Length > 0 && command.Values.Length % 4 == 0)
                            {
                                for (int j = 0; j < command.Values.Length; j += 4)
                                {
                                    AddSmoothCurveTo(ctx,
                                        command.Values[j], command.Values[j + 1],
                                        command.Values[j + 2], command.Values[j + 3],
                                        command.Absolute);
                                }
                            }
                            break;
                        case SvgXmlPathCommandType.QuadraticBezier:
                            // (x1 y1 x y)+
                            if (command.Values.Length > 0 && command.Values.Length % 4 == 0)
                            {
                                for (int j = 0; j < command.Values.Length; j += 4)
                                {
                                    AddQuadraticBezier(ctx,
                                        command.Values[j], command.Values[j + 1],
                                        command.Values[j + 2], command.Values[j + 3],
                                        command.Absolute);
                                }
                            }
                            break;
                        case SvgXmlPathCommandType.SmoothQuadraticBezier:
                            // (x y)+
                            if (command.Values.Length > 0 && command.Values.Length % 2 == 0)
                            {
                                for (int j = 0; j < command.Values.Length; j += 2)
                                {
                                    AddSmoothQuadraticBezier(ctx,
                                        command.Values[j], command.Values[j + 1],
                                        command.Absolute);
                                }
                            }
                            break;
                        case SvgXmlPathCommandType.EllipticalArc:
                            // (rx ry x-axis-rotation large-arc-flag sweep-flag x y)+
                            if (command.Values.Length > 0 && command.Values.Length % 7 == 0)
                            {
                                for (int j = 0; j < command.Values.Length; j += 7)
                                {
                                    AddEllipticalArc(ctx,
                                        command.Values[j],
                                        command.Values[j + 1],
                                        command.Values[j + 2],
                                        command.Values[j + 3],
                                        command.Values[j + 4],
                                        command.Values[j + 5],
                                        command.Values[j + 6],
                                        command.Absolute);
                                }
                            }
                            break;
                        case SvgXmlPathCommandType.ClosePath:
                            ctx.ClosePath();
                            break;
                    } // end switch
                } // end loop

                // create & store svg path
                StoreSvgPath(ctx, style);
            } // end if
        }

        #endregion

        #region Rect

        static void CreateRect(NvgContext ctx)
        {
            SvgStyle? style = null;
            Matrix3x2? transform = null;

            // The x-position for the top-left corner of the rectangle
            float x = 0;
            // The y-position for the top-left corner of the rectangle
            float y = 0;
            // The width of the rectangle
            float width = 0;
            // The height of the rectangle
            float height = 0;
            // The x radius of the corners of the rectangle (used to round the corners)
            float rx = 0;
            // The y radius of the corners of the rectangle (used to round the corners)
            float ry = 0;

            // collect values
            foreach (var attr in _xmlAttributes.AsReadOnlySpan())
            {
                switch (attr.Name)
                {
                    case "style":
                        // combine with current style
                        style = attr.GetValue<SvgStyle>().Combine(style);
                        break;
                    case "transform":
                        transform = attr.GetValue<Matrix3x2>();
                        break;
                    case "x":
                        x = attr.GetValue<float>();
                        break;
                    case "y":
                        y = attr.GetValue<float>();
                        break;
                    case "width":
                        width = attr.GetValue<float>();
                        break;
                    case "height":
                        height = attr.GetValue<float>();
                        break;
                    case "rx":
                        rx = attr.GetValue<float>();
                        break;
                    case "ry":
                        ry = attr.GetValue<float>();
                        break;
                }
            }

            // create
            if (width > 0 && height > 0)
            {
                // we must set final values (take into account parent elements) before really processing
                CreateFinalStyleTransform(ref style, ref transform);

                ctx.BeginPath();

                // set transform to state
                SetTransform(ctx, transform);

                if (rx == 0 && ry == 0)
                {
                    // normal rect
                    ctx.Rect(x, y, width, height);
                }
                else
                {
                    // rounded rect
                    ctx.RoundedRect(x, y, width, height, rx > ry ? rx : ry);
                }

                // create & store svg path
                StoreSvgPath(ctx, style);
            }
        }

        #endregion

        #region Circle

        static void CreateCircle(NvgContext ctx)
        {
            SvgStyle? style = null;
            Matrix3x2? transform = null;

            // The radius of the circle
            float r = 0;
            // The x-axis center of the circle
            float cx = 0;
            // The y-axis center of the circle
            float cy = 0;

            // collect values
            foreach (var attr in _xmlAttributes.AsReadOnlySpan())
            {
                switch (attr.Name)
                {
                    case "style":
                        // combine with current style
                        style = attr.GetValue<SvgStyle>().Combine(style);
                        break;
                    case "transform":
                        transform = attr.GetValue<Matrix3x2>();
                        break;
                    case "r":
                        r = attr.GetValue<float>();
                        break;
                    case "cx":
                        cx = attr.GetValue<float>();
                        break;
                    case "cy":
                        cy = attr.GetValue<float>();
                        break;
                }
            }

            if(r > 0)
            {
                // we must set final values (take into account parent elements) before really processing
                CreateFinalStyleTransform(ref style, ref transform);

                ctx.BeginPath();

                // set transform to state
                SetTransform(ctx, transform);

                ctx.Circle(cx, cy, r);

                // create & store svg path
                StoreSvgPath(ctx, style);
            }
        }

        #endregion

        #region Ellipse

        static void CreateEllipse(NvgContext ctx)
        {
            SvgStyle? style = null;
            Matrix3x2? transform = null;

            // The x radius of the ellipse
            float rx = 0;
            // The y radius of the ellipse
            float ry = 0;
            // The x-axis center of the ellipse
            float cx = 0;
            // The y-axis center of the ellipse
            float cy = 0;

            // collect values
            foreach (var attr in _xmlAttributes.AsReadOnlySpan())
            {
                switch (attr.Name)
                {
                    case "style":
                        // combine with current style
                        style = attr.GetValue<SvgStyle>().Combine(style);
                        break;
                    case "transform":
                        transform = attr.GetValue<Matrix3x2>();
                        break;
                    case "rx":
                        rx = attr.GetValue<float>();
                        break;
                    case "ry":
                        ry = attr.GetValue<float>();
                        break;
                    case "cx":
                        cx = attr.GetValue<float>();
                        break;
                    case "cy":
                        cy = attr.GetValue<float>();
                        break;
                }
            }

            if (rx > 0 && ry > 0)
            {
                // we must set final values (take into account parent elements) before really processing
                CreateFinalStyleTransform(ref style, ref transform);

                ctx.BeginPath();

                // set transform to state
                SetTransform(ctx, transform);

                ctx.Ellipse(cx, cy, rx, ry);

                // create & store svg path
                StoreSvgPath(ctx, style);
            }
        }

        #endregion

        #region Line

        static void CreateLine(NvgContext ctx)
        {
            SvgStyle? style = null;
            Matrix3x2? transform = null;

            // The start of the line on the x-axis
            float x1 = 0;
            // The start of the line on the y-axis
            float y1 = 0;
            // The end of the line on the x-axis
            float x2 = 0;
            // The end of the line on the y-axis
            float y2 = 0;

            // collect values
            foreach (var attr in _xmlAttributes.AsReadOnlySpan())
            {
                switch (attr.Name)
                {
                    case "style":
                        // combine with current style
                        style = attr.GetValue<SvgStyle>().Combine(style);
                        break;
                    case "transform":
                        transform = attr.GetValue<Matrix3x2>();
                        break;
                    case "x1":
                        x1 = attr.GetValue<float>();
                        break;
                    case "y1":
                        y1 = attr.GetValue<float>();
                        break;
                    case "x2":
                        x2 = attr.GetValue<float>();
                        break;
                    case "y2":
                        y2 = attr.GetValue<float>();
                        break;
                }
            }

            if (x1 != x2 || y1 != y2)
            {
                // we must set final values (take into account parent elements) before really processing
                CreateFinalStyleTransform(ref style, ref transform);

                ctx.BeginPath();

                // set transform to state
                SetTransform(ctx, transform);

                ctx.MoveTo(x1, y1);
                ctx.LineTo(x2, y2);

                // create & store svg path
                StoreSvgPath(ctx, style);
            }
        }

        #endregion

        #region Polyline

        static void CreatePolyline(NvgContext ctx)
        {
            SvgStyle? style = null;
            Matrix3x2? transform = null;

            // The list of points of the polyline. Each point must contain an x coordinate and a y coordinate
            float[] points = Array.Empty<float>();

            // collect values
            foreach (var attr in _xmlAttributes.AsReadOnlySpan())
            {
                switch (attr.Name)
                {
                    case "style":
                        // combine with current style
                        style = attr.GetValue<SvgStyle>().Combine(style);
                        break;
                    case "transform":
                        transform = attr.GetValue<Matrix3x2>();
                        break;
                    case "points":
                        var arr = attr.GetValue<SvgXmlPoints>();
                        points = arr.Points;
                        break;
                }
            }

            // must be even number of points (x, y)
            if(points.Length > 1 && points.Length % 2 == 0)
            {
                // we must set final values (take into account parent elements) before really processing
                CreateFinalStyleTransform(ref style, ref transform);

                ctx.BeginPath();

                // set transform to state
                SetTransform(ctx, transform);

                // first point
                ctx.MoveTo(points[0], points[1]);

                // loop
                for (var i = 2; i < points.Length; i += 2)
                {
                    ctx.LineTo(points[i], points[i + 1]);
                }

                // create & store svg path
                StoreSvgPath(ctx, style);
            }
        }

        #endregion

        #region Polygon

        static void CreatePolygon(NvgContext ctx)
        {
            SvgStyle? style = null;
            Matrix3x2? transform = null;

            // The list of points of the polygon. Each point must contain an x coordinate and a y coordinate
            float[] points = Array.Empty<float>();

            // collect values
            foreach (var attr in _xmlAttributes.AsReadOnlySpan())
            {
                switch (attr.Name)
                {
                    case "style":
                        // combine with current style
                        style = attr.GetValue<SvgStyle>().Combine(style);
                        break;
                    case "transform":
                        transform = attr.GetValue<Matrix3x2>();
                        break;
                    case "points":
                        var arr = attr.GetValue<SvgXmlPoints>();
                        points = arr.Points;
                        break;
                }
            }

            // must be even number of points (x, y)
            if (points.Length > 1 && points.Length % 2 == 0)
            {
                // we must set final values (take into account parent elements) before really processing
                CreateFinalStyleTransform(ref style, ref transform);

                ctx.BeginPath();

                // set transform to state
                SetTransform(ctx, transform);

                // first point
                ctx.MoveTo(points[0], points[1]);

                // loop
                for (var i = 2; i < points.Length; i += 2)
                {
                    ctx.LineTo(points[i], points[i + 1]);
                }

                // line to first point to close polygon
                ctx.LineTo(points[0], points[1]);

                // create & store svg path
                StoreSvgPath(ctx, style);
            }
        }

        #endregion

        #region Helpers

        // now we must create final style & transform taking into account stored/stacked SvgXmlElement's
        // style & transform
        static void CreateFinalStyleTransform(ref SvgStyle? current, ref Matrix3x2? currentTransform)
        {
            if (_parentElements.Count == 0)
            {
                // no modifications
                return;
            }

            // now loop stored/"stacked" elements backwards & combine
            for(int i = _parentElements.Count - 1; i >= 0; i--)
            {
                SvgStyle? parentStyle = _parentElements[i].Style;

                // combine
                if (parentStyle != null)
                {
                    if (current == null)
                    {
                        current = parentStyle;
                    }
                    else
                    {
                        current = parentStyle.Value.Combine(current);
                    }
                }

                // get + combine transforms?
                Matrix3x2? parentTransform = _parentElements[i].Transform;
                
                if (parentTransform != null)
                {
                    if(currentTransform == null)
                    {
                        currentTransform = parentTransform.Value;
                    }
                    else
                    {
                        // combine
                        currentTransform = parentTransform.Value * currentTransform;
                    }
                }
            }
        }

        static void SetTransform(NvgContext ctx, Matrix3x2? localTransform)
        {
            if (localTransform != null)
            {
                ctx.Transform(localTransform.Value);
            }
        }

        // note: we must create paths & points, so we can get bounds
        // bounds are used with gradients
        static void StoreSvgPath(NvgContext ctx, in SvgStyle? style)
        {
            // we don't store path, if we don't get valid style
            // todo: we could have more specific null checks (fill & stroke properties)
            if (style == null || ctx._commands.Count == 0)
            {
                return;
            }

            // we must get bounds first, so we can create possible gradient Paint's
            var bounds = ctx.GetBounds();

            var pathStyle = style.Value;

            // set opacities (alpha) to colors if defined
            pathStyle.SetOpacities();

            // if there are linear/radient gradients create them now
            if (pathStyle.FillPaintId != null)
            {
                if(_svgResources.TryGetValue(pathStyle.FillPaintId, out (object obj, Type type) gradient))
                {
                    pathStyle.FillPaint = GetPaint(gradient.obj, gradient.type, bounds);
                }
            }

            if (pathStyle.StrokePaintId != null)
            {
                if (_svgResources.TryGetValue(pathStyle.StrokePaintId, out (object obj, Type type) gradient))
                {
                    pathStyle.StrokePaint = GetPaint(gradient.obj, gradient.type, bounds);
                }
            }

            // build the path
            SvgPath path = new SvgPath()
            {
                Commands = ctx._commands.AsReadOnlySpan().ToArray(),
                Style = pathStyle,
                Bounds = bounds
            };

            // store path
            _svgPaths.Add(path);
        }

        static Paint? GetPaint(object obj, Type type, in NvgBounds bounds)
        {
            if (obj != null)
            {
                if (type == typeof(SvgLinearGradient))
                {
                    // cast & create Paint
                    if (((SvgLinearGradient)obj).TryGetPaint(bounds, out var paint))
                    {
                        return paint;
                    }
                }
                else if (type == typeof(SvgRadialGradient))
                {
                    // cast & create Paint
                    if (((SvgRadialGradient)obj).TryGetPaint(bounds, out var paint))
                    {
                        return paint;
                    }
                }
            }

            return default;
        }

        #endregion
    }
}