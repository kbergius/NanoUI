using NanoUI.Common;
using NanoUI.Nvg.Data;
using NanoUI.Rendering.Data;
using System;
using System.Numerics;

namespace NanoUI.Rendering
{
    /// <summary>
    /// Static cache to collect all information needed in rendering phase.
    /// </summary>
    public static partial class DrawCache
    {
        static UnsafeBuffer<FillStrokeInfo> _fillStrokes = new(100);        
        static UnsafeBuffer<Vertex> _vertices = new(8192);
        static UnsafeBuffer<FragmentUniform> _uniforms = new(100);

        internal static int VerticesCount => _vertices.Count;

        /// <summary>
        /// Collected vertices since last BeginFrame command.
        /// </summary>
        public static ReadOnlySpan<Vertex> Vertices => _vertices.AsReadOnlySpan();

        /// <summary>
        /// Collected fragment uniforms since last BeginFrame command.
        /// </summary>
        public static ReadOnlySpan<FragmentUniform> Uniforms => _uniforms.AsReadOnlySpan();

        static DrawCall _call;

        /// <summary>
        /// Clear all buffers in DrawCache.
        /// </summary>
        public static void Clear()
        {
            _vertices.Clear();
            _indexes.Clear();
            _uniforms.Clear();

            _drawCommands.Clear();
            _fillStrokes.Clear();
        }

        internal static void AddVertex(float x, float y, float u, float v)
        {
            ref Vertex vertex = ref _vertices.Add();

            vertex.Position = new Vector2(x, y);
            vertex.UV = new Vector2(u, v);
        }

        internal static void AddVertex(Vector2 pos, float u, float v)
        {
            ref Vertex vertex = ref _vertices.Add();

            vertex.Position = pos;
            vertex.UV = new Vector2(u, v);
        }

        internal static Vector2 GetStrokePosition(in NvgPath path, int index)
        {
            return _vertices[path.StrokeOffset + index].Position;
        }

        // Fill, ConvexFill
        internal static void CreateFillCommands(in NvgState state, in Paint paint,
            float fringe, in NvgBounds bounds, ReadOnlySpan<NvgPath> paths)
        {
            if (paths.Length == 1 && paths[0].Convex)
            {
                _call.Type = DrawCallType.ConvexFill;
            }
            else
            {
                _call.Type = DrawCallType.Fill;
            }

            _call.FillStrokeCount = paths.Length;
            _call.FillStrokeOffset = _fillStrokes.Count;
            _call.Texture = paint.Texture;
            _call.Uniform1 = _uniforms.Count;

            foreach(var path in paths)
            {
                ref FillStrokeInfo info = ref _fillStrokes.Add();

                info.FillOffset = path.FillOffset;
                info.FillCount = path.FillCount;
                info.StrokeOffset = path.StrokeOffset;
                info.StrokeCount = path.StrokeCount;
            }

            // Setup uniforms for draw calls
            if (_call.Type == DrawCallType.Fill)
            {
                // Quad
                _call.VertexOffset = _vertices.Count;
                _call.VertexCount = 4;

                // add quad vertices
                ref Vertex bottomRight = ref _vertices.Add();
                bottomRight.Position = bounds.Max;
                bottomRight.UV = new Vector2(0.5f, 1.0f);

                ref Vertex topRight = ref _vertices.Add();
                topRight.Position = new Vector2(bounds.Max.X, bounds.Min.Y);
                topRight.UV = new Vector2(0.5f, 1.0f);

                ref Vertex bottomLeft = ref _vertices.Add();
                bottomLeft.Position = new Vector2(bounds.Min.X, bounds.Max.Y);
                bottomLeft.UV = new Vector2(0.5f, 1.0f);

                ref Vertex topLeft = ref _vertices.Add();
                topLeft.Position = bounds.Min;
                topLeft.UV = new Vector2(0.5f, 1.0f);

                // Fill stencil
                ref FragmentUniform uniform = ref _uniforms.Add();
                uniform.ActionType = (float)DrawActionType.FillStencil;
                // todo: Clear rest????

                // Fill
                _call.Uniform2 = _uniforms.Count;
                BuildUniform(state, paint, fringe);
            }
            else
            {
                // Convex fill
                BuildUniform(state, paint, fringe);
            }

            // CREATE FILL DRAW COMMANDS
            if(_call.Type == DrawCallType.Fill)
            {
                // fill
                CreateFill();
            }
            else
            {
                // convex fill
                CreateConvexFill();
            }
        }

