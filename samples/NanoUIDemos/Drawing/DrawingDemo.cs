using NanoUI.Common;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUIDemos.Drawing
{
    // This is ported demo from NanoVG https://github.com/memononen/nanovg.
    // It shows drawing layer capabilities (quite a lot of code).
    public class DrawingDemo : DemoBase
    {
        private const int ICON_SEARCH = (int)FontAwesomeIcon.MagnifyingGlass;
        private const int ICON_CIRCLED_CROSS = (int)FontAwesomeIcon.CircleXmark;
        private const int ICON_CHEVRON_RIGHT = (int)FontAwesomeIcon.ChevronRight;
        private const int ICON_CHECK = (int)FontAwesomeIcon.Check;
        private const int ICON_LOGIN = (int)FontAwesomeIcon.CaretRight;
        private const int ICON_TRASH = (int)FontAwesomeIcon.Trash;

        private const int ICON_SIZE = 16;

        // these use baking SDF
        public static int _fontNormal, _fontBold, _fontIcons;
        
        // this uses baking Normal
        static int _fontEffects;

        private readonly NvgContext _ctx;
        Vector2 _windoSize;
        Vector2 _windowCenter => _windoSize / 2;

        bool _drawTextAtlas = false;

        public DrawingDemo(NvgContext ctx, Vector2 windowSize)
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

            _fontEffects = _ctx.CreateFont("effects", DemoAssets.FontBold, GlyphBaking.Normal);

            if (_fontEffects == -1)
            {
                Console.Error.WriteLine("Could not add effects font.");
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
            float width = _windoSize.X;
            float height = _windoSize.Y;

            DrawEyes(width - 250.0f, 50.0f, 150.0f, 100.0f, mx, my, t);
            DrawParagraph(width - 450.0f, 50.0f, 150.0f, mx, my);
            DrawGraph(0.0f, height / 2.0f, width, height / 2.0f, t);
            DrawColourwheel(width - 300.0f, height - 300.0f, 250.0f, 250.0f, t);
            DrawLines(120.0f, height - 50.0f, 600.0f, 50.0f, t);

            DrawWidths(10, 50, 30);
            DrawCaps(10, 300, 30);
            DrawScissor(50.0f, height - 80.0f, t);

            _ctx.SaveState();

            DrawWindow("Widgets 'n' Stuff", 50.0f, 50.0f, 300.0f, 400.0f);
            float x = 60.0f;
            float y = 95.0f;
            DrawSearchBox("Search...", x, y, 280.0f, 25.0f);
            y += 40.0f;
            DrawDropDown("Effects:", x, y, 280.0f, 28.0f);
            float popY = y + 14.0f;
            y += 45.0f;

            DrawLabel("Login", x, y, 20.0f);
            y += 25.0f;
            DrawEditBox("Email", x, y, 280.0f, 28.0f);
            y += 35.0f;
            DrawEditBox("Password", x, y, 280.0f, 28.0f);
            y += 38.0f;
            DrawCheckBox("Remember me", x, y, 140.0f, 28.0f);
            DrawButton(ICON_LOGIN, "Sign in", x + 138.0f, y, 140.0f, 28.0f, new Color(0, 96, 128, 255));
            y += 45.0f;

            DrawLabel("Diameter", x, y, 20.0f);
            y += 25.0f;
            DrawEditBoxNum("123.00", "px", x + 180.0f, y, 100.0f, 28.0f);
            DrawSlider(0.4f, x, y, 170.0f, 28.0f);
            y += 55.0f;

            DrawButton(ICON_TRASH, "Delete", x, y, 160.0f, 28.0f, new Color(128, 16, 8, 255));
            DrawButton(0, "Cancel", x + 170.0f, y, 110.0f, 28.0f, new Color(0, 0, 0, 0));

            DrawThumbnails(365.0f, popY - 30.0f, 160.0f, 300.0f, DemoAssets.Textures, t);

            _ctx.RestoreState();

            DrawStars(new Vector2(width - 220.0f, 230.0f), 25, 3);

            DrawTextEffects();

            // this shows how glyphs are drawn in atlas texture
            if (_drawTextAtlas)
            {
                DrawFontAtlas();
            }
        }

        #region DrawTextEffects

        // Some text effects & scaling & skewing with Normal glyph baking
        void DrawTextEffects()
        {
            _ctx.SaveState();

            _ctx.FontSize(80);
            _ctx.FontFaceId(_fontEffects);
            _ctx.TextAlign(TextAlignment.Center | TextAlignment.Top);

            // Outline / Dilate
            Vector2 pos = _windowCenter - new Vector2(0, 50);
            
            _ctx.TextNormalDilate(3);
            _ctx.TextColor(Color.Red);
            _ctx.TextCharSpacing(2.5f);
            _ctx.Text(_windowCenter - new Vector2(0, 50), "Outline text");
            _ctx.TextNormalDilate(0);
            _ctx.TextColor(Color.Yellow);
            _ctx.Text(_windowCenter - new Vector2(0, 50), "Outline text");

            // blur
            // should be between 0 - 20
            // note: value is clamped in NvgContext.Text
            pos = _windowCenter + new Vector2(0, 30);
            
            _ctx.TextNormalBlur(5);
            _ctx.TextColor(Color.White);
            _ctx.Text(pos, "Blurred text");
            _ctx.TextNormalBlur(0);

            // Scale & Skew
            pos = _windowCenter + new Vector2(100, 70);

            _ctx.Scale(new Vector2(0.8f, 1.6f), _windowCenter);
            _ctx.Skew(-0.8f, 0, _windowCenter);
            
            _ctx.TextColor(Color.Red);
            _ctx.Text(pos, "Scaled & Skewed");

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

        #region DrawWindow

        const float _windowCornerRadius = 3.0f;
        private void DrawWindow(string title, float x, float y, float w, float h)
        {
            _ctx.SaveState();

            _ctx.BeginPath();
            _ctx.RoundedRect(x, y, w, h, _windowCornerRadius);
            _ctx.FillColor(new Color(28, 30, 34, 192));
            _ctx.Fill();

            Paint shadowPaint = Paint.BoxGradient(x, y + 2.0f, w, h, _windowCornerRadius * 2.0f, 10.0f, new Color(0, 0, 0, 128), new Color(0, 0, 0, 0));
            
            _ctx.BeginPath();
            _ctx.Rect(x - 10.0f, y - 10.0f, w + 20.0f, h + 30.0f);
            _ctx.RoundedRect(x, y, w, h, _windowCornerRadius);
            _ctx.PathWinding(Solidity.Hole);
            _ctx.FillPaint(shadowPaint);
            _ctx.Fill();

            Paint headerPaint = Paint.LinearGradient(x, y, x, y + 15.0f, new Color(255, 255, 255, 8), new Color(0, 0, 0, 16));
            
            _ctx.BeginPath();
            _ctx.RoundedRect(x + 1.0f, y + 1.0f, w - 2.0f, 30.0f, _windowCornerRadius - 1.0f);
            _ctx.FillPaint(headerPaint);
            _ctx.Fill();

            _ctx.BeginPath();
            _ctx.MoveTo(x + 0.5f, y + 0.5f + 30.0f);
            _ctx.LineTo(x + 0.5f + w - 1.0f, y + 0.5f + 30.0f);
            _ctx.StrokeColor(new Color(0, 0, 0, 30));
            _ctx.Stroke();

            _ctx.FontSize(15.0f);
            _ctx.FontFaceId(_fontBold);
            _ctx.TextAlign(TextAlignment.Center | TextAlignment.Middle);

            _ctx.FillColor(new Color(0, 0, 0, 128));
            _ctx.Text(x + w / 2.0f, y + 16.0f + 1.0f, title);

            _ctx.FillColor(new Color(220, 220, 220, 160));
            _ctx.Text(x + w / 2.0f, y + 16.0f, title);

            _ctx.RestoreState();
        }

        #endregion

        #region DrawSearchBox

        float _searhBoxCornerRadius;
        private void DrawSearchBox(string text, float x, float y, float w, float h)
        {
            _searhBoxCornerRadius = h / 2.0f - 1.0f;

            Paint bg = Paint.BoxGradient(x, y + 1.5f, w, h, h / 2.0f, 5.0f, new Color(0, 0, 0, 16), new Color(0, 0, 0, 92));
            
            _ctx.BeginPath();
            _ctx.RoundedRect(x, y, w, h, _searhBoxCornerRadius);
            _ctx.FillPaint(bg);
            _ctx.Fill();

            _ctx.BeginPath();
            _ctx.RoundedRect(x + 0.5f, y + 0.5f, w - 1.0f, h - 1.0f, _searhBoxCornerRadius - 0.5f);
            _ctx.StrokeColor(new Color(0, 0, 0, 48));
            _ctx.Stroke();

            _ctx.BeginPath();
            _ctx.FontSize(h * 1.3f);
            _ctx.FontFaceId(_fontIcons);
            _ctx.FillColor(new Color(255, 255, 255, 64));
            _ctx.TextAlign(TextAlignment.Center | TextAlignment.Middle);
            _ctx.FontSize(ICON_SIZE);
            _ctx.Text(x + h * 0.55f, y + h * 0.55f, ICON_SEARCH);

            _ctx.FontSize(17.0f);
            _ctx.FontFaceId(_fontNormal);
            _ctx.FillColor(new Color(255, 255, 255, 32));

            _ctx.TextAlign(TextAlignment.Left | TextAlignment.Middle);
            _ctx.Text(x + h * 1.05f, y + h * 0.5f, text);

            _ctx.FontSize(h * 1.3f);
            _ctx.FontFaceId(_fontIcons);
            _ctx.FillColor(new Color(255, 255, 255, 32));
            _ctx.TextAlign(TextAlignment.Center | TextAlignment.Middle);
            _ctx.FontSize(ICON_SIZE);
            _ctx.Text(x + w - h * 0.55f, y + h * 0.55f, ICON_CIRCLED_CROSS);
        }

        #endregion

        #region DrawDropDown

        const float _dropDownCornerRadius = 4.0f;

        private void DrawDropDown(string text, float x, float y, float w, float h)
        {
            Paint bg = Paint.LinearGradient(x, y, x, y + h, new Color(255, 255, 255, 16), new Color(0, 0, 0, 16));
            
            _ctx.BeginPath();
            _ctx.RoundedRect(x + 1.0f, y + 1.0f, w - 2.0f, h - 2.0f, _dropDownCornerRadius - 1.0f);
            _ctx.FillPaint(bg);
            _ctx.Fill();

            _ctx.BeginPath();
            _ctx.RoundedRect(x + 0.5f, y + 0.5f, w - 1.0f, h - 1.0f, _dropDownCornerRadius - 0.5f);
            _ctx.StrokeColor(new Color(0, 0, 0, 48));
            _ctx.Stroke();

            _ctx.FontSize(17.0f);
            _ctx.FontFaceId(_fontNormal);
            _ctx.FillColor(new Color(255, 255, 255, 160));
            _ctx.TextAlign(TextAlignment.Left | TextAlignment.Middle);
            _ctx.Text(x + h * 0.3f, y + h * 0.5f, text);

            _ctx.FontSize(h * 1.3f);
            _ctx.FontFaceId(_fontIcons);
            _ctx.FillColor(new Color(255, 255, 255, 64));
            _ctx.TextAlign(TextAlignment.Center | TextAlignment.Middle);
            _ctx.FontSize(ICON_SIZE);
            _ctx.Text(x + w - h * 0.5f, y + h * 0.5f, ICON_CHEVRON_RIGHT);
        }

        #endregion

        #region DrawLabel

        private void DrawLabel(string text, float x, float y, float h)
        {
            _ctx.FontSize(15.0f);
            _ctx.FontFaceId(_fontNormal);
            _ctx.FillColor(new Color(255, 255, 255, 128));

            _ctx.TextAlign(TextAlignment.Left | TextAlignment.Middle);
            _ctx.Text(x, y + h * 0.5f, text);
        }

        #endregion

        #region DrawEditBoxBase

        private void DrawEditBoxBase(float x, float y, float w, float h)
        {
            Paint bg = Paint.BoxGradient(x + 1.0f, y + 1.0f + 1.5f, w - 2.0f, h - 2.0f, 3.0f, 4.0f, new Color(255, 255, 255, 32), new Color(32, 32, 32, 32));
            
            _ctx.BeginPath();
            _ctx.RoundedRect(x + 1.0f, y + 1.0f, w - 2.0f, h - 2.0f, 4.0f - 1.0f);
            _ctx.FillPaint(bg);
            _ctx.Fill();

            _ctx.BeginPath();
            _ctx.RoundedRect(x + 0.5f, y + 0.5f, w - 1.0f, h - 1.0f, 4.0f - 1.0f);
            _ctx.StrokeColor(new Color(0, 0, 0, 48));
            _ctx.Stroke();
        }

        #endregion

        #region DrawEditBox

        private void DrawEditBox(string text, float x, float y, float w, float h)
        {
            DrawEditBoxBase(x, y, w, h);

            _ctx.FontSize(17.0f);
            _ctx.FontFaceId(_fontNormal);
            _ctx.FillColor(new Color(255, 255, 255, 64));
            _ctx.TextAlign(TextAlignment.Left | TextAlignment.Middle);
            _ctx.Text(x + h * 0.3f, y + h * 0.5f, text);
        }

        #endregion

        #region DrawEditBoxNum

        private void DrawEditBoxNum(string text, string units, float x, float y, float w, float h)
        {
            DrawEditBoxBase(x, y, w, h);

            float uw = _ctx.TextBounds(0.0f, 0.0f, units, out _);

            _ctx.FontSize(15.0f);
            _ctx.FontFaceId(_fontNormal);
            _ctx.FillColor(new Color(255, 255, 255, 64));
            _ctx.TextAlign(TextAlignment.Right | TextAlignment.Middle);
            _ctx.Text(x + w - h * 0.3f, y + h * 0.5f, units);

            _ctx.FontSize(17.0f);
            _ctx.FontFaceId(_fontNormal);
            _ctx.FillColor(new Color(255, 255, 255, 128));
            _ctx.TextAlign(TextAlignment.Right | TextAlignment.Middle);
            _ctx.Text(x + w - uw - h * 0.5f, y + h * 0.5f, text);
        }

        #endregion

        #region DrawCheckBox

        private void DrawCheckBox(string text, float x, float y, float _, float h)
        {
            _ctx.FontSize(15.0f);
            _ctx.FontFaceId(_fontNormal);
            _ctx.FillColor(new Color(255, 255, 255, 160));

            _ctx.TextAlign(TextAlignment.Left | TextAlignment.Middle);
            _ctx.Text(x + 28.0f, y + h * 0.5f, text);

            Paint bg = Paint.BoxGradient(x + 1.0f, y + (int)(h * 0.5f) - 9 + 1, 18.0f, 18.0f, 3.0f, 3.0f, new Color(0, 0, 0, 32), new Color(0, 0, 0, 92));
            
            _ctx.BeginPath();
            _ctx.RoundedRect(x + 1.0f, y + (int)(h * 0.5f) - 9, 18.0f, 18.0f, 3.0f);
            _ctx.FillPaint(bg);
            _ctx.Fill();

            _ctx.FontSize(33.0f);
            _ctx.FontFaceId(_fontIcons);
            _ctx.FillColor(new Color(255, 255, 255, 128));
            _ctx.TextAlign(TextAlignment.Center | TextAlignment.Middle);
            _ctx.FontSize(ICON_SIZE);
            _ctx.Text(x + 9.0f + 2.0f, y + h * 0.5f, ICON_CHECK);
        }

        #endregion

        #region DrawButton

        const float _buttonCornerRadius = 4.0f;
        private void DrawButton(int preIcon, string text, float x, float y, float w, float h, Color col)
        {
            float iw = 0.0f;

            Paint bg = Paint.LinearGradient(x, y, x, y + h, new Color(255, 255, 255, IsTransparent(col) ? (byte)16 : (byte)32), new Color(0, 0, 0, IsTransparent(col) ? (byte)16 : (byte)32));
            
            _ctx.BeginPath();
            _ctx.RoundedRect(x + 1.0f, y + 1.0f, w - 2.0f, h - 2.0f, _buttonCornerRadius - 1.0f);
            
            if (!IsTransparent(col))
            {
                _ctx.FillColor(col);
                _ctx.Fill();
            }

            _ctx.FillPaint(bg);
            _ctx.Fill();

            _ctx.BeginPath();
            _ctx.RoundedRect(x + 0.5f, y + 0.5f, w - 1.0f, h - 1.0f, _buttonCornerRadius - 0.5f);
            _ctx.StrokeColor(new Color(0, 0, 0, 48));
            _ctx.Stroke();

            _ctx.FontSize(17.0f);
            _ctx.FontFaceId(_fontBold);
            
            float tw = _ctx.TextBounds(0.0f, 0.0f, text, out _);
            
            if (preIcon != 0)
            {
                _ctx.FontSize(h * 1.3f);
                _ctx.FontFaceId(_fontIcons);
                _ctx.FontSize(ICON_SIZE);
                iw = _ctx.TextBounds(0.0f, 0.0f, preIcon, out _);
                iw += h * 0.15f;
            }

            if (preIcon != 0)
            {
                _ctx.FontSize(h * 1.3f);
                _ctx.FontFaceId(_fontIcons);
                _ctx.FillColor(new Color(255, 255, 255, 96));
                _ctx.TextAlign(TextAlignment.Left | TextAlignment.Middle);
                _ctx.FontSize(ICON_SIZE);
                _ctx.Text(x + w * 0.5f - tw * 0.5f - iw * 0.75f, y + h * 0.5f, preIcon);
            }

            _ctx.FontSize(17.0f);
            _ctx.FontFaceId(_fontBold);
            _ctx.TextAlign(TextAlignment.Left | TextAlignment.Middle);
            _ctx.FillColor(new Color(0, 0, 0, 160));
            _ctx.Text(x + w * 0.5f - tw * 0.5f + iw * 0.25f, y + h * 0.5f - 1.0f, text);
            _ctx.FillColor(new Color(255, 255, 255, 160));
            _ctx.Text(x + w * 0.5f - tw * 0.5f + iw * 0.25f, y + h * 0.5f, text);
        }

        #endregion

        #region DrawSlider

        private void DrawSlider(float pos, float x, float y, float w, float h)
        {
            float cy = y + (int)(h * 0.5f);
            float kr = (int)(h * 0.25f);

            _ctx.SaveState();

            Paint bg = Paint.BoxGradient(x, cy - 2.0f + 1.0f, w, 4.0f, 2.0f, 2.0f, new Color(0, 0, 0, 32), new Color(0, 0, 0, 128));
            
            _ctx.BeginPath();
            _ctx.RoundedRect(x, cy - 2.0f, w, 4.0f, 2.0f);
            _ctx.FillPaint(bg);
            _ctx.Fill();

            bg = Paint.RadialGradient(x + (int)(pos * w), cy + 1.0f, kr - 3.0f, kr + 3.0f, new Color(0, 0, 0, 64), new Color(0, 0, 0, 0));
            
            _ctx.BeginPath();
            _ctx.Rect(x + (int)(pos * w) - kr - 5, cy - kr - 5, kr * 2.0f + 5.0f + 5.0f, kr * 2.0f + 5.0f + 5.0f + 3.0f);
            _ctx.Circle(x + (int)(pos * w), cy, kr);
            _ctx.PathWinding(Solidity.Hole);
            _ctx.FillPaint(bg);
            _ctx.Fill();

            Paint knob = Paint.LinearGradient(x, cy - kr, x, cy + kr, new Color(255, 255, 255, 16), new Color(0, 0, 0, 16));
            
            _ctx.BeginPath();
            _ctx.Circle(x + (int)(pos * w), cy, kr - 1.0f);
            _ctx.FillColor(new Color(40, 43, 48, 255));
            _ctx.Fill();
            _ctx.FillPaint(knob);
            _ctx.Fill();

            _ctx.BeginPath();
            _ctx.Circle(x + (int)(pos * w), cy, kr - 0.5f);
            _ctx.StrokeColor(new Color(0, 0, 0, 92));
            _ctx.Stroke();

            _ctx.RestoreState();
        }

        #endregion

        #region DrawEyes

        private void DrawEyes(float x, float y, float w, float h, float mx, float my, float t)
        {
            float ex = w * 0.23f;
            float ey = h * 0.5f;
            float lx = x + ex;
            float ly = y + ey;
            float rx = x + w - ex;
            float ry = y + ey;
            float br = MathF.Min(ex, ey) * 0.5f;
            float blink = 1.0f - MathF.Pow(MathF.Sin(t * 0.5f), 200.0f) * 0.8f;

            Paint bg = Paint.LinearGradient(x, y + h * 0.5f, x + w * 0.1f, y + h, new Color(0, 0, 0, 32), new Color(0, 0, 0, 16));
            _ctx.BeginPath();
            _ctx.Ellipse(lx + 3.0f, ly + 16.0f, ex, ey);
            _ctx.Ellipse(rx + 3.0f, ry + 16.0f, ex, ey);
            _ctx.FillPaint(bg);
            _ctx.Fill();

            bg = Paint.LinearGradient(x, y + h * 0.25f, x + w * 0.1f, y + h, new Color(220, 220, 220, 255), new Color(128, 128, 128, 255));
            _ctx.BeginPath();
            _ctx.Ellipse(lx, ly, ex, ey);
            _ctx.Ellipse(rx, ry, ex, ey);
            _ctx.FillPaint(bg);
            _ctx.Fill();

            float dx = (mx - rx) / (ex * 10.0f);
            float dy = (my - ry) / (ey * 10.0f);
            float d = MathF.Sqrt(dx * dx + dy * dy);
            if (d > 1.0f)
            {
                dx /= d;
                dy /= d;
            }
            dx *= ex * 0.4f;
            dy *= ey * 0.5f;
            _ctx.BeginPath();
            _ctx.Ellipse(lx + dx, ly + dy + ey * 0.25f * (1.0f - blink), br, br * blink);
            _ctx.FillColor(new Color(32, 32, 32, 255));
            _ctx.Fill();

            dx = (mx - rx) / (ex * 10.0f);
            dy = (my - ry) / (ey * 10.0f);
            d = MathF.Sqrt(dx * dx + dy * dy);
            if (d > 1.0f)
            {
                dx /= d;
                dy /= d;
            }
            dx *= ex * 0.4f;
            dy *= ey * 0.5f;
            _ctx.BeginPath();
            _ctx.Ellipse(rx + dx, ry + dy + ey * 0.25f * (1.0f - blink), br, br * blink);
            _ctx.FillColor(new Color(32, 32, 32, 255));
            _ctx.Fill();

            Paint gloss = Paint.RadialGradient(lx - ex * 0.25f, ly - ey * 0.5f, ex * 0.1f, ex * 0.75f, new Color(255, 255, 255, 128), new Color(255, 255, 255, 0));
            _ctx.BeginPath();
            _ctx.Ellipse(lx, ly, ex, ey);
            _ctx.FillPaint(gloss);
            _ctx.Fill();

            gloss = Paint.RadialGradient(rx - ex * 0.25f, ry - ey * 0.5f, ex * 0.1f, ex * 0.75f, new Color(255, 255, 255, 128), new Color(255, 255, 255, 0));
            _ctx.BeginPath();
            _ctx.Ellipse(rx, ry, ex, ey);
            _ctx.FillPaint(gloss);
            _ctx.Fill();
        }

        #endregion

        #region DrawGraph

        private void DrawGraph(float x, float y, float w, float h, float t)
        {
            float dx = w / 5.0f;

            Span<float> samples = stackalloc float[]
            {
                (1 + MathF.Sin(t * 1.2345f + MathF.Cos(t * 0.33457f) * 0.44f)) * 0.5f,
                (1 + MathF.Sin(t * 0.68363f + MathF.Cos(t * 1.3f) * 1.55f)) * 0.5f,
                (1 + MathF.Sin(t * 1.1642f + MathF.Cos(t * 0.33457f) * 1.24f)) * 0.5f,
                (1 + MathF.Sin(t * 0.56345f + MathF.Cos(t * 1.63f) * 0.14f)) * 0.5f,
                (1 + MathF.Sin(t * 1.6245f + MathF.Cos(t * 0.254f) * 0.3f)) * 0.5f,
                (1 + MathF.Sin(t * 0.345f + MathF.Cos(t * 0.03f) * 0.6f)) * 0.5f
            };

            Span<float> sx = stackalloc float[6];
            Span<float> sy = stackalloc float[6];

            for (int i = 0; i < 6; i++)
            {
                sx[i] = x + i * dx;
                sy[i] = y + h * samples[i] * 0.8f;
            }

            Paint bg = Paint.LinearGradient(x, y, x, y + h, new Color(0, 160, 192, 0), new Color(0, 160, 192, 64));
            _ctx.BeginPath();
            _ctx.MoveTo(sx[0], sy[0]);
            for (int i = 1; i < 6; i++)
            {
                _ctx.BezierTo(sx[i - 1] + dx * 0.5f, sy[i - 1], sx[i] - dx * 0.5f, sy[i], sx[i], sy[i]);
            }
            _ctx.LineTo(x + w, y + h);
            _ctx.LineTo(x, y + h);
            _ctx.FillPaint(bg);
            _ctx.Fill();

            _ctx.BeginPath();
            _ctx.MoveTo(sx[0], sy[0] + 2);
            for (int i = 1; i < 6; i++)
            {
                _ctx.BezierTo(sx[i - 1] + dx * 0.5f, sy[i - 1] + 2, sx[i] - dx * 0.5f, sy[i] + 2, sx[i], sy[i] + 2);
            }
            _ctx.StrokeColor(new Color(0, 0, 0, 32));
            _ctx.StrokeWidth(3.0f);
            _ctx.Stroke();

            _ctx.BeginPath();
            _ctx.MoveTo(sx[0], sy[0]);
            for (int i = 1; i < 6; i++)
            {
                _ctx.BezierTo(sx[i - 1] + dx * 0.5f, sy[i - 1], sx[i] - dx * 0.5f, sy[i], sx[i], sy[i]);
            }
            _ctx.StrokeColor(new Color(0, 160, 192, 255));
            _ctx.StrokeWidth(3.0f);
            _ctx.Stroke();

            for (int i = 0; i < 6; i++)
            {
                bg = Paint.RadialGradient(sx[i], sy[i] + 2, 3.0f, 8.0f, new Color(0, 0, 0, 32), new Color(0, 0, 0, 0));
                _ctx.BeginPath();
                _ctx.Rect(sx[i] - 10, sy[i] - 10 + 2, 20, 20);
                _ctx.FillPaint(bg);
                _ctx.Fill();
            }

            _ctx.BeginPath();
            for (int i = 0; i < 6; i++)
            {
                _ctx.Circle(sx[i], sy[i], 4.0f);
            }
            _ctx.FillColor(new Color(0, 160, 192, 255));
            _ctx.Fill();
            _ctx.BeginPath();
            for (int i = 0; i < 6; i++)
            {
                _ctx.Circle(sx[i], sy[i], 2.0f);
            }
            _ctx.FillColor(new Color(220, 220, 220, 255));
            _ctx.Fill();
        }

        #endregion

        #region DrawSpinner

        void DrawSpinner( float cx, float cy, float r, float t)
        {
            float a0 = 0.0f + t * 6;
            float a1 = (float)Math.PI + t * 6;
            float r0 = r;
            float r1 = r * 0.75f;
            float ax, ay, bx, by;

            _ctx.SaveState();

            _ctx.BeginPath();
            _ctx.Arc(cx, cy, r0, a0, a1, Winding.Clockwise);
            _ctx.Arc(cx, cy, r1, a1, a0, Winding.CounterClockwise);
            _ctx.ClosePath();

            ax = cx + (float)Math.Cos(a0) * (r0 + r1) * 0.5f;
            ay = cy + (float)Math.Sin(a0) * (r0 + r1) * 0.5f;
            bx = cx + (float)Math.Cos(a1) * (r0 + r1) * 0.5f;
            by = cy + (float)Math.Sin(a1) * (r0 + r1) * 0.5f;

            Paint paint = Paint.LinearGradient(ax, ay, bx, by, new Color(0, 0, 0, 0), new Color(0, 0, 0, 128));
            
            _ctx.FillPaint(paint);
            _ctx.Fill();

            _ctx.RestoreState();
        }

        #endregion

        #region DrawThumbnails

        private void DrawThumbnails(float x, float y, float w, float h, int[] textures, float t)
        {
            const float cornerRadius = 3.0f;
            const float thumb = 60.0f;
            const float arry = 30.5f;
            float stackh = textures.Length / 2 * (thumb + 10.0f) + 10.0f;
            float u = (1.0f + MathF.Cos(t * 0.5f)) * 0.5f;
            float u2 = (1.0f - MathF.Cos(t * 0.2f)) * 0.5f;

            _ctx.SaveState();

            Paint shadowPaint = Paint.BoxGradient(x, y + 4.0f, w, h, cornerRadius * 2.0f, 20.0f, new Color(0, 0, 0, 128), new Color(0, 0, 0, 0));
            
            /*_ctx.BeginPath();
            _ctx.Rect(x - 10.0f, y - 10.0f, w + 20.0f, h + 30.0f);
            _ctx.RoundedRect(x, y, w, h, cornerRadius);
            _ctx.PathWinding(Solidity.Hole);
            _ctx.FillPaint(shadowPaint);
            _ctx.Fill();*/

            _ctx.BeginPath();
            _ctx.RoundedRect(x, y, w, h, cornerRadius);
            _ctx.MoveTo(x - 10.0f, y + arry);
            _ctx.LineTo(x + 1.0f, y + arry - 11.0f);
            _ctx.LineTo(x + 1.0f, y + arry + 11.0f);
            _ctx.FillColor(new Color(200, 200, 200, 255));
            _ctx.Fill();

            _ctx.SaveState();

            _ctx.Scissor(x, y, w, h);
            _ctx.Translate(0, -(stackh - h) * u);

            float dv = 1.0f / (textures.Length - 1);

            for (uint i = 0; i < textures.Length; i++)
            {
                float iw, ih;
                float ix, iy;
                float tx = x + 10.0f;
                float ty = y + 10.0f;
                tx += i % 2 * (thumb + 10.0f);
                ty += i / 2 * (thumb + 10.0f);
                
                _ctx.GetTextureSize(textures[i], out uint imgW, out uint imgH);

                if (imgW < imgH)
                {
                    iw = thumb;
                    ih = iw * imgH / imgW;
                    ix = 0.0f;
                    iy = -(ih - thumb) * 0.5f;
                }
                else
                {
                    ih = thumb;
                    iw = ih * imgW / imgH;
                    ix = -(iw - thumb) * 0.5f;
                    iy = 0.0f;
                }

                float v = i * dv;
                float a = Math.Clamp((u2 - v) / dv, 0.0f, 1.0f);

                if (a < 1.0f)
                {
                    DrawSpinner(tx + thumb / 2.0f, ty + thumb / 2.0f, thumb * 0.25f, t);
                }

                Paint imgPaint = Paint.ImagePattern(tx + ix, ty + iy, iw, ih, 0.0f / 180.0f * MathF.PI, textures[i], Color.Transparent);
                
                _ctx.BeginPath();
                _ctx.RoundedRect(tx, ty, thumb, thumb, 5.0f);
                _ctx.FillPaint(imgPaint);
                _ctx.Fill();

                shadowPaint = Paint.BoxGradient(tx - 1.0f, ty, thumb + 2.0f, thumb + 2.0f, 5.0f, 3.0f, new Color(0, 0, 0, 128), new Color(0, 0, 0, 0));
                
                _ctx.BeginPath();
                _ctx.Rect(tx - 5.0f, ty - 5.0f, thumb + 10.0f, thumb + 10.0f);
                _ctx.RoundedRect(tx, ty, thumb, thumb, 6.0f);
                _ctx.PathWinding(Solidity.Hole);
                _ctx.FillPaint(shadowPaint);
                _ctx.Fill();

                // note: creates flickering
                /*_ctx.BeginPath();
                _ctx.RoundedRect(tx + 0.5f, ty + 0.5f, thumb - 1.0f, thumb - 1.0f, 4.0f - 0.5f);
                _ctx.StrokeWidth(1.0f);
                _ctx.StrokeColor(new Color(255, 255, 255, 192));
                _ctx.Fill();*/
            }

            _ctx.RestoreState();

            Paint fadePaint = Paint.LinearGradient(x, y, x, y + 6.0f, new Color(200, 200, 200, 255), new Color(200, 200, 200, 0));
            
            _ctx.BeginPath();
            _ctx.Rect(x + 4.0f, y, w - 8.0f, 6.0f);
            _ctx.FillPaint(fadePaint);
            _ctx.Fill();

            fadePaint = Paint.LinearGradient(x, y + h, x, y + h - 6.0f, new Color(200, 200, 200, 255), new Color(200, 200, 200, 0));
            
            _ctx.BeginPath();
            _ctx.Rect(x + 4.0f, y + h - 6.0f, w - 8.0f, 6.0f);
            _ctx.FillPaint(fadePaint);
            _ctx.Fill();

            shadowPaint = Paint.BoxGradient(x + w - 12.0f + 1.0f, y + 4.0f + 1.0f, 8.0f, h - 8.0f, 3.0f, 4.0f, new Color(0, 0, 0, 32), new Color(0, 0, 0, 92));
            
            _ctx.BeginPath();
            _ctx.RoundedRect(x + w - 12.0f, y + 4.0f, 8.0f, h - 8.0f, 3.0f);
            _ctx.FillPaint(shadowPaint);
            _ctx.Fill();

            float scrollH = h / stackh * (h - 8.0f);
            
            shadowPaint = Paint.BoxGradient(x + w - 12.0f - 1.0f, y + 4.0f + (h - 8.0f - scrollH) * u - 1.0f, 8.0f, scrollH, 3.0f, 4.0f, new Color(220, 220, 220, 255), new Color(128, 128, 128, 255));
            
            _ctx.BeginPath();
            _ctx.RoundedRect(x + w - 12.0f + 1.0f, y + 4.0f + 1.0f + (h - 8.0f - scrollH) * u, 8.0f - 2.0f, scrollH - 2.0f, 2.0f);
            _ctx.FillPaint(shadowPaint);
            _ctx.Fill();

            _ctx.RestoreState();
        }

        #endregion

        #region DrawColourwheel

        private void DrawColourwheel(float x, float y, float w, float h, float t)
        {
            float hue = MathF.Sin(t * 0.12f);

            _ctx.SaveState();

            float cx = x + w * 0.5f;
            float cy = y + h * 0.5f;
            float r1 = (w < h ? w : h) * 0.5f - 5.0f;
            float r0 = r1 - 20.0f;
            float aeps = 0.5f / r1;

            float a0, a1, ax, ay, bx, by;

            for (int i = 0; i < 6; i++)
            {
                a0 = i / 6.0f * MathF.PI * 2.0f - aeps;
                a1 = (float)(i + 1.0f) / 6.0f * MathF.PI * 2.0f + aeps;

                _ctx.BeginPath();
                _ctx.Arc(cx, cy, r0, a0, a1, Winding.Clockwise);
                _ctx.Arc(cx, cy, r1, a1, a0, Winding.CounterClockwise);
                _ctx.ClosePath();

                ax = cx + MathF.Cos(a0) * (r0 + 1) * 0.5f;
                ay = cy + MathF.Sin(a0) * (r0 + 1) * 0.5f;
                bx = cx + MathF.Cos(a1) * (r0 + 1) * 0.5f;
                by = cy + MathF.Sin(a1) * (r0 + 1) * 0.5f;
                
                Paint paint0 = Paint.LinearGradient(ax, ay, bx, by, Color.HSLA(a0 / (MathF.PI * 2), 1.0f, 0.55f, 255), Color.HSLA(a1 / (MathF.PI * 2), 1.0f, 0.55f, 255));
                
                _ctx.FillPaint(paint0);
                _ctx.Fill();
            }

            _ctx.BeginPath();
            _ctx.Circle(cx, cy, r0 - 0.5f);
            _ctx.Circle(cx, cy, r1 + 0.5f);
            _ctx.StrokeColor(new Color(0, 0, 0, 64));
            _ctx.StrokeWidth(1.0f);
            _ctx.Stroke();

            _ctx.SaveState();
            _ctx.Translate(cx, cy);
            _ctx.Rotate(hue * MathF.PI * 2);

            _ctx.StrokeWidth(2.0f);
            _ctx.BeginPath();
            _ctx.Rect(r0 - 1, -3, r1 - r0 + 2, 6);
            _ctx.StrokeColor(new Color(255, 255, 255, 255));
            _ctx.Stroke();

            Paint paint1 = Paint.BoxGradient(r0 - 3, -5, r1 - r0 + 6, 10, 2, 4, new Color(0, 0, 0, 128), new Color(0, 0, 0, 0));
            
            _ctx.BeginPath();
            _ctx.Rect(r0 - 2 - 10, -4 - 10, r1 - r0 + 4 + 20, 8 + 20);
            _ctx.Rect(r0 - 2, -4, r1 - r0 + 4, 8);
            _ctx.PathWinding(Solidity.Hole);
            _ctx.FillPaint(paint1);
            _ctx.Fill();

            float r = r0 - 6;
            ax = MathF.Cos(120.0f / 180.0f * MathF.PI) * r;
            ay = MathF.Sin(120.0f / 180.0f * MathF.PI) * r;
            bx = MathF.Cos(-120.0f / 180.0f * MathF.PI) * r;
            by = MathF.Sin(-120.0f / 180.0f * MathF.PI) * r;

            _ctx.BeginPath();
            _ctx.MoveTo(r, 0);
            _ctx.LineTo(ax, ay);
            _ctx.LineTo(bx, by);
            _ctx.ClosePath();

            paint1 = Paint.LinearGradient(r, 0, ax, ay, Color.HSLA(hue, 1.0f, 0.5f, 255), new Color(255, 255, 255, 255));
            
            _ctx.FillPaint(paint1);
            _ctx.Fill();

            paint1 = Paint.LinearGradient((r + ax) * 0.5f, (0 + ay) * 0.5f, bx, by, new Color(0, 0, 0, 0), new Color(0, 0, 0, 255));
            
            _ctx.FillPaint(paint1);
            _ctx.Fill();
            _ctx.StrokeColor(new Color(0, 0, 0, 64));
            _ctx.Stroke();

            ax = MathF.Cos(120.0f / 180.0f * MathF.PI) * r * 0.3f;
            ay = MathF.Sin(120.0f / 180.0f * MathF.PI) * r * 0.4f;
            _ctx.StrokeWidth(2.0f);
            _ctx.BeginPath();
            _ctx.Circle(ax, ay, 5);
            _ctx.StrokeColor(new Color(255, 255, 255, 192));
            _ctx.Stroke();

            paint1 = Paint.RadialGradient(ax, ay, 7, 9, new Color(0, 0, 0, 64), new Color(0, 0, 0, 0));

            _ctx.BeginPath();
            _ctx.Rect(ax - 20, ay - 20, 40, 40);
            _ctx.Circle(ax, ay, 7);
            _ctx.PathWinding(Solidity.Hole);
            _ctx.FillPaint(paint1);
            _ctx.Fill();

            _ctx.RestoreState();
            _ctx.RestoreState();
        }

        #endregion

        #region DrawLines

        float[] _linePts = new float[4 * 2];
        LineCap[] _lineJoins = { LineCap.Miter, LineCap.Round, LineCap.Bevel };
        LineCap[] _lineCaps = { LineCap.Butt, LineCap.Round, LineCap.Square };

        private unsafe void DrawLines(float x, float y, float w, float h, float t)
        {
            float pad = 5.0f;
            float s = w / 9.0f - pad * 2;
            
            _ctx.SaveState();

            _linePts[0] = -s * 0.25f + MathF.Cos(t * 0.3f) * s * 0.5f;
            _linePts[1] = MathF.Sin(t * 0.3f) * s * 0.5f;
            _linePts[2] = -s * 0.25f;
            _linePts[3] = 0;
            _linePts[4] = s * 0.25f;
            _linePts[5] = 0;
            _linePts[6] = s * 0.25f + MathF.Cos(-t * 0.3f) * s * 0.5f;
            _linePts[7] = MathF.Sin(-t * 0.3f) * s * 0.5f;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    float fx = x + s * 0.5f + (i * 3 + j) / 9.0f * w + pad;
                    float fy = y - s * 0.5f + pad;

                    _ctx.LineCap(_lineCaps[i]);
                    _ctx.LineJoin(_lineJoins[j]);

                    _ctx.StrokeWidth(s * 0.3f);
                    _ctx.StrokeColor(new Color(0, 0, 0, 160));

                    _ctx.BeginPath();
                    _ctx.MoveTo(fx + _linePts[0], fy + _linePts[1]);
                    _ctx.LineTo(fx + _linePts[2], fy + _linePts[3]);
                    _ctx.LineTo(fx + _linePts[4], fy + _linePts[5]);
                    _ctx.LineTo(fx + _linePts[6], fy + _linePts[7]);
                    _ctx.Stroke();

                    _ctx.LineCap(LineCap.Butt);
                    _ctx.LineJoin(LineCap.Bevel);

                    _ctx.StrokeWidth(1.0f);
                    _ctx.StrokeColor(new Color(0, 192, 255, 255));

                    _ctx.BeginPath();
                    _ctx.MoveTo(fx + _linePts[0], fy + _linePts[1]);
                    _ctx.LineTo(fx + _linePts[2], fy + _linePts[3]);
                    _ctx.LineTo(fx + _linePts[4], fy + _linePts[5]);
                    _ctx.LineTo(fx + _linePts[6], fy + _linePts[7]);
                    _ctx.Stroke();
                }
            }

            _ctx.RestoreState();
        }

        #endregion

        #region DrawParagraph

        private void DrawParagraph(float x, float y, float width, float mx, float my)
        {
            const string text = "This is a longer chunk of text.\n  \n"
                + "  Would have used lorem ipsum but she was busy jumping over the lazy dog with the fox and all the men who came to the aid of the party.";
            const string hoverText = "Hover your mouse over the text to see the calculated caret position.";
            
            uint gutter = 0;
            uint lnum = 0;
            float gx = 0.0f, gy = 0.0f;
            Rect bounds;
            float px;

            _ctx.SaveState();

            _ctx.FontSize(18.0f);
            _ctx.FontFaceId(_fontNormal);
            _ctx.TextAlign(TextAlignment.Left | TextAlignment.Top);
            _ctx.TextMetrics(out _, out _, out float lineh);

            // Test TextBox function
            /*_ctx.TextColor(Color.White);
            _ctx.TextLineHeight(1.3f);
            _ctx.TextAlign(TextAlignment.Center | TextAlignment.Top);
            _ctx.TextBox(new Vector2(x, y), width, text, 50);
            return;*/
            
            // increase line height sligthly
            lineh *= 1.05f;


            // text looping params
            ReadOnlySpan<char> wholeText = text;
            ReadOnlySpan<char> rowText;

            _ctx.TextBreakLines(wholeText, width, out ReadOnlySpan<TextRow> rows);

            foreach (TextRow row in rows)
            {
                bool hit = mx > x && mx < x + width && my >= y && my < y + lineh;

                _ctx.BeginPath();
                _ctx.FillColor(new Color(255, 255, 255, hit ? (byte)64 : (byte)16));
                _ctx.Rect(x + row.MinX, y, row.MaxX - row.MinX, lineh);
                _ctx.Fill();

                _ctx.FillColor(Color.White);

                rowText = wholeText.Slice(row.StartPos, row.TextLength);

                _ctx.Text(x, y, rowText);

                if (hit)
                {
                    float caretX = mx < x + row.Width / 2.0f ? x : x + row.Width;
                    px = x;

                    _ctx.TextGlyphPositions(x, y, rowText, 100, out ReadOnlySpan<GlyphPosition> glyphs);

                    for (int j = 0; j < glyphs.Length; j++)
                    {
                        float x0 = glyphs[j].X;
                        float x1 = j + 1 < glyphs.Length ? glyphs[j + 1].X : x + row.Width;
                        gx = x0 * 0.3f + x1 * 0.7f;

                        if (mx >= px && mx < gx)
                        {
                            caretX = glyphs[j].X;
                        }
                        px = gx;
                    }
                    _ctx.BeginPath();
                    _ctx.FillColor(new Color(255, 192, 0, 255));
                    _ctx.Rect(caretX, y, 1.0f, lineh);
                    _ctx.Fill();

                    gutter = lnum + 1;
                    gx = x - 10.0f;
                    gy = y + lineh / 2.0f;
                }

                lnum++;
                y += lineh;
            }

            if (gutter != 0)
            {
                _ctx.FontSize(12.0f);
                _ctx.TextAlign(TextAlignment.Right | TextAlignment.Middle);

                _ctx.TextBounds(gx, gy, gutter.ToString(), out bounds);

                _ctx.BeginPath();
                _ctx.FillColor(new Color(255, 192, 0, 255));
                _ctx.RoundedRect(bounds.X - 4.0f, bounds.Y - 2.0f, bounds.Size.X + 8.0f, bounds.Size.Y + 4.0f, (bounds.Size.Y + 4.0f) / 2.0f - 1.0f);
                _ctx.Fill();

                _ctx.FillColor(new Color(32, 32, 32, 255));
                _ctx.Text(gx, gy, gutter.ToString());
            }

            y += 20.0f;

            _ctx.FontSize(13.0f);
            _ctx.TextAlign(TextAlignment.Left | TextAlignment.Top);
            _ctx.TextLineHeight(1.2f);

            _ctx.TextBoxBounds(x, y, 150.0f, hoverText, out bounds);

            // ensure mouse is not inside hover text
            if(!bounds.Contains(mx, my))
            {
                gx = Math.Clamp(mx, bounds.X, bounds.Max.X) - mx;
                gy = Math.Clamp(my, bounds.Y, bounds.Max.Y) - my;

                float a = MathF.Sqrt(gx * gx + gy * gy) / 30.0f;
                a = Math.Clamp(a, 0.0f, 1.0f);

                _ctx.GlobalAlpha(a);

                _ctx.BeginPath();
                _ctx.FillColor(new Color(220, 220, 220, 255));
                _ctx.RoundedRect(new Rect(bounds.Position - new Vector2(2.0f), bounds.Size + new Vector2(4.0f)), 3.0f);

                px = (bounds.Max.X + bounds.X) / 2.0f;

                _ctx.MoveTo(px, bounds.Y - 10.0f);
                _ctx.LineTo(px + 7.0f, bounds.Y + 1.0f);
                _ctx.LineTo(px - 7.0f, bounds.Y + 1.0f);
                _ctx.Fill();

                _ctx.FillColor(new Color(0, 0, 0, 220));

                // hover text looping
                ReadOnlySpan<char> hoverSpanText = hoverText;

                _ctx.TextBreakLines(hoverSpanText, 150.0f, out ReadOnlySpan<TextRow> hoverRows);

                foreach (TextRow hoverRow in hoverRows)
                {
                    _ctx.Text(x, y, hoverSpanText.Slice(hoverRow.StartPos, hoverRow.TextLength));

                    // lineh is for bigger font!
                    y += lineh - 4;
                }
            }

            _ctx.RestoreState();
        }

        #endregion

        #region DrawWidths

        float _drawWidth;
        private void DrawWidths(float x, float y, float width)
        {
            _ctx.SaveState();

            _ctx.StrokeColor(Color.Black);

            for (uint i = 0; i < 20; i++)
            {
                _drawWidth = (i + 0.5f) * 0.1f;
                _ctx.StrokeWidth(_drawWidth);

                _ctx.BeginPath();
                _ctx.MoveTo(x, y);
                _ctx.LineTo(x + width, y + width * 0.3f);
                _ctx.Stroke();

                y += 10.0f;
            }

            _ctx.RestoreState();
        }

        #endregion

        #region DrawCaps

        LineCap[] _drawCaps = { LineCap.Butt, LineCap.Round, LineCap.Square };

        private void DrawCaps(float x, float y, float width)
        {
            const float lineWidth = 8.0f;

            _ctx.SaveState();

            _ctx.BeginPath();
            _ctx.Rect(x - lineWidth / 2.0f, y, width + lineWidth, 40.0f);
            _ctx.FillColor(new Color(255, 255, 255, 32));
            _ctx.Fill();

            _ctx.BeginPath();
            _ctx.Rect(x, y, width, 40.0f);
            _ctx.FillColor(new Color(255, 255, 255, 32));
            _ctx.Fill();

            _ctx.StrokeWidth(lineWidth);

            for (uint i = 0; i < 3; i++)
            {
                _ctx.LineCap(_drawCaps[i]);
                _ctx.StrokeColor(Color.Black);

                _ctx.BeginPath();
                _ctx.MoveTo(x, y + i * 10.0f + 5.0f);
                _ctx.LineTo(x + width, y + i * 10.0f + 5.0f);
                _ctx.Stroke();
            }

            _ctx.RestoreState();
        }

        #endregion

        #region DrawScissor

        private void DrawScissor(float x, float y, float t)
        {
            _ctx.SaveState();

            _ctx.Translate(x, y);
            _ctx.Rotate(float.DegreesToRadians(5.0f));

            _ctx.BeginPath();
            _ctx.Rect(-20.0f, -20.0f, 60.0f, 40.0f);
            _ctx.FillColor(Color.Red);
            _ctx.Fill();
            _ctx.Scissor(-20.0f, -20.0f, 60.0f, 40.0f);

            _ctx.Translate(40.0f, 0.0f);
            _ctx.Rotate(t);

            _ctx.SaveState();

            _ctx.ResetScissor();
            
            _ctx.BeginPath();
            _ctx.Rect(-20.0f, -10.0f, 60.0f, 30.0f);
            _ctx.FillColor(new Color(255, 128, 0, 64));
            _ctx.Fill();
            _ctx.RestoreState();

            _ctx.IntersectScissor(-20.0f, -10.0f, 60.0f, 30.0f);

            _ctx.BeginPath();
            _ctx.Rect(-20.0f, -10.0f, 60.0f, 30.0f);
            _ctx.FillColor(new Color(255, 128, 0, 255));
            _ctx.Fill();

            _ctx.RestoreState();
        }

        #endregion

        #region DrawStars (pentagrams)

        void DrawStars(Vector2 position, float radius, int outlineWidth)
        {
            // Filled
            _ctx.BeginPath();
            _ctx.Pentagram(position, radius);
            _ctx.FillColor(Color.Yellow);
            _ctx.Fill();

            position += new Vector2(radius * 2 + 5, 0);

            // Filled & Outline
            _ctx.BeginPath();
            _ctx.Pentagram(position, radius);
            _ctx.StrokeWidth(outlineWidth);
            _ctx.StrokeColor(Color.Red);
            _ctx.Stroke();

            _ctx.BeginPath();
            _ctx.Pentagram(position, radius - outlineWidth);
            _ctx.FillColor(Color.Yellow);
            _ctx.Fill();

            position += new Vector2(radius * 2 + 5, 0);

            // Outline
            _ctx.BeginPath();
            _ctx.Pentagram(position, radius);
            _ctx.StrokeWidth(outlineWidth);
            _ctx.StrokeColor(Color.Red);
            _ctx.Stroke();
        }

        #endregion

        private static bool IsTransparent(Color col)
        {
            return col.R == 0.0f && col.G == 0.0f && col.B == 0.0f && col.A == 0.0f;
        }
    }
}
