using System;

namespace NanoUI.Svg.Data
{
    /// <summary>
    /// Svg xml attribute
    /// </summary>
    public struct SvgXmlAttribute
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name;

        /// <summary>
        /// Value
        /// </summary>
        public object Value;

        /// <summary>
        /// AttributeType tells how we cast value.
        /// </summary>
        public Type AttributeType;

        /// <summary>
        /// Is percent?
        /// </summary>
        public bool IsPercent;

        /// <summary>
        /// Returns number value and flag if value was percent.
        /// </summary>
        /// <param name="isPercent">Is percent?</param>
        /// <returns>number value</returns>
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

        /// <summary>
        /// Returns value in type of T or default, if conversion can't be made.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>type of T or default, if conversion can't be made</returns>
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
