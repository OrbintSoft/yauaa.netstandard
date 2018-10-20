using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS;
using System;
using Xunit;
using FluentAssertions;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS.Utils
{
    public class TestVersionCollisionChecks
    {
        [Fact]
        public void TestBadVersion()
        {
           UserAgentAnalyzer uaa = UserAgentAnalyzer
            .NewBuilder()
            .DelayInitialization()
            .Build();

            Action a = new Action(() => uaa.LoadResources("YamlResources/Versions", "BadVersion.yaml"));
            a.Should().Throw<InvalidParserConfigurationException>().Where(e => e.Message.Contains("Found unexpected config entry: bad"));
        }

        [Fact]
        public void TestDifferentVersion()
        {
            UserAgentAnalyzer uaa = UserAgentAnalyzer
             .NewBuilder()
             .DelayInitialization()
             .Build();

            Action a = new Action(() => uaa.LoadResources("YamlResources/Versions", "DifferentVersion.yaml"));
            a.Should().Throw<InvalidParserConfigurationException>().Where(e => e.Message.Contains("Two different Yauaa versions have been loaded:"));
        }

        [Fact]
        public void TestDoubleLoadedResources()
        {
            UserAgentAnalyzer uaa = UserAgentAnalyzer
             .NewBuilder()
             .DelayInitialization()
             .Build();

            Action a = new Action(() => uaa.LoadResources("YamlResources/Useragents"));
            a.Should().Throw<InvalidParserConfigurationException>().Where(e => e.Message.Contains("resources for the second time"));
        }
    }
}
