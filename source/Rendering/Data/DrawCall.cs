namespace NanoUI.Rendering.Data
{
    // this is internal draw call info that is used to create draw command(s) for the user
    internal struct DrawCall
    {
        public DrawCallType Type;

        public int Uniform1;
        public int Uniform2;

        public int VertexOffset;
        public int VertexCount;

        public int FillStrokeCount;
        public int FillStrokeOffset;

        public int Texture;
    }
}