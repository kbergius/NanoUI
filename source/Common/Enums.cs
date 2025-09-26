using System;

namespace NanoUI.Common
{
    /// <summary>
    /// BrushType.
    /// </summary>
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

    /// <summary>
    /// ButtonFlags.
    /// Flags to specify button widget behavior (can be combined).
    /// </summary>
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

    // todo: Body not implemented

    /// <summary>
    /// DragMode.
    /// Currently only Window widget uses.
    /// </summary>
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

    /// <summary>
    /// FileDialogType.
    /// </summary>
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

    /// <summary>
    /// FileFolderType.
    /// </summary>
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

    /// <summary>
    /// GlyphBaking determines which way glyphs are stored and rendered.
    /// </summary>
    public enum GlyphBaking : int
    {
        /// <summary>
        /// Glyphs converted to bitmaps.
        /// </summary>
        Normal,

        /// <summary>
        /// Glyphs converted to SDF bitmaps.
        /// </summary>
        SDF,

        /// <summary>
        /// Glyphs converted to shapes (vectorized glyphs).
        /// Slower than bitmap methods, but adds possiblity to scale, skew etc glyphs dynamically without loosing sharpness
        /// and paint glyphs with different paint methods (solid, gradients, images).
        /// </summary>
        Shapes
    }

    /// <summary>
    /// GlyphShapeCommandType is converted to NvgPathCommandType
    /// </summary>
    public enum GlyphShapeCommandType
    {
        MoveTo,
        LineTo,
        BezierTo,
        QuadTo,
        Close,
        Winding,
    }

    /// <summary>
    /// IconAlign.
    /// </summary>
    public enum IconAlign
    {
        /// <summary>
        /// Icon on the far left
        /// </summary>
        Left = 0,

        /// <summary>
        /// Icon on the left, centered (depends on caption text length).
        /// </summary>
        LeftCentered = 1,

        /// <summary>
        /// Icon on the right, centered (depends on caption text length).
        /// </summary>
        RightCentered = 2,

        /// <summary>
        /// Icon on the far right.
        /// </summary>
        Right = 3
    }

    /// <summary>
    /// LayoutAlignment is used in layouts.
    /// </summary>
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

    /// <summary>
    /// LineCap.
    /// </summary>
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

    /// <summary>
    /// MessageDialogType determines icon.
    /// </summary>
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

    /// <summary>
    /// NumericFormat determines how many decimals are shown.
    /// </summary>
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

    /// <summary>
    /// PointerType. These are default pointer types used internally,
    /// but type is stored as int so you can set any int value.
    /// Value is passed to screen & user application.
    /// </summary>
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

    /// <summary>
    /// Orientation is primarily used to determine the direction of data flows in layouts.
    /// </summary>
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

    /// <summary>
    /// PopupPosition is popup's position relative to its parent (mostly UIPopupButton).
    /// </summary>
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

    /// <summary>
    /// SeparatorDrawFlag.
    /// </summary>
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

    /// <summary>
    /// ScrollableDragMode.
    /// </summary>
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

    /// <summary>
    /// ScrollbarType.
    /// </summary>
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

    /// <summary>
    /// Solidity determines how shape is interpreted.
    /// </summary>
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


    /// <summary>
    /// TextAlignment is a shortcut to both horizontal & vertical aligment.
    /// </summary>
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

    /// <summary>
    /// TextHorizontalAlign.
    /// </summary>
    /// <remarks>Some widgets have fixed alignment - ie changing property has no effect.</remarks>
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

    /// <summary>
    /// TextVerticalAlign.
    /// </summary>
    /// <remarks>Some widgets have fixed alignment - ie changing property has no effect.</remarks>
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

    /// <summary>
    /// TextureFlags is to deteremine extra texture handling.
    /// </summary>
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

    /// <summary>
    /// TextureFormat.
    /// </summary>
    /// <remarks>You should map these values to your graphics engine's pixel formats.</remarks>
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

    /// <summary>
    /// ViewSelectionMode is used in view widgets.
    /// </summary>
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

    /// <summary>
    /// WidgetState is used basically to get correct widget's background brush.
    /// </summary>
    /// <remarks>Hovered/MouseFocus (with tint color) is combined with these.</remarks>
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

    /// <summary>
    /// Winding for solid shapes (CounterClockwise) & holes (Clockwise).
    /// Note: Normally you should use Winding.CounterClockwise (solid) or Winding.Clockwise (hole).
    /// If you have issues with fills, you could also try setting Winding.Manual.
    /// </summary>
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
