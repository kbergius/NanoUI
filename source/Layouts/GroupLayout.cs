using NanoUI.Common;
using NanoUI.Components;
using NanoUI.Nvg;
using System;
using System.Numerics;

namespace NanoUI.Layouts
{
    /// <summary>
    /// GroupLayout arranges widgets in stack layout vertical manner.
    /// If there is UILabel it will be positioned at left and other widgets
    /// are indented.
    /// </summary>
    public class GroupLayout : Layout
    {
        // The spacing between groups
        int _groupSpacing = 14;

        // The indent amount of a group under its defining Label (normally)
        int _groupIndent = 20;

        public GroupLayout()
        {
            Spacing = new Vector2(6);
        }

        #region Properties

        /// <summary>
        /// The indent of widgets in a group (underneath a UILabel - normally).
        /// </summary>
        public virtual int GroupIndent
        {
            get => _groupIndent;
            set => _groupIndent = value;
        }

        /// <summary>
        /// Spacing between groups.
        /// </summary>
        public virtual int GroupSpacing
        {
            get => _groupSpacing;
            set => _groupSpacing = value;
        }

        #endregion

        #region Layout

        /// <inheritdoc />
        public override Vector2 PreferredSize(NvgContext ctx, UIWidget parent)
        {
            var margin = parent.Margin;

            int height = (int)margin.Vertical;
            int width = 2 * (int)margin.Horizontal;

            bool first = true;
            bool indent = false;

            foreach (var c in parent.Children.AsReadOnlySpan())
            {
                if (!c.Visible)
                    continue;

                UILabel? label = null;

                if(c is UILabel lbl)
                { 
                    label = lbl;
                }

                if (!first)
                    height += label == null ? (int)Spacing.Y : _groupSpacing;

                first = false;

                // note: PreferredSize can be also calculated from child layouts,
                // so we check fixed size
                Vector2 ps = c.PreferredSize(ctx);
                Vector2 fs = c.FixedSize;

                Vector2 targetSize = new Vector2(
                    fs[0] > 0 ? fs[0] : ps[0],
                    fs[1] > 0 ? fs[1] : ps[1]
                );

                int currentIndent = indent && label == null ? _groupIndent : 0;

                height += (int)targetSize.Y;
                width = (int)MathF.Max(width, targetSize.X + 2 * margin.Horizontal + currentIndent);

                if (label != null)
                    indent = !string.IsNullOrEmpty(label.Caption);
            }

            height += (int)margin.Vertical;

            return new Vector2(width, height);
        }

        /// <inheritdoc />
        public override void PerformLayout(NvgContext ctx, UIWidget parent)
        {
            var margin = parent.Margin;

            Rect layoutArea = parent.GetLayoutArea();

            int height = (int)margin.Vertical;

            int availableWidth = (int)layoutArea.Width - 2 * (int)margin.Horizontal;

            // y offset
            height += (int)layoutArea.Y;

            bool first = true;
            bool indent = false;

            foreach (var c in parent.Children.AsReadOnlySpan())
            {
                if (!c.Visible)
                    continue;

                UILabel? label = null;

                if(c is UILabel lbl)
                {
                    label= lbl;
                }

                if (!first)
                    height += label == null ? (int)Spacing.Y : _groupSpacing;

                first = false;

                int currentIndent = indent && label == null ? _groupIndent : 0;

                // note: PreferredSize can be also calculated from child layouts,
                // so we check fixed size
                Vector2 ps = new Vector2(
                    availableWidth - currentIndent,
                    c.PreferredSize(ctx).Y);

                Vector2 fs = c.FixedSize;

                Vector2 targetSize = new Vector2(
                        fs[0] > 0 ? fs[0] : ps[0],
                        fs[1] > 0 ? fs[1] : ps[1]
                );

                // set position using x offset & current height
                c.Position = new Vector2(margin.Horizontal + currentIndent + layoutArea.X, height);
                c.Size = targetSize;

                c.PerformLayout(ctx);

                height += (int)targetSize.Y;

                if (label != null)
                    indent = !string.IsNullOrEmpty(label.Caption);
            }
        }

        #endregion
    }
}
