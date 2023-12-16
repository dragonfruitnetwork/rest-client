// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

namespace DragonFruit.Data.Tests.Requests
{
    public partial class DummyFileDownloadRequest : ApiRequest
    {
        public override string RequestPath => $"http://xcal1.vodafone.co.uk/{FileSize}.zip";

        public DummyFileDownloadRequest(string fileSize)
        {
            FileSize = fileSize;
        }

        public string FileSize { get; set; }
    }
}
