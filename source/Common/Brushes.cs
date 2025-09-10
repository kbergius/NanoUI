using NanoUI.Nvg;
using System.Numerics;
using System.Text.Json.Serialization;

namespace NanoUI.Common
{
    #region BrushBase

    // Json serializer types needed to get right inherited type
    [JsonDerivedType(typeof(BrushBase), typeDiscriminator: "BrushBase")]
    [JsonDerivedType(typeof(BoxGradient), typeDiscriminator: "BoxGradient")]
    [JsonDerivedType(typeof(LinearGradient), typeDiscriminator: "LinearGradient")]
    [JsonDerivedType(typeof(RadialGradient), typeDiscriminator: "RadialGradient")]
    [JsonDerivedType(typeof(SolidBrush), typeDiscriminator: "SolidBrush")]
    [JsonDerivedType(typeof(ImageBrush), typeDiscriminator: "ImageBrush")]
    [JsonDerivedType(typeof(SvgBrush), typeDiscriminator: "SvgBrush")]
    public class BrushBase
    {
        public BrushBase()
            : this(BrushType.Undefinied)
        {

        }

        public BrushBase(BrushType type)
        {
            Type = type;
        }

        #region Properties

        public BrushType Type { get; set; }
        public CornerRadius Rounding { get; set; }
        public float StrokeWidth { get; set; }// = 1;
        public bool Filled { get; set; } //= true;

        #endregion

        // this is for NanoUI widgets
        public virtual void Draw(NvgContext ctx, Vector2 topLeft, Vector2 size, Color? tint)
        {
            // no op - should overridden
        }
    }

    #endregion

    #region BoxGradient

    /// <summary>
    /// BoxGradient brush
    /// </summary>
    public class BoxGradient : BrushBase
    {
        public BoxGradient()
            : base(BrushType.BoxGradient) { }

        public BoxGradient(Color innerColor, Color outerColor, float cornerRadius, float feather = 6)
            :this()
        {
            InnerColor = innerColor;
            OuterColor = outerColor;
            CornerRadius = cornerRadius;
            Feather = feather;
        }

        #region Properties

        /// <summary>
        /// CornerRadius
        /// </summary>
        public float CornerRadius { get; set; }

        /// <summary>
        /// Feather
        /// </summary>
        public float Feather { get; set; } = 6;

        /// <summary>
        /// InnerColor
        /// </summary>
        public Color InnerColor { get; set; }

        /// <summary>
        /// OuterColor
        /// </summary>
        public Color OuterColor { get; set; }

        #endregion

        /// <summary>
        /// Draw
        /// </summary>
        public override void Draw(NvgContext ctx, Vector2 topLeft, Vector2 size, Color? tint)
        {
            ctx.BeginPath();
            ctx.RoundedRect(topLeft, size, CornerRadius);

            var paint = Paint.BoxGradient(topLeft, size.X, size.Y, CornerRadius, Feather,
                InnerColor, OuterColor);

            ctx.FillPaint(paint);
            ctx.Fill();

            // Draw tint?
            if (tint.HasValue && tint.Value.A > 0)
            {
                ctx.BeginPath();
                ctx.RoundedRect(topLeft, size, CornerRadius);
                ctx.FillColor(tint.Value);
                ctx.Fill();
            }
        }
    }

    #endregion

    #region LinearGradient

    /// <summary>
    /// LinearGradient brush
    /// </summary>
    public class LinearGradient : BrushBase
    {
        public LinearGradient()
            : base(BrushType.LinearGradient)
        { }

        public LinearGradient(Color startColor, Color endColor, CornerRadius rounding, bool horizontal = false)
            : this()
        {
            StartColor = startColor;
            EndColor = endColor;
            Horizontal = horizontal;
            Rounding = rounding;
        }

        #region Properties

        /// <summary>
        /// Horizontal or vertical drawing
        /// </summary>
        public bool Horizontal { get; set; }

        /// <summary>
        /// StartColor
        /// </summary>
        public Color StartColor { get; set; }

        /// <summary>
        /// EndColor
        /// </summary>
        public Color EndColor { get; set; }

        #endregion

        /// <summary>
        /// Draw
        /// </summary>
        public override void Draw(NvgContext ctx, Vector2 topLeft, Vector2 size, Color? tint)
        {
            ctx.BeginPath();
            ctx.RoundedRectVarying(topLeft, size, Rounding);
            ctx.FillPaint(Paint.LinearGradient(topLeft, size, StartColor, EndColor, Horizontal));
            ctx.Fill();

            // Draw tint?
            if(tint.HasValue && tint.Value.A > 0)
            {
                ctx.BeginPath();
                ctx.RoundedRectVarying(topLeft, size, Rounding);
                ctx.FillColor(tint.Value);
                ctx.Fill();
            }
        }
    }

    #endregion

    #region RadialGradient

    /// <summary>
    /// RadialGradient brush
    /// </summary>
    public class RadialGradient : BrushBase
    {
        public RadialGradient()
            : base(BrushType.RadialGradient)
        { }

        #region Properties

        /// <summary>
        /// CenterOffset
        /// </summary>
        public Vector2 CenterOffset { get; set; }

        /// <summary>
        /// InnerRadius
        /// </summary>
        public float InnerRadius { get; set; }

        /// <summary>
        /// OuterRadius
        /// </summary>
        public float OuterRadius { get; set; }

        /// <summary>
        /// InnerColor
        /// </summary>
        public Color InnerColor { get; set; }

        /// <summary>
        /// OuterColor
        /// </summary>
        public Color OuterColor { get; set; }

