using NanoUI.Common;
using NanoUI.Fonts;
using NanoUI.Nvg.Data;
using NanoUI.Rendering;
using NanoUI.Utils;
using System;
using System.Numerics;

namespace NanoUI.Nvg
{
    /// <summary>
    /// NvgContext is the real engine of the NanoUI. 
    /// It provides API for the drawing layer methods.
    /// You should create it at your application's startup.
    /// </summary>
    public partial class NvgContext : IDisposable
    {
        // ratios
        NvgParams _nvgParams;

        // unmanaged buffers
        UnsafeBuffer<NvgState> _states = new(10);
        UnsafeBuffer<NvgPoint> _points = new(100);
        UnsafeBuffer<NvgPath> _paths = new(16);

        // must be internal since svg must access these
        internal UnsafeBuffer<NvgPathCommand> _commands = new(256);

#pragma warning disable CS8618 // disable nullable warning
        /// <summary>
        /// Returns the instance of the NvgContext.
        /// </summary>
        /// <remarks>You should have created & stored NvgContext in your application's startup before using this.</remarks>
        public static NvgContext Instance { get; private set; }
#pragma warning restore CS8618 // enable nullable warning

        INvgRenderer _nvgRenderer;

        /// <summary>
        /// Constructor. Pass your renderer implementation when creating NvgContext.
        /// </summary>
        /// <param name="nvgRenderer">Your INvgRenderer implementation</param>
        /// <param name="useSafeFontManager">Safe/unsafe font manager</param>
        /// <param name="devicePixelRatio">DPI</param>
        /// <remarks>
        /// If you set useSafeFontManager = true, NanoUI uses builtin managed version
        /// of the font manager (unmanaged uses pointers).
        /// </remarks>
        public NvgContext(INvgRenderer nvgRenderer, bool useSafeFontManager = false, float devicePixelRatio = 1.0f)
            :this(nvgRenderer, useSafeFontManager? new SafeStbTrueTypeManager() : new StbTrueTypeManager(), devicePixelRatio)
        {
        }

        /// <summary>
        /// Constructor. Pass your INvgRenderer implementation when creating NvgContext.
        /// You can also set your own font manager.
        /// </summary>
        /// <param name="nvgRenderer">INvgRenderer</param>
        /// <param name="fontManager">Your IFontManager implementation</param>
        /// <param name="devicePixelRatio">DPI</param>
        public NvgContext(INvgRenderer nvgRenderer, IFontManager fontManager, float devicePixelRatio = 1.0f)
        {
            // some widgets may want to get NvgContext
            Instance = this;

            _nvgRenderer = nvgRenderer;

            _nvgParams.SetDevicePixelRatio(devicePixelRatio);

            // todo: we could also set that Fontstash uses NvgRenderer from here (through Instance)
            // so there would be only 1 place where renderer is stored
            Fontstash.Init(nvgRenderer, fontManager);
        }

        #region Frames

        /// <summary>
        /// Begins drawing a new frame and clears buffers.
        /// </summary>
        /// <remarks>All draw commands should be executed between BeginFrame and EndFrame.</remarks>
        public void BeginFrame()
        {
            // note: these could also be done in EndFrame

            // clear used buffers
            Fontstash.Clear();
            _states.Clear();

            // note: paths etc clearance done in BeginPath?
        }

        /// <summary>
        /// Ends drawing and triggers rendering in INvgRenderer.
        /// After this DrawCache is cleared.
        /// </summary>
        public void EndFrame()
        {
            _nvgRenderer.Render();

            // clear draw cache
            DrawCache.Clear();
        }

        #endregion

        #region States

        /// <summary>
        /// Pushes and saves the current render state into a state stack.
        /// A matching RestoreState must be used to restore the state.
        /// </summary>
        public void SaveState()
        {
            // add
            _states.Add();

            if (_states.Count > 1)
            {
                // we use last state as base settings
                _states.Copy(_states.Count - 2, _states.Count - 1);
            }
            else
            {
                GetState().Reset();
            }
        }

