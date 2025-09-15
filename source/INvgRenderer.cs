using NanoUI.Common;
using System;
using System.Numerics;

namespace NanoUI
{
    /// <summary>
    /// NanoUI knows nothing about your OS, graphics system and windowing environment.
    /// So you must have INvgRenderer implementation, that handles rendering to your
    /// graphics system.
    /// Note: You must send this implementation to NanoUI, when you init NvgContext.
    /// </summary>
    public interface INvgRenderer
    {
        /// <summary>
		/// Create a texture from the specified path with texture options (flags).
        /// Note: Path can be any kind of identification you like, since NanoUI
        /// passes it as-is.
		/// </summary>
        int CreateTexture(string path, TextureFlags textureFlags = 0);

        /// <summary>
		/// Create a texture from the texture description.
		/// </summary>
        int CreateTexture(TextureDesc description);

        /// <summary>
		/// Update a texture with data.
        /// Note: Data is in 8-bit format when updating font texture.
		/// </summary>
        bool UpdateTexture(int texture, ReadOnlySpan<byte> data);

        /// <summary>
        /// Resize a texture according to texture desctiption.
        /// Note: you can copy data from the old texture to the new, but NanoUI is
        /// not demanding it when font texture is resized
        /// (NanoUI calls UpdateTexture command with all the new data after this).
        /// </summary>
        bool ResizeTexture(int texture, TextureDesc description);

        /// <summary>
		/// Delete a texture
		/// </summary>
        bool DeleteTexture(int texture);

        /// <summary>
		/// Get a texture size (width, height)
		/// </summary>
        bool GetTextureSize(int texture, out Vector2 size);

        /// <summary>
		/// Triggerred from NvgContext when EndFrame called.
        /// DrawCache is cleared after rendering is done.
		/// </summary>
        void Render();
    }
}
