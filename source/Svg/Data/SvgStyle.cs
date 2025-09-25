using NanoUI.Common;
using System;

namespace NanoUI.Svg.Data
{
    // note: we set all fields nullable so we know if field value is set
    // (if null, we could try to get value from parent element style - combine)

    /// <summary>
    /// SvgStyle.
    /// Note: StrokeDashArray & StrokeDashOffset are not implemented.
    /// </summary>
    public struct SvgStyle
    {
        /// <summary>
        /// Opacity.
        /// </summary>
        public float? Opacity;

        /// <summary>
        /// Fill color
        /// </summary>
        public Color? FillColor;

        /// <summary>
        /// Fill opacity.
        /// </summary>
        public float? FillOpacity;

        /// <summary>
        /// Stroke color
        /// </summary>
        public Color? StrokeColor;

        /// <summary>
        /// Stroke width
        /// </summary>
        public float? StrokeWidth;

        /// <summary>
        /// Stroke line sap
        /// </summary>
        public LineCap? StrokeLineCap;

        /// <summary>
        /// Stroke line join
        /// </summary>
        public LineCap? StrokeLineJoin;

        /// <summary>
        /// Stroke opacity
        /// </summary>
        public float? StrokeOpacity;

        /// <summary>
        /// Stroke miter limit
        /// </summary>
        public float? StrokeMiterLimit;
        
        /// <summary>
        /// Stroke dash array (not implemented by now).
        /// </summary>
        public float[]? StrokeDashArray;

        /// <summary>
        /// Stroke dash offset (not implemented by now).
        /// </summary>
        public float? StrokeDashOffset;

        /// <summary>
        /// Fill paint
        /// </summary>
        public Paint? FillPaint;

        /// <summary>
        /// Fill paint id
        /// </summary>
        public string? FillPaintId;

        /// <summary>
        /// Stroke paint
        /// </summary>
        public Paint? StrokePaint;

        /// <summary>
        /// Stroke paint id
        /// </summary>
        public string? StrokePaintId;

        /// <summary>
        /// Combines this to given SvgStyle.
        /// </summary>
        /// <param name="current">SvgStyle</param>
        /// <returns>Combined SvgStyle</returns>
        /// <remarks>
        /// Elements can have many style attributes (we convert individual 'fill', 'stroke' etc to style).
        /// If current style doesn't have value set, we set value (can be null).
        /// We use first encountered valid style value (that is not null).
        /// </remarks>
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

        /// <summary>
        /// Sets opacities to colors, if fill and stroke opacities & colors defined.
        /// </summary>
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
