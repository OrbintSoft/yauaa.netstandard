//-----------------------------------------------------------------------
// <copyright file="TestDeveloperTools.cs" company="OrbintSoft">
//    Yet Another User Agent Analyzer for .NET Standard
//    porting realized by Stefano Balzarotti, Copyright 2018-2019 (C) OrbintSoft
//
//    Original Author and License:
//
//    Yet Another UserAgent Analyzer
//    Copyright(C) 2013-2019 Niels Basjes
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
using FluentAssertions;
using OrbintSoft.Yauaa.Analyze;
using OrbintSoft.Yauaa.Debug;
using OrbintSoft.Yauaa.Testing.Fixtures;
using OrbintSoft.Yauaa.Tests;
using Xunit;

namespace OrbintSoft.Yauaa.Testing.Tests.Basic
{
    /// <summary>
    /// I test that test and debugging tools works as expected.
    /// </summary>
    public class TestDeveloperTools : IClassFixture<LogFixture>
    {
        /// <summary>
        /// I check with a wrong test case define in in CheckErrorOutput.yaml, the test fails.
        /// </summary>
        [Fact]
        public void ValidateErrorSituationOutput()
        {
            var uaa = UserAgentAnalyzerTester
                .NewBuilder()
                .HideMatcherLoadStats()
                .DelayInitialization()
                .DropTests()
                .Build() as UserAgentAnalyzerTester;
            uaa.SetShowMatcherStats(true);
            uaa.KeepTests();
            uaa.LoadResources(Config.RESOURCES_PATH , "CheckErrorOutput.yaml");
            uaa.RunTests(false, true).Should().BeFalse();  // This test must return an error state
        }

        /// <summary>
        /// I check taht if I add a new empty test case, the other test cases continue to woirk as expected.
        /// </summary>
        [Fact]
        public void ValidateNewTestcaseSituationOutput()
        {
            var uaa = UserAgentAnalyzerTester
            .NewBuilder()
            .DelayInitialization()
            .HideMatcherLoadStats()
            .DropTests()
            .Build() as UserAgentAnalyzerTester;
            uaa.SetShowMatcherStats(true);
            uaa.KeepTests();
            uaa.LoadResources(Config.RESOURCES_PATH, "CheckNewTestcaseOutput.yaml");
            uaa.RunTests(false, true).Should().BeTrue();
        }

        /// <summary>
        /// I validate that the ouput of tests and matchers is formatted as expected.
        /// </summary>
        [Fact]
        public void ValidateStringOutputsAndMatches()
        {
            var uaa = UserAgentAnalyzerTester.NewBuilder().WithField("DeviceName").Build() as UserAgentAnalyzerTester;
            var useragent = uaa.Parse("Mozilla/5.0 (Linux; Android 7.0; Nexus 6 Build/NBD90Z) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.124 Mobile Safari/537.36");
            useragent.ToString().Contains("'Google Nexus 6'").Should().BeTrue();
            useragent.ToJson().Contains("\"DeviceName\":\"Google Nexus 6\"").Should().BeTrue();
            useragent.ToYamlTestCase(true).Contains("'Google Nexus 6'").Should().BeTrue();

            var ok = false;
            foreach (var match in uaa.GetMatches())
            {
                if ("agent.(1)product.(1)comments.(3)entry[3-3]".Equals(match.Key))
                {
                    match.Value.Should().Be("Build");
                    ok = true;
                    break;
                }
            }
            ok.Should().BeTrue("Did not see the expected match.");

            ok = false;
            foreach (var match in uaa.GetUsedMatches(useragent))
            {
                if ("agent.(1)product.(1)comments.(3)entry[3-3]".Equals(match.Key))
                {
                    match.Value.Should().Be("Build");
                    ok = true;
                    break;
                }
            }
            ok.Should().BeTrue("Did not see the expected match.");
        }
    }
}
