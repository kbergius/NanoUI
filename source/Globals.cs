using System;

namespace NanoUI
{
    public class Globals
    {
        // this is used to indicate that int value (that points to struct/class) is not defined/incorrect.
        // used with textures, fonts, glyphs etc
        // note: basically all negative values are treated as invalid
        public const int INVALID = -1;
        
        // Uncomment the following definition to draw red bounding
        // boxes around widgets(useful for debugging drawing code)
        public static bool DEBUG_WIDGET_BOUNDS = false;
        public static bool SHOW_TOOLTIPS = true;

        // these are initial font texture values. if texture becomes full, it informs user to expand texture
        public static int FONT_TEXTURE_WIDTH = 1024;
        public static int FONT_TEXTURE_HEIGHT = 512;

        public static int MAX_TEXT_ROWS = 1024;

        // we restrict layout updates per frame (used in screen)
        public const int MAX_LAYOUT_UPDATE_PER_FRAME = 10;

        // this determines the time delay (when button is down) after repeated action is fired
        // or when we should update some values (like performance graph text)
        // value is in seconds
        public static float UPDATE_DELAY = 0.1f;

        // shall we make caret in active textbox blink?
        public static bool BLINK_TEXTBOX_CARET = true;       

        // this is used if folder path not specified or specified path is invalid
        public static string DEFAULT_FOLDER_PATH = Environment.CurrentDirectory;

        // Files / folders
        public static string FILEFOLDER_DIRECTORY_UP = "...";

        // DEFAULT CATEGORIES
        // These are attributes that all objects properties can have
        // (System.ComponentModel.CategoryAttribute)
        // note : these are used as ids in categorySorter by now
        public const string CATEGORY_BASIC = "Basic";
        public const string CATEGORY_LAYOUT = "Layout";
        public const string CATEGORY_APPEARANCE = "Appearance";
        public const string CATEGORY_TEXT = "Text";
        public const string CATEGORY_BEHAVIOR = "Behavior";
        public const string CATEGORY_TRANSFORM = "Transform";
        // no spesific category
        public const string CATEGORY_MISC = "Misc";

        // the order of this array is used as default category sort order in CategorySorter
        public static string[] GetDefaultCategoryIds()
        {
            return[
                CATEGORY_BASIC,
                CATEGORY_LAYOUT,
                CATEGORY_APPEARANCE,
                CATEGORY_TEXT,
                CATEGORY_BEHAVIOR,
                CATEGORY_TRANSFORM
                ];
        }
    }
}