//-----------------------------------------------------------------------
// <copyright file="TestYamlParsing.cs" company="OrbintSoft">
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
namespace OrbintSoft.Yauaa.Testing.Tests.Utils
{
    using FluentAssertions;
    using OrbintSoft.Yauaa.Analyze;
    using OrbintSoft.Yauaa.Analyzer;
    using OrbintSoft.Yauaa.Tests;
    using System;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="TestYamlParsing" />
    /// </summary>
    public class TestYamlParsing
    {
        /// <summary>
        /// The RunTest
        /// </summary>
        /// <param name="inputFilename">The inputFilename<see cref="string"/></param>
        /// <param name="message">The message<see cref="string"/></param>
        private void RunTest(string inputFilename, string message)
        {
            UserAgentAnalyzer uaa = UserAgentAnalyzer
                .NewBuilder()
                .DropDefaultResources()
                .KeepTests()
                .DelayInitialization()
                .Build();

            Action a = new Action(() => { uaa.LoadResources(Config.RESOURCES_PATH + "/YamlParsingTests", inputFilename); });
            a.Should().Throw<InvalidParserConfigurationException>().Where(e => e.Message.Contains(message));
        }

        /// <summary>
        /// The TestEmpty
        /// </summary>
        [Fact]
        public void TestEmpty()
        {
            RunTest("Empty.yaml", "The file Empty.yaml is empty");
        }

        /// <summary>
        /// The TestTopNotConfig
        /// </summary>
        [Fact]
        public void TestTopNotConfig()
        {
            RunTest("TopNotConfig.yaml", "The top level entry MUST be 'config'");
        }

        /// <summary>
        /// The TestNotAMapFile
        /// </summary>
        [Fact]
        public void TestNotAMapFile()
        {
            RunTest("NotAMapFile.yaml", "File must be a Map");
        }

        /// <summary>
        /// The TestBadConfig1
        /// </summary>
        [Fact]
        public void TestBadConfig1()
        {
            RunTest("BadConfig1.yaml", "The value should be a sequence but it is a Scalar");
        }

        /// <summary>
        /// The TestBadConfig2
        /// </summary>
        [Fact]
        public void TestBadConfig2()
        {
            RunTest("BadConfig2.yaml", "The entry MUST be a mapping");
        }

        /// <summary>
        /// The TestInputAbsent
        /// </summary>
        [Fact]
        public void TestInputAbsent()
        {
            RunTest("InputAbsent.yaml", "Test is missing input");
        }

        /// <summary>
        /// The TestInputNotString
        /// </summary>
        [Fact]
        public void TestInputNotString()
        {
            RunTest("InputNotString.yaml", "The value should be a string but it is a Sequence");
        }

        /// <summary>
        /// The TestNotAMap
        /// </summary>
        [Fact]
        public void TestNotAMap()
        {
            RunTest("NotAMap.yaml", "The value should be a map but it is a Scalar");
        }

        /// <summary>
        /// The TestNotSingle
        /// </summary>
        [Fact]
        public void TestNotSingle()
        {
            RunTest("NotSingle.yaml", "There must be exactly 1 value in the list");
        }

        /// <summary>
        /// The TestNotStringList1
        /// </summary>
        [Fact]
        public void TestNotStringList1()
        {
            RunTest("NotStringList1.yaml", "The value should be a string but it is a Mapping");
        }

        /// <summary>
        /// The TestNotStringList2
        /// </summary>
        [Fact]
        public void TestNotStringList2()
        {
            RunTest("NotStringList2.yaml", "The provided node must be a sequence but it is a Scalar");
        }

        /// <summary>
        /// The TestKeyNotString
        /// </summary>
        [Fact]
        public void TestKeyNotString()
        {
            RunTest("KeyNotString.yaml", "The key should be a string but it is a Sequence");
        }

        /// <summary>
        /// The TestParseError
        /// </summary>
        [Fact]
        public void TestParseError()
        {
            RunTest("ParseError.yaml", "Parse error in the file ParseError.yaml: ");
        }
    }
}
