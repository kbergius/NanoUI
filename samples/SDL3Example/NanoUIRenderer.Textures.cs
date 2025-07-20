using Nano.Graphics;
using NanoUI.Common;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Rect = NanoUI.Common.Rect;
using TextureFormat = Nano.Graphics.TextureFormat;

namespace SDL3Example
{
    public partial class NanoUIRenderer
    {
        // mapping texture ids & textures
        readonly Dictionary<int, Texture> _textures = new();
        
        Texture _depthStencilTexture;

        // used if no texture/textureSampler found
        Texture _nullTexture;
        TextureSamplerBinding _nullTextureSampler;

        // same sampler for all!
        Sampler _sampler;        

        int counter = 0;

        void InitTextures()
        {
            // sampler
            _sampler = Sampler.Create(_gd, SamplerCreateInfo.AnisotropicWrap);

            // create white Texture
            ReadOnlySpan<byte> data = [255, 255, 255, 255];

            _nullTexture = _resourceUploader.CreateTexture2D(data, TextureFormat.R8G8B8A8Unorm,
                TextureUsageFlags.Sampler, 1, 1);

            _resourceUploader.Upload();

            // null texture sampler binding
            _nullTextureSampler = new TextureSamplerBinding(_nullTexture, _sampler);
        }

        void CreateDepthStencilTexture()
        {
            _depthStencilTexture?.Dispose();

            // Create and populate the GPU resources
            _depthStencilTexture = Texture.Create2D(
                _gd,
                _window.Width,
                _window.Height,
                _gd.SupportedDepthStencilFormat,
                TextureUsageFlags.DepthStencilTarget
            );
        }

        // renderer calls
        TextureSamplerBinding GetTextureBinding(int texture)
        {
            if (texture < 0 || !_textures.TryGetValue(texture, out var tex))
            {
                return _nullTextureSampler;
            }

            return new TextureSamplerBinding(tex, _sampler);
        }

        // note: this supports only relative paths directly under Assembly path & PNG images
        public unsafe int CreateTexture(string path, TextureFlags textureFlags = 0)
        {
            // must make relative path without '.' & '/' at the beginning of the path
            StringBuilder sb = new();

            bool validStart = false;

            for(int i = 0; i < path.Length; i++)
            {
                if(!validStart && (path[i]== '.' || path[i] == '/'))
                {
                    continue;
                }

                validStart = true;
                sb.Append(path[i]);
            }

            byte* pixels = ImageUtils.GetPixelDataFromFile(
                    _rootTitleStorage,
                    sb.ToString(),
                    out var width,
                    out var height,
                    out var byteCount
                );

            ReadOnlySpan<byte> data = new(pixels, (int)byteCount);

            var tex = _resourceUploader.CreateTexture2D(data, TextureFormat.R8G8B8A8Unorm,
                TextureUsageFlags.Sampler, width, height);

            _resourceUploader.Upload();

            counter++;

            // add texture
            _textures.Add(counter, tex);

            ImageUtils.FreePixelData(pixels);

            return counter;
        }

        // this is used for creating textures for images (RGBA) & font atlas (R)
        public int CreateTexture(TextureDesc description)
        {
            var texture = Texture.Create2D(
                _gd,
                description.Width,
                description.Height,
                description.Format == NanoUI.Common.TextureFormat.RGBA ? TextureFormat.R8G8B8A8Unorm : TextureFormat.R8Unorm,
                TextureUsageFlags.Sampler);

            // note: id is 1-based (not 0-based), since then we can neglect texture (int) value when
            // serializing theme/ui to file & have all properties with texture as default (= 0)
            // todo: check this!
            counter++;

            // add texture
            _textures.Add(counter, texture);

            return counter;
        }

        public bool UpdateTexture(int texture, ReadOnlySpan<byte> data)
        {
            if (!data.IsEmpty && _textures.TryGetValue(texture, out var tex))
            {
                TextureRegion textureRegion = new TextureRegion(tex);
                _resourceUploader.SetTextureData(textureRegion, data, _resourceUploaderCycle);

                _resourceUploader.Upload();

                return true;
            }

            return false;
        }

        public bool ResizeTexture(int texture, TextureDesc description)
        {
            if (!_textures.TryGetValue(texture, out var tex))
            {
                return false;
            }

            tex?.Dispose();

            var newTexture = Texture.Create2D(
                _gd,
                description.Width,
                description.Height,
                description.Format == NanoUI.Common.TextureFormat.RGBA ? TextureFormat.R8G8B8A8Unorm : TextureFormat.R8Unorm,
                TextureUsageFlags.Sampler);

            _textures[texture] = newTexture;
            
            return true;
        }

        // this is called also when font atlas texture is resized
        // (delete old, create new)
        public bool DeleteTexture(int texture)
        {
            if (_textures.TryGetValue(texture, out var tex))
            {
                tex?.Dispose();

                _textures.Remove(texture);

                return true;
            }

            return false;
        }

        // todo: we should pass only region data (now gets all texture data)
        public bool UpdateTextureRegion(int texture, Rect regionRect, ReadOnlySpan<byte> allData)
        {
            return UpdateTexture(texture, allData);
        }

        public bool GetTextureSize(int texture, out Vector2 size)
        {
            if (_textures.TryGetValue(texture, out var tex))
            {
                size = new Vector2(tex.Width, tex.Height);
                return true;
            }

            size = Vector2.Zero;

            return false;
        }
    }
}