//-----------------------------------------------------------------------
// <copyright file="TestBasics.cs" company="OrbintSoft">
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
namespace OrbintSoft.Yauaa.Testing.Tests.Analyze
{
    using FluentAssertions;
    using OrbintSoft.Yauaa.Analyzer;
    using OrbintSoft.Yauaa.Testing.Fixtures;
    using OrbintSoft.Yauaa.Tests;
    using Xunit;

    /// <summary>
    /// Here I do some basic tests on the analyzer
    /// </summary>
    public class TestBasics : IClassFixture<LogFixture>
    {
        /// <summary>
        /// Here I test if I can set the cache size.
        /// I don't check the real size of internal cache, just the property.
        /// </summary>
        [Fact]
        public void TestCacheSetter()
        {
            var userAgentAnalyzer = UserAgentAnalyzer.NewBuilder().Build();
            userAgentAnalyzer.LoadResources(Config.RESOURCES_PATH, "*-tests.yaml");

            userAgentAnalyzer.CacheSize.Should().Be(10000, "Default cache size");

            userAgentAnalyzer.SetCacheSize(50);
            userAgentAnalyzer.CacheSize.Should().Be(50, "I set that size");

            userAgentAnalyzer.SetCacheSize(50000);
            userAgentAnalyzer.CacheSize.Should().Be(50000, "I set that size");

            userAgentAnalyzer.SetCacheSize(-5);
            userAgentAnalyzer.CacheSize.Should().Be(0, "I set incorrect cache size");

            userAgentAnalyzer.SetCacheSize(50);
            userAgentAnalyzer.CacheSize.Should().Be(50, "I set that size");

            userAgentAnalyzer.SetCacheSize(50000);
            userAgentAnalyzer.CacheSize.Should().Be(50000, "I set that size");

            userAgentAnalyzer.SetUserAgentMaxLength(555);
            userAgentAnalyzer.GetUserAgentMaxLength().Should().Be(555, "I set that size");
        }

        /// <summary>
        /// Here I set if I can set the user agent max lenght.
        /// </summary>
        [Fact]
        public void TestUserAgentMaxLengthSetter()
        {
            var userAgentAnalyzer = UserAgentAnalyzer.NewBuilder().Build();
            userAgentAnalyzer.LoadResources(Config.RESOURCES_PATH, "*-tests.yaml");

            userAgentAnalyzer.GetUserAgentMaxLength().Should().Be(UserAgentAnalyzerDirect.DEFAULT_USER_AGENT_MAX_LENGTH, "Default user agent max length");

            userAgentAnalyzer.SetUserAgentMaxLength(250);
            userAgentAnalyzer.GetUserAgentMaxLength().Should().Be(250, "I set that size");

            userAgentAnalyzer.SetUserAgentMaxLength(-100);
            userAgentAnalyzer.GetUserAgentMaxLength().Should().Be(UserAgentAnalyzerDirect.DEFAULT_USER_AGENT_MAX_LENGTH, "I set incorrect cache size"); ;
        }
    }
}
