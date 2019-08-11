//-----------------------------------------------------------------------
// <copyright file="TestErrorHandling.cs" company="OrbintSoft">
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
namespace OrbintSoft.Yauaa.Testing.Tests.Basic
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
    /// With this test class, I verify that all exceptions are handled and thrown as expected in case of invalid confguration or misusage.
    /// </summary>
    public class TestErrorHandling : IClassFixture<LogFixture>
    {
        /// <summary>
        /// This method is an helper that runs the tests passed in yaml definitions and check if they throw an <see cref="InvalidParserConfigurationException"/> with the expected message.
        /// </summary>
        /// <param name="directory">The directory where yaml definitions are contained</param>
        /// <param name="file">The name of file you want load</param>
        /// <param name="expectedMessage">The expected message thwrown by the exception<see cref="string"/></param>
        private void RunTest(string directory, string file, string expectedMessage)
        {
            var a = new Action(() =>
            {
                var uaa = new UserAgentAnalyzerTester(directory, file);
                uaa.RunTests(false, false);
            });
            a.Should().Throw<InvalidParserConfigurationException>().Where(e => e.Message.Contains(expectedMessage));
        }

        /// <summary>
        /// I check a file that doesn't exist.
        /// </summary>
        [Fact]
        public void CheckNoFile()
        {
            this.RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "ThisOneDoesNotExist---Really.yaml", "No matchers were loaded at all.");
        }

        /// <summary>
        /// I check an empty file.
        /// </summary>
        [Fact]
        public void CheckEmptyFile()
        {
            this.RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "EmptyFile.yaml", "No matchers were loaded at all.");
        }

        /// <summary>
        /// I check a bad definition with a yaml map instead of a sequence.
        /// </summary>
        [Fact]
        public void CheckFileIsNotAMap()
        {
            this.RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "FileIsNotAMap.yaml", "Yaml config problem.(FileIsNotAMap.yaml:21): The value should be a sequence but it is a Mapping");
        }

        /// <summary>
        /// I check a yaml definition with a missing lookupset.
        /// </summary>
        [Fact]
        public void CheckLookupSetMissing()
        {
            this.RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "LookupSetMissing.yaml", "Missing lookupSet");
        }

        /// <summary>
        /// I check a yaml definition with a bad entry.
        /// </summary>
        [Fact]
        public void CheckBadEntry()
        {
            this.RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "BadEntry.yaml", "Found unexpected config entry:");
        }

        /// <summary>
        /// I check a yaml definition with a missing lookup. 
        /// </summary>
        [Fact]
        public void CheckLookupMissing()
        {
            this.RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "LookupMissing.yaml", "Missing lookup");
        }

        /// <summary>
        /// I check a yaml definition with a missing lookup prefix.
        /// </summary>
        [Fact]
        public void CheckLookupPrefixMissing()
        {
            this.RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "LookupPrefixMissing.yaml", "Missing lookup");
        }

        /// <summary>
        /// I check a yaml definition with a missing lookup prefix.
        /// </summary>
        [Fact]
        public void CheckIsInLookupPrefixMissing()
        {
            this.RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "IsInLookupPrefixMissing.yaml", "Missing lookup");
        }

        /// <summary>
        /// I check a yaml definition with a duplicated lookup key.
        /// </summary>
        [Fact]
        public void CheckLookupDuplicateKey()
        {
            this.RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "LookupDuplicateKey.yaml", "An item with the same key has already been added");
        }


        /// <summary>
        /// I check a yaml definition with a fixed string and missing lookup.
        /// </summary>
        [Fact]
        public void CheckFixedStringLookupMissing()
        {
            this.RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "FixedStringLookupMissing.yaml", "Missing lookup");
        }

        /// <summary>
        /// I check a yaml definition where matcher doesn't extract anything.
        /// </summary>
        [Fact]
        public void CheckNoExtract()
        {
            this.RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "NoExtract.yaml", "Matcher does not extract anything");
        }

        /// <summary>
        /// I check a yaml definition with an invalid extract field.
        /// </summary>
        [Fact]
        public void CheckInvalidExtract()
        {
            this.RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "InvalidExtract.yaml", "Invalid extract config line: agent.text=\"foo\"");
        }

        /// <summary>
        /// I check a yaml definition with a test and missing input.
        /// </summary>
        [Fact]
        public void CheckNoTestInput()
        {
            this.RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "NoTestInput.yaml", "Test is missing input");
        }

        /// <summary>
        /// I test a yaml definition with a syntx error in require.
        /// </summary>
        [Fact]
        public void CheckSyntaxErrorRequire()
        {
            this.RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "SyntaxErrorRequire.yaml", "Syntax error");
        }

        /// <summary>
        /// I test a yaml definition with a syntx error in the extract field.
        /// </summary>
        [Fact]
        public void CheckSyntaxErrorExtract1()
        {
            this.RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "SyntaxErrorExtract1.yaml", "Syntax error");
        }

        /// <summary>
        /// I test a yaml definition with another syntx error in the extract field.
        /// </summary>
        [Fact]
        public void CheckSyntaxErrorExtract2()
        {
            this.RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "SyntaxErrorExtract2.yaml", "Invalid extract config line");
        }

        /// <summary>
        /// I test a yaml definition with a syntx error in the variable.
        /// </summary>
        [Fact]
        public void CheckSyntaxErrorVariable1()
        {
            this.RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "SyntaxErrorVariable1.yaml", "Syntax error");
        }

        /// <summary>
        /// I test a yaml definition with another syntx error in the variable.
        /// </summary>
        [Fact]
        public void CheckSyntaxErrorVariable2()
        {
            this.RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "SyntaxErrorVariable2.yaml", "Invalid variable config line:");
        }

        /// <summary>
        /// I test a yaml definition with a variable back reference.
        /// </summary>
        [Fact]
        public void CheckSyntaxErrorVariableBackReference()
        {
            this.RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "Variable-BackReference.yaml", "Syntax error");
        }

        /// <summary>
        /// I test a yaml definition with a bad defined variable.
        /// </summary>
        [Fact]
        public void CheckSyntaxErrorVariableBadDefinition()
        {
            this.RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "Variable-BadDefinition.yaml", "Invalid variable config line:");
        }

        /// <summary>
        /// I test a yaml definition with a syntax error in a fixed string variable.
        /// </summary>
        [Fact]
        public void CheckSyntaxErrorVariableFixedString()
        {
            this.RunTest(Config.RESOURCES_PATH + "/BadDefinitions", "Variable-FixedString.yaml", "Syntax error");
        }

        /// <summary>
        /// I test tht user agent analyzer works with null inputs.
        /// </summary>
        [Fact]
        public void MethodInputValidation()
        {
            var uaa = UserAgentAnalyzer.NewBuilder()
                .WithField("AgentClass")
                .Build();

            var agent = uaa.Parse((string)null);
            agent.Should().NotBeNull();
            agent.UserAgentString.Should().BeNull();

            agent = uaa.Parse((UserAgent)null);
            agent.Should().BeNull();

            EvilManualUseragentStringHacks.FixIt(null).Should().BeNull();
        }
    }
}
