// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using DragonFruit.Data.Serializers;
using DragonFruit.Data.Tests.Requests;
using Xunit;

namespace DragonFruit.Data.Tests
{
    public class ClientTests : IDisposable
    {
        private readonly Lazy<FileStream> _fileStream = new(() => new FileStream(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan | FileOptions.DeleteOnClose));

        private readonly ApiClient _client = new ApiClient<ApiJsonSerializer>
        {
            UserAgent = "DragonFruit.Data.Tests"
        };

        [Fact]
        public async Task TestClientVersionDefaults()
        {
            using var response = await _client.PerformAsync("https://cloudflare-quic.com");
            Assert.Equal(RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? HttpVersion.Version11 : HttpVersion.Version30, response.Version);
        }

        [Fact]
        public async Task TestUserAgentHeader()
        {
            var response = await _client.PerformAsync<JsonObject>(new BasicEchoRequest());
            Assert.Equal(_client.UserAgent, response["headers"]?["user-agent"]?.GetValue<string>());
        }

        [Fact]
        public async Task TestClientDeserialization()
        {
            var echoResponse = await _client.PerformAsync<JsonObject>(new InheritedEchoRequest());
            Assert.Equal("additional_content", echoResponse["form"]?["extra"]?.GetValue<string>());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task TestClientDownload(bool safe)
        {
            _fileStream.Value.Seek(0, SeekOrigin.Begin);
            var downloadResult = await _client.PerformDownload(new DummyFileDownloadRequest("5MB"), _fileStream.Value, safe: safe);

            Assert.Equal(HttpStatusCode.OK, downloadResult);
            Assert.Equal(5242880, _fileStream.Value.Length);
        }

        [Fact]
        public async Task TestClientDownloadTruncation()
        {
            _fileStream.Value.Seek(0, SeekOrigin.Begin);
            await _client.PerformDownload(new DummyFileDownloadRequest("10MB"), _fileStream.Value, truncate: true);

            Assert.Equal(10485760, _fileStream.Value.Length);

            _fileStream.Value.Seek(0, SeekOrigin.Begin);
            await _client.PerformDownload(new DummyFileDownloadRequest("5MB"), _fileStream.Value, truncate: true);

            // if truncate worked, the file should have halved in size
            Assert.Equal(5242880, _fileStream.Value.Length);
        }

        [Fact]
        public async Task TestClientDownloadProgressReporting()
        {
            _fileStream.Value.Seek(0, SeekOrigin.Begin);

            var progressHits = 0d;
            var progressHandler = new Progress<(long, long?)>(delegate { progressHits++; });

            var downloadResult = await _client.PerformDownload(new DummyFileDownloadRequest("5MB"), _fileStream.Value, progressHandler);

            Assert.Equal(HttpStatusCode.OK, downloadResult);
            Assert.True(progressHits >= 20);
        }

        public void Dispose()
        {
            if (_fileStream.IsValueCreated)
            {
                _fileStream.Value.Dispose();
            }
        }
    }
}
