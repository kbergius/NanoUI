using NanoUI.Common;
using NanoUI.Components;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUIDemos.Experimental.Components
{
    // todo : this is not tested
    // todo: should there really be sepaerator, since its easy to draw line
    // - could be in WidgetEXT
    public class UISeparator : UIWidget
    {
        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        public UISeparator()
        {
            // set defaults to theme impl - prevents circular reference
            SeparatorColor = default;
            DisablePointerFocus = true;
            Border = false;
        }

        // todo: check
        public UISeparator(UIWidget parent)
            :this(parent, 1)
        {
        
        }

        public UISeparator(UIWidget parent, int lineWidth)
            : base(parent)
        {
            DrawFlags = SeparatorDrawFlag.Horizontal | 
                SeparatorDrawFlag.CenterV | SeparatorDrawFlag.CenterH;
            LineWidth = lineWidth;

            DisablePointerFocus = true;
            Border = false;

            ThemeType = typeof(UISeparator);
        }

        #region Properties

        public SeparatorDrawFlag DrawFlags { get; set; }
        
        public int LineWidth { get; set; }
        
        Color? _separatorColor;
        public Color SeparatorColor
        {
            // todo:
            get=> _separatorColor.HasValue? _separatorColor.Value : ((ThemeEXT)GetTheme()).Separator.SeparatorColor;
            set => _separatorColor = value;
        }

        #endregion

        #region Layout

        public override Vector2 PreferredSize(NvgContext ctx)
        {
            // todo : padding?
            if ((DrawFlags & SeparatorDrawFlag.Horizontal) != 0)
                return new Vector2(0, LineWidth);
            else
                return new Vector2(LineWidth, 0);
        }

        public override void PerformLayout(NvgContext ctx)
        {
            Vector2 psize = Parent != null ? Parent.Size : new Vector2(1, 1);
            Vector2 pref = PreferredSize(ctx);

            // todo : padding
            if ((DrawFlags & SeparatorDrawFlag.Horizontal) != 0)
                pref.X = psize.X;
            else
                pref.Y = psize.Y;

            if ((DrawFlags & SeparatorDrawFlag.Left) != 0)
                Position = new Vector2(0, Position.Y);
            else if ((DrawFlags & SeparatorDrawFlag.Rigth) != 0)
                Position = new Vector2((psize - pref).X, Position.Y);
            else if ((DrawFlags & SeparatorDrawFlag.CenterH) != 0)
                Position = new Vector2((psize - pref).X / 2, Position.Y);
            if ((DrawFlags & SeparatorDrawFlag.Top) != 0)
                Position = new Vector2(Position.X, 0);
            else if ((DrawFlags & SeparatorDrawFlag.Bottom) != 0)
                Position = new Vector2(Position.X, (psize - pref).Y);
            else if ((DrawFlags & SeparatorDrawFlag.CenterV) != 0)
                Position = new Vector2(Position.X, (psize - pref).Y / 2);

            base.PerformLayout(ctx);
        }

        #endregion

        #region Drawing

        // note: no backround, border draw
        public override void Draw(NvgContext ctx)
        {
            ctx.BeginPath();
            ctx.StrokeWidth(LineWidth);
            ctx.StrokeColor(SeparatorColor);

            if ((DrawFlags & SeparatorDrawFlag.Vertical) != 0)
            {
                var center = Position + new Vector2(Size.X, 0) / 2;

                ctx.MoveTo(center);
                ctx.LineTo(center + new Vector2(0, Size.Y));
            }
            else
            {
                var center = Position + new Vector2(0, Size.Y) / 2;

                ctx.MoveTo(center);
                ctx.LineTo(center + new Vector2(Size.X, 0));
            }

            ctx.Stroke();

            base.Draw(ctx);
        }

        #endregion
    }
}