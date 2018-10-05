using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using System.Reflection;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS
{
    public class TestCaching
    {
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