        /// <summary>
        /// Pops and restores current render state.
        /// </summary>
        public void RestoreState()
        {
            if (_states.Count > 0)
            {
                _states.RemoveLast();
            }
        }

        /// <summary>
        /// Resets current render state to default values.
        /// Does not affect the render state stack.
        /// </summary>
        public void ResetState()
        {
            if (_states.Count > 0)
            {
                GetState().Reset();
            }
        }

        /// <summary>
        /// Returns current state.
        /// </summary>
        ref NvgState GetState()
        {
            if (_states.Count == 0)
            {
                _states.Add().Reset();
            }

            return ref _states.Last;
        }

        #endregion

        #region Render styles

        /// <summary>
        /// Sets the stroke width of the stroke style.
        /// </summary>
        /// <param name="width">Width</param>
        public void StrokeWidth(float width)
        {
            GetState().StrokeWidth = width;
        }

        /// <summary>
        /// Sets the miter limit of the stroke style.
        /// Miter limit controls when a sharp corner is beveled.
        /// </summary>
        /// <param name="limit">Limit</param>
        public void MiterLimit(float limit)
        {
            GetState().MiterLimit = limit;
        }

        /// <summary>
        /// Sets how the end of the line (cap) is drawn,
        /// </summary>
        /// <param name="cap">LineCap</param>
        public void LineCap(LineCap cap)
        {
            GetState().LineCap = cap;
        }

        /// <summary>
        /// Sets how sharp path corners are drawn.
        /// </summary>
        /// <param name="join">LineCap</param>
        public void LineJoin(LineCap join)
        {
            GetState().LineJoin = join;
        }

        /// <summary>
        /// Sets the transparency applied to all rendered shapes.
        /// Already transparent paths will get proportionally more transparent as well.
        /// </summary>
        /// <param name="alpha">Alpha</param>
        public void GlobalAlpha(float alpha)
        {
            GetState().Alpha = alpha;
        }

        /// <summary>
        /// Sets current stroke style to a solid color.
        /// </summary>
        /// <param name="color">Color</param>
        public void StrokeColor(Color color)
        {
            GetState().Stroke.Reset(color);
        }

        /// <summary>
        /// Sets current stroke style to a paint, which can be a one of the gradients or a pattern.
        /// </summary>
        /// <param name="paint">Paint</param>
        public void StrokePaint(Paint paint)
        {
            paint.Transform *= GetState().Transform;
            GetState().Stroke.Copy(paint);
        }

        /// <summary>
        /// Sets current fill style to a solid color.
        /// </summary>
        /// <param name="color">Color</param>
        public void FillColor(Color color)
        {
            GetState().Fill.Reset(color);
        }

        /// <summary>
        /// Sets current fill style to a paint, which can be a one of the gradients or a pattern.
        /// </summary>
        /// <param name="paint">Paint</param>
        public void FillPaint(Paint paint)
        {
            paint.Transform *= GetState().Transform;
            GetState().Fill.Copy(paint);
        }

        #endregion

        #region Transforms

        /// <summary>
        /// Translates with the specified X and Y components.
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        public void Translate(float x, float y)
        {
            Translate(new Vector2(x, y));
        }

        /// <summary>
        /// Translates with the specified 2-dimensional vector
        /// </summary>
        /// <param name="position">Position</param>
        public void Translate(Vector2 position)
        {
            GetState().Transform = Matrix3x2.CreateTranslation(position) * GetState().Transform;
        }

        /// <summary>
        /// Rotates current transform with angle in radians.
        /// </summary>
        /// <param name="radians">Radians</param>
        public void Rotate(float radians)
        {
            GetState().Transform = Matrix3x2.CreateRotation(radians) * GetState().Transform;
        }

