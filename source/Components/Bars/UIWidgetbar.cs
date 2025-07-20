using NanoUI.Common;
using NanoUI.Layouts;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUI.Components.Bars
{
    // abstract bar component that provides some common functionalty. Could be used for examle in Window
    // if there are many bars (menubar, toolbar, etc) to calculate children layout offset

    // note: parent should do the psoitioning, since it knows what bars there exist
    // todo: statusbar (needs positioning info: top / bottom)
    // yodo2: if also Scrollbar extends this, there should be also "right" (needs to refactor scrollbar logic!)
    public abstract class UIWidgetbar : UIWidget
    {
        public UIWidgetbar()
        {
        
        }

        public UIWidgetbar(UIWidget parent)
            :base(parent)
        {
            // todo: configurable?
            ChildrenLayout = new StackLayout(Orientation.Horizontal, LayoutAlignment.Middle);
            ChildrenLayout.Spacing = new Vector2 (5, 0);

            ThemeType = GetType();
        }

        #region Layout

        public override void PerformLayout(NvgContext ctx)
        {
            base.PerformLayout(ctx);

            // stretch
            Size = new Vector2(Parent.Size.X, Size.Y);
        }

        #endregion

        #region Events

        public override void OnScreenResize(Vector2 size, NvgContext ctx)
        {
            // note: this should be unnecessary, since other parents should not call this function
            if(Parent is not UIScreen screen)
                return;

            Size = new Vector2(size.X, Size.Y);

            PerformLayout(ctx);
        }

        #endregion
    }
}