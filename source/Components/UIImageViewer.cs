using NanoUI.Common;
using NanoUI.Nvg;
using NanoUI.Utils;
using System;
using System.Numerics;

namespace NanoUI.Components
{
    // todo : there is alternate way (set texture inside scroll panel)

    /// <summary>
    /// UIImageViewer.
    /// </summary>
    public class UIImageViewer : UIWidget
    {
        Vector2 _textureSize;

        // texture changed must update params
        bool _needUpdate = false;

        /// <inheritdoc />
        public UIImageViewer()
        {
            // set defaults to theme impl - prevents circular reference
            ZoomSensitivity = default;
            _texture = 0;
        }

        /// <inheritdoc />
        public UIImageViewer(UIWidget parent)
            : base(parent)
        {
            Scale = 1.0f;
            ThemeType = typeof(UIWidget);
        }

        #region Properties

        public Color TintColor { get; set; }
        public float Scale { get; set; }
        public bool FixedScale { get; set; }
       
        Vector2 _offset;
        public Vector2 Offset
        {
            get => _offset;
            set => _offset = value;
        }

        public bool FixedOffset { get; set; }
        
        float? _zoomSensitivity;
        public float ZoomSensitivity
        {
            get => _zoomSensitivity?? GetTheme().ImageViewer.ZoomSensitivity;
            set => _zoomSensitivity = value;
        }

        int? _texture;
        public int Texture
        {
            get => _texture?? Globals.INVALID;
            set
            {
                _texture = value;
                
                if(_texture != null && _texture >= 0)
                {
                    // we need update texture params
                    _needUpdate = true;
                }
            }
        }
        
        public Vector2 TextureSize => _textureSize;
        public Vector2 ScaledTextureSize => Scale * _textureSize;

        #endregion

        #region Methods

        // Calculates the texture coordinates of the given pixel position on the widget.
        public Vector2 TextureCoordinateAt(Vector2 position)
        {
            return (position - Offset) / Scale;
        }

        public Vector2 ClampedTextureCoordinateAt(Vector2 position)
        {
            var textureCoordinate = TextureCoordinateAt(position);

            return Vector2.Clamp(textureCoordinate, Vector2.Zero, _textureSize);
        }

        // Calculates the position inside the widget for the given texture coordinate.
        public Vector2 PositionForCoordinate(Vector2 textureCoordinate)
        {
            return Scale * textureCoordinate + Offset;
        }

        public void SetTextureCoordinateAt(Vector2 position, Vector2 textureCoordinate)
        {
            // Calculate where the new offset must be in order to satisfy the texture position equation.
            // Round the floating point values to balance out the floating point to integer conversions.
            Offset = position - textureCoordinate * Scale;

            // Clamp offset so that the texture remains near the screen.
            Offset = Vector2.Max(Vector2.Min(Offset, Size), -ScaledTextureSize);
        }

        // Centers the texture without affecting the scaling factor.
        public void Center()
        {
            Offset = (Size - ScaledTextureSize) / 2;
        }

        // Centers and scales the texture so that it fits inside the widgets.
        public void Fit()
        {
            // Calculate the appropriate scaling factor.
            Scale = Size.Quotient(_textureSize).MinCoefficient();

            Center();
        }

        // Set the scale while keeping the texture centered
        public void SetScaleCentered(float scale)
        {
            var centerPosition = Size / 2;
            var p = TextureCoordinateAt(centerPosition);
            Scale = scale;

            SetTextureCoordinateAt(centerPosition, p);
        }

        // Moves the offset by the specified amount. Does bound checking.
        public void MoveOffset(Vector2 delta)
        {
            // Apply the delta to the offset.
            Offset += delta;

            // Prevent the texture from going out of bounds.
            var scaledSize = ScaledTextureSize;

            if (Offset.X + scaledSize.X < 0)
                _offset.X = -scaledSize.X;

            if (Offset.X > Size.X)
                _offset.X = Size.X;

            if (Offset.Y + scaledSize.Y < 0)
                _offset.Y = -scaledSize.Y;

            if (Offset.Y > Size.Y)
                _offset.Y = Size.Y;
        }

        public void Zoom(int amount, Vector2 focusPosition)
        {
            var focusedCoordinate = TextureCoordinateAt(focusPosition);
            float scaleFactor = MathF.Pow(ZoomSensitivity, amount);
            Scale = Math.Max(0.01f, scaleFactor * Scale);

            SetTextureCoordinateAt(focusPosition, focusedCoordinate);
        }

        #endregion

        #region Events

