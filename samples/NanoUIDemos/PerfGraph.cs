using NanoUI.Common;
using NanoUI.Nvg;
using System;
using System.Numerics;

namespace NanoUIDemos
{
    public class PerfGraph
    {
        public enum GraphRenderStyle
        {
            Fps,
            Ms,
            Percent
        }

        const uint GRAPH_HISTORY_COUNT = 100;
        const float PADDING = 8;
        readonly GraphRenderStyle _style;
        readonly string _name;
        readonly string? _timeType;
        readonly float[] _values = new float[GRAPH_HISTORY_COUNT];

        uint _head;

        float w = 240.0f;
        float h = 50f;
        float _fontSize = 16;
        Color _graphColor = new Color(255, 192, 0, 128);
        Color _textColor = new Color(240, 240, 240, 192);

        public PerfGraph(GraphRenderStyle style, string name, string? timeType = null)
        {
            _style = style;
            _name = name;
            _timeType = timeType;
        }


        // note: these are used just to reduce string allocations unnecessarily (too often)
        string? _milliseconds;
        string? _framesPerSecond;
        string? _percent;
        float _updateTime = 1;

        public void Update(float frameTime)
        {
            _head = (_head + 1) % GRAPH_HISTORY_COUNT;
            _values[_head] = frameTime;

            _updateTime += frameTime;

            if(_updateTime > 0.1f)
            {
                _updateTime = 0;

                float average = GraphAverage;
                float framesPerSec = 1.0f / average;

                // note: these use string interpolation
                if (_style == GraphRenderStyle.Percent)
                {
                    _percent = $"{average:0.000} %";
                }
                else if (_style == GraphRenderStyle.Fps)
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

        public void Draw(float x, float y, NvgContext ctx)
        {
            ctx.BeginPath();
            ctx.Rect(x, y, w, h);
            ctx.FillColor(new Color(0, 0, 0, 128));
            ctx.Fill();

            ctx.BeginPath();
            ctx.MoveTo(x, y + h);

            if (_style == GraphRenderStyle.Fps)
            {
                for (int i = 0; i < GRAPH_HISTORY_COUNT; i++)
                {
                    float v = 1.0f / (0.00001f + _values[(_head + i) % GRAPH_HISTORY_COUNT]);

                    // this puts graph around middle
                    v = MathF.Min(v, 40.0f);

                    float vx = x + (float)i / (GRAPH_HISTORY_COUNT - 1) * w;
                    float vy = y + h - v / 80.0f * h;

                    ctx.LineTo(vx, vy);
                }
            }
            else if (_style == GraphRenderStyle.Percent)
            {
                for (uint i = 0; i < GRAPH_HISTORY_COUNT; i++)
                {
                    float v = _values[(_head + i) % GRAPH_HISTORY_COUNT] * 1.0f;
                    v = MathF.Min(v, 100.0f);
                    float vx = x + (float)i / (GRAPH_HISTORY_COUNT - 1) * w;
                    float vy = y + h - v / 100.0f * h;
                    ctx.LineTo(vx, vy);
                }
            }
            else if (_style == GraphRenderStyle.Ms)
            {
                for (uint i = 0; i < GRAPH_HISTORY_COUNT; i++)
                {
                    float v = _values[(_head + i) % GRAPH_HISTORY_COUNT] * 1000.0f;
                    v = MathF.Min(v, 20.0f);
                    float vx = x + (float)i / (GRAPH_HISTORY_COUNT - 1) * w;
                    float vy = y + h - v / 20.0f * h;
                    ctx.LineTo(vx, vy);
                }
            }

            ctx.LineTo(x + w, y + h);
            ctx.FillColor(_graphColor);
            ctx.Fill();

            Vector2 valueTopPos = new Vector2(x + w - PADDING, y + PADDING / 2);
            Vector2 valueBottomPos = new Vector2(x + w - PADDING, y + h - PADDING);

            ctx.FontFaceId(1);
            ctx.FontSize(_fontSize);
            ctx.TextColor(_textColor);
         
            if (_name != null)
            {
                ctx.TextAlign(TextAlignment.Left | TextAlignment.Top);
                ctx.Text(x + PADDING, valueTopPos.Y, _name);
            }
            
            if(_timeType != null)
            {
                // "Fixed"
                ctx.TextAlign(TextAlignment.Left | TextAlignment.Baseline);
                ctx.Text(x + PADDING, valueBottomPos.Y, _timeType);
            }

            if (_style == GraphRenderStyle.Fps)
            {
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
            else if (_style == GraphRenderStyle.Percent)
            {
                if (!string.IsNullOrEmpty(_percent))
                {
                    ctx.TextAlign(TextAlignment.Right | TextAlignment.Top);
                    ctx.Text(valueTopPos, _percent);
                }
            }
            else if (_style == GraphRenderStyle.Ms)
            {
                if (!string.IsNullOrEmpty(_milliseconds))
                {
                    ctx.TextAlign(TextAlignment.Right | TextAlignment.Top);
                    ctx.Text(valueTopPos, _milliseconds);
                }
            }
        }

        float GraphAverage
        {
            get
            {
                float average = 0;
                for (uint i = 0; i < GRAPH_HISTORY_COUNT; i++)
                {
                    average += _values[i];
                }
                return average / GRAPH_HISTORY_COUNT;
            }
        }
    }
}
