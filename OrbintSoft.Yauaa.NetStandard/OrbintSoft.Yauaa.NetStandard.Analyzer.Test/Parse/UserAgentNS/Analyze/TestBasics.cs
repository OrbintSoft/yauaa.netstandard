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
