using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace NanoUI.Common
{
    /// <summary>
    /// NanoUI supports four types of paints: linear gradient, box gradient, radial gradient and image pattern.
    /// These can be used as paints for strokes and fills.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Paint
    {
        public Matrix3x2 Transform;
        public Vector2 Extent;
        public float Radius;
        public float Feather;
        public Color InnerColor;
        public Color OuterColor;
        public int Texture;

        public Paint() { }

        #region Methods

        public void PremultiplyAlpha(float alpha)
        {
            InnerColor = InnerColor.MultiplyAlpha(alpha);
            OuterColor = OuterColor.MultiplyAlpha(alpha);
        }

        public void Copy(in Paint paint)
        {
            Transform = paint.Transform;
            Extent = paint.Extent;
            Radius = paint.Radius;
            Feather = paint.Feather;
            InnerColor = paint.InnerColor;
            OuterColor = paint.OuterColor;
            Texture = paint.Texture;
        }

        // nvg__setPaintColor
        public void Reset(in Color innerColor)
        {
            Reset(innerColor, innerColor);
        }

        public void Reset(in Color innerColor, in Color outerColor)
        {
            Transform = Matrix3x2.Identity;
            InnerColor = innerColor;
            OuterColor = outerColor;
            Texture = Globals.INVALID;
            Extent = Vector2.Zero;
            Radius = 0.0f;
            Feather = 1.0f;
        }

        #endregion

        #region Statics

        #region Solid

        public static Paint SolidPaint(Color color)
        {
            Paint paint = new();

            paint.Transform = Matrix3x2.Identity;

            paint.Extent = Vector2.Zero;

            paint.Radius = 0;
            paint.Feather = 1;

            paint.InnerColor = paint.OuterColor = color;

            paint.Texture = Globals.INVALID;

            return paint;
        }

        #endregion

        #region LinearGradient

        /// <summary>
        /// Creates and returns a linear gradient.
        /// The gradient is transformed by the current transform when it is passed to FillPaint() or StrokePaint().
        /// </summary>
        /// <param name="topLeft">Specifies the start coordinates of the linear gradient.</param>
        /// <param name="size"></param>
        /// <param name="startColor">Specifies the start color.</param>
        /// <param name="endColor">Specifies the end color.</param>
        public static Paint LinearGradient(Vector2 topLeft, Vector2 size, Color startColor, Color endColor, bool horizontal)
        {
            if (horizontal)
            {
                return LinearGradient(topLeft, topLeft + size, startColor, endColor);
            }
            else
            {
                return LinearGradient(topLeft, new Vector2(topLeft.X, topLeft.Y + size.Y), startColor, endColor);
            }
        }

        /// <summary>
        /// Creates and returns a linear gradient.
        /// The gradient is transformed by the current transform when it is passed to FillPaint() or StrokePaint().
        /// </summary>
        /// <param name="startX">Specifies the start x-coordinate of the linear gradient.</param>
        /// <param name="startY">Specifies the start y-coordinate of the linear gradient.</param>
        /// <param name="endX">Specifies the end x-coordinate of the linear gradient.</param>
        /// <param name="endY">Specifies the end y-coordinate of the linear gradient.</param>
        /// <param name="startColor">Specifies the start color.</param>
        /// <param name="endColor">Specifies the end color.</param>
        public static Paint LinearGradient(float startX, float startY, float endX, float endY, Color startColor, Color endColor)
        {
            return LinearGradient(new Vector2(startX, startY), new Vector2(endX, endY), startColor, endColor);
        }

        /// <summary>
        /// Creates and returns a linear gradient.
        /// The gradient is transformed by the current transform when it is passed to FillPaint() or StrokePaint().
        /// </summary>
        /// <param name="start">Specifies the start coordinates of the linear gradient.</param>
        /// <param name="end">Specifies the end coordinates of the linear gradient.</param>
        /// <param name="startColor">Specifies the start color.</param>
        /// <param name="endColor">Specifies the end color.</param>
        public static Paint LinearGradient(Vector2 start, Vector2 end, Color startColor, Color endColor)
        {
            const float large = 1e5f;

            Vector2 delta = end - start;
            float d = MathF.Sqrt(delta.X * delta.X + delta.Y * delta.Y);

            if (d > 0.0001f)
            {
                delta /= d;
            }
            else
            {
                delta = new(0, 1);
            }

            Matrix3x2 transform = new
            (
                delta.Y, -delta.X,
                delta.X, delta.Y,
                start.X - delta.X * large, start.Y - delta.Y * large
            );

            Vector2 extent = new(large, large + d * 0.5f);

            return GetPaint(transform, extent, 0.0f, MathF.Max(1.0f, d), startColor, endColor, Globals.INVALID);
        }

        #endregion

        #region BoxGradient

        /// <summary>
        /// Creates and returns a box gradient. Box gradient is a feathered rounded rectangle, it is useful for rendering
        /// drop shadows or highlights for boxes. Feather defines how blurry the corner of the rectangle is. The gradient
        /// is transformed by the current transform when it is passed to FillPaint() or StrokePaint();
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="cornerRadius">Defines the corner radius.</param>
        /// <param name="feather">Defines the feather.</param>
        /// <param name="innerColor">Inner color of the gradient.</param>
        /// <param name="outerColor">Outer color of the gradient.</param>
        /// <returns></returns>
        public static Paint BoxGradient(float x, float y, float width, float height, float cornerRadius, float feather, Color innerColor, Color outerColor)
        {
            return BoxGradient(new Rect(x, y, width, height), cornerRadius, feather, innerColor, outerColor);
        }

        /// <summary>
        /// Creates and returns a box gradient. Box gradient is a feathered rounded rectangle, it is useful for rendering
        /// drop shadows or highlights for boxes. Feather defines how blurry the corner of the rectangle is. The gradient
        /// is transformed by the current transform when it is passed to FillPaint() or StrokePaint();
        /// </summary>
        /// <param name="boxTopLeft"></param>
        /// <param name="size"></param>
        /// <param name="cornerRadius">Defines the corner radius.</param>
        /// <param name="feather">Defines the feather.</param>
        /// <param name="innerColor">Inner color of the gradient.</param>
        /// <param name="outerColor">Outer color of the gradient.</param>
        public static Paint BoxGradient(Vector2 boxTopLeft, Vector2 size, float cornerRadius, float feather, Color innerColor, Color outerColor)
        {
            return BoxGradient(new Rect(boxTopLeft, size), cornerRadius, feather, innerColor, outerColor);
        }

        /// <summary>
        /// Creates and returns a box gradient. Box gradient is a feathered rounded rectangle, it is useful for rendering
        /// drop shadows or highlights for boxes. Feather defines how blurry the corner of the rectangle is. The gradient
        /// is transformed by the current transform when it is passed to FillPaint() or StrokePaint();
        /// </summary>
        /// <param name="boxTopLeft"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="cornerRadius">Defines the corner radius.</param>
        /// <param name="feather">Defines the feather.</param>
        /// <param name="innerColor">Inner color of the gradient.</param>
        /// <param name="outerColor">Outer color of the gradient.</param>
        /// <returns></returns>
        public static Paint BoxGradient(Vector2 boxTopLeft, float w, float h, float cornerRadius, float feather, Color innerColor, Color outerColor)
        {
            return BoxGradient(new Rect(boxTopLeft.X, boxTopLeft.Y, w, h), cornerRadius, feather, innerColor, outerColor);
        }

        /// <summary>
        /// Creates and returns a box gradient. Box gradient is a feathered rounded rectangle, it is useful for rendering
        /// drop shadows or highlitghts for boxes. Feather defines how blurry the corner of the rectangle is. The gradient
        /// is transformed by the current transform when it is passed to NvgRenderStyle.FillPaint() or NvgRenderStyle.StrokePaint();
        /// </summary>
        /// <param name="box">The rectangle.</param>
        /// <param name="cornerRadius">Defines the corner radius.</param>
        /// <param name="feather">Defines the feather.</param>
        /// <param name="innerColor">Inner color of the gradient.</param>
        /// <param name="outerColor">Outer color of the gradient.</param>
        /// <returns></returns>
        public static Paint BoxGradient(Rect box, float cornerRadius, float feather, Color innerColor, Color outerColor)
        {
            Matrix3x2 transform = Matrix3x2.Identity;
            transform.M31 = box.Center.X;
            transform.M32 = box.Center.Y;

            Vector2 extent = box.Size * 0.5f;

            return GetPaint(transform, extent, cornerRadius, MathF.Max(1.0f, feather), innerColor, outerColor, Globals.INVALID);
        }

        #endregion

        #region RadialGradient

        /// <summary>
        /// Creates and returns a radial gradient.
        /// The gradient is transformed by the current transform when it is passed to NvgRenderStyle.FillPaint() or NvgRenderStyle.StrokePaint().
        /// </summary>
        /// <param name="centerX">The centre x.</param>
        /// <param name="centerY">The centre y.</param>
        /// <param name="innerRadius">Specifies the inner radius of the gradient.</param>
        /// <param name="outerRadius">Specifies the outer radius of the gradient.</param>
        /// <param name="innerColor">Specifies the start color.</param>
        /// <param name="outerColor">Specifies the end color.</param>
        /// <returns></returns>
        public static Paint RadialGradient(float centerX, float centerY, float innerRadius, float outerRadius, Color innerColor, Color outerColor)
        {
            return RadialGradient(new Vector2(centerX, centerY), innerRadius, outerRadius, innerColor, outerColor);
        }

        /// <summary>
        /// Creates and returns a radial gradient.
        /// The gradient is transformed by the current transform when it is passed to NvgRenderStyle.FillPaint() or NvgRenderStyle.StrokePaint().
        /// </summary>
        /// <param name="center">The centre.</param>
        /// <param name="innerRadius">Specifies the inner radius of the gradient.</param>
        /// <param name="outerRadius">Specifies the outer radius of the gradient.</param>
        /// <param name="innerColor">Specifies the start color.</param>
        /// <param name="outerColor">Specifies the end color.</param>
        /// <returns></returns>
        public static Paint RadialGradient(Vector2 center, float innerRadius, float outerRadius, Color innerColor, Color outerColor)
        {
            float r = (innerRadius + outerRadius) * 0.5f;
            float f = outerRadius - innerRadius;

            Matrix3x2 transform = Matrix3x2.Identity;
            transform.M31 = center.X;
            transform.M32 = center.Y;

            Vector2 extent = new(r);

            return GetPaint(transform, extent, r, MathF.Max(1.0f, f), innerColor, outerColor, Globals.INVALID);
        }

        #endregion

        #region ImagePattern

        /// <summary>
        /// Creates and returns an image pattern.
        /// The gradient is transformed by the current transform when it is passed to Nvg.FillPaint() or Nvg.StrokePaint().
        /// Note: if tint color is not spesified (= Color.Transparent), fragment shader neglects tint
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="angleInRadians">Specified rotation around the top-left corner</param>
        /// <param name="texture">Is handle to the texture to render</param>
        /// <param name="tintColor"></param>
        /// <returns>An image pattern.</returns>
        public static Paint ImagePattern(float x, float y, float width, float height, float angleInRadians, int texture, Color tintColor)
        {
            return ImagePattern(new Vector2(x, y), new Vector2(width, height), angleInRadians, texture, tintColor);
        }

        /// <summary>
        /// Creates and returns an image pattern.
        /// The gradient is transformed by the current transform when it is passed to Nvg.FillPaint() or Nvg.StrokePaint().
        /// Note: if tint color is not spesified (= Color.Transparent), fragment shader neglects tint
        /// </summary>
        /// <param name="topLeft"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="angleInRadians">Specified rotation around the top-left corner</param>
        /// <param name="texture">Is handle to the texture to render</param>
        /// <param name="tintColor"></param>
        /// <returns>An image pattern.</returns>
        public static Paint ImagePattern(Vector2 topLeft, float width, float height, float angleInRadians, int texture, Color tintColor)
        {
            return ImagePattern(topLeft, new Vector2(width, height), angleInRadians, texture, tintColor);
        }

        /// <summary>
        /// Creates and returns an image pattern.
        /// The gradient is transformed by the current transform when it is passed to Nvg.FillPaint() or Nvg.StrokePaint().
        /// Note: if tint color is not spesified (= Color.Transparent), fragment shader neglects tint
        /// </summary>
        /// <param name="topLeft"></param>
        /// <param name="size"></param>
        /// <param name="angleInRadians">Specified rotation around the top-left corner</param>
        /// <param name="texture">Is handle to the texture to render</param>
        /// <param name="tintColor"></param>
        /// <returns>An image pattern.</returns>
        public static Paint ImagePattern(Vector2 topLeft, Vector2 size, float angleInRadians, int texture, Color tintColor)
        {
            Matrix3x2 transform = Matrix3x2.CreateRotation(angleInRadians);
            transform.M31 = topLeft.X;
            transform.M32 = topLeft.Y;

            return GetPaint(transform, size, default, default, tintColor, tintColor, texture);
        }

        #endregion

        static Paint GetPaint(Matrix3x2 transform, Vector2 extent, float radius, float feather,
            Color innerColor, Color outerColor, int texture)
        {
            return new()
            {
                Transform = transform,
                Extent = extent,
                Radius = radius,
                Feather = feather,
                InnerColor = innerColor,
                OuterColor = outerColor,
                Texture = texture,
            };
        }

        #endregion
    }
}
