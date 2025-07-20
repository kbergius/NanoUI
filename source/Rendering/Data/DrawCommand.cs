namespace NanoUI.Rendering.Data
{
    // this is user facing draw command (user should loop all draw commands & render them)
    public struct DrawCommand
    {
        public DrawCommandType DrawCommandType;

        // this can be used in rendering, if some special operation is needed (optional)
        public DrawCallType DrawCallType;

        public int UniformOffset;
        public int Texture;

        public int IndexOffset;
        public int IndexCount;

        // todo not needed by now
        // note: if we set indexes starting in each command from 0, we must use this in real draw call
        public int VertexOffset;
        // todo not needed by now
        public int VertexCount;
    }
}