using NanoUI.Common;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUI.Components
{
    /// <summary>
    /// UISplitter works by finding previous widget of this splitter ("left", "top") and
    /// setting its size fixed based on splitter position, then request layout change
    /// in parent.
    /// </summary>
    /// <remarks>Needs SplitLayout in parent to work.</remarks>
    public class UISplitter : UIWidget
    {
        /// <inheritdoc />
        public UISplitter()
        {
            // set defaults to theme impl - prevents circular reference
            Dimension = default;
            DotsRadius = default;
            DotsSpacing = default;
            DotsColor = default;

            // no border drawing - pointer type should change / pointer focus color?
            Border = false;
        }

        // must get orientation since we wnt to set fixed size & pointer type

        /// <inheritdoc />
        public UISplitter(UIWidget parent)
            :this(parent, Orientation.Horizontal)
        {
            // no border drawing - pointer type should change / pointer focus color?
            Border = false;
        }

        /// <inheritdoc />
        public UISplitter(UIWidget parent, Orientation dir = Orientation.Horizontal)
            : base(parent)
        {
            Orientation = dir;

            // note: no border drawing - pointer type should change
            Border = false;

            ThemeType = typeof(UISplitter);
        }

        #region Properties

        uint? _dimension;

        /// <summary>
        /// Dimension
        /// </summary>
        public uint Dimension
        {
            get => _dimension?? GetTheme().Splitter.Dimension;
            set => _dimension = value;
        }

        // todo: should we calculate this dynamically from dimension?

        float? _dotsRadius;

        /// <summary>
        /// DotsRadius
        /// </summary>
        public float DotsRadius
        {
            get => _dotsRadius?? GetTheme().Splitter.DotsRadius;
            set => _dotsRadius = value;
        }

        // todo: should we calculate this dynamically from dimension / dots radius?

        float? _dotsSpacing;

        /// <summary>
        /// Spacing between dots.
        /// </summary>
        public float DotsSpacing
        {
            get => _dotsSpacing ?? GetTheme().Splitter.DotsSpacing;
            set => _dotsSpacing = value;
        }

        Color? _dotsColor;

        /// <summary>
        /// Dots color
        /// </summary>
        public Color DotsColor
        {
            get => _dotsColor?? GetTheme().Splitter.DotsColor;
            set => _dotsColor = value;
        }

        Orientation _orientation;

        /// <summary>
        /// Orientation
        /// </summary>
        public Orientation Orientation
        {
            get => _orientation;
            set 
            {
                _orientation = value;

                if (_orientation == Orientation.Vertical)
                {
                    FixedSize = new Vector2(1, Dimension);
                }
                else
                {
                    FixedSize = new Vector2(Dimension, 1);
                }
            }
        }

        #endregion

        #region Events

        /// <inheritdoc />
        public override bool OnPointerUpDown(Vector2 p, PointerButton button, bool down)
        {
            if (!Disabled && down)
            {
                if (_orientation == Orientation.Vertical)
                {
                    SetPointerType((int)PointerType.SizeNS);
                }
                else
                {
                    SetPointerType((int)PointerType.SizeWE);
                }

                Screen?.SetDragWidget(this);

                return true;
            }

            return base.OnPointerUpDown(p, button, down);
        }

        /// <inheritdoc />
        public override bool OnPointerMove(Vector2 p, Vector2 rel)
        {
            // we must first call base, since it resets pointer type
            var res = base.OnPointerMove(p, rel);

            if (_orientation == Orientation.Vertical)
            {
                SetPointerType((int)PointerType.SizeNS);
            }
            else
            {
                SetPointerType((int)PointerType.SizeWE);
            }

            return res;
        }

        /// <inheritdoc />
        public override bool OnPointerDrag(Vector2 p, Vector2 rel)
        {
            if(Parent == null)
                return false;

            // Check pointer drag not outside parent area
            if (Orientation == Orientation.Vertical)
            {
                if (p.Y > Parent.Size.Y - Size.Y || p.Y < Size.Y)
                {
                    // do nothing but return handled to prevent other drag actions
                    return true;
                }
            }
            else if (Orientation == Orientation.Horizontal)
            {
                if (p.X > Parent.Size.X - Size.X || p.X < Size.X)
                {
                    // do nothing but return handled to prevent other drag actions
                    return true;
                }
            }

            // find this
            var childs = Parent.Children;

            // todo : normally this is 1 (2 widgets & splitter)
            int index = childs.IndexOf(this);

            // we resize previous widget

            // check we have previous widget in list
            if (index - 1 >= 0)
            {
                // previous widget - set fixed size increse/decrease
                UIWidget prevElem = childs[index - 1];
                Vector2 ws = prevElem.Size;

                // increase / decrease
                ws += (Orientation == Orientation.Vertical)
                         ? new Vector2(0, rel.Y)
                         : new Vector2(rel.X, 0);

                prevElem.Size = ws;
                prevElem.FixedSize = ws;

                // request layout update so next widget is resized
                RequestLayoutUpdate(Parent);
            }

            return true;
        }

        #endregion

        #region Drawing

        /// <inheritdoc />
        public override void Draw(NvgContext ctx)
        {
            // splitter background
            DrawBackgroundBrush(ctx);

            // note: not really needed - since there should not be any childs
            base.Draw(ctx);

            // "dots"            
            var dotsRadius = DotsRadius;

            if(dotsRadius > 0)
            {
                Vector2 center = Position + (Size / 2);

                // calculate spacing between dots
                Vector2 spacing = (Orientation == Orientation.Vertical) ?
                    new Vector2(DotsSpacing, 0) : 
                    new Vector2(0, DotsSpacing);

                // draw dots
                ctx.BeginPath();
                ctx.FillColor(DotsColor);
                ctx.Circle(center - spacing, dotsRadius);
                ctx.Circle(center, dotsRadius);
                ctx.Circle(center + spacing, dotsRadius);
                ctx.Fill();
            }

            // note: no border drawing
        }

        #endregion
    }
}
