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
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Debug;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Parse;
using OrbintSoft.Yauaa.Analyzer.Test.Fixtures;
using System;
using Xunit;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS
{
    public class TestErrorHandling : IClassFixture<LogFixture>
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
