using NanoUI.Common;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUIDemos.Drawing
{
    // note: there are currently no extra SDF params passed to the fragment/pixel shader to make
    // text effects, but they can be added to NvgContext class.
    // note2: Scrolling text is just a basic demo without bells & whistles (like 3D transform)
    public class SDFTextDemo : DemoBase
    {
        public static int _fontNormal, _fontBold, _fontChinese;

        private readonly NvgContext _ctx;
        Vector2 _windowSize;

        bool _drawTextAtlas = false;

        // texts
        const string _lowerCaseText = "quick brown fox jumps over the lazy dog!";
        //const string _upperCaseText = "QUICK BROWN FOX JUMPS OVER THE LAZY DOG!";
        const string _chineseText = "敏捷的棕色狐狸跳过了懒狗！";

        const string _starWarsEpisodeIV = "It is a period of civil war.\r\nRebel spaceships, striking\r\nfrom a hidden base, have won\r\ntheir first victory against\r\nthe evil Galactic Empire.\r\n\r\nDuring the battle, Rebel\r\nspies managed to steal secret\r\nplans to the Empire's\r\nultimate weapon, the DEATH\r\nSTAR, an armored space\r\nstation with enough power to\r\ndestroy an entire planet.\r\n\r\nPursued by the Empire's\r\nsinister agents, Princess\r\nLeia races home aboard her\r\nstarship, custodian of the\r\nstolen plans that can save\r\nher people and restore\r\nfreedom to the galaxy....\r\n";

        const float _scollingSpeed = 0.7f;
        string[] _scrollingLines;

        public SDFTextDemo(NvgContext ctx, Vector2 windowSize)
            :base(null)
        {
            _ctx = ctx;
            _windowSize = windowSize;

            // start empty lines
            int startEmptyLines = 9;

            _scrollingLines = new string[startEmptyLines];

            for (int i = 0; i < _scrollingLines.Length; i++)
            {
                _scrollingLines[i] = "\n";
            }

            var splitted = _starWarsEpisodeIV.Split(['\r', '\n'],
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            Array.Resize(ref _scrollingLines, _scrollingLines.Length + splitted.Length);

            for (int i = 0; i < splitted.Length; i++)
            {
                _scrollingLines[i + startEmptyLines] = splitted[i];
            }

            // note: there is no theme - so we manually load fonts
            _fontNormal = _ctx.CreateFont("normal", DemoAssets.FontNormal, GlyphBaking.SDF);
            if (_fontNormal == -1)
            {
                Console.Error.WriteLine("Could not add font regular.");
                Environment.Exit(-1);
            }

            _fontBold = _ctx.CreateFont("bold", DemoAssets.FontBold, GlyphBaking.SDF);
            if (_fontBold == -1)
            {
                Console.Error.WriteLine("Could not add font bold.");
                Environment.Exit(-1);
            }

            _fontChinese = _ctx.CreateFont("chinese", "./Assets/fonts/NotoSansTC-Regular.ttf", GlyphBaking.SDF);
            if (_fontChinese == -1)
            {
                Console.Error.WriteLine("Could not add chinese font.");
                Environment.Exit(-1);
            }

            // init test textures
            //DemoAssets.InitTestTextures(_ctx);
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
            _windowSize = size;
        }

        public override void Draw(NvgContext ctx)
        {
            DrawText();

            // this shows how glyphs are drawn in atlas texture
            if (_drawTextAtlas)
            {
                DrawFontAtlas();
            }
        }

        #region DrawText

        // dynamic params
        Vector2 _textPosition;
        float _fontSize;

        int _scrollingStartLine = 0;
        float _scrollingOffsetY;

        void DrawText()
        {
            _textPosition = new Vector2(40, 30);
            _fontSize = 16;

            _ctx.FontFaceId(_fontNormal);
            _ctx.TextAlign(TextAlignment.Left | TextAlignment.Top);
            _ctx.TextColor(Color.White);

            for (int i = 0; i < 15; i++)
            {
                _ctx.FontSize(_fontSize);

                _ctx.Text(_textPosition, _lowerCaseText);

                _textPosition += new Vector2(0, _fontSize * 1.2f);

                _fontSize += 2;
            }

            // chinese
            _ctx.FontFaceId(_fontChinese);
            _ctx.TextCharSpacing(5);
            _ctx.Text(_textPosition, _chineseText);
            _ctx.TextCharSpacing(0);

            // Scrolling text
            _fontSize = 60;

            // tick?
            if (t > _scollingSpeed)
            {
                t = 0;

                _scrollingStartLine++;

                if(_scrollingStartLine >= _scrollingLines.Length)
                {
                    _scrollingStartLine = 0;
                }
            }
            
            // get offset
            _scrollingOffsetY = t * _fontSize / _scollingSpeed;

            _ctx.FontFaceId(_fontBold);
            _ctx.FontSize(_fontSize);
            _ctx.TextAlign(TextAlignment.Center | TextAlignment.Middle);
            _ctx.TextColor(Color.Yellow);

            _textPosition = new Vector2(_windowSize.X / 2, 0);

            for (int i = _scrollingStartLine; i < _scrollingLines.Length; i++)
            {
                _textPosition.Y -= _scrollingOffsetY;

                _ctx.Text(_textPosition, _scrollingLines[i]);

                _textPosition.Y += _fontSize + _scrollingOffsetY;
            }

            for (int i = 0; i < _scrollingStartLine; i++)
            {
                _textPosition.Y -= _scrollingOffsetY;

                _ctx.Text(_textPosition, _scrollingLines[i]);

                _textPosition.Y += _fontSize + _scrollingOffsetY;
            }
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