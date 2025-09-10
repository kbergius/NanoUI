namespace NanoUI.Common
{
    /// <summary>
    /// CornerRadius.
    /// </summary>
    public struct CornerRadius
    {
        /// <summary>
        /// TopLeft.
        /// </summary>
        public float TopLeft { get; set; }

        /// <summary>
        /// TopRight.
        /// </summary>
        public float TopRight { get; set; }

        /// <summary>
        /// BottomLeft.
        /// </summary>
        public float BottomLeft { get; set; }

        /// <summary>
        /// BottomRight.
        /// </summary>
        public float BottomRight { get; set; }

        public CornerRadius(float rounding)
        {
            TopLeft = TopRight = BottomLeft = BottomRight = rounding;
        }

        public CornerRadius(float topLeft, float topRight, float bottomLeft, float bottomRight)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;
        }
    }
}
