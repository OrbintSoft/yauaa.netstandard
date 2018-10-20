using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Debug;
using FluentAssertions;
using Xunit;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS
{
    public class DocumentationExample
    {
        [Fact]
        public void RunDocumentationExample()
        {
            UserAgentAnalyzerTester uaa = new UserAgentAnalyzerTester("YamlResources", "DocumentationExample.yaml");
            uaa.RunTests(false, true).Should().BeTrue();
        }
    }
}
