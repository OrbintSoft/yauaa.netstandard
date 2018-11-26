//-----------------------------------------------------------------------
// <copyright file="TestPredefinedBrowsers.cs" company="OrbintSoft">
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
namespace OrbintSoft.Yauaa.Testing.Tests
{
    using FluentAssertions;
    using log4net;
    using OrbintSoft.Yauaa.Debug;
    using OrbintSoft.Yauaa.Testing.Fixtures;
    using System.Collections.Generic;
    using System.Text;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="TestPredefinedBrowsers" />
    /// </summary>
    public class TestPredefinedBrowsers : IClassFixture<LogFixture>
    {
        /// <summary>
        /// Defines the LOG
        /// </summary>
        private static readonly ILog LOG = LogManager.GetLogger(typeof(TestPredefinedBrowsers));

        /// <summary>
        /// The ValidateAllPredefinedBrowsers
        /// </summary>
        [Fact]
        public void ValidateAllPredefinedBrowsers()
        {
            UserAgentAnalyzerTester uaa;
            uaa = UserAgentAnalyzerTester
                .NewBuilder()
                .ImmediateInitialization()
                .Build() as UserAgentAnalyzerTester;
            LOG.Info("==============================================================");
            LOG.Info("Validating when getting all fields");
            LOG.Info("--------------------------------------------------------------");
            uaa.RunTests(false, true, null, false, true).Should().BeTrue();
        }

        /// <summary>
        /// The ValidateAllPredefinedBrowsersMultipleFields
        /// </summary>
        /// <param name="fields">The fields<see cref="ICollection{string}"/></param>
        private void ValidateAllPredefinedBrowsersMultipleFields(ICollection<string> fields)
        {
            LOG.Info("==============================================================");
            LOG.Info(string.Format("Validating when ONLY asking for {0}", fields.ToString()));
            LOG.Info("--------------------------------------------------------------");
            UserAgentAnalyzerTester userAgentAnalyzer =
                UserAgentAnalyzerTester
                    .NewBuilder()
                    .WithoutCache()
                    .WithFields(fields)
                    .HideMatcherLoadStats()
                    .Build() as UserAgentAnalyzerTester;

            userAgentAnalyzer.Should().NotBeNull();
            userAgentAnalyzer.RunTests(false, true, fields, false, false).Should().BeTrue();
        }

        /// <summary>
        /// The Validate_DeviceClass_AgentNameVersionMajor
        /// </summary>
        [Fact]
        public void Validate_DeviceClass_AgentNameVersionMajor()
        {
            HashSet<string> fields = new HashSet<string>
            {
                "DeviceClass",
                "AgentNameVersionMajor"
            };
            ValidateAllPredefinedBrowsersMultipleFields(fields);
        }

        /// <summary>
        /// The Validate_DeviceClass_AgentNameVersionMajor_OperatingSystemVersionBuild
        /// </summary>
        [Fact]
        public void Validate_DeviceClass_AgentNameVersionMajor_OperatingSystemVersionBuild()
        {
            HashSet<string> fields = new HashSet<string>
            {
                "DeviceClass",
                "AgentNameVersionMajor",
                "OperatingSystemVersionBuild"
            };
            ValidateAllPredefinedBrowsersMultipleFields(fields);
        }

        /// <summary>
        /// The MakeSureWeDoNotHaveDuplicateTests
        /// </summary>
        [Fact]
        public void MakeSureWeDoNotHaveDuplicateTests()
        {
            UserAgentAnalyzerTester uaa = UserAgentAnalyzerTester.NewBuilder().Build() as UserAgentAnalyzerTester;

            IDictionary<string, IList<string>> allTestInputs = new Dictionary<string, IList<string>>(2000);
            HashSet<string> duplicates = new HashSet<string>();
            foreach (IDictionary<string, IDictionary<string, string>> testCase in uaa.TestCases)
            {
                string input = testCase["input"]["user_agent_string"];
                string location = testCase["metaData"]["filename"] + ":" + testCase["metaData"]["fileline"];
                IList<string> locations;
                if (allTestInputs.ContainsKey(input))
                {
                    locations = allTestInputs[input];
                }
                else
                {
                    locations = new List<string>();
                }
                locations.Add(location);

                if (locations.Count > 1)
                {
                    duplicates.Add(input);
                }
                allTestInputs[input] = locations;
            }

            duplicates.Count.Should().Be(0);
            if (duplicates.Count > 0)
            {
                StringBuilder sb = new StringBuilder(1024);
                foreach (string duplicate in duplicates)
                {
                    sb.Append("======================================================\n")
                        .Append("Testcase > ").Append(duplicate).Append("\n");
                    int count = 0;
                    foreach (string location in allTestInputs[duplicate])
                    {
                        sb.Append(">Location ").Append(++count).Append(".(").Append(location).Append(")\n");
                    }
                }
                LOG.Info("Found " + duplicates.Count + " testcases multiple times: \n" + sb.ToString());
            }
        }
    }
}
