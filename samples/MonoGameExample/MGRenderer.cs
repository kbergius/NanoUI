using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NanoUI;
using NanoUI.Rendering;
using NanoUI.Rendering.Data;
using System;

namespace MonoGameExample
{
    public partial class MGRenderer: INvgRenderer
    {
        // graphics device
        readonly GraphicsDevice _device;

        // note: this is just to prevent warnings (all resources could be inited in ctor)
#pragma warning disable CS8618 // disable nullable warning
        public MGRenderer(GraphicsDevice device)
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

        // triggerred from NvgContext when EndFrame called
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
                // uniform params
                if (_uniformOffset != drawCommand.UniformOffset)
                {
                    _uniformOffset = drawCommand.UniformOffset;

                    // get uniform
                    var uniform = uniforms[drawCommand.UniformOffset];

                    // set uniform params
                    _effect.Parameters["scissorMat"].SetValue(uniform.ScissorMat);
                    _effect.Parameters["scissorExt"].SetValue(uniform.ScissorExt);
                    _effect.Parameters["scissorScale"].SetValue(uniform.ScissorScale);
                    _effect.Parameters["paintMat"].SetValue(uniform.PaintMat);
                    _effect.Parameters["extent"].SetValue(uniform.Extent);
                    _effect.Parameters["radius"].SetValue(uniform.Radius);
                    _effect.Parameters["feather"].SetValue(uniform.Feather);
                    _effect.Parameters["innerCol"].SetValue(uniform.InnerCol);
                    _effect.Parameters["outerCol"].SetValue(uniform.OuterCol);
                    // note: this is not actually used, since shader has been split to separate
                    // techniques (didn't compile as 1, because it is too complicated)
                    //_effect.Parameters["actionType"].SetValue(uniform.ActionType);
                    _effect.Parameters["fontSize"].SetValue(uniform.FontSize);

                    // set technique based on action type
                    _effect.CurrentTechnique = _techniques[(int)uniform.ActionType];
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

                    if (drawCommand.IndexCount > 0)
                    {
                        // draw indexed
                        _device.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            drawCommand.VertexOffset,
                            drawCommand.IndexOffset,
                            drawCommand.IndexCount / 3);
                    }
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