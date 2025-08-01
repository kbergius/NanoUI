﻿using NanoUI.Common;
using NanoUI.Nvg.Data;
using NanoUI.Svg;
using NanoUI.Svg.Data;
using System.IO;
using System.Numerics;

namespace NanoUI.Nvg
{
    // this handles SVG operations
    public partial class NvgContext
    {
        // store created shapes (parsed svg files)
        // todo: should be dictionary if we supoort removeing shapes!
        ArrayBuffer<SvgShape> _svgShapes = new();

        // returns shapeId or -1 if couldn't create svg shape
        public int CreateSvg(string path)
        {
            if (!File.Exists(path))
            {
                return Globals.INVALID;
            }

            // store
            SvgShape shape = SvgManager.CreateSvg(this, path);
            _svgShapes.Add(shape);

            return _svgShapes.Count - 1;
        }

        // get the size of the svg - needed when used in ui
        public bool TryGetSvgSize(int svgId, out Vector2 size)
        {
            if(svgId < 0 || svgId >= _svgShapes.Count)
            {
                size = Vector2.Zero;
                return false;
            }

            ref SvgShape shape = ref _svgShapes[svgId];

            // we get max value of paths or width & height specified in <svg> tag
            size = Vector2.Max(shape.Size, shape.PathsMax);
            return true;
        }

        // if returns false; means shapeId is not found
        public bool DrawSvg(int shapeId)
        {
            if(shapeId < 0 || shapeId >= _svgShapes.Count)
            {
                return false;
            }

            // we use this lots - so get it
            ref NvgState state = ref GetState();

            // get shape
            ref SvgShape shape = ref _svgShapes[shapeId];

            // loop paths
            for (int i = 0; i < shape.Paths.Length; i++)
            {
                // get path
                ref SvgPath path = ref shape.Paths[i];

                ref SvgStyle style = ref path.Style;

                // note: there is different winding & logic for fills & strokes (outline),
                // so we must draw shape twice, if both of them are defined
                if (style.FillColor != null || style.FillPaint != null)
                {
                    DrawPath(state, path, style, true);
                }

                if ((style.StrokeColor != null || style.StrokePaint != null) && style.StrokeWidth > 0)
                {
                    DrawPath(state, path, style, false);
                }
            }

            return true;
        }

        // note: this is kind of "hack":
        // - with strokes we don't create subpaths; instead we force to create separate paths
        // - with fills we force winding manual
        internal void DrawPath(in NvgState state, in SvgPath path, in SvgStyle style, bool fill)
        {
            // start path - reset commands etc
            BeginPath();

            // loop commands
            for (int j = 0; j < path.Commands.Length; j++)
            {
                ref NvgPathCommand command = ref path.Commands[j];

                switch (command.CommandType)
                {
                    case NvgPathCommandType.MoveTo:
                        _pathMoveTo(state, command.P0);

                        if (fill)
                        {
                            // this is a hack to correctly handle many holes in glyph shape
                            _pathWinding(Winding.Manual);
                        }
                        break;
                    case NvgPathCommandType.LineTo:
                        _pathLineTo(state, command.P0);
                        
                        if (!fill)
                        {
                            // force new path
                            _pathMoveTo(state, command.P0);
                        }
                        break;
                    case NvgPathCommandType.BezierTo:
                        _pathBezierTo(state, command.P0, command.P1, command.P2);

                        if (!fill)
                        {
                            // force new path
                            _pathMoveTo(state, command.P2);
                        }
                        break;
                    case NvgPathCommandType.Close:
                        _pathClose();
                        break;
                    case NvgPathCommandType.Winding:
                        // todo: check - shall we call this (we use even-odd winding rule,
                        // but there can be nonzero winding rule)
                        _pathWinding(command.Winding);
                        break;
                }
            }

            // we have already checked that fill & storke values are not null, therefore "!"
            if (fill)
            {
                if(style.FillPaint != null)
                {
                    FillPaint(style.FillPaint!.Value);
                }
                else
                {
                    FillColor(style.FillColor!.Value);
                }
                Fill();
            }
            else
            {
                if (style.StrokeLineCap != null)
                {
                    LineCap(style.StrokeLineCap.Value);
                }
                else
                {
                    // this is a hack - we need this when stroke width is "big" or scaled upwards
                    // note: if not used, there is "gaps" between line ends and starts
                    // todo: could be some conditions that have to be met?
                    LineCap(Common.LineCap.Round);
                }

                if (style.StrokeLineJoin != null)
                {
                    LineJoin(style.StrokeLineJoin.Value);
                }

                if (style.StrokeMiterLimit > 0)
                {
                    MiterLimit(style.StrokeMiterLimit.Value);
                }

                StrokeWidth(style.StrokeWidth!.Value);

                if (style.StrokePaint != null)
                {
                    StrokePaint(style.StrokePaint!.Value);
                }
                else
                {
                    StrokeColor(style.StrokeColor!.Value);
                }

                Stroke();
            }
        }
    }
}