namespace NanoUI.Common
{
    // this is passed to texture manager to create texture
    public struct TextureDesc
    {
        public uint Width;
        public uint Height;
        public TextureFormat Format;
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