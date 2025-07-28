using NanoUI;
using NanoUI.Common;
using NanoUI.Components;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUIDemos.Experimental.Components
{
    public enum PerfGraphMode
    {
        FPS,
        MilliSeconds,
        Percent
    }

    // todo : padding
    // todo: "magical" values
    // todo: percentage seems wrong
    public class UIPerformanceGraph : UIWidget
    {
        const uint GRAPH_HISTORY_COUNT = 100;
        // todo:
        const float PADDING = 6;

        readonly float[] _values = new float[GRAPH_HISTORY_COUNT];

        uint _head;

        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        public UIPerformanceGraph()
        {
            // set defaults to theme impl - prevents circular reference
            FillColor = default;
            Mode = default;
        }

        public UIPerformanceGraph(UIWidget parent)
            : this(parent, "FrameTime")
        {
        }

        public UIPerformanceGraph(UIWidget parent, string caption)
            : base(parent)
        {
            Caption = caption;

            // defaults
            DisablePointerFocus = true;
            Border = false;

            ThemeType = typeof(UILabel);
        }

        #region Properties

        PerfGraphMode? _mode;
        public PerfGraphMode Mode
        {
            get => _mode.HasValue ? _mode.Value : ((ThemeEXT)GetTheme()).PerformanceGraph.Mode;
            set => _mode = value;
        }

        public string? Caption { get; set; }
       
        Color? _fillColor;
        public Color FillColor
        {
            get => _fillColor.HasValue ? _fillColor.Value : ((ThemeEXT)GetTheme()).PerformanceGraph.FillColor;
            set => _fillColor = value;
        }

        #endregion

        #region Drawing

        public override void Draw(NvgContext ctx)
        {
            // update graph
            UpdateGraph();

            // get average
            float avg = GetGraphAverage();

            // background
            //this.DrawBackground(ctx);
            DrawBackgroundBrush(ctx);

            // Graph
            ctx.BeginPath();
            ctx.MoveTo(Position.X, Position.Y + Size.Y);

            if (Mode == PerfGraphMode.FPS)
            {
                for (int i = 0; i < GRAPH_HISTORY_COUNT; i++)
                {
                    float v = 1.0f / (0.00001f + _values[(_head + i) % GRAPH_HISTORY_COUNT]);

                    v = MathF.Min(v, 40.0f);

                    float vx = Position.X + (float)i / (GRAPH_HISTORY_COUNT - 1) * Size.X;
                    float vy = Position.Y + Size.Y - v / 80.0f * Size.Y;

                    ctx.LineTo(vx, vy);
                }
            }
            else if (Mode == PerfGraphMode.Percent)
            {
                for (int i = 0; i < GRAPH_HISTORY_COUNT; i++)
                {
                    float v = _values[(_head + i) % GRAPH_HISTORY_COUNT] * 1.0f;

                    v = MathF.Min(v, 100.0f);

                    float vx = Position.X + (float)i / (GRAPH_HISTORY_COUNT - 1) * Size.X;
                    float vy = Position.Y + Size.Y - v / 100.0f * Size.Y;
                    ctx.LineTo(vx, vy);
                }
            }
            else if (Mode == PerfGraphMode.MilliSeconds)
            {
                for (int i = 0; i < GRAPH_HISTORY_COUNT; i++)
                {
                    float v = _values[(_head + i) % GRAPH_HISTORY_COUNT] * 1000.0f;

                    v = MathF.Min(v, 20.0f);

                    float vx = Position.X + (float)i / (GRAPH_HISTORY_COUNT - 1) * Size.X;
                    float vy = Position.Y + Size.Y - v / 20.0f * Size.Y;

                    ctx.LineTo(vx, vy);
                }
            }

            ctx.LineTo(Position.X + Size.X, Position.Y + Size.Y);
            ctx.FillColor(FillColor);
            ctx.Fill();

            // texts
            ctx.FontFaceId(FontFaceId);
            ctx.FontSize(FontSize);
            ctx.FillColor(TextColor);

            Vector2 valueTopPos = new Vector2(Position.X + Size.X - PADDING, Position.Y + PADDING / 2);
            Vector2 valueBottomPos = new Vector2(Position.X + Size.X - PADDING, Position.Y + Size.Y - PADDING);

            if (!string.IsNullOrEmpty(Caption))
            {
                if (!string.IsNullOrEmpty(Caption))
                {
                    ctx.TextAlign(TextAlignment.Left | TextAlignment.Top);
                    ctx.Text(Position.X + PADDING, valueTopPos.Y, Caption);
                }
            }            

            if (Mode == PerfGraphMode.FPS)
            {
                float val = 1.0f / avg;

                if (!string.IsNullOrEmpty(_milliseconds))
                {
                    ctx.TextAlign(TextAlignment.Right | TextAlignment.Top);
                    ctx.Text(valueTopPos, _milliseconds);
                }

                if (!string.IsNullOrEmpty(_framesPerSecond))
                {
                    ctx.TextAlign(TextAlignment.Right | TextAlignment.Baseline);
                    ctx.Text(valueBottomPos, _framesPerSecond);
                }
            }
            else if (Mode == PerfGraphMode.Percent)
            {
                if (!string.IsNullOrEmpty(_percent))
                {
                    ctx.TextAlign(TextAlignment.Right | TextAlignment.Top);
                    ctx.Text(valueTopPos, _percent);
                }
            }
            else if (Mode == PerfGraphMode.MilliSeconds)
            {
                if (!string.IsNullOrEmpty(_milliseconds))
                {
                    ctx.TextAlign(TextAlignment.Right | TextAlignment.Top);
                    ctx.Text(valueTopPos, _milliseconds);
                }
            }

            // border
            this.DrawBorder(ctx, false);
        }

        #endregion

        #region Private

        // note: these are used just to reduce string allocations unnecessarily (too often)
        string? _milliseconds;
        string? _framesPerSecond;
        string? _percent;
        float _accumulatedTime = 1;

        void UpdateGraph()
        {
            _head = (_head + 1) % GRAPH_HISTORY_COUNT;
            _values[_head] = Screen.DeltaSeconds;

            _accumulatedTime += Screen.DeltaSeconds;

            if (_accumulatedTime > Globals.UPDATE_DELAY)
            {
                _accumulatedTime = 0;

                float average = GetGraphAverage();
                float framesPerSec = 1.0f / average;

                // note: these use string interpolation
                if (Mode == PerfGraphMode.Percent)
                {
                    _percent = $"{average:0.000} %";
                }
                else if (Mode == PerfGraphMode.FPS)
                {
                    _milliseconds = $"{average * 1000.0f:0.000} ms";
                    _framesPerSecond = $"{framesPerSec:0.00} FPS";
                }
                else
                {
                    _milliseconds = $"{average * 1000.0f:0.000} ms";
                }
            }
        }

        float GetGraphAverage()
        {
            float avg = 0;

            for (uint i = 0; i < GRAPH_HISTORY_COUNT; i++)
            {
                avg += _values[i];
            }

            return avg / GRAPH_HISTORY_COUNT;
        }

        #endregion
    }
}
