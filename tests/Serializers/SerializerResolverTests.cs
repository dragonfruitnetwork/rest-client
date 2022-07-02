// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.IO;
using System.Net.Http;
using DragonFruit.Data.Serializers;
using DragonFruit.Data.Serializers.Newtonsoft;
using NUnit.Framework;

namespace DragonFruit.Data.Tests.Serializers
{
    [TestFixture]
    public class SerializerResolverTests : ApiTest
    {
        [SetUp]
        public void Setup()
        {
            // testobject will use xml
            SerializerResolver.Register<TestObject, ApiXmlSerializer>();

            // anothertestobject will use the dummyserializer
            SerializerResolver.Register<AnotherTestObject, DummySerializer>();
        }

        [Test]
        public void TestResolution()
        {
            Assert.AreEqual(typeof(ApiXmlSerializer), Client.Serializer.Resolve<TestObject>(DataDirection.In).GetType());
            Assert.AreEqual(typeof(ApiJsonSerializer), Client.Serializer.Resolve<YetAnotherTestObject>(DataDirection.Out).GetType());
        }

        [Test]
        public void TestRemovalResolution()
        {
            Assert.AreEqual(typeof(DummySerializer), Client.Serializer.Resolve<AnotherTestObject>(DataDirection.In).GetType());

            SerializerResolver.Unregister<AnotherTestObject>();
            Assert.AreEqual(typeof(ApiJsonSerializer), Client.Serializer.Resolve<AnotherTestObject>(DataDirection.In).GetType());
        }

        [Test]
        public void TestConfigure()
        {
            var a = string.Empty;
            Client.Serializer.Configure<DummySerializer>(o => a = o.ContentType);
            Assert.AreEqual("nothing", a);
        }

        [TearDown]
        public void Cleanup()
        {
            SerializerResolver.Unregister<TestObject>();
            SerializerResolver.Unregister<AnotherTestObject>();
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

    public class DummySerializer : ApiSerializer
    {
        public override string ContentType => "nothing";
        public override bool IsGeneric => true;

        public override HttpContent Serialize(object input) => throw new System.NotImplementedException();

        public override T Deserialize<T>(Stream input) where T : class => throw new System.NotImplementedException();
    }
}
