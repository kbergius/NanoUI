using NanoUI.Common;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUI.Components.Buttons
{
    /// <summary>
    /// UIToolButton can be combined with button flags RadioButton & ToggleButton.
    /// </summary>
    /// <remarks>There is no text by default.</remarks>
    public class UIToolButton : UIButton
    {
        /// <inheritdoc />
        public UIToolButton()
        {
            // set defaults to theme impl - prevents circular reference
            Dimension = default;
            Flags = default;
            
            // no text!
        }

        /// <inheritdoc />
        public UIToolButton(UIWidget parent)
            : this(parent, -1)
        {
        }

        /// <inheritdoc />
        public UIToolButton(UIWidget parent, int icon)
           : base(parent, string.Empty, icon)
        {
            // no text!
            ThemeType = typeof(UIButton);
        }

        #region Properties

        uint? _dimension;

        /// <summary>
        /// Dimension
        /// </summary>
        public uint Dimension
        {
            get => _dimension?? GetTheme().ToolButton.Dimension;
            set => _dimension = value;
        }

        ButtonFlags? _flags;

        /// <inheritdoc />
        public override ButtonFlags Flags
        {
            get => _flags?? GetTheme().ToolButton.Flags;
            set => _flags = value;
        }

        #endregion

        #region Layout

        /// <inheritdoc />
        public override Vector2 PreferredSize(NvgContext ctx)
        {
            return Vector2.Max(MinSize, new Vector2(Dimension));
        }

        #endregion
    }
}