        // Stroke
        internal static void CreateStrokeCommands(in NvgState state, in Paint paint,
            float fringe, ReadOnlySpan<NvgPath> paths)
        {
            _call.Type = DrawCallType.Stroke;
            _call.FillStrokeOffset = _fillStrokes.Count;
            _call.FillStrokeCount = paths.Length;
            _call.Texture = paint.Texture;
            _call.Uniform1 = _uniforms.Count;

            foreach (var path in paths)
            {
                ref FillStrokeInfo info = ref _fillStrokes.Add();

                info.FillOffset = 0;
                info.FillCount = 0;
                info.StrokeOffset = path.StrokeOffset;
                info.StrokeCount = path.StrokeCount;
            }

            // Stroke
            BuildUniform(state, paint, fringe);

            // CREATE STROKE DRAW COMMANDS
            CreateStroke();
        }

        // Uses quads
        internal static void CreateTextCommands(in NvgState state, int verticesOffset, float fringe,
            GlyphBaking fontBaking, float fontSize)
        {
            _call.Type = DrawCallType.Text;
            _call.VertexOffset = verticesOffset;
            _call.VertexCount = _vertices.Count - verticesOffset;
            _call.Texture = state.Fill.Texture;

            int index = _uniforms.Count;
            _call.Uniform1 = index;

            // Text
            BuildUniform(state, state.Fill, fringe);

            ref FragmentUniform uniform = ref _uniforms[index];

            uniform.ActionType = fontBaking == GlyphBaking.SDF?
                (float)DrawActionType.TextSDF : (float)DrawActionType.Text;
            uniform.FontSize = fontSize;

            // Create quad draw command(s)
            CreateText();
        }

        static void BuildUniform(in NvgState state, in Paint paint, float fringe)
        {
            ref FragmentUniform uniform = ref _uniforms.Add();

            // we multiply with alpha
            paint.PremultiplyAlpha(state.Alpha);

            uniform.InnerCol = paint.InnerColor.ToVector4();
            uniform.OuterCol = paint.OuterColor.ToVector4();

            if (state.Scissor.Extent.X < -0.5f || state.Scissor.Extent.Y < -0.5f)
            {
                uniform.ScissorMat = new Matrix4x4();
                uniform.ScissorExt = Vector2.One;
                uniform.ScissorScale = Vector2.One;
            }
            else
            {
                Matrix3x2.Invert(state.Scissor.Transform, out var scissorInvert);
                uniform.ScissorMat = new Matrix4x4(scissorInvert);
                uniform.ScissorExt = state.Scissor.Extent;
                uniform.ScissorScale = new Vector2(
                    MathF.Sqrt(state.Scissor.Transform.M11 * state.Scissor.Transform.M11 + state.Scissor.Transform.M21 * state.Scissor.Transform.M21) / fringe,
                    MathF.Sqrt(state.Scissor.Transform.M21 * state.Scissor.Transform.M21 + state.Scissor.Transform.M22 * state.Scissor.Transform.M22) / fringe
                );
            }

            uniform.Extent = paint.Extent;
            
            if (paint.Texture > Globals.INVALID)
            {
                uniform.ActionType = (float)DrawActionType.FillImage;
            }
            else
            {
                uniform.ActionType = (float)DrawActionType.FillGradient;
                uniform.Radius = paint.Radius;
                uniform.Feather = paint.Feather;
            }

            Matrix3x2.Invert(paint.Transform, out var paintInvert);
            uniform.PaintMat = new Matrix4x4(paintInvert);
        }

        public static void Dispose()
        {
            _vertices.Dispose();
            _indexes.Dispose();
            _uniforms.Dispose();

            _drawCommands.Dispose();
            _fillStrokes.Dispose();
        }
    }
}
