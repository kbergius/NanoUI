using NanoUI;
using NanoUI.Rendering;
using NanoUI.Rendering.Data;
using System;
using System.Numerics;
using Veldrid;

namespace VeldridExample
{
    internal partial class VeldridRenderer : INvgRenderer, IDisposable
    {
        GraphicsDevice _gd;
        CommandList _commandList;

        Vector2 _windowSize;
        Matrix4x4 _tranformMatrix;
        
        public VeldridRenderer(GraphicsDevice gd, CommandList commandList, Vector2 windowSize)
        {
            _gd = gd;
            _commandList = commandList;
            _windowSize = windowSize;
            
            CreateTransformMatrix();

            InitResources();
        }

        public void WindowResize(Vector2 windowSize)
        {
            // set winfow size - used with transform buffer
            _windowSize = windowSize;

            CreateTransformMatrix();
        }

        #region Render

        public void Render()
        {
            // Update vertices & indices buffers
            // Resizes buffers if they are not big enough
            UpdateVertices(DrawCache.Vertices);
            UpdateIndices(DrawCache.Indexes);

            _commandList.SetVertexBuffer(0, _vertexBuffer);
            _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);

            // Transform matrix
            _commandList.UpdateBuffer(_transformBuffer, 0, ref _tranformMatrix);

            // do the rendering
            DoRender();
        }

        void DoRender()
        {
            // previous params
            DrawCommandType? _previousDrawCommandType = null;
            bool _updateTextureRS = true;
            int _previousTexture = -1; // if below 0, gets null rs
            int _uniformOffset = -1;

            // get uniforms once
            ReadOnlySpan<FragmentUniform> uniforms = DrawCache.Uniforms;

            // loop draw commands
            foreach (var drawCommand in DrawCache.DrawCommands)
            {
                // uniform buffer
                if(_uniformOffset != drawCommand.UniformOffset)
                {
                    _uniformOffset = drawCommand.UniformOffset;

                    // update uniform buffer
                    _commandList.UpdateBuffer(_fragmentUniformBuffer, 0,
                                uniforms[drawCommand.UniformOffset]);
                }

                // pipeline
                if(_previousDrawCommandType != drawCommand.DrawCommandType)
                {
                    _previousDrawCommandType = drawCommand.DrawCommandType;

                    // set new pipeline
                    _commandList.SetPipeline(GetPipeline(drawCommand.DrawCommandType));
                    // must rebind
                    _commandList.SetGraphicsResourceSet(0, _uniformBufferRS);

                    _updateTextureRS = true;
                }
                else if (_previousTexture != drawCommand.Texture)
                {
                    // texture changed
                    _updateTextureRS = true;
                }

                // texture resourceset
                if (_updateTextureRS)
                {
                    _previousTexture = drawCommand.Texture;

                    _commandList.SetGraphicsResourceSet(1, GetTextureRS(drawCommand.Texture));

                    _updateTextureRS = false;
                }

                // draw
                _commandList.DrawIndexed((uint)drawCommand.IndexCount, 1, (uint)drawCommand.IndexOffset, drawCommand.VertexOffset, 0);
            }
        }

        #endregion

        // after window size changed we must create new transform matrix
        void CreateTransformMatrix()
        {
            _tranformMatrix = Matrix4x4.CreateOrthographicOffCenter(
                    0f,
                    _windowSize.X,
                    _windowSize.Y,
                    0.0f,
                    -1.0f,
                    1.0f);
        }

        // dispose resources
        public void Dispose()
        {
            // pipelines
            foreach (var pipeline in _pipelines.Values)
            {
                pipeline?.Dispose();
            }

            // buffers
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
            _transformBuffer?.Dispose();
            _fragmentUniformBuffer?.Dispose();

            // textures
            foreach (var tex in _textures.Values)
            {
                tex?.Dispose();
            }
        }
    }
}
