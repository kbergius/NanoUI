namespace NanoUI.Rendering
{
    /// <summary>
    /// (Optional) DrawCallType can be used in rendering,
    /// if some special operation is needed.
    /// </summary>
    public enum DrawCallType
    {
        Fill,
        ConvexFill,
        Stroke,
        Text
    }

    /// <summary>
    /// DrawCommandType should be used in rendering,
    /// when setting correct pipeline settings.
    /// </summary>
    public enum DrawCommandType
    {
        // TriangleFan --> TriangleList - indexed
        FillStencil,

        // TriangleStrip --> TriangleList - indexed
        Fill,

        // TriangleFan/TriangleStrip/TriangleList --> TriangleList - indexed
        // used by ConvexFill, Stroke, Text
        Triangles
    }

    /// <summary>
    /// DrawActionType is used in fragment/pixel shader to determine
    /// the correct action.
    /// </summary>
    public enum DrawActionType : int
    {
        FillGradient = 0,
        FillImage = 1,
        FillStencil = 2,
        Text = 3,
        // this allows to use normal & additional text effects in shader (outline, blur etc)
        TextSDF = 4
    }
}