        #endregion

        /// <summary>
        /// Draw
        /// </summary>
        public override void Draw(NvgContext ctx, Vector2 topLeft, Vector2 size, Color? tint)
        {
            // check valid colors & radius values
            if ((InnerColor.A == 0 && OuterColor.A == 0) ||
                (InnerRadius <= 0 && OuterRadius <= 0))
                return;

            Vector2 center = topLeft + size * 0.5f + CenterOffset;

            Paint bgRadial = Paint.RadialGradient(
                center.X, center.Y,
                InnerRadius,
                OuterRadius,
                InnerColor,
                OuterColor);

            ctx.BeginPath();
            ctx.RoundedRectVarying(topLeft, size, Rounding);
            ctx.FillPaint(bgRadial);
            ctx.Fill();

            // Draw tint?
            if (tint.HasValue && tint.Value.A > 0)
            {
                ctx.BeginPath();
                ctx.RoundedRectVarying(topLeft, size, Rounding);
                ctx.FillColor(tint.Value);
                ctx.Fill();
            }
        }
    }

    #endregion

    #region SolidBrush

    /// <summary>
    /// Solid color brush (1 color)
    /// </summary>
    public class SolidBrush : BrushBase
    {
        /// <summary>
        /// Color
        /// </summary>
        public Color Color { get; set; }

        public SolidBrush()
            : base(BrushType.Solid)
        { }

        public SolidBrush(Color color)
            : base(BrushType.Solid)
        {
            Color = color;
        }

        /// <summary>
        /// Draw
        /// </summary>
        public override void Draw(NvgContext ctx, Vector2 topLeft, Vector2 size, Color? tint)
        {
            ctx.BeginPath();
            ctx.RoundedRectVarying(topLeft, size, Rounding);
            ctx.FillPaint(Paint.SolidPaint(Color));
            ctx.Fill();

            // Draw tint?
            if (tint.HasValue && tint.Value.A > 0)
            {
                ctx.BeginPath();
                ctx.RoundedRectVarying(topLeft, size, Rounding);
                ctx.FillColor(tint.Value);
                ctx.Fill();
            }
        }
    }

    #endregion

    #region ImageBrush

    /// <summary>
    /// Image brush
    /// </summary>
    public class ImageBrush : BrushBase
    {
        public ImageBrush()
            : base(BrushType.Texture)
        { }

        public ImageBrush(int texture)
            : base(BrushType.Texture)
        {
            Texture = texture;
        }

        #region Properties

        /// <summary>
        /// Texture id
        /// </summary>
        public int Texture { get; set; }

        /// <summary>
        /// Texture width
        /// </summary>
        public uint TexWidth { get; set; } = 32;

        /// <summary>
        /// Texture height
        /// </summary>
        public uint TexHeight { get; set; } = 32;

        /// <summary>
        /// Offset
        /// </summary>
        public Vector2 Offset { get; set; }

        /// <summary>
        /// Scale. Default is 1.
        /// </summary>
        public Vector2 Scale { get; set; } = Vector2.One;

        /// <summary>
        /// Stretch. Default is false.
        /// </summary>
        public bool Stretch { get; set; } = false;

        #endregion

        /// <summary>
        /// Draw.
        /// </summary>
        public override void Draw(NvgContext ctx, Vector2 topLeft, Vector2 size, Color? tint)
        {
            if (Texture <= Globals.INVALID)
                return;

            ctx.GetTextureSize(Texture, out Vector2 textureSize);

            Paint bgImage = Paint.ImagePattern(
                topLeft.X,
                topLeft.Y,
                textureSize.X,
                textureSize.Y,
                0, Texture, Color.White);

            ctx.BeginPath();
            ctx.RoundedRectVarying(topLeft, size, Rounding);
            ctx.FillPaint(bgImage);
            ctx.Fill();

            // Draw tint?
            if (tint.HasValue && tint.Value.A > 0)
            {
                ctx.BeginPath();
                ctx.RoundedRectVarying(topLeft, size, Rounding);
                ctx.FillColor(tint.Value);
                ctx.Fill();
            }
        }
    }

    #endregion

    #region SvgBrush

    /// <summary>
    /// Svg brush.
    /// </summary>
    public class SvgBrush : BrushBase
    {
        /// <summary>
        /// Svg id.
        /// </summary>
        public int SvgId { get; set; }

        /// <summary>
        /// Should svg fit widgets area? Default is true.
        /// </summary>
        public bool FitSvg { get; set; } = true;

        public SvgBrush()
            : base(BrushType.Svg)
        { }

        public SvgBrush(int svgId)
            : base(BrushType.Svg)
        {
            SvgId = svgId;
        }

        // <summary>
        /// Draw
        /// </summary>
        public override void Draw(NvgContext ctx, Vector2 topLeft, Vector2 size, Color? tint)
        {
            if (SvgId >= 0)
            {
                ctx.SaveState();

                // move to topLeft
                ctx.Translate(topLeft);

                if (FitSvg && ctx.TryGetSvgSize(SvgId, out Vector2 svgSize))
                {
                    // we must set scale so svg fits in widget
                    ctx.Scale(size / svgSize);
                }

                ctx.DrawSvg(SvgId);

                ctx.RestoreState();
            }

            // Draw tint?
            if (tint.HasValue && tint.Value.A > 0)
            {
                ctx.BeginPath();
                ctx.RoundedRectVarying(topLeft, size, Rounding);
                ctx.FillColor(tint.Value);
                ctx.Fill();
            }
        }
    }

    #endregion
}
