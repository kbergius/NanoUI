namespace NanoUI.Common
{
    public struct TextRow
    {
        /// <summary>
        /// Row start in passed text
        /// </summary>
        public int StartPos;

        /// <summary>
        /// Text length in row
        /// </summary>
        public int TextLength;

        /// <summary>
        /// Logical width of the row
        /// </summary>
        public float Width;

        /// <summary>
        /// Actual least X-bound of the row. Logical with and bounds can differ
        /// because of kerning and some parts over extending
        /// </summary>
        public float MinX;

        /// <summary>
        /// Actual largest X-bound of the row. Logical with and bounds can differ
        /// because of kerning and some parts over extending.
        /// </summary>
        public float MaxX;
    }
}
