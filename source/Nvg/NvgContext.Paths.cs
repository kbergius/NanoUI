using NanoUI.Common;
using NanoUI.Nvg.Data;
using NanoUI.Rendering;
using NanoUI.Utils;
using System;
using System.Numerics;

namespace NanoUI.Nvg
{
    // private/internal
    partial class NvgContext
    {
        // Length proportional to radius of a cubic bezier handle for 90deg arcs
        const float NVG_KAPPA90 = 0.5522847493f;
        const byte MAX_TESSELATION_DEPTH = 10;

        NvgBounds _bounds;

        // last registered position in path creation
        Vector2 _lastPosition;

        // 5-star (pentagram)
        Vector2[] _starPoints = Array.Empty<Vector2>();

        #region PathCommands

        // note: must be internal, since FontStash when using glyph shapes must access this function
        internal void _pathMoveTo(in NvgState state, Vector2 pos)
        {
            _lastPosition = pos;

            ref NvgPathCommand command = ref _commands.Add();

            command.CommandType = NvgPathCommandType.MoveTo;
            command.P0 = Vector2.Transform(pos, state.Transform);
        }

        // note: must be internal, since FontStash when using glyph shapes must access this function
        internal void _pathLineTo(in NvgState state, Vector2 pos)
        {
            _lastPosition = pos;

            ref NvgPathCommand command = ref _commands.Add();

            command.CommandType = NvgPathCommandType.LineTo;
            command.P0 = Vector2.Transform(pos, state.Transform);
        }

        // note: must be internal, since FontStash when using glyph shapes must access this function
        internal void _pathBezierTo(in NvgState state, Vector2 p0, Vector2 p1, Vector2 p2)
        {
            ref NvgParams nvgParams = ref _nvgParams;

            _lastPosition = p2;

            ref NvgPathCommand command = ref _commands.Add();

            command.CommandType = NvgPathCommandType.BezierTo;
            command.P0 = Vector2.Transform(p0, state.Transform);
            command.P1 = Vector2.Transform(p1, state.Transform);
            command.P2 = Vector2.Transform(p2, state.Transform);
            command.TessTol = nvgParams.TessTol;
        }

        internal void _pathQuadTo(in NvgState state, Vector2 cp, Vector2 p)
        {
            _pathBezierTo(state,
                    _lastPosition + 2.0f / 3.0f * (cp - _lastPosition),
                    p + 2.0f / 3.0f * (cp - p),
                    p);
        }

        // note: must be internal, since FontStash when using glyph shapes must access this function
        internal void _pathArcTo(in NvgState state, Vector2 p1, Vector2 p2, float radius)
        {
            if (_commands.Count == 0)
            {
                return;
            }

            Vector2 p0 = _lastPosition;

            float distToll = _nvgParams.DistTol;

            if (_ptEquals(p0, p1, distToll) ||
                _ptEquals(p1, p2, distToll) ||
                _distPtSeg(p1, p0, p2) < distToll ||
                radius < distToll)
            {
                _pathLineTo(state, p1);
                return;
            }

            Vector2 d0 = p0 - p1;
            Vector2 d1 = p2 - p1;
            d0 = Vector2.Normalize(d0);
            d1 = Vector2.Normalize(d1);
            float a = MathF.Acos(d0.X * d1.X + d0.Y * d1.Y);
            float d = radius / MathF.Tan(a / 2.0f);

            if (d > 10000.0f)
            {
                _pathLineTo(state, p1);
                return;
            }

            Winding dir;
            Vector2 valuesC;
            float a0, a1;

            if (d0.Cross(d1) > 0.0f)
            {
                float cx = p1.X + d0.X * d + d0.Y * radius;
                float cy = p1.Y + d0.Y * d + -d0.X * radius;
                a0 = MathF.Atan2(d0.X, -d0.Y);
                a1 = MathF.Atan2(-d1.X, d1.Y);
                dir = Winding.Clockwise;
                valuesC = new Vector2(cx, cy);
            }
            else
            {
                float cx = p1.X + d0.X * d + -d0.Y * radius;
                float cy = p1.Y + d0.Y * d + d0.X * radius;
                a0 = MathF.Atan2(-d0.X, d0.Y);
                a1 = MathF.Atan2(d1.X, -d1.Y);
                dir = Winding.CounterClockwise;
                valuesC = new Vector2(cx, cy);
            }

            _pathArc(state, valuesC, radius, a0, a1, dir);
        }

