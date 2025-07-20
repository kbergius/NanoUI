namespace NanoUI.Common
{
    // todo : check Min < Max
    public struct MinMax
    {
        public float Min { get; set; }
        public float Max { get; set; }

        public MinMax() { }
        public MinMax(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}