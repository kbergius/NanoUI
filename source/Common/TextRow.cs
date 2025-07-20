namespace NanoUI.Common
{
    public struct TextRow
    {
        // row start in passed text
        public int StartPos;

        // text length in row
        public int TextLength;

        // Logical width of the row.
        public float Width;

        // Actual least X-bound of the row. Logical with and bounds can differ because of kerning and some parts over extending.
        public float MinX;

        // Actual largest X-bound of the row. Logical with and bounds can differ because of kerning and some parts over extending.
        public float MaxX;
    }
}