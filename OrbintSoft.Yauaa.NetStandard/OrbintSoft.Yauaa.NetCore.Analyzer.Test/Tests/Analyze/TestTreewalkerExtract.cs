using FluentAssertions;
using OrbintSoft.Yauaa.Analyze;
using OrbintSoft.Yauaa.Analyze.TreeWalker;
using OrbintSoft.Yauaa.Analyze.TreeWalker.Steps;
using OrbintSoft.Yauaa.Analyzer;
using OrbintSoft.Yauaa.Testing.Fixtures;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace OrbintSoft.Yauaa.Testing.Tests.Analyze
{
    public class TestTreewalkerExtract : IClassFixture<LogFixture>
    {
        [Fact]
        public void ValidateWalkPathSimpleName()
        {
            string path = "agent.(1)product.(1)name";

            string[] expectedHashEntries = {"agent.(1)product.(1)name"};

            string[] expectedWalkList = {};

            CheckPath(path, expectedHashEntries, expectedWalkList);
        }

        [Fact]
        public void ValidateWalkPathSimpleNameEquals()
        {
            string path = "agent.(1)product.(1)name=\"Foo\"^.(1-3)version";

            string[] expectedHashEntries = { "agent.(1)product.(1)name=\"Foo\""};

            string[] expectedWalkList = { "Up()", "Down([1:3]version)"};

            CheckPath(path, expectedHashEntries, expectedWalkList);
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

            TestMatcher matcher = new TestMatcher(new Dictionary<string, IDictionary<string, string>>(), new Dictionary<string, ISet<string>>());
            MatcherExtractAction action = new MatcherExtractAction("Dummy", 42, path, matcher);
            Action a = new Action(() => action.Initialize());
            a.Should().Throw<InvalidParserConfigurationException>();
        }

        private void CheckPath(string path, string[] expectedHashEntries, string[] expectedWalkList)
        {
            var lookups = new Dictionary<string, IDictionary<string, string>>
            {
                ["TridentVersions"] = new Dictionary<string, string>()
            };

            TestMatcher matcher = new TestMatcher(lookups, new Dictionary<string, ISet<string>>());
            var action = new MatcherExtractAction("Dummy", 42, path, matcher);
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

            expectedHashEntries.Length.Should().Be(matcher.reveicedValues.Count, "Found that number of entries");

            // Validate the expected walk list entries (i.e. the dynamic part of the path)
            TreeExpressionEvaluator evaluator = action.EvaluatorForUnitTesting;
            WalkList walkList = evaluator.GetWalkListForUnitTesting();

            Step step = walkList.GetFirstStep();
            foreach (string walkStep in expectedWalkList)
            {
                step.Should().NotBeNull("Step: " + walkStep);
                walkStep.Should().Be(step.ToString(), "step(" + step.ToString() + " should be " + walkStep + ")");
                step = step.GetNextStep();
            }
            step.Should().BeNull();
        }

        private class TestMatcher: Matcher
        {
            internal readonly IList<string> reveicedValues = new List<string>(128);

            internal TestMatcher(IDictionary<string, IDictionary<string, string>> lookups, IDictionary<string, ISet<string>> lookupSets): base(null, lookups, lookupSets)
            {
            }

       
            public override void InformMeAbout(MatcherAction matcherAction, string keyPattern)
            {
                reveicedValues.Add(keyPattern);
            }

        
            public override void InformMeAboutPrefix(MatcherAction matcherAction, string treeName, string prefix)
            {
                InformMeAbout(matcherAction, treeName + "{\"" + UserAgentAnalyzerDirect.FirstCharactersForPrefixHash(prefix, UserAgentAnalyzerDirect.MAX_PREFIX_HASH_MATCH) + "\"");
            }

        
            public override void Analyze(UserAgent userAgent)
            {
                // Do nothing
            }

 
            public override void LookingForRange(string treeName, WordRangeVisitor.Range range)
            {
                // Do nothing
            }
        }
    }
}
