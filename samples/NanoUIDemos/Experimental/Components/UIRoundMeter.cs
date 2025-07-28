using NanoUI.Common;
using NanoUI.Components;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUIDemos.Experimental.Components
{
    // Round, analog meter
    // todo : DrawFlags or equivalent system to control what to draw
    // todo : there is lots of "magical" numbers
    // todo: there is lots of values that should be configurable
    // todo: check Threshold value is valid (between min & max) & other checks
    public class UIRoundMeter : UIWidget
    {
        // todo: configurable
        const float StartAngle = 320f;
        const float EndAngle = 40f;        

        Vector2 _unitsRealSize;
        Vector2 _valueTextRealSize;
        Vector2 _captionRealSize;

        // this is current value rounded with ValueTolerance
        float _value;
        string? _valueText;

        bool _isOverThreshold = false;

        bool _needUpdateValueText = true;
        bool _needUpdateCaption;
        bool _needUpdateUnitsText;

        // note : caption gets normal font size
        // todo : should these be configurable?
        float _valueFontSize => FontSize + 3;
        float _unitsFontSize => FontSize - 2;
        float _scaleFontSize => FontSize - 3;

        // note : this sends rounded value.
        public Action<float>? ValueChanged;
        public Action<bool>? OverThreshold;

        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        public UIRoundMeter()
        {
            // set defaults to theme impl - prevents circular reference
            MeterBackgroundColor = default;
            CrownColor = default;
            CrownColor2 = default;
            OverThresholdColor = default;
            UnderThresholdColor = default;
            NeedleColor = default;
            NeedleColor2 = default;
            NeedleHatColor = default;
            NeedleHatColor2 = default;
        }

        public UIRoundMeter(UIWidget parent)
            : base(parent)
        {
            // todo: in theme
            IgnoreDrawingThreshold = 50;
            ValueTolerance = 1.0f;
            Steps = 10;
            Threshold = 120;
            ThresholdEnabled = true;
            NumericIndicatorEnabled = true;

            _minValue = 0;
            _maxValue = 100;

            ThemeType = typeof(UILabel);
        }

        #region Properties

        // this is value when size goes below this, we ignore drawing some parts
        // mainly text-related. so this set based on font size & texts
        public float IgnoreDrawingThreshold { get; set; }

        // this affects how often we update value display and send ValueChanged event
        public float ValueTolerance { get; set; }

        float _currentValue;
        public float CurrentValue
        {
            get => _currentValue;
            set
            {
                _currentValue = Math.Clamp(value, MinValue, MaxValue);

                if (Math.Abs(_currentValue - _value) > ValueTolerance)
                {
                    _value = RoundCurrentValue();
                    _needUpdateValueText = true;

                    ValueChanged?.Invoke(_value);

                    CheckThreshold();
                }
            }
        }

        // this is rounded value
        public float Value => _value;

        // todo : range
        float _minValue;
        public float MinValue
        {
            get => _minValue;
            set => _minValue = value;
        }

        float _maxValue;
        public float MaxValue
        {
            get => _maxValue;
            set
            {
                if (value > _minValue)
                {
                    _maxValue = value;
                }
            }
        }

        string? _unitsText;
        public string? UnitsText
        {
            get => _unitsText;
            set
            {
                _unitsText = value;
                _needUpdateUnitsText = true;
            }
        }

        string? _caption;
        public string? Caption
        {
            get => _caption;
            set
            {
                _caption = value;
                _needUpdateCaption = true;
            }
        }

        public int Steps { get; set; }
        public float Threshold { get; set; }
        public bool ThresholdEnabled { get; set; }
        public bool NumericIndicatorEnabled { get; set; }

        #region Colors

        Color? _meterBackgroundColor;
        public Color MeterBackgroundColor
        {
            get => _meterBackgroundColor ?? ((ThemeEXT)GetTheme()).RoundMeter.MeterBackgroundColor;
            set => _meterBackgroundColor = value;
        }

        Color? _crownColor;
        public Color CrownColor
        {
            get => _crownColor ?? ((ThemeEXT)GetTheme()).RoundMeter.CrownColor;
            set=> _crownColor = value;
        }

        Color? _crownColor2;
        public Color CrownColor2
        {
            get => _crownColor2 ?? ((ThemeEXT)GetTheme()).RoundMeter.CrownColor2;
            set => _crownColor2 = value;
        }

        Color? _overThresholdColor;
        public Color OverThresholdColor
        {
            get => _overThresholdColor ?? ((ThemeEXT)GetTheme()).RoundMeter.OverThresholdColor;
            set => _overThresholdColor = value;
        }

        Color? _underThresholdColor;
        public Color UnderThresholdColor
        {
            get => _underThresholdColor ?? ((ThemeEXT)GetTheme()).RoundMeter.UnderThresholdColor;
            set => _underThresholdColor = value;
        }

        Color? _needleColor;
        public Color NeedleColor
        {
            get => _needleColor ?? ((ThemeEXT)GetTheme()).RoundMeter.NeedleColor;
            set => _needleColor = value;
        }

        Color? _needleColor2;
        public Color NeedleColor2
        {
            get => _needleColor2 ?? ((ThemeEXT)GetTheme()).RoundMeter.NeedleColor2;
            set => _needleColor2 = value;
        }

        Color? _needleHatColor;
        public Color NeedleHatColor
        {
            get => _needleHatColor ?? ((ThemeEXT)GetTheme()).RoundMeter.NeedleHatColor;
            set => _needleHatColor = value;
        }

        Color? _needleHatColor2;
        public Color NeedleHatColor2
        {
            get => _needleHatColor2 ?? ((ThemeEXT)GetTheme()).RoundMeter.NeedleHatColor2;
            set => _needleHatColor2 = value;
        }

        #endregion

        #endregion

        #region Methods

        // user can extend widget and provide override to this method (how value is represented)
        // default implementation converts rounded value as-is to string
        protected virtual string ValueToString()
        {
            // todo: ReadonlySpan?
            return _value.ToString();
        }

        #endregion

        #region Layout

        // note : there could be problems in drawing parts when size is too small, so we ignore
        // some drawing them (there is also posiiblity that we calculate dynamically font sizes)
        bool _ignoreDrawing = false;

        float _meterRadius;
        Vector2 _meterSize;
        Vector2 _center;
        
        public override void PerformLayout(NvgContext ctx)
        {
            base.PerformLayout(ctx);

            // calculate center, radius etc so meter fits inside widget
            // todo: there could be some minimum size
            float min = MathF.Min(Size.X, Size.Y);

            // used when calculating "outer" parts (background, crown, glass)
            _meterSize = new Vector2(min);

            // note : this is not 0.5f, since we want to have little padding when displying meter inner graphs
            _meterRadius = min * 0.465f;
            
            _center = Position + Size * 0.5f;

            // some parts are not drawn when true
            _ignoreDrawing = _meterRadius < IgnoreDrawingThreshold;

            if (_ignoreDrawing)
            {
                _meterRadius = min * 0.55f;
            }
        }

        #endregion

        #region Drawing

        public override void Draw(NvgContext ctx)
        {
            // in base
            //this.DrawBackground(ctx);
            DrawBackgroundBrush(ctx);

            DrawMeterBackground(ctx);

            DrawTicks(ctx);
            DrawThresholdLine(ctx);

            // if size is too small we can't draw parts below
            if (!_ignoreDrawing)
            {
                DrawScale(ctx);
                DrawUnits(ctx);              
                DrawNumericValue(ctx);                
                DrawCaption(ctx);
            }            

            DrawNeedle(ctx);

            // outer "metallic border"
            DrawCrown(ctx);
        }       

        // Private draw functions

        // outer "border"
        void DrawCrown(NvgContext ctx)
        {
            // todo: static, in MathUtils?
            float startAngle = float.DegreesToRadians(0);
            float spanAngle = float.DegreesToRadians(360);

            float crownRadius = _meterSize.X / 2;

            Vector2 pos = _center - _meterSize / 2;

            var paint = Paint.LinearGradient(
                pos,
                pos + _meterSize,
                CrownColor, CrownColor2);
            
            ctx.BeginPath();
            ctx.Arc(_center, crownRadius, startAngle, spanAngle, Winding.Clockwise);
            ctx.Arc(_center, crownRadius * 0.94f, spanAngle, startAngle, Winding.CounterClockwise);

            ctx.ClosePath();

            ctx.FillPaint(paint);
            ctx.Fill();
        }

        void DrawMeterBackground(NvgContext ctx)
        {
            if (MeterBackgroundColor.A > 0)
            {
                ctx.BeginPath();
                ctx.FillColor(MeterBackgroundColor);
                ctx.Circle(_center, _meterSize.X / 2);
                ctx.Fill();
            }
        }

        void DrawTicks(NvgContext ctx)
        {
            float angleStep = (EndAngle - StartAngle) / Steps;

            // todo: magical numbers
            float radius1 = _meterRadius * 0.6f;
            float radius2 = _meterRadius * 0.7f;

            ctx.BeginPath();

            for (int i = 0; i <= Steps; i++)
            {
                float ax = MathF.Sin(float.DegreesToRadians(StartAngle + angleStep * i)) * radius1;
                float ay = MathF.Cos(float.DegreesToRadians(StartAngle + angleStep * i)) * radius1;
                float bx = MathF.Sin(float.DegreesToRadians(StartAngle + angleStep * i)) * radius2;
                float by = MathF.Cos(float.DegreesToRadians(StartAngle + angleStep * i)) * radius2;

                ctx.MoveTo(ax + _center.X, ay + _center.Y);
                ctx.LineTo(bx + _center.X, by + _center.Y);
            }

            ctx.StrokeColor(TextColor);
            // todo: configurable?
            ctx.StrokeWidth(2);
            ctx.Stroke();

            ctx.BeginPath();
            ctx.Arc(
                _center,
                radius1,
                float.DegreesToRadians(EndAngle + 90),
                float.DegreesToRadians(StartAngle + 90),
                Winding.Clockwise);
            ctx.Stroke();
        }
        
        void DrawScale(NvgContext ctx)
        {
            float angleStep = (EndAngle - StartAngle) / Steps;
            float radius = _meterRadius * 0.82f;

            ctx.FillColor(TextColor);
            ctx.FontSize(_scaleFontSize);
            ctx.FontFaceId(FontFaceId);

            ctx.TextAlign(TextAlignment.Left | TextAlignment.Top);

            for (int i = 0; i <= Steps; i++)
            {
                float ax = MathF.Sin(float.DegreesToRadians(StartAngle + angleStep * i)) * radius;
                float ay = MathF.Cos(float.DegreesToRadians(StartAngle + angleStep * i)) * radius;

                float tmpVal = i * ((MaxValue - MinValue) / Steps);

                tmpVal += MinValue;
                string str = ((int)tmpVal).ToString();

                float w = ctx.TextBounds(0, 0, str, out _);

                ctx.Text(_center.X + ax - w / 2, _center.Y + ay - _scaleFontSize / 2, str);
            }
        }

        // todo: magical numbers
        void DrawUnits(NvgContext ctx)
        {
            ctx.FontSize(_unitsFontSize);
            ctx.FontFaceId(FontFaceId);

            if (_needUpdateUnitsText && !string.IsNullOrEmpty(UnitsText))
            {
                ctx.TextAlign(TextAlignment.Left | TextAlignment.Top);
                float tw = ctx.TextBounds(0, 0, UnitsText, out _);

                _unitsRealSize = new Vector2(tw, _unitsFontSize);
                _needUpdateUnitsText = false;
            }

            ctx.TextAlign(TextAlignment.Left | TextAlignment.Top);
            ctx.FillColor(TextColor);
            // todo : draws on top of needle hat (so 0.26f * _meterRadius)
            ctx.Text(_center.X - _unitsRealSize.X / 2, _center.Y - _unitsRealSize.Y / 2 - 0.26f * _meterRadius, UnitsText);
            
        }

        // todo: magical numbers
        void DrawNeedle(NvgContext ctx)
        {
            float degRotate = (StartAngle + (EndAngle - StartAngle) / (MaxValue - MinValue) * (_currentValue - MinValue));

            // this is base value for needle width
            float w = _meterRadius * 2;

            Paint haloGradient = Paint.RadialGradient(
                _center.X,
                _center.Y, 
                0, 
                0.3f * w,
                NeedleColor, NeedleColor2);

            ctx.BeginPath();
            ctx.MoveTo(
                _center.X + SinDegrees2Radians(degRotate + 90, 0.02f * w),
                _center.Y + CosDegrees2Radians(degRotate + 90, 0.02f * w));
            ctx.LineTo(
                _center.X + SinDegrees2Radians(degRotate + 279, 0.02f * w),
                _center.Y + CosDegrees2Radians(degRotate + 270, 0.02f * w));
            ctx.LineTo(
                _center.X + SinDegrees2Radians(degRotate, 0.4f * w),
                _center.Y + CosDegrees2Radians(degRotate, 0.4f * w));
            ctx.ClosePath();

            ctx.FillPaint(haloGradient);
            ctx.Fill();

            // needle hat (todo: should calculate rect, because values are used to position texts)
            var linearGrad = Paint.LinearGradient(
                _center.X - 0.07f * w,
                _center.Y - 0.07f * w,
                _center.X + 0.14f * w,
                _center.Y + 0.14f * w,
                NeedleHatColor, NeedleHatColor2);

            ctx.BeginPath();
            ctx.FillPaint(linearGrad);
            ctx.Circle(_center, 0.07f * w);
            ctx.Fill();
        }

        // todo: magical numbers
        void DrawThresholdLine(NvgContext ctx)
        {
            if (!ThresholdEnabled)
                return;

            float thresholdAngle = (StartAngle - EndAngle) / (MaxValue - MinValue) * (Threshold - MinValue);

            float radius = _meterRadius * 0.48f;

            ctx.BeginPath();
            // todo: configurable?
            ctx.StrokeWidth(5);
            ctx.StrokeColor(UnderThresholdColor);
            ctx.Arc(_center.X, _center.Y, radius, 
                float.DegreesToRadians(EndAngle + 90),
                float.DegreesToRadians(EndAngle + thresholdAngle + 90),
                Winding.Clockwise);
            ctx.Stroke();

            ctx.BeginPath();
            ctx.StrokeColor(OverThresholdColor);
            ctx.Arc(_center.X, _center.Y, radius,
                float.DegreesToRadians(EndAngle + thresholdAngle + 90),
                float.DegreesToRadians(StartAngle + 90),
                Winding.Clockwise);
            ctx.Stroke();
        }

        // todo: magical numbers
        void DrawNumericValue(NvgContext ctx)
        {
            if (!NumericIndicatorEnabled)
                return;

            // set font size
            ctx.FontSize(_valueFontSize);

            if (_needUpdateValueText)
            {
                // note: this supports user override, how value is displayed
                _valueText = ValueToString();

                float tw = ctx.TextBounds(0, 0, _valueText, out _);

                _valueTextRealSize = new Vector2(tw, _valueFontSize);
                _needUpdateValueText = false;
            }

            //ctx.FontFaceId(ValueFontFaceId);
            ctx.FontFaceId(FontFaceId);

            if (ThresholdEnabled)
            {
                ctx.FillColor(_isOverThreshold ? OverThresholdColor : UnderThresholdColor);
            }
            else
            {
                ctx.FillColor(TextColor);
            }

            if (!string.IsNullOrEmpty(_valueText))
            {
                ctx.TextAlign(TextAlignment.Left | TextAlignment.Top);
                // goes below center Y
                ctx.Text(
                    _center.X - _valueTextRealSize.X / 2,
                    _center.Y - _valueTextRealSize.Y / 2 + 0.4f * _meterRadius,
                    _valueText);
            }
        }

        void DrawCaption(NvgContext ctx)
        {
            ctx.FontSize(FontSize);
            ctx.FontFaceId(FontFaceId);

            if (_needUpdateCaption && !string.IsNullOrEmpty(Caption))
            {
                ctx.TextAlign(TextAlignment.Left | TextAlignment.Top);
                float tw = ctx.TextBounds(0, 0, Caption, out _);

                _captionRealSize = new Vector2(tw, FontSize);
                _needUpdateCaption = false;
            }

            ctx.TextAlign(TextAlignment.Left | TextAlignment.Top);
            ctx.FillColor(TextColor);
            ctx.Text(
                _center.X - _captionRealSize.X / 2,
                _center.Y - _captionRealSize.Y / 2 + 0.7f * _meterRadius,
                Caption);
        }

        #endregion

        #region Private

        void CheckThreshold()
        {
            if (!ThresholdEnabled)
                return;

            // we invoke event only when threshold status changes
            if (_currentValue > Threshold && !_isOverThreshold)
            {
                _isOverThreshold = true;

                OverThreshold?.Invoke(_isOverThreshold);
            }
            else if (_currentValue < Threshold && _isOverThreshold)
            {
                _isOverThreshold = false;
                OverThreshold?.Invoke(_isOverThreshold);
            }
        }

        // round value based on ValueTolerance
        float RoundCurrentValue()
        {
            return MathF.Round(_currentValue / ValueTolerance) * ValueTolerance;
        }

        static float SinDegrees2Radians(float angle, float r)
        {
            return MathF.Sin(float.DegreesToRadians(angle)) * r;
        }

        static float CosDegrees2Radians(float angle, float r)
        {
            return MathF.Cos(float.DegreesToRadians(angle)) * r;
        }

        #endregion
    }
}
