using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Numerics;

namespace NanoUI.Serializers
{
    internal static class FloatArraySerializer
    {
        public static float[] Read(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            var floats = new List<float>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;
                floats.Add(JsonSerializer.Deserialize<float>(ref reader, options));
            }

            return floats.ToArray();
        }

        public static void Write(float[] floats, Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            for (int i = 0; i < floats.Length; i++)
            {
                JsonSerializer.Serialize(writer, floats[i], options);
            }

            writer.WriteEndArray();
        }
    }

    // Vector2
    public class JsonVector2Converter : JsonConverter<Vector2>
    {
        public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var floats = FloatArraySerializer.Read(ref reader, options);

            return new Vector2(floats[0], floats[1]);
        }

        public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
        {
            FloatArraySerializer.Write(value.ToArray(), writer, options);
        }
    }

    // Vector3
    public class JsonVector3Converter : JsonConverter<Vector3>
    {
        public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var floats = FloatArraySerializer.Read(ref reader, options);

            return new Vector3(floats[0], floats[1], floats[2]);
        }

        public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
        {
            FloatArraySerializer.Write(value.ToArray(), writer, options);
        }
    }

    // Vector4
    public class JsonVector4Converter : JsonConverter<Vector4>
    {
        public override Vector4 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var floats = FloatArraySerializer.Read(ref reader, options);

            return new Vector4(floats[0], floats[1], floats[2], floats[3]);
        }

        public override void Write(Utf8JsonWriter writer, Vector4 value, JsonSerializerOptions options)
        {
            FloatArraySerializer.Write(value.ToArray(), writer, options);
        }
    }

    // Quaternion
    public class JsonQuaternionConverter : JsonConverter<Quaternion>
    {
        public override Quaternion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var floats = FloatArraySerializer.Read(ref reader, options);

            return new Quaternion(floats[0], floats[1], floats[2], floats[3]);
        }

        public override void Write(Utf8JsonWriter writer, Quaternion value, JsonSerializerOptions options)
        {
            FloatArraySerializer.Write(value.ToArray(), writer, options);
        }
    }

    // Matrix4x4
    public class JsonMatrix4x4Converter : JsonConverter<Matrix4x4>
    {
        public override Matrix4x4 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var floats = FloatArraySerializer.Read(ref reader, options);

            return new Matrix4x4(
                floats[0], floats[1], floats[2], floats[3],
                floats[4], floats[5], floats[6], floats[7],
                floats[8], floats[9], floats[10], floats[11],
                floats[12], floats[13], floats[14], floats[15]);
        }

        public override void Write(Utf8JsonWriter writer, Matrix4x4 value, JsonSerializerOptions options)
        {
            FloatArraySerializer.Write(value.ToArray(), writer, options);
        }
    }

    // Plane
    public class JsonPlaneConverter : JsonConverter<Plane>
    {
        public override Plane Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var floats = FloatArraySerializer.Read(ref reader, options);

            return new Plane(floats[0], floats[1], floats[2], floats[3]);
        }

        public override void Write(Utf8JsonWriter writer, Plane value, JsonSerializerOptions options)
        {
            FloatArraySerializer.Write(value.ToArray(), writer, options);
        }
    }
}