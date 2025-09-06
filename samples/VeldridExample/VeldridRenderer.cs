using NanoUI;
using NanoUI.Rendering;
using NanoUI.Rendering.Data;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Extensions.Veldrid;
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
        RgbaFloat _clearColor = new RgbaFloat(0.3f, 0.3f, 0.32f, 1);

        public VeldridRenderer(IWindow window) //, bool msaa)
        {
            _windowSize = new Vector2(window.Size.X, window.Size.Y);
            
            CreateTransformMatrix();

            // Create a device with the "default" Graphics API (Vulkan for linux, DirectX11 for Windows, Metal for MacOS.)
            _gd = window.CreateGraphicsDevice(new GraphicsDeviceOptions(false, PixelFormat.D32_Float_S8_UInt, false, ResourceBindingModel.Improved, true, true));

            _commandList = _gd.ResourceFactory.CreateCommandList();

            InitResources();
        }

        public void WindowResize(Vector2 windowSize)
        {
            // set winfow size - used with transform buffer
            _windowSize = windowSize;
            CreateTransformMatrix();

            // Main framebuffer
            _gd.ResizeMainWindow((uint)windowSize.X, (uint)windowSize.Y);
        }

        #region Render

        public void Render()
        {
            _commandList.Begin();

            _commandList.SetFramebuffer(_gd.SwapchainFramebuffer);

            _commandList.ClearColorTarget(0, _clearColor);
            _commandList.ClearDepthStencil(1, 0x00);

            _commandList.SetFullViewports();

            // todo: scissors rect based on draw command?
            // now scissoring is done in fragment shader
            _commandList.SetFullScissorRects();

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

            _commandList.End();

            _gd.SubmitCommands(_commandList);
            _gd.SwapBuffers();
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

                    // must set new pipeline & uniform rs
                    _commandList.SetPipeline(GetPipeline(drawCommand.DrawCommandType));
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

        public void Dispose()
        {
            _gd.WaitForIdle();

            _commandList?.Dispose();

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

            _gd.Dispose();
        }
    }
}
