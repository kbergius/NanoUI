using Nano;
using NanoUIDemos;

namespace SDL3Example
{
    internal class Program
    {
        // DemoTypes:
        // Docking, Drawing, SDFText, SvgShapes, TextShapes, UIBasic, UIExtended, UIExtended2,
        // UIExperimental, UILayouts
        static void Main(string[] args)
        {
            var demoType = DemoType.UIBasic;

            var windowCreateInfo = new WindowCreateInfo(
                $"NanoUI - {demoType.ToString()}",
                1200,
                700,
                ScreenMode.Windowed,
                true // window resizable
            );

            var framePacingSettings = FramePacingSettings.CreateCapped(60, 120);

            var game = new NanoUIApp(
                new AppInfo("NanoUI", "NanoUIDemos"),
                windowCreateInfo,
                framePacingSettings,
                demoType
            );

            game.Run();
        }
    }
}