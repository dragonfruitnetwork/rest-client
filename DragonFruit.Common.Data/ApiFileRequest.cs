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

        /// <summary>
        /// Length, in bytes, of the buffer used for copying data from the network stream to the file.
        /// Defaults to 32kb
        /// </summary>
        /// <remarks>
        /// This should be less than 85KiB (87040 bytes) to avoid potential memory leaks
        /// </remarks>
        public virtual int BufferSize => 32 * 1024;
    }
}