        // note: must be internal, since FontStash when using glyph shapes must access this function
        internal void _pathClose()
        {
            ref NvgPathCommand command = ref _commands.Add();

            command.CommandType = NvgPathCommandType.Close;
        }

        // note: must be internal, since FontStash when using glyph shapes must access this function
        internal void _pathWinding(Winding winding)
        {
            ref NvgPathCommand command = ref _commands.Add();

            command.CommandType = NvgPathCommandType.Winding;
            command.Winding = winding;
        }

        #endregion

        #region PathPrimitives

        void _pathArc(in NvgState state, Vector2 c, float r, float a0, float a1, Winding dir)
        {
            Vector2 pPos = default;
            Vector2 pTan = default;

            bool line = _commands.Count > 0;

            float da = a1 - a0;

            if (dir == Winding.Clockwise)
            {
                if (MathF.Abs(da) >= MathF.PI * 2.0f)
                {
                    da = MathF.PI * 2.0f;
                }
                else
                {
                    while (da < 0.0f)
                    {
                        da += MathF.PI * 2.0f;
                    }
                }
            }
            else
            {
                if (MathF.Abs(da) >= MathF.PI * 2.0f)
                {
                    da = -MathF.PI * 2.0f;
                }
                else
                {
                    while (da > 0.0f)
                    {
                        da -= MathF.PI * 2.0f;
                    }
                }
            }

            int ndivs = Math.Max(1, Math.Min((int)(MathF.Abs(da) / (MathF.PI * 0.5f) + 0.5f), 5));
            float hda = da / ndivs / 2.0f;
            float kappa = MathF.Abs(4.0f / 3.0f * (1.0f - MathF.Cos(hda)) / MathF.Sin(hda));

            if (dir == Winding.CounterClockwise)
            {
                kappa *= -1.0f;
            }

            for (int i = 0; i <= ndivs; i++)
            {
                float alpha = a0 + da * (i / (float)ndivs);
                Vector2 d = new(MathF.Cos(alpha), MathF.Sin(alpha));
                Vector2 pos = new(c.X + d.X * r, c.Y + d.Y * r);
                Vector2 tan = new(-d.Y * r * kappa, d.X * r * kappa);

                if (i == 0)
                {
                    if (line)
                    {
                        _pathLineTo(state, pos);
                    }
                    else
                    {
                        _pathMoveTo(state, pos);
                    }
                }
                else
                {
                    _pathBezierTo(state, pPos + pTan, pos - tan, pos);
                }

                pPos = pos;
                pTan = tan;
            }
        }

        void _pathRect(in NvgState state, in Rect rect)
        {
            _pathMoveTo(state, rect.Position);
            _pathLineTo(state, new(rect.X, rect.Max.Y));
            _pathLineTo(state, rect.Max);
            _pathLineTo(state, new(rect.Max.X, rect.Y));
            _pathClose();
        }

