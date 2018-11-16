﻿//<copyright file="TestYamlParsing.cs" company="OrbintSoft">
//	Yet Another UserAgent Analyzer.NET Standard
//	Porting realized by Stefano Balzarotti, Copyright (C) OrbintSoft
//
//	Original Author and License:
//
//	Yet Another UserAgent Analyzer
//	Copyright(C) 2013-2018 Niels Basjes
//
//	Licensed under the Apache License, Version 2.0 (the "License");
//	you may not use this file except in compliance with the License.
//	You may obtain a copy of the License at
//
//	https://www.apache.org/licenses/LICENSE-2.0
//
//	Unless required by applicable law or agreed to in writing, software
//	distributed under the License is distributed on an "AS IS" BASIS,
//	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//	See the License for the specific language governing permissions and
//	limitations under the License.
//
//</copyright>
//<author>Stefano Balzarotti, Niels Basjes</author>
//<date>2018, 11, 14, 20:22</date>
//<summary></summary>

using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS;
using System;
using FluentAssertions;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze;
using Xunit;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS.Utils
{
    public class TestYamlParsing
    {
        private void RunTest(string inputFilename, string message)
        {
            UserAgentAnalyzer uaa = UserAgentAnalyzer
                .NewBuilder()
                .DropDefaultResources()
                .KeepTests()
                .DelayInitialization()
                .Build();

            Action a = new Action(() => { uaa.LoadResources("YamlResources/YamlParsingTests", inputFilename); });
            a.Should().Throw<InvalidParserConfigurationException>().Where(e => e.Message.Contains(message));

        }
        [Fact]
        public void TestEmpty()
        {
            RunTest("Empty.yaml", "The file Empty.yaml is empty");
        }

        [Fact]
        public void TestTopNotConfig()
        {
            RunTest("TopNotConfig.yaml", "The top level entry MUST be 'config'");
        }

        [Fact]
        public void TestNotAMapFile()
        {
            RunTest("NotAMapFile.yaml", "File must be a Map");
        }

        [Fact]
        public void TestBadConfig1()
        {
            RunTest("BadConfig1.yaml", "The value should be a sequence but it is a Scalar");
        }

        [Fact]
        public void TestBadConfig2()
        {
            RunTest("BadConfig2.yaml", "The entry MUST be a mapping");
        }

        [Fact]
        public void TestInputAbsent()
        {
            RunTest("InputAbsent.yaml", "Test is missing input");
        }

        [Fact]
        public void TestInputNotString()
        {
            RunTest("InputNotString.yaml", "The value should be a string but it is a Sequence");
        }

        [Fact]
        public void TestNotAMap()
        {
            RunTest("NotAMap.yaml", "The value should be a map but it is a Scalar");
        }

        [Fact]
        public void TestNotSingle()
        {
            RunTest("NotSingle.yaml", "There must be exactly 1 value in the list");
        }

        [Fact]
        public void TestNotStringList1()
        {
            RunTest("NotStringList1.yaml", "The value should be a string but it is a Mapping");
        }

        [Fact]
        public void TestNotStringList2()
        {
            RunTest("NotStringList2.yaml", "The provided node must be a sequence but it is a Scalar");
        }

        [Fact]
        public void TestKeyNotString()
        {
            RunTest("KeyNotString.yaml", "The key should be a string but it is a Sequence");
        }

        [Fact]
        public void TestParseError()
        {
            RunTest("ParseError.yaml", "Parse error in the file ParseError.yaml: ");
        }        
    }
}