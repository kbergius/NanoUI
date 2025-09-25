using NanoUI.Common;
using System;
using System.Numerics;

namespace NanoUI
{
    /// <summary>
    /// NanoUI knows nothing about your graphics system.
    /// So you must have INvgRenderer implementation, that handles rendering to your
    /// graphics system.
    /// Note: You must send this implementation to NanoUI, when you init NvgContext.
    /// </summary>
    public interface INvgRenderer
    {
        /// <summary>
        /// Create a texture from the specified path with texture options (flags).
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="textureFlags">Texture flags</param>
        /// <returns>Texture id</returns>
        /// <remarks>
        /// Path can be any kind of identification you like, since NanoUI passes it as-is.
        /// </remarks>
        int CreateTexture(string path, TextureFlags textureFlags = 0);

        /// <summary>
		/// Create a texture from the texture description.
		/// </summary>
        /// <param name="description">Texture description</param>
        /// <returns>Texture id</returns>
        int CreateTexture(TextureDesc description);

        /// <summary>
        /// Update a texture with data.
        /// </summary>
        /// <param name="texture">Texture id</param>
        /// <param name="data">Data</param>
        /// <returns></returns>
        /// <remarks>Data is in 8-bit format when updating font texture.</remarks>
        bool UpdateTexture(int texture, ReadOnlySpan<byte> data);

        /// <summary>
        /// Resize a texture according to texture description.
        /// </summary>
        /// <param name="texture">Texture id</param>
        /// <param name="description">Texture description</param>
        /// <returns>Success</returns>
        /// <remarks>
        /// You can copy data from the old texture to the new, but NanoUI is
        /// not demanding it when font texture is resized
        /// (NanoUI calls UpdateTexture command with all the new data after this).
        /// </remarks>
        bool ResizeTexture(int texture, TextureDesc description);

        /// <summary>
		/// Delete a texture
		/// </summary>
        /// <param name="texture">Texture id</param>
        /// <returns>Success</returns>
        bool DeleteTexture(int texture);

        /// <summary>
        /// Get a texture size (width, height)
        /// </summary>
        /// <param name="texture">Texture id</param>
        /// <param name="size">Texture size</param>
        /// <returns>Success</returns>
        bool GetTextureSize(int texture, out Vector2 size);

        /// <summary>
		/// Triggerred from NvgContext when EndFrame called.
        /// DrawCache is cleared after rendering is done.
		/// </summary>
        void Render();
    }
}
