using System;
using System.Numerics;

namespace NanoUI.Common
{
    // Describes a 32-bit packed color.
    // Stored as ABGR with R in the least significant octet:
    // |-------|-------|-------|-------
    // A       B       G       R

    // todo? : make R,G,B,A readonly - if user must modify -> create new
    public struct Color : IEquatable<Color>
    {
        uint _packedValue;

        public Color()
            : this(0, 0, 0, 0) { }

        internal Color(uint packedValue)
        {
            _packedValue = packedValue;
        }

        /// <summary>
        /// Constructs color from red, green, blue values. Alpha will be (by default) set to 255 (1.0f).
        /// </summary>
        public Color(byte r, byte g, byte b, byte alpha = 255)
        {
            _packedValue = (uint)alpha << 24 | (uint)b << 16 | (uint)g << 8 | r;
        }

        /// <summary>
        /// Constructs color from red, green, blue and alpha values.
        /// </summary>
        public Color(float r, float g, float b, float a)
            :this((byte)(r * 255), (byte)(g * 255), (byte)(b * 255), (byte)(a * 255)) { }

        public Color(Color c, float alpha)
            : this(c.R, c.G, c.B, (byte)(alpha * 255)) { }

        public Color(Vector4 value)
            :this(value.X, value.Y, value.Z, value.W) { }

        public byte R
        {
            get => (byte)_packedValue;
            set => _packedValue = _packedValue & 0xffffff00 | value;
        }

        public byte G
        {
            get => (byte)(_packedValue >> 8);
            set => _packedValue = _packedValue & 0xffff00ff | (uint)value << 8;
        }

        public byte B
        {
            get => (byte)(_packedValue >> 16);
            set => _packedValue = _packedValue & 0xff00ffff | (uint)value << 16;
        }

        public byte A
        {
            get => (byte)(_packedValue >> 24);
            set => _packedValue = _packedValue & 0x00ffffff | (uint)value << 24;
        }

        public uint PackedValue
        {
            get => _packedValue;
            set => _packedValue = value;
        }