        void _pathRoundedRectVarying(in NvgState state, in Rect rect, float radTopLeft, float radTopRight, float radBottomRight, float radBottomLeft)
        {
            if (radTopLeft < 0.1f && radTopRight < 0.1f && radBottomRight < 0.1f && radBottomLeft < 0.1f)
            {
                _pathRect(state, rect);
            }
            else
            {
                float factor = 1 - NVG_KAPPA90;

                Vector2 half = Vector2.Abs(rect.Size) * 0.5f;
                Vector2 rBL = new(MathF.Min(radBottomLeft, half.X) * Math.Sign(rect.Size.X), MathF.Min(radBottomLeft, half.Y) * Math.Sign(rect.Size.Y));
                Vector2 rBR = new(MathF.Min(radBottomRight, half.X) * Math.Sign(rect.Size.X), MathF.Min(radBottomRight, half.Y) * Math.Sign(rect.Size.Y));
                Vector2 rTR = new(MathF.Min(radTopRight, half.X) * Math.Sign(rect.Size.X), MathF.Min(radTopRight, half.Y) * Math.Sign(rect.Size.Y));
                Vector2 rTL = new(MathF.Min(radTopLeft, half.X) * Math.Sign(rect.Size.X), MathF.Min(radTopLeft, half.Y) * Math.Sign(rect.Size.Y));

                _pathMoveTo(state, new(rect.X, rect.Y + rTL.Y));
                _pathLineTo(state, new(rect.X, rect.Y + rect.Size.Y - rBL.Y));
                _pathBezierTo(state,
                    new(rect.X, rect.Y + rect.Size.Y - rBL.Y * factor),
                    new(rect.X + rBL.X * factor, rect.Y + rect.Size.Y),
                    new(rect.X + rBL.X, rect.Y + rect.Size.Y)
                );

                _pathLineTo(state, new(rect.X + rect.Size.X - rBR.X, rect.Y + rect.Size.Y));
                _pathBezierTo(state,
                    new(rect.X + rect.Size.X - rBR.X * factor, rect.Y + rect.Size.Y),
                    new(rect.X + rect.Size.X, rect.Y + rect.Size.Y - rBR.Y * factor),
                    new(rect.X + rect.Size.X, rect.Y + rect.Size.Y - rBR.Y)
                );

                _pathLineTo(state, new(rect.X + rect.Size.X, rect.Y + rTR.Y));
                _pathBezierTo(state,
                    new(rect.X + rect.Size.X, rect.Y + rTR.Y * factor),
                    new(rect.X + rect.Size.X - rTR.X * factor, rect.Y),
                    new(rect.X + rect.Size.X - rTR.X, rect.Y)
                );

                _pathLineTo(state, new(rect.X + rTL.X, rect.Y));
                _pathBezierTo(state,
                    new(rect.X + rTL.X * factor, rect.Y),
                    new(rect.X, rect.Y + rTL.Y * factor),
                    new(rect.X, rect.Y + rTL.Y)
                );

                _pathClose();
            }
        }

        void _pathEllipse(in NvgState state, Vector2 c, float rx, float ry)
        {
            _pathMoveTo(state, new(c.X - rx, c.Y));
            _pathBezierTo(state,
                new(c.X - rx, c.Y + ry * NVG_KAPPA90),
                new(c.X - rx * NVG_KAPPA90, c.Y + ry),
                new(c.X, c.Y + ry));
            _pathBezierTo(state,
                new(c.X + rx * NVG_KAPPA90, c.Y + ry),
                new(c.X + rx, c.Y + ry * NVG_KAPPA90),
                new(c.X + rx, c.Y));
            _pathBezierTo(state,
                new(c.X + rx, c.Y - ry * NVG_KAPPA90),
                new(c.X + rx * NVG_KAPPA90, c.Y - ry),
                new(c.X, c.Y - ry));
            _pathBezierTo(state,
                new(c.X - rx * NVG_KAPPA90, c.Y - ry),
                new(c.X - rx, c.Y - ry * NVG_KAPPA90),
                new(c.X - rx, c.Y));
            _pathClose();
        }

        void _pathPentagram(in NvgState state, Vector2 center, float radius)
        {
            if (_starPoints.Length == 0)
            {
                _starPoints = new Vector2[5];

                // create initial star points
                float dtheta = 2 * MathF.PI / 5;
                float theta = -MathF.PI / 2;

                for (int i = 0; i < _starPoints.Length; i++)
                {
                    _starPoints[i] = new Vector2(MathF.Cos(theta), MathF.Sin(theta));
                    theta += dtheta;
                }
            }

            _pathMoveTo(state, _starPoints[0] * radius + center);
            _pathLineTo(state, _starPoints[2] * radius + center);
            _pathLineTo(state, _starPoints[4] * radius + center);
            _pathLineTo(state, _starPoints[1] * radius + center);
            _pathLineTo(state, _starPoints[3] * radius + center);

            // this is a fix to fill
            _pathWinding(Winding.Manual);

            _pathClose();
        }

        #endregion

        #region _createPaths

        // svg needs to get bounds for creating gradients
        internal NvgBounds GetBounds()
        {
            if (_points.Count == 0)
            {
                // we have created commands, but not paths & points - so create
                _createPaths(_nvgParams);
            }

            return _bounds;
        }

