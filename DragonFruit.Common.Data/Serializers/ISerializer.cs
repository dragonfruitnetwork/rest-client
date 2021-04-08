// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.IO;
using System.Net.Http;
using System.Text;

namespace DragonFruit.Common.Data.Serializers
{
    public interface ISerializer
    {
        public string ContentType { get; }

        public Encoding Encoding { get; set; }

        HttpContent Serialize<T>(T input) where T : class;

        T Deserialize<T>(Stream input) where T : class;
    }
}
