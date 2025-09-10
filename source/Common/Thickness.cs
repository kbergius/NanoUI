namespace NanoUI.Common
{
    // note: this could be a Vector2, but for possible future use could be extended
    // (separate left/right & top/bottom values)

    /// <summary>
    /// Thickness. Used as layouting hints (margins, paddings etc).
    /// </summary>
    public struct Thickness
    {
        /// <summary>
        /// Horizontal.
        /// </summary>
        public float Horizontal { get; set; }

        /// <summary>
        /// Vertical.
        /// </summary>
        public float Vertical { get; set; }

        public Thickness(float val)
        {
            Horizontal = Vertical = val;
        }

        public Thickness(float horizontal, float vertical)
        {
            Horizontal = horizontal;
            Vertical = vertical;
        }

        public float this[int index]
        {
            get => index == 1? Vertical : Horizontal;
            set
            {
                if(index == 1)
                    Vertical = value;
                else 
                    Horizontal = value;
            }
        }
    }
}