        void _createPaths(in NvgParams nvgParams)
        {
            for (int i = 0; i < _commands.Count; i++)
            {
                ref NvgPathCommand c = ref _commands[i];

                switch (c.CommandType)
                {
                    case NvgPathCommandType.BezierTo:
                        ref NvgPath lastPath = ref _paths.Last;

                        if (lastPath.PointCount > 0)
                        {
                            _tesselateBezier(c.TessTol, _points[lastPath.PointOffset + lastPath.PointCount - 1].Position,
                                c.P0, c.P1, c.P2, 0, NvgPointFlags.Corner);
                        }
                        break;
                    case NvgPathCommandType.Close:
                        _paths.Last.Closed = true;
                        break;
                    case NvgPathCommandType.LineTo:
                        _addPoint(ref _paths.Last, c.P0, NvgPointFlags.Corner);
                        break;
                    case NvgPathCommandType.MoveTo:
                        ref NvgPath newPath = ref _paths.Add();

                        // must reset all values
                        newPath.Reset();

                        // do add point
                        _addPoint(ref _paths.Last, c.P0, NvgPointFlags.Corner);
                        break;
                    case NvgPathCommandType.Winding:
                        _paths.Last.Winding = c.Winding;
                        break;
                }
            }

            _bounds.Min.X = _bounds.Min.Y = 1e6f;
            _bounds.Max.X = _bounds.Max.Y = -1e6f;

            for (int i = 0; i < _paths.Count; i++)
            {
                ref NvgPath path = ref _paths[i];

                _flattenPath(ref path, nvgParams.DistTol);

                _bounds.Min.X = MathF.Min(_bounds.Min.X, path.Bounds.Min.X);
                _bounds.Min.Y = MathF.Min(_bounds.Min.Y, path.Bounds.Min.Y);
                _bounds.Max.X = MathF.Max(_bounds.Max.X, path.Bounds.Max.X);
                _bounds.Max.Y = MathF.Max(_bounds.Max.Y, path.Bounds.Max.Y);
            }
        }

        #endregion

        #region _expandStroke

        void _expandStroke(float w, float fringe, LineCap lineCap, LineCap lineJoin, float miterLimit,
            float tessTol)
        {
            float aa = fringe;
            float u0 = 0.0f;
            float u1 = 1.0f;
            uint ncap = _curveDivs(w, MathF.PI, tessTol);

            w += aa * 0.5f;

            if (aa == 0.0f)
            {
                u0 = 0.5f;
                u1 = 0.5f;
            }

            _calculateJoins(w, lineJoin, miterLimit);

            // start point indexes
            int p0Index;
            int p1Index;

            for (int i = 0; i < _paths.Count; i++)
            {
                ref NvgPath path = ref _paths[i];

                // set current offset
                path.StrokeOffset = DrawCache.VerticesCount;
                path.FillCount = 0;

                bool loop = path.Closed;

                if (loop)
                {
                    p0Index = path.PointOffset + path.PointCount - 1;
                    p1Index = path.PointOffset;

                    //int s = 0;
                    //int e = path.PointCount;

                    // add stroke vertices
                    _doExpandStroke(p0Index, p1Index, loop, 0, path.PointCount,
                        path, aa, u0, u1, w, lineCap, lineJoin, ncap);
                }
                else
                {
                    p0Index = path.PointOffset;
                    p1Index = path.PointOffset + 1;

                    //int s = 1;
                    //int e = path.PointCount - 1;

                    // add stroke vertices
                    _doExpandStroke(p0Index, p1Index, loop, 1, path.PointCount - 1,
                        path, aa, u0, u1, w, lineCap, lineJoin, ncap);
                }

                // set count
                path.StrokeCount = DrawCache.VerticesCount - path.StrokeOffset;
            }
        }

