using Nano;
using Nano.Graphics;
using NanoUI.Rendering;
using NanoUI.Rendering.Data;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Buffer = Nano.Graphics.Buffer;

namespace SDL3Example
{
    partial class NanoUIRenderer
    {
        const uint INIT_VERTICES_COUNT = 8192;
        const string SHADER_PATH = "Assets/shaders";

        Buffer _vertexBuffer;
        Buffer _indexBuffer;
        Buffer _transformBuffer;
        Buffer _fragmentUniformBuffer;

        GraphicsPipelineCreateInfo _modelPipeline;
        Dictionary<DrawCommandType, GraphicsPipeline> _pipelines = new();

        #region Buffers

        void InitBuffers()
        {
            // buffers
            _vertexBuffer = Buffer.Create<Vertex>(_gd, BufferUsageFlags.Vertex, INIT_VERTICES_COUNT);
            _indexBuffer = Buffer.Create<ushort>(_gd, BufferUsageFlags.Index, INIT_VERTICES_COUNT * 3);
            _transformBuffer = Buffer.Create<Matrix4x4>(_gd, BufferUsageFlags.GraphicsStorageRead, 1);
            _fragmentUniformBuffer = Buffer.Create<FragmentUniform>(_gd, BufferUsageFlags.GraphicsStorageRead, 1);
        }

        #endregion

        #region Pipelines

        // note: we dynamically create graphic pipelines when needed (support for resizing?)
        void InitPipelines()
        {
            // create shaders
            Shader vertShader = ShaderCross.Create(
                _gd,
                _rootTitleStorage,
                $"{SHADER_PATH}/NanoUI.vert.hlsl",
                "main",
                ShaderCross.ShaderFormat.HLSL,
                ShaderStage.Vertex
            );

            Shader fragShader = ShaderCross.Create(
                _gd,
                _rootTitleStorage,
                $"{SHADER_PATH}/NanoUI.frag.hlsl",
                "main",
                ShaderCross.ShaderFormat.HLSL,
                ShaderStage.Fragment
            );

            Logger.LogInfo("Shaders created!");

            // pipelines
            _modelPipeline = new GraphicsPipelineCreateInfo
            {
                TargetInfo = new GraphicsPipelineTargetInfo
                {
                    ColorTargetDescriptions = [
                        new ColorTargetDescription
                        {
                            Format = _window.SwapchainFormat,
                            BlendState = ColorTargetBlendState.NonPremultipliedAlphaBlend
                        }
                    ],
                    DepthStencilFormat = _gd.SupportedDepthStencilFormat,
                    HasDepthStencilTarget = true,
                },
                DepthStencilState = DepthStencilState.Disable,
                MultisampleState = MultisampleState.None,
                PrimitiveType = PrimitiveType.TriangleList,
                RasterizerState = RasterizerState.CCW_CullBack,
                VertexInputState = VertexInputState.CreateSingleBinding<Position2TextureVertex>(0),
                VertexShader = vertShader,
                FragmentShader = fragShader
            };
        }

        GraphicsPipeline GetPipeline(DrawCommandType drawCommandType)
        {
            if (!_pipelines.TryGetValue(drawCommandType, out var pipeline))
            {
                switch (drawCommandType)
                {
                    case DrawCommandType.FillStencil:
                        pipeline = GraphicsPipeline.Create(_gd, CreateFillStencilPipeline());
                        break;
                    case DrawCommandType.Fill:
                        pipeline = GraphicsPipeline.Create(_gd, CreateFillPipeline());
                        break;
                    case DrawCommandType.Triangles:
                    default:
                        pipeline = GraphicsPipeline.Create(_gd, _modelPipeline);
                        break;
                }

                // store
                _pipelines[drawCommandType] = pipeline;
            }

            return pipeline;
        }

        // todo: do not need to configure depth values (only stencil)
        GraphicsPipelineCreateInfo CreateFillStencilPipeline()
        {
            // copy defaults
            var desc = _modelPipeline;

            // no draw blend (this is only used here, others use model pipeline blenstate)
            desc.TargetInfo = new GraphicsPipelineTargetInfo
            {
                ColorTargetDescriptions = [
                    new ColorTargetDescription
                    {
                        Format = _window.SwapchainFormat,
                        BlendState = ColorTargetBlendState.NoWrite
                    }
                ],
                DepthStencilFormat = _gd.SupportedDepthStencilFormat,
                HasDepthStencilTarget = true,
            };

            // special rasterizer (this is only used here, others use model pipeline rasterizer)
            desc.RasterizerState.CullMode = CullMode.None;

            // depth stencil
            desc.DepthStencilState = new DepthStencilState
            {
                WriteMask = 0xff,
                CompareMask = 0xff,
                EnableDepthWrite = false,
                EnableDepthTest = false,
                EnableStencilTest = true,

                FrontStencilState = new StencilOpState
                {
                    CompareOp = CompareOp.Always,
                    FailOp = StencilOp.Keep,
                    DepthFailOp = StencilOp.Keep,
                    PassOp = StencilOp.IncrementAndWrap,
                },

                BackStencilState = new StencilOpState
                {
                    CompareOp = CompareOp.Always,
                    FailOp = StencilOp.Keep,
                    DepthFailOp = StencilOp.Keep,
                    PassOp = StencilOp.DecrementAndWrap,
                },
            };

            return desc;
        }

        GraphicsPipelineCreateInfo CreateFillPipeline()
        {
            // copy defaults
            var desc = _modelPipeline;

            // special depth stencil - after fill stencil
            desc.DepthStencilState = new DepthStencilState
            {
                WriteMask = 0xff,
                CompareMask = 0xff,
                EnableStencilTest = true,
                EnableDepthWrite = false,
                EnableDepthTest = false,
                
                FrontStencilState = new StencilOpState
                {
                    CompareOp = CompareOp.NotEqual,
                    FailOp = StencilOp.Zero,
                    DepthFailOp = StencilOp.Zero,
                    PassOp = StencilOp.Zero,
                },
            };

            return desc;
        }

        #endregion

        void DestroyResources()
        {
            // buffers
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
            _transformBuffer?.Dispose();
            _fragmentUniformBuffer?.Dispose();

            // sampler
            _sampler?.Dispose();

            // pipelines
            foreach (var pipeline in _pipelines.Values)
            {
                pipeline?.Dispose();
            }

            _pipelines.Clear();

            // textures
            foreach (var texture in _textures.Values)
            {
                texture?.Dispose();
            }

            _textures.Clear();

            _depthStencilTexture?.Dispose();
            _nullTexture?.Dispose();
        }
    }

    // note: this has same layout as NanoUI.Rendering.Data.Vertex
    [StructLayout(LayoutKind.Sequential)]
    public struct Position2TextureVertex : IVertexType
    {
        public Vector2 Position;
        public Vector2 TexCoord;

        public Position2TextureVertex(Vector2 position, Vector2 texCoord)
        {
            Position = position;
            TexCoord = texCoord;
        }

        public static VertexElementFormat[] Formats { get; } = new VertexElementFormat[2]
        {
            VertexElementFormat.Float2,
            VertexElementFormat.Float2
        };

        public static uint[] Offsets { get; } =
        [
            0,
            8
        ];

        public override string ToString()
        {
            return Position + " | " + TexCoord;
        }
    }
}