namespace NanoUI.Common
{
    public struct CornerRadius
    {
        public float TopLeft { get; set; }
        public float TopRight { get; set; }
        public float BottomLeft { get; set; }
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