        /// <summary>
        /// Rotates current transform with angle in radians around centerPoint.
        /// </summary>
        /// <param name="radians">Radians</param>
        /// <param name="centerPoint">Center point</param>
        public void Rotate(float radians, Vector2 centerPoint)
        {
            GetState().Transform = Matrix3x2.CreateRotation(radians, centerPoint) * GetState().Transform;
        }

        /// <summary>
        /// Scales uniformly with the given scale.
        /// </summary>
        /// <param name="scale">Scale</param>
        public void Scale(float scale)
        {
            GetState().Transform = Matrix3x2.CreateScale(scale) * GetState().Transform;
        }

        /// <summary>
        /// Scales with the specified vector scale (x & y).
        /// </summary>
        /// <param name="scales">Scales</param>
        public void Scale(Vector2 scales)
        {
            GetState().Transform = Matrix3x2.CreateScale(scales) * GetState().Transform;
        }

        /// <summary>
        /// Scales with the specified vector scale with an offset from the specified center point.
        /// </summary>
        /// <param name="scales">Scales</param>
        /// <param name="centerPoint">Center point</param>
        public void Scale(Vector2 scales, Vector2 centerPoint)
        {
            GetState().Transform = Matrix3x2.CreateScale(scales, centerPoint) * GetState().Transform;
        }

        /// <summary>
        /// Skews with the specified angles in radians.
        /// </summary>
        /// <param name="radiansX">RadiansX</param>
        /// <param name="radiansY">RadiansY</param>
        public void Skew(float radiansX, float radiansY)
        {
            GetState().Transform = Matrix3x2.CreateSkew(radiansX, radiansY) * GetState().Transform;
        }

        /// <summary>
        /// Skews with the specified angles in radians and a center point.
        /// </summary>
        /// <param name="radiansX">RadiansX</param>
        /// <param name="radiansY">RadiansY</param>
        /// <param name="centerPoint">Center point</param>
        public void Skew(float radiansX, float radiansY, Vector2 centerPoint)
        {
            GetState().Transform = Matrix3x2.CreateSkew(radiansX, radiansY, centerPoint) * GetState().Transform;
        }

        /// <summary>
        /// Multiplies current transform by specified matrix.
        /// </summary>
        /// <param name="transform">Transform</param>
        public void Transform(Matrix3x2 transform)
        {
            GetState().Transform = transform * GetState().Transform;
        }

        /// <summary>
        /// Resets current transform to a identity matrix.
        /// </summary>
        public void ResetTransform()
        {
            GetState().Transform = Matrix3x2.Identity;
        }

        /// <summary>
        /// Returns current transform matrix.
        /// </summary>
        public ref Matrix3x2 CurrentTransform()
        {
            return ref GetState().Transform;
        }

        #endregion

        #region Paths

        /// <summary>
        /// Clears the current path and sub-paths.
        /// </summary>
        public void BeginPath()
        {
            _commands.Clear();
            _paths.Clear();
            _points.Clear();
        }

        /// <summary>
        /// Starts a new sub-path with specified point as a first point.
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        public void MoveTo(float x, float y)
            => MoveTo(new Vector2(x, y));

        /// <summary>
        /// Starts a new sub-path with specified point as a first point.
        /// </summary>
        /// <param name="p">Point</param>
        public void MoveTo(Vector2 p)
        {
            _pathMoveTo(GetState(), p);
        }

        /// <summary>
        /// Adds line segment from the last point in the path to the specified point.
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        public void LineTo(float x, float y)
            => LineTo(new Vector2(x, y));

        /// <summary>
        /// Adds line segment from the last point in the path to the specified point.
        /// </summary>
        /// <param name="p">Point</param>
        public void LineTo(Vector2 p)
        {
            _pathLineTo(GetState(), p);
        }

