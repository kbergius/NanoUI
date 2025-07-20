namespace NanoUI.Svg.Data
{
    public struct SvgXmlPathCommand
    {
        public SvgXmlPathCommandType CommandType;
        // collect all values
        public float[] Values;
        // are values absolute/relative
        public bool Absolute;
    }
}