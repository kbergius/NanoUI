using NanoUI.Common;
using System;
using System.Numerics;

namespace NanoUI
{
    /// <summary>
    /// INvgRenderer.
    /// </summary>
    public interface INvgRenderer
    {
        /// <summary>
		/// Creates a texture from the specified path with texture options (flags)
		/// </summary>
        int CreateTexture(string path, TextureFlags textureFlags = 0);

        /// <summary>
		/// Creates a texture from the texture description
		/// </summary>
        int CreateTexture(TextureDesc description);

        /// <summary>
		/// Updates a texture with data
		/// </summary>
        bool UpdateTexture(int texture, ReadOnlySpan<byte> data);

        // The basic purpose here is to just resize texture. It doesn't necessarily
        // copy data from the old texture to the new one. You can of course do it, if you like.
        // note: NanoUI uses this function - when it wants to resize font atlas texture -
        // this way: it first calls ResizeTexture and then calls UpdateTexture with new data.

        /// <summary>
		/// Resizes a texture according to texture desctiption
		/// </summary>
        bool ResizeTexture(int texture, TextureDesc description);

        // this is called also when font atlas texture is resized
        // (delete old, create new)

        /// <summary>
		/// Deletes a texture
		/// </summary>
        bool DeleteTexture(int texture);

        /// <summary>
		/// Gets a texture size (width, height)
		/// </summary>
        bool GetTextureSize(int texture, out Vector2 size);

        /// <summary>
		/// Triggerred from NvgContext when EndFrame called.
        /// DrawCache is cleared after this
		/// </summary>
        void Render();
    }
}
