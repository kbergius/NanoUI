using System.Numerics;

namespace NanoUI.Svg.Data
{
    // this is used to store "parent" element properties that affect all children element
    // it is stored in "stack"
    internal struct SvgXmlElement
    {
        public string? Id;
        public SvgStyle? Style;
        // transform property
        public Matrix3x2? Transform;

        //public SvgLinearGradient[]? LinearGradients;
        //public SvgRadialGradient[]? RadialGradients;
    }
}
