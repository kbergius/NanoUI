using System;

namespace NanoUI.Common
{
    public enum BrushType
    {
        /// <summary>
        /// Undefinied (none)
        /// </summary>
        Undefinied,

        /// <summary>
        /// BoxGradient
        /// </summary>
        BoxGradient,

        /// <summary>
        /// LinearGradient
        /// </summary>
        LinearGradient,

        /// <summary>
        /// RadialGradient
        /// </summary>
        RadialGradient,

        /// <summary>
        /// Solid (single) color
        /// </summary>
        Solid,

        /// <summary>
        /// Texture (image)
        /// </summary>
        Texture,

        /// <summary>
        /// Svg file
        /// </summary>
        Svg
    }

    // Flags to specify the button behavior (can be combined)
    [Flags]
    public enum ButtonFlags
    {
        /// <summary>
        /// NormalButton
        /// </summary>
        NormalButton = 1 << 0,

        /// <summary>
        /// RadioButton
        /// </summary>
        RadioButton = 1 << 1,

        /// <summary>
        /// ToggleButton
        /// </summary>
        ToggleButton = 1 << 2,
    }

    // Currently only Window uses
    // todo: Body not implemented
    public enum DragMode
    {
        /// <summary>
        /// Header (reposition widget)
        /// </summary>
        Header,

        /// <summary>
        /// Left (resize widget)
        /// </summary>
        Left,

        /// <summary>
        /// Right (resize widget)
        /// </summary>
        Right,

        /// <summary>
        /// Top (resize widget)
        /// </summary>
        Top,

        /// <summary>
        /// Bottom (resize widget)
        /// </summary>
        Bottom,

        /// <summary>
        /// RightBottomCorner (resize widget horizontally & vertically)
        /// </summary>
        RightBottomCorner,

        /// <summary>
        /// None (no grag behavior)
        /// </summary>
        NONE,
    }

    public enum FileDialogType
    {
        /// <summary>
        /// New file dialog
        /// </summary>
        New,

        /// <summary>
        /// Open file dialog
        /// </summary>
        Open,

        /// <summary>
        /// SaveAs file dialog
        /// </summary>
        SaveAs,
    }

    public enum FileFolderType
    {
        /// <summary>
        /// HardDrive
        /// </summary>
        HardDrive,

        /// <summary>
        /// Folder
        /// </summary>
        Folder,

        /// <summary>
        /// File
        /// </summary>
        File
    }

    public enum GlyphBaking : int
    {
        /// <summary>
        /// Glyphs converted to "normal" bitmaps
        /// </summary>
        Normal,

        /// <summary>
        /// Glyphs converted to SDF bitmaps
        /// </summary>
        SDF,

        /// <summary>
        /// Glyphs converted to shapes
        /// </summary>
        Shapes
    }

    public enum IconAlign
    {
        /// <summary>
        /// Button icon on the far left
        /// </summary>
        Left = 0,

        /// <summary>
        /// Button icon on the left, centered (depends on caption text length).
        /// </summary>
        LeftCentered = 1,

        /// <summary>
        /// Button icon on the right, centered (depends on caption text length).
        /// </summary>
        RightCentered = 2,

        /// <summary>
        /// Button icon on the far right.
        /// </summary>
        Right = 3
    }

    // The available icon positions.
    // todo: same as IconAlign
    public enum IconPosition
    {
        /// <summary>
        /// Button icon on the far left
        /// </summary>
        Left,

        /// <summary>
        /// Button icon on the left, centered (depends on caption text length)
        /// </summary>
        LeftCentered,

        /// <summary>
        /// Button icon on the right, centered (depends on caption text length)
        /// </summary>
        RightCentered,

        /// <summary>
        /// Button icon on the far right
        /// </summary>
        Right
    }

    public enum LayoutAlignment : byte
    {
        /// <summary>
        /// Take only as much space as is required (top/left)
        /// </summary>
        Minimum = 0,

        /// <summary>
        /// Center align (center/middle)
        /// </summary>
        Middle,

        /// <summary>
        /// Take as much space as is allowed (bottom/right)
        /// </summary>
        Maximum,

        /// <summary>
        /// Fill according to preferred sizes
        /// </summary>
        Fill
    }

    public enum LineCap
    {
        /// <summary>
        /// Butt
        /// </summary>
        Butt,

        /// <summary>
        /// Round
        /// </summary>
        Round,

        /// <summary>
        /// Square
        /// </summary>
        Square,