        /// <inheritdoc />
        public override bool OnKeyUpDown(Key key, bool down, KeyModifiers modifiers)
        {
            if (!Focused)
                return false;

            if (down)
            {
                // todo: magical numbers
                switch (key)
                {
                    case Key.Left:
                        if (!FixedOffset)
                        {
                            if (modifiers == KeyModifiers.Control)
                                MoveOffset(new Vector2(30, 0));
                            else
                                MoveOffset(new Vector2(10, 0));

                            return true;
                        }
                        break;
                    case Key.Right:
                        if (!FixedOffset)
                        {
                            if (modifiers == KeyModifiers.Control)
                                MoveOffset(new Vector2(-30, 0));
                            else
                                MoveOffset(new Vector2(-10, 0));
                            return true;
                        }
                        break;
                    case Key.Down:
                        if (!FixedOffset)
                        {
                            if (modifiers == KeyModifiers.Control)
                                MoveOffset(new Vector2(0, -30));
                            else
                                MoveOffset(new Vector2(0, -10));
                            return true;
                        }
                        break;
                    case Key.Up:
                        if (!FixedOffset)
                        {
                            if (modifiers == KeyModifiers.Control)
                                MoveOffset(new Vector2(0, 30));
                            else
                                MoveOffset(new Vector2(0, 10));
                            return true;
                        }
                        break;
                }
            }

            return base.OnKeyUpDown(key, down, modifiers);
        }

        /// <inheritdoc />
        public override bool OnKeyChar(char c)
        {
            switch (c)
            {
                case '-':
                    if (!FixedScale)
                    {
                        // zoom out
                        Zoom(-1, Size / 2);
                        return true;
                    }
                    break;
                case '+':
                    if (!FixedScale)
                    {
                        // zoom in
                        Zoom(1, Size / 2);
                        return true;
                    }
                    break;
                case 'c':
                    if (!FixedOffset)
                    {
                        Center();
                        return true;
                    }
                    break;
                case 'f':
                    if (!FixedOffset && !FixedScale)
                    {
                        Fit();
                        return true;
                    }
                    break;
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    if (!FixedScale)
                    {
                        SetScaleCentered(1 << c - '1');
                        return true;
                    }
                    break;
                default:
                    return false;
            }
            return false;
        }

        /// <inheritdoc />
        public override bool OnPointerUpDown(Vector2 p, PointerButton button, bool down)
        {
            if (down)
            {
                // set drag widget
                Screen?.SetDragWidget(this);
            }

            return base.OnPointerUpDown(p, button, down);
        }

        /// <inheritdoc />
        public override bool OnPointerDrag(Vector2 p, Vector2 rel)
        {
            if (!FixedOffset)
            {
                SetTextureCoordinateAt(p + rel, TextureCoordinateAt(p));
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public override bool OnPointerScroll(Vector2 p, Vector2 scroll)
        {
            if (FixedScale)
                return false;

            float v = scroll.Y;

            if (Math.Abs(v) < 1)
                v = MathF.CopySign(1.0f, v);

            Zoom((int)v, p - Position);

            return true;
        }

        #endregion

        #region Layout

        /// <inheritdoc />
        public override Vector2 PreferredSize(NvgContext ctx)
        {
            if (Texture != Globals.INVALID)
            {
                ctx.GetTextureSize(Texture, out Vector2 size);
                return Vector2.Max(MinSize, size);
            }

            return Vector2.Max(MinSize, _textureSize);
        }

        /// <inheritdoc />
        public override void PerformLayout(NvgContext ctx)
        {
            base.PerformLayout(ctx);
            Center();
        }

        #endregion

        #region Drawing

        /// <inheritdoc />
        public override void Draw(NvgContext ctx)
        {
            if (_needUpdate)
            {
                _needUpdate = false;

                if(Texture == Globals.INVALID)
                {
                    // set defaults
                    _textureSize = Vector2.Zero;
                    Scale = 1;
                }
                else
                {
                    ctx.GetTextureSize(Texture, out _textureSize);
                    Fit();
                }
            }

            // background
            DrawBackgroundBrush(ctx);

            base.Draw(ctx);

            // draw texture?
            if (Texture > Globals.INVALID)
            {
                ctx.SaveState();

                // Prevent overflow
                ctx.IntersectScissor(Position, Size);

                Vector2 texturePosition = Position + Offset;

                Paint imgPaint = Paint.ImagePattern(texturePosition,
                     TextureSize * Scale, 0, Texture, TintColor);

                ctx.BeginPath();
                ctx.Rect(texturePosition, TextureSize * Scale);
                ctx.FillPaint(imgPaint);
                ctx.Fill();

                ctx.RestoreState();
            }

            // draw widget borders
            this.DrawBorder(ctx, true);
        }

        #endregion
    }
}
