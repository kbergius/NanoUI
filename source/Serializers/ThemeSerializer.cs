using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NanoUI.Serializers
{
    /// <summary>
    /// Serializes/deserializes UITheme to/from JSON using various converters.
    /// </summary>
    public class ThemeSerializer
    {
        JsonSerializerOptions _jsonOptions;

        public ThemeSerializer()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                IncludeFields = false,// true,
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                Converters =
                {
                    // convert enum to strings - else uses ints
                    new JsonStringEnumConverter(),
                    // Numerics
                    new JsonVector2Converter(),
                    new JsonVector3Converter(),
                    new JsonVector4Converter(),
                    new JsonQuaternionConverter(),
                    new JsonMatrix4x4Converter(),
                    new JsonPlaneConverter(),
                    // base
                    new JsonColorConverter(),
                    new JsonMinMaxConverter(),
                    new JsonThicknessConverter(),
                    new JsonCornerRadiusConverter(),
                }
            };
        }

        public ThemeSerializer(JsonSerializerOptions jsonOptions)
        {
            _jsonOptions = jsonOptions;
        }

        public T? Load<T>(Stream stream) where T : UITheme
        {
            string json = string.Empty;

            using (var textReader = new StreamReader(stream))
            {
                json = textReader.ReadToEnd();
            }

            return Deserialize<T>(json);
        }

        // todo? : all non-generic functions
        public object? Load(Stream stream, Type type)
        {
            string json = string.Empty;

            using (var textReader = new StreamReader(stream))
            {
                json = textReader.ReadToEnd();
            }

            return Deserialize(json, type);
        }

        public T? Load<T>(string filePath) where T : UITheme
        {
            if (!File.Exists(filePath))
                return default;

            var json = File.ReadAllText(filePath);

            return Deserialize<T>(json);
        }

        public bool TryLoad<T>(string filePath, out T? model) where T : UITheme
        {
            if (!File.Exists(filePath))
            {
                model = default;
                return false;
            }

            var json = File.ReadAllText(filePath);
            model = Deserialize<T>(json);

            return true;
        }

        public void Save<T>(string filePath, T model) where T : UITheme
        {
            var json = Serialize(model);

            File.WriteAllText(filePath, json);
        }

        // Serializes the specified object and returns the json value
        public string Serialize<T>(T obj) where T : UITheme
        {
            if (obj == null)
                return string.Empty;

            return JsonSerializer.Serialize(obj, _jsonOptions);
        }

        //  Deserializes the Json content to a specified object
        public T? Deserialize<T>(string json) where T : UITheme
        {
            if (string.IsNullOrEmpty(json))
                return default;

            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }

        public object? Deserialize(string json, Type type)
        {
            if (string.IsNullOrEmpty(json))
                return default;

            return JsonSerializer.Deserialize(json, type, _jsonOptions);
        }
    }
}
