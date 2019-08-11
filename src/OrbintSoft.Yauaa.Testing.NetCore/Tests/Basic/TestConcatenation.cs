//-----------------------------------------------------------------------
// <copyright file="TestConcatenation.cs" company="OrbintSoft">
//    Yet Another User Agent Analyzer for .NET Standard
//    porting realized by Stefano Balzarotti, Copyright 2018-2019 (C) OrbintSoft
//
//    Original Author and License:
//
//    Yet Another UserAgent Analyzer
//    Copyright(C) 2013-2019 Niels Basjes
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//    https://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//   
// </copyright>
// <author>Stefano Balzarotti, Niels Basjes</author>
// <date>2018, 11, 24, 17:39</date>
// <summary></summary>
//-----------------------------------------------------------------------
namespace OrbintSoft.Yauaa.Testing.Tests.Basic
{
    using FluentAssertions;
    using OrbintSoft.Yauaa.Analyzer;
    using OrbintSoft.Yauaa.Testing.Fixtures;
    using OrbintSoft.Yauaa.Tests;
    using System.IO;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="TestConcatenation" />
    /// </summary>
    public class TestConcatenation : IClassFixture<LogFixture>
    {
        /// <summary>
        /// I create a UserAgent Object forcing custom fields values
        /// </summary>
        /// <returns>The <see cref="UserAgent"/></returns>
        private UserAgent CreateUserAgent()
        {
            var userAgent = new UserAgent();
            userAgent.SetForced("MinusOne", "MinusOne", -1);
            userAgent.SetForced("Zero", "Zero", 0);
            userAgent.SetForced("One", "One", 1);
            userAgent.SetForced("Two", "Two", 2);
            userAgent.SetForced("One Two", "One Two", 12);
            return userAgent;
        }

        /// <summary>
        /// I create an instance of <see cref="UserAgentAnalyzer"/> dropping default resources, and adding a custom yaml definition: AllSteps.yaml.
        /// </summary>
        /// <returns></returns>
        private UserAgentAnalyzer CreateUserAgentAnalyzer()
        {
            return UserAgentAnalyzer.NewBuilder().DropTests().DropDefaultResources().AddResources($"{Config.RESOURCES_PATH}{Path.DirectorySeparatorChar}YamlParsingTests", $"AllSteps.yaml").Build();
        }

        /// <summary>
        /// The 
        /// </summary>
        [Fact]
        public void TestFieldConcatenation()
        {
            var uaa = this.CreateUserAgentAnalyzer();
            var userAgent = this.CreateUserAgent();

            uaa.ConcatFieldValuesNONDuplicated(userAgent, "Combined1", "One", "Two");
            userAgent.GetValue("Combined1").Should().Be("One Two");

            uaa.ConcatFieldValuesNONDuplicated(userAgent, "Combined2", "One", "One");
            userAgent.GetValue("Combined2").Should().Be("One");

            uaa.ConcatFieldValuesNONDuplicated(userAgent, "Combined3", "MinusOne", "One");
            userAgent.GetValue("Combined3").Should().Be("MinusOne One");

            uaa.ConcatFieldValuesNONDuplicated(userAgent, "Combined4", "One", "MinusOne");
            userAgent.GetValue("Combined4").Should().Be("One MinusOne");
        }

        /// <summary>
        /// I test that field concatenation works even if I pass a null field.
        /// </summary>
        [Fact]
        public void TestFieldConcatenationNulls()
        {
            var uaa = this.CreateUserAgentAnalyzer();
            var userAgent = this.CreateUserAgent();

            uaa.ConcatFieldValuesNONDuplicated(userAgent, "Combined3", "MinusOne", null);
            userAgent.GetValue("Combined3").Should().Be("Unknown");

            uaa.ConcatFieldValuesNONDuplicated(userAgent, "Combined4", null, "MinusOne");
            userAgent.GetValue("Combined4").Should().Be("Unknown");

            uaa.ConcatFieldValuesNONDuplicated(userAgent, "Combined3", null, "One");
            userAgent.GetValue("Combined3").Should().Be("One");

            uaa.ConcatFieldValuesNONDuplicated(userAgent, "Combined4", "One", null);
            userAgent.GetValue("Combined4").Should().Be("One");
        }

