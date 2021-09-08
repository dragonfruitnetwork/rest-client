// DragonFruit.Common Copyright 2021 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.IO;
using System.Net.Http;
using System.Text;
using DragonFruit.Common.Data.Serializers;
using NUnit.Framework;

namespace DragonFruit.Common.Data.Tests.Serializer
{
    [TestFixture]
    public class SerializerResolverTests : ApiTest
    {
        [SetUp]
        public void Setup()
        {
            SerializerResolver.Register<TestObject, ApiXmlSerializer>();
            SerializerResolver.Register<AnotherTestObject, DummySerializer>();
        }

        [Test]
        public void TestResolution()
        {
            Assert.AreEqual(Client.Serializer.Resolve<TestObject>(DataDirection.In).GetType(), typeof(ApiXmlSerializer));
            Assert.AreEqual(Client.Serializer.Resolve<YetAnotherTestObject>(DataDirection.Out).GetType(), typeof(ApiJsonSerializer));
        }

        [Test]
        public void TestRemovalResolution()
        {
            Assert.AreEqual(Client.Serializer.Resolve<AnotherTestObject>(DataDirection.In).GetType(), typeof(DummySerializer));

            SerializerResolver.Unregister<AnotherTestObject>();

            Assert.AreEqual(Client.Serializer.Resolve<AnotherTestObject>(DataDirection.In).GetType(), typeof(ApiJsonSerializer));
        }
    }

    public class TestObject
    {
    }

    public class AnotherTestObject
    {
    }

    public class YetAnotherTestObject
    {
    }

    public class DummySerializer : ISerializer
    {
        public string ContentType { get; }
        public Encoding Encoding { get; set; }

        public HttpContent Serialize<T>(T input) where T : class
        {
            throw new System.NotImplementedException();
        }

        public T Deserialize<T>(Stream input) where T : class
        {
            throw new System.NotImplementedException();
        }
    }
}
