using Microsoft.Xna.Framework.Graphics;
using NanoUI.Rendering.Data;
using System;
using System.IO;

namespace FNAExample
{
    // resources
    public partial class FNARenderer
    {
        const int INIT_VERTICES_COUNT = 8192;
        const int BUFFER_RESIZE_FACTOR = 2;

        // embedded effect path
        const string EFFECT_PATH = "FNAExample.Resources.Effect.fxb";

        // fill stencil blend state (no real drawing)
        BlendState _blendStateFillStencil;

        // fill & triangles blend state
        BlendState _blendStateFillTriangles = BlendState.NonPremultiplied;

        // depth stencil states
        DepthStencilState _depthStateFillStencil;
        DepthStencilState _depthStateFill;
        DepthStencilState _depthStateTriangles = DepthStencilState.None;

        // shader techniques - based on action type in fragment uniform
        // 0 = FillGradient
        // 1 = FillImage
        // 2 = FillStencil
        // 3 = Text (normal)
        // 4 = SDF
        EffectTechnique[] _techniques = new EffectTechnique[5];

        // shader
        Effect _effect;

        // these must match technique names is shader TECHNIQUEs
        enum ShaderTechnique
        {
            FillGradient,
            FillImage,
            FillStencil,
            Text,
            SDF
        }

        #region Vertex & Index Buffer

        // store vertex declaration in case we must resize vertex buffer
        VertexDeclaration _vertexDeclaration;
        VertexBuffer _vertexBuffer;
        IndexBuffer _indexBuffer;

        void InitBuffers()
        {
            // vertex declaration
            // note: this has same layout as NanoUI.Rendering.Data.Vertex
            VertexElement[] elements = [
                new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
                new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
            ];

            // store vertex declaration in case we must resize vertex buffer
            _vertexDeclaration = new VertexDeclaration(elements);

            // vertices
            _vertexBuffer = new VertexBuffer(_device, _vertexDeclaration, INIT_VERTICES_COUNT, BufferUsage.WriteOnly);
                        
            // indices
            _indexBuffer = new IndexBuffer(_device, typeof(ushort), INIT_VERTICES_COUNT * 3, BufferUsage.WriteOnly);
        }

        // update buffers
        public unsafe void UpdateVertices(ReadOnlySpan<Vertex> vertices)
        {
            // check buffer size
            if (_vertexBuffer.VertexCount < vertices.Length)
            {
                _vertexBuffer?.Dispose();

                // create new
                _vertexBuffer = new VertexBuffer(_device, _vertexDeclaration, vertices.Length * BUFFER_RESIZE_FACTOR, BufferUsage.WriteOnly);
            }
                        
            fixed(Vertex* ptr = &vertices[0])
            {
                _vertexBuffer.SetDataPointerEXT(0, (IntPtr)ptr, vertices.Length * Vertex.SizeInBytes, SetDataOptions.None);
            }
        }

        public unsafe void UpdateIndices(ReadOnlySpan<ushort> indices)
        {
            // check buffer size
            if (_indexBuffer.IndexCount < indices.Length)
            {
                _indexBuffer?.Dispose();

                _indexBuffer = new IndexBuffer(_device, typeof(ushort), indices.Length * BUFFER_RESIZE_FACTOR, BufferUsage.WriteOnly);
            }

            fixed (ushort* ptr = &indices[0])
            {
                _indexBuffer.SetDataPointerEXT(0, (IntPtr)ptr, indices.Length * sizeof(ushort), SetDataOptions.None);
            }
        }

        #endregion

        #region Pipelines

        // init all "pipelines" spesific stuff
        void InitPipelines()
        {
            // effect (shader)
            _effect = new Effect(_device, LoadEmbeddedEffect());

            // fill stencil blend (no drawing)
            _blendStateFillStencil = new BlendState
            {
                ColorWriteChannels = ColorWriteChannels.None
            };

            // depth stencils

            // fill stencil
            _depthStateFillStencil = new DepthStencilState
            {
                StencilEnable = true,
                TwoSidedStencilMode = true,
                StencilWriteMask = 0xff,
                ReferenceStencil = 0,
                StencilMask = 0xff,
                StencilFunction = CompareFunction.Always,
                StencilFail = StencilOperation.Keep,
                StencilDepthBufferFail = StencilOperation.Keep,
                StencilPass = StencilOperation.Increment,
                CounterClockwiseStencilFunction = CompareFunction.Always,
                CounterClockwiseStencilFail = StencilOperation.Keep,
                CounterClockwiseStencilDepthBufferFail = StencilOperation.Keep,
                CounterClockwiseStencilPass = StencilOperation.Decrement,
            };

            // fill
            _depthStateFill = new DepthStencilState
            {
                StencilEnable = true,
                TwoSidedStencilMode = false,
                StencilWriteMask = 0xff,
                ReferenceStencil = 0,
                StencilMask = 0xff,
                StencilFunction = CompareFunction.NotEqual,
                StencilFail = StencilOperation.Zero,
                StencilDepthBufferFail = StencilOperation.Zero,
                StencilPass = StencilOperation.Zero,
            };

            // shader techniques
            string[] names = Enum.GetNames<ShaderTechnique>();

            for(int i = 0; i < _techniques.Length; i++)
            {
                _techniques[i] = _effect.Techniques[names[i]];
            }
        }

        static byte[] LoadEmbeddedEffect()
        {
            var assembly = typeof(FNARenderer).Assembly;

            var ms = new MemoryStream();
            using (var stream = assembly.GetManifestResourceStream(EFFECT_PATH))
            {
                if (stream != null)
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }

                throw new Exception($"Effect not found in {EFFECT_PATH}");
            }
        }

        #endregion
    }
}
