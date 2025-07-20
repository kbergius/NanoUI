using NanoUI.Common;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUIDemos.Drawing
{
    public class TextShapesDemo : DemoBase
    {
        public static int _fontNormal, _fontBold, _fontIcons, _fontShapes;

        private readonly NvgContext _ctx;
        Vector2 _windoSize;

        bool _drawTextAtlas = false;

        public TextShapesDemo(NvgContext ctx, Vector2 windowSize)
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

            _fontIcons = _ctx.CreateFont("icons", DemoAssets.FontIcons);
            if (_fontIcons == -1)
            {
                Console.Error.WriteLine("Could not add font icons.");
                Environment.Exit(-1);
            }

            _fontShapes = _ctx.CreateFont("shapes", DemoAssets.FontBold, GlyphBaking.Shapes);
            if (_fontShapes == -1)
            {
                Console.Error.WriteLine("Could not add font shapes.");
                Environment.Exit(-1);
            }

            // init test textures
            DemoAssets.InitTestTextures(_ctx);
        }

        #region Inputs

        public override bool OnPointerMove(Vector2 pointerPos, Vector2 rel)
        {
            mx = pointerPos.X;
            my = pointerPos.Y;

            return true;
        }

        #endregion

        float mx;
        float my;
        float t;
        public override void Update(float deltaSeconds)
        {
            // note: using cumulative value
            t += deltaSeconds;
        }

        public override void ScreenResize(Vector2 size, NvgContext ctx)
        {
            _windoSize = size;
        }

        public override void Draw(NvgContext ctx)
        {
            DrawTextShapes();

            // this shows how glyphs are drawn in atlas texture
            if (_drawTextAtlas)
            {
                DrawFontAtlas();
            }
        }

        #region DrawTextShapes

        // Some text shape effects & scaling with vector graphics fonts
        const string _lowerCaseText = "quick brown fox jumps over the lazy dog! ?%&åäöüñàéçø";
        const string _upperCaseText = "QUICK BROWN FOX JUMPS OVER THE LAZY DOG!";

        const float _outlineWidth = 3;

        void DrawTextShapes()
        {
            float fontSize = 40;
            
            _ctx.SaveState();

            _ctx.FontSize(fontSize);
            _ctx.FontFaceId(_fontShapes);
            _ctx.TextAlign(TextAlignment.Left | TextAlignment.Top);

            // Outline
            Vector2 pos = new Vector2(45, 40);
            
            _ctx.TextShapeOutline(Color.White, _outlineWidth - 1);
            _ctx.TextShapeFill(null);
            _ctx.Text(pos, _lowerCaseText);

            // Fill
            pos += new Vector2(0, fontSize * 1.5f);
            
            _ctx.TextShapeOutline(null);
            _ctx.TextShapeFill(Paint.LinearGradient(pos, new Vector2(fontSize), Color.Red, Color.Yellow, false));
            _ctx.Text(pos, _lowerCaseText);

            // Outline & Fill
            pos += new Vector2(0, fontSize * 1.5f);

            // increse font size
            fontSize *= 2;

            _ctx.FontSize(fontSize);

            _ctx.TextShapeOutline(Color.White, _outlineWidth);
            _ctx.TextShapeFill(Paint.LinearGradient(pos, new Vector2(fontSize * 0.8f), Color.Red, Color.Yellow, false));
            _ctx.Text(pos, _lowerCaseText);

            _ctx.SaveState();

            // Upper case outline & fill
            pos += new Vector2(0, fontSize * 1.2f);

            _ctx.TextShapeOutline(Color.White, _outlineWidth);
            _ctx.TextShapeFill(Paint.LinearGradient(pos, new Vector2(fontSize * 0.8f), Color.Red, Color.Yellow, false));
            _ctx.Text(pos, _upperCaseText);

            _ctx.ResetState();

            _ctx.SaveState();

            // Scaling & Fill Image
            pos += new Vector2(0, fontSize * 1.1f);

            _ctx.FontSize(fontSize);
            _ctx.FontFaceId(_fontShapes);
            _ctx.TextAlign(TextAlignment.Left | TextAlignment.Top);

            _ctx.Scale(new Vector2(0.85f, 1.5f), pos);
            
            _ctx.TextShapeOutline(Color.White, _outlineWidth);
            _ctx.TextShapeFill(Paint.ImagePattern(pos, new Vector2(_windoSize.X, fontSize * 1.5f), 0, DemoAssets.TestTexture, Color.Transparent));
            _ctx.Text(pos, _upperCaseText);

            _ctx.RestoreState();

            _ctx.RestoreState();
        }

        #endregion

        #region DrawFontAtlas

        void DrawFontAtlas()
        {
            Vector2 pos = Vector2.Zero;

            _ctx.GetAtlasSize(out int width, out int height);
            Vector2 size = new Vector2(width, height);

            int texture = _ctx.GetAtlasTextureId();
            
            Paint imgPaint = Paint.ImagePattern(pos, size, 0.0f / 180.0f * MathF.PI, texture, Color.White);
            
            _ctx.BeginPath();
            _ctx.Rect(pos, size);
            _ctx.FillPaint(imgPaint);
            _ctx.Fill();
        }

        #endregion
    }
}