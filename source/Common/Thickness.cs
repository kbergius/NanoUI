namespace NanoUI.Common
{
    // note: this could be a Vector2, but for possible future use could be extended
    // (separate left/right & top/bottom values)

    /// <summary>
    /// Thickness is used as layouting hints (margins, paddings etc).
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

        /// <summary>
        /// Creates.
        /// </summary>
        /// <param name="val">Horizontal & vertical value</param>
        public Thickness(float val)
        {
            Horizontal = Vertical = val;
        }

        /// <summary>
        /// Creates.
        /// </summary>
        /// <param name="horizontal">Horizontal</param>
        /// <param name="vertical">Vertical</param>
        public Thickness(float horizontal, float vertical)
        {
            Horizontal = horizontal;
            Vertical = vertical;
        }

        /// <summary>
        /// Gets value by index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Value</returns>
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
