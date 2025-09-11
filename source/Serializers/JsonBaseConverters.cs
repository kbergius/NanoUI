using NanoUI.Common;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;


namespace NanoUI.Serializers
{
    #region IntArray

    /// <summary>
    /// IntArraySerializer.
    /// </summary>
    internal static class IntArraySerializer
    {
        public static int[] Read(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            var ints = new List<int>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                ints.Add(JsonSerializer.Deserialize<int>(ref reader, options));
            }

            return ints.ToArray();
        }

        public static void Write(int[] ints, Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            for (int i = 0; i < ints.Length; i++)
            {
                JsonSerializer.Serialize(writer, ints[i], options);
            }

            writer.WriteEndArray();
        }
    }
    #endregion

    #region StringArray

    /// <summary>
    /// StringArraySerializer.
    /// </summary>
    internal static class StringArraySerializer
    {
        public static string[] Read(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            string? val = JsonSerializer.Deserialize<string>(ref reader, options);

            if (!string.IsNullOrEmpty(val))
            {
                string[] res = val.Split(",");

                for(int i = 0; i < res.Length; i++) 
                {
                    res[i] = res[i].Trim();
                }

                return res;
            }

            return Array.Empty<string>();
        }

        public static void Write(string[] strings, Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            //writer.WriteStartArray();

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < strings.Length; i++)
            {
                if(i > 0) 
                { 
                    sb.Append(", ");
                }
                sb.Append(strings[i]);
            }

            JsonSerializer.Serialize(writer, sb.ToString(), options);

            //writer.WriteEndArray();
        }
    }
    #endregion

    #region Range

    /// <summary>
    /// JsonRangeConverter.
    /// </summary>
    public class JsonRangeConverter : JsonConverter<MinMax>
    {
        public override MinMax Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string[] res = StringArraySerializer.Read(ref reader, options);

            if (res.Length != 2)
            {
                throw new Exception("Range should have 2 elements. Got " + res.Length);
            }

            if(float.TryParse(res[0], out var min) && 
               float.TryParse(res[1], out var max))
            {
                return new MinMax(min, max);
            }

            return default;
            
        }

        public override void Write(Utf8JsonWriter writer, MinMax value, JsonSerializerOptions options)
        {
            string[] range = [value.Min.ToString(), value.Max.ToString()];

            StringArraySerializer.Write(range, writer, options);
        }
    }

    #endregion

    #region Color

    /// <summary>
    /// JsonColorConverter.
    /// </summary>
    public class JsonColorConverter : JsonConverter<Color>
    {
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string[] res = StringArraySerializer.Read(ref reader, options);

            if (res.Length != 4)
            {
                throw new Exception("Color should have 4 elements. Got " + res.Length);
            }

            if (byte.TryParse(res[0], out var r) && 
                byte.TryParse(res[1], out var g) &&
                byte.TryParse(res[2], out var b) &&
                byte.TryParse(res[3], out var a))
            {
                return new Color(r, g, b, a);
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            string[] color =
            [
                value.R.ToString(),
                value.G.ToString(),
                value.B.ToString(),
                value.A.ToString()
            ];

            StringArraySerializer.Write(color, writer, options);
        }
    }

    #endregion

    #region Thickness

    /// <summary>
    /// JsonPaddingConverter.
    /// </summary>
    public class JsonPaddingConverter : JsonConverter<Thickness>
    {
        public override Thickness Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string[] res = StringArraySerializer.Read(ref reader, options);

            if (res.Length != 2)
            {
                throw new Exception("Thickness should have 2 elements. Got " + res.Length);
            }

            if (float.TryParse(res[0], out var horizontal) &&
                float.TryParse(res[1], out var vertical))
            {
                return new Thickness(horizontal, vertical);
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, Thickness value, JsonSerializerOptions options)
        {
            string[] paddings =
            [
                value.Horizontal.ToString(),
                value.Vertical.ToString(),
            ];

            StringArraySerializer.Write(paddings, writer, options);
        }
    }

    #endregion

    #region CornerRadius

    /// <summary>
    /// JsonCornerRadiusConverter.
    /// </summary>
    public class JsonCornerRadiusConverter : JsonConverter<CornerRadius>
    {
        public override CornerRadius Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string[] res = StringArraySerializer.Read(ref reader, options);

            if (res.Length != 4)
            {
                throw new Exception("UICornerRadius should have 4 elements. Got " + res.Length);
            }

            if (float.TryParse(res[0], out var topLeft) &&
                float.TryParse(res[1], out var topRight) &&
                float.TryParse(res[2], out var bottomLeft) &&
                float.TryParse(res[3], out var bottomRight))
            {
                return new CornerRadius(topLeft, topRight, bottomLeft, bottomRight);
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, CornerRadius value, JsonSerializerOptions options)
        {
            string[] radiuses =
            [
                value.TopLeft.ToString(),
                value.TopRight.ToString(),
                value.BottomLeft.ToString(),
                value.BottomRight.ToString()
            ];

            StringArraySerializer.Write(radiuses, writer, options);
        }
    }

    #endregion
}