        /// <summary>
        /// Adds cubic bezier segment from last point in the path via two control points to the specified point.
        /// </summary>
        /// <param name="c0x">ControlPoint0 - X</param>
        /// <param name="c0y">ControlPoint0 - Y</param>
        /// <param name="c1x">ControlPoint1 - X</param>
        /// <param name="c1y">ControlPoint1 - Y</param>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        public void BezierTo(float c0x, float c0y, float c1x, float c1y, float x, float y)
            => BezierTo(new Vector2(c0x, c0y), new Vector2(c1x, c1y), new Vector2(x, y));

        /// <summary>
        /// Adds cubic bezier segment from last point in the path via two control points to the specified point.
        /// </summary>
        /// <param name="cp0">ControlPoint0</param>
        /// <param name="cp1">ControlPoint1</param>
        /// <param name="p">Point</param>
        public void BezierTo(Vector2 cp0, Vector2 cp1, Vector2 p)
        {
            _pathBezierTo(GetState(), cp0, cp1, p);
        }

        /// <summary>
        /// Adds quadratic bezier segment from last point in the path via a control point to the specified point.
        /// </summary>
        /// <param name="cx">ControlPoint - X</param>
        /// <param name="cy">ControlPoint - Y</param>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        public void QuadTo(float cx, float cy, float x, float y)
            => QuadTo(new Vector2(cx, cy), new Vector2(x, y));

        /// <summary>
        /// Adds quadratic bezier segment from last point in the path via a control point to the specified point.
        /// </summary>
        /// <param name="cp">ControlPoint</param>
        /// <param name="p">Point</param>
        public void QuadTo(Vector2 cp, Vector2 p)
        {
            _pathQuadTo(GetState(), cp, p);
        }

        /// <summary>
        /// Adds an arc segment at the corner defined by the last path point and two specified points.
        /// </summary>
        /// <param name="x1">Point1 - X</param>
        /// <param name="y1">Point1 - Y</param>
        /// <param name="x2">Point2 - X</param>
        /// <param name="y2">Point2 - Y</param>
        /// <param name="radius">Radius</param>
        public void ArcTo(float x1, float y1, float x2, float y2, float radius)
            => _pathArcTo(GetState(), new Vector2(x1, y1), new Vector2(x2, y2), radius);

        /// <summary>
        /// Closes current sub-path with a line segment.
        /// </summary>
        public void ClosePath()
        {
            _pathClose();
        }

        /// <summary>
        /// Sets the current sub-path winding.
        /// </summary>
        /// <param name="sol">Solidity</param>
        public void PathWinding(Solidity sol)
            => PathWinding((Winding)sol);

        /// <summary>
        /// Sets the current sub-path winding.
        /// </summary>
        /// <param name="dir">Winding</param>
        /// <remarks>
        /// Normally you should use Winding.CounterClockwise (solid) or Winding.Clockwise (hole)
        /// If you have issues with fills, you could also try setting Winding.Manual, which
        /// bypasses automatic winding check & points conversion.
        /// </remarks>
        public void PathWinding(Winding dir)
        {
            _pathWinding(dir);
        }

        /// <summary>
        /// Fills the current path with current fill style.
        /// </summary>
        public void Fill()
        {
            ref NvgState state = ref GetState();

            _createPaths(_nvgParams);

            float fringeWidth = _nvgParams.FringeWidth;

            _expandFill(0.0f, Common.LineCap.Miter, 2.4f, fringeWidth);

            DrawCache.CreateFillCommands(state, state.Fill, fringeWidth, _bounds, _paths.AsReadOnlySpan());
        }

