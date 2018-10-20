using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using System.Reflection;
using OrbintSoft.Yauaa.Analyzer.Test.Fixtures;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS
{
    public class TestCaching : IClassFixture<LogFixture>
    {
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
