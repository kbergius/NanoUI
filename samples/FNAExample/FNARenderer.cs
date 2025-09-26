using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NanoUI;
using NanoUI.Rendering;
using NanoUI.Rendering.Data;
using System;

namespace FNAExample
{
    public partial class FNARenderer: INvgRenderer
    {
        // graphics device
        readonly GraphicsDevice _device;

        // current uniform in render loop
        FragmentUniform _currentUniform;

        // note: this is just to prevent warnings (all resources could be inited in ctor)
#pragma warning disable CS8618 // disable nullable warning
        public FNARenderer(GraphicsDevice device)
        {
            if (device == null)
                throw new ArgumentNullException("device");

            _device = device;
            
            // init "pipeline" stuff, effect (shader)
            InitPipelines();

            // vertices & indices
            InitBuffers();

            // init no texture found etc
            InitTextures();
        }
#pragma warning restore CS8618 // enable nullable warning

        // triggered from NvgContext when EndFrame called
        public void Render()
        {
            // setup common stuff
            _device.RasterizerState = RasterizerState.CullNone;
            _device.SamplerStates[0] = SamplerState.AnisotropicClamp;
            
            // transform
            var transform = Matrix.CreateOrthographicOffCenter(0, _device.Viewport.Width, _device.Viewport.Height, 0, 0, -1);
            _effect.Parameters["transformMat"].SetValue(transform);

            // Vertices & indices (recreate buffers if they are not big enough)
            UpdateVertices(DrawCache.Vertices);
            UpdateIndices(DrawCache.Indexes);

            // bind vertex & index buffer
            _device.SetVertexBuffer(_vertexBuffer);
            _device.Indices = _indexBuffer;

            // loop draw commands & draw
            DoRender();   
        }

        void DoRender()
        {
            // previous params
            DrawCommandType? _previousDrawCommandType = null;
            int _previousTexture = -1; // if below 0, gets "no texture"
            int _uniformOffset = -1;

            // get all uniforms once
            ReadOnlySpan<FragmentUniform> uniforms = DrawCache.Uniforms;

            // loop draw commands
            foreach (var drawCommand in DrawCache.DrawCommands)
            {
                if (drawCommand.IndexCount <= 0)
                {
                    continue;
                }

                // uniform params
                if (_uniformOffset != drawCommand.UniformOffset)
                {
                    _uniformOffset = drawCommand.UniformOffset;

                    // get uniform
                    _currentUniform = uniforms[drawCommand.UniformOffset];

                    // set uniform params (convert)
                    _effect.Parameters["scissorMat"].SetValue(_currentUniform.ScissorMat.ToFna());
                    _effect.Parameters["scissorExt"].SetValue(_currentUniform.ScissorExt.ToFna());
                    _effect.Parameters["scissorScale"].SetValue(_currentUniform.ScissorScale.ToFna());
                    _effect.Parameters["paintMat"].SetValue(_currentUniform.PaintMat.ToFna());
                    _effect.Parameters["extent"].SetValue(_currentUniform.Extent.ToFna());
                    _effect.Parameters["radius"].SetValue(_currentUniform.Radius);
                    _effect.Parameters["feather"].SetValue(_currentUniform.Feather);
                    _effect.Parameters["innerCol"].SetValue(_currentUniform.InnerCol.ToFna());
                    _effect.Parameters["outerCol"].SetValue(_currentUniform.OuterCol.ToFna());

                    // note: this is not actually used, since shader is split to separate techniques
                    _effect.Parameters["actionType"].SetValue((int)_currentUniform.ActionType);
                    _effect.Parameters["fontSize"].SetValue(_currentUniform.FontSize);

                    // set technique based on action type
                    _effect.CurrentTechnique = _techniques[(int)_currentUniform.ActionType];
                }

                // "pipeline"
                if (_previousDrawCommandType != drawCommand.DrawCommandType)
                {
                    _previousDrawCommandType = drawCommand.DrawCommandType;

                    // set "pipeline" stuff based on DrawCommandType
                    switch (drawCommand.DrawCommandType)
                    {
                        case DrawCommandType.FillStencil:
                            _device.BlendState = _blendStateFillStencil;
                            _device.DepthStencilState = _depthStateFillStencil;
                            break;
                        case DrawCommandType.Fill:
                            _device.BlendState = _blendStateFillTriangles;
                            _device.DepthStencilState = _depthStateFill;
                            break;
                        case DrawCommandType.Triangles:
                        default:
                            _device.BlendState = _blendStateFillTriangles;
                            _device.DepthStencilState = _depthStateTriangles;
                            break;
                    }
                }

                // texture
                if (_previousTexture != drawCommand.Texture)
                {
                    _previousTexture = drawCommand.Texture;

                    // set texture
                    _effect.Parameters["g_texture"].SetValue(GetTexture(drawCommand.Texture));
                }

                // draw
                foreach (var pass in _effect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    // draw indexed
                    _device.DrawIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        drawCommand.VertexOffset,
                        0,
                        drawCommand.VertexCount,
                        drawCommand.IndexOffset,
                        drawCommand.IndexCount / 3);
                }
            }
        }

        // dispose resources
        public void Dispose()
        {
            // buffers
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();

            // textures
            _noTexture?.Dispose();

            foreach (var tex in _textures.Values)
            {
                tex?.Dispose();
            }

            // shader
            _effect?.Dispose();

            // todo: rest are disposed when game disposes?
        }
    }
}
