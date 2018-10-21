using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Debug;
using Xunit;
using FluentAssertions;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS
{
    public class TestDeveloperTools : IClassFixture<LogFixture>
    {
        [Fact]
        public void ValidateErrorSituationOutput()
        {
            UserAgentAnalyzerTester uaa = UserAgentAnalyzerTester
                .NewBuilder()
                .HideMatcherLoadStats()
                .DelayInitialization()
                .DropTests()
                .Build() as UserAgentAnalyzerTester;
            uaa.SetShowMatcherStats(true);
            uaa.KeepTests();
            uaa.LoadResources("YamlResources", "CheckErrorOutput.yaml");
            uaa.RunTests(false, true).Should().BeFalse();  // This test must return an error state
        }

        [Fact]
        public void ValidateNewTestcaseSituationOutput()
        {
            UserAgentAnalyzerTester uaa = UserAgentAnalyzerTester
            .NewBuilder()
            .DelayInitialization()
            .HideMatcherLoadStats()
            .DropTests()
            .Build() as UserAgentAnalyzerTester;
            uaa.SetShowMatcherStats(true);
            uaa.KeepTests();
            uaa.LoadResources("YamlResources", "CheckNewTestcaseOutput.yaml");
            uaa.RunTests(false, true).Should().BeTrue();  // This test must return an error state
        }

        [Fact]
        public void ValidateStringOutputsAndMatches()
        {
            UserAgentAnalyzerTester uaa = UserAgentAnalyzerTester.NewBuilder().WithField("DeviceName").Build() as UserAgentAnalyzerTester;
            UserAgent useragent = uaa.Parse("Mozilla/5.0 (Linux; Android 7.0; Nexus 6 Build/NBD90Z) " +
                "AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.124 Mobile Safari/537.36");
            useragent.ToString().Contains("'Google Nexus 6'").Should().BeTrue();
            useragent.ToJson().Contains("\"DeviceName\":\"Google Nexus 6\"").Should().BeTrue();
            useragent.ToYamlTestCase(true).Contains("'Google Nexus 6'").Should().BeTrue();

            bool ok = false;
            foreach (MatchesList.Match match in uaa.GetMatches())
            {
                if ("agent.(1)product.(1)comments.(3)entry[3-3]".Equals(match.GetKey()))
                {
                    match.GetValue().Should().Be("Build");
                    ok = true;
                    break;
                }
            }
            ok.Should().BeTrue("Did not see the expected match.");

            ok = false;
            foreach (MatchesList.Match match in uaa.GetUsedMatches(useragent))
            {
                if ("agent.(1)product.(1)comments.(3)entry[3-3]".Equals(match.GetKey()))
                {
                    match.GetValue().Should().Be("Build");
                    ok = true;
                    break;
                }
            }
            ok.Should().BeTrue("Did not see the expected match.");
        }
    }
}
