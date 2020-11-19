// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

namespace DragonFruit.Common.Data.Tests.Files.Objects
{
    /// <summary>
    /// Simple file download request with a path and a destination
    /// </summary>
    internal sealed class FileDownloadRequest : ApiFileRequest
    {
        public FileDownloadRequest()
        {
            if (System.IO.File.Exists(Destination))
            {
                System.IO.File.Delete(Destination);
            }
        }

        public override string Path => "https://github.com/ppy/osu/archive/2020.327.0.zip";

        public override string Destination => System.IO.Path.Combine(System.IO.Path.GetTempPath(), "file.zip");
    }
}
