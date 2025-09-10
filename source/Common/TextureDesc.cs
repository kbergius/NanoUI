namespace NanoUI.Common
{
    /// <summary>
    /// Texture description. This is passed to renderer when creating textures.
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
        /// TextureFormat. Should be mapped to graphic engine's pixel format.
        /// </summary>
        public TextureFormat Format;

        /// <summary>
        /// TextureFlags. Optional flags to pass.
        /// </summary>
        public TextureFlags TextureFlags;

        public TextureDesc(uint width, uint height, TextureFormat format = TextureFormat.RGBA, TextureFlags flags = 0)
        {
            Width = width;
            Height = height;
            Format = format;
            TextureFlags = flags;
        }
    }
}
