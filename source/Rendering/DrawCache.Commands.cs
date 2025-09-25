using NanoUI.Common;
using NanoUI.Rendering.Data;
using System;

namespace NanoUI.Rendering
{
    public static partial class DrawCache
    {
        // draw commands
        static UnsafeBuffer<DrawCommand> _drawCommands = new(1000);

        /// <summary>
        /// Collected draw commands since last BeginFrame command.
        /// </summary>
        public static ReadOnlySpan<DrawCommand> DrawCommands => _drawCommands.AsReadOnlySpan();

        // we save current indices offset before adding indices
        static int _startIndexOffset;

        #region Fill

        static void CreateFill()
        {
            if (_call.VertexCount == 0)
                return;

            // Fill stencil
            // note: can't combine - since can have many disconnected paths & TriangleFan
            // todo: if we originally create TriangleList - we can combine?
            for (var i = 0; i < _call.FillStrokeCount; i++)
            {
                ref FillStrokeInfo info = ref _fillStrokes[_call.FillStrokeOffset + i];

                if (info.FillCount == 0)
                    continue;

                // create indices first
                _startIndexOffset = _indexes.Count;

                // create triangle fan indices
                AddTriangleFanIndexes(info.FillCount);

                // create draw command
                ref DrawCommand drawCommand0 = ref _drawCommands.Add();

                drawCommand0.UniformOffset = _call.Uniform1;
                drawCommand0.Texture = _call.Texture;

                // TriangleFan
                drawCommand0.DrawCommandType = DrawCommandType.FillStencil;
                drawCommand0.DrawCallType = _call.Type;

                drawCommand0.IndexOffset = _startIndexOffset;
                drawCommand0.IndexCount = _indexes.Count - drawCommand0.IndexOffset;

                drawCommand0.VertexOffset = info.FillOffset;
                drawCommand0.VertexCount = info.FillCount; // not needed
            }

            // Fill

            // create indices first
            _startIndexOffset = _indexes.Count;

            // triangle strip
            AddTriangleStripIndexes(_call.VertexCount);

            // create draw command
            ref DrawCommand drawCommand2 = ref _drawCommands.Add();

            drawCommand2.UniformOffset = _call.Uniform2;
            drawCommand2.Texture = _call.Texture;

            // TriangleStrip
            drawCommand2.DrawCommandType = DrawCommandType.Fill;
            drawCommand2.DrawCallType = _call.Type;

            drawCommand2.IndexOffset = _startIndexOffset;
            drawCommand2.IndexCount = _indexes.Count - drawCommand2.IndexOffset;

            drawCommand2.VertexOffset = _call.VertexOffset;
            drawCommand2.VertexCount = _call.VertexCount; // not needed
        }

        #endregion

        #region ConvexFill

        static void CreateConvexFill()
        {
            // note: can't combine - since can have many disconnected paths & TriangleFan
            // todo: if we originally create TriangleList - we can combine?
            for (var i = 0; i < _call.FillStrokeCount; i++)
            {
                ref FillStrokeInfo info = ref _fillStrokes[_call.FillStrokeOffset + i];

                if (info.FillCount > 0)
                {
                    // create indices first
                    _startIndexOffset = _indexes.Count;

                    // triangle fan - create indices
                    AddTriangleFanIndexes(info.FillCount);

                    // create draw command
                    ref DrawCommand drawCommand0 = ref _drawCommands.Add();
                    
                    drawCommand0.UniformOffset = _call.Uniform1;
                    drawCommand0.Texture = _call.Texture;

                    // TriangleFan
                    drawCommand0.DrawCommandType = DrawCommandType.Triangles;
                    drawCommand0.DrawCallType = _call.Type;

                    drawCommand0.IndexOffset = _startIndexOffset;
                    drawCommand0.IndexCount = _indexes.Count - drawCommand0.IndexOffset;                    
                    
                    drawCommand0.VertexOffset = info.FillOffset;
                    drawCommand0.VertexCount = info.FillCount; // not needed
                }
            }
        }

        #endregion

        #region Stroke

        static void CreateStroke()
        {
            // note: can't combine - since can have many disconnected paths & TriangleStrip
            // todo: if we originally create TriangleList - we can combine?
            for (var i = 0; i < _call.FillStrokeCount; i++)
            {
                ref FillStrokeInfo info = ref _fillStrokes[_call.FillStrokeOffset + i];

                if (info.StrokeCount == 0)
                    continue;

                // create indices first
                _startIndexOffset = _indexes.Count;

                // triangle strip
                AddTriangleStripIndexes(info.StrokeCount);

                // create draw command
                ref DrawCommand drawCommand = ref _drawCommands.Add();

                drawCommand.UniformOffset = _call.Uniform1;
                drawCommand.Texture = _call.Texture;

                // TriangleStrip
                drawCommand.DrawCommandType = DrawCommandType.Triangles;
                drawCommand.DrawCallType = _call.Type;

                drawCommand.IndexOffset = _startIndexOffset;
                drawCommand.IndexCount = _indexes.Count - drawCommand.IndexOffset;

                drawCommand.VertexOffset = info.StrokeOffset;
                drawCommand.VertexCount = info.StrokeCount; // not needed
            }
        }

        #endregion

        #region Text

        static void CreateText()
        {
            if (_call.VertexCount == 0)
                return;

            // create indices first
            _startIndexOffset = _indexes.Count;

            // quad indices
            AddQuadIndexes(_call.VertexCount);

            // create draw command
            ref DrawCommand drawCommand = ref _drawCommands.Add();

            drawCommand.UniformOffset = _call.Uniform1;
            drawCommand.Texture = _call.Texture;

            // TriangleList
            drawCommand.DrawCommandType = DrawCommandType.Triangles;
            drawCommand.DrawCallType = _call.Type;

            drawCommand.IndexOffset = _startIndexOffset;
            drawCommand.IndexCount = _indexes.Count - drawCommand.IndexOffset;

            drawCommand.VertexOffset = _call.VertexOffset;
            drawCommand.VertexCount = _call.VertexCount; // not needed
        }

        #endregion
    }
}
