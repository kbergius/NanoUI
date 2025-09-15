using System;

namespace NanoUI
{
    /// <summary>
    /// Globals has global values for NanoUI.
    /// </summary>
    public class Globals
    {
        /// <summary>
        /// Used to indicate that methods that return int value is not defined/incorrect/missing.
        /// Used mainly with textures, fonts, icons, glyphs. Value: -1.
        /// Note: basically all negative values are treated as invalid.
        /// </summary>
        public const int INVALID = -1;

        /// <summary>
        /// Set to true, if you want to draw red bounding rectangles around widgets.
        /// Default value: false.
        /// </summary>
        public static bool DEBUG_WIDGET_BOUNDS = false;

        /// <summary>
        /// Global setting to determine, if tooltips are shown.
        /// Default value: true;
        /// </summary>
        public static bool SHOW_TOOLTIPS = true;

        /// <summary>
        /// Initial font texture width. Default value: 1024.
        /// If font texture becomes full, NanoUI informs user to expand it.
        /// </summary>
        public static int FONT_TEXTURE_WIDTH = 1024;

        /// <summary>
        /// Initial font texture height. Default value: 512.
        /// If font texture becomes full, NanoUI informs user to expand it.
        /// </summary>
        public static int FONT_TEXTURE_HEIGHT = 512;

        /// <summary>
        /// Maximum text rows that NanoUI processes. Default value: 1024.
        /// </summary>
        public static int MAX_TEXT_ROWS = 1024;

        /// <summary>
        /// When calling RequestLayoutUpdate, NanoUI queues commands. 
        /// This value tells, howe many queued commands are processed in scene Draw
        /// method / frame. Value: 10.
        /// </summary>
        public const int MAX_LAYOUT_UPDATE_PER_FRAME = 10;

        /// <summary>
        /// Determines the time delay (when button is down) after repeated action is fired
        /// or when NanoUI should update some values in widgets.
        //  Value is in seconds.
        /// </summary>
        public static float UPDATE_DELAY = 0.1f;

        /// <summary>
        /// Make caret in active text field blink. Default = true.
        /// </summary>
        public static bool BLINK_TEXTBOX_CARET = true;

        /// <summary>
        /// Used if folder path is not specified or specified path is invalid.
        /// Default value: Environment.CurrentDirectory.
        /// </summary>
        public static string DEFAULT_FOLDER_PATH = Environment.CurrentDirectory;

        /// <summary>
        /// File/folder widgets directory up string. Default value: "...".
        /// </summary>
        public static string FILEFOLDER_DIRECTORY_UP = "...";

        /// <summary>
        /// Category BASIC to group many properties in widgets into same group.
        /// </summary>
        public const string CATEGORY_BASIC = "Basic";

        /// <summary>
        /// Category LAYOUT to group many properties in widgets into same group.
        /// </summary>
        public const string CATEGORY_LAYOUT = "Layout";

        /// <summary>
        /// Category APPEARANCE to group many properties in widgets into same group.
        /// </summary>
        public const string CATEGORY_APPEARANCE = "Appearance";

        /// <summary>
        /// Category TEXT to group many properties in widgets into same group.
        /// </summary>
        public const string CATEGORY_TEXT = "Text";

        /// <summary>
        /// Category BEHAVIOR to group many properties in widgets into same group.
        /// </summary>
        public const string CATEGORY_BEHAVIOR = "Behavior";

        /// <summary>
        /// Category TRANSFORM to group many properties in widgets into same group.
        /// </summary>
        public const string CATEGORY_TRANSFORM = "Transform";
        
        /// <summary>
        /// Category MISC (no spesific category) to group many properties in widgets into same group.
        /// </summary>
        public const string CATEGORY_MISC = "Misc";

        /// <summary>
        /// Order of the categories to display.
        /// </summary>
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
