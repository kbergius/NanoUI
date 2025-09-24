using NanoUI.Common;
using NanoUI.Components.Files;
using NanoUI.Nvg;

namespace NanoUI.Components.Simple
{
    /// <summary>
    /// UIFileIcon.
    /// Note: supports dynamic theming. If dynamic theming is not needed,
    /// use UIIcon instead of UIFileIcon with fixed Icon & IconColor (better performance).
    /// </summary>
    public class UIFileIcon : UIIcon
    {
        public UIFileIcon(FileFolderInfo fileFolderInfo)
        {
            FileFolderInfo = fileFolderInfo;
        }

        /// <inheritdoc />
        public UIFileIcon(UIWidget parent)
            : base(parent)
        {

        }

        #region Properties

        /// <summary>
        /// FileFolderInfo
        /// </summary>
        public FileFolderInfo FileFolderInfo { get; set; }

        #endregion

        #region Drawing

        /// <inheritdoc />
        public override void Draw(NvgContext ctx)
        {
            // set icon & icon color
            (int Icon, Color IconColor) iconSpec = GetTheme().GetFileIcon(FileFolderInfo);

            Icon = iconSpec.Icon;
            IconColor = iconSpec.IconColor;

            base.Draw(ctx);
        }

        #endregion
    }
}
