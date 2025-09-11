using NanoUI.Common;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUI.Components.Simple
{
    /// <summary>
    /// UIImage.
    /// </summary>
    public class UIImage : UIWidget
    {
        // calculated texture size
        Vector2 _imgSize;

        public UIImage(int texture)
        {
            Texture = texture;
        }

        public UIImage(UIWidget parent)
            : base(parent)
        {

        }

        #region Properties

        int _texture;
        public int Texture
        {
            get => _texture;
            set
            {
                _texture = value;

                if(_texture > Globals.INVALID)
                {
                    // calculate image size
                    NvgContext.Instance.GetTextureSize(Texture, out _imgSize);
                }
            }
        }
        public Color TintColor { get; set; }

        #endregion

        #region Drawing

        // todo: does this handle case when width != height
        /// <inheritdoc />
        public override void Draw(NvgContext ctx)
        {
            if (Texture <= Globals.INVALID)
                return;


            // note: Y is calculated dynamically from thumb size & image real size
            // todo: these calculations could be in PerformLayout?
            // todo2: make confugurable MathF.Min(thumbSize, Size.X)?
            float thumbSize = Size.X;

            float iw, ih, ix, iy;

            if (_imgSize.X < _imgSize.Y)
            {
                iw = thumbSize;
                ih = iw * _imgSize.Y / _imgSize.X;
                ix = 0;
                iy = -(ih - thumbSize) * 0.5f;
            }
            else
            {
                ih = thumbSize;
                iw = ih * _imgSize.X / _imgSize.Y;
                ix = -(iw - thumbSize) * 0.5f;
                iy = 0;
            }

            // draw texture
            Paint imgPaint = Paint.ImagePattern(
                Position.X + ix, Position.Y + iy, iw, ih, 0, Texture, TintColor);

            ctx.BeginPath();
            ctx.Rect(Position.X, Position.Y, thumbSize, thumbSize);
            ctx.FillPaint(imgPaint);
            ctx.Fill();
        }

        #endregion
    }
}
