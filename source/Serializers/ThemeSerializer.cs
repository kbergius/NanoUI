using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NanoUI.Serializers
{
    /// <summary>
    /// ThemeSerializer serializes/deserializes UITheme to/from JSON using various converters.
    /// </summary>
    public class ThemeSerializer
    {
        JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Constructor.
        /// </summary>
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

        /// <summary>
        /// Constructor with user defined JSON options.
        /// </summary>
        /// <param name="jsonOptions">JsonSerializerOptions</param>
        public ThemeSerializer(JsonSerializerOptions jsonOptions)
        {
            _jsonOptions = jsonOptions;
        }

        /// <summary>
        /// Load from stream. T must be UITheme or its extension.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="stream">Stream</param>
        /// <returns>T</returns>
        public T? Load<T>(Stream stream) where T : UITheme
        {
            string json = string.Empty;

            using (var textReader = new StreamReader(stream))
            {
                json = textReader.ReadToEnd();
            }

            return Deserialize<T>(json);
        }

        /// <summary>
        /// Load from stream.
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="type">Type</param>
        /// <returns>object</returns>
        public object? Load(Stream stream, Type type)
        {
            string json = string.Empty;

            using (var textReader = new StreamReader(stream))
            {
                json = textReader.ReadToEnd();
            }

            return Deserialize(json, type);
        }

        /// <summary>
        /// Load from file path. T must be UITheme or its extension.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="filePath">File path</param>
        /// <returns>T</returns>
        public T? Load<T>(string filePath) where T : UITheme
        {
            if (!File.Exists(filePath))
                return default;

            var json = File.ReadAllText(filePath);

            return Deserialize<T>(json);
        }

        /// <summary>
        /// Try load from file path. T must be UITheme or its extension.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="filePath">File path</param>
        /// <param name="theme">Theme</param>
        /// <returns>Success</returns>
        public bool TryLoad<T>(string filePath, out T? theme) where T : UITheme
        {
            if (!File.Exists(filePath))
            {
                theme = default;
                return false;
            }

            var json = File.ReadAllText(filePath);
            theme = Deserialize<T>(json);

            return true;
        }

        /// <summary>
        /// Save to file path. T must be UITheme or its extension.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="filePath">File path</param>
        /// <param name="theme">Theme</param>
        public void Save<T>(string filePath, T theme) where T : UITheme
        {
            var json = Serialize(theme);

            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Serializes the specified theme and returns the json value.
        /// T must be UITheme or its extension.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="theme">Theme</param>
        /// <returns>JSON string</returns>
        public string Serialize<T>(T theme) where T : UITheme
        {
            if (theme == null)
                return string.Empty;

            return JsonSerializer.Serialize(theme, _jsonOptions);
        }

        /// <summary>
        /// Deserializes the JSON content to a specified theme.
        /// T must be UITheme or its extension.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="json">JSON string</param>
        /// <returns>T</returns>
        public T? Deserialize<T>(string json) where T : UITheme
        {
            if (string.IsNullOrEmpty(json))
                return default;

            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }

        /// <summary>
        /// Deserializes the JSON content to a specified object.
        /// </summary>
        /// <param name="json">JSON string</param>
        /// <param name="type">Type</param>
        /// <returns>Object</returns>
        public object? Deserialize(string json, Type type)
        {
            if (string.IsNullOrEmpty(json))
                return default;

            return JsonSerializer.Deserialize(json, type, _jsonOptions);
        }
    }
}
