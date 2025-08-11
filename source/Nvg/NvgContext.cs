using NanoUI.Common;
using NanoUI.Fonts;
using NanoUI.Nvg.Data;
using NanoUI.Rendering;
using NanoUI.Utils;
using System;
using System.Numerics;

namespace NanoUI.Nvg
{
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

        // This is not a "correct" way of doing things. However on the user's point of view
        // it would be quite frustrating to do allways nullable checks with this.
        // And I guess this will be used a lot.
        // Since NanoUI is a library, that doesn't know anything where/how/why it is used,
        // the responsibility to init NanoUI correctly, before any user can access it,
        // lies on the framework, that provides NanoUI service/renderer.
        // So if you, the user, get any NPE, blame your framework provider not the NanoUI!
        // note: this "hack" is not used elsewhere in the NanoUI.
#pragma warning disable CS8618 // disable nullable warning
        public static NvgContext Instance { get; private set; }
#pragma warning restore CS8618 // enable nullable warning

        INvgRenderer _nvgRenderer;

        public NvgContext(INvgRenderer nvgRenderer, float devicePixelRatio = 1.0f)
            :this(nvgRenderer, new StbTrueTypeManager(), devicePixelRatio)
        {
        }

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

        // nvgBeginFrame
        public void BeginFrame()
        {
            // note: these could also be done in EndFrame

            // clear used buffers
            Fontstash.Clear();
            _states.Clear();

            // note: paths etc clearance done in BeginPath?
        }

        // nvgEndFrame
        public void EndFrame()
        {
            _nvgRenderer.Render();

            // clear draw cache
            DrawCache.Clear();
        }

        #endregion

        #region States

        // nvgSave
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

        // nvgRestore
        public void RestoreState()
        {
            if (_states.Count > 0)
            {
                _states.RemoveLast();
            }
        }

        // nvgReset
        public void ResetState()
        {
            if (_states.Count > 0)
            {
                GetState().Reset();
            }
        }

        // nvg__getState
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

        // nvgStrokeWidth
        public void StrokeWidth(float width)
        {
            GetState().StrokeWidth = width;
        }

        // nvgMiterLimit
        public void MiterLimit(float limit)
        {
            GetState().MiterLimit = limit;
        }

        // nvgLineCap
        public void LineCap(LineCap cap)
        {
            GetState().LineCap = cap;
        }

        // nvgLineJoin
        public void LineJoin(LineCap join)
        {
            GetState().LineJoin = join;
        }

        // nvgGlobalAlpha
        public void GlobalAlpha(float alpha)
        {
            GetState().Alpha = alpha;
        }

        // nvgStrokeColor
        public void StrokeColor(Color color)
        {
            GetState().Stroke.Reset(color);
        }

        // nvgStrokePaint
        public void StrokePaint(Paint paint)
        {
            paint.Transform *= GetState().Transform;
            GetState().Stroke.Copy(paint);
        }

        // nvgFillColor
        public void FillColor(Color color)
        {
            GetState().Fill.Reset(color);
        }

        // nvgFillPaint
        public void FillPaint(Paint paint)
        {
            paint.Transform *= GetState().Transform;
            GetState().Fill.Copy(paint);
        }

        #endregion

        #region Transforms

        // note: you can easily use any transformation method that Matrix3x2 provides

        // Translate with the specified X and Y components
        // nvgTranslate
        public void Translate(float x, float y)
        {
            Translate(new Vector2(x, y));
        }

        // Translate with the specified 2-dimensional vector
        public void Translate(Vector2 position)
        {
            GetState().Transform = Matrix3x2.CreateTranslation(position) * GetState().Transform;
        }

        // Rotate current transform with angle in radians.
        // nvgRotate
        public void Rotate(float radians)
        {
            GetState().Transform = Matrix3x2.CreateRotation(radians) * GetState().Transform;
        }

        // Rotate current transform with angle in radians around centerPoint
        public void Rotate(float radians, Vector2 centerPoint)
        {
            GetState().Transform = Matrix3x2.CreateRotation(radians, centerPoint) * GetState().Transform;
        }

        // Scale uniformly with the given scale.
        // nvgScale
        public void Scale(float scale)
        {
            GetState().Transform = Matrix3x2.CreateScale(scale) * GetState().Transform;
        }

        // Scale with the specified vector scale (x & y)
        public void Scale(Vector2 scales)
        {
            GetState().Transform = Matrix3x2.CreateScale(scales) * GetState().Transform;
        }

        // Scalw with the specified vector scale with an offset from the specified center point.
        public void Scale(Vector2 scales, Vector2 centerPoint)
        {
            GetState().Transform = Matrix3x2.CreateScale(scales, centerPoint) * GetState().Transform;
        }

