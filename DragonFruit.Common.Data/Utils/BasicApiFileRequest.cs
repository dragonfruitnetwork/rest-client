// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using SystemPath = System.IO.Path;

namespace DragonFruit.Common.Data.Utils
{
    public class BasicApiFileRequest : ApiFileRequest
    {
        public BasicApiFileRequest(string path)
            : this(path, SystemPath.GetFileName(path))
        {
        }

        public BasicApiFileRequest(string path, Environment.SpecialFolder baseFolder)
            : this(path, SystemPath.Combine(Environment.GetFolderPath(baseFolder), SystemPath.GetFileName(path)))

        {
        }

        public BasicApiFileRequest(string path, string destination)
        {
            Path = path;

            if (string.IsNullOrEmpty(SystemPath.GetExtension(destination)) && !(destination.StartsWith(".") || destination.EndsWith(".")))
            {
                // we were provided with a path
                Destination = SystemPath.Combine(destination, SystemPath.GetFileName(path));
            }
            else
            {
                Destination = destination;
            }
        }

        public override string Path { get; }
        public override string Destination { get; }
    }
}
