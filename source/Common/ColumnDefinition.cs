namespace NanoUI.Common
{
    // todo: alignments

    /// <summary>
    /// ColumnDefinition.
    /// </summary>
    public struct ColumnDefinition
    {
        /// <summary>
        /// Width.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Stretch.
        /// </summary>
        public bool Stretch { get; set; }

        /// <summary>
        /// Ctor,
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="stretch">Stretch</param>
        public ColumnDefinition(int width, bool stretch = false)
        {
            Width = width;
            Stretch = stretch;
        }
    }
}
