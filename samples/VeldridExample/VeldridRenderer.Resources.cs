using NanoUI.Rendering;
using NanoUI.Rendering.Data;
using VeldridExample.Shaders;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid;

namespace VeldridExample
{
    internal partial class VeldridRenderer
    {
        const uint INIT_VERTICES_COUNT = 8192;

        DeviceBuffer _vertexBuffer;
        DeviceBuffer _indexBuffer;
        DeviceBuffer _transformBuffer;
        DeviceBuffer _fragmentUniformBuffer;
        ResourceSet _uniformBufferRS;
        Framebuffer _framebuffer;
        Texture _colorTexture;
        Texture _depthTexture;

        // Pipelines
        // note: you must have 3 pipelines:
        // - "normal" pipeline
        // - fill stencil pipeline
        // - fill pipeline
        // and use one based on DrawCommand's DrawCommandType
        GraphicsPipelineDescription _modelPipeline;
        Dictionary<DrawCommandType, Pipeline> _pipelines = new();

        // Texture resourcesets
        ResourceSetDescription _textureRSDesc;
        ResourceSet _nullTextureRS;
        Dictionary<int, ResourceSet> _textureRes = new();

        void InitResources()
        {
            InitFramebuffer();

            InitModelPipeline();

            InitTextures();

            InitBuffers();
        }

        #region Framebuffer

        void InitFramebuffer()
        {
            // msaa / non-msaa
            TextureSampleCount sampleCount = _msaa ? TextureSampleCount.Count4 : TextureSampleCount.Count1;

            var colorDesc = TextureDescription.Texture2D(
                (uint)_windowSize.X,
                (uint)_windowSize.Y, 1, 1,
                PixelFormat.R8_G8_B8_A8_UNorm,
                TextureUsage.Sampled | TextureUsage.RenderTarget,
                sampleCount);

            _colorTexture = _gd.ResourceFactory.CreateTexture(colorDesc);

            // note: create with stencil
            var depthDesc = TextureDescription.Texture2D(
                (uint)_windowSize.X,
                (uint)_windowSize.Y, 1, 1,
                PixelFormat.D24_UNorm_S8_UInt,
                TextureUsage.DepthStencil,
                sampleCount);

            _depthTexture = _gd.ResourceFactory.CreateTexture(depthDesc);

            var framebufferDesc = new FramebufferDescription(_depthTexture, _colorTexture);

            _framebuffer = _gd.ResourceFactory.CreateFramebuffer(framebufferDesc);
        }

        #endregion

        #region Pipelines

        void InitModelPipeline()
        {
            ResourceLayoutDescription[] resourceLayouts = [
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("TransformBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex,
                        ResourceLayoutElementOptions.None),
                    new ResourceLayoutElementDescription("FragmentUniformBuffer", ResourceKind.UniformBuffer, ShaderStages.Fragment,
                        ResourceLayoutElementOptions.None)
                    ),
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("tex", ResourceKind.TextureReadOnly, ShaderStages.Fragment,
                        ResourceLayoutElementOptions.None),
                    new ResourceLayoutElementDescription("samp", ResourceKind.Sampler, ShaderStages.Fragment,
                        ResourceLayoutElementOptions.None)
                    )
            ];

            ResourceLayout[] layouts = new ResourceLayout[resourceLayouts.Length];

            for (int i = 0; i < layouts.Length; i++)
            {
                layouts[i] = _gd.ResourceFactory.CreateResourceLayout(resourceLayouts[i]);
            }

            var creator = new VeldridShaderCreator("shader.vert", "shader.frag");

            // todo: we should use scissor test - if "hardware" sciccors set?
            var rasterizer = new RasterizerStateDescription
            {
                FillMode = PolygonFillMode.Solid,
                CullMode = FaceCullMode.Back, // todo: could this be cull none?
                FrontFace = FrontFace.CounterClockwise,
                DepthClipEnabled = false,
                ScissorTestEnabled = false,
            };

