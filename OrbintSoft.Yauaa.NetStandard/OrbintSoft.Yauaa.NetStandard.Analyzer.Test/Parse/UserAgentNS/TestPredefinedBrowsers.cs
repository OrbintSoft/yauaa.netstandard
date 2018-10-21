using log4net;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Debug;
using Xunit;
using FluentAssertions;
using OrbintSoft.Yauaa.Analyzer.Test.Fixtures;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS
{
    public class TestPredefinedBrowsers : IClassFixture<LogFixture>
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(TestPredefinedBrowsers));

        [Fact]
        public void ValidateAllPredefinedBrowsers()
        {
            UserAgentAnalyzerTester uaa;
            uaa = UserAgentAnalyzerTester.NewBuilder().ImmediateInitialization().Build() as UserAgentAnalyzerTester;
            LOG.Info("==============================================================");
            LOG.Info("Validating when getting all fields");
            LOG.Info("--------------------------------------------------------------");
            uaa.RunTests(false, true, null, false, true).Should().BeTrue();
        }
    }
}
