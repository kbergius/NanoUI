using NanoUI.Fonts;
using NanoUI.Nvg;
using System;
using System.Collections.Generic;
using System.IO;

namespace NanoUI.Styles
{
    // todo?: embed FontAwesome icons & use it as default icon file (because icons point at enum FontAwesome)?

    // Fonts - normal & icons file is required, rest is optional
    public partial class FontsStyle
    {
        // note: at runtime we use this
        Dictionary<string, int> _fontMappings = new();

        #region Fonts

        public string DefaultFontType { get; set; }
        public string DefaultIconsType { get; set; }

        // note: first font type is default
        // this is saved to JSON
        public Dictionary<string, string> FontTypes { get; set; } = new();

        #endregion

        #region Icons

        // these are minimum set of icons that is used internally
        // note: icon values should correnspond the loaded font icon file (defaults are set with FontAwesome icons)
        // Icons - minimal set of icons used in core widgets

        // scale used as base size value for icons, real icon scale is calculated by
        // DefaultIconScale * Widget.IconExtraScale
        public float IconBaseScale { get; set; } = 1;

        // todo: check : these should be used in many widgets so they are not tied with widget
        // Icon to use when a text box has an up toggle (e.g. UpDownButton)
        public int IconCaretUp { get; set; } = (int)FontAwesomeIcon.CaretUp;
        public int IconCaretDown { get; set; } = (int)FontAwesomeIcon.CaretDown;
        public int IconCaretRight { get; set; } = (int)FontAwesomeIcon.CaretRight;
        public int IconCaretLeft { get; set; } = (int)FontAwesomeIcon.CaretLeft;

        // used in CollapsablePanel, TreeView, Window
        public int IconCollapsed { get; set; } = (int)FontAwesomeIcon.CaretRight;
        public int IconExpanded { get; set; } = (int)FontAwesomeIcon.CaretDown;

        // Checkbox
        public int IconChecked { get; set; } = (int)FontAwesomeIcon.Check;

        // SearchBox
        public int IconSearch { get; set; } = (int)FontAwesomeIcon.MagnifyingGlass;
        public int IconCancel { get; set; } = (int)FontAwesomeIcon.CircleXmark;

        // DockPanel
        public int IconClose { get; set; } = (int)FontAwesomeIcon.CircleXmark;

        // MessageBox
        public int IconInformation { get; set; } = (int)FontAwesomeIcon.CircleExclamation;
        public int IconQuestion { get; set; } = (int)FontAwesomeIcon.CircleQuestion;
        public int IconWarning { get; set; } = (int)FontAwesomeIcon.TriangleExclamation;
        public int IconError { get; set; } = (int)FontAwesomeIcon.CircleXmark;

        // File/Folder widgets
        public int IconHardDrive { get; set; } = (int)FontAwesomeIcon.HardDrive;
        public int IconFolder { get; set; } = (int)FontAwesomeIcon.Folder;
        public int IconFile { get; set; } = (int)FontAwesomeIcon.File;

        #endregion

        #region Methods

        public void Init(NvgContext ctx)
        {
            // loop fontypes, create fonts & mappings
            foreach (var fontType in FontTypes)
            {
                // check validity
                if (string.IsNullOrEmpty(fontType.Key) || !File.Exists(fontType.Value))
                    continue;

                // create font
                int fontId = ctx.CreateFont(fontType.Key, fontType.Value);

                if (fontId >= 0)
                {
                    // add mapping
                    _fontMappings[fontType.Key] = fontId;
                }
            }

            // we should have at least 1 font mapping
            if (_fontMappings.Count == 0)
                throw new Exception("No valid font types found");
        }

        public int GetFontId(string fontType)
        {
            if (!string.IsNullOrEmpty(fontType) && _fontMappings.TryGetValue(fontType, out int fontId))
            {
                return fontId;
            }

            // default
            return 0;
        }

        public string GetFontType(int fontId)
        {
            foreach (var kvp in _fontMappings)
            {
                if (kvp.Value == fontId)
                    return kvp.Key;
            }

            return null;
        }

        // todo: check this
        public int AddFont(NvgContext ctx, string fontName, string fontPath)
        {
            if (!string.IsNullOrEmpty(fontName) && File.Exists(fontPath))
            {
                int fontId = ctx.CreateFont(fontName, fontPath);

                if (fontId >= 0)
                {
                    FontTypes[fontName] = fontPath;

                    _fontMappings.Add(fontName, fontId);

                    return fontId;
                }
            }

            return -1;
        }

        #endregion
    }
}