//<copyright file="TestCaching.cs" company="OrbintSoft">
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
//<date>2018, 10, 4, 16:57</date>
//<summary></summary>

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS
{
    using FluentAssertions;
    using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS;
    using OrbintSoft.Yauaa.Analyzer.Test.Fixtures;
    using System.Collections.Generic;
    using System.Reflection;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="TestCaching" />
    /// </summary>
    public class TestCaching : IClassFixture<LogFixture>
    {
        /// <summary>
        /// The TestSettingCaching
        /// </summary>
        [Fact]
        public void TestSettingCaching()
        {
            UserAgentAnalyzer uaa = UserAgentAnalyzer
            .NewBuilder()
            .WithCache(42)
            .HideMatcherLoadStats()
            .WithField("AgentUuid")
            .Build();

            uaa.GetCacheSize().Should().Be(42);
            GetAllocatedCacheSize(uaa).Should().BeGreaterOrEqualTo(42);

            uaa.DisableCaching();
            uaa.GetCacheSize().Should().Be(0);
            GetAllocatedCacheSize(uaa).Should().Be(0);

            uaa.SetCacheSize(42);
            uaa.GetCacheSize().Should().Be(42);
            GetAllocatedCacheSize(uaa).Should().BeGreaterOrEqualTo(42);
        }

        /// <summary>
        /// The TestSettingNoCaching
        /// </summary>
        [Fact]
        public void TestSettingNoCaching()
        {
            UserAgentAnalyzer uaa = UserAgentAnalyzer
            .NewBuilder()
            .WithoutCache()
            .HideMatcherLoadStats()
            .WithField("AgentUuid")
            .Build();

            uaa.GetCacheSize().Should().Be(0);
            GetAllocatedCacheSize(uaa).Should().Be(0);

            uaa.SetCacheSize(42);
            uaa.GetCacheSize().Should().Be(42);
            GetAllocatedCacheSize(uaa).Should().BeGreaterOrEqualTo(42);

            uaa.DisableCaching();
            uaa.GetCacheSize().Should().Be(0);
            GetAllocatedCacheSize(uaa).Should().BeGreaterOrEqualTo(0);
        }

        /// <summary>
        /// The TestCache
        /// </summary>
        [Fact]
        public void TestCache()
        {
            string uuid = "11111111-2222-3333-4444-555555555555";
            string fieldName = "AgentUuid";

            UserAgentAnalyzer uaa = UserAgentAnalyzer
            .NewBuilder()
            .WithCache(1)
            .HideMatcherLoadStats()
            .WithField(fieldName)
            .Build();

            UserAgent agent;

            uaa.GetCacheSize().Should().Be(1);
            GetAllocatedCacheSize(uaa).Should().BeGreaterOrEqualTo(1);

            agent = uaa.Parse(uuid);
            agent.Get(fieldName).GetValue().Should().Be(uuid);
            var a1 = agent;
            var a2 = GetCache(uaa)[uuid];
            GetCache(uaa)[uuid].Should().BeEquivalentTo(agent);

            agent = uaa.Parse(uuid);
            agent.Get(fieldName).GetValue().Should().Be(uuid);
            GetCache(uaa)[uuid].Should().BeEquivalentTo(agent);

            uaa.DisableCaching();
            uaa.GetCacheSize().Should().Be(0);
            GetAllocatedCacheSize(uaa).Should().Be(0);

            agent = uaa.Parse(uuid);
            agent.Get(fieldName).GetValue().Should().Be(uuid);
            GetCache(uaa).Should().BeNull();
        }

        /// <summary>
        /// The GetCache
        /// </summary>
        /// <param name="uaa">The uaa<see cref="UserAgentAnalyzer"/></param>
        /// <returns>The <see cref="Dictionary{string, UserAgent}"/></returns>
        private Dictionary<string, UserAgent> GetCache(UserAgentAnalyzer uaa)
        {
            Dictionary<string, UserAgent> actualCache = null;
            var parseCacheProperty = uaa.GetType().GetField("parseCache", BindingFlags.Instance | BindingFlags.NonPublic);

            object rawParseCache = parseCacheProperty.GetValue(uaa);
            if (rawParseCache is Dictionary<string, UserAgent>)
            {
                actualCache = rawParseCache as Dictionary<string, UserAgent>;
            }
            return actualCache;
        }

        /// <summary>
        /// The GetAllocatedCacheSize
        /// </summary>
        /// <param name="uaa">The uaa<see cref="UserAgentAnalyzer"/></param>
        /// <returns>The <see cref="int"/></returns>
        private int GetAllocatedCacheSize(UserAgentAnalyzer uaa)
        {
            var cache = GetCache(uaa);

            if (cache == null)
            {
                return 0;
            }

            var bucketsProperty = cache.GetType().GetField("_buckets", BindingFlags.Instance | BindingFlags.NonPublic);
            var buckets = bucketsProperty.GetValue(cache) as int[];
            return buckets.Length;
        }
    }
}
