using NanoUI.Common;
using Color = NanoUI.Common.Color;
using NanoUI.Nvg;
using System;
using System.Numerics;

namespace NanoUI.Components.Colors
{
    /// <summary>
    /// UIAlphaBar.
    /// </summary>
    public class UIAlphaBar : UIWidget
    {
        const float MARKER_HEIGHT = 5;
        const float MARKER_STROKE_WIDTH = 2;

        // marker pos (y position in relation to size y)
        float _markerY = 0;

        public Action<byte>? AlphaChanged;

        public UIAlphaBar(UIWidget parent)
            : base(parent)
        {
            BackgroundFocused = BackgroundUnfocused = new LinearGradient(Color.Black, Color.White,
                new CornerRadius(0));
        }

        #region Properties

        public Color MarkerColor { get; set; } = Color.White;


        byte _alphaValue;

        public byte AlphaValue
        {
            get => _alphaValue;
            set
            {
                _alphaValue = value;

                // calculate marker position
                float val = (float)value / 255;

                _markerY = (Size.Y - MARKER_HEIGHT) * val;

                AlphaChanged?.Invoke(_alphaValue);
            }
        }

        #endregion

        #region Events

        /// <inheritdoc />
        public override bool OnPointerUpDown(Vector2 p, PointerButton button, bool down)
        {
            if (down)
            {
                SetAlphaValue(p);

                // set drag widget
                Screen?.SetDragWidget(this);
            }

            return true;
        }

        /// <inheritdoc />
        public override bool OnPointerDrag(Vector2 p, Vector2 rel)
        {
            SetAlphaValue(p);

            return true;
        }

        #endregion

        #region Drawing

        /// <inheritdoc />
        public override void Draw(NvgContext ctx)
        {
            // background
            DrawBackgroundBrush(ctx);

            // Marker
            // we must reset scissor for marker, since we are drawing outside widget bounds
            ctx.SaveState();

            ctx.ResetScissor();

            ctx.BeginPath();
            ctx.Rect(
                new Vector2(Position.X - MARKER_STROKE_WIDTH, Position.Y + _markerY),
                new Vector2(Size.X + 2 * MARKER_STROKE_WIDTH, MARKER_HEIGHT));

            ctx.StrokeColor(MarkerColor);
            ctx.StrokeWidth(MARKER_STROKE_WIDTH);
            ctx.Stroke();

            ctx.RestoreState();
        }

        #endregion

        #region Private

        void SetAlphaValue(Vector2 p)
        {
            // get the value
            float realSize = Size.Y - MARKER_HEIGHT;
            float value = Math.Clamp((p - Position).Y, 0, realSize) / realSize;

            AlphaValue = (byte)(255 * value);
        }

        #endregion
    }
}