            _modelPipeline = new GraphicsPipelineDescription
            {
                BlendState = BlendStateDescription.SingleAlphaBlend,
                DepthStencilState = DepthStencilStateDescription.Disabled,
                RasterizerState = rasterizer,
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                ShaderSet = creator.GetShaderSet(_gd),
                ResourceLayouts = layouts,
                Outputs = _framebuffer.OutputDescription,
                ResourceBindingModel = ResourceBindingModel.Improved,
            };
        }

        Pipeline GetPipeline(DrawCommandType drawCommandType)
        {
            if (!_pipelines.TryGetValue(drawCommandType, out var pipeline))
            {
                switch (drawCommandType)
                {
                    case DrawCommandType.FillStencil:
                        pipeline = _gd.ResourceFactory.CreateGraphicsPipeline(CreateFillStencilPipeline());
                        break;
                    case DrawCommandType.Fill:
                        pipeline = _gd.ResourceFactory.CreateGraphicsPipeline(CreateFillPipeline());
                        break;
                    // default
                    case DrawCommandType.Triangles:
                        pipeline = _gd.ResourceFactory.CreateGraphicsPipeline(_modelPipeline);
                        break;
                }

                // store
                _pipelines[drawCommandType] = pipeline;
            }

            return pipeline;
        }

        // write to stencil buffer pipeline
        GraphicsPipelineDescription CreateFillStencilPipeline()
        {
            // no draw blend (this is only used here, others use model pipeline blenstate)
            var blendDesc = BlendStateDescription.SingleDisabled;
            blendDesc.AttachmentStates[0].ColorWriteMask = ColorWriteMask.None;

            // special rasterizer (this is only used here, others use model pipeline rasterizer)
            var rasterizerDesc = _modelPipeline.RasterizerState;
            rasterizerDesc.CullMode = FaceCullMode.None;

            // write stencil
            var depthStencilDesc = new DepthStencilStateDescription
            {
                StencilTestEnabled = true,
                StencilWriteMask = 0xff,
                StencilReference = 0,
                StencilReadMask = 0xff,

                StencilFront = new StencilBehaviorDescription
                {
                    Comparison = ComparisonKind.Always,
                    Fail = StencilOperation.Keep,
                    DepthFail = StencilOperation.Keep,
                    Pass = StencilOperation.IncrementAndWrap,
                },

                StencilBack = new StencilBehaviorDescription
                {
                    Comparison = ComparisonKind.Always,
                    Fail = StencilOperation.Keep,
                    DepthFail = StencilOperation.Keep,
                    Pass = StencilOperation.DecrementAndWrap,
                },
            };

            // copy defaults
            var desc = _modelPipeline;
            // set modified
            desc.BlendState = blendDesc;
            desc.RasterizerState = rasterizerDesc;
            desc.DepthStencilState = depthStencilDesc;

            return desc;
        }

        // after fill stencil 
        GraphicsPipelineDescription CreateFillPipeline()
        {
            // copy defaults
            var desc = _modelPipeline;

            // special depth stencil - after fill stencil "reset"
            desc.DepthStencilState = new DepthStencilStateDescription
            {
                StencilTestEnabled = true,
                StencilWriteMask = 0xff,
                StencilReference = 0,
                StencilReadMask = 0xff,

                StencilFront = new StencilBehaviorDescription
                {
                    Comparison = ComparisonKind.NotEqual,
                    Fail = StencilOperation.Zero,
                    DepthFail = StencilOperation.Zero,
                    Pass = StencilOperation.Zero,
                },
                StencilBack = new StencilBehaviorDescription
                {
                    Comparison = ComparisonKind.Never,
                    Fail = StencilOperation.Zero,
                    DepthFail = StencilOperation.Zero,
                    Pass = StencilOperation.Zero,
                },
            };

            return desc;
        }

        #endregion

        #region Resourcesets

        unsafe void InitTextures()
        {
            // create white Texture
            RgbaByte color = RgbaByte.White;
            var nullTexture = _gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
                    1, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));

            _gd.UpdateTexture(nullTexture, (nint)(&color), 4, 0, 0, 0, 1, 1, 1, 0, 0);

            _textureRSDesc = new ResourceSetDescription(
                _modelPipeline.ResourceLayouts[1],
               nullTexture,
               _gd.LinearSampler); // todo overridden!

            _nullTextureRS = _gd.ResourceFactory.CreateResourceSet(_textureRSDesc);
        }

        ResourceSet GetTextureRS(int texture)
        {
            if (texture < 0)
            {
                return _nullTextureRS;
            }

            if (!_textureRes.TryGetValue(texture, out var rs))
            {
                if (!TryGetTexture(texture, out var tex, out var sampler))
                {
                    return _nullTextureRS;
                }

                _textureRSDesc.BoundResources[0] = tex;
                _textureRSDesc.BoundResources[1] = sampler;

                rs = _gd.ResourceFactory.CreateResourceSet(_textureRSDesc);

                _textureRes[texture] = rs;
            }

            return rs;
        }

        public void DeleteTextureRS(int texture)
        {
            if (_textureRes.ContainsKey(texture))
            {
                _textureRes.Remove(texture);
            }
        }

        #endregion

        #region Buffers

        void InitBuffers()
        {
            var factory = _gd.ResourceFactory;

            _vertexBuffer = factory.CreateBuffer(new BufferDescription(
                INIT_VERTICES_COUNT * Vertex.SizeInBytes, BufferUsage.VertexBuffer));

            // size is an approx
            _indexBuffer = factory.CreateBuffer(new BufferDescription(
                INIT_VERTICES_COUNT * 3 * sizeof(ushort), BufferUsage.IndexBuffer));

            _transformBuffer = factory.CreateBuffer(new BufferDescription(
                (uint)Unsafe.SizeOf<Matrix4x4>(), BufferUsage.UniformBuffer));

            _fragmentUniformBuffer = factory.CreateBuffer(new BufferDescription(
                (uint)Unsafe.SizeOf<FragmentUniform>(), BufferUsage.UniformBuffer));

            _uniformBufferRS = factory.CreateResourceSet(new ResourceSetDescription(
               _modelPipeline.ResourceLayouts[0],
               _transformBuffer,
               _fragmentUniformBuffer));
        }

        void UpdateVertices(ReadOnlySpan<Vertex> vertices)
        {
            // check buffer size
            if (_vertexBuffer.SizeInBytes < vertices.Length * Vertex.SizeInBytes)
            {
                uint newVerticesCount = (uint)vertices.Length * 2;

                _vertexBuffer?.Dispose();

                _vertexBuffer = _gd.ResourceFactory.CreateBuffer(new BufferDescription(
                    newVerticesCount * Vertex.SizeInBytes,
                    BufferUsage.VertexBuffer));
            }

            _commandList.UpdateBuffer(_vertexBuffer, 0, vertices);
        }

        void UpdateIndices(ReadOnlySpan<ushort> indices)
        {
            // check buffer size
            if (_indexBuffer.SizeInBytes < indices.Length * sizeof(ushort))
            {
                uint newIndicesCount = (uint)indices.Length * 2;

                _indexBuffer?.Dispose();

                _indexBuffer = _gd.ResourceFactory.CreateBuffer(new BufferDescription(
                    newIndicesCount * sizeof(ushort),
                    BufferUsage.IndexBuffer));
            }

            _commandList.UpdateBuffer(_indexBuffer, 0, indices);
        }

        #endregion

        void ResizeResources()
        {
            // Recreate framebuffer, framebuffer textures
            _framebuffer?.Dispose();
            _colorTexture?.Dispose();
            _depthTexture?.Dispose();

            // reinit framebuffer
            InitFramebuffer();

            // nothing to do in buffers  & pipelines
        }
    }
}
