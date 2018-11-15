//<copyright file="TestBasics.cs" company="OrbintSoft">
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
//<date>2018, 10, 3, 14:47</date>
//<summary></summary>

using FluentAssertions;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS;
using OrbintSoft.Yauaa.Analyzer.Test.Fixtures;
using Xunit;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS.Analyze
{
    public class TestBasics: IClassFixture<LogFixture>
    {
        [Fact]
        public void TestCacheSetter()
        {
            UserAgentAnalyzer userAgentAnalyzer = UserAgentAnalyzer.NewBuilder().Build();
            userAgentAnalyzer.LoadResources("YamlResources", "*-tests.yaml");

            userAgentAnalyzer.GetCacheSize().Should().Be(10000, "Default cache size");

            userAgentAnalyzer.SetCacheSize(50);
            userAgentAnalyzer.GetCacheSize().Should().Be(50, "I set that size");

            userAgentAnalyzer.SetCacheSize(50000);
            userAgentAnalyzer.GetCacheSize().Should().Be(50000, "I set that size");

            userAgentAnalyzer.SetCacheSize(-5);
            userAgentAnalyzer.GetCacheSize().Should().Be(0, "I set incorrect cache size");

            userAgentAnalyzer.SetCacheSize(50);
            userAgentAnalyzer.GetCacheSize().Should().Be(50, "I set that size");

            userAgentAnalyzer.SetCacheSize(50000);
            userAgentAnalyzer.GetCacheSize().Should().Be(50000, "I set that size");

            userAgentAnalyzer.SetUserAgentMaxLength(555);
            userAgentAnalyzer.GetUserAgentMaxLength().Should().Be(555, "I set that size");
        }

        [Fact]
        public void TestUserAgentMaxLengthSetter()
        {
            UserAgentAnalyzer userAgentAnalyzer = UserAgentAnalyzer.NewBuilder().Build();
            userAgentAnalyzer.LoadResources("YamlResources", "*-tests.yaml");

            userAgentAnalyzer.GetUserAgentMaxLength().Should().Be(UserAgentAnalyzerDirect.DEFAULT_USER_AGENT_MAX_LENGTH, "Default user agent max length");

            userAgentAnalyzer.SetUserAgentMaxLength(250);
            userAgentAnalyzer.GetUserAgentMaxLength().Should().Be(250, "I set that size");

            userAgentAnalyzer.SetUserAgentMaxLength(-100);
            userAgentAnalyzer.GetUserAgentMaxLength().Should().Be(UserAgentAnalyzerDirect.DEFAULT_USER_AGENT_MAX_LENGTH, "I set incorrect cache size"); ;
        }

        [Fact]
        public void TestUseragent()
        {
            string uaString = "Foo Bar";
            UserAgent agent = new UserAgent(uaString);
            agent.Get(UserAgent.USERAGENT).GetValue().Should().Be(uaString);
            agent.Get(UserAgent.USERAGENT).GetConfidence().Should().Be(0);
        }

    }
}
