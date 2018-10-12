using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Debug;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS.Analyze
{
    public class TestYamlBasedExpressions
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
    }
}
