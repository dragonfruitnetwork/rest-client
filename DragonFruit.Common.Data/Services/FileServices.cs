// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.IO;
using DragonFruit.Common.Data.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DragonFruit.Common.Data.Services
{
    /// <summary>
    /// Lock-enabled file based I/O Methods
    /// </summary>
    public static class FileServices
    {
        /// <summary>
        /// Read data from file as specified type
        /// </summary>
        /// <typeparam name="T">Type the data was saved in</typeparam>
        /// <param name="location">Location of the file</param>
        /// <returns>Type with populated data</returns>
        public static T ReadFile<T>(string location) => ReadFile<T>(location, ServiceUtils.DefaultSerialiser);

        /// <summary>
        /// Read data from file as specified type, or return default value if the file doesn't exist
        /// </summary>
        /// <typeparam name="T">Type the data was saved in</typeparam>
        /// <param name="location">Location of the file</param>
        /// <returns>Type with populated data</returns>
        public static T ReadFileOrDefault<T>(string location)
        {
            return ReadFileOrDefault<T>(location, ServiceUtils.DefaultSerialiser);
        }

        /// <summary>
        /// Read data from file as specified type, or return default value if the file doesn't exist
        /// </summary>
        /// <typeparam name="T">Type the data was saved in</typeparam>
        /// <param name="location">Location of the file</param>
        /// <param name="serialiser">The <see cref="JsonSerializer"/> to use</param>
        /// <returns>Type with populated data</returns>
        public static T ReadFileOrDefault<T>(string location, JsonSerializer serialiser)
        {
            return File.Exists(location) ? ReadFile<T>(location, serialiser) : default;
        }

        /// <summary>
        /// Read data from file as specified type, or return default value if the file doesn't exist
        /// </summary>
        /// <typeparam name="T">Type the data was saved in</typeparam>
        /// <param name="location">Location of the file</param>
        /// <param name="serialiser">The <see cref="JsonSerializer"/> to use</param>
        /// <returns>Type with populated data</returns>
        public static T ReadFile<T>(string location, JsonSerializer serialiser)
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
                    return serialiser.Deserialize<T>(jsonReader);
                }
            }
        }

        /// <summary>
        /// Read data from file as JObject
        /// </summary>
        /// <param name="location">Location of the file</param>
        /// <returns>JObject with data</returns>
        public static JObject ReadFile(string location)
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
                    return JObject.Load(jsonReader);
                }
            }
        }

        /// <summary>
        /// Writes data to a file. If the file exists then it is overwritten with no notice
        /// </summary>
        /// <param name="location">Location of the file</param>
        /// <param name="data">Data to be written</param>
        public static void WriteFile<T>(string location, T data) => WriteFile(location, data, ServiceUtils.DefaultSerialiser);

        /// <summary>
        /// Writes data to a file. If the file exists then it is overwritten with no notice
        /// </summary>
        /// <param name="location">Location of the file</param>
        /// <param name="data">Data to be written</param>
        /// <param name="serialiser">The <see cref="JsonSerializer"/> to use</param>
        public static void WriteFile<T>(string location, T data, JsonSerializer serialiser)
        {
            lock (location)
            {
                using (var reader = File.Open(location, FileMode.Create))
                using (var textWriter = new StreamWriter(reader))
                using (var jsonWriter = new JsonTextWriter(textWriter))
                {
                    serialiser.Serialize(jsonWriter, data);
                }
            }
        }
    }
}
