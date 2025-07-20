using System;

namespace NanoUI.Svg.Data
{
    public struct SvgXmlAttribute
    {
        public string Name;
        public object Value;
        public Type AttributeType; // this is how we cast it
        public bool IsPercent;

        // get number value & if is percent value
        public float GetNumber(out bool isPercent)
        {
            isPercent = IsPercent;

            // check null & correct type
            if (Value != null && AttributeType == typeof(float))
            {
                return (float)Value;
            }

            return default;
        }

        public T? GetValue<T>()
        {
            // check null & correct type
            if(Value != null && AttributeType == typeof(T))
            {
                return (T)Value;
            }
            
            return default;
        }
    }
}