        /// <summary>
        /// Fills the current path with current stroke style.
        /// </summary>
        public void Stroke()
        {
            ref NvgState state = ref GetState();

            float scale = MathUtils.GetAverageScale(state.Transform);
            float strokeWidth = Math.Clamp(state.StrokeWidth * scale, 0.0f, 200.0f);
            Paint strokePaint = state.Stroke;

            float fringeWidth = _nvgParams.FringeWidth;

            if (strokeWidth < fringeWidth)
            {
                float alpha = Math.Clamp(strokeWidth / fringeWidth, 0.0f, 1.0f);
                strokePaint.PremultiplyAlpha(alpha * alpha);
                strokeWidth = fringeWidth;
            }

            _createPaths(_nvgParams);

            // todo: FringeWidth always 0
            _expandStroke(strokeWidth * 0.5f, 0.0f, state.LineCap, state.LineJoin, state.MiterLimit,
                _nvgParams.TessTol);

            // note: strokeWidth maybe needed if anti-alias
            DrawCache.CreateStrokeCommands(state, strokePaint, fringeWidth, _paths.AsReadOnlySpan());
        }

        #endregion

        #region Primitives

        /// <summary>
        /// Creates new circle arc shaped sub-path.
        /// The arc is drawn from angle a0 to a1.
        /// </summary>
        /// <param name="cx">Center X</param>
        /// <param name="cy">Center Y</param>
        /// <param name="r">Radius</param>
        /// <param name="a0">Angle0</param>
        /// <param name="a1">Angle1</param>
        /// <param name="dir">Winding</param>
        public void Arc(float cx, float cy, float r, float a0, float a1, Winding dir)
            => Arc(new Vector2(cx, cy), r, a0, a1, dir);

        /// <summary>
        /// Creates new circle arc shaped sub-path.
        /// The arc is drawn from angle a0 to a1.
        /// </summary>
        /// <param name="c">Center</param>
        /// <param name="r">Radius</param>
        /// <param name="a0">Angle0</param>
        /// <param name="a1">Angle1</param>
        /// <param name="dir">Winding</param>
        public void Arc(Vector2 c, float r, float a0, float a1, Winding dir)
        {
            _pathArc(GetState(), c, r, a0, a1, dir);
        }

        /// <summary>
        /// Creates a new rectangle shaped sub-path.
        /// </summary>
        /// <param name="pos">TopLeft position</param>
        /// <param name="size">Size</param>
        public void Rect(Vector2 pos, Vector2 size)
            => Rect(new Rect(pos, size));

        /// <summary>
        /// Creates a new rectangle shaped sub-path.
        /// </summary>
        /// <param name="x">Left position</param>
        /// <param name="y">Top position</param>
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        public void Rect(float x, float y, float width, float height)
            => Rect(new Rect(x, y, width, height));

        /// <summary>
        /// Creates a new rectangle shaped sub-path.
        /// </summary>
        /// <param name="rect">Rectangle</param>
        public void Rect(Rect rect)
        {
            _pathRect(GetState(), rect);
        }

        /// <summary>
        /// Creates a new rounded rectangle shaped sub-path.
        /// </summary>
        /// <param name="rect">Rectangle</param>
        /// <param name="r">Corner radius</param>
        public void RoundedRect(Rect rect, float r)
        {
            RoundedRectVarying(rect, r, r, r, r);
        }

        /// <summary>
        /// Creates a new rounded rectangle shaped sub-path.
        /// </summary>
        /// <param name="pos">TopLeft position</param>
        /// <param name="size">Size</param>
        /// <param name="r">Corner radius</param>
        public void RoundedRect(Vector2 pos, Vector2 size, float r)
            => RoundedRect(pos.X, pos.Y, size.X, size.Y, r);

        /// <summary>
        /// Creates a new rounded rectangle shaped sub-path.
        /// </summary>
        /// <param name="x">Left position</param>
        /// <param name="y">Top position</param>
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        /// <param name="r">Corner radius</param>
        public void RoundedRect(float x, float y, float width, float height, float r)
            => RoundedRect(new Rect(x, y, width, height), r);

