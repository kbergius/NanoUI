namespace NanoUI.Rendering
{
    // this is enum spesifing internal draw call, that is used to create user facing draw command(s)
    // it can also be used in rendering, if some special operation is needed (optional)
    public enum DrawCallType
    {
        Fill,
        ConvexFill,
        Stroke,
        Text
    }

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

    // this is used in shader to determine the draw action (stored in fragment uniform)
    // note: this could be public & set directly to uniform, because it converts into int in the fragment shader
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
