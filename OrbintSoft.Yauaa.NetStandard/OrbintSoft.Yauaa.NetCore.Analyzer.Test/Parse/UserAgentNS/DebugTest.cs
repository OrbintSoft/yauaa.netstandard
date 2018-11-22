using FluentAssertions;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Debug;
using OrbintSoft.Yauaa.Analyzer.Test.Fixtures;
using Xunit;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS
{
    public class DebugTest : IClassFixture<LogFixture>
    {
        //[Fact]
        //public void TestError()
        //{
        //    UserAgentAnalyzerTester uaa = UserAgentAnalyzerTester
        //        .NewBuilder()
        //        .HideMatcherLoadStats()
        //        .DelayInitialization()
        //        .DropDefaultResources()
        //        .DropTests()
        //        .AddResources("YamlResources/UserAgents", "GooglePixel.yaml")
        //        .Build() as UserAgentAnalyzerTester;
        //    uaa.SetShowMatcherStats(false);                 
        //    uaa.RunTests(false, true).Should().BeFalse();  // This test must return an error state
        //}
    }
}
