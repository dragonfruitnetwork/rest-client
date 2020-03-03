// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.IO;
using DragonFruit.Common.Storage.File;
using DragonFruit.Common.Storage.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace DragonFruit.Common.Storage.Shared.Tests
{
    [TestClass]
    public class JsonProcessor
    {
        private readonly TestData _data = new TestData();
        private readonly JObject _object;

        private readonly Shared.JsonProcessor _processor;
        private readonly string _tempFile = Path.GetTempFileName();

        //each method tests the extension AND the jsonprocessor class
        public JsonProcessor()
        {
            //write the data
            FileServices.WriteFile(_tempFile, _data);

            //recall it into a JObject and a Processor
            _object = JObject.Parse(System.IO.File.ReadAllText(_tempFile));
            _processor = new Shared.JsonProcessor(_object);
        }

        [TestMethod]
        public void GetByte()
        {
            Assert.AreEqual(_object.GetByte("SmallNumber"), _data.SmallNumber);
            Assert.AreEqual(_processor.GetByte("SmallNumber"), _data.SmallNumber);

            //signed (i.e can go negative)
            Assert.AreEqual(_object.GetSByte("SmallNumber"), Convert.ToSByte(_data.SmallNumber));
            Assert.AreEqual(_processor.GetSByte("SmallNumber"), Convert.ToSByte(_data.SmallNumber));
        }

        [TestMethod]
        public void GetShort()
        {
            Assert.AreEqual(_object.GetShort("Year"), _data.Year);
            Assert.AreEqual(_processor.GetShort("Year"), _data.Year);

            //unsigned
            Assert.AreEqual(_object.GetUShort("Year"), Convert.ToUInt16(_data.Year));
            Assert.AreEqual(_processor.GetUShort("Year"), Convert.ToUInt16(_data.Year));
        }

        [TestMethod]
        public void GetInt()
        {
            Assert.AreEqual(_object.GetInt("LargeNumber"), _data.LargeNumber);
            Assert.AreEqual(_processor.GetInt("LargeNumber"), _data.LargeNumber);

            //unsigned
            Assert.AreEqual(_object.GetUInt("LargeNumber"), Convert.ToUInt32(_data.LargeNumber));
            Assert.AreEqual(_processor.GetUInt("LargeNumber"), Convert.ToUInt32(_data.LargeNumber));
        }

        [TestMethod]
        public void GetLong()
        {
            Assert.AreEqual(_object.GetLong("VeryLargeNumber"), _data.VeryLargeNumber);
            Assert.AreEqual(_processor.GetLong("VeryLargeNumber"), _data.VeryLargeNumber);

            //unsigned
            Assert.AreEqual(_object.GetULong("VeryLargeNumber"), Convert.ToUInt64(_data.VeryLargeNumber));
            Assert.AreEqual(_processor.GetULong("VeryLargeNumber"), Convert.ToUInt64(_data.VeryLargeNumber));
        }

        [TestMethod]
        public void GetDouble()
        {
            Assert.AreEqual(_object.GetDouble("Pi"), _data.Pi);
            Assert.AreEqual(_processor.GetDouble("Pi"), _data.Pi);
        }

        [TestMethod]
        public void GetFloat()
        {
            Assert.AreEqual(_object.GetFloat("Year"), _data.Year);
            Assert.AreEqual(_processor.GetFloat("Year"), _data.Year);
        }

        [TestMethod]
        public void GetDecimal()
        {
            Assert.AreEqual(_object.GetDecimal("Year"), _data.Year);
            Assert.AreEqual(_processor.GetDecimal("Year"), _data.Year);
        }

        [TestMethod]
        public void GetBool()
        {
            Assert.AreEqual(_object.GetBool("IsAlive"), _data.IsAlive);
            Assert.AreEqual(_processor.GetBool("IsAlive"), _data.IsAlive);
        }

        [TestMethod]
        public void GetNull()
        {
            Assert.AreEqual(_object.GetShort("NullValue", 5), 5);
            Assert.AreEqual(_processor.GetShort("NullValue", 5), 5);
        }
    }
}