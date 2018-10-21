using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Debug;
using FluentAssertions;
using System;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze;
using Xunit;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Parse;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS
{
    public class TestErrorHandling
    {
        private void RunTest(string directory, string file, string expectedMessage)
        {            
            Action a = new Action(() => {
                UserAgentAnalyzerTester uaa = new UserAgentAnalyzerTester(directory, file);
                uaa.RunTests(false, false);
            });
            a.Should().Throw<InvalidParserConfigurationException>().Where(e => e.Message.Contains(expectedMessage));
        }

        [Fact]
        public void CheckNoFile()
        {
            RunTest("YamlResources/BadDefinitions", "ThisOneDoesNotExist---Really.yaml", "Unable to find ANY config files");
        }

        [Fact]
        public void CheckEmptyFile()
        {
            RunTest("YamlResources/BadDefinitions", "EmptyFile.yaml", "The file EmptyFile.yaml is empty");
        }
        
        [Fact]
        public void CheckFileIsNotAMap()
        {
            RunTest("YamlResources/BadDefinitions", "FileIsNotAMap.yaml", "Yaml config problem.(FileIsNotAMap.yaml:21): The value should be a sequence but it is a Mapping");
        }

        [Fact]
        public void CheckLookupSetMissing()
        {
            RunTest("YamlResources/BadDefinitions", "LookupSetMissing.yaml", "Missing lookupSet");
        }

        [Fact]
        public void CheckBadEntry()
        {
            RunTest("YamlResources/BadDefinitions", "BadEntry.yaml", "Found unexpected config entry:");
        }

        [Fact]
        public void CheckLookupMissing()
        {
            RunTest("YamlResources/BadDefinitions", "LookupMissing.yaml", "Missing lookup");
        }

        [Fact]
        public void CheckFixedStringLookupMissing()
        {
            RunTest("YamlResources/BadDefinitions", "FixedStringLookupMissing.yaml", "Missing lookup");
        }

        [Fact]
        public void CheckNoExtract()
        {
            RunTest("YamlResources/BadDefinitions", "NoExtract.yaml", "Matcher does not extract anything");
        }

        [Fact]
        public void CheckInvalidExtract()
        {
            RunTest("YamlResources/BadDefinitions", "InvalidExtract.yaml", "Invalid extract config line: agent.text=\"foo\"");
        }

        [Fact]
        public void CheckNoTestInput()
        {
            RunTest("YamlResources/BadDefinitions", "NoTestInput.yaml", "Test is missing input");
        }

        [Fact]
        public void CheckSyntaxErrorRequire()
        {
            RunTest("YamlResources/BadDefinitions", "SyntaxErrorRequire.yaml", "Syntax error");
        }

        [Fact]
        public void CheckSyntaxErrorExtract1()
        {
            RunTest("YamlResources/BadDefinitions", "SyntaxErrorExtract1.yaml", "Syntax error");
        }

        [Fact]
        public void CheckSyntaxErrorExtract2()
        {
            RunTest("YamlResources/BadDefinitions", "SyntaxErrorExtract2.yaml", "Invalid extract config line");
        }

        [Fact]
        public void CheckSyntaxErrorVariable1()
        {
            RunTest("YamlResources/BadDefinitions", "SyntaxErrorVariable1.yaml", "Syntax error");
        }

        [Fact]
        public void CheckSyntaxErrorVariable2()
        {
            RunTest("YamlResources/BadDefinitions", "SyntaxErrorVariable2.yaml", "Invalid variable config line:");
        }

        [Fact]
        public void CheckSyntaxErrorVariableBackReference()
        {
            RunTest("YamlResources/BadDefinitions", "Variable-BackReference.yaml", "Syntax error");
        }

        [Fact]
        public void CheckSyntaxErrorVariableBadDefinition()
        {
            RunTest("YamlResources/BadDefinitions", "Variable-BadDefinition.yaml", "Invalid variable config line:");
        }

        [Fact]
        public void CheckSyntaxErrorVariableFixedString()
        {
            RunTest("YamlResources/BadDefinitions", "Variable-FixedString.yaml", "Syntax error");
        }

        [Fact]
        public void MethodInputValidation()
        {
            UserAgentAnalyzer uaa = UserAgentAnalyzer.NewBuilder()
                .WithField("AgentClass")
                .Build();

            UserAgent agent = uaa.Parse((string)null);
            agent.Should().NotBeNull();
            agent.GetUserAgentString().Should().BeNull();

            agent = uaa.Parse((UserAgent)null);
            agent.Should().BeNull();

            EvilManualUseragentStringHacks.FixIt(null).Should().BeNull();
        }
    }
}