        void _doExpandStroke(int p0Index, int p1Index, bool loop, int s, int e, in NvgPath path, float aa,
            float u0, float u1, float w, LineCap lineCap, LineCap lineJoin, uint ncap)
        {
            if (!loop)
            {
                ref NvgPoint p0 = ref _points[p0Index];
                ref NvgPoint p1 = ref _points[p1Index];

                Vector2 d = p1.Position - p0.Position;
                d = Vector2.Normalize(d);

                if (lineCap is Common.LineCap.Butt)
                {
                    _buttCapStart(p0, d, w, -aa * 0.5f, aa, u0, u1);
                }
                else if (lineCap is Common.LineCap.Butt or Common.LineCap.Square)
                {
                    _buttCapStart(p0, d, w, w - aa, aa, u0, u1);
                }
                else if (lineCap is Common.LineCap.Round)
                {
                    _roundCapStart(p0, d, w, ncap, u0, u1);
                }
            }

            for (int i = s; i < e; i++)
            {
                p1Index = path.PointOffset + i;

                ref NvgPoint p0 = ref _points[p0Index];
                ref NvgPoint p1 = ref _points[p1Index];

                if ((p1.Flags & NvgPointFlags.Bevel) != 0 || (p1.Flags & NvgPointFlags.InnerBevel) != 0)
                {
                    if (lineJoin == Common.LineCap.Round)
                    {
                        _roundJoin(p0, p1, w, w, u0, u1, ncap);
                    }
                    else
                    {
                        _bevelJoin(p0, p1, w, w, u0, u1);
                    }
                }
                else
                {
                    DrawCache.AddVertex(p1.Position + p1.DeltaM * w, u0, 1.0f);
                    DrawCache.AddVertex(p1.Position - p1.DeltaM * w, u1, 1.0f);
                }

                p0Index = p1Index;
            }

            if (s > 0)
            {
                p1Index = path.PointOffset + e;
            }

            if (loop)
            {
                // offset already set when we come here (use it to find 1 & 2 position)
                DrawCache.AddVertex(DrawCache.GetStrokePosition(path, 0), u0, 1.0f);
                DrawCache.AddVertex(DrawCache.GetStrokePosition(path, 1), u1, 1.0f);
            }
            else
            {
                ref NvgPoint p0 = ref _points[p0Index];
                ref NvgPoint p1 = ref _points[p1Index];

                Vector2 d = p1.Position - p0.Position;
                d = Vector2.Normalize(d);

                if (lineCap is Common.LineCap.Butt)
                {
                    _buttCapEnd(p1, d, w, -aa * 0.5f, aa, u0, u1);
                }
                else if (lineCap is Common.LineCap.Butt or Common.LineCap.Square)
                {
                    _buttCapEnd(p1, d, w, w - aa, aa, u0, u1);
                }
                else if (lineCap is Common.LineCap.Round)
                {
                    _roundCapEnd(p1, d, w, ncap, u0, u1);
                }
            }
        }

        #endregion

        #region _expandFill

        void _expandFill(float w, LineCap lineJoin, float miterLimit, float fringeWidth)
        {
            float aa = fringeWidth;
            float woff = 0.5f * aa;
            bool fringe = w > 0.0f;

            _calculateJoins(w, lineJoin, miterLimit);

            bool convex = _paths.Count == 1 && _paths[0].Convex;

            for (int i = 0; i < _paths.Count; i++)
            {
                ref NvgPath path = ref _paths[i];

                // 1. FILL
                // set current offset
                path.FillOffset = DrawCache.VerticesCount;
                // add fill vertices
                _expandFillFill(path, woff, fringe);
                // set count
                path.FillCount = DrawCache.VerticesCount - path.FillOffset;

                // 2. STROKE
                // set current offset
                path.StrokeOffset = DrawCache.VerticesCount;
                // add stroke vertices
                _expandFillStroke(ref path, woff, fringe, convex, w);
                // set count
                path.StrokeCount = DrawCache.VerticesCount - path.StrokeOffset;
            }
        }

        void _expandFillFill(in NvgPath path, float woff, bool fringe)
        {
            if (fringe)
            {
                int p0Index = path.PointOffset + path.PointCount - 1;
                int p1Index = path.PointOffset;

                for (int i = 0; i < path.PointCount; i++)
                {
                    ref NvgPoint p0 = ref _points[p0Index];
                    ref NvgPoint p1 = ref _points[p1Index];

                    //Vertex(p0, p1, woff);
                    if ((p1.Flags & NvgPointFlags.Bevel) != 0)
                    {
                        Vector2 dl0 = new(p0.Delta.Y, -p0.Delta.X);
                        Vector2 dl1 = new(p1.Delta.Y, -p1.Delta.X);

                        if ((p1.Flags & NvgPointFlags.Left) != 0)
                        {
                            Vector2 l = p1.Position + p1.DeltaM * woff;

                            DrawCache.AddVertex(l, 0.5f, 1.0f);
                        }
                        else
                        {
                            Vector2 l0 = (p1.Position + dl0) * woff;
                            Vector2 l1 = (p1.Position + dl1) * woff;

                            DrawCache.AddVertex(l0, 0.5f, 1.0f);
                            DrawCache.AddVertex(l1, 0.5f, 1.0f);
                        }
                    }
                    else
                    {
                        DrawCache.AddVertex(p1.Position + p1.DeltaM * woff, 0.5f, 1.0f);
                    }

                    p0Index = p1Index++;
                }
            }
            else
            {
                for (var i = 0; i < path.PointCount; ++i)
                {
                    ref NvgPoint point = ref _points[path.PointOffset + i];

                    DrawCache.AddVertex(point.Position, 0.5f, 1.0f);
                }
            }
        }

