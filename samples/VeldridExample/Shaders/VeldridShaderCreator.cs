using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace VeldridExample.Shaders
{
    internal class VeldridShaderCreator
    {
        const string shaderResourcePath = "VeldridExample.Shaders.";

        readonly byte[] _vertexBytes, _fragmentBytes;

        bool _shader = false;
        // shall we write generated shaders to file?
        bool _writeGeneratedShaders = false;

        public VeldridShaderCreator(string vertexFile, string fragmentFile)
        {
            _shader = vertexFile.StartsWith("shader");

            // load embedded shader codes
            string vertexShader = LoadEmbedded(vertexFile);
            string fragmentShader = LoadEmbedded(fragmentFile);

            var vertexCompilationResult = SpirvCompilation.CompileGlslToSpirv(vertexShader, "VertexShader", ShaderStages.Vertex, new GlslCompileOptions());
            _vertexBytes = vertexCompilationResult.SpirvBytes;

            var fragmentCompilationResult = SpirvCompilation.CompileGlslToSpirv(fragmentShader, "FragmentShader", ShaderStages.Fragment, new GlslCompileOptions());
            _fragmentBytes = fragmentCompilationResult.SpirvBytes;

            if (_shader && _writeGeneratedShaders)
            {
                File.WriteAllBytes("NanoUI.vert.spirv", _vertexBytes);
                File.WriteAllBytes("NanoUI.frag.spirv", _fragmentBytes);
            }
        }

        public ShaderSetDescription GetShaderSet(GraphicsDevice device)
        {
            byte[] vsBytes;
            byte[] fsBytes;

            // Shaders with Specialization Constants
            SpecializationConstant[] specializations = GetShaderSpecializations(device);

            if (device.BackendType == GraphicsBackend.Vulkan)
            {
                vsBytes = _vertexBytes;
                fsBytes = _fragmentBytes;
            }
            else
            {
                CrossCompileTarget target = device.BackendType switch
                {
                    GraphicsBackend.Direct3D11 => CrossCompileTarget.HLSL,
                    GraphicsBackend.Metal => CrossCompileTarget.MSL,
                    GraphicsBackend.OpenGL => CrossCompileTarget.GLSL,
                    GraphicsBackend.OpenGLES => CrossCompileTarget.ESSL,
                    _ => throw new NotImplementedException(),
                };

                CrossCompileOptions options = GetShaderOptions(device, specializations);

                VertexFragmentCompilationResult things = SpirvCompilation.CompileVertexFragment(
                    _vertexBytes,
                    _fragmentBytes,
                    target,
                    options
                );
                vsBytes = Encoding.UTF8.GetBytes(things.VertexShader);
                fsBytes = Encoding.UTF8.GetBytes(things.FragmentShader);

                if (_shader && _writeGeneratedShaders)
                {
                    string ending = string.Empty;

                    switch (device.BackendType)
                    {
                        case GraphicsBackend.OpenGL:
                            ending = "glsl";
                            break;
                        case GraphicsBackend.Direct3D11:
                            ending = "hlsl";
                            break;
                        case GraphicsBackend.Metal:
                            ending = "mtl";
                            break;
                        case GraphicsBackend.OpenGLES:
                            ending = "glsl_es";
                            break;

                    }
                    File.WriteAllText($"NanoUI.vert.{ending}", things.VertexShader);
                    File.WriteAllText($"NanoUI.frag.{ending}", things.FragmentShader);
                }
            }

            Shader vs = device.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Vertex, vsBytes, "main"));
            Shader fs = device.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Fragment, fsBytes, "main"));

            ShaderSetDescription shaderSet = new ShaderSetDescription
            {
                Shaders = [vs, fs],
                VertexLayouts = GetVertexLayouts(),
                Specializations = specializations
            };

            return shaderSet;
        }

        static CrossCompileOptions GetShaderOptions(GraphicsDevice gd, SpecializationConstant[] specializations)
        {
            bool fixClipZ = (gd.BackendType == GraphicsBackend.OpenGL || gd.BackendType == GraphicsBackend.OpenGLES)
                && !gd.IsDepthRangeZeroToOne;
            bool invertY = false;

            return new CrossCompileOptions(fixClipZ, invertY, specializations);
        }

        static SpecializationConstant[] GetShaderSpecializations(GraphicsDevice gd)
        {
            bool glOrGles = gd.BackendType == GraphicsBackend.OpenGL || gd.BackendType == GraphicsBackend.OpenGLES;

            List<SpecializationConstant> specializations =
            [
                new SpecializationConstant(100, gd.IsClipSpaceYInverted),
                new SpecializationConstant(101, glOrGles), // TextureCoordinatesInvertedY
                new SpecializationConstant(102, gd.IsDepthRangeZeroToOne) // ReverseDepthRange
            ];

            PixelFormat swapchainFormat = gd.MainSwapchain.Framebuffer.OutputDescription.ColorAttachments[0].Format;
            bool swapchainIsSrgb = swapchainFormat == PixelFormat.B8_G8_R8_A8_UNorm_SRgb
                || swapchainFormat == PixelFormat.R8_G8_B8_A8_UNorm_SRgb;
            specializations.Add(new SpecializationConstant(103, swapchainIsSrgb));

            return specializations.ToArray();
        }

        // same in ui & postprocess
        // note: if your postprocess FullScreenVertex differs from NanoUI Vertex, you must set this differently
        // for postprocess
        static VertexLayoutDescription[] GetVertexLayouts()
        {
            return
            [
                new VertexLayoutDescription(
                [
                    new VertexElementDescription("vertex",  VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate),
                    new VertexElementDescription("tcoord",  VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate),
                ]),
            ];
        }
        
        static string LoadEmbedded(string shaderFile)
        {
            string resourceName = shaderResourcePath + shaderFile;

            var assembly = typeof(VeldridShaderCreator).GetTypeInfo().Assembly;

            string res;

            using (Stream s = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader sr = new StreamReader(s))
            {
                res = sr.ReadToEnd();
            }

            return res;
        }
    }
}