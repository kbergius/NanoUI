using NanoUI.Common;
using System;

namespace NanoUI.Svg.Data
{
    // note: we set all fields nullable so we know if field value is set
    // (if null, we could try to get value from parent element style - combine)
    public struct SvgStyle
    {
        public float? Opacity;
        public Color? FillColor;
        public float? FillOpacity;
        public Color? StrokeColor;
        public float? StrokeWidth;
        public LineCap? StrokeLineCap;
        public LineCap? StrokeLineJoin;
        public float? StrokeOpacity;
        public float? StrokeMiterLimit;
        // todo: we don't have drawing function for these bynow
        public float[]? StrokeDashArray;
        public float? StrokeDashOffset;

        // support for gradients
        public Paint? FillPaint;
        public string? FillPaintId;
        public Paint? StrokePaint;
        public string? StrokePaintId;

        // elements can have many style attributes (we convert individual 'fill', 'stroke' etc to style)
        // if current style doesn't have value set, we set value (can be null)
        // note: we use first encountered valid style value (that is not null)
        public SvgStyle Combine(SvgStyle? current)
        {
            if(current == null)
            {
                return this;
            }

            var result = current.Value;

            if (result.Opacity == null && Opacity != null)
            {
                result.Opacity = Opacity;
            }

            if (result.FillColor == null && FillColor != null)
            {
                result.FillColor = FillColor;
            }

            if (result.FillOpacity == null && FillOpacity != null)
            {
                result.FillOpacity = FillOpacity;
            }

            if (result.StrokeColor == null && StrokeColor != null)
            {
                result.StrokeColor = StrokeColor;
            }

            if (result.StrokeWidth == null && StrokeWidth != null)
            {
                result.StrokeWidth = StrokeWidth;
            }

            if (result.StrokeLineCap == null && StrokeLineCap != null)
            {
                result.StrokeLineCap = StrokeLineCap;
            }

            if (result.StrokeLineJoin == null && StrokeLineJoin != null)
            {
                result.StrokeLineJoin = StrokeLineJoin;
            }

            if (result.StrokeOpacity == null && StrokeOpacity != null)
            {
                result.StrokeOpacity = StrokeOpacity;
            }

            if (result.StrokeMiterLimit == null && StrokeMiterLimit != null)
            {
                result.StrokeMiterLimit = StrokeMiterLimit;
            }

            if (result.StrokeDashArray == null && StrokeDashArray != null)
            {
                result.StrokeDashArray = StrokeDashArray;
            }

            if (result.StrokeDashOffset == null && StrokeDashOffset != null)
            {
                result.StrokeDashOffset = StrokeDashOffset;
            }

            // fill gradient
            if (result.FillPaint == null && FillPaint != null)
            {
                result.FillPaint = FillPaint;
            }

            if (result.FillPaintId == null && FillPaintId != null)
            {
                result.FillPaintId = FillPaintId;
            }

            // stroke gradient
            if (result.StrokePaint == null && StrokePaint != null)
            {
                result.StrokePaint = StrokePaint;
            }

            if (result.StrokePaintId == null && StrokePaintId != null)
            {
                result.StrokePaintId = StrokePaintId;
            }

            return result;
        }

        // set opacities to colors if opacities & colors defined
        public void SetOpacities()
        {
            if(FillColor != null)
            {
                float? fillOpacity = FillOpacity != null ? FillOpacity : Opacity != null ? Opacity : null;

                if (fillOpacity != null)
                {
                    var op = Math.Clamp(fillOpacity.Value, 0, 1);

                    var col = FillColor.Value;
                    col.A = (byte)(op * 255);
                    FillColor = col;
                }
            }

            if (StrokeColor != null)
            {
                float? strokeOpacity = StrokeOpacity != null ? StrokeOpacity : Opacity != null ? Opacity : null;

                if (strokeOpacity != null)
                {
                    var op = Math.Clamp(strokeOpacity.Value, 0, 1);

                    var col = StrokeColor.Value;
                    col.A = (byte)(op * 255);
                    StrokeColor = col;
                }
            }
        }
    }
}