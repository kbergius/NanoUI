using NanoUI.Common;
using NanoUI.Components.Scrolling;
using NanoUI.Nvg;
using System.Numerics;
using System;

namespace NanoUI.Components
{
    /// <summary>
    /// UIWidgetEXT consists some helper functions for ui widgets.
    /// </summary>
    public static class UIWidgetEXT
    {
        #region Reorder parent's widget list

        /// <summary>
        /// Sets widget as a first widget in parent's widget list. Same as MoveToFirst.
        /// </summary>
        public static bool SendToBack(this UIWidget w) => MoveToFirst(w);

        /// <summary>
        /// Sets widget as a first widget in parent's widget list. 
        /// </summary>
        public static bool MoveToFirst(this UIWidget w)
        {
            if (w.Parent != null)
                return w.Parent.Children.MoveToFirst(w);

            return false;
        }

        /// <summary>
        /// Sets widget as a last widget in parent's widget list. Same as MoveToLast.
        /// </summary>
        public static bool BringToFront(this UIWidget w) => MoveToLast(w);

        /// <summary>
        /// Sets widget as a last widget in parent's widget list.
        /// </summary>
        public static bool MoveToLast(this UIWidget w)
        {
            if (w.Parent != null)
                return w.Parent.Children.MoveToLast(w);

            return false;
        }

        #endregion

        #region Autoellipsis

        // todo : this should be implemented in all widgets drawing text
        // todo: better to have DidplayText property?

        /// <summary>
        /// Gets text with ellpsis ("..."), if text doesn't fit in available width.
        /// Note: you should probably call PerformLayout or RequestLayoutUpdate, when you are using this.
        /// </summary>
        public static string GetText(this UIWidget w, NvgContext ctx, ReadOnlySpan<char> text)
        {
            return GetText(w, ctx, text, w.FixedSize.X > 0 ? w.FixedSize.X : w.Size.X);
        }

        /// <summary>
        /// Gets text with ellpsis ("..."), if text doesn't fit in available width.
        /// Note: you should probably call PerformLayout or RequestLayoutUpdate, when you are using this.
        /// </summary>
        public static string GetText(this UIWidget w, NvgContext ctx, ReadOnlySpan<char> textSpan, float maxWidth)
        {
            ctx.FontSize(w.FontSize);
            ctx.FontFaceId(w.FontFaceId);

            // calculate text width
            float width = ctx.TextBounds(Vector2.Zero, textSpan, out _);

            if (width > maxWidth)
            {
                float ellipsisWidth = ctx.TextBounds(Vector2.Zero, "...", out _);

                ctx.TextGlyphPositions(Vector2.Zero, textSpan, textSpan.Length, out ReadOnlySpan<GlyphPosition> positions);

                for (int i = 0; i < positions.Length; i++)
                {
                    if (positions[i].MaxX + ellipsisWidth >= maxWidth)
                    {
                        return textSpan.Slice(0, i).ToString() + "...";
                    }
                }
            }

            return textSpan.ToString();
        }

        #endregion

        #region Misc methods

        /// <summary>
        /// Finds parent UIWindow widget if any.
        /// Note: used in UIContextMenu & UIPopup when they want to keep their owner's window widget focused
        /// (their own parent is Screen).
        /// </summary>
        public static UIWindow? FindParentWindow(this UIWidget w)
        {
            if(w is UIWindow window)
            {
                return window;
            }

            return FindParent<UIWindow>(w);
        }

        /// <summary>
        /// Finds parent widget of type T if any.
        /// </summary>
        public static T? FindParent<T>(this UIWidget w) where T : UIWidget
        {
            if (w.Parent is T res)
            {
                return res;
            }

            // Walk up the hierarchy and return the parent
            UIWidget? parent = w.Parent;

            while (true)
            {
                if (parent == null)
                {
                    return null;
                }

                if (parent is T res2)
                {
                    return res2;
                }

                parent = parent.Parent;
            }
        }

        /// <summary>
        /// Gets widget's widget tree position.
        /// Returns widget's absolute position in widget tree.
        /// Note: use GetDisplayPosition, when you want to get widget's position in display.
        /// </summary>
        public static Vector2 GetWidgetTreePosition(this UIWidget w)
        {
            return w.Parent != null ? (w.Parent.GetWidgetTreePosition() + w.Position) : w.Position;
        }

