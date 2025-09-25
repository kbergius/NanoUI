namespace NanoUI.Common
{
    /// <summary>
    /// Texture description is passed to renderer when creating textures.
    /// </summary>
    public struct TextureDesc
    {
        /// <summary>
        /// Width.
        /// </summary>
        public uint Width;

        /// <summary>
        /// Height.
        /// </summary>
        public uint Height;

        /// <summary>
        /// TextureFormat should be mapped to graphic engine's pixel format.
        /// </summary>
        public TextureFormat Format;

        /// <summary>
        /// TextureFlags. Optional flags to pass.
        /// </summary>
        public TextureFlags TextureFlags;

        /// <summary>
        /// Creates TextureDesc.
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="format">TextureFormat</param>
        /// <param name="flags">TextureFlags</param>
        public TextureDesc(uint width, uint height, TextureFormat format = TextureFormat.RGBA, TextureFlags flags = 0)
        {
            Width = width;
            Height = height;
            Format = format;
            TextureFlags = flags;
        }
    }
}
