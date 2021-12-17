// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.IO;
using System.Text.Json;

namespace DragonFruit.Data.Serializers.SystemJson
{
    /// <summary>
    /// Lock-enabled file based I/O Methods
    /// </summary>
    public static class FileServices
    {
        public static JsonSerializerOptions DefaultSerializer { get; set; }

        /// <summary>
        /// Read data from file as specified type
        /// </summary>
        /// <typeparam name="T">Type the data was saved in</typeparam>
        /// <param name="location">Location of the file</param>
        /// <returns>Type with populated data</returns>
        public static T ReadFile<T>(string location)
        {
            return ReadFile<T>(location, DefaultSerializer);
        }

        /// <summary>
        /// Read data from file as specified type, or return default value if the file doesn't exist
        /// </summary>
        /// <typeparam name="T">Type the data was saved in</typeparam>
        /// <param name="location">Location of the file</param>
        /// <returns>Type with populated data</returns>
        public static T ReadFileOrDefault<T>(string location)
        {
            return ReadFileOrDefault<T>(location, DefaultSerializer);
        }

        /// <summary>
        /// Read data from file as specified type, or return default value if the file doesn't exist
        /// </summary>
        /// <typeparam name="T">Type the data was saved in</typeparam>
        /// <param name="location">Location of the file</param>
        /// <param name="serializer">The <see cref="JsonSerializer"/> to use</param>
        /// <returns>Type with populated data</returns>
        public static T ReadFileOrDefault<T>(string location, JsonSerializerOptions serializer)
        {
            return File.Exists(location) ? ReadFile<T>(location, serializer) : default;
        }

        /// <summary>
        /// Read data from file as a <see cref="JsonDocument"/>
        /// </summary>
        /// <param name="location">Location of the file</param>
        /// <returns><see cref="JsonDocument"/> with data</returns>
        public static JsonDocument ReadFile(string location) => ReadFile(location, s => JsonDocument.Parse(s));

        /// <summary>
        /// Read data from file as specified type, or return default value if the file doesn't exist
        /// </summary>
        /// <typeparam name="T">Type the data was saved in</typeparam>
        /// <param name="location">Location of the file</param>
        /// <param name="serializer">The <see cref="JsonSerializer"/> to use</param>
        /// <returns>Type with populated data</returns>
        public static T ReadFile<T>(string location, JsonSerializerOptions serializer) => ReadFile(location, s => JsonSerializer.Deserialize<T>(s, serializer));

        /// <summary>
        /// Writes data to a file. If the file exists then it is overwritten with no notice
        /// </summary>
        /// <param name="location">Location of the file</param>
        /// <param name="data">Data to be written</param>
        public static void WriteFile<T>(string location, T data) => WriteFile(location, data, DefaultSerializer);

        /// <summary>
        /// Writes data to a file. If the file exists then it is overwritten with no notice
        /// </summary>
        /// <param name="location">Location of the file</param>
        /// <param name="data">Data to be written</param>
        /// <param name="serializer">The <see cref="JsonSerializer"/> to use</param>
        public static void WriteFile<T>(string location, T data, JsonSerializerOptions serializer)
        {
            lock (location)
            {
                using var writer = File.Open(location, FileMode.Create);
                JsonSerializer.Serialize(writer, data, serializer);
            }
        }

        private static T ReadFile<T>(string location, Func<Stream, T> deserializeAction)
        {
            lock (location)
            {
                if (!File.Exists(location))
                {
                    throw new FileNotFoundException($"The File, {Path.GetFileName(location)}, does not exist in directory, {Path.GetDirectoryName(location)}.");
                }

                using var reader = File.OpenRead(location);
                return deserializeAction.Invoke(reader);
            }
        }
    }
}
