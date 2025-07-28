using NanoUI.Common;
using NanoUI.Components;
using NanoUI.Nvg;

namespace NanoUIDemos.Experimental.Components
{
    // This is just an barebones example how to create graph (not supposed to be used as-is)
    public class UIGraph : UIWidget
    {
        // this is used to animate graph (determine if we execute animation func)
        double _accumulatedTime = 0;

        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        public UIGraph()
        {
            // set defaults to theme impl - prevents circular reference
            FillColor = default;
            FillColor2 = default;
            StrokeColor = default;
            StrokeWidth = default;
            AnimationThreshold = default;
        }

        public UIGraph(UIWidget parent)
            : base(parent)
        {
            ThemeType = typeof(UIGraph);
        }

        #region Properties

        // this uses values between 0 - 1
        // todo : could be array/span
        List<float>? _values;
        public List<float>? Values 
        {
            get => _values;
            set => _values = value;
        }

        // threshold when we update values
        // if <= 0 -> no animation
        double? _animationThreshold;
        public double AnimationThreshold
        {
            get => _animationThreshold ?? ((ThemeEXT)GetTheme()).Graph.AnimationThreshold;
            set => _animationThreshold = value;
        }

        int? _strokeWidth;
        public int StrokeWidth
        {
            get => _strokeWidth ?? ((ThemeEXT)GetTheme()).Graph.StrokeWidth;
            set => _strokeWidth = value;
        }

        Color? _strokeColor;
        public Color StrokeColor
        {
            get => _strokeColor ?? ((ThemeEXT)GetTheme()).Graph.StrokeColor;
            set => _strokeColor = value;
        }

        Color? _fillColor;
        public Color FillColor
        {
            get => _fillColor ?? ((ThemeEXT)GetTheme()).Graph.FillColor;
            set => _fillColor = value;
        }

        Color? _fillColor2;
        public Color FillColor2
        {
            get => _fillColor2 ?? ((ThemeEXT)GetTheme()).Graph.FillColor2;
            set => _fillColor2 = value;
        }
                
        #endregion

        #region Drawing

        public override void Draw(NvgContext ctx)
        {
            if (Values == null || Values.Count < 2)
                return;

            // prepare animation
            AnimationFunc();

            // background
            DrawBackgroundBrush(ctx);

            // contruct graph path
            ctx.BeginPath();
            ctx.MoveTo(Position.X, Position.Y + Size.Y);

            for (int i = 0; i < Values.Count; i++)
            {
                float value = Values[i];
                float vx = Position.X + i * Size.X / (Values.Count - 1);
                float vy = Position.Y + (1 - value) * Size.Y;

                ctx.LineTo(vx, vy);
            }

            ctx.LineTo(Position.X + Size.X, Position.Y + Size.Y);

            // fill graph
            // todo : should use Brush
            Paint bgLinear = Paint.LinearGradient(Position.X, Position.Y,
                        Position.X, Position.Y + Size.Y, FillColor, FillColor2);
            ctx.FillPaint(bgLinear);
            ctx.Fill();

            // graph line
            if (StrokeWidth > 0)
            {
                ctx.StrokeColor(StrokeColor);
                ctx.StrokeWidth(StrokeWidth);
                ctx.Stroke();
            }

            // draw possible children (like labels)
            base.Draw(ctx);

            // border
            this.DrawBorder(ctx, true);
        }

        #endregion

        #region Animation

        // if you extend this graph, override this
        protected virtual void AnimationFunc()
        {
            if (AnimationThreshold > 0)
            {
                _accumulatedTime += Screen.DeltaSeconds;

                if (_accumulatedTime > AnimationThreshold)
                {
                    // reset
                    _accumulatedTime = 0;

                    // simple animation
                    float val = Values[0];

                    Values.RemoveAt(0);
                    Values.Add(val);
                }
            }
        }

        #endregion
    }
}