        void _expandFillStroke(ref NvgPath path, float woff, bool fringe, bool convex, float w)
        {
            if (fringe)
            {
                float lw = w + woff;
                float rw = w - woff;
                float lu = 0, ru = 1;

                if (convex)
                {
                    lw = woff;
                    lu = 0.5f;
                }

                int p0Index = path.PointOffset + path.PointCount - 1;
                int p1Index = path.PointOffset;

                for (var i = 0; i < path.PointCount; ++i)
                {
                    ref NvgPoint p0 = ref _points[p0Index];
                    ref NvgPoint p1 = ref _points[p1Index];

                    if ((p1.Flags & (NvgPointFlags.Bevel | NvgPointFlags.InnerBevel)) != 0)
                    {
                        _bevelJoin(p0, p1, lw, rw, lu, ru);
                    }
                    else
                    {
                        DrawCache.AddVertex(p1.Position + p1.DeltaM * lw, lu, 1.0f);
                        DrawCache.AddVertex(p1.Position - p1.DeltaM * rw, ru, 1.0f);
                    }

                    p0Index = p1Index++;
                }

                // offset already set when we come here (use it to find 1 & 2 position)
                DrawCache.AddVertex(DrawCache.GetStrokePosition(path, 0), lu, 1.0f);
                DrawCache.AddVertex(DrawCache.GetStrokePosition(path, 1), ru, 1.0f);
            }
            else
            {
                path.StrokeCount = 0;
            }
        }

        #endregion

        #region _flattenPath

        void _flattenPath(ref NvgPath path, float distTol)
        {
            int p0Index = path.PointOffset + path.PointCount - 1;
            int p1Index = path.PointOffset;

            if (_ptEquals(_points[p0Index].Position, _points[p1Index].Position, distTol))
            {
                // 'remove' last;
                path.PointCount--;

                p0Index--;

                path.Closed = true;
            }

            // Enforce winding

            // note: Winding.Manual neglects these functions & fixes some fill problems in certain shapes
            if (path.Winding != Winding.Manual && path.PointCount > 2)
            {
                // can't get read only span, since we modify values (PolyReverse)
                var span = _points.AsSpan(path.PointOffset, path.PointCount);

                float area = _polyArea(span);

                if (path.Winding == Winding.CounterClockwise && area < 0.0f)
                {
                    _polyReverse(span);
                }

                if (path.Winding == Winding.Clockwise && area > 0.0f)
                {
                    _polyReverse(span);
                }
            }

            for (var i = 0; i < path.PointCount; i++)
            {
                ref NvgPoint p0 = ref _points[p0Index];
                ref NvgPoint p1 = ref _points[p1Index];

                Vector2 delta = p1.Position - p0.Position;

                p0.Length = delta.Length();

                if (p0.Length > 1e-6f)
                {
                    float id = 1.0f / p0.Length;
                    delta *= id;
                }

                p0.Delta = delta;

                path.Bounds.Min.X = MathF.Min(path.Bounds.Min.X, p0.Position.X);
                path.Bounds.Min.Y = MathF.Min(path.Bounds.Min.Y, p0.Position.Y);
                path.Bounds.Max.X = MathF.Max(path.Bounds.Max.X, p0.Position.X);
                path.Bounds.Max.Y = MathF.Max(path.Bounds.Max.Y, p0.Position.Y);

                p0Index = p1Index++;
            }
        }

        #endregion

        #region _calculateJoins

        void _calculateJoins(float w, LineCap lineJoin, float miterLimit)
        {
            float iw = 0.0f;

            if (w > 0.0f)
            {
                iw = 1.0f / w;
            }

            for (int i = 0; i < _paths.Count; i++)
            {
                ref NvgPath path = ref _paths[i];

                int p0Index = path.PointOffset + path.PointCount - 1;
                int p1Index = path.PointOffset;
                uint nleft = 0;

                path.BevelCount = 0;

                for (int j = 0; j < path.PointCount; j++)
                {
                    ref NvgPoint p0 = ref _points[p0Index];
                    ref NvgPoint p1 = ref _points[p1Index];

                    bool bevelOrRound = lineJoin == Common.LineCap.Bevel || lineJoin == Common.LineCap.Round;
                    
                    _doJoin(ref p1, p0, iw, bevelOrRound, miterLimit, ref nleft, ref path.BevelCount);

                    p0Index = p1Index++;
                }

                path.Convex = nleft == path.PointCount;
            }
        }

