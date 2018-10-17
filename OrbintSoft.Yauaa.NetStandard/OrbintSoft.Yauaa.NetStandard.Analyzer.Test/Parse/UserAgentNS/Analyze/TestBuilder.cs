using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS;
using FluentAssertions;
using Xunit;
using OrbintSoft.Yauaa.Analyzer.Test.Fixtures;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS.Analyze
{
    public class TestBuilder : IClassFixture<LogFixture>
    {
        private void RunTestCase(UserAgentAnalyzerDirect userAgentAnalyzer)
        {
            UserAgent parsedAgent = userAgentAnalyzer.Parse("Mozilla/5.0 (Linux; Android 7.0; Nexus 6 Build/NBD90Z) " +
                "AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.124 Mobile Safari/537.36");

            // The requested fields
            parsedAgent.GetValue("DeviceClass").Should().Be("Phone"); // Phone
            parsedAgent.GetValue("AgentNameVersionMajor").Should().Be("Chrome 53"); // Chrome 53

            // The fields that are internally needed to build the requested fields
            parsedAgent.GetValue("AgentName").Should().Be("Chrome"); // Chrome
            parsedAgent.GetValue("AgentVersion").Should().Be("53.0.2785.124"); // 53.0.2785.124
            parsedAgent.GetValue("AgentVersionMajor").Should().Be("53"); // 53

             long min1 = -1;

            // The rest must be at confidence -1 (i.e. no rules fired)
            parsedAgent.GetConfidence("DeviceName").Should().Be(min1); // Nexus 6
            parsedAgent.GetConfidence("DeviceBrand").Should().Be(min1); // Google
            parsedAgent.GetConfidence("OperatingSystemClass").Should().Be(min1); // Mobile
            parsedAgent.GetConfidence("OperatingSystemName").Should().Be(min1); // Android
            parsedAgent.GetConfidence("OperatingSystemVersion").Should().Be(min1); // 7.0
            parsedAgent.GetConfidence("OperatingSystemNameVersion").Should().Be(min1); // Android 7.0
            parsedAgent.GetConfidence("OperatingSystemVersionBuild").Should().Be(min1); // NBD90Z
            parsedAgent.GetConfidence("LayoutEngineClass").Should().Be(min1); // Browser
            parsedAgent.GetConfidence("LayoutEngineName").Should().Be(min1); // Blink
            parsedAgent.GetConfidence("LayoutEngineVersion").Should().Be(min1); // 53.0
            parsedAgent.GetConfidence("LayoutEngineVersionMajor").Should().Be(min1); // 53
            parsedAgent.GetConfidence("LayoutEngineNameVersion").Should().Be(min1); // Blink 53.0
            parsedAgent.GetConfidence("LayoutEngineNameVersionMajor").Should().Be(min1); // Blink 53
            parsedAgent.GetConfidence("AgentClass").Should().Be(min1); // Browser
            parsedAgent.GetConfidence("AgentNameVersion").Should().Be(min1); // Chrome 53.0.2785.124
        }

        [Fact]
        public void TestLimitedFieldsDirect()
        {
            UserAgentAnalyzerDirect userAgentAnalyzer =
                UserAgentAnalyzer
                    .NewBuilder()
                    .Preheat(100)
                    .Preheat()
                    .HideMatcherLoadStats()
                    .ShowMatcherLoadStats()
                    .WithAllFields()
                    .WithField("DeviceClass")
                    .WithField("AgentNameVersionMajor")
                    .WithUserAgentMaxLength(1234)
                    .Build();

            userAgentAnalyzer.GetUserAgentMaxLength().Should().Be(1234);

            RunTestCase(userAgentAnalyzer);
        }


    }
}
