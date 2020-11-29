// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Linq;
using DragonFruit.Common.Data.Parameters;
using NUnit.Framework;

namespace DragonFruit.Common.Data.Tests
{
    [TestFixture]
    public class QueryCompilationTests
    {
        [TestCase]
        public void TestQueries()
        {
            var query = new TestRequest().FullUrl.Split('?').Last().Split('&');

            for (int i = 0; i < TestRequest.TestDataset.Length; i++)
            {
                var testString = TestRequest.TestDataset[i];
                Assert.IsTrue(query.Contains($"{TestRequest.QueryName}={testString}"));
                Assert.IsTrue(query.Contains($"{TestRequest.QueryName}[]={testString}"));
                Assert.IsTrue(query.Contains($"{TestRequest.QueryName}[{i}]={testString}"));
            }

            Assert.IsTrue(query.Contains($"{TestRequest.QueryName}={string.Join(":", TestRequest.TestDataset)}"));
        }
    }

    internal class TestRequest : ApiRequest
    {
        internal const string QueryName = "data";
        internal static string[] TestDataset = { "a", "b", "c" };

        public override string Path => "http://example.com";

        [QueryParameter(QueryName, CollectionConversionMode.Recursive)]
        public string[] RecursiveData { get; set; } = TestDataset;

        [QueryParameter(QueryName, CollectionConversionMode.Ordered)]
        public string[] OrderedData { get; set; } = TestDataset;

        [QueryParameter(QueryName, CollectionConversionMode.Unordered)]
        public string[] UnorderedData { get; set; } = TestDataset;

        [QueryParameter(QueryName, CollectionConversionMode.Concatenated, CollectionSeparator = ":")]
        public string[] ConcatenatedData { get; set; } = TestDataset;
    }
}