        // Skew with the specified angles in radians
        // nvgSkewX, nvgSkewY
        public void Skew(float radiansX, float radiansY)
        {
            GetState().Transform = Matrix3x2.CreateSkew(radiansX, radiansY) * GetState().Transform;
        }

        // Skew with the specified angles in radians and a center point
        public void Skew(float radiansX, float radiansY, Vector2 centerPoint)
        {
            GetState().Transform = Matrix3x2.CreateSkew(radiansX, radiansY, centerPoint) * GetState().Transform;
        }

        // nvgTransform
        public void Transform(Matrix3x2 transform)
        {
            GetState().Transform = transform * GetState().Transform;
        }

        // nvgResetTransform
        public void ResetTransform()
        {
            // todo: should that be new Matrix3x2()?
            GetState().Transform = Matrix3x2.Identity;
        }

        // nvgCurrentTransform
        public ref Matrix3x2 CurrentTransform()
        {
            return ref GetState().Transform;
        }

        #endregion

        #region Paths

        // nvgBeginPath
        public void BeginPath()
        {
            _commands.Clear();
            _paths.Clear();
            _points.Clear();
        }

        // nvgMoveTo
        public void MoveTo(float x, float y)
            => MoveTo(new Vector2(x, y));

        public void MoveTo(Vector2 p)
        {
            _pathMoveTo(GetState(), p);
        }

        // nvgLineTo
        public void LineTo(float x, float y)
            => LineTo(new Vector2(x, y));

        public void LineTo(Vector2 p)
        {
            _pathLineTo(GetState(), p);
        }

        // nvgBezierTo
        public void BezierTo(float c1x, float c1y, float c2x, float c2y, float x, float y, float kw, float kh)
        {
            BezierTo(c1x * kw, c1y * kh, c2x * kw, c2y * kh, x * kw, y * kh);
        }

        public void BezierTo(float c0x, float c0y, float c1x, float c1y, float x, float y)
            => BezierTo(new Vector2(c0x, c0y), new Vector2(c1x, c1y), new Vector2(x, y));

        public void BezierTo(Vector2 cp0, Vector2 cp1, Vector2 p)
        {
            _pathBezierTo(GetState(), cp0, cp1, p);
        }

        // nvgQuadTo
        public void QuadTo(float cx, float cy, float x, float y)
            => QuadTo(new Vector2(cx, cy), new Vector2(x, y));

        public void QuadTo(Vector2 cp, Vector2 p)
        {
            _pathQuadTo(GetState(), cp, p);
        }

        // nvgArcTo
        public void ArcTo(float x1, float y1, float x2, float y2, float radius)
            => _pathArcTo(GetState(), new Vector2(x1, y1), new Vector2(x2, y2), radius);

        // nvgClosePath
        public void ClosePath()
        {
            _pathClose();
        }

        // nvgPathWinding
        public void PathWinding(Solidity sol)
            => PathWinding((Winding)sol);

        // Normally you should use Winding.CounterClockwise (solid) or Winding.Clockwise (hole)
        // note: if you have issues with fills, you could also try setting Winding.Manual, which
        // bypasses automatic winding check & points conversion
        public void PathWinding(Winding dir)
        {
            _pathWinding(dir);
        }

        // nvgFill
        public void Fill()
        {
            ref NvgState state = ref GetState();

            _createPaths(_nvgParams);

            float fringeWidth = _nvgParams.FringeWidth;

            _expandFill(0.0f, Common.LineCap.Miter, 2.4f, fringeWidth);

            DrawCache.CreateFillCommands(state, state.Fill, fringeWidth, _bounds, _paths.AsReadOnlySpan());
        }

        // nvgStroke
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

        // nvgArc
        public void Arc(float cx, float cy, float r, float a0, float a1, Winding dir)
            => Arc(new Vector2(cx, cy), r, a0, a1, dir);

        public void Arc(Vector2 c, float r, float a0, float a1, Winding dir)
        {
            _pathArc(GetState(), c, r, a0, a1, dir);
        }

        // nvgRect
        public void Rect(Vector2 pos, Vector2 size)
            => Rect(new Rect(pos, size));

        public void Rect(float x, float y, float width, float height)
            => Rect(new Rect(x, y, width, height));

        public void Rect(Rect rect)
        {
            _pathRect(GetState(), rect);
        }

        // nvgRoundedRect
        public void RoundedRect(Rect rect, float r)
        {
            RoundedRectVarying(rect, r, r, r, r);
        }

        public void RoundedRect(Vector2 pos, Vector2 size, float r)
            => RoundedRect(pos.X, pos.Y, size.X, size.Y, r);

