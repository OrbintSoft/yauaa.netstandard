using OrbintSoft.Yauaa.Analyzer.Parse.UserAgent;
using Xunit;
using FluentAssertions;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgent.Analyze
{
    public class TestBasics
    {
        [Fact]
        public void TestCacheSetter()
        {
            UserAgentAnalyzer userAgentAnalyzer = UserAgentAnalyzer.NewBuilder().Build();
            userAgentAnalyzer.LoadResources("classpath*:AllFields-tests.yaml");

            userAgentAnalyzer.getCacheSize().Should().Be(10000, "Default cache size");
            
            userAgentAnalyzer.SetCacheSize(50);
            userAgentAnalyzer.getCacheSize().Should().Be(50, "I set that size");

            userAgentAnalyzer.SetCacheSize(50000);
            userAgentAnalyzer.getCacheSize().Should().Be(50000, "I set that size");

            userAgentAnalyzer.SetCacheSize(-5);
            userAgentAnalyzer.getCacheSize().Should().Be(0, "I set incorrect cache size");

            userAgentAnalyzer.SetCacheSize(50);
            userAgentAnalyzer.getCacheSize().Should().Be(50, "I set that size");

            userAgentAnalyzer.SetCacheSize(50000);
            userAgentAnalyzer.getCacheSize().Should().Be(50000, "I set that size");

            userAgentAnalyzer.SetUserAgentMaxLength(555);
            userAgentAnalyzer.GetUserAgentMaxLength().Should().Be(555, "I set that size");           
        }
    }
}
