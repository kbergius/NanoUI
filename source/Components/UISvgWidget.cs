using NanoUI.Nvg;
using System.Numerics;

namespace NanoUI.Components
{
    /// <summary>
    /// UISvgWidget.
    /// </summary>
    public class UISvgWidget : UIWidget
    {
        /// <inheritdoc />
        public UISvgWidget()
        {
        
        }

        /// <inheritdoc />
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

        /// <summary>
        /// Shall we scale svg so it fits into widgets area. Default: true;
        /// </summary>
        public bool FitSvg { get; set; } = true;

        #endregion

        #region Drawing

        /// <inheritdoc />
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
