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

        // The basic purpose here is to just resize texture. It doesn't necessarily
        // copy data from the old texture to the new one. You can of course do it, if you like.
        // note: NanoUI uses this function - when it wants to resize font atlas texture -
        // this way: it first calls ResizeTexture and then calls UpdateTexture with new data.
        bool ResizeTexture(int texture, TextureDesc description);

        // this is called also when font atlas texture is resized
        // (delete old, create new)
        bool DeleteTexture(int texture);

        bool GetTextureSize(int texture, out Vector2 size);

        // triggerred from NvgContext when EndFrame called
        // note: NvgContext clears DrawCache after it has called this method
        void Render();
    }
}
