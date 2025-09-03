using Microsoft.Xna.Framework.Graphics;
using NanoUI.Rendering.Data;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MonoGameExample
{
    // resources
    public partial class MGRenderer
    {
        const int INIT_VERTICES_COUNT = 8192;
        const int BUFFER_RESIZE_FACTOR = 2;

        // embedded effect base path
        const string EFFECT_PATH = "MonoGameExample.Resources.Effect";

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
        // note: couldn't use only 1 technique, since pixel shader is too complicated and it didn't compile.
        // (error X5608: Compiled shader code uses too many arithmetic instruction slots ...)
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

        // buffer data
        Vertex[] _vertices;
        ushort[] _indices;

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
            _vertices = new Vertex[_vertexBuffer.VertexCount];
            
            // indices
            _indexBuffer = new IndexBuffer(_device, typeof(ushort), INIT_VERTICES_COUNT * 3, BufferUsage.WriteOnly);
            _indices = new ushort[_indexBuffer.IndexCount];
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
                _vertices = new Vertex[_vertexBuffer.VertexCount];
            }

            // prevent allocations
            // note: must do this way, since MonoGame doesn't have SetData with pointer
            // todo: Buffer.MemoryCopy (unsafe, faster) or Unsafe?
            for (int i = 0; i < vertices.Length; i++)
            {
                _vertices[i].Position = vertices[i].Position;
                _vertices[i].UV = vertices[i].UV;
            }

            // update
            _vertexBuffer.SetData(_vertices);
        }

        public void UpdateIndices(ReadOnlySpan<ushort> indices)
        {
            // check buffer size
            if (_indexBuffer.IndexCount < indices.Length)
            {
                _indexBuffer?.Dispose();

                _indexBuffer = new IndexBuffer(_device, typeof(ushort), indices.Length * BUFFER_RESIZE_FACTOR, BufferUsage.WriteOnly);
                _indices = new ushort[_indexBuffer.IndexCount];
            }

            // prevent allocations
            // note: must do this way, since MonoGame doesn't have SetData with pointer
            // todo: Buffer.MemoryCopy (unsafe) or Unsafe?
            for (int i = 0; i < indices.Length; i++)
            {
                _indices[i] = indices[i];
            }

            // update
            _indexBuffer.SetData(_indices);
        }

        #endregion

        #region Pipelines

        // here we init all "pipelines" spesific stuff
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
            foreach (ShaderTechnique param in Enum.GetValues(typeof(ShaderTechnique)))
                _techniques[(int)param] = _effect.Techniques[param.ToString()];
        }

        static byte[] LoadEmbeddedEffect()
        {
            bool IsOpenGL()
            {
                return (from f in typeof(GraphicsDevice).GetFields(BindingFlags.NonPublic |
                             BindingFlags.Instance)
                        where f.Name == "glFramebuffer"
                        select f).FirstOrDefault() != null;

            }

            // get embedded path
            var path = IsOpenGL() ? $"{EFFECT_PATH}.ogl.mgfxo" : $"{EFFECT_PATH}.dx11.mgfxo";

            var assembly = typeof(MGRenderer).Assembly;

            var ms = new MemoryStream();
            using (var stream = assembly.GetManifestResourceStream(path))
            {
                if (stream != null)
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }

                throw new Exception($"Effect not found in {path}");
            }
        }

        #endregion
    }
}