using NanoUI.Common;
using NanoUI.Fonts.Data;
using System;

namespace NanoUI.Fonts
{
    internal static partial class Fontstash
    {
        static FontAtlas _atlas;
        static UnsafeBuffer<FontAtlasNode> _atlasNodes = new(256);

        static byte[] _atlasData = Array.Empty<byte>();

        static int _atlasTexture = Globals.INVALID;

        // when new glyph created, we configure dirty rect (min, max)
        static readonly int[] _atlasDirtyRect = new int[4];

        // this is used to collect rect values for atlas texture update (min, max)
        static readonly int[] _updateTextureRect = new int[4];

        #region Public API

        public static int GetAtlasTextureId() => _atlasTexture;

        // fonsGetAtlasSize
        public static void GetAtlasSize(out int width, out int height)
        {
            width = _atlas.Width;
            height = _atlas.Height;
        }

        // fonsGetTextureData
        public static ReadOnlySpan<byte> GetTextureData(out int width, out int height)
        {
            width = _atlas.Width;
            height = _atlas.Height;

            return _atlasData;
        }

        // fonsExpandAtlas, fons__atlasExpand
        public static bool ExpandAtlas(int width, int height)
        {
            // calculate new width & height
            width = Math.Max(width, _atlas.Width);
            height = Math.Max(height, _atlas.Height);

            if (width == _atlas.Width && height == _atlas.Height)
                return true;

            // texture description
            var desc = new TextureDesc
            {
                Format = TextureFormat.R,
                Height = (uint)height,
                Width = (uint)width,
                TextureFlags = 0,
            };

            // check if there is existing texture
            if (_atlasTexture == Globals.INVALID)
            {
                // create new texture
                _atlasTexture = _nvgRenderer.CreateTexture(desc);
            }
            else
            {
                // resize existing texture
                _nvgRenderer.ResizeTexture(_atlasTexture, desc);
            }

            // Copy old texture data over
            byte[] data = new byte[width * height];
            
            for (int i = 0; i < _atlas.Height; i++)
            {
                int dstIdx = i * width;
                int srcIdx = i * _atlas.Width;

                Array.Copy(_atlasData, srcIdx, data, dstIdx, _atlas.Width);

                if (width > _atlas.Width)
                {
                    // todo: maybe this is not needed, since default(bool) = 0?
                    Array.Fill<byte>(data, 0, dstIdx + _atlas.Width, width - _atlas.Width);
                }
            }

            if (height > _atlas.Height)
            {
                // todo: maybe this is not needed, since default(bool) = 0?
                Array.Fill<byte>(data, 0, _atlas.Height * width, (height - _atlas.Height) * width);
            }

            _atlasData = data;

            // Increase atlas size - fons__atlasExpand

            // Insert node for empty space
            if (width > _atlas.Width)
                _atlasInsertNode(_atlasNodes.Count, _atlas.Width, 0, width - _atlas.Width);

            _atlas.Width = width;
            _atlas.Height = height;

            // Add existing data as dirty
            int maxy = 0;

            for (int i = 0; i < _atlasNodes.Count; i++)
            {
                maxy = Math.Max(maxy, _atlasNodes[i].Y);
            }

            // we set dirty, so all data is updated when text draw is issued
            _atlasDirtyRect[0] = 0;
            _atlasDirtyRect[1] = 0;
            _atlasDirtyRect[2] = _atlas.Width;
            _atlasDirtyRect[3] = maxy;

            // note: we don't need to update font texture here, since the update is triggered in DrawText
            // function, that is invoked before any real rendering is happening

            return true;
        }

        // fonsResetAtlas, fons__atlasReset
        public static bool ResetAtlas(int width, int height)
        {
            // texture description
            var desc = new TextureDesc
            {
                Format = TextureFormat.R,
                Height = (uint)height,
                Width = (uint)width,
                TextureFlags = 0,
            };

            // check if there is existing texture
            if (_atlasTexture == Globals.INVALID)
            {
                // create new texture
                _atlasTexture = _nvgRenderer.CreateTexture(desc);
            }
            else
            {
                // resize existing texture
                _nvgRenderer.ResizeTexture(_atlasTexture, desc);
            }

            // Reset atlas params - fons__atlasReset
            _atlas.Width = width;
            _atlas.Height = height;

            _atlasNodes.Clear();

            // Init root node
            ref FontAtlasNode node = ref _atlasNodes.Add();
            node.X = 0;
            node.Y = 0;
            node.Width = width;

            // Clear texture data.
            _atlasData = new byte[width * height];

            // Reset dirty rect
            _atlasDirtyRect[0] = width;
            _atlasDirtyRect[1] = height;
            _atlasDirtyRect[2] = 0;
            _atlasDirtyRect[3] = 0;

            // Reset cached glyphs
            for (int i = 0; i < _glyphs.Length; i++)
            {
                _glyphs[i].Clear();
            }

            // Add white rect at 0,0 for debug drawing.
            _addWhiteRect(2, 2);

            return true;
        }

        #endregion

        #region Private

