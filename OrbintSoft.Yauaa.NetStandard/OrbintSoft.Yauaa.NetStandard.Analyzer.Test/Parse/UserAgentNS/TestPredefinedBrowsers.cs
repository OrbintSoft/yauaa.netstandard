using log4net;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Debug;
using Xunit;
using FluentAssertions;
using OrbintSoft.Yauaa.Analyzer.Test.Fixtures;
using System.Collections.Generic;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS
{
    public class TestPredefinedBrowsers : IClassFixture<LogFixture>
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(TestPredefinedBrowsers));

        [Fact]
        public void ValidateAllPredefinedBrowsers()
        {
            UserAgentAnalyzerTester uaa;
            uaa = UserAgentAnalyzerTester
                .NewBuilder()
                .ImmediateInitialization()                
                .Build() as UserAgentAnalyzerTester;
            LOG.Info("==============================================================");
            LOG.Info("Validating when getting all fields");
            LOG.Info("--------------------------------------------------------------");
            uaa.RunTests(false, true, null, false, true).Should().BeTrue();
        }

        private void ValidateAllPredefinedBrowsersMultipleFields(ICollection<string> fields)
        {
            LOG.Info("==============================================================");
            LOG.Info(string.Format("Validating when ONLY asking for {0}", fields.ToString()));
            LOG.Info("--------------------------------------------------------------");
            UserAgentAnalyzerTester userAgentAnalyzer =
                UserAgentAnalyzerTester
                    .NewBuilder()
                    .WithoutCache()
                    .WithFields(fields)
                    .HideMatcherLoadStats()
                    .Build() as UserAgentAnalyzerTester;

            userAgentAnalyzer.Should().NotBeNull();
            userAgentAnalyzer.RunTests(false, true, fields, false, false).Should().BeTrue();
        }

        [Fact]
        public void Validate_DeviceClass_AgentNameVersionMajor()
        {
            HashSet<string> fields = new HashSet<string>
            {
                "DeviceClass",
                "AgentNameVersionMajor"
            };
            ValidateAllPredefinedBrowsersMultipleFields(fields);
        }

        [Fact]
        public void Validate_DeviceClass_AgentNameVersionMajor_OperatingSystemVersionBuild()
        {
            HashSet<string> fields = new HashSet<string>();
            fields.Add("DeviceClass");
            fields.Add("AgentNameVersionMajor");
            fields.Add("OperatingSystemVersionBuild");
            ValidateAllPredefinedBrowsersMultipleFields(fields);
        }
    }
}
