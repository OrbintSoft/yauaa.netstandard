using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze.TreeWalker;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze.TreeWalker.Steps;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS.Analyze
{
    public class TestTreewalkerParsing
    {
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

        private void CheckPath(string path, string[] expectedHashEntries, string[] expectedWalkList)
        {
            Dictionary<string, Dictionary<string, string>> lookups = new Dictionary<string, Dictionary<string, string>>();
            lookups["TridentVersions"] = new Dictionary<string, string>();

            TestMatcher matcher = new TestMatcher(lookups, new Dictionary<string, HashSet<string>>());
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
            TreeExpressionEvaluator evaluator = action.GetEvaluatorForUnitTesting();
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

        private class TestMatcher: Matcher
        {
            internal readonly List<string> reveicedValues = new List<string>(128);

            internal TestMatcher(Dictionary<string, Dictionary<string, string>> lookups, Dictionary<string, HashSet<string>> lookupSets) : base(null, lookups, lookupSets)
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
