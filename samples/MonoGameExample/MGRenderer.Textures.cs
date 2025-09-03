using Microsoft.Xna.Framework.Graphics;
using NanoUI;
using NanoUI.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace MonoGameExample
{
    // texture handling
    public partial class MGRenderer
    {
        readonly Dictionary<int, Texture2D> _textures = [];
        Texture2D _noTexture;
        int _texCounter = 0;

        void InitTextures()
        {
            // init no texture (pink)
            byte[] noTextureBytes = [255, 192, 203, 255];

            _noTexture = new Texture2D(_device,
                1,
                1,
                false, // mipmap
                SurfaceFormat.Color);

            _noTexture.SetData(noTextureBytes);
        }

        // called when rendering
        Texture GetTexture(int index)
        {
            if (_textures.TryGetValue(index, out var tex))
            {
                return tex;
            }

            return _noTexture;
        }

        // INvgRenderer texture methods

        public int CreateTexture(string path, TextureFlags textureFlags = 0)
        {
            if (!File.Exists(path))
            {
                return Globals.INVALID;
            }

            // note: id is 1-based (not 0-based), since then we can neglect texture (int) value when
            // serializing theme/ui to file & have all properties with texture as default (= 0)
            _texCounter++;

            // create & store texture
            _textures[_texCounter] = Texture2D.FromFile(_device, path);

            return _texCounter;
        }

        // this is used for creating textures for images (RGBA) & font atlas (R)
        public int CreateTexture(TextureDesc description)
        {
            var texture2d = new Texture2D(_device,
                (int)description.Width,
                (int)description.Height,
                false, // mipmap
                MapTextureFormat(description.Format));

            // note: id is 1-based (not 0-based), since then we can neglect texture (int) value when
            // serializing theme/ui to file & have all properties with texture as default (= 0)
            _texCounter++;

            _textures[_texCounter] = texture2d;

            return _texCounter;
        }

        public bool UpdateTexture(int texture, ReadOnlySpan<byte> data)
        {
            if (!data.IsEmpty && _textures.TryGetValue(texture, out var tex))
            {
                switch (tex.Format)
                {
                    case SurfaceFormat.Single:
                        // note: NanoUI's font manager sends data in 8-bit format, when it wants to update
                        // font atlas texture. In this example this surface format is configured to use
                        // Alpha8 format (unsigned A 8-bit format).
                        tex.SetData(data.ToArray());
                        break;
                    case SurfaceFormat.Rg32:
                        // 32-bit - 16-bits per channel
                        // not implemented
                        throw new NotImplementedException("Rg32 (32-bit - 16-bits per channel) is not implemented") ;
                    case SurfaceFormat.Color:
                    default:
                        // 32-bit ARGB - 8 bits per channel
                        // we assume that data size is correct
                        tex.SetData(data.ToArray());
                        break;
                }

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

            var texture2d = new Texture2D(_device,
                (int)description.Width,
                (int)description.Height,
                false, // mipmap
                MapTextureFormat(description.Format));

            _textures[texture] = texture2d;

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

        public bool GetTextureSize(int texture, out System.Numerics.Vector2 size)
        {
            if (_textures.TryGetValue(texture, out var tex))
            {
                size = new System.Numerics.Vector2(tex.Width, tex.Height);
                return true;
            }

            size = System.Numerics.Vector2.Zero;
            return false;
        }

        internal static SurfaceFormat MapTextureFormat(TextureFormat format)
        {
            return format switch
            {
                TextureFormat.R => SurfaceFormat.Alpha8,   // Unsigned A 8-bit format for store 8 bits to alpha channel.
                TextureFormat.RG => SurfaceFormat.Rg32,    // 32-bit - 16-bits per channel
                TextureFormat.RGBA => SurfaceFormat.Color, // 32-bit ARGB - 8 per channel
                _ => SurfaceFormat.Color,
            };
        }
    }
}