// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DragonFruit.Common.Storage
{
    /// <summary>
    ///     Lock-enabled file based I/O Methods
    /// </summary>
    public static class FileServices
    {
        /// <summary>
        ///     Read data from file as specified type
        /// </summary>
        /// <typeparam name="T">Type the data was saved in</typeparam>
        /// <param name="location">Location of the file</param>
        /// <returns>Type with populated data</returns>
        public static T ReadFile<T>(string location)
        {
            lock (location)
            {
                if (!System.IO.File.Exists(location))
                    throw new FileNotFoundException(
                        $"The File, {Path.GetFileName(location)}, does not exist in directory, {Path.GetDirectoryName(location)}.");

                using (var reader = System.IO.File.OpenRead(location))
                using (var textReader = new StreamReader(reader))
                using (var jsonReader = new JsonTextReader(textReader))
                {
                    return Shared.Newtonsoft.Serializer.Deserialize<T>(jsonReader);
                }
            }
        }

        /// <summary>
        ///     Read data from file as JObject
        /// </summary>
        /// <param name="location">Location of the file</param>
        /// <returns>JObject with data</returns>
        public static JObject ReadFile(string location)
        {
            lock (location)
            {
                if (!System.IO.File.Exists(location))
                    throw new FileNotFoundException(
                        $"The File, {Path.GetFileName(location)}, does not exist in directory, {Path.GetDirectoryName(location)}.");

                using (var reader = System.IO.File.OpenRead(location))
                using (var textReader = new StreamReader(reader))
                using (var jsonReader = new JsonTextReader(textReader))
                {
                    return JObject.Load(jsonReader);
                }
            }
        }

        /// <summary>
        ///     Writes data to a file. If the file exists then it is overwritten with no notice
        /// </summary>
        /// <param name="location">Location of the file</param>
        /// <param name="data">Data to be written</param>
        public static void WriteFile<T>(string location, T data)
        {
            lock (location)
            {
                using (var reader = System.IO.File.Open(location, FileMode.Create))
                using (var textWriter = new StreamWriter(reader))
                using (var jsonWriter = new JsonTextWriter(textWriter))
                {
                    Shared.Newtonsoft.Serializer.Serialize(jsonWriter, data);
                }
            }
        }
    }
}