        public void RoundedRect(float x, float y, float width, float height, float r)
            => RoundedRect(new Rect(x, y, width, height), r);

        // nvgRoundedRectVarying
        public void RoundedRectVarying(float x, float y, float width, float height, in CornerRadius radius)
            => RoundedRectVarying(new Rect(x, y, width, height),
                radius.TopLeft, radius.TopRight, radius.BottomRight, radius.BottomLeft);
        public void RoundedRectVarying(Vector2 pos, Vector2 size, in CornerRadius radius)
            => RoundedRectVarying(new Rect(pos, size), 
                radius.TopLeft, radius.TopRight, radius.BottomRight, radius.BottomLeft);

        public void RoundedRectVarying(float x, float y, float width, float height, float radTopLeft, float radTopRight, float radBottomRight, float radBottomLeft)
            => RoundedRectVarying(new Rect(x, y, width, height), radTopLeft, radTopRight, radBottomRight, radBottomLeft);

        public void RoundedRectVarying(Rect rect, float radTopLeft, float radTopRight, float radBottomRight, float radBottomLeft)
        {
            _pathRoundedRectVarying(GetState(), rect, radTopLeft, radTopRight, radBottomRight, radBottomLeft);
        }

        // nvgCircle
        public void Circle(float centerX, float centerY, float radius)
            => Circle(new Vector2(centerX, centerY), radius);

        public void Circle(Vector2 center, float radius)
        {
            Ellipse(center, radius, radius);
        }

        // nvgEllipse
        public void Ellipse(float cx, float cy, float rx, float ry)
            => Ellipse(new Vector2(cx, cy), rx, ry);

        public void Ellipse(Vector2 c, float rx, float ry)
        {
            _pathEllipse(GetState(), c, rx, ry);
        }

        public void Pentagram(Vector2 center, float radius)
        {
            if (radius <= 0)
                return;

            _pathPentagram(GetState(), center, radius);
        }

        #endregion

        #region Scissoring

        // todo: is Scissor working if directly called (not from IntersectScissor)?
        // nvgScissor
        public void Scissor(Rect rect)
            => Scissor(rect.Position, rect.Size);

        public void Scissor(float x, float y, float width, float height)
            => Scissor(new Vector2(x, y), new Vector2(width, height));

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

        // nvgIntersectScissor
        public void IntersectScissor(Vector2 pos, Vector2 size)
            => IntersectScissor(new Rect(pos, size));

        public void IntersectScissor(float x, float y, float width, float height)
            => IntersectScissor(new Rect(x, y, width, height));

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

        // nvgResetScissor
        public void ResetScissor()
        {
            GetState().Scissor.Reset();
        }

        #endregion

        #region Images

        // nvgCreateImage, nvgCreateImageMem
        // note: this is just a helper method and params are passed as-is to renderer.
        // so you can set in path param whatever file identication you like or
        // call your renderer directly
        public int CreateTexture(string path, TextureFlags textureFlags = 0)
        {
            return _nvgRenderer.CreateTexture(path, textureFlags);
        }

        public int CreateTexture(TextureDesc description)
        {
            return _nvgRenderer.CreateTexture(description);
        }

        // nvgImageSize
        public bool GetTextureSize(int texture, out Vector2 textureSize)
        {
            if (texture == Globals.INVALID)
            {
                textureSize = Vector2.Zero;
                return false;
            }

            return _nvgRenderer.GetTextureSize(texture, out textureSize);
        }

        public bool GetTextureSize(int textureIndex, out uint texWidth, out uint texHeight)
        {
            if(GetTextureSize(textureIndex, out Vector2 size))
            {
                texWidth = (uint)size.X;
                texHeight = (uint)size.Y;
                
                return true;
            }

            texWidth = texHeight = 0;
            return false;
        }

        // nvgUpdateImage
        public bool UpdateTexture(int texture, ReadOnlySpan<byte> data)
        {
            if (texture == Globals.INVALID)
                return false;

            return _nvgRenderer.UpdateTexture(texture, data);
        }

        // rect is (x, y, width, height)
        public bool UpdateTextureRegion(int texture, Vector4 regionRect, ReadOnlySpan<byte> allData)
        {
            if (texture == Globals.INVALID)
                return false;

            return _nvgRenderer.UpdateTextureRegion(texture, regionRect, allData);
        }

        public void ResizeTexture(int texture, TextureDesc description)
        {
            if (texture == Globals.INVALID)
                return;

            _nvgRenderer.ResizeTexture(texture, description);
        }

        // nvgDeleteImage
        public bool DeleteTexture(int texture)
        {
            if (texture == Globals.INVALID)
                return false;

            return _nvgRenderer.DeleteTexture(texture);
        }

        #endregion

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
