namespace NanoUI.Common
{
    // todo: alignments
    public struct ColumnDefinition
    {
        public int Width { get; set; }
        public bool Stretch { get; set; }

        public ColumnDefinition(int width, bool stretch = false)
        {
            Width = width;
            Stretch = stretch;
        }
    }
}