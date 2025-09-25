namespace NanoUI.Common
{
    // todo : check Min < Max

    /// <summary>
    /// MinMax.
    /// </summary>
    public struct MinMax
    {
        /// <summary>
        /// Min
        /// </summary>
        public float Min { get; set; }

        /// <summary>
        /// Max
        /// </summary>
        public float Max { get; set; }

        /// <summary>
        /// Ctor.
        /// </summary>
        public MinMax() { }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="min">Min</param>
        /// <param name="max">Max</param>
        public MinMax(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}
