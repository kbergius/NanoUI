using NanoUI.Common;
using System.Numerics;

namespace NanoUI.Nvg.Data
{
    // todo: should some values be nullable?
    internal struct NvgState
    {
        public Paint Fill;
        public Paint Stroke;
        public float StrokeWidth;
        public float MiterLimit;
        public LineCap LineJoin;
        public LineCap LineCap;
        public float Alpha;
        public Matrix3x2 Transform;
        public NvgScissor Scissor;
        public float FontSize;
        // note: this is additional spacing (default is 0)
        public float TextCharSpacing;
        // note: this is proportional line height (default is 1)
        public float TextLineHeight;
        public TextAlignment TextAlign;
        public int FontId;
        public int TextBlur;
        public int TextDilate;
        
        // glyph shapes
        public Color? TextShapeOutline;
        public float TextShapeOutlineWidth;
        public Paint? TextShapeFill;

        public NvgState()
        {
            Reset();
        }

        public void Reset()
        {
            // init defaults
            Fill.Reset(Color.White);
            Stroke.Reset(Color.Black);
            StrokeWidth = 1.0f;
            MiterLimit = 10.0f;
            LineJoin = LineCap.Miter;
            LineCap = LineCap.Butt;
            Alpha = 1.0f;
            Transform = Matrix3x2.Identity;
            Scissor.Reset();
            FontSize = 16.0f;
            TextCharSpacing = 0.0f;
            TextLineHeight = 1.0f;
            TextAlign = TextAlignment.Left | TextAlignment.Baseline;
            FontId = 0;
            TextBlur = 0;
            TextDilate = 0;
            // glyph shapes
            TextShapeOutline = null;
            TextShapeOutlineWidth = 1;
            TextShapeFill = null;
        }
    }
}