        /// <summary>
        /// Bevel
        /// </summary>
        Bevel,

        /// <summary>
        /// Miter
        /// </summary>
        Miter
    }

    public enum MessageDialogType
    {
        /// <summary>
        /// MessageBox with information icon
        /// </summary>
        Information,

        /// <summary>
        /// MessageBox with question icon
        /// </summary>
        Question,

        /// <summary>
        /// MessageBox with warning icon
        /// </summary>
        Warning,

        /// <summary>
        /// MessageBox with error icon
        /// </summary>
        Error
    }

    public enum NumericFormat
    {
        /// <summary>
        /// No decimals
        /// </summary>
        NONE,

        /// <summary>
        /// No decimals (todo same as above)
        /// </summary>
        Decimal0,

        /// <summary>
        /// 1 decimal
        /// </summary>
        Decimal1,

        /// <summary>
        /// 2 decimals
        /// </summary>
        Decimal2,

        /// <summary>
        /// 3 decimals
        /// </summary>
        Decimal3,

        /// <summary>
        /// 4 decimals
        /// </summary>
        Decimal4,

        /// <summary>
        /// 5 decimals
        /// </summary>
        Decimal5,

        /// <summary>
        /// 6 decimals
        /// </summary>
        Decimal6,
    }

    // these are default pointer types used internally, but type is stored as int so user can set any int value
    // value is passed to screen & user app
    public enum PointerType
    {
        /// <summary>
        /// Arrow
        /// </summary>
        Arrow,

        /// <summary>
        /// IBeam (for editable texts)
        /// </summary>
        IBeam,

        /// <summary>
        /// Crosshair
        /// </summary>
        Crosshair,

        /// <summary>
        /// Hand
        /// </summary>
        Hand,

        /// <summary>
        /// No
        /// </summary>
        No,

        /// <summary>
        /// SizeAll (horizontal & vertical)
        /// </summary>
        SizeAll,

        /// <summary>
        /// SizeNESW
        /// </summary>
        SizeNESW,

        /// <summary>
        /// SizeNS
        /// </summary>
        SizeNS,

        /// <summary>
        /// SizeNWSE
        /// </summary>
        SizeNWSE,

        /// <summary>
        /// SizeWE
        /// </summary>
        SizeWE,

        /// <summary>
        /// Wait
        /// </summary>
        Wait,

        /// <summary>
        /// WaitArrow
        /// </summary>
        WaitArrow,
    }

    // The direction of data flow for a layout
    public enum Orientation
    {
        /// <summary>
        /// Layout expands on horizontal axis
        /// </summary>
        Horizontal = 0,

        /// <summary>
        /// Layout expands on vertical axis
        /// </summary>
        Vertical,
    }

    // this is popup's position relative to its parent (UIPopupButton)
    public enum PopupPosition
    {
        /// <summary>
        /// LeftMiddle
        /// </summary>
        LeftMiddle = 0,

        /// <summary>
        /// RightMiddle
        /// </summary>
        RightMiddle,

        /// <summary>
        /// RightTop
        /// </summary>
        RightTop,

        /// <summary>
        /// LeftTop
        /// </summary>
        LeftTop,

        /// <summary>
        /// Bottom
        /// </summary>
        Bottom
    }

    [Flags]
    public enum SeparatorDrawFlag
    {
        /// <summary>
        /// Horizontal
        /// </summary>
        Horizontal = 1 << 0,

        /// <summary>
        /// Vertical
        /// </summary>
        Vertical = 1 << 1,

        /// <summary>
        /// Bottom
        /// </summary>
        Bottom = 1 << 2,

        /// <summary>
        /// Top
        /// </summary>
        Top = 1 << 3,

        /// <summary>
        /// Left
        /// </summary>
        Left = 1 << 4,

        /// <summary>
        /// Rigth
        /// </summary>
        Rigth = 1 << 5,

        /// <summary>
        /// CenterH
        /// </summary>
        CenterH = 1 << 6,

        /// <summary>
        /// CenterV
        /// </summary>
        CenterV = 1 << 7
    }

    public enum ScrollableDragMode
    {
        /// <summary>
        /// None
        /// </summary>
        None,

        /// <summary>
        /// Vertical
        /// </summary>
        Vertical,

        /// <summary>
        /// Horizontal
        /// </summary>
        Horizontal,
    }

    public enum ScrollbarType
    {
        /// <summary>
        /// No scrollbars
        /// </summary>
        NONE,

