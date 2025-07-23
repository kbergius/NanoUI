using NanoUI.Common;
using System;
using System.Numerics;

namespace NanoUI
{
    public interface INvgRenderer
    {
        int CreateTexture(string path, TextureFlags textureFlags = 0);

        // this is used for creating textures for images (RGBA) & font atlas (R)
        int CreateTexture(TextureDesc description);
        bool UpdateTexture(int texture, ReadOnlySpan<byte> data);
        bool ResizeTexture(int texture, TextureDesc description);

        // this is called also when font atlas texture is resized
        // (delete old, create new)
        bool DeleteTexture(int texture);

        // todo: we should pass only region data (now gets all texture data)
        // rect is (x, y, width, height)
        bool UpdateTextureRegion(int texture, Vector4 regionRect, ReadOnlySpan<byte> allData);

        bool GetTextureSize(int texture, out Vector2 size);

        // triggerred from NvgContext when EndFrame called
        // note: NvgContext clears DrawCache after it has called this method
        void Render();
    }
}