        void _doJoin(ref NvgPoint point, in NvgPoint other, float iw, bool bevelOrRound, float miterLimit, ref uint nleft, ref uint bevelCount)
        {
            Vector2 dl0 = new(other.Delta.Y, -other.Delta.X);
            Vector2 dl1 = new(point.Delta.Y, -point.Delta.X);

            point.DeltaM = new((dl0.X + dl1.X) * 0.5f, (dl0.Y + dl1.Y) * 0.5f);

            float dmr2 = point.DeltaM.X * point.DeltaM.X + point.DeltaM.Y * point.DeltaM.Y;

            if (dmr2 > 0.000001f)
            {
                float scale = 1.0f / dmr2;
                scale = MathF.Min(scale, 600.0f);
                point.DeltaM *= scale;
            }

            point.Flags = (point.Flags & NvgPointFlags.Corner) != 0 ? NvgPointFlags.Corner : 0;

            float cross = point.Delta.X * other.Delta.Y - other.Delta.X * point.Delta.Y;

            if (cross > 0.0f)
            {
                nleft++;
                point.Flags |= NvgPointFlags.Left;
            }

            float limit = MathF.Max(1.0f, MathF.Min(other.Length, point.Length) * iw);

            if (dmr2 * limit * limit < 1.0f)
            {
                point.Flags |= NvgPointFlags.InnerBevel;
            }

            if ((point.Flags & NvgPointFlags.Corner) != 0)
            {
                if (dmr2 * miterLimit * miterLimit < 1.0f || bevelOrRound)
                {
                    point.Flags |= NvgPointFlags.Bevel;
                }
            }

            if ((point.Flags & (NvgPointFlags.Bevel | NvgPointFlags.InnerBevel)) != 0)
            {
                bevelCount++;
            }
        }

        #endregion

        #region _tesselateBezier

        void _tesselateBezier(float tessTol, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4,
            byte level, NvgPointFlags flags)
        {
            if (level > MAX_TESSELATION_DEPTH)
            {
                return;
            }

            Vector2 p12 = (p1 + p2) * 0.5f;
            Vector2 p23 = (p2 + p3) * 0.5f;
            Vector2 p34 = (p3 + p4) * 0.5f;
            Vector2 p123 = (p12 + p23) * 0.5f;

            Vector2 d = p4 - p1;
            float d2 = MathF.Abs((p2.X - p4.X) * d.Y - (p2.Y - p4.Y) * d.X);
            float d3 = MathF.Abs((p3.X - p4.X) * d.Y - (p3.Y - p4.Y) * d.X);

            if ((d2 + d3) * (d2 + d3) < tessTol * (d.X * d.X + d.Y * d.Y))
            {
                _addPoint(ref _paths.Last, p4, flags);
                return;
            }

            Vector2 p234 = (p23 + p34) * 0.5f;
            Vector2 p1234 = (p123 + p234) * 0.5f;

            _tesselateBezier(tessTol, p1, p12, p123, p1234, (byte)(level + 1), 0);
            _tesselateBezier(tessTol, p1234, p234, p34, p4, (byte)(level + 1), flags);
        }

        #endregion

        #region _addPoint

        void _addPoint(ref NvgPath path, Vector2 position, NvgPointFlags flags)
        {
            if (path.PointCount > 0)
            {
                ref NvgPoint pt = ref _points[path.PointOffset + path.PointCount - 1];

                if (_ptEquals(pt.Position, position, _nvgParams.DistTol))
                {
                    pt.Flags |= flags;
                    return;
                }
            }

            // if first time, set offset
            if (path.PointCount == 0)
            {
                // set offset
                path.PointOffset = _points.Count;
            }

            // "add" point
            ref NvgPoint point = ref _points.Add();
            point.Reset();

            point.Position = position;
            point.Flags = flags;

            // add point count
            path.PointCount++;
        }

        #endregion
    }
}
