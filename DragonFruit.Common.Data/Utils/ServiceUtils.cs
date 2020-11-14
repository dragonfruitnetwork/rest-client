// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using Newtonsoft.Json;

namespace DragonFruit.Common.Data.Utils
{
    public static class ServiceUtils
    {
        private static JsonSerializer _serializer;

        public static JsonSerializer DefaultSerializer
        {
            get => _serializer ??= JsonSerializer.CreateDefault();
            set => _serializer = value;
        }
    }
}
