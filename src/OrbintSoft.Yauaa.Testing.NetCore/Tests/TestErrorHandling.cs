//-----------------------------------------------------------------------
// <copyright file="TestErrorHandling.cs" company="OrbintSoft">
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
    using OrbintSoft.Yauaa.Analyze;
    using OrbintSoft.Yauaa.Analyzer;
    using OrbintSoft.Yauaa.Debug;
    using OrbintSoft.Yauaa.Parse;
    using OrbintSoft.Yauaa.Testing.Fixtures;
    using OrbintSoft.Yauaa.Tests;
    using System;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="TestErrorHandling" />
    /// </summary>
    public class TestErrorHandling : IClassFixture<LogFixture>
    {
        /// <summary>
        /// The RunTest
        /// </summary>
        /// <param name="directory">The directory<see cref="string"/></param>
        /// <param name="file">The file<see cref="string"/></param>
        /// <param name="expectedMessage">The expectedMessage<see cref="string"/></param>
        private void RunTest(string directory, string file, string expectedMessage)
        {
            Action a = new Action(() =>
            {
                UserAgentAnalyzerTester uaa = new UserAgentAnalyzerTester(directory, file);
                uaa.RunTests(false, false);
            });
            a.Should().Throw<InvalidParserConfigurationException>().Where(e => e.Message.Contains(expectedMessage));
        }

        /// <summary>
        /// The CheckNoFile
        /// </summary>
        [Fact]
        public void CheckNoFile()
        {
            RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "ThisOneDoesNotExist---Really.yaml", "Unable to find ANY config files");
        }

        /// <summary>
        /// The CheckEmptyFile
        /// </summary>
        [Fact]
        public void CheckEmptyFile()
        {
            RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "EmptyFile.yaml", "The file EmptyFile.yaml is empty");
        }

        /// <summary>
        /// The CheckFileIsNotAMap
        /// </summary>
        [Fact]
        public void CheckFileIsNotAMap()
        {
            RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "FileIsNotAMap.yaml", "Yaml config problem.(FileIsNotAMap.yaml:21): The value should be a sequence but it is a Mapping");
        }

        /// <summary>
        /// The CheckLookupSetMissing
        /// </summary>
        [Fact]
        public void CheckLookupSetMissing()
        {
            RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "LookupSetMissing.yaml", "Missing lookupSet");
        }

        /// <summary>
        /// The CheckBadEntry
        /// </summary>
        [Fact]
        public void CheckBadEntry()
        {
            RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "BadEntry.yaml", "Found unexpected config entry:");
        }

        /// <summary>
        /// The CheckLookupMissing
        /// </summary>
        [Fact]
        public void CheckLookupMissing()
        {
            RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "LookupMissing.yaml", "Missing lookup");
        }

        [Fact]
        public void CheckLookupPrefixMissing()
        {
            RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "LookupPrefixMissing.yaml", "Missing lookup");
        }

        [Fact]
        public void CheckIsInLookupPrefixMissing()
        {
            RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "IsInLookupPrefixMissing.yaml", "Missing lookup");
        }

        [Fact]
        public void CheckLookupDuplicateKey()
        {
            RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "LookupDuplicateKey.yaml", "An item with the same key has already been added");
        }


        /// <summary>
        /// The CheckFixedStringLookupMissing
        /// </summary>
        [Fact]
        public void CheckFixedStringLookupMissing()
        {
            RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "FixedStringLookupMissing.yaml", "Missing lookup");
        }

        /// <summary>
        /// The CheckNoExtract
        /// </summary>
        [Fact]
        public void CheckNoExtract()
        {
            RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "NoExtract.yaml", "Matcher does not extract anything");
        }

        /// <summary>
        /// The CheckInvalidExtract
        /// </summary>
        [Fact]
        public void CheckInvalidExtract()
        {
            RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "InvalidExtract.yaml", "Invalid extract config line: agent.text=\"foo\"");
        }

        /// <summary>
        /// The CheckNoTestInput
        /// </summary>
        [Fact]
        public void CheckNoTestInput()
        {
            RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "NoTestInput.yaml", "Test is missing input");
        }

        /// <summary>
        /// The CheckSyntaxErrorRequire
        /// </summary>
        [Fact]
        public void CheckSyntaxErrorRequire()
        {
            RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "SyntaxErrorRequire.yaml", "Syntax error");
        }

        /// <summary>
        /// The CheckSyntaxErrorExtract1
        /// </summary>
        [Fact]
        public void CheckSyntaxErrorExtract1()
        {
            RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "SyntaxErrorExtract1.yaml", "Syntax error");
        }

        /// <summary>
        /// The CheckSyntaxErrorExtract2
        /// </summary>
        [Fact]
        public void CheckSyntaxErrorExtract2()
        {
            RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "SyntaxErrorExtract2.yaml", "Invalid extract config line");
        }

        /// <summary>
        /// The CheckSyntaxErrorVariable1
        /// </summary>
        [Fact]
        public void CheckSyntaxErrorVariable1()
        {
            RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "SyntaxErrorVariable1.yaml", "Syntax error");
        }

        /// <summary>
        /// The CheckSyntaxErrorVariable2
        /// </summary>
        [Fact]
        public void CheckSyntaxErrorVariable2()
        {
            RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "SyntaxErrorVariable2.yaml", "Invalid variable config line:");
        }

        /// <summary>
        /// The CheckSyntaxErrorVariableBackReference
        /// </summary>
        [Fact]
        public void CheckSyntaxErrorVariableBackReference()
        {
            RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "Variable-BackReference.yaml", "Syntax error");
        }

        /// <summary>
        /// The CheckSyntaxErrorVariableBadDefinition
        /// </summary>
        [Fact]
        public void CheckSyntaxErrorVariableBadDefinition()
        {
            RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "Variable-BadDefinition.yaml", "Invalid variable config line:");
        }

        /// <summary>
        /// The CheckSyntaxErrorVariableFixedString
        /// </summary>
        [Fact]
        public void CheckSyntaxErrorVariableFixedString()
        {
            RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "Variable-FixedString.yaml", "Syntax error");
        }

        /// <summary>
        /// The MethodInputValidation
        /// </summary>
        [Fact]
        public void MethodInputValidation()
        {
            UserAgentAnalyzer uaa = UserAgentAnalyzer.NewBuilder()
                .WithField("AgentClass")
                .Build();

            UserAgent agent = uaa.Parse((string)null);
            agent.Should().NotBeNull();
            agent.UserAgentString.Should().BeNull();

            agent = uaa.Parse((UserAgent)null);
            agent.Should().BeNull();

            EvilManualUseragentStringHacks.FixIt(null).Should().BeNull();
        }
    }
}