        // fons__allocAtlas
        static void _allocAtlas(int width, int height)
        {
            // note: we could also do here some init
            ResetAtlas(width, height);
        }

        // fons__atlasInsertNode
        static bool _atlasInsertNode(int idx, int x, int y, int w)
        {
            ref FontAtlasNode _node = ref _atlasNodes.Insert(idx);

            _node.X = x;
            _node.Y = y;
            _node.Width = w;

            return true;
        }

        // fons__atlasRemoveNode
        static bool _atlasRemoveNode(int idx)
        {
            _atlasNodes.RemoveAt(idx);
            return true;
        }

        // fons__atlasAddSkylineLevel
        static bool _atlasAddSkylineLevel(int idx, int x, int y, int w, int h)
        {
            if (!_atlasInsertNode(idx, x, y + h, w))
                return false;

            for (int i = idx + 1; i < _atlasNodes.Count; i++)
            {
                if (_atlasNodes[i].X < _atlasNodes[i - 1].X + _atlasNodes[i - 1].Width)
                {
                    int shrink = _atlasNodes[i - 1].X + _atlasNodes[i - 1].Width - _atlasNodes[i].X;
                    _atlasNodes[i].X += shrink;
                    _atlasNodes[i].Width -= shrink;

                    if (_atlasNodes[i].Width <= 0)
                    {
                        _atlasRemoveNode(i);
                        i--;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            for (int i = 0; i < _atlasNodes.Count - 1; i++)
            {
                if (_atlasNodes[i].Y == _atlasNodes[i + 1].Y)
                {
                    _atlasNodes[i].Width += _atlasNodes[i + 1].Width;
                    _atlasRemoveNode(i + 1);
                    i--;
                }
            }

            return true;
        }

        // Checks if there is enough space at the location of skyline span 'i',
        // and return the max height of all skyline spans under that at that location,
        // fons__atlasRectFits
        static int _atlasRectFits(int i, int w, int h)
        {
            int x = _atlasNodes[i].X;
            int y = _atlasNodes[i].Y;

            if (x + w > _atlas.Width)
                return Globals.INVALID;

            int spaceLeft = w;

            while (spaceLeft > 0)
            {
                if (i == _atlasNodes.Count)
                    return Globals.INVALID;

                y = Math.Max(y, _atlasNodes[i].Y);

                if (y + h > _atlas.Height)
                    return Globals.INVALID;

                spaceLeft -= _atlasNodes[i].Width;
                i++;
            }

            return y;
        }

        // fons__atlasAddRect
        static bool _atlasAddRect(int rw, int rh, ref int rx, ref int ry)
        {
            int besth = _atlas.Height;
            int bestw = _atlas.Width;
            int besti = -1;
            int bestx = -1;
            int besty = -1;

            // Bottom left fit heuristic
            for (int i = 0; i < _atlasNodes.Count; i++)
            {
                int y = _atlasRectFits(i, rw, rh);

                if (y != -1)
                {
                    if (y + rh < besth || (y + rh == besth && _atlasNodes[i].Width < bestw))
                    {
                        besti = i;
                        bestw = _atlasNodes[i].Width;
                        besth = y + rh;
                        bestx = _atlasNodes[i].X;
                        besty = y;
                    }
                }
            }

            if (besti == -1)
                return false;

            // Perform the actual packing
            if (!_atlasAddSkylineLevel(besti, bestx, besty, rw, rh))
                return false;

            rx = bestx;
            ry = besty;

            return true;
        }

        // fons__addWhiteRect
        static bool _addWhiteRect(int w, int h)
        {
            int currentWidth = _atlas.Width;

            int gx = 0, gy = 0;

            if (!_atlasAddRect(w, h, ref gx, ref gy))
                return false;

            int index = gx + (gy * currentWidth);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    _atlasData[index + x] = 0xff;
                }

                index += currentWidth;
            }

            _atlasDirtyRect[0] = Math.Min(_atlasDirtyRect[0], gx);
            _atlasDirtyRect[1] = Math.Min(_atlasDirtyRect[1], gy);
            _atlasDirtyRect[2] = Math.Max(_atlasDirtyRect[2], gx + w);
            _atlasDirtyRect[3] = Math.Max(_atlasDirtyRect[3], gy + h);

            return true;
        }

        // fonsValidateTexture
        static bool _validateAtlasTexture()
        {
            if (_atlasDirtyRect[0] < _atlasDirtyRect[2] && _atlasDirtyRect[1] < _atlasDirtyRect[3])
            {
                // set update rect
                _updateTextureRect[0] = _atlasDirtyRect[0];
                _updateTextureRect[1] = _atlasDirtyRect[1];
                _updateTextureRect[2] = _atlasDirtyRect[2];
                _updateTextureRect[3] = _atlasDirtyRect[3];

                // reset
                _atlasDirtyRect[0] = _atlas.Width;
                _atlasDirtyRect[1] = _atlas.Height;
                _atlasDirtyRect[2] = 0;
                _atlasDirtyRect[3] = 0;

                return true;
            }

            return false;
        }

        #endregion
    }
}