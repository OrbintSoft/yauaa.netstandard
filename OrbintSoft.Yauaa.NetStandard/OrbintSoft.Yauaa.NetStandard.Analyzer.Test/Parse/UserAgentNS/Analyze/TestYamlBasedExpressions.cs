/*
 * Yet Another UserAgent Analyzer .NET Standard
 * Porting realized by Balzarotti Stefano, Copyright (C) OrbintSoft
 * 
 * Original Author and License:
 * 
 * Yet Another UserAgent Analyzer
 * Copyright (C) 2013-2018 Niels Basjes
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * All rights should be reserved to the original author Niels Basjes
 */

using FluentAssertions;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Debug;
using OrbintSoft.Yauaa.Analyzer.Test.Fixtures;
using Xunit;

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
            uaa.SetVerbose(true);
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
            uaa.SetVerbose(true);
            uaa.RunTests(false, false).Should().BeTrue();
        }
    }
}
