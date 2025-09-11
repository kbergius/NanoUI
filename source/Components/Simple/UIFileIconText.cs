using NanoUI.Common;
using NanoUI.Components.Files;
using NanoUI.Nvg;

namespace NanoUI.Components.Simple
{
    // note: supports dynamic theming
    // note2: if dynamic theming is not needed, use IconTextPart with fixed Icon & IconColor
    // (better performance)

    /// <summary>
    /// UIFileIconText.
    /// </summary>
    public class UIFileIconText : UIIconText
    {
        public UIFileIconText(FileFolderInfo fileFolderInfo, string displayName)
        {
            FileFolderInfo = fileFolderInfo;
            Text = displayName;
        }

        public UIFileIconText(UIWidget parent)
            : base(parent)
        {

        }

        #region Properties

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
