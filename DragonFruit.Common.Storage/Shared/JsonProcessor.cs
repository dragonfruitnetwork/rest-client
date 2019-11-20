// DragonFruit.Common Copyright 2019 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace DragonFruit.Common.Storage.Shared
{
    /// <summary>
    ///     The easy way to extract data from a <see cref="JObject" />/<see cref="JToken" />
    /// </summary>
    public class JsonProcessor
    {
        public JObject Source { get; private set; }

        public JsonProcessor(JObject obj)
        {
            Source = obj;
        }

        public JsonProcessor(JToken obj)
        {
            Source = (JObject) obj;
        }

        public bool IsValid => Source != null;


        public string GetString(string key, string @default = "") => (string) GetBase(key) ?? @default;

        public bool GetBool(string key, bool @default = false) => ((bool?) GetBase(key)).GetValueOrDefault(@default);


        public byte GetByte(string key, byte @default = 0) => ((byte?) GetBase(key)).GetValueOrDefault(@default);

        public short GetShort(string key, short @default = 0) => ((short?) GetBase(key)).GetValueOrDefault(@default);

        public int GetInt(string key, int @default = 0) => ((int?) GetBase(key)).GetValueOrDefault(@default);

        public long GetLong(string key, long @default = 0) => ((long?) GetBase(key)).GetValueOrDefault(@default);


        public sbyte GetSByte(string key, sbyte @default = 0) => ((sbyte?) GetBase(key)).GetValueOrDefault(@default);

        public ushort GetUShort(string key, ushort @default = 0) =>
            ((ushort?) GetBase(key)).GetValueOrDefault(@default);

        public uint GetUInt(string key, uint @default = 0) => ((uint?) GetBase(key)).GetValueOrDefault(@default);

        public ulong GetULong(string key, ulong @default = 0) => ((ulong?) GetBase(key)).GetValueOrDefault(@default);


        public double GetDouble(string key, double @default = 0) =>
            ((double?) GetBase(key)).GetValueOrDefault(@default);

        public float GetFloat(string key, float @default = 0) => ((float?) GetBase(key)).GetValueOrDefault(@default);

        public decimal GetDecimal(string key, decimal @default = 0) =>
            ((decimal?) GetBase(key)).GetValueOrDefault(@default);


        public IEnumerable<T> GetArray<T>(string key)
        {
            try
            {
                return (IEnumerable<T>) GetBase(key);
            }
            catch
            {
                return new List<T>();
            }
        }

        /// <summary>
        ///     Unrecommended method for accessing and casting from <see cref="JObject" />
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key) => GetBase(key).ToObject<T>();

        /// <summary>
        ///     Change the level. This will change the source defined in the constructor and will be lost
        /// </summary>
        /// <param name="key">key of section to drop to</param>
        public void DropLevel(string key)
        {
            Source = (JObject) Source[key];
        }

        /// <summary>
        ///     Change the level. This will change the source defined in the constructor and will be lost
        /// </summary>
        /// <param name="keys">Array of sections to drop (in order of definition)</param>
        public void DropLevel(string[] keys)
        {
            Source = (JObject) Source[keys];
        }

        /// <summary>
        ///     Gets the value as a <see cref="JToken" /> type from <see cref="Source" />, returning null in event of issue.
        /// </summary>
        private JToken GetBase(string key)
        {
            try
            {
                return Source[key];
            }
            catch
            {
                return null;
            }
        }
    }
}