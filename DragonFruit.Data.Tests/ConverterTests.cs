// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DragonFruit.Data.Converters;
using DragonFruit.Data.Requests;
using Xunit;

namespace DragonFruit.Data.Tests
{
    public class ConverterTests
    {
        private const string DefaultParameterName = "property";

        [Theory]
        [InlineData(EnumOption.None, TestEnum.Option3, "Option3")]
        [InlineData(EnumOption.Numeric, TestEnum.Option1 | TestEnum.Option3, "3")]
        [InlineData(EnumOption.StringLower, TestEnum.Option1 | TestEnum.Option3, "option1, option3")]
        [InlineData(EnumOption.StringUpper, TestEnum.Option1 | TestEnum.Option3, "OPTION1, OPTION3")]
        public void TestEnumConverter(EnumOption mode, TestEnum input, string expectedOutput)
        {
            var builder = new StringBuilder();
            EnumConverter.WriteEnum(builder, input, mode, DefaultParameterName);

            if (builder.Length == 0)
            {
                Assert.Fail("Builder was empty");
            }

            builder.Length--; // remove the trailing &
            Assert.Equal($"{DefaultParameterName}={expectedOutput}", builder.ToString());
        }

        [Theory]
        [InlineData(EnumerableOption.Concatenated, $"{DefaultParameterName}=s1,s2,s3,a1,a2")]
        [InlineData(EnumerableOption.Recursive, $"{DefaultParameterName}=s1&{DefaultParameterName}=s2&{DefaultParameterName}=s3&{DefaultParameterName}=a1&{DefaultParameterName}=a2")]
        [InlineData(EnumerableOption.Unordered, $"{DefaultParameterName}[]=s1&{DefaultParameterName}[]=s2&{DefaultParameterName}[]=s3&{DefaultParameterName}[]=a1&{DefaultParameterName}[]=a2")]
        [InlineData(EnumerableOption.Indexed, $"{DefaultParameterName}[0]=s1&{DefaultParameterName}[1]=s2&{DefaultParameterName}[2]=s3&{DefaultParameterName}[3]=a1&{DefaultParameterName}[4]=a2")]
        public void TestEnumerableConverter(EnumerableOption mode, string expectedOutput)
        {
            var builder = new StringBuilder();
            var testData = new[] { "s1", "s2", "s3", "a1", "a2" };

            EnumerableConverter.WriteEnumerable(builder, testData, mode, DefaultParameterName, null);

            if (builder.Length == 0)
            {
                Assert.Fail("Builder was empty");
            }

            builder.Length--; // remove the trailing &
            Assert.Equal(expectedOutput, builder.ToString());

            // test getpairs method to ensure consistent outputs
            var resultPairs = expectedOutput.Split('&').Select(x =>
            {
                var segments = x.Split('=');
                return new KeyValuePair<string, string>(segments[0], segments[1]);
            });

            Assert.True(EnumerableConverter.GetPairs(testData, mode, DefaultParameterName, null).SequenceEqual(resultPairs));
        }

        [Fact]
        public void TestKeyValuePairConverter()
        {
            var pairs = new Dictionary<string, string>
            {
                ["user"] = "test",
                ["user_id"] = "494c0d6dad004a9d8c2d8f086c674a81",
                ["last_login"] = "2020-01-01T00:00:00Z"
            };

            var builder = new StringBuilder();
            KeyValuePairConverter.WriteKeyValuePairs(builder, pairs);

            if (builder.Length == 0)
            {
                Assert.Fail("Builder was empty");
            }

            builder.Length--; // remove the trailing &
            Assert.Equal("user=test&user_id=494c0d6dad004a9d8c2d8f086c674a81&last_login=2020-01-01T00%3A00%3A00Z", builder.ToString());
        }

        [Flags]
        public enum TestEnum
        {
            Option1 = 1,
            Option3 = 2
        }
    }
}
