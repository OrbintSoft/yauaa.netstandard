//-----------------------------------------------------------------------
// <copyright file="TestTreewalkerExtract.cs" company="OrbintSoft">
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
using OrbintSoft.Yauaa.Testing.Fixtures;
using OrbintSoft.Yauaa.Testing.Tests.Analyze.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace OrbintSoft.Yauaa.Testing.Tests.Analyze
{
    /// <summary>
    /// Thi class is used to test Treewalker extract.
    /// </summary>
    public class TestTreewalkerExtract : IClassFixture<LogFixture>
    {
        /// <summary>
        /// I test a simple path, it should extract the name entry and there are no walking lists.
        /// </summary>
        [Fact]
        public void ValidateWalkPathSimpleName()
        {
            var path = "agent.(1)product.(1)name"; // no operations, no walk.

            var expectedHashEntries = new string[]{"agent.(1)product.(1)name"};

            var expectedWalkList = new string[]{};

            this.CheckPath(path, expectedHashEntries, expectedWalkList);
        }

        /// <summary>
        /// I test a simple open start range path, it should extract all entries in the range, no walink list.
        /// </summary>
        [Fact]
        public void ValidateWalkPathSimpleNameOpenStartRange()
        {
            //From open to 3, expected 1, 2 ,3
            var path = "agent.(-3)product.(1)name";

            var expectedHashEntries = new string[]{
                "agent.(1)product.(1)name",
                "agent.(2)product.(1)name",
                "agent.(3)product.(1)name",
            };

            var expectedWalkList = new string[]{};

            this.CheckPath(path, expectedHashEntries, expectedWalkList);
        }

        /// <summary>
        /// I test a simple open end range path, it should extract all entries in the range, no walink list.
        /// </summary>
        [Fact]
        public void ValidateWalkPathSimpleNameOpenEndRange()
        {
            var path = "agent.(5-)product.(1)name";

            var expectedHashEntries = new string[] {
                "agent.(5)product.(1)name",
                "agent.(6)product.(1)name",
                "agent.(7)product.(1)name",
                "agent.(8)product.(1)name",
                "agent.(9)product.(1)name",
                "agent.(10)product.(1)name",
            };

            var expectedWalkList = new string[] { };

            this.CheckPath(path, expectedHashEntries, expectedWalkList);
        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void ValidateWalkPathSimpleNameEquals()
        {
            var path = "agent.(1)product.(1)name=\"Foo\"^.(1-3)version";

            var expectedHashEntries = new string[] { "agent.(1)product.(1)name=\"Foo\""};

            var expectedWalkList = new string[] { "Up()", "Down([1:3]version)"};

            this.CheckPath(path, expectedHashEntries, expectedWalkList);
        }

        [Fact]
        public void ValidateWalkPathNameSubstring()
        {
            string path = "agent.(1)product.(1)name[1-2]=\"Foo\"^.(1-3)version";

            string[] expectedHashEntries = { "agent.(1)product.(1)name[1-2]=\"Foo\"" };

            string[] expectedWalkList = { "Up()", "Down([1:3]version)" };

            CheckPath(path, expectedHashEntries, expectedWalkList);
        }

        [Fact]
        public void ValidateWalkPathNameSubstring2()
        {
            string path = "agent.(1)product.(1)name[3-5]=\"Foo\"^.(1-3)version";

            string[] expectedHashEntries = { "agent.(1)product.(1)name[3-5]=\"Foo\"" };

            string[] expectedWalkList = { "Up()", "Down([1:3]version)" };

            CheckPath(path, expectedHashEntries, expectedWalkList);
        }

        [Fact]
        public void ValidateWalkAroundTheWorld()
        {
            string path = "agent.(2-4)product.(1)comments.(5-6)entry.(1)text[2]=\"seven\"^^^<.name=\"foo faa\"^.comments.entry.text[-2]=\"three\"@[1-2]";

            string[] expectedHashEntries = {
                "agent.(2)product.(1)comments.(5)entry.(1)text[2-2]=\"seven\"",
                "agent.(2)product.(1)comments.(6)entry.(1)text[2-2]=\"seven\"",
                "agent.(3)product.(1)comments.(5)entry.(1)text[2-2]=\"seven\"",
                "agent.(3)product.(1)comments.(6)entry.(1)text[2-2]=\"seven\"",
                "agent.(4)product.(1)comments.(5)entry.(1)text[2-2]=\"seven\"",
                "agent.(4)product.(1)comments.(6)entry.(1)text[2-2]=\"seven\"",
            };

            string[] expectedWalkList = {
                "Up()",
                "Up()",
                "Up()",
                "Prev(1)",
                "Down([1:1]name)",
                "Equals(foo faa)",
                "Up()",
                "Down([1:2]comments)",
                "Down([1:20]entry)",
                "Down([1:8]text)",
                "WordRange([1:2])",
                "Equals(three)",
                "BackToFull()",
                "WordRange([1:2])",
            };

            CheckPath(path, expectedHashEntries, expectedWalkList);
        }

        [Fact]
        public void ValidateWalkPathParsingCleanVersion()
        {

            string path = "CleanVersion[LookUp[TridentVersions;agent.(1)product.(2-4)comments.(*)product.name[1-1]=\"Trident\"" + "^.(*)version[-2]{\"7.\";\"DefaultValue\"]]";

            string[] expectedHashEntries = {
                "agent.(1)product.(2)comments.(1)product.(1)name[1-1]=\"Trident\"",
                "agent.(1)product.(2)comments.(2)product.(1)name[1-1]=\"Trident\"",
                "agent.(1)product.(2)comments.(3)product.(1)name[1-1]=\"Trident\"",
                "agent.(1)product.(2)comments.(4)product.(1)name[1-1]=\"Trident\"",
                "agent.(1)product.(2)comments.(5)product.(1)name[1-1]=\"Trident\"",
                "agent.(1)product.(2)comments.(6)product.(1)name[1-1]=\"Trident\"",
                "agent.(1)product.(2)comments.(7)product.(1)name[1-1]=\"Trident\"",
                "agent.(1)product.(2)comments.(8)product.(1)name[1-1]=\"Trident\"",
                "agent.(1)product.(2)comments.(9)product.(1)name[1-1]=\"Trident\"",
                "agent.(1)product.(2)comments.(10)product.(1)name[1-1]=\"Trident\"",
                "agent.(1)product.(3)comments.(1)product.(1)name[1-1]=\"Trident\"",
                "agent.(1)product.(3)comments.(2)product.(1)name[1-1]=\"Trident\"",
                "agent.(1)product.(3)comments.(3)product.(1)name[1-1]=\"Trident\"",
                "agent.(1)product.(3)comments.(4)product.(1)name[1-1]=\"Trident\"",
                "agent.(1)product.(3)comments.(5)product.(1)name[1-1]=\"Trident\"",
                "agent.(1)product.(3)comments.(6)product.(1)name[1-1]=\"Trident\"",
                "agent.(1)product.(3)comments.(7)product.(1)name[1-1]=\"Trident\"",
                "agent.(1)product.(3)comments.(8)product.(1)name[1-1]=\"Trident\"",
                "agent.(1)product.(3)comments.(9)product.(1)name[1-1]=\"Trident\"",
                "agent.(1)product.(3)comments.(10)product.(1)name[1-1]=\"Trident\"",
                "agent.(1)product.(4)comments.(1)product.(1)name[1-1]=\"Trident\"",
                "agent.(1)product.(4)comments.(2)product.(1)name[1-1]=\"Trident\"",
                "agent.(1)product.(4)comments.(3)product.(1)name[1-1]=\"Trident\"",
                "agent.(1)product.(4)comments.(4)product.(1)name[1-1]=\"Trident\"",
                "agent.(1)product.(4)comments.(5)product.(1)name[1-1]=\"Trident\"",
                "agent.(1)product.(4)comments.(6)product.(1)name[1-1]=\"Trident\"",
                "agent.(1)product.(4)comments.(7)product.(1)name[1-1]=\"Trident\"",
                "agent.(1)product.(4)comments.(8)product.(1)name[1-1]=\"Trident\"",
                "agent.(1)product.(4)comments.(9)product.(1)name[1-1]=\"Trident\"",
                "agent.(1)product.(4)comments.(10)product.(1)name[1-1]=\"Trident\"",
            };

            string[] expectedWalkList = {
                "Up()",
                "Down([1:5]version)",
                "WordRange([1:2])",
                "StartsWith(7.)",
                "Lookup(@TridentVersions ; default=DefaultValue)",
                "CleanVersion()"
            };

            CheckPath(path, expectedHashEntries, expectedWalkList);
        }

        [Fact]
        public void ValidateParseError()
        {
            string path = "agent.product.(1)name[3]\"Safari\"";

            var matcher = new TestMatcher(new Dictionary<string, IDictionary<string, string>>(), new Dictionary<string, ISet<string>>());
            MatcherExtractAction action = new MatcherExtractAction("Dummy", 42, path, matcher);
            Action a = new Action(() => action.Initialize());
            a.Should().Throw<InvalidParserConfigurationException>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="expectedHashEntries"></param>
        /// <param name="expectedWalkList"></param>
        private void CheckPath(string path, string[] expectedHashEntries, string[] expectedWalkList)
        {
            var lookups = new Dictionary<string, IDictionary<string, string>>
            {
                ["TridentVersions"] = new Dictionary<string, string>()
            };

            var matcher = new TestMatcher(lookups, new Dictionary<string, ISet<string>>());
            var action = new MatcherExtractAction("Dummy", 42, path, matcher);
            action.Initialize();

            var sb = new StringBuilder("\n---------------------------\nActual list (")
                        .Append(matcher.receivedValues.Count)
                        .Append(" entries):\n");

            foreach (var actual in matcher.receivedValues)
            {
                sb.Append(actual).Append('\n');
            }
            sb.Append("---------------------------\n");

            // Validate the expected hash entries (i.e. the first part of the path)
            foreach (var expect in expectedHashEntries)
            {
                matcher.receivedValues.Contains(expect).Should().BeTrue("\nExpected:\n" + expect + sb.ToString());
            }

            expectedHashEntries.Length.Should().Be(matcher.receivedValues.Count, "Found that number of entries");

            // Validate the expected walk list entries (i.e. the dynamic part of the path)
            var evaluator = action.EvaluatorForUnitTesting;
            var walkList = evaluator.WalkListForUnitTesting;

            var step = walkList.FirstStep;
            foreach (var walkStep in expectedWalkList)
            {
                step.Should().NotBeNull("Step: " + walkStep);
                walkStep.Should().Be(step.ToString(), "step(" + step.ToString() + " should be " + walkStep + ")");
                step = step.NextStep;
            }
            step.Should().BeNull();
        }
    }
}
