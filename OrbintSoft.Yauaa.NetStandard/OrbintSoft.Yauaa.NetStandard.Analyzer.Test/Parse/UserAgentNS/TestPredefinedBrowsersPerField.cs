using log4net;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Debug;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using OrbintSoft.Yauaa.Analyzer.Test.Fixtures;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Parse;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS
{
    public class TestPredefinedBrowsersPerField : IClassFixture<LogFixture>
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(TestPredefinedBrowsersPerField));

        public static IEnumerable<object[]> Data()
        {
            var fieldNames = UserAgentAnalyzer
                .NewBuilder()
                .HideMatcherLoadStats()
                .DelayInitialization()
                .Build()
                .GetAllPossibleFieldNamesSorted();
            foreach (var fieldName in fieldNames)
            {
                yield return new object[] { fieldName };
            }
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void ValidateAllPredefinedBrowsersForField(string fieldName)
        {
            HashSet<string> singleFieldList = new HashSet<string>();
            LOG.Info("==============================================================");
            LOG.Info(string.Format("Validating when ONLY asking for {0}", fieldName));
            LOG.Info("--------------------------------------------------------------");
            UserAgentAnalyzerTester userAgentAnalyzer =
                UserAgentAnalyzerTester
                    .NewBuilder()
                    .WithoutCache()
                    .WithField(fieldName)
                    .HideMatcherLoadStats()
                    .Build() as UserAgentAnalyzerTester;
            userAgentAnalyzer.SetVerbose(true);
            singleFieldList.Clear();
            singleFieldList.Add(fieldName);
            userAgentAnalyzer.Should().NotBeNull();
            userAgentAnalyzer.RunTests(false, true, singleFieldList, false, false).Should().BeTrue();
        }

        [Fact]
        public void TestUA()
        {
            UserAgentAnalyzer userAgentAnalyzer =
            UserAgentAnalyzer
                .NewBuilder()
                .DropDefaultResources()
                .AddResources("YamlResources/UserAgents", "CPUTypes.yaml")
                .AddResources("YamlResources/UserAgents", "GeckoFirefox.yaml")
                .AddResources("YamlResources/UserAgents", "OperatingSystemDeviceNames.yaml")
                .AddResources("YamlResources/UserAgents", "OtherOS.yaml")
                .WithField("AgentName")
                .WithoutCache()
                .HideMatcherLoadStats()
                .Build();

            UserAgent parsedAgent = userAgentAnalyzer.Parse("Mozilla/5.0 (Windows NT; Win64; x64; rv:26.0) Gecko/20100101 Firefox/26.0 Waterfox/26.0");

            // The requested fields
            parsedAgent.GetValue("AgentName").Should().Be("Waterfox");
        }
    }
}
