﻿// DragonFruit.Common Copyright 2021 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using NUnit.Framework;

namespace DragonFruit.Common.Data.Tests.Requests
{
    [TestFixture]
    public class RequestFilterTests : ApiTest
    {
        [Test]
        public void TestFilteredRequests()
        {
            Assert.Catch<ArgumentException>(() => Client.Perform(new FilteredRequest()));
            Assert.Catch<ArgumentException>(() => Client.Perform(new InheritedRequest()));
        }
    }
}