        public byte this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return R;
                    case 1: return G;
                    case 2: return B;
                    case 3: return A;
                    default: throw new IndexOutOfRangeException();
                }
            }
            /*set
            {
                switch (index)
                {
                    case 0: R = value; break;
                    case 1: G = value; break;
                    case 2: B = value; break;
                    case 3: A = value; break;
                    default: throw new IndexOutOfRangeException();
                }
            }*/
        }

        public Vector4 ToVector4()
        {
            return new Vector4(R / 255.0f, G / 255.0f, B / 255.0f, A / 255.0f);
        }

        /// <summary>
        /// Computes the luminance as ``l = 0.299r + 0.587g + 0.144b + 0.0a``.  If
        /// the luminance is less than 0.5, white is returned.  If the luminance is
        /// greater than or equal to 0.5, black is returned.  Both returns will have
        /// an alpha component of 1.0.
        /// </summary>
        public Color ContrastingColor()
        {
            //float luminance = (dot(this, new Color(0.299f, 0.587f, 0.144f, 0.0f))) / (255f * 255f);
            float luminance = Vector4.Dot(this.ToVector4(), new Vector4(0.299f, 0.587f, 0.144f, 0.0f));
            //float luminance = (0.2126f * R + 0.7152f * G + 0.0722f * B) / 255f;
            var val = luminance < 0.5f ? 1.0f : 0.0f;
            return new Color(val, val, val, 1.0f);
        }

        public static Color Negative(in Color color)
        {
            return new Color(255 - color.R, 255 - color.G, 255 - color.B, color.A);
        }

        public Color WithAlpha(Color c)
        {
            return A == 0 ? c : this;
        }

        // multiply all but alpha
        public Color MultiplyColors(float multiplier)
        {
            return new Color
                (
                    R * multiplier / 255,
                    G * multiplier / 255,
                    B * multiplier / 255,
                    A / 255
                );
        }

        internal Color MultiplyAlpha(float alpha)
        {
            //float a = A * alpha;

            //return new Color(R, G, B, A);
            return new Color(R, G, B, (byte)(A * alpha));
        }

        /// <summary>
        /// Performs linear interpolation.
        /// </summary>
        public static Color Lerp(in Color value1, in Color value2, float amount)
        {
            return new Color(Vector4.Lerp(value1.ToVector4(), value2.ToVector4(), amount));
        }

        public static Color Clamp(in Color value, in Color min, in Color max)
        {
            return new (Vector4.Clamp(value.ToVector4(), min.ToVector4(), max.ToVector4()));
        }

        public static Color Min(in Color left, in Color right)
        {
            return new Color(Vector4.Min(left.ToVector4(), right.ToVector4()));
        }

        public static Color Max(in Color left, in Color right)
        {
            return new Color(Vector4.Max(left.ToVector4(), right.ToVector4()));
        }

        /// <summary>
        /// Returns color specified by hue, saturation and lightness.
        /// HSL values are all in range [0..1], alpha will be in range [0..255].
        /// </summary>
        public static Color HSLA(float h, float s, float l, byte a)
        {
            h = h % 1.0f;

            if (h < 0.0f)
                h += 1.0f;

            s = Math.Clamp(s, 0.0f, 1.0f);
            l = Math.Clamp(l, 0.0f, 1.0f);

            var m2 = l <= 0.5f ? (l * (1 + s)) : (l + s - l * s);
            var m1 = 2 * l - m2;

            float fr = Math.Clamp(Hue(h + 1.0f / 3.0f, m1, m2), 0.0f, 1.0f);
            float fg = Math.Clamp(Hue(h, m1, m2), 0.0f, 1.0f);
            float fb = Math.Clamp(Hue(h - 1.0f / 3.0f, m1, m2), 0.0f, 1.0f);
            float fa = a / 255.0f;

            return new Color(
                (byte)(int)(fr * 255),
                (byte)(int)(fg * 255),
                (byte)(int)(fb * 255),
                (byte)(int)(fa * 255));
        }

        public static float Hue(float h, float m1, float m2)
        {
            if (h < 0)
                h += 1;

            if (h > 1)
                h -= 1;

            if (h < 1.0f / 6.0f)
                return m1 + (m2 - m1) * h * 6.0f;
            else if (h < 3.0f / 6.0f)
                return m2;
            else if (h < 4.0f / 6.0f)
                return m1 + (m2 - m1) * (2.0f / 3.0f - h) * 6.0f;
            return m1;
        }

        public static bool operator ==(in Color a, in Color b)
        {
            return a.PackedValue == b.PackedValue;
        }

        public static bool operator !=(in Color a, in Color b)
        {
            return a.PackedValue != b.PackedValue;
        }

        public static Color operator -(in Color color)
        {
            return Negative(color);
        }

        public static Color operator +(in Color left, in Color right)
        {
            return new(
                (byte)(left.R + right.R),
                (byte)(left.G + right.G),
                (byte)(left.B + right.B),
                (byte)(left.A + right.A));
        }

        public static Color operator -(in Color left, in Color right)
        {
            return new Color(
                (byte)(left.R - right.R),
                (byte)(left.G - right.G),
                (byte)(left.B - right.B),
                (byte)(left.A - right.A));
        }

        public static Color operator *(in Color value, float scale)
        {
            return new Color(
                (byte)(value.R * scale),
                (byte)(value.G * scale),
                (byte)(value.B * scale),
                (byte)(value.A * scale));
        }

        public static Color operator *(float scale, in Color value)
        {
            return new Color(
                (byte)(value.R * scale),
                (byte)(value.G * scale),
                (byte)(value.B * scale),
                (byte)(value.A * scale));
        }

        public static Color operator *(in Color left, in Color right)
        {
            return new Color(
                (byte)(left.R * right.R),
                (byte)(left.G * right.G),
                (byte)(left.B * right.B),
                (byte)(left.A * right.A));
        }

        public static Color operator /(in Color color, float value)
        {
            return new Color(
                (byte)(color.R / value),
                (byte)(color.G / value),
                (byte)(color.B / value),
                (byte)(color.A / value));
        }

        public override bool Equals(object? obj)
        {
            return obj != null && obj is Color && Equals((Color)obj);
        }

        public bool Equals(Color other)
        {
            return PackedValue == other.PackedValue;
        }

        public override int GetHashCode()
        {
            return (int)PackedValue;
        }

        #region Named Colors

        // Transparent color (R:0,G:0,B:0,A:0).
        public static Color Transparent => new Color(0);

        // AliceBlue color (R:240,G:248,B:255,A:255).
        public static Color AliceBlue => new Color(0xfffff8f0);

        // AntiqueWhite color (R:250,G:235,B:215,A:255).
        public static Color AntiqueWhite => new Color(0xffd7ebfa);

        // Aqua color (R:0,G:255,B:255,A:255).
        public static Color Aqua => new Color(0xffffff00);

        // Aquamarine color (R:127,G:255,B:212,A:255).
        public static Color Aquamarine => new Color(0xffd4ff7f);

        // Azure color (R:240,G:255,B:255,A:255).
        public static Color Azure => new Color(0xfffffff0);

        // Beige color (R:245,G:245,B:220,A:255).
        public static Color Beige => new Color(0xffdcf5f5);

        // Bisque color (R:255,G:228,B:196,A:255).
        public static Color Bisque => new Color(0xffc4e4ff);

        // Black color (R:0,G:0,B:0,A:255).
        public static Color Black => new Color(0xff000000);

        // BlanchedAlmond color (R:255,G:235,B:205,A:255).
        public static Color BlanchedAlmond => new Color(0xffcdebff);

        // Blue color (R:0,G:0,B:255,A:255).
        public static Color Blue => new Color(0xffff0000);

        // BlueViolet color (R:138,G:43,B:226,A:255).
        public static Color BlueViolet => new Color(0xffe22b8a);

        // Brown color (R:165,G:42,B:42,A:255).
        public static Color Brown => new Color(0xff2a2aa5);

        // BurlyWood color (R:222,G:184,B:135,A:255).
        public static Color BurlyWood => new Color(0xff87b8de);

        // CadetBlue color (R:95,G:158,B:160,A:255).
        public static Color CadetBlue => new Color(0xffa09e5f);

        // Chartreuse color (R:127,G:255,B:0,A:255).
        public static Color Chartreuse => new Color(0xff00ff7f);

        // Chocolate color (R:210,G:105,B:30,A:255).
        public static Color Chocolate => new Color(0xff1e69d2);

        // Coral color (R:255,G:127,B:80,A:255).
        public static Color Coral => new Color(0xff507fff);

        // CornflowerBlue color (R:100,G:149,B:237,A:255).
        public static Color CornflowerBlue => new Color(0xffed9564);

        // Cornsilk color (R:255,G:248,B:220,A:255).
        public static Color Cornsilk => new Color(0xffdcf8ff);

        // Crimson color (R:220,G:20,B:60,A:255).
        public static Color Crimson => new Color(0xff3c14dc);

        // Cyan color (R:0,G:255,B:255,A:255).
        public static Color Cyan => new Color(0xffffff00);

        // DarkBlue color (R:0,G:0,B:139,A:255).
        public static Color DarkBlue => new Color(0xff8b0000);

        // DarkCyan color (R:0,G:139,B:139,A:255).
        public static Color DarkCyan => new Color(0xff8b8b00);

        // DarkGoldenrod color (R:184,G:134,B:11,A:255).
        public static Color DarkGoldenrod => new Color(0xff0b86b8);

        // DarkGray color (R:169,G:169,B:169,A:255).
        public static Color DarkGray => new Color(0xffa9a9a9);

        // DarkGreen color (R:0,G:100,B:0,A:255).
        public static Color DarkGreen => new Color(0xff006400);

        // DarkKhaki color (R:189,G:183,B:107,A:255).
        public static Color DarkKhaki => new Color(0xff6bb7bd);

        // DarkMagenta color (R:139,G:0,B:139,A:255).
        public static Color DarkMagenta => new Color(0xff8b008b);

        // DarkOliveGreen color (R:85,G:107,B:47,A:255).
        public static Color DarkOliveGreen => new Color(0xff2f6b55);

        // DarkOrange color (R:255,G:140,B:0,A:255).
        public static Color DarkOrange => new Color(0xff008cff);

        // DarkOrchid color (R:153,G:50,B:204,A:255).
        public static Color DarkOrchid => new Color(0xffcc3299);

        // DarkRed color (R:139,G:0,B:0,A:255).
        public static Color DarkRed => new Color(0xff00008b);

        // DarkSalmon color (R:233,G:150,B:122,A:255).
        public static Color DarkSalmon => new Color(0xff7a96e9);

        // DarkSeaGreen color (R:143,G:188,B:139,A:255).
        public static Color DarkSeaGreen => new Color(0xff8bbc8f);

        // DarkSlateBlue color (R:72,G:61,B:139,A:255).
        public static Color DarkSlateBlue => new Color(0xff8b3d48);

        // DarkSlateGray color (R:47,G:79,B:79,A:255).
        public static Color DarkSlateGray => new Color(0xff4f4f2f);

        // DarkTurquoise color (R:0,G:206,B:209,A:255).
        public static Color DarkTurquoise => new Color(0xffd1ce00);

        // DarkViolet color (R:148,G:0,B:211,A:255).
        public static Color DarkViolet => new Color(0xffd30094);

        // DeepPink color (R:255,G:20,B:147,A:255).
        public static Color DeepPink => new Color(0xff9314ff);

        // DeepSkyBlue color (R:0,G:191,B:255,A:255).
        public static Color DeepSkyBlue => new Color(0xffffbf00);

        // DimGray color (R:105,G:105,B:105,A:255).
        public static Color DimGray => new Color(0xff696969);

        // DodgerBlue color (R:30,G:144,B:255,A:255).
        public static Color DodgerBlue => new Color(0xffff901e);

        // Firebrick color (R:178,G:34,B:34,A:255).
        public static Color Firebrick => new Color(0xff2222b2);

        // FloralWhite color (R:255,G:250,B:240,A:255).
        public static Color FloralWhite => new Color(0xfff0faff);

        // ForestGreen color (R:34,G:139,B:34,A:255).
        public static Color ForestGreen => new Color(0xff228b22);

        // Fuchsia color (R:255,G:0,B:255,A:255).
        public static Color Fuchsia => new Color(0xffff00ff);

        // Gainsboro color (R:220,G:220,B:220,A:255).
        public static Color Gainsboro => new Color(0xffdcdcdc);

        // GhostWhite color (R:248,G:248,B:255,A:255).
        public static Color GhostWhite => new Color(0xfffff8f8);

        // Gold color (R:255,G:215,B:0,A:255).
        public static Color Gold => new Color(0xff00d7ff);

        // Goldenrod color (R:218,G:165,B:32,A:255).
        public static Color Goldenrod => new Color(0xff20a5da);

        // Gray color (R:128,G:128,B:128,A:255).
        public static Color Gray => new Color(0xff808080);

        // Green color (R:0,G:128,B:0,A:255).
        public static Color Green => new Color(0xff008000);

        // GreenYellow color (R:173,G:255,B:47,A:255).
        public static Color GreenYellow => new Color(0xff2fffad);

        // Honeydew color (R:240,G:255,B:240,A:255).
        public static Color Honeydew => new Color(0xfff0fff0);

        // HotPink color (R:255,G:105,B:180,A:255).
        public static Color HotPink => new Color(0xffb469ff);

        // IndianRed color (R:205,G:92,B:92,A:255).
        public static Color IndianRed => new Color(0xff5c5ccd);

        // Indigo color (R:75,G:0,B:130,A:255).
        public static Color Indigo => new Color(0xff82004b);

        // Ivory color (R:255,G:255,B:240,A:255).
        public static Color Ivory => new Color(0xfff0ffff);

        // Khaki color (R:240,G:230,B:140,A:255).
        public static Color Khaki => new Color(0xff8ce6f0);

        // Lavender color (R:230,G:230,B:250,A:255).
        public static Color Lavender => new Color(0xfffae6e6);

        // LavenderBlush color (R:255,G:240,B:245,A:255).
        public static Color LavenderBlush => new Color(0xfff5f0ff);

        // LawnGreen color (R:124,G:252,B:0,A:255).
        public static Color LawnGreen => new Color(0xff00fc7c);

        // LemonChiffon color (R:255,G:250,B:205,A:255).
        public static Color LemonChiffon => new Color(0xffcdfaff);

        // LightBlue color (R:173,G:216,B:230,A:255).
        public static Color LightBlue => new Color(0xffe6d8ad);

        // LightCoral color (R:240,G:128,B:128,A:255).
        public static Color LightCoral => new Color(0xff8080f0);

        // LightCyan color (R:224,G:255,B:255,A:255).
        public static Color LightCyan => new Color(0xffffffe0);

        // LightGoldenrodYellow color (R:250,G:250,B:210,A:255).
        public static Color LightGoldenrodYellow => new Color(0xffd2fafa);

        // LightGray color (R:211,G:211,B:211,A:255).
        public static Color LightGray => new Color(0xffd3d3d3);

        // LightGreen color (R:144,G:238,B:144,A:255).
        public static Color LightGreen => new Color(0xff90ee90);

        // LightPink color (R:255,G:182,B:193,A:255).
        public static Color LightPink => new Color(0xffc1b6ff);

        // LightSalmon color (R:255,G:160,B:122,A:255).
        public static Color LightSalmon => new Color(0xff7aa0ff);

        // LightSeaGreen color (R:32,G:178,B:170,A:255).
        public static Color LightSeaGreen => new Color(0xffaab220);

        // LightSkyBlue color (R:135,G:206,B:250,A:255).
        public static Color LightSkyBlue => new Color(0xffface87);

        // LightSlateGray color (R:119,G:136,B:153,A:255).
        public static Color LightSlateGray => new Color(0xff998877);

        // LightSteelBlue color (R:176,G:196,B:222,A:255).
        public static Color LightSteelBlue => new Color(0xffdec4b0);

        // LightYellow color (R:255,G:255,B:224,A:255).
        public static Color LightYellow => new Color(0xffe0ffff);

        // Lime color (R:0,G:255,B:0,A:255).
        public static Color Lime => new Color(0xff00ff00);

        // LimeGreen color (R:50,G:205,B:50,A:255).
        public static Color LimeGreen => new Color(0xff32cd32);

        // Linen color (R:250,G:240,B:230,A:255).
        public static Color Linen => new Color(0xffe6f0fa);

        // Magenta color (R:255,G:0,B:255,A:255).
        public static Color Magenta => new Color(0xffff00ff);

        // Maroon color (R:128,G:0,B:0,A:255).
        public static Color Maroon => new Color(0xff000080);

        // MediumAquamarine color (R:102,G:205,B:170,A:255).
        public static Color MediumAquamarine => new Color(0xffaacd66);

        // MediumBlue color (R:0,G:0,B:205,A:255).
        public static Color MediumBlue => new Color(0xffcd0000);

        // MediumOrchid color (R:186,G:85,B:211,A:255).
        public static Color MediumOrchid => new Color(0xffd355ba);

        // MediumPurple color (R:147,G:112,B:219,A:255).
        public static Color MediumPurple => new Color(0xffdb7093);

        // MediumSeaGreen color (R:60,G:179,B:113,A:255).
        public static Color MediumSeaGreen => new Color(0xff71b33c);

        // MediumSlateBlue color (R:123,G:104,B:238,A:255).
        public static Color MediumSlateBlue => new Color(0xffee687b);

        // MediumSpringGreen color (R:0,G:250,B:154,A:255).
        public static Color MediumSpringGreen => new Color(0xff9afa00);

        // MediumTurquoise color (R:72,G:209,B:204,A:255).
        public static Color MediumTurquoise => new Color(0xffccd148);

        // MediumVioletRed color (R:199,G:21,B:133,A:255).
        public static Color MediumVioletRed => new Color(0xff8515c7);

        // MidnightBlue color (R:25,G:25,B:112,A:255).
        public static Color MidnightBlue => new Color(0xff701919);

        // MintCream color (R:245,G:255,B:250,A:255).
        public static Color MintCream => new Color(0xfffafff5);

        // MistyRose color (R:255,G:228,B:225,A:255).
        public static Color MistyRose => new Color(0xffe1e4ff);

        // Moccasin color (R:255,G:228,B:181,A:255).
        public static Color Moccasin => new Color(0xffb5e4ff);

        // NavajoWhite color (R:255,G:222,B:173,A:255).
        public static Color NavajoWhite => new Color(0xffaddeff);

        // Navy color (R:0,G:0,B:128,A:255).
        public static Color Navy => new Color(0xff800000);

        // OldLace color (R:253,G:245,B:230,A:255).
        public static Color OldLace => new Color(0xffe6f5fd);

        // Olive color (R:128,G:128,B:0,A:255).
        public static Color Olive => new Color(0xff008080);

        // OliveDrab color (R:107,G:142,B:35,A:255).
        public static Color OliveDrab => new Color(0xff238e6b);

        // Orange color (R:255,G:165,B:0,A:255).
        public static Color Orange => new Color(0xff00a5ff);

        // OrangeRed color (R:255,G:69,B:0,A:255).
        public static Color OrangeRed => new Color(0xff0045ff);

        // Orchid color (R:218,G:112,B:214,A:255).
        public static Color Orchid => new Color(0xffd670da);

        // PaleGoldenrod color (R:238,G:232,B:170,A:255).
        public static Color PaleGoldenrod => new Color(0xffaae8ee);

        // PaleGreen color (R:152,G:251,B:152,A:255).
        public static Color PaleGreen => new Color(0xff98fb98);

        // PaleTurquoise color (R:175,G:238,B:238,A:255).
        public static Color PaleTurquoise => new Color(0xffeeeeaf);

        // PaleVioletRed color (R:219,G:112,B:147,A:255).
        public static Color PaleVioletRed => new Color(0xff9370db);

        // PapayaWhip color (R:255,G:239,B:213,A:255).
        public static Color PapayaWhip => new Color(0xffd5efff);

        // PeachPuff color (R:255,G:218,B:185,A:255).
        public static Color PeachPuff => new Color(0xffb9daff);

        // Peru color (R:205,G:133,B:63,A:255).
        public static Color Peru => new Color(0xff3f85cd);

        // Pink color (R:255,G:192,B:203,A:255).
        public static Color Pink => new Color(0xffcbc0ff);

        // Plum color (R:221,G:160,B:221,A:255).
        public static Color Plum => new Color(0xffdda0dd);

        // PowderBlue color (R:176,G:224,B:230,A:255).
        public static Color PowderBlue => new Color(0xffe6e0b0);

        // Purple color (R:128,G:0,B:128,A:255).
        public static Color Purple => new Color(0xff800080);

        // Red color (R:255,G:0,B:0,A:255).
        public static Color Red => new Color(0xff0000ff);

        // RosyBrown color (R:188,G:143,B:143,A:255).
        public static Color RosyBrown => new Color(0xff8f8fbc);

        // RoyalBlue color (R:65,G:105,B:225,A:255).
        public static Color RoyalBlue => new Color(0xffe16941);

        // SaddleBrown color (R:139,G:69,B:19,A:255).
        public static Color SaddleBrown => new Color(0xff13458b);

        // Salmon color (R:250,G:128,B:114,A:255).
        public static Color Salmon => new Color(0xff7280fa);

        // SandyBrown color (R:244,G:164,B:96,A:255).
        public static Color SandyBrown => new Color(0xff60a4f4);

        // SeaGreen color (R:46,G:139,B:87,A:255).
        public static Color SeaGreen => new Color(0xff578b2e);

        // SeaShell color (R:255,G:245,B:238,A:255).
        public static Color SeaShell => new Color(0xffeef5ff);

        // Sienna color (R:160,G:82,B:45,A:255).
        public static Color Sienna => new Color(0xff2d52a0);

        // Silver color (R:192,G:192,B:192,A:255).
        public static Color Silver => new Color(0xffc0c0c0);

        // SkyBlue color (R:135,G:206,B:235,A:255).
        public static Color SkyBlue => new Color(0xffebce87);

        // SlateBlue color (R:106,G:90,B:205,A:255).
        public static Color SlateBlue => new Color(0xffcd5a6a);

        // SlateGray color (R:112,G:128,B:144,A:255).
        public static Color SlateGray => new Color(0xff908070);

        // Snow color (R:255,G:250,B:250,A:255).
        public static Color Snow => new Color(0xfffafaff);

        // SpringGreen color (R:0,G:255,B:127,A:255).
        public static Color SpringGreen => new Color(0xff7fff00);

        // SteelBlue color (R:70,G:130,B:180,A:255).
        public static Color SteelBlue => new Color(0xffb48246);

        // Tan color (R:210,G:180,B:140,A:255).
        public static Color Tan => new Color(0xff8cb4d2);

        // Teal color (R:0,G:128,B:128,A:255).
        public static Color Teal => new Color(0xff808000);

        // Thistle color (R:216,G:191,B:216,A:255).
        public static Color Thistle => new Color(0xffd8bfd8);

        // Tomato color (R:255,G:99,B:71,A:255).
        public static Color Tomato => new Color(0xff4763ff);

        // Turquoise color (R:64,G:224,B:208,A:255).
        public static Color Turquoise => new Color(0xffd0e040);

        // Violet color (R:238,G:130,B:238,A:255).
        public static Color Violet => new Color(0xffee82ee);

        // Wheat color (R:245,G:222,B:179,A:255).
        public static Color Wheat => new Color(0xffb3def5);

        // White color (R:255,G:255,B:255,A:255).
        public static Color White => new Color(uint.MaxValue);

        // WhiteSmoke color (R:245,G:245,B:245,A:255).
        public static Color WhiteSmoke => new Color(0xfff5f5f5);

        // Yellow color (R:255,G:255,B:0,A:255).
        public static Color Yellow => new Color(0xff00ffff);

        // YellowGreen color (R:154,G:205,B:50,A:255).
        public static Color YellowGreen => new Color(0xff32cd9a);

        #endregion
    }
}