        /// <summary>
        /// I test that fields concatenation works even if I use the same field name.
        /// </summary>
        [Fact]
        public void TestFieldConcatenationSamePrefix()
        {
            var uaa = this.CreateUserAgentAnalyzer();
            var userAgent = this.CreateUserAgent();

            uaa.ConcatFieldValuesNONDuplicated(userAgent, "Combined1", "One", "Two");
            userAgent.GetValue("Combined1").Should().Be("One Two");

            uaa.ConcatFieldValuesNONDuplicated(userAgent, "Combined2", "One", "One");
            userAgent.GetValue("Combined2").Should().Be("One");

            uaa.ConcatFieldValuesNONDuplicated(userAgent, "Combined3", "One", "One Two");
            userAgent.GetValue("Combined3").Should().Be("One Two");
        }

        /// <summary>
        /// I test that if i try to concatenate a field that does not exist, everything works well
        /// </summary>
        [Fact]
        public void TestFieldConcatenationNonExistent()
        {
            var uaa = this.CreateUserAgentAnalyzer();
            var userAgent = this.CreateUserAgent();

            uaa.ConcatFieldValuesNONDuplicated(userAgent, "Combined2", "One", "NonExistent");
            userAgent.GetValue("Combined2").Should().Be("One");

            uaa.ConcatFieldValuesNONDuplicated(userAgent, "Combined3", "NonExistent", "Two");
            userAgent.GetValue("Combined3").Should().Be("Two");

            uaa.ConcatFieldValuesNONDuplicated(userAgent, "Combined4", "NonExistent1", "NonExistent2");
            userAgent.GetValue("Combined4").Should().Be("Unknown");
        }

        /// <summary>
        /// Another test with null concatenation.
        /// </summary>
        [Fact]
        public void TestFieldConcatenationNull()
        {
            var uaa = this.CreateUserAgentAnalyzer();
            var userAgent = this.CreateUserAgent();

            uaa.ConcatFieldValuesNONDuplicated(userAgent, "Combined2", "One", null);
            userAgent.GetValue("Combined2").Should().Be("One");

            uaa.ConcatFieldValuesNONDuplicated(userAgent, "Combined3", null, "Two");
            userAgent.GetValue("Combined3").Should().Be("Two");

            uaa.ConcatFieldValuesNONDuplicated(userAgent, "Combined4", null, null);
            userAgent.GetValue("Combined4").Should().Be("Unknown");
        }

        /// <summary>
        /// I test that if I concatenate fields without confidence (-1) everything works well.
        /// </summary>
        [Fact]
        public void TestFieldConcatenationNoConfidence()
        {
            var uaa = this.CreateUserAgentAnalyzer();
            var userAgent = this.CreateUserAgent();

            uaa.ConcatFieldValuesNONDuplicated(userAgent, "Combined2", "One", "MinusOne");
            userAgent.GetValue("Combined2").Should().Be("One MinusOne");

            uaa.ConcatFieldValuesNONDuplicated(userAgent, "Combined3", "MinusOne", "Two");
            userAgent.GetValue("Combined3").Should().Be("MinusOne Two");
        }

        /// <summary>
        /// I test that if I cancatenate two field with an unwanted (DeviceClass) everything works.
        /// </summary>
        [Fact]
        public void TestFieldConcatenationUnwanted()
        {
            var uaa = UserAgentAnalyzer.NewBuilder().DropTests().WithField("DeviceClass").Build();
            var userAgent = this.CreateUserAgent();

            uaa.ConcatFieldValuesNONDuplicated(userAgent, "Unwanted", "One", "Two");
            userAgent.GetValue("Unwanted").Should().Be("Unknown");
        }
    }
}
