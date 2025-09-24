using NanoUI.Common;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUI.Components.Simple
{
    /// <summary>
    /// UISeparator.
    /// </summary>
    public class UISeparator : UIWidget
    {
        public UISeparator(int height)
        {
            Height = height;
        }

        /// <inheritdoc />
        public UISeparator(UIWidget parent)
            : base(parent)
        {

        }

        #region Properties

        // todo: also orientation vertical & Height => Dimension?
        float? _height;

        /// <summary>
        /// Height
        /// </summary>
        public override float Height
        {
            get => _height?? 1f;
            set => _height = value;
        }

        // todo: configurable?

        Color? _color;

        /// <summary>
        /// Color. If not set makes this dimmer(currently TextColor * 0.7f).
        /// </summary>
        public Color Color
        {
            get => _color?? GetTheme().Widget.TextColor * 0.7f;
            set => _color = value;
        }

        /// <summary>
        /// Padding is set in view panel - when drawing.
        /// </summary>
        internal int Padding { get; set; }

        #endregion

        #region Drawing

        /// <inheritdoc />
        public override void Draw(NvgContext ctx)
        {
            ctx.BeginPath();
            ctx.StrokeWidth(Height);
           
            ctx.StrokeColor(Color);

            // todo : use column padding or this Padding?
            var center = Position + new Vector2(Padding, Size.Y / 2);

            ctx.MoveTo(center);
            ctx.LineTo(center + new Vector2(Size.X - 2 * Padding, 0));

            ctx.Stroke();
        }

        #endregion
    }
}
