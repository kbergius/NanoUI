namespace NanoUI.Rendering.Data
{
    /// <summary>
    /// Draw command to be used when rendering. Renderer should loop all draw commands &
    /// prepare graphics engine before drawing.
    /// </summary>
    public struct DrawCommand
    {
        /// <summary>
        /// DrawCommandType to select correct pipeline settings.
        /// </summary>
        public DrawCommandType DrawCommandType;

        /// <summary>
        /// DrawCallType can be used in rendering, if some special operation is needed (optional).
        /// </summary>
        public DrawCallType DrawCallType;

        /// <summary>
        /// Uniform index.
        /// </summary>
        public int UniformOffset;

        /// <summary>
        /// Texture id.
        /// </summary>
        public int Texture;

        /// <summary>
        /// Indices start index.
        /// </summary>
        public int IndexOffset;

        /// <summary>
        /// Indices count.
        /// </summary>
        public int IndexCount;

        /// <summary>
        /// Vertices start index.
        /// </summary>
        public int VertexOffset;
        
        /// <summary>
        /// Vertices count.
        /// Note: You should not use this when drawing (use indexed drawing),
        /// since it will produce unspecified results.
        /// </summary>
        public int VertexCount;
    }
}
