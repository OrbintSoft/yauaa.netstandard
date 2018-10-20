using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Debug;
using OrbintSoft.Yauaa.Analyzer.Test.Fixtures;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS.Analyze
{
    public class TestYamlBasedExpressions : IClassFixture<LogFixture>
    {
        //    @Test
        //    public void runSingleMatcherFile() {
        //        UserAgentAnalyzerTester uaa = new UserAgentAnalyzerTester("classpath*:**/Linux.yaml");
        //        Assert.assertTrue(uaa.runTests(true, false));
        //    }

        [Fact]
        public void RunMatcherTests()
        {
            UserAgentAnalyzerTester uaa = new UserAgentAnalyzerTester("YamlResources", "Matcher-tests.yaml");
            uaa.RunTests(false, false).Should().BeTrue();
        }

        [Fact]
        public void RunMatcherNestedFunctionsTests()
        {
            UserAgentAnalyzerTester uaa = new UserAgentAnalyzerTester("YamlResources", "Matcher-nested-functions.yaml");
            uaa.RunTests(false, false).Should().BeTrue();
        }

        [Fact]
        public void RunMatcherIsNullTests()
        {
            UserAgentAnalyzerTester uaa = new UserAgentAnalyzerTester("YamlResources", "Matcher-IsNull-tests.yaml");
            uaa.RunTests(false, false).Should().BeTrue();
        }

        [Fact]
        public void RunSubstringTests()
        {
            UserAgentAnalyzerTester uaa = new UserAgentAnalyzerTester("YamlResources", "SubString-tests.yaml");
            uaa.RunTests(false, false).Should().BeTrue();
        }

        [Fact]
        public void RunSubstringVersionTests()
        {
            UserAgentAnalyzerTester uaa = new UserAgentAnalyzerTester("YamlResources", "SubStringVersion-tests.yaml");
            uaa.RunTests(false, false).Should().BeTrue();
        }

        [Fact]
        public void RunLookupTests()
        {
            UserAgentAnalyzerTester uaa = new UserAgentAnalyzerTester("YamlResources", "Lookup-tests.yaml");
            uaa.RunTests(false, false).Should().BeTrue();
        }

        [Fact]
        public void RunVariableTests()
        {
            UserAgentAnalyzerTester uaa = new UserAgentAnalyzerTester("YamlResources", "Variable-tests.yaml");
            uaa.RunTests(false, false).Should().BeTrue();
        }

        [Fact]
        public void RunPositionalTests()
        {
            UserAgentAnalyzerTester uaa = new UserAgentAnalyzerTester("YamlResources", "Positional-tests.yaml");
            uaa.RunTests(false, true).Should().BeTrue();
        }

        [Fact]
        public void RunWalkingTests()
        {
            UserAgentAnalyzerTester uaa = new UserAgentAnalyzerTester("YamlResources", "Walking-tests.yaml");
            uaa.RunTests(false, false).Should().BeTrue();
        }

        [Fact]
        public void RunAllFieldsTests()
        {
            UserAgentAnalyzerTester uaa = new UserAgentAnalyzerTester("YamlResources", "AllFields-tests.yaml");
            uaa.RunTests(false, true).Should().BeTrue();
        }

        [Fact]
        public void RunDebugOutputTest()
        {
            UserAgentAnalyzerTester uaa = new UserAgentAnalyzerTester("YamlResources", "DebugOutput-tests.yaml");
            uaa.RunTests(true, true).Should().BeTrue();
        }

        [Fact]
        public void RunEdgecasesTest()
        {
            UserAgentAnalyzerTester uaa = new UserAgentAnalyzerTester("YamlResources", "Edgecases-tests.yaml");
            uaa.RunTests(false, true).Should().BeTrue();
        }

        [Fact]
        public void RunAllPossibleSteps()
        {
            UserAgentAnalyzerTester uaa = new UserAgentAnalyzerTester("YamlResources", "AllPossibleSteps.yaml");
            uaa.RunTests(false, false).Should().BeTrue();
        }
    }
}
