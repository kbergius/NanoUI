using NanoUI;
using NanoUI.Common;
using NanoUIDemos.Drawing;
using NanoUI.Nvg;
using NanoUIDemos.UI;
using System.Numerics;

namespace NanoUIDemos
{
    public enum DemoType
    {
        Docking,
        Drawing,
        SDFText,
        SvgShapes,
        TextShapes,
        UIBasic,
        UIExtended,
        UIExtended2,
        UIExperimental,
        UILayouts
    }

    public static class DemoFactory
    {
        public static DemoBase CreateDemo(NvgContext ctx, DemoType demoType, Vector2 windoSize)
        {
            if(demoType == DemoType.Drawing)
            {
                // no screen
                return new DrawingDemo(ctx, windoSize);
            }
            else if (demoType == DemoType.TextShapes)
            {
                // no screen
                return new TextShapesDemo(ctx, windoSize);
            }
            else if (demoType == DemoType.SDFText)
            {
                // no screen
                return new SDFTextDemo(ctx, windoSize);
            }
            else if (demoType == DemoType.SvgShapes)
            {
                // no screen
                return new SvgDemo(ctx, windoSize);
            }

            // get default theme
            var theme = DemoAssets.GetTheme(ctx);

            // create screen
            var screen = new UIScreen(theme, windoSize);
            screen.BackgroundFocused = screen.BackgroundUnfocused = new SolidBrush(new Color(0.3f, 0.3f, 0.32f, 1.0f));

            switch (demoType)
            {
                case DemoType.Docking:
                    return new UIDockingDemo(screen);
                case DemoType.UIExperimental:
                    return new UIExperimentalDemo(screen);
                case DemoType.UIExtended:
                    return new UIExtendedDemo(screen, ctx);
                case DemoType.UIExtended2:
                    return new UIExtended2Demo(screen);
                case DemoType.UILayouts:
                    return new UILayoutDemo(screen);
                default:
                    return new UIBasicDemo(screen);
            }
        }
    }
}