        /// <summary>
        /// Horizontal
        /// </summary>
        Horizontal,

        /// <summary>
        /// Vertical
        /// </summary>
        Vertical,

        /// <summary>
        /// Both (vertical & horizontal)
        /// </summary>
        Both
    }

    public enum Solidity
    {
        /// <summary>
        /// Winding CounterClockwise
        /// </summary>
        Solid = Winding.CounterClockwise,

        /// <summary>
        /// Winding Clockwise
        /// </summary>
        Hole = Winding.Clockwise
    }

    
    // this is a shortcut to both horizontal & vertical aligment
    [Flags]
    public enum TextAlignment : int
    {
        /// <summary>
        /// Left
        /// </summary>
        Left = 1 << 0,

        /// <summary>
        /// Center (horizontally)
        /// </summary>
        Center = 1 << 1,

        /// <summary>
        /// Right
        /// </summary>
        Right = 1 << 2,

        /// <summary>
        /// Top
        /// </summary>
        Top = 1 << 3,

        /// <summary>
        /// Middle (vertically)
        /// </summary>
        Middle = 1 << 4,

        /// <summary>
        /// Bottom
        /// </summary>
        Bottom = 1 << 5,

        /// <summary>
        /// Baseline
        /// </summary>
        Baseline = 1 << 6
    }

    // note : some widgets have fixed alignment - ie changing property has no effect
    public enum TextHorizontalAlign
    {
        /// <summary>
        /// Left
        /// </summary>
        Left,

        /// <summary>
        /// Center
        /// </summary>
        Center,

        /// <summary>
        /// Right
        /// </summary>
        Right
    }

    // note : some widgets have fixed alignment - ie changing property has no effect
    public enum TextVerticalAlign
    {
        /// <summary>
        /// Top
        /// </summary>
        Top,

        /// <summary>
        /// Middle
        /// </summary>
        Middle,

        /// <summary>
        /// Bottom
        /// </summary>
        Bottom
    }

    // todo: add to all property enums Theme =0 (this is default) => uses theme spesified value
    [Flags]
    public enum TextureFlags
    {
        /// <summary>
        /// Generate mipmaps during creation of the texture
        /// </summary>
        GenerateMipmaps = 1 << 0,

        /// <summary>
        /// Repeat texture in X direction
        /// </summary>
        RepeatX = 1 << 1,

        /// <summary>
        /// Repeat texture in Y direction
        /// </summary>
        RepeatY = 1 << 2,

        /// <summary>
        /// Flips texture in Y direction when rendered
        /// </summary>
        FlipY = 1 << 3,

        /// <summary>
        /// Texture data has premultiplied alpha
        /// </summary>
        Premultiplied = 1 << 4,

        /// <summary>
        /// Texture interpolation is Nearest instead Linear
        /// </summary>
        Nearest = 1 << 5
    }

    public enum TextureFormat
    {
        /// <summary>
        /// Single channel texture used to for example create font atlas texture
        /// </summary>
        R,
        /// <summary>
        /// 2-channel texture
        /// </summary>
        RG,
        /// <summary>
        /// Full 4-channel texture
        /// </summary>
        RGBA
    }

    // used in views
    public enum ViewSelectionMode
    {
        /// <summary>
        /// Whole item selected
        /// </summary>
        Item,

        /// <summary>
        /// Item's cell selected
        /// </summary>
        Cell
    }

    // used basically to get correct background brush
    // note: hovered (MouseFocus) is combined with these
    public enum WidgetState
    {
        /// <summary>
        /// Inactive
        /// </summary>
        Unfocused,

        /// <summary>
        /// Active
        /// </summary>
        Focused,

        /// <summary>
        /// Disabled
        /// </summary>
        Disabled,

        /// <summary>
        /// Pushed
        /// </summary>
        Pushed,

        /// <summary>
        /// Invalid
        /// </summary>
        Invalid // todo: should we handle it here or in widget level?
    }

    // Normally you should use Winding.CounterClockwise (solid) or Winding.Clockwise (hole)
    // note: if you have issues with fills, you could also try setting Winding.Manual
    public enum Winding
    {
        /// <summary>
        /// Bypasses automatic winding check & points conversion
        /// </summary>
        Manual = 0,

        /// <summary>
        /// Winding for solid shapes
        /// </summary>
        CounterClockwise = 1,

        /// <summary>
        /// Winding for holes
        /// </summary>
        Clockwise = 2
    }
}
