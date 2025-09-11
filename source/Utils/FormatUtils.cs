using NanoUI.Common;
using System.Numerics;

namespace NanoUI.Utils
{
    // this is a class to handle all formatting (basically strings)

    /// <summary>
    /// FormatUtils.
    /// </summary>
    public static class FormatUtils
    {
        public static string GetNumberFormat<T>(T value, NumericFormat numericFormat) where T : INumber<T>
        {
            switch (numericFormat)
            {
                case NumericFormat.Decimal0: return $"{value:0}";
                case NumericFormat.Decimal1: return $"{value:0.0}";
                case NumericFormat.Decimal2: return $"{value:0.00}";
                case NumericFormat.Decimal3: return $"{value:0.000}";
                case NumericFormat.Decimal4: return $"{value:0.0000}";
                case NumericFormat.Decimal5: return $"{value:0.00000}";
                case NumericFormat.Decimal6: return $"{value:0.000000}";
                default: return $"{value}";
            }
        }
    }
}
