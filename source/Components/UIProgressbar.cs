using NanoUI.Common;
using NanoUI.Nvg;
using System;
using System.ComponentModel;

namespace NanoUI.Components
{
    // todo : Orientation
    // todo : draw progress text value into the center of widget
    public class UIProgressbar : UIWidget
    {
        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        public UIProgressbar()
        {
            // set defaults to theme impl - prevents circular reference
            ValueColor = default;
        }

        public UIProgressbar(UIWidget parent)
           : base(parent)
        {
            ThemeType = typeof(UIProgressbar);
        }

        #region Properties

        [Browsable(false)]
        public virtual float Value { get; set; }
       
        // note: we use accent color as default
        Color? _valueColor;
        public Color ValueColor
        {
            get => _valueColor?? GetTheme().Progressbar.ValueColor;
            set => _valueColor = value;
        }

        #endregion

        #region Drawing

        public override void Draw(NvgContext ctx)
        {
            // background
            DrawBackgroundBrush(ctx);

            // todo : there should not be children but call base.Draw anyway
            base.Draw(ctx);

            // Progress
            float value = Math.Clamp(Value, 0.0f, 1.0f);
            int barPos = (int)Math.Round((Size.X - 2) * value);

            // todo : magical numbers
            var cornerRadius = CornerRadius.TopLeft;

            Paint paint = Paint.BoxGradient(
               Position.X, Position.Y,
               barPos + 1.5f, Size.Y - 1, cornerRadius, 4, // corner radius fixed was 3
               ValueColor, ValueColor);

            ctx.BeginPath();
            ctx.RoundedRectVarying(
                Position.X + 1, Position.Y + 1,
                barPos, Size.Y - 2, CornerRadius);

            ctx.FillPaint(paint);
            ctx.Fill();

            // border
            this.DrawBorder(ctx, true);
        }

        #endregion
    }
}