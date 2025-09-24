using NanoUI.Common;
using NanoUI.Components;
using NanoUI.Nvg;
using System;
using System.Numerics;

namespace NanoUI.Layouts
{
    /// <summary>
    /// StackLayout arranges widgets either horizontally or vertically stacked.
    /// </summary>
    public class StackLayout : Layout
    {
        public StackLayout(Orientation orientation)
            : this(orientation, LayoutAlignment.Middle)
        {
            
        }

        public StackLayout(Orientation orientation, LayoutAlignment alignment)   
        {
            _orientation = orientation;
            _alignment = alignment;
        }

        #region Properties

        Orientation _orientation;

        /// <summary>
        /// Orientation
        /// </summary>
        public Orientation Orientation
        {
            get => _orientation;
            set => _orientation = value;
        }

        LayoutAlignment _alignment;

        /// <summary>
        /// Layout alignment
        /// </summary>
        public LayoutAlignment Alignment
        {
            get => _alignment;
            set => _alignment = value;
        }

        #endregion

        #region Layout

        /// <inheritdoc />
        public override Vector2 PreferredSize(NvgContext ctx, UIWidget parent)
        {
            var margin = parent.Margin;

            // start size
            Vector2 size = new Vector2(margin.Horizontal, margin.Vertical) * 2;

            bool first = true;
            int axis1 = (int)_orientation;
            int axis2 = ((int)_orientation + 1) % 2;

            foreach (var w in parent.Children.AsReadOnlySpan())
            {
                if (!w.Visible)
                    continue;

                if (first)
                    first = false;
                else
                {
                    // todo: both spacings? must be used also in PerformLayout
                    size[axis1] += _orientation == Orientation.Horizontal? Spacing.X: Spacing.Y;
                }

                // note: PreferredSize can be also calculated from child layouts,
                // so we check fixed size
                Vector2 ps = w.PreferredSize(ctx);
                Vector2 fs = w.FixedSize;

                Vector2 targetSize = new Vector2(
                    fs[0] > 0 ? fs[0] : ps[0],
                    fs[1] > 0 ? fs[1] : ps[1]
                );

                size[axis1] += targetSize[axis1];
                size[axis2] = Math.Max(size[axis2], targetSize[axis2] + 2 * margin[axis2]);
            }

            // collected children size
            return size;
        }

        /// <inheritdoc />
        public override void PerformLayout(NvgContext ctx, UIWidget parent)
        {
            var margin = parent.Margin;

            Rect layoutArea = parent.GetLayoutArea();

            Vector2 containerSize = layoutArea.Size;

            int axis1 = (int)_orientation;
            int axis2 = ((int)_orientation + 1) % 2;

            // start position in primary axis
            int position = (int)margin[axis1];

            Vector2 offset = layoutArea.Position;

            if (_orientation == Orientation.Vertical)
            {
                position += (int)layoutArea.Y;
            }
            else
            {
                position += (int)layoutArea.X;
            }

            bool first = true;

            foreach (var w in parent.Children.AsReadOnlySpan())
            {
                if (!w.Visible)
                    continue;

                if (first)
                    first = false;
                else
                {
                    // todo: handle both spacings?
                    position += _orientation == Orientation.Horizontal? (int)Spacing.X : (int)Spacing.Y;
                }

                // note: PreferredSize can be also calculated from child layouts,
                // so we check fixed size
                Vector2 ps = w.PreferredSize(ctx);
                Vector2 fs = w.FixedSize;

                Vector2 targetSize = new Vector2(
                    fs[0] > 0 ? fs[0] : ps[0],
                    fs[1] > 0 ? fs[1] : ps[1]
                );

                Vector2 pos = offset;
                
                pos[axis1] = position;

                switch (_alignment)
                {
                    case LayoutAlignment.Minimum:
                        pos[axis2] += margin[axis2];
                        break;
                    case LayoutAlignment.Middle:
                        pos[axis2] += (containerSize[axis2] - targetSize[axis2]) / 2;
                        break;
                    case LayoutAlignment.Maximum:
                        pos[axis2] += containerSize[axis2] - targetSize[axis2] - margin[axis2] * 2;
                        break;
                    case LayoutAlignment.Fill:
                        pos[axis2] += margin[axis2];
                        targetSize[axis2] = fs[axis2] > 0 ? fs[axis2] : containerSize[axis2] - margin[axis2] * 2;
                        break;
                }

                w.Position = pos;
                w.Size = targetSize;

                w.PerformLayout(ctx);

                position += (int)targetSize[axis1];
            }
        }

        #endregion
    }
}
