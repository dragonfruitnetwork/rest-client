// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using DragonFruit.Data.Converters;
using DragonFruit.Data.Requests;
using DragonFruit.Data.Tests.Requests;
using Xunit;

namespace DragonFruit.Data.Tests
{
    public class RequestBuildingTests
    {
        [Fact]
        public void TestBasicEchoRequest()
        {
            var request = new BasicEchoRequest();
            using var sourceGenMessage = ((IRequestBuilder)request).BuildRequest(null);
            using var reflectionGenMessage = ReflectionRequestMessageBuilder.CreateHttpRequestMessage(request, null);

            Assert.NotNull(sourceGenMessage.RequestUri);
            Assert.NotNull(reflectionGenMessage.RequestUri);

            // test reflection-generated requests match source-generated ones.
            Assert.Equal(sourceGenMessage.RequestUri, reflectionGenMessage.RequestUri);

            // test query string contains correct parameters
            Assert.Contains("q1=test_query_1", sourceGenMessage.RequestUri.Query);
            Assert.Contains("q2=test_query_2", sourceGenMessage.RequestUri.Query);
            Assert.Contains("q3=test_query_3", sourceGenMessage.RequestUri.Query); // static property
        }
    }
}
