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

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="rounding">Rounding all corners</param>
        public CornerRadius(float rounding)
        {
            TopLeft = TopRight = BottomLeft = BottomRight = rounding;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="topLeft">TopLeft</param>
        /// <param name="topRight">TopRight</param>
        /// <param name="bottomLeft">BottomLeft</param>
        /// <param name="bottomRight">BottomRight</param>
        public CornerRadius(float topLeft, float topRight, float bottomLeft, float bottomRight)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;
        }
    }
}
