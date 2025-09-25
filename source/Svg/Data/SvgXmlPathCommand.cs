namespace NanoUI.Svg.Data
{
    /// <summary>
    /// Svg xml path command.
    /// </summary>
    public struct SvgXmlPathCommand
    {
        /// <summary>
        /// SvgXmlPathCommandType
        /// </summary>
        public SvgXmlPathCommandType CommandType;

        /// <summary>
        /// All values
        /// </summary>
        public float[] Values;

        /// <summary>
        /// Are values absolute or relative.
        /// </summary>
        public bool Absolute;
    }
}
