// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DragonFruit.Data.Serializers.Newtonsoft
{
    /// <summary>
    /// Lock-enabled file based I/O Methods
    /// </summary>
    public static class FileServices
    {
        public static JsonSerializer DefaultSerializer { get; } = new JsonSerializer();

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
        public static T ReadFileOrDefault<T>(string location, JsonSerializer serializer)
        {
            return File.Exists(location) ? ReadFile<T>(location, serializer) : default;
        }

        /// <summary>
        /// Read data from file as JObject
        /// </summary>
        /// <param name="location">Location of the file</param>
        /// <returns>JObject with data</returns>
        public static JObject ReadFile(string location) => ReadFile(location, JObject.Load);

        /// <summary>
        /// Read data from file as specified type, or return default value if the file doesn't exist
        /// </summary>
        /// <typeparam name="T">Type the data was saved in</typeparam>
        /// <param name="location">Location of the file</param>
        /// <param name="serializer">The <see cref="JsonSerializer"/> to use</param>
        /// <returns>Type with populated data</returns>
        public static T ReadFile<T>(string location, JsonSerializer serializer) => ReadFile(location, serializer.Deserialize<T>);

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
        public static void WriteFile<T>(string location, T data, JsonSerializer serializer)
        {
            lock (location)
            {
                using (var reader = File.Open(location, FileMode.Create))
                using (var textWriter = new StreamWriter(reader))
                using (var jsonWriter = new JsonTextWriter(textWriter))
                {
                    serializer.Serialize(jsonWriter, data);
                }
            }
        }

        private static T ReadFile<T>(string location, Func<JsonTextReader, T> deserializeAction)
        {
            lock (location)
            {
                if (!File.Exists(location))
                {
                    throw new FileNotFoundException($"The File, {Path.GetFileName(location)}, does not exist in directory, {Path.GetDirectoryName(location)}.");
                }

                using (var reader = File.OpenRead(location))
                using (var textReader = new StreamReader(reader))
                using (var jsonReader = new JsonTextReader(textReader))
                {
                    return deserializeAction.Invoke(jsonReader);
                }
            }
        }
    }
}
