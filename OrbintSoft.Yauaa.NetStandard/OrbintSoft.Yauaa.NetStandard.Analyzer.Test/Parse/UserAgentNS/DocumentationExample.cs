using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Debug;
using FluentAssertions;
using Xunit;
using OrbintSoft.Yauaa.Analyzer.Test.Fixtures;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS
{
    public class DocumentationExample : IClassFixture<LogFixture>
    {
        [Fact]
        public void RunDocumentationExample()
        {
            UserAgentAnalyzerTester uaa = new UserAgentAnalyzerTester("YamlResources", "DocumentationExample.yaml");
            uaa.RunTests(false, true).Should().BeTrue();
        }
    }
}
