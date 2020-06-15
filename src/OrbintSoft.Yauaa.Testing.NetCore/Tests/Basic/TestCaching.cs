//-----------------------------------------------------------------------
// <copyright file="TestCaching.cs" company="OrbintSoft">
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
    using System.Collections.Generic;
    using System.Reflection;
    using Xunit;

    /// <summary>
    /// This class tests the UA Parser cache, this is an adaption of the Java libary cache mechanism.
    /// In .NET list caching works differently, I am not sure this tests are still valid, I should inestigate more the performance with benchmarking to do a proper tuning.
    /// </summary>
    public class TestCaching : IClassFixture<LogFixture>
    {
        /// <summary>
        /// I test that I can set cache to a proper value.
        /// Since the cache is managed with dictionary allocation, we deal with .NET allocated dictionary size.
        /// .NET sets the dictionary allocated size to the first prime number greater than the value passed.
        /// I need to investigate if I can implement a better caching mechanism.
        /// </summary>
        [Fact]
        public void TestSettingCaching()
        {
            var uaa = UserAgentAnalyzer
            .NewBuilder()
            .WithCache(42)
            .HideMatcherLoadStats()
            .WithField("AgentUuid")
            .Build();

            uaa.CacheSize.Should().Be(42);
            this.GetAllocatedCacheSize(uaa).Should().BeGreaterOrEqualTo(42);

            uaa.DisableCaching();
            uaa.CacheSize.Should().Be(0);
            this.GetAllocatedCacheSize(uaa).Should().Be(0);

            uaa.SetCacheSize(42);
            uaa.CacheSize.Should().Be(42);
            this.GetAllocatedCacheSize(uaa).Should().BeGreaterOrEqualTo(42);
        }

        /// <summary>
        /// I test that everything works fine even if I set to don't use cache and if I set the cache size later.
        /// </summary>
        [Fact]
        public void TestSettingNoCaching()
        {
            var uaa = UserAgentAnalyzer
            .NewBuilder()
            .WithoutCache()
            .HideMatcherLoadStats()
            .WithField("AgentUuid")
            .Build();

            uaa.CacheSize.Should().Be(0);
            this.GetAllocatedCacheSize(uaa).Should().Be(0);

            uaa.SetCacheSize(42);
            uaa.CacheSize.Should().Be(42);
            this.GetAllocatedCacheSize(uaa).Should().BeGreaterOrEqualTo(42);

            uaa.DisableCaching();
            uaa.CacheSize.Should().Be(0);
            this.GetAllocatedCacheSize(uaa).Should().BeGreaterOrEqualTo(0);
        }

        /// <summary>
        /// I test that the user agent and the field that I try to parse is cached correctly.
        /// </summary>
        [Fact]
        public void TestCache()
        {
            var uuid = "11111111-2222-3333-4444-555555555555";
            var fieldName = "AgentUuid";

            var uaa = UserAgentAnalyzer
            .NewBuilder()
            .WithCache(1)
            .HideMatcherLoadStats()
            .WithField(fieldName)
            .Build();

            UserAgent agent;

            uaa.CacheSize.Should().Be(1);
            this.GetAllocatedCacheSize(uaa).Should().BeGreaterOrEqualTo(1);

            agent = uaa.Parse(uuid);
            agent.Get(fieldName).GetValue().Should().Be(uuid);
            this.GetCache(uaa)[uuid].Should().BeEquivalentTo(agent);

            agent = uaa.Parse(uuid);
            agent.Get(fieldName).GetValue().Should().Be(uuid);
            this.GetCache(uaa)[uuid].Should().BeEquivalentTo(agent);

            uaa.DisableCaching();
            uaa.CacheSize.Should().Be(0);
            this.GetAllocatedCacheSize(uaa).Should().Be(0);

            agent = uaa.Parse(uuid);
            agent.Get(fieldName).GetValue().Should().Be(uuid);
            this.GetCache(uaa).Should().BeNull();
        }

        /// <summary>
        /// This method is an helper that does a little trick for us, it reads private property to get the private cache.
        /// this is used to check if the parsed user agent are really cached.
        /// </summary>
        /// <param name="uaa">The user agent analyzer from wich we want retrieve the private cache.</param>
        /// <returns>The cached <see cref="IDictionary<string, UserAgent>"> of parsed user agents.</returns>
        private IDictionary<string, UserAgent> GetCache(UserAgentAnalyzer uaa)
        {
            return uaa?.ParseCache;
        }

        /// <summary>
        /// This method is an helper that does a little trick for us, it reads the actual allocated size in the .NET dictionary using reflection.
        /// This method is unsafe, it is used to test the cache mechanism ported by Java, we need ti study a better mechanism for .NET.
        /// </summary>
        /// <param name="uaa">The <see cref="UserAgentAnalyzer"/> from wich we want read the allocated cache.</param>
        /// <returns>The allocated number of elements in the .NET Dictionary</returns>
        private int GetAllocatedCacheSize(UserAgentAnalyzer uaa)
        {
            var cache = this.GetCache(uaa);

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
