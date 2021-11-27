// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.IO;

namespace DragonFruit.Common.Data
{
    public abstract class ApiFileRequest : ApiRequest
    {
        /// <summary>
        /// The location, on the disk, to put the resultant file
        /// </summary>
        public abstract string Destination { get; }

        /// <summary>
        /// The mode of file creation
        /// </summary>
        public virtual FileMode FileCreationMode => FileMode.Create;
    }
}
