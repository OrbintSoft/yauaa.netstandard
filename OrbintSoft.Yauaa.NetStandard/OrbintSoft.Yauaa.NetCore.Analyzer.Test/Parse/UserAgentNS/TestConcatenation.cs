//<copyright file="TestConcatenation.cs" company="OrbintSoft">
//	Yet Another UserAgent Analyzer.NET Standard
//	Porting realized by Stefano Balzarotti, Copyright (C) OrbintSoft
//
//	Original Author and License:
//
//	Yet Another UserAgent Analyzer
//	Copyright(C) 2013-2018 Niels Basjes
//
//	Licensed under the Apache License, Version 2.0 (the "License");
//	you may not use this file except in compliance with the License.
//	You may obtain a copy of the License at
//
//	https://www.apache.org/licenses/LICENSE-2.0
//
//	Unless required by applicable law or agreed to in writing, software
//	distributed under the License is distributed on an "AS IS" BASIS,
//	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//	See the License for the specific language governing permissions and
//	limitations under the License.
//
//</copyright>
//<author>Stefano Balzarotti, Niels Basjes</author>
//<date>2018, 11, 14, 20:22</date>
//<summary></summary>

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS
{
    using FluentAssertions;
    using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS;
    using OrbintSoft.Yauaa.Analyzer.Test.Fixtures;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="TestConcatenation" />
    /// </summary>
    public class TestConcatenation : IClassFixture<LogFixture>
    {
        /// <summary>
        /// The CreateUserAgent
        /// </summary>
        /// <returns>The <see cref="UserAgent"/></returns>
        private UserAgent CreateUserAgent()
        {
            UserAgent userAgent = new UserAgent();
            userAgent.SetForced("MinusOne", "MinusOne", -1);
            userAgent.SetForced("Zero", "Zero", 0);
            userAgent.SetForced("One", "One", 1);
            userAgent.SetForced("Two", "Two", 2);
            userAgent.SetForced("One Two", "One Two", 12);
            return userAgent;
        }

        /// <summary>
        /// The TestFieldConcatenation
        /// </summary>
        [Fact]
        public void TestFieldConcatenation()
        {
            UserAgentAnalyzer uaa = UserAgentAnalyzer.NewBuilder().DropTests().DropDefaultResources().Build();
            UserAgent userAgent = CreateUserAgent();

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
        /// The TestFieldConcatenationNulls
        /// </summary>
        [Fact]
        public void TestFieldConcatenationNulls()
        {
            UserAgentAnalyzer uaa = UserAgentAnalyzer.NewBuilder().DropTests().DropDefaultResources().Build();
            UserAgent userAgent = CreateUserAgent();

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
        /// The TestFieldConcatenationSamePrefix
        /// </summary>
        [Fact]
        public void TestFieldConcatenationSamePrefix()
        {
            UserAgentAnalyzer uaa = UserAgentAnalyzer.NewBuilder().DropTests().DropDefaultResources().Build();
            UserAgent userAgent = CreateUserAgent();

            uaa.ConcatFieldValuesNONDuplicated(userAgent, "Combined1", "One", "Two");
            userAgent.GetValue("Combined1").Should().Be("One Two");

            uaa.ConcatFieldValuesNONDuplicated(userAgent, "Combined2", "One", "One");
            userAgent.GetValue("Combined2").Should().Be("One");

            uaa.ConcatFieldValuesNONDuplicated(userAgent, "Combined3", "One", "One Two");
            userAgent.GetValue("Combined3").Should().Be("One Two");
        }

        /// <summary>
        /// The TestFieldConcatenationNonExistent
        /// </summary>
        [Fact]
        public void TestFieldConcatenationNonExistent()
        {
            UserAgentAnalyzer uaa = UserAgentAnalyzer.NewBuilder().DropTests().DropDefaultResources().Build();
            UserAgent userAgent = CreateUserAgent();

            uaa.ConcatFieldValuesNONDuplicated(userAgent, "Combined2", "One", "NonExistent");
            userAgent.GetValue("Combined2").Should().Be("One");

            uaa.ConcatFieldValuesNONDuplicated(userAgent, "Combined3", "NonExistent", "Two");
            userAgent.GetValue("Combined3").Should().Be("Two");

            uaa.ConcatFieldValuesNONDuplicated(userAgent, "Combined4", "NonExistent1", "NonExistent2");
            userAgent.GetValue("Combined4").Should().Be("Unknown");
        }

        /// <summary>
        /// The TestFieldConcatenationNull
        /// </summary>
        [Fact]
        public void TestFieldConcatenationNull()
        {
            UserAgentAnalyzer uaa = UserAgentAnalyzer.NewBuilder().DropTests().DropDefaultResources().Build();
            UserAgent userAgent = CreateUserAgent();

            uaa.ConcatFieldValuesNONDuplicated(userAgent, "Combined2", "One", null);
            userAgent.GetValue("Combined2").Should().Be("One");

            uaa.ConcatFieldValuesNONDuplicated(userAgent, "Combined3", null, "Two");
            userAgent.GetValue("Combined3").Should().Be("Two");

            uaa.ConcatFieldValuesNONDuplicated(userAgent, "Combined4", null, null);
            userAgent.GetValue("Combined4").Should().Be("Unknown");
        }

        /// <summary>
        /// The TestFieldConcatenationNoConfidence
        /// </summary>
        [Fact]
        public void TestFieldConcatenationNoConfidence()
        {
            UserAgentAnalyzer uaa = UserAgentAnalyzer.NewBuilder().DropTests().DropDefaultResources().Build();
            UserAgent userAgent = CreateUserAgent();

            uaa.ConcatFieldValuesNONDuplicated(userAgent, "Combined2", "One", "MinusOne");
            userAgent.GetValue("Combined2").Should().Be("One MinusOne");

            uaa.ConcatFieldValuesNONDuplicated(userAgent, "Combined3", "MinusOne", "Two");
            userAgent.GetValue("Combined3").Should().Be("MinusOne Two");
        }

        /// <summary>
        /// The TestFieldConcatenationUnwanted
        /// </summary>
        [Fact]
        public void TestFieldConcatenationUnwanted()
        {
            UserAgentAnalyzer uaa = UserAgentAnalyzer.NewBuilder().DropTests().WithField("DeviceClass").Build();
            UserAgent userAgent = CreateUserAgent();

            uaa.ConcatFieldValuesNONDuplicated(userAgent, "Unwanted", "One", "Two");
            userAgent.GetValue("Unwanted").Should().Be("Unknown");
        }
    }
}
