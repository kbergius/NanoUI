using NanoUI.Nvg;
using System.Numerics;

namespace NanoUI.Components
{
    public class UISvgWidget : UIWidget
    {
        public UISvgWidget()
        {
        
        }
        
        public UISvgWidget(UIWidget parent)
            : base(parent)
        {
        
        }

        #region Properties

        int? svgId;
        public int? SvgId
        {
            get => svgId;
            set => svgId = value;
        }

        // shall we scale svg so it fits into widgets area
        public bool FitSvg { get; set; } = true;

        #endregion

        #region Draw

        public override void Draw(NvgContext ctx)
        {
            // Background
            DrawBackgroundBrush(ctx);

            if (svgId != null)
            {
                ctx.SaveState();

                // move to widgets parent position
                ctx.Translate(Position);

                if(FitSvg && ctx.TryGetSvgSize(svgId.Value, out Vector2 svgSize))
                {
                    // we must set scale so svg fits in this widget
                    ctx.Scale(Size / svgSize);
                }

                ctx.DrawSvg(svgId.Value);

                ctx.RestoreState();
            }

            base.Draw(ctx);
        }

        #endregion
    }
}