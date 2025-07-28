using NanoUI;
using NanoUIDemos.Experimental;
using NanoUI.Nvg;
using NanoUI.Styles;

namespace NanoUIDemos
{
    // Note: images are in PNG format (maximum portability).
    internal static class DemoAssets
    {
        public const string FontNormal = "./Assets/fonts/Roboto-Regular.ttf";
        public const string FontBold = "./Assets/fonts/Roboto-Bold.ttf";
        public const string FontIcons = "./Assets/fonts/fa-solid-900.ttf";

        static int[] _textures = new int[12];

        public static int[] Textures => _textures;

        static int _testTexture;

        public static int TestTexture => _testTexture;

        public static void InitTestTextures(NvgContext ctx)
        {
            for (uint i = 0; i < 12; i++)
            {
                string file = "./Assets/images/image" + i + ".png";
                _textures[i] = ctx.CreateTexture(file);

                if (_textures[i] < 0)
                {
                    Console.Error.WriteLine("Could not load " + file);
                    Environment.Exit(-1);
                }
            }

            // test image
            string file2 = "./Assets/landscape.png";
            _testTexture = ctx.CreateTexture(file2);
        }

        public static UITheme GetTheme(NvgContext ctx)
        {
            // init fonts
            FontsStyle fonts = new FontsStyle()
            {
                DefaultFontType = "Normal",
                DefaultIconsType = "Icons",
            };

            fonts.FontTypes.Add("Normal", FontNormal);
            fonts.FontTypes.Add("Bold", FontBold);
            fonts.FontTypes.Add("Icons", FontIcons);

            // create default theme
            ThemeEXT theme = UITheme.CreateDefault<ThemeEXT>(ctx, fonts);
            theme.Save("DefaultTheme.json");

            // create
            ThemeEXT? loadTheme = UITheme.Load<ThemeEXT>(ctx, "DefaultTheme.json");

            if(loadTheme != null)
            {
                loadTheme.PopulateExt();

                // this is just for testing
                InitTestTextures(ctx);

                return loadTheme;
            }

            // this is just for testing
            InitTestTextures(ctx);

            return theme;
        }
    }
}
