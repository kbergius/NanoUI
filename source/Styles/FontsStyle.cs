using NanoUI.Fonts;
using NanoUI.Nvg;
using System;
using System.Collections.Generic;
using System.IO;

namespace NanoUI.Styles
{
    // todo: this could be simpler & support also non-desktops (AddFont(byte[] array)!
    // todo: could be Screen.AddFont / Theme.AddFont?
    // todo?: embed FontAwesome icons & use it as default icon file (because icons point at enum FontAwesome)?

    /// <summary>
    /// Global font style. Normal & icons file is required, rest is optional.
    /// Note: Icon specifications is by default set based on FontAwesome (FontAwesome6Free-Solid-900.otf).
    /// There is enum for all available icons in Fonts/FontAwesomeIcon.
    /// </summary>
    public struct FontsStyle
    {
        // note: at runtime we use this
        Dictionary<string, int> _fontMappings = new();

        public FontsStyle() { }

        #region Fonts

        public string? DefaultFontType { get; set; }

        public string? DefaultIconsType { get; set; }

        /// <summary>
        /// First font type is default.
        /// </summary>
        public Dictionary<string, string> FontTypes { get; set; } = new();

        #endregion

        #region Icons

        /// <summary>
        /// Scale used as base size value for icons. Display icon scale is calculated by
        /// DefaultIconScale * widget's IconExtraScale. Default value: 1.
        /// </summary>
        public float IconBaseScale { get; set; } = 1;

        // todo: check : these should be used in many widgets so they are not tied with widget
        // Icon to use when a text box has an up toggle (e.g. UpDownButton)

        /// <summary>
        /// IconCaretUp is used internally; so must be defined.
        /// Default value: FontAwesomeIcon.CaretUp.
        /// </summary>
        public int IconCaretUp { get; set; } = (int)FontAwesomeIcon.CaretUp;

        /// <summary>
        /// IconCaretDown is used internally; so must be defined.
        /// Default value: FontAwesomeIcon.CaretDown.
        /// </summary>
        public int IconCaretDown { get; set; } = (int)FontAwesomeIcon.CaretDown;

        /// <summary>
        /// IconCaretRight is used internally; so must be defined.
        /// Default value: FontAwesomeIcon.CaretRight.
        /// </summary>
        public int IconCaretRight { get; set; } = (int)FontAwesomeIcon.CaretRight;

        /// <summary>
        /// IconCaretLeft is used internally; so must be defined.
        /// Default value: FontAwesomeIcon.CaretLeft.
        /// </summary>
        public int IconCaretLeft { get; set; } = (int)FontAwesomeIcon.CaretLeft;

        /// <summary>
        /// IconCollapsed is used internally; so must be defined.
        /// Default value: FontAwesomeIcon.CaretRight.
        /// </summary>
        public int IconCollapsed { get; set; } = (int)FontAwesomeIcon.CaretRight;

        /// <summary>
        /// IconExpanded is used internally; so must be defined.
        /// Default value: FontAwesomeIcon.CaretDown.
        /// </summary>
        public int IconExpanded { get; set; } = (int)FontAwesomeIcon.CaretDown;

        /// <summary>
        /// IconChecked is used internally; so must be defined.
        /// Default value: FontAwesomeIcon.Check.
        /// </summary>
        public int IconChecked { get; set; } = (int)FontAwesomeIcon.Check;

        /// <summary>
        /// IconSearch is used internally; so must be defined.
        /// Default value: FontAwesomeIcon.MagnifyingGlass.
        /// </summary>
        public int IconSearch { get; set; } = (int)FontAwesomeIcon.MagnifyingGlass;

        /// <summary>
        /// IconCancel is used internally; so must be defined.
        /// Default value: FontAwesomeIcon.CircleXmark.
        /// </summary>
        public int IconCancel { get; set; } = (int)FontAwesomeIcon.CircleXmark;

        /// <summary>
        /// IconClose is used internally; so must be defined.
        /// Default value: FontAwesomeIcon.CircleXmark.
        /// </summary>
        public int IconClose { get; set; } = (int)FontAwesomeIcon.CircleXmark;

        /// <summary>
        /// IconInformation is used internally; so must be defined.
        /// Default value: FontAwesomeIcon.CircleExclamation.
        /// </summary>
        public int IconInformation { get; set; } = (int)FontAwesomeIcon.CircleExclamation;

        /// <summary>
        /// IconQuestion is used internally; so must be defined.
        /// Default value: FontAwesomeIcon.CircleQuestion.
        /// </summary>
        public int IconQuestion { get; set; } = (int)FontAwesomeIcon.CircleQuestion;

        /// <summary>
        /// IconWarning is used internally; so must be defined.
        /// Default value: FontAwesomeIcon.TriangleExclamation.
        /// </summary>
        public int IconWarning { get; set; } = (int)FontAwesomeIcon.TriangleExclamation;

        /// <summary>
        /// IconError is used internally; so must be defined.
        /// Default value: FontAwesomeIcon.CircleXmark.
        /// </summary>
        public int IconError { get; set; } = (int)FontAwesomeIcon.CircleXmark;

        /// <summary>
        /// IconHardDrive is used internally; so must be defined.
        /// Default value: FontAwesomeIcon.HardDrive.
        /// </summary>
        public int IconHardDrive { get; set; } = (int)FontAwesomeIcon.HardDrive;

        /// <summary>
        /// IconFolder is used internally; so must be defined.
        /// Default value: FontAwesomeIcon.Folder.
        /// </summary>
        public int IconFolder { get; set; } = (int)FontAwesomeIcon.Folder;

        /// <summary>
        /// IconFile is used internally; so must be defined.
        /// Default value: FontAwesomeIcon.File.
        /// </summary>
        public int IconFile { get; set; } = (int)FontAwesomeIcon.File;

        #endregion

        #region Methods

        /// <summary>
        /// Init style after you have added fonts.
        /// </summary>
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

        /// <summary>
        /// GetFontId by name. If not found, returns first font, 
        /// that is added with AddFont method (id = 0).
        /// </summary>
        public int GetFontId(string fontType)
        {
            if (!string.IsNullOrEmpty(fontType) && _fontMappings.TryGetValue(fontType, out int fontId))
            {
                return fontId;
            }

            // default
            return 0;
        }

        public string GetDefaultFontType()
        {
            if(DefaultFontType != null)
                return DefaultFontType;

            foreach (var fontMapping in _fontMappings)
            {
                return fontMapping.Key;
            }

            // todo: should we throw?
            return string.Empty;
        }

        public string GetDefaultIconType()
        {
            if (DefaultIconsType != null)
                return DefaultIconsType;

            foreach (var fontMapping in _fontMappings)
            {
                // todo: this is just a guess
                if (fontMapping.Key.ToLower().Contains("icon"))
                {
                    return fontMapping.Key;
                }
            }

            // todo: should we throw?
            return string.Empty;
        }

        public string? GetFontType(int fontId)
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
