using NanoUI;
using NanoUI.Common;
using StbImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Veldrid;

namespace VeldridExample
{
    internal partial class VeldridRenderer
    {
        // mapping texture ids & textures
        readonly Dictionary<int, Texture> _textures = new();

        int counter = 0;

        // todo: we use same sampler to all textures
        public bool TryGetTexture(int texture, out Texture tex, out Sampler sampler)
        {
            if (_textures.TryGetValue(texture, out tex))
            {
                sampler = _gd.Aniso4xSampler;
                return true;
            }

            sampler = default;
            return false;
        }

        public int CreateTexture(string path, TextureFlags textureFlags = 0)
        {
            // use StbImageSharp to load bytes
            ImageResult result;

            using (Stream stream = File.OpenRead(path))
            {
                result = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            }

            if (result == null)
            {
                return Globals.INVALID;
            }

            // create
            int texture = CreateTexture(new TextureDesc((uint)result.Width, (uint)result.Height));

            // update
            UpdateTexture(texture, result.Data);

            return texture;
        }

        // this is used for creating textures for images (RGBA) & font atlas (R)
        public int CreateTexture(TextureDesc description)
        {
            var desc = TextureDescription.Texture2D(
                description.Width,
                description.Height,
                1, 1,
                MapTextureFormat(description.Format),
                TextureUsage.Sampled);

            // note: id is 1-based (not 0-based), since then we can neglect texture (int) value when
            // serializing theme/ui to file & have all properties with texture as default (= 0)
            counter++;

            _textures.Add(counter, _gd.ResourceFactory.CreateTexture(desc));

            return counter;
        }

        // this is used for resizing existing textures for images (RGBA) & font atlas (R)
        // note: this implemenation does not support TextureFormat.RG & TextureFormat.RGB
        public bool ResizeTexture(int texture, TextureDesc description)
        {
            if (!_textures.TryGetValue(texture, out var tex))
            {
                return false;
            }

            tex?.Dispose();

            var desc = TextureDescription.Texture2D(
                description.Width,
                description.Height,
                1, 1,
                MapTextureFormat(description.Format),
                TextureUsage.Sampled);

            _textures[texture] = _gd.ResourceFactory.CreateTexture(desc);

            return true;
        }

        public bool UpdateTexture(int texture, ReadOnlySpan<byte> data)
        {
            if (!data.IsEmpty && _textures.TryGetValue(texture, out var tex))
            {
                UpdateTextureBytes(tex, data, Vector2.Zero, new Vector2(tex.Width, tex.Height));

                return true;
            }

            return false;
        }

        // this is called also when font atlas texture is resized
        // (delete old, create new)
        public bool DeleteTexture(int texture)
        {
            if (_textures.TryGetValue(texture, out var tex))
            {
                tex?.Dispose();

                // try delete texture resourceset also
                DeleteTextureRS(texture);

                _textures.Remove(texture);

                return true;
            }

            return false;
        }

        static PixelFormat MapTextureFormat(TextureFormat format)
        {
            return format switch
            {
                TextureFormat.R => PixelFormat.R8_UNorm,
                TextureFormat.RG => PixelFormat.R8_G8_UNorm,
                TextureFormat.RGBA => PixelFormat.R8_G8_B8_A8_UNorm,
                _ => PixelFormat.R8_G8_B8_A8_UNorm,
            };
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

        // this is bit complicated, but it handles also cases when bounds & offset is outside
        // current texture dimensions (must create new bigger texture and dispose old)
        Texture UpdateTextureBytes(Texture tex, ReadOnlySpan<byte> bytes, Vector2 offset, Vector2 bounds)
        {
            // if the addition totally fits inside the current texture, update the current texture and return.
            if (bounds.X + offset.X <= tex.Width && bounds.Y + offset.Y <= tex.Height)
            {
                _gd.UpdateTexture(tex, bytes, (uint)offset.X, (uint)offset.Y, 0, (uint)bounds.X, (uint)bounds.Y, 1, 0, 0);

                return tex;
            }

            // Store the current texture for proper disposal at a later time and for reference when setting the new texture up.
            var oldTexture = tex;

            // Create a new texture fit to the correct size.
            var textureDescription = TextureDescription.Texture2D(
                (uint)offset.X + (uint)bounds.X, (uint)offset.Y + (uint)bounds.Y, mipLevels: 1, 1, oldTexture.Format, oldTexture.Usage);

            tex = _gd.ResourceFactory.CreateTexture(textureDescription);

            // Copy the old texture's contents into the new one. We need to use a command list for this unfortunately...
            using (CommandList commandBuffer = _gd.ResourceFactory.CreateCommandList())
            {
                commandBuffer.CopyTexture(oldTexture, 0, 0, 0, 0, 0, tex, 0, 0, 0, 0, 0, oldTexture.Width, oldTexture.Height, oldTexture.Depth, oldTexture.ArrayLayers);
                _gd.SubmitCommands(commandBuffer);
            }

            // Update the texture we just created with the new information
            _gd.UpdateTexture(tex, bytes, 0, 0, 0, (uint)offset.X, (uint)offset.Y, 1, 0, 0);

            // dispose of the old texture and command list used to copy the texture
            _gd.DisposeWhenIdle(oldTexture);

            return tex;
        }
    }
}
