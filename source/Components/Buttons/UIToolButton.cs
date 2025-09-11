using NanoUI.Common;
using NanoUI.Nvg;
using System.Numerics;

namespace NanoUI.Components.Buttons
{
    // Tool button that can be combined with button flags.RadioButton & ToggleButton;
    // note: there is no text by default

    /// <summary>
    /// UIToolButton.
    /// </summary>
    public class UIToolButton : UIButton
    {
        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        public UIToolButton()
        {
            // set defaults to theme impl - prevents circular reference
            Dimension = default;
            Flags = default;
            
            // no text!
        }

        public UIToolButton(UIWidget parent)
            : this(parent, -1)
        {
        }

        public UIToolButton(UIWidget parent, int icon)
           : base(parent, string.Empty, icon)
        {
            // no text!
            ThemeType = typeof(UIButton);
        }

        #region Properties

        uint? _dimension;
        public uint Dimension
        {
            get => _dimension?? GetTheme().ToolButton.Dimension;
            set => _dimension = value;
        }

        ButtonFlags? _flags;
        public override ButtonFlags Flags
        {
            get => _flags?? GetTheme().ToolButton.Flags;
            set => _flags = value;
        }

        #endregion

        #region Layout

        public override Vector2 PreferredSize(NvgContext ctx)
        {
            return Vector2.Max(MinSize, new Vector2(Dimension));
        }

        #endregion
    }
}
