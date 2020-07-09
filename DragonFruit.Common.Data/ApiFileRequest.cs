// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.IO;

namespace DragonFruit.Common.Data
{
    public abstract class ApiFileRequest : ApiRequest
    {
        protected override Methods Method => Methods.Get;

        public abstract string Destination { get; }

        public virtual FileMode FileCreationMode => FileMode.Create;
    }
}
