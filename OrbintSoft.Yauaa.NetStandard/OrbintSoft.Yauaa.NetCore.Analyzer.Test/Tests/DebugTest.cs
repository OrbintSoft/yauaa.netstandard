using OrbintSoft.Yauaa.Testing.Fixtures;
using Xunit;

namespace OrbintSoft.Yauaa.Testing.Tests
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
