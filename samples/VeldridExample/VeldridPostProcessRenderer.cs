using VeldridExample.Shaders;
using System.Numerics;
using Veldrid;

namespace VeldridExample
{
    internal struct FullScreenVertex
    {
        public const byte SizeInBytes = 16;

        public Vector2 Position;
        public Vector2 UV;

        public FullScreenVertex(float x, float y, float u, float v)
        {
            Position.X = x;
            Position.Y = y;
            UV.X = u;
            UV.Y = v;
        }
    }

    // this is mainly used to provide msaa antialiasing
    internal class VeldridPostProcessRenderer
    {
        DeviceBuffer _vertexBuffer;
        DeviceBuffer _indexBuffer;
        Pipeline _pipeline;
        Texture _resolvedTexture;
        ResourceSet _resolvedTextureRS;

        bool _msaa;

        public VeldridPostProcessRenderer(GraphicsDevice gd, Vector2 windowSize, bool msaa)
        {
            _msaa = msaa;
            InitResources(gd, windowSize);
        }

        void InitResources(GraphicsDevice gd, Vector2 windowSize)
        {
            var creator = new VeldridShaderCreator("postprocess.vert", "postprocess.frag");

            var description = new GraphicsPipelineDescription
            {
                BlendState = BlendStateDescription.SingleOverrideBlend,
                DepthStencilState = DepthStencilStateDescription.Disabled,
                RasterizerState = RasterizerStateDescription.Default,
                Outputs = gd.SwapchainFramebuffer.OutputDescription,
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                ResourceBindingModel = ResourceBindingModel.Improved,
                ResourceLayouts = GetShaderResourceLayouts(gd),
                ShaderSet = creator.GetShaderSet(gd)
            };

            _pipeline = gd.ResourceFactory.CreateGraphicsPipeline(description);

            var textDesc = TextureDescription.Texture2D(
                (uint)windowSize.X,
                (uint)windowSize.Y, 1, 1,
                PixelFormat.R8_G8_B8_A8_UNorm,
                TextureUsage.Sampled);

            _resolvedTexture = gd.ResourceFactory.CreateTexture(textDesc);

            var textureRSDesc = new ResourceSetDescription(
                description.ResourceLayouts[0],
               _resolvedTexture,
               gd.Aniso4xSampler);

            _resolvedTextureRS = gd.ResourceFactory.CreateResourceSet(textureRSDesc);

            CreateFullScreenQuad(gd);
        }

        public void WindowResize(GraphicsDevice gd, Vector2 windowSize)
        {
            // todo : we need only really to crete new resolved texture & resourceset
            Dispose();
            InitResources(gd, windowSize);
        }

        public void Render(CommandList commandList, GraphicsDevice gd, Texture colorTexture)
        {
            // MSAA?
            if (_msaa)
            {
                commandList.ResolveTexture(colorTexture, _resolvedTexture);
            }
            else
            {
                commandList.CopyTexture(colorTexture, _resolvedTexture);
            }

            commandList.SetFramebuffer(gd.SwapchainFramebuffer);
            commandList.SetPipeline(_pipeline);
            commandList.SetVertexBuffer(0, _vertexBuffer);
            commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);

            commandList.SetGraphicsResourceSet(0, _resolvedTextureRS);

            commandList.DrawIndexed(6);
        }

        public void Dispose()
        {
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
            _pipeline?.Dispose();
            _resolvedTexture?.Dispose();
            _resolvedTextureRS?.Dispose();
        }

        ResourceLayout[] GetShaderResourceLayouts(GraphicsDevice gd)
        {
            ResourceLayoutDescription[] descriptions = [
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("SourceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment,
                        ResourceLayoutElementOptions.None),
                    new ResourceLayoutElementDescription("SourceSampler", ResourceKind.Sampler, ShaderStages.Fragment,
                        ResourceLayoutElementOptions.None)
                    )
            ];

            ResourceLayout[] layouts = new ResourceLayout[descriptions.Length];

            for (int i = 0; i < layouts.Length; i++)
            {
                layouts[i] = gd.ResourceFactory.CreateResourceLayout(descriptions[i]);
            }

            return layouts;
        }

        void CreateFullScreenQuad(GraphicsDevice gd)
        {
            FullScreenVertex[] vertices =
                [
                        new FullScreenVertex{ Position = new Vector2(-1, 1), UV = new Vector2(0, 0) },
                        new FullScreenVertex{ Position = new Vector2(1, 1), UV = new Vector2(1, 0) },
                        new FullScreenVertex{ Position = new Vector2(1, -1), UV = new Vector2(1, 1) },
                        new FullScreenVertex{ Position = new Vector2(-1, -1), UV = new Vector2(0, 1) }
                ];

            ushort[] quadIndices = [0, 1, 2, 0, 2, 3];

            _vertexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription(
                4 * (uint)FullScreenVertex.SizeInBytes, BufferUsage.VertexBuffer));

            gd.UpdateBuffer(_vertexBuffer, 0, vertices);

            _indexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription(
                (uint)quadIndices.Length * sizeof(ushort), BufferUsage.IndexBuffer));

            gd.UpdateBuffer(_indexBuffer, 0, quadIndices);
        }
    }
}