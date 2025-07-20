using Nano;
using Nano.Graphics;
using Nano.Storage;
using NanoUI;
using NanoUI.Rendering;
using NanoUI.Rendering.Data;
using System;
using System.Numerics;
using Color = Nano.Graphics.Color;
using CommandBuffer = Nano.Graphics.CommandBuffer;

namespace SDL3Example
{
    public partial class NanoUIRenderer : INvgRenderer
    {
        const bool _resourceUploaderCycle = true;

        Window _window;
        GraphicsDevice _gd;
        TitleStorage _rootTitleStorage;
        Matrix4x4 _tranformMatrix;
        Color _clearColor = new Color(0.3f, 0.3f, 0.32f, 1);
        Viewport _viewport;
        Rect _scissorRect;
        ResourceUploader _resourceUploader;

        public void Init(Game game)
        {
            _gd = game.GraphicsDevice;
            _window = game.MainWindow;
            _rootTitleStorage = game.RootTitleStorage;
            _resourceUploader = new ResourceUploader(_gd);

            // init resources
            InitBuffers();
            InitTextures();
            InitPipelines();

            // transform matrix & viewport & scissors rect
            CreateTransformMatrix();
        }

        // triggerred from NvgContext when EndFrame called
        // note: NvgContext clears DrawCache after it has called this method
        public void Render()
        {
            CommandBuffer cmdbuf = _gd.AcquireCommandBuffer();
            Texture swapchainTexture = cmdbuf.AcquireSwapchainTexture(_window);

            if (swapchainTexture != null)
            {
                // Update vertices & indices buffers
                if (_vertexBuffer.Size < DrawCache.Vertices.Length * Vertex.SizeInBytes)
                {
                    _vertexBuffer?.Dispose();
                    _vertexBuffer = _resourceUploader.CreateBuffer(DrawCache.Vertices, BufferUsageFlags.Vertex);
                }
                else
                {
                    _resourceUploader.SetBufferData(_vertexBuffer, 0, DrawCache.Vertices, _resourceUploaderCycle);
                }

                if (_indexBuffer.Size < DrawCache.Indexes.Length * sizeof(ushort))
                {
                    _indexBuffer?.Dispose();
                    _indexBuffer = _resourceUploader.CreateBuffer(DrawCache.Indexes, BufferUsageFlags.Index);
                }
                else
                {
                    _resourceUploader.SetBufferData(_indexBuffer, 0, DrawCache.Indexes, _resourceUploaderCycle);
                }

                // upload all pending data
                _resourceUploader.Upload();

                // transform - vertex uniform
                cmdbuf.PushVertexUniformData(_tranformMatrix);

                // do rendering
                DoRender(cmdbuf, swapchainTexture);
            }

            _gd.Submit(cmdbuf);
        }

       
        void DoRender(CommandBuffer cmdbuf, Texture swapchainTexture)
        {
            // previous params
            DrawCommandType? _previousDrawCommandType = null;
            int _previousTexture = -1; // if below 0, gets null binding
            int _uniformOffset = -1;
            bool _updateTextureBinding = true;

            // get uniforms once
            ReadOnlySpan<FragmentUniform> uniforms = DrawCache.Uniforms;

            // begin & clear
           var renderPass = cmdbuf.BeginRenderPass(
                new DepthStencilTargetInfo(_depthStencilTexture, 0, 0, true),
                new ColorTargetInfo(swapchainTexture, _clearColor));

            // set viewport & scissors (needed?)
            // note: scissors if nano ui provides it?
            renderPass.SetViewport(_viewport);
            renderPass.SetScissor(_scissorRect);

            // bind basic buffers
            renderPass.BindVertexBuffers(_vertexBuffer);
            renderPass.BindIndexBuffer(_indexBuffer, IndexElementSize.Sixteen); // ushort
            renderPass.BindVertexStorageBuffers(_transformBuffer);
            renderPass.BindFragmentStorageBuffers(_fragmentUniformBuffer);

            // loop draw commands
            foreach (var drawCommand in DrawCache.DrawCommands)
            {
                // uniform changes?
                if (_uniformOffset != drawCommand.UniformOffset)
                {
                    _uniformOffset = drawCommand.UniformOffset;

                    // update uniform buffer
                    cmdbuf.PushFragmentUniformData(uniforms[drawCommand.UniformOffset]);
                }

                // pipeline changes?
                if (_previousDrawCommandType != drawCommand.DrawCommandType)
                {
                    // stencil - todo we could just watch if previous stencil ref changed
                    var stencilref = drawCommand.DrawCommandType == DrawCommandType.FillStencil ? 1 : 0;
                    renderPass.SetStencilReference((byte)stencilref);

                    _previousDrawCommandType = drawCommand.DrawCommandType;

                    // must set new pipeline
                    renderPass.BindGraphicsPipeline(GetPipeline(drawCommand.DrawCommandType));

                    // we must rebind texture-sampler pair
                    _updateTextureBinding = true;
                }
                else if (_previousTexture != drawCommand.Texture)
                {
                    // texture changed
                    _updateTextureBinding = true;
                }

                // texture changed?
                if (_updateTextureBinding)
                {
                    _previousTexture = drawCommand.Texture;

                    renderPass.BindFragmentSamplers(GetTextureBinding(drawCommand.Texture));

                    _updateTextureBinding = false;
                }

                // draw
                renderPass.DrawIndexedPrimitives((uint)drawCommand.IndexCount, 1, (uint)drawCommand.IndexOffset, drawCommand.VertexOffset, 0);
            }

            // end
            cmdbuf.EndRenderPass(renderPass);
        }

        public void WindowResize(Vector2 windowSize)
        {
            // todo: do we need more resize
            CreateTransformMatrix();
        }

        // after window size changed we must create new transform matrix
        void CreateTransformMatrix()
        {
            _tranformMatrix = Matrix4x4.CreateOrthographicOffCenter(
                    0f,
                    _window.Width,
                    _window.Height,
                    0.0f,
                    -1.0f,
                    1.0f);

            // window possibly resized so we must create new depth stencil texture in these cases
            CreateDepthStencilTexture();

            // todo: are these needed?
            _viewport = new Viewport(_window.Width, _window.Height);
            _scissorRect = new Rect((int)_window.Width, (int)_window.Height);
        }

        public void Dispose()
        {
            DestroyResources();

            _resourceUploader?.Dispose();
        }
    }
}