        /// <summary>
        /// Gets widget's display position. Returns widget's position in display.
        /// This handles also cases when widget is inside IScrollables & there is some scrolling happening.
        /// Note: use this allways when you are dealing with pointer position
        /// </summary>
        public static Vector2 GetDisplayPosition(this UIWidget w)
        {
            if (w.Parent == null)
            {
                return w.Position;
            }
            else if (w.Parent is IScrollable scrollable)
            {
                // note: ScrollOffset is negative
                return w.Parent.GetDisplayPosition() + w.Position + scrollable.ScrollOffset;
            }

            return w.Parent.GetDisplayPosition() + w.Position;
        }

        /// <summary>
        /// Gets visible property recursively.
        /// Takes parent widgets into account.
        /// </summary>
        public static bool GetVisibleRecursive(this UIWidget w)
        {
            bool visible = true;
            UIWidget? widget = w;

            while (widget != null)
            {
                visible &= widget.Visible;
                widget = widget.Parent;
            }

            return visible;
        }

        #endregion

        #region Draw border

       /// <summary>
        /// Draws (rounded) rectangle borders.
        /// Note: this should be called last in draw phase, since this draws in widget area that cound be
        /// possibly drawn before.
        /// </summary>
        public static void DrawBorder(this UIWidget w, NvgContext ctx, bool sunken)
        {
            DrawBorder(w, ctx, w.Position, w.Size, sunken);
        }

        /// <summary>
        /// Draws (rounded) rectangle borders.
        /// Note: this should be called last in draw phase, since this draws in widget area that cound be
        /// possibly drawn before.
        /// </summary>
        public static void DrawBorder(this UIWidget w, NvgContext ctx, Vector2 position, Vector2 size,
            bool sunken)
        {
            if (!w.Border || w.Disabled)
                return;

            // todo : should we draw special border when focused
            if (w.PointerFocus)
            {
                DrawBorder(w, ctx, position, size, 1, w.GetTheme().Borders.PointerFocus);
            }
            else
            {
                ctx.BeginPath();
                ctx.StrokeWidth(1.0f);
                ctx.RoundedRectVarying(position.X + 0.5f, position.Y + (sunken ? 0.5f : 1.5f), size.X - 1,
                               size.Y - 1 - (sunken ? 0.0f : 1.0f), w.CornerRadius);
                ctx.StrokeColor(w.GetTheme().Borders.Light);
                ctx.Stroke();

                ctx.BeginPath();
                ctx.RoundedRectVarying(position.X + 0.5f, position.Y + 0.5f, size.X - 1,
                               size.Y - 2, w.CornerRadius);
                ctx.StrokeColor(w.GetTheme().Borders.Dark);
                ctx.Stroke();
            }
        }

        /// <summary>
        /// Draws (rounded) rectangle borders.
        /// Note: this should be called last in draw phase, since this draws in widget area that cound be
        /// possibly drawn before.
        /// </summary>
        public static void DrawBorder(this UIWidget w, NvgContext ctx, float borderSize, Color borderColor)
        {
            DrawBorder(w, ctx, w.Position, w.Size, borderSize, borderColor);
        }

        /// <summary>
        /// Draws (rounded) rectangle borders.
        /// Note: this should be called last in draw phase, since this draws in widget area that cound be
        /// possibly drawn before.
        /// </summary>
        public static void DrawBorder(this UIWidget w, NvgContext ctx, Vector2 position, Vector2 size,
            float borderSize, Color borderColor)
        {
            if (borderSize <= 0 || borderColor.A == 0 || w.Disabled)
                return;

            ctx.BeginPath();

            ctx.RoundedRectVarying(
                        position + new Vector2(borderSize),
                        size - new Vector2(2 * borderSize),
                        w.CornerRadius);

            ctx.StrokeWidth(borderSize);
            ctx.StrokeColor(borderColor);
            ctx.Stroke();
        }

        #endregion

        #region Draw debug

        /// <summary>
        /// Draws red rectangle around widget.
        /// </summary>
        public static void DrawDebug(this UIWidget w, NvgContext ctx)
        {
            // we are inside scissor - so we must reset in order to draw outside scissor
            ctx.SaveState();
            ctx.ResetScissor();

            ctx.StrokeWidth(1.0f);
            ctx.BeginPath();
            ctx.Rect(w.Position.X - 0.5f, w.Position.Y - 0.5f,
                    w.Size.X + 1, w.Size.Y + 1);
            ctx.StrokeColor(Color.Red);
            ctx.Stroke();

            ctx.RestoreState();
        }

        #endregion
    }
}