        /// <summary>
        /// Creates a new rounded rectangle shaped sub-path with varying radii (CornerRadius) for each corner.
        /// </summary>
        /// <param name="x">Left</param>
        /// <param name="y">Top</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="radius">CornerRadius</param>
        public void RoundedRectVarying(float x, float y, float width, float height, in CornerRadius radius)
            => RoundedRectVarying(new Rect(x, y, width, height),
                radius.TopLeft, radius.TopRight, radius.BottomRight, radius.BottomLeft);

        /// <summary>
        /// Creates a new rounded rectangle shaped sub-path with varying radii (CornerRadius) for each corner.
        /// </summary>
        /// <param name="pos">TopLeft</param>
        /// <param name="size">Size</param>
        /// <param name="radius">CornerRadius</param>
        public void RoundedRectVarying(Vector2 pos, Vector2 size, in CornerRadius radius)
            => RoundedRectVarying(new Rect(pos, size), 
                radius.TopLeft, radius.TopRight, radius.BottomRight, radius.BottomLeft);

        /// <summary>
        /// Creates a new rounded rectangle shaped sub-path with varying radii (radXXX values) for each corner.
        /// </summary>
        /// <param name="x">Left</param>
        /// <param name="y">Top</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="radTopLeft">Radius top-left</param>
        /// <param name="radTopRight">Radius top-right</param>
        /// <param name="radBottomRight">Radius bottom-right</param>
        /// <param name="radBottomLeft">Radius bottom-left</param>
        public void RoundedRectVarying(float x, float y, float width, float height, float radTopLeft, float radTopRight, float radBottomRight, float radBottomLeft)
            => RoundedRectVarying(new Rect(x, y, width, height), radTopLeft, radTopRight, radBottomRight, radBottomLeft);

        /// <summary>
        /// Creates a new rounded rectangle shaped sub-path with varying radii (radXXX values) for each corner.
        /// </summary>
        /// <param name="rect">Rectangle</param>
        /// <param name="radTopLeft">Radius top-left</param>
        /// <param name="radTopRight">Radius top-right</param>
        /// <param name="radBottomRight">Radius bottom-right</param>
        /// <param name="radBottomLeft">Radius bottom-left</param>
        public void RoundedRectVarying(Rect rect, float radTopLeft, float radTopRight, float radBottomRight, float radBottomLeft)
        {
            _pathRoundedRectVarying(GetState(), rect, radTopLeft, radTopRight, radBottomRight, radBottomLeft);
        }

        /// <summary>
        /// Creates a new circle shaped sub-path.
        /// </summary>
        /// <param name="centerX">Center-X</param>
        /// <param name="centerY">Center-Y</param>
        /// <param name="radius">Radius</param>
        public void Circle(float centerX, float centerY, float radius)
            => Circle(new Vector2(centerX, centerY), radius);

        /// <summary>
        /// Creates a new circle shaped sub-path.
        /// </summary>
        /// <param name="center">Center</param>
        /// <param name="radius">Radius</param>
        public void Circle(Vector2 center, float radius)
        {
            Ellipse(center, radius, radius);
        }

        /// <summary>
        /// Creates a new ellipse shaped sub-path.
        /// </summary>
        /// <param name="cx">Center-X</param>
        /// <param name="cy">Center-Y</param>
        /// <param name="rx">Radius-X</param>
        /// <param name="ry">Radius-Y</param>
        public void Ellipse(float cx, float cy, float rx, float ry)
            => Ellipse(new Vector2(cx, cy), rx, ry);

        /// <summary>
        /// Creates a new ellipse shaped sub-path.
        /// </summary>
        /// <param name="c">Center</param>
        /// <param name="rx">Radius-X</param>
        /// <param name="ry">Radius-Y</param>
        public void Ellipse(Vector2 c, float rx, float ry)
        {
            _pathEllipse(GetState(), c, rx, ry);
        }

        /// <summary>
        /// Creates a new pentagram shaped sub-path.
        /// </summary>
        /// <param name="center">Center</param>
        /// <param name="radius">Radius</param>
        public void Pentagram(Vector2 center, float radius)
        {
            if (radius <= 0)
                return;

            _pathPentagram(GetState(), center, radius);
        }

