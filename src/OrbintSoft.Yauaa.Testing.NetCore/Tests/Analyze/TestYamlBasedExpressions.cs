//-----------------------------------------------------------------------
// <copyright file="TestYamlBasedExpressions.cs" company="OrbintSoft">
//    Yet Another User Agent Analyzer for .NET Standard
//    porting realized by Stefano Balzarotti, Copyright 2018 (C) OrbintSoft
//
//    Original Author and License:
//
//    Yet Another UserAgent Analyzer
//    Copyright(C) 2013-2018 Niels Basjes
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//    https://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//   
// </copyright>
// <author>Stefano Balzarotti, Niels Basjes</author>
// <date>2018, 11, 24, 17:39</date>
// <summary></summary>
//-----------------------------------------------------------------------
namespace OrbintSoft.Yauaa.Testing.Tests.Analyze
{
    using FluentAssertions;
    using OrbintSoft.Yauaa.Debug;
    using OrbintSoft.Yauaa.Testing.Fixtures;
    using OrbintSoft.Yauaa.Tests;
    using Xunit;

    /// <summary>
    /// I use this class to to test all possible kind of exoressions that can be defined in yaml.
    /// </summary>
    public class TestYamlBasedExpressions : IClassFixture<LogFixture>
    {
        //    @Test
        //    public void runSingleMatcherFile() {
        //        UserAgentAnalyzerTester uaa = new UserAgentAnalyzerTester("classpath*:**/Linux.yaml");
        //        Assert.assertTrue(uaa.runTests(true, false));
        //    }
        /// <summary>
        /// The RunMatcherTests
        /// </summary>
        [Fact]
        public void RunMatcherTests()
        {
            var uaa = new UserAgentAnalyzerTester(Config.RESOURCES_PATH, "Matcher-tests.yaml");
            uaa.RunTests(false, false).Should().BeTrue();
        }

        /// <summary>
        /// The RunMatcherNestedFunctionsTests
        /// </summary>
        [Fact]
        public void RunMatcherNestedFunctionsTests()
        {
            var uaa = new UserAgentAnalyzerTester(Config.RESOURCES_PATH, "Matcher-nested-functions.yaml");
            uaa.RunTests(false, false).Should().BeTrue();
        }

        /// <summary>
        /// The RunMatcherIsNullTests
        /// </summary>
        [Fact]
        public void RunMatcherIsNullTests()
        {
            var uaa = new UserAgentAnalyzerTester(Config.RESOURCES_PATH, "Matcher-IsNull-tests.yaml");
            uaa.RunTests(false, false).Should().BeTrue();
        }

        /// <summary>
        /// The RunSubstringTests
        /// </summary>
        [Fact]
        public void RunSubstringTests()
        {
            var uaa = new UserAgentAnalyzerTester(Config.RESOURCES_PATH, "SubString-tests.yaml");
            uaa.RunTests(false, false).Should().BeTrue();
        }

        /// <summary>
        /// The RunSubstringVersionTests
        /// </summary>
        [Fact]
        public void RunSubstringVersionTests()
        {
            var uaa = new UserAgentAnalyzerTester(Config.RESOURCES_PATH, "SubStringVersion-tests.yaml");
            uaa.RunTests(false, false).Should().BeTrue();
        }

        /// <summary>
        /// The RunLookupTests
        /// </summary>
        [Fact]
        public void RunLookupTests()
        {
            var uaa = new UserAgentAnalyzerTester(Config.RESOURCES_PATH, "Lookup-tests.yaml");
            uaa.RunTests(false, false).Should().BeTrue();
        }

        [Fact]
        public void RunLookupPrefixTests()
        {
            var uaa = new UserAgentAnalyzerTester(Config.RESOURCES_PATH, "LookupPrefix-tests.yaml");
            uaa.RunTests(false, true).Should().BeTrue();
        }

        /// <summary>
        /// The RunVariableTests
        /// </summary>
        [Fact]
        public void RunVariableTests()
        {
            var uaa = new UserAgentAnalyzerTester(Config.RESOURCES_PATH, "Variable-tests.yaml");
            uaa.RunTests(false, false).Should().BeTrue();
        }

        /// <summary>
        /// The RunPositionalTests
        /// </summary>
        [Fact]
        public void RunPositionalTests()
        {
            var uaa = new UserAgentAnalyzerTester(Config.RESOURCES_PATH, "Positional-tests.yaml");
            uaa.RunTests(false, true).Should().BeTrue();
        }

        /// <summary>
        /// The RunWalkingTests
        /// </summary>
        [Fact]
        public void RunWalkingTests()
        {
            var uaa = new UserAgentAnalyzerTester(Config.RESOURCES_PATH, "Walking-tests.yaml");
            uaa.SetVerbose(true);
            uaa.RunTests(false, false).Should().BeTrue();
        }

        /// <summary>
        /// The RunAllFieldsTests
        /// </summary>
        [Fact]
        public void RunAllFieldsTests()
        {
            var uaa = new UserAgentAnalyzerTester(Config.RESOURCES_PATH, "AllFields-tests.yaml");
            uaa.RunTests(false, true).Should().BeTrue();
        }

        /// <summary>
        /// The RunDebugOutputTest
        /// </summary>
        [Fact]
        public void RunDebugOutputTest()
        {
            var uaa = new UserAgentAnalyzerTester(Config.RESOURCES_PATH, "DebugOutput-tests.yaml");
            uaa.RunTests(true, true).Should().BeTrue();
        }

        /// <summary>
        /// The RunEdgecasesTest
        /// </summary>
        [Fact]
        public void RunEdgecasesTest()
        {
            var uaa = new UserAgentAnalyzerTester(Config.RESOURCES_PATH, "Edgecases-tests.yaml");
            uaa.RunTests(false, true).Should().BeTrue();
        }

        /// <summary>
        /// The RunAllPossibleSteps
        /// </summary>
        [Fact]
        public void RunAllPossibleSteps()
        {
            var uaa = new UserAgentAnalyzerTester(Config.RESOURCES_PATH, "AllPossibleSteps.yaml");
            uaa.SetVerbose(true);
            uaa.RunTests(false, false).Should().BeTrue();
        }

        [Fact]
        public void RunOnlyOneTest()
        {
            var uaa = new UserAgentAnalyzerTester(Config.RESOURCES_PATH, "TestOnlyOneTest.yaml");
            uaa.SetVerbose(true);
            uaa.RunTests(false, false).Should().BeTrue();
        }
    }
}
