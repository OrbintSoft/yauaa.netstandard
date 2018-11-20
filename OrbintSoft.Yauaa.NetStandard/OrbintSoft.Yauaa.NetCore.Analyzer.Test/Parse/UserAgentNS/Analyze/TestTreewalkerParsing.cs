//<copyright file="TestTreewalkerParsing.cs" company="OrbintSoft">
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
//<date>2018, 10, 8, 11:06</date>
//<summary></summary>

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS.Analyze
{
    using FluentAssertions;
    using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS;
    using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze;
    using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze.TreeWalker;
    using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze.TreeWalker.Steps;
    using OrbintSoft.Yauaa.Analyzer.Test.Fixtures;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="TestTreewalkerParsing" />
    /// </summary>
    public class TestTreewalkerParsing : IClassFixture<LogFixture>
    {
        /// <summary>
        /// The ValidateWalkPathParsing
        /// </summary>
        [Fact]
        public void ValidateWalkPathParsing()
        {
            string path = "IsNull[LookUp[TridentVersions;agent.(1)product.(2-4)comments.(*)product.name[1]=\"Trident\"" +
                "[2-3]~\"Foo\"^.(*)version[-2]{\"7.\";\"DefaultValue\"]]";

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
                "IsNull()",
                "WordRange([2:3])",
                "Contains(foo)",
                "Up()",
                "Down([1:5]version)",
                "WordRange([1:2])",
                "StartsWith(7.)",
                "Lookup(@TridentVersions ; default=DefaultValue)",
            };

            CheckPath(path, expectedHashEntries, expectedWalkList);
        }

        /// <summary>
        /// The ValidateWalkPathParsingRange
        /// </summary>
        [Fact]
        public void ValidateWalkPathParsingRange()
        {
            string path = "IsNull[LookUp[TridentVersions;agent.(1)product.(2-4)comments.(*)product.name[1]=\"Trident\"" +
                "[2-3]~\"Foo\"^.(*)version[2]{\"7.\";\"DefaultValue\"]]";

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
                "IsNull()",
                "WordRange([2:3])",
                "Contains(foo)",
                "Up()",
                "Down([1:5]version)",
                "WordRange([2:2])",
                "StartsWith(7.)",
                "Lookup(@TridentVersions ; default=DefaultValue)",
            };

            CheckPath(path, expectedHashEntries, expectedWalkList);
        }

        /// <summary>
        /// The ValidateStartsWithLength
        /// </summary>
        [Fact]
        public void ValidateStartsWithLength()
        {
            string value = "OneTwoThree";

            for (int i = 1; i <= value.Length; i++)
            {
                string matchValue = value.Substring(0, i);
                string hashValue = matchValue.Substring(0, Math.Min(UserAgentAnalyzerDirect.MAX_PREFIX_HASH_MATCH, matchValue.Length));

                string path = "IsNull[LookUp[TridentVersions;agent.(1)product.(1)name{\"" + matchValue + "\";\"DefaultValue\"]]";

                string[] expectedHashEntries = {
                    "agent.(1)product.(1)name{\"" + hashValue + "\"",
                };

                string[] expectedWalkList;
                if (matchValue.Length > UserAgentAnalyzerDirect.MAX_PREFIX_HASH_MATCH)
                {
                    expectedWalkList = new string[]{
                        "IsNull()",
                        "StartsWith("+matchValue.ToLower()+")",
                        "Lookup(@TridentVersions ; default=DefaultValue)",
                    };
                }
                else
                {
                    expectedWalkList = new string[]{
                        "IsNull()",
                        // Short entries should not appear in the walk list to optimize performance
                        "Lookup(@TridentVersions ; default=DefaultValue)",
                    };
                }

                CheckPath(path, expectedHashEntries, expectedWalkList);
            }
        }

        /// <summary>
        /// The ValidateWalkPathSimpleName
        /// </summary>
        [Fact]
        public void ValidateWalkPathSimpleName()
        {
            string path = "agent.(1)product.(1)name";

            string[] expectedHashEntries = {
                "agent.(1)product.(1)name",
            };

            string[] expectedWalkList = { };

            CheckPath(path, expectedHashEntries, expectedWalkList);
        }

        /// <summary>
        /// The ValidateWalkPathSimpleNameEquals
        /// </summary>
        [Fact]
        public void ValidateWalkPathSimpleNameEquals()
        {
            string path = "agent.(1)product.(1)name=\"Foo\"^.(1-3)version";

            string[] expectedHashEntries = {
                "agent.(1)product.(1)name=\"Foo\"",
            };

            string[] expectedWalkList = {
                "Up()",
                "Down([1:3]version)"
            };

            CheckPath(path, expectedHashEntries, expectedWalkList);
        }

        /// <summary>
        /// The ValidateWalkPathNameSubstring
        /// </summary>
        [Fact]
        public void ValidateWalkPathNameSubstring()
        {
            string path = "agent.(1)product.(1)name[1-2]=\"Foo\"^.(1-3)version";

            string[] expectedHashEntries = {
                "agent.(1)product.(1)name[1-2]=\"Foo\"",
            };

            string[] expectedWalkList = {
                "Up()",
                "Down([1:3]version)"
            };

            CheckPath(path, expectedHashEntries, expectedWalkList);
        }

        /// <summary>
        /// The ValidateWalkPathNameSubstring2
        /// </summary>
        [Fact]
        public void ValidateWalkPathNameSubstring2()
        {
            string path = "agent.(1)product.(1)name[3-5]=\"Foo\"^.(1-3)version";

            string[] expectedHashEntries = {
                "agent.(1)product.(1)name[3-5]=\"Foo\"",
            };

            string[] expectedWalkList = {
                "Up()",
                "Down([1:3]version)"
            };

            CheckPath(path, expectedHashEntries, expectedWalkList);
        }

        /// <summary>
        /// The ValidateWalkAroundTheWorld
        /// </summary>
        [Fact]
        public void ValidateWalkAroundTheWorld()
        {
            string path = "agent.(2-4)product.(1)comments.(5-6)entry.(1)text[2]=\"seven\"^^^<.name=\"foo faa\"^.comments.entry.text[-2]=\"three\"@[1-1]";

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
                "WordRange([1:1])",
            };

            CheckPath(path, expectedHashEntries, expectedWalkList);
        }

        /// <summary>
        /// The ValidateWalkPathParsingCleanVersion
        /// </summary>
        [Fact]
        public void ValidateWalkPathParsingCleanVersion()
        {

            string path = "CleanVersion[LookUp[TridentVersions;agent.(1)product.(2-4)comments.(*)product.name[1-1]=\"Trident\"" +
                "^.(*)version[-2]{\"7.\";\"DefaultValue\"]]";

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

        /// <summary>
        /// The CheckPath
        /// </summary>
        /// <param name="path">The path<see cref="string"/></param>
        /// <param name="expectedHashEntries">The expectedHashEntries<see cref="string[]"/></param>
        /// <param name="expectedWalkList">The expectedWalkList<see cref="string[]"/></param>
        private void CheckPath(string path, string[] expectedHashEntries, string[] expectedWalkList)
        {
            Dictionary<string, IDictionary<string, string>> lookups = new Dictionary<string, IDictionary<string, string>>();
            lookups["TridentVersions"] = new Dictionary<string, string>();

            TestMatcher matcher = new TestMatcher(lookups, new Dictionary<string, ISet<string>>());
            MatcherRequireAction action = new MatcherRequireAction(path, matcher);
            action.Initialize();

            StringBuilder sb = new StringBuilder("\n---------------------------\nActual list (")
                .Append(matcher.reveicedValues.Count)
                .Append(" entries):\n");

            foreach (string actual in matcher.reveicedValues)
            {
                sb.Append(actual).Append('\n');
            }
            sb.Append("---------------------------\n");

            // Validate the expected hash entries (i.e. the first part of the path)
            foreach (string expect in expectedHashEntries)
            {
                matcher.reveicedValues.Contains(expect).Should().BeTrue("\nExpected:\n" + expect + sb.ToString());
            }
            expectedHashEntries.Length.Should().Be(matcher.reveicedValues.Count, "I expect that number of entries");

            // Validate the expected walk list entries (i.e. the dynamic part of the path)
            TreeExpressionEvaluator evaluator = action.EvaluatorForUnitTesting;
            WalkList walkList = evaluator.GetWalkListForUnitTesting();

            Step step = walkList.GetFirstStep();
            foreach (string walkStep in expectedWalkList)
            {
                step.Should().NotBeNull(step + " + walkStep");
                walkStep.Should().Be(step.ToString(), "Wrong step (" + step.ToString() + " should be " + walkStep + ")");
                step = step.GetNextStep();
            }
            step.Should().Be(null);
        }

        /// <summary>
        /// Defines the <see cref="TestMatcher" />
        /// </summary>
        private class TestMatcher : Matcher
        {
            /// <summary>
            /// Defines the reveicedValues
            /// </summary>
            internal readonly List<string> reveicedValues = new List<string>(128);

            /// <summary>
            /// Initializes a new instance of the <see cref="TestMatcher"/> class.
            /// </summary>
            /// <param name="lookups">The lookups<see cref="IDictionary{string, IDictionary{string, string}}"/></param>
            /// <param name="lookupSets">The lookupSets<see cref="IDictionary{string, ISet{string}}"/></param>
            internal TestMatcher(IDictionary<string, IDictionary<string, string>> lookups, IDictionary<string, ISet<string>> lookupSets) : base(null, lookups, lookupSets)
            {
            }

            /// <summary>
            /// The InformMeAbout
            /// </summary>
            /// <param name="matcherAction">The matcherAction<see cref="MatcherAction"/></param>
            /// <param name="keyPattern">The keyPattern<see cref="string"/></param>
            public override void InformMeAbout(MatcherAction matcherAction, string keyPattern)
            {
                reveicedValues.Add(keyPattern);
            }

            /// <summary>
            /// The InformMeAboutPrefix
            /// </summary>
            /// <param name="matcherAction">The matcherAction<see cref="MatcherAction"/></param>
            /// <param name="treeName">The treeName<see cref="string"/></param>
            /// <param name="prefix">The prefix<see cref="string"/></param>
            public override void InformMeAboutPrefix(MatcherAction matcherAction, string treeName, string prefix)
            {
                InformMeAbout(matcherAction, treeName + "{\"" + UserAgentAnalyzerDirect.FirstCharactersForPrefixHash(prefix, UserAgentAnalyzerDirect.MAX_PREFIX_HASH_MATCH) + "\"");
            }

            /// <summary>
            /// The Analyze
            /// </summary>
            /// <param name="userAgent">The userAgent<see cref="UserAgent"/></param>
            public override void Analyze(UserAgent userAgent)
            {
            }

            /// <summary>
            /// The LookingForRange
            /// </summary>
            /// <param name="treeName">The treeName<see cref="string"/></param>
            /// <param name="range">The range<see cref="WordRangeVisitor.Range"/></param>
            public override void LookingForRange(string treeName, WordRangeVisitor.Range range)
            {
            }
        }
    }
}