        #endregion

        #region Scissoring

        /// <summary>
        /// Sets the current scissor rectangle.
        /// The scissor rectangle is transformed by the current transform.
        /// </summary>
        /// <param name="rect">Rectangle</param>
        public void Scissor(Rect rect)
            => Scissor(rect.Position, rect.Size);

        /// <summary>
        /// Sets the current scissor rectangle.
        /// The scissor rectangle is transformed by the current transform.
        /// </summary>
        /// <param name="x">Left position</param>
        /// <param name="y">Top position</param>
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        public void Scissor(float x, float y, float width, float height)
            => Scissor(new Vector2(x, y), new Vector2(width, height));

        /// <summary>
        /// Sets the current scissor rectangle.
        /// The scissor rectangle is transformed by the current transform.
        /// </summary>
        /// <param name="pos">TopLeft position</param>
        /// <param name="size">Size</param>
        public void Scissor(Vector2 pos, Vector2 size)
        {
            size.X = MathF.Max(0.0f, size.X);
            size.Y = MathF.Max(0.0f, size.Y);

            Vector2 lastRow = pos + size * 0.5f;
            Matrix3x2 transform = Matrix3x2.Identity;
            transform.M31 = lastRow.X;
            transform.M32 = lastRow.Y;

            ref NvgState state = ref GetState();

            state.Scissor.Transform = transform * state.Transform;
            state.Scissor.Extent = size * 0.5f;
        }

        /// <summary>
        /// Intersects current scissor rectangle with the specified rectangle.
        /// The scissor rectangle is transformed by the current transform.
        /// </summary>
        /// <param name="pos">TopLeft position</param>
        /// <param name="size">Size</param>
        /// <remarks>
        /// In case the rotation of previous scissor rect differs from
        /// the current one, the intersection will be done between the specified
        /// rectangle and the previous scissor rectangle transformed in the current
        /// transform space. The resulting shape is always a rectangle.
        /// </remarks>
        public void IntersectScissor(Vector2 pos, Vector2 size)
            => IntersectScissor(new Rect(pos, size));

        /// <summary>
        /// Intersects current scissor rectangle with the specified rectangle.
        /// The scissor rectangle is transformed by the current transform.
        /// </summary>
        /// <param name="x">Left position</param>
        /// <param name="y">Top position</param>
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        /// <remarks>
        /// In case the rotation of previous scissor rect differs from
        /// the current one, the intersection will be done between the specified
        /// rectangle and the previous scissor rectangle transformed in the current
        /// transform space. The resulting shape is always a rectangle.
        /// </remarks>
        public void IntersectScissor(float x, float y, float width, float height)
            => IntersectScissor(new Rect(x, y, width, height));

        /// <summary>
        /// Intersects current scissor rectangle with the specified rectangle.
        /// The scissor rectangle is transformed by the current transform.
        /// </summary>
        /// <param name="rect">Rectangle</param>
        /// <remarks>
        /// In case the rotation of previous scissor rect differs from
        /// the current one, the intersection will be done between the specified
        /// rectangle and the previous scissor rectangle transformed in the current
        /// transform space. The resulting shape is always a rectangle.
        /// </remarks>
        public void IntersectScissor(Rect rect)
        {
            ref NvgState state = ref GetState();

            if (state.Scissor.Extent.X < 0)
            {
                Scissor(rect.Position, rect.Size);
                return;
            }

            Matrix3x2 ptransform = state.Scissor.Transform;
            Vector2 e = state.Scissor.Extent;

            Matrix3x2.Invert(state.Transform, out Matrix3x2 invtransform);

            ptransform *= invtransform;

            Vector2 te = new(
                e.X * MathF.Abs(ptransform.M11) + e.Y * MathF.Abs(ptransform.M21),
                e.X * MathF.Abs(ptransform.M12) + e.Y * MathF.Abs(ptransform.M22)
            );

            Rect r = _isectRects(new Rect(ptransform.M31 - te.X, ptransform.M32 - te.Y, te.X * 2.0f, te.Y * 2.0f), rect);

            Scissor(r.Position, r.Size);
        }

