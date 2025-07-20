using System;

namespace NanoUI.Common
{
    public enum BrushType
    {
        Undefinied,
        BoxGradient,
        LinearGradient,
        RadialGradient,
        Solid,
        Texture,
        Svg
    }

    // Flags to specify the button behavior (can be combined)
    [Flags]
    public enum ButtonFlags
    {
        NormalButton = 1 << 0,
        RadioButton = 1 << 1,
        ToggleButton = 1 << 2,
    }

    // Currently only Window uses
    public enum DragMode
    {
        Header,
        //Body, // this is not implemented
        Left,
        Right,
        Top,
        Bottom,
        RightBottomCorner,
        NONE,
    }

    public enum FileDialogType
    {
        New,
        Open,
        SaveAs,
    }

    public enum FileFolderType
    {
        HardDrive,
        Folder,
        File
    }

    public enum GlyphBaking : int
    {
        Normal,  // uses normal rendering in shader (bitmap)
        SDF,     // uses sdf rendering in shader  (bitmap)
        Shapes   // draws glyphs as vector graphics shapes (uses Paint & PathAPI)
    }

    public enum IconAlign
    {
        Left = 0,         ///< Button icon on the far left.
        LeftCentered = 1, ///< Button icon on the left, centered (depends on caption text length).
        RightCentered = 2,///< Button icon on the right, centered (depends on caption text length).
        Right = 3         ///< Button icon on the far right.
    }

    // The available icon positions.
    public enum IconPosition
    {
        Left,         //< Button icon on the far left.
        LeftCentered, //< Button icon on the left, centered (depends on caption text length).
        RightCentered,//< Button icon on the right, centered (depends on caption text length).
        Right         //< Button icon on the far right.
    }

    public enum LayoutAlignment : byte
    {
        Minimum = 0, // Take only as much space as is required. (top/left)
        Middle,      // Center align. (center/middle)
        Maximum,     // Take as much space as is allowed. (bottom/right)
        Fill         // Fill according to preferred sizes. // (stretch)
    }

    public enum LineCap
    {
        Butt,
        Round,
        Square,
        Bevel,
        Miter
    }

    public enum MessageDialogType
    {
        Information,
        Question,
        Warning,
        Error
    }

    public enum NumericFormat
    {
        NONE,
        Decimal0,
        Decimal1,
        Decimal2,
        Decimal3,
        Decimal4,
        Decimal5,
        Decimal6,
    }

    // these are default pointer types used internally, but type is stored as int so user can set any int value
    // value is passed to screen & user app
    public enum PointerType
    {
        Arrow,
        IBeam,
        Crosshair,
        Hand,
        No,
        SizeAll,
        SizeNESW,
        SizeNS,
        SizeNWSE,
        SizeWE,
        Wait,
        WaitArrow,
    }

    // The direction of data flow for a layout
    public enum Orientation
    {
        Horizontal = 0, // Layout expands on horizontal axis.
        Vertical,       // Layout expands on vertical axis.
    }

    // this is popup's position relative to its parent (UIPopupButton)
    public enum PopupPosition
    {
        LeftMiddle = 0,
        RightMiddle,
        RightTop, // menus etc
        LeftTop,
        Bottom
    }

    [Flags]
    public enum SeparatorDrawFlag
    {
        Horizontal = 1 << 0,
        Vertical = 1 << 1,
        Bottom = 1 << 2,
        Top = 1 << 3,
        Left = 1 << 4,
        Rigth = 1 << 5,
        CenterH = 1 << 6,
        CenterV = 1 << 7
    }

    public enum ScrollableDragMode
    {
        None,
        Vertical,
        Horizontal,
    }

    public enum ScrollbarType
    {
        NONE,
        Horizontal,
        Vertical,
        Both
    }

    public enum Solidity
    {
        Solid = Winding.CounterClockwise,
        Hole = Winding.Clockwise
    }

    
    // this is a shortcut to both horizontal & vertical aligment
    [Flags]
    public enum TextAlignment : int
    {
        Left = 1 << 0,
        Center = 1 << 1,
        Right = 1 << 2,

        Top = 1 << 3,
        Middle = 1 << 4,
        Bottom = 1 << 5,
        Baseline = 1 << 6
    }

    // note : some widgets have fixed alignment - ie changing property has no effect
    public enum TextHorizontalAlign
    {
        Left,
        Center,
        Right
    }

    // note : some widgets have fixed alignment - ie changing property has no effect
    public enum TextVerticalAlign
    {
        Top,
        Middle,
        Bottom
    }

    // todo: add to all property enums Theme =0 (this is default) => uses theme spesified value
    [Flags]
    public enum TextureFlags
    {
        // Generate mipmaps during creation of the texture.
        GenerateMipmaps = 1 << 0,

        // Repeate texture in X direction.
        RepeatX = 1 << 1,

        // Repeate texture in Y direction.
        RepeatY = 1 << 2,

        // Flips (inverses) texture in Y direction when rendered.
        FlipY = 1 << 3,

        // Texture data has premultiplied alpha.
        Premultiplied = 1 << 4,

        // Texture interpolation is Nearest instead Linear
        Nearest = 1 << 5
    }

    public enum TextureFormat
    {
        R, // this is single channel texture used to create font atlas texture
        RG,
        RGB,
        RGBA // this is used for images (default in TextureDesc)
    }

    // used in views
    public enum ViewSelectionMode
    {
        Item, // default
        Cell // indidual widget in "cell" from item's children
    }

    // used basically to get correct background brush
    // note: hovered (MouseFocus) is combined with these
    public enum WidgetState
    {
        Unfocused, // Inactive,
        Focused, // Active,
        Disabled,
        Pushed, // Selected // or Pressed
        Invalid // todo: should we handle it here or in widget level?
    }

    // Normally you should use Winding.CounterClockwise (solid) or Winding.Clockwise (hole)
    // note: if you have issues with fills, you could also try setting Winding.Manual
    public enum Winding
    {
        // Bypasses automatic winding check & points conversion
        Manual = 0,

        // Winding for solid shapes.
        CounterClockwise = 1,
        
        // Winding for holes.
        Clockwise = 2
    }
}