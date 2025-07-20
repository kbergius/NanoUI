using NanoUI.Nvg;
using System.Numerics;

namespace NanoUIDemos.Drawing
{
    public class SvgDemo : DemoBase
    {
        // note: these are needed for performance stats
        public static int _fontNormal, _fontBold;

        private readonly NvgContext _ctx;
        Vector2 _windoSize;

        static int _svgShape;
        static int _svgTextShape;

        public SvgDemo(NvgContext ctx, Vector2 windowSize)
            :base(null)
        {
            _ctx = ctx;
            _windoSize = windowSize;

            // note: there is no theme - so we manually load fonts
            _fontNormal = _ctx.CreateFont("normal", DemoAssets.FontNormal);
            if (_fontNormal == -1)
            {
                Console.Error.WriteLine("Could not add font regular.");
                Environment.Exit(-1);
            }
            _fontBold = _ctx.CreateFont("bold", DemoAssets.FontBold);
            if (_fontBold == -1)
            {
                Console.Error.WriteLine("Could not add font bold.");
                Environment.Exit(-1);
            }

            // init test textures
            //DemoAssets.InitTestTextures(_ctx);

            // Create svg
            string path = "./Assets/svg/tiger.svg";
            //path = "./Assets/svg/testBasic.svg";

            _svgShape = ctx.CreateSvg(path);

            _svgTextShape = ctx.CreateSvg("./Assets/svg/text.svg");
        }

        #region Inputs

        public override bool OnPointerMove(Vector2 pointerPos, Vector2 rel)
        {
            return false;
        }

        #endregion

        float _deltaSeconds;
        float t;
        public override void Update(float deltaSeconds)
        {
            _deltaSeconds = deltaSeconds;
            t += deltaSeconds;
        }

        public override void ScreenResize(Vector2 size, NvgContext ctx)
        {
            _windoSize = size;
        }

        float scale = 0.5f;
        float sign = 1;
        public override void Draw(NvgContext ctx)
        {
            if(_svgShape >= 0)
            {
                if (scale > 2.5f)
                {
                    sign = -1;
                }
                else if (scale < 0.5f)
                {
                    sign = 1;
                }

                scale += sign * _deltaSeconds * 0.5f;

                ctx.SaveState();

                ctx.Scale(scale);
                //ctx.Skew(float.DegreesToRadians(30), 0);
                //ctx.Translate(new Vector2(_windoSize.X * 0.3f, 100));
                //ctx.Rotate(t, new Vector2(_windoSize.X * 0.3f));

                _ctx.DrawSvg(_svgShape);
                _ctx.DrawSvg(_svgTextShape);

                ctx.RestoreState();
            }
        }
    }
}