        /// <summary>
        /// Resets and disables scissoring.
        /// </summary>
        public void ResetScissor()
        {
            GetState().Scissor.Reset();
        }

        #endregion

        #region Images

        /// <summary>
        /// Creates texture with specified string identifier (normally path).
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="textureFlags">Texture flags</param>
        /// <returns>Id of the texture.</returns>
        /// <remarks>
        /// This is just a helper method and params are passed as-is to renderer.
        /// so you can set in path param whatever file identication you like or
        /// call your renderer directly.
        /// </remarks>
        public int CreateTexture(string path, TextureFlags textureFlags = 0)
        {
            return _nvgRenderer.CreateTexture(path, textureFlags);
        }

        /// <summary>
        /// Creates texture from texture description.
        /// </summary>
        /// <param name="description">Texture description</param>
        /// <returns>Id of the texture.</returns>
        public int CreateTexture(TextureDesc description)
        {
            return _nvgRenderer.CreateTexture(description);
        }

        /// <summary>
        /// Returns the dimensions of a created texture.
        /// </summary>
        /// <param name="texture">Texture id</param>
        /// <param name="textureSize">Texture size</param>
        /// <returns>Success</returns>
        public bool GetTextureSize(int texture, out Vector2 textureSize)
        {
            if (texture == Globals.INVALID)
            {
                textureSize = Vector2.Zero;
                return false;
            }

            return _nvgRenderer.GetTextureSize(texture, out textureSize);
        }

        /// <summary>
        /// Returns the dimensions of a created texture.
        /// </summary>
        /// <param name="texture">Texture id</param>
        /// <param name="texWidth">Texture width</param>
        /// <param name="texHeight">Texture height</param>
        /// <returns>Success</returns>
        public bool GetTextureSize(int texture, out uint texWidth, out uint texHeight)
        {
            if(GetTextureSize(texture, out Vector2 size))
            {
                texWidth = (uint)size.X;
                texHeight = (uint)size.Y;
                
                return true;
            }

            texWidth = texHeight = 0;
            return false;
        }

        /// <summary>
        /// Updates texture data specified by texture id.
        /// </summary>
        /// <param name="texture">Texture id</param>
        /// <param name="data">Texture data</param>
        /// <returns>Success</returns>
        public bool UpdateTexture(int texture, ReadOnlySpan<byte> data)
        {
            if (texture == Globals.INVALID)
                return false;

            return _nvgRenderer.UpdateTexture(texture, data);
        }

        /// <summary>
        /// Resizes created texture.
        /// </summary>
        /// <param name="texture">Texture id</param>
        /// <param name="description">Texture description</param>
        public void ResizeTexture(int texture, TextureDesc description)
        {
            if (texture == Globals.INVALID)
                return;

            _nvgRenderer.ResizeTexture(texture, description);
        }

        /// <summary>
        /// Deletes created texture.
        /// </summary>
        /// <param name="texture">Texture id</param>
        /// <returns>Success</returns>
        public bool DeleteTexture(int texture)
        {
            if (texture == Globals.INVALID)
                return false;

            return _nvgRenderer.DeleteTexture(texture);
        }

        #endregion

        /// <summary>
        /// Disposes unmanaged memories.
        /// </summary>
        /// <remarks>This should be always called when application is closing.</remarks>
        public void Dispose()
        {
            // free unmanaged memories
            _points.Dispose();
            _paths.Dispose();
            _commands.Dispose();
            _states.Dispose();

            DrawCache.Dispose();
            Fontstash.Dispose();
        }
    }
}
