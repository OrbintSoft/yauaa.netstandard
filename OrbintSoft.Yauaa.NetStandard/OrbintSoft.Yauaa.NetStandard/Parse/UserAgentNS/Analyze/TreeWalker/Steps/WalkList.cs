using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using log4net;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze.TreeWalker.Steps.Compare;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze.TreeWalker.Steps.Lookup;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze.TreeWalker.Steps.Value;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze.TreeWalker.Steps.Walk;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Antlr4Source;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze.TreeWalker.Steps
{
    /// <summary>
    /// This class gets the symbol table (1 value) uses that to evaluate
    /// the expression against the parsed user agent
    /// </summary>
    public class WalkList
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(WalkList));

        private readonly Dictionary<string, Dictionary<string, string>> lookups;
        private readonly Dictionary<string, HashSet<string>> lookupSets;
        private readonly List<Step> steps = new List<Step>();

        private readonly bool verbose;

        public sealed class WalkResult
        {
            private readonly IParseTree tree;
            private readonly string value;

            public WalkResult(IParseTree tree, string value)
            {
                this.tree = tree;
                this.value = value;
                //            if (value == null || tree == null ) {
                //                throw new IllegalStateException("An invalid WalkResult was created :" + this.toString());
                //            }
            }

            public IParseTree GetTree()
            {
                return tree;
            }

            public string GetValue()
            {
                return value;
            }


            public override string ToString()
            {
                return "WalkResult{" +
                    "tree=" + (tree == null ? ">>>NULL<<<" : tree.GetText()) +
                    ", value=" + (value == null ? ">>>NULL<<<" : '\'' + value + '\'') +
                    '}';
            }
        }

        public WalkList(ParserRuleContext requiredPattern, Dictionary<String, Dictionary<String, String>> lookups, Dictionary<String, HashSet<String>> lookupSets, bool verbose)
        {
            this.lookups = lookups;
            this.lookupSets = lookupSets;
            this.verbose = verbose;
            // Generate the walkList from the requiredPattern
            new WalkListBuilder(this).Visit(requiredPattern);
            LinkSteps();

            int i = 1;
            if (verbose)
            {
                LOG.Info("------------------------------------");
                LOG.Info("Required: " + requiredPattern.GetText());
                foreach (Step step in steps)
                {
                    step.SetVerbose(true);
                    LOG.Info(string.Format("{0}: {1}", i++, step));
                }
            }
        }

        private void LinkSteps()
        {
            Step nextStep = null;
            for (int i = steps.Count - 1; i >= 0; i--)
            {
                Step current = steps[i];
                current.SetNextStep(i, nextStep);
                nextStep = current;
            }
        }

        public WalkResult Walk(IParseTree tree, String value)
        {
            if (steps.Count == 0)
            {
                return new WalkResult(tree, value);
            }
            Step firstStep = steps[0];
            if (verbose)
            {
                Step.LOG.Info(string.Format("Tree: >>>{0}<<<", tree.GetText()));
                Step.LOG.Info(string.Format("Enter step: {0}", firstStep));
            }
            WalkResult result = firstStep.Walk(tree, value);
            if (verbose)
            {
                Step.LOG.Info(string.Format("Leave step ({0}): {1}", result == null ? "-" : "+", firstStep));
            }
            return null;
        }

        public Step GetFirstStep()
        {
            return steps == null || steps.Count == 0 ? null : steps[0];
        }

        private bool? usesIsNull = null;

        public bool UsesIsNull
        {
            get
            {
                if (usesIsNull != null)
                {
                    return usesIsNull.Value;
                }

                Step step = GetFirstStep();
                while (step != null)
                {
                    if (step is StepIsNull)
                    {
                        usesIsNull = true;
                        return true;
                    }
                    step = step.GetNextStep();
                }
                usesIsNull = false;
                return false;
            }
        }

        public override string ToString()
        {
            if (steps.Count == 0)
            {
                return "Empty";
            }
            StringBuilder sb = new StringBuilder(128);
            foreach (Step step in steps)
            {
                sb.Append(" --> ").Append(step);
            }
            return sb.ToString();
        }

        private class WalkListBuilder: UserAgentTreeWalkerBaseVisitor<object> {
            // Because we are jumping in 'mid way' we need to skip creating steps until that point.
            bool foundHashEntryPoint = false;

            private readonly WalkList walkList = null;

            public WalkListBuilder(WalkList walkList) :base()
            {
                this.walkList = walkList;
            }

            private void FromHereItCannotBeInHashMapAnymore()
            {
                foundHashEntryPoint = true;
            }

            private bool StillGoingToHashMap()
            {
                return !foundHashEntryPoint;
            }

            private void Add(Step step)
            {
                if (foundHashEntryPoint)
                {
                    walkList.steps.Add(step);
                }
            }

            private void VisitNext(UserAgentTreeWalkerParser.PathContext nextStep)
            {
                if (nextStep != null)
                {
                    Visit(nextStep);
                }
            }

            public override object VisitMatcherPath([NotNull] UserAgentTreeWalkerParser.MatcherPathContext context)
            {
                Visit(context.basePath());
                return null; // Void
            }

            public override object VisitMatcherPathLookup([NotNull] UserAgentTreeWalkerParser.MatcherPathLookupContext context)
            {
                Visit(context.matcher());

                FromHereItCannotBeInHashMapAnymore();

                string lookupName = context.lookup.Text;
                Dictionary<string, string> lookup = walkList.lookups[lookupName];
                if (lookup == null)
                {
                    throw new InvalidParserConfigurationException("Missing lookup \"" + context.lookup.Text + "\" ");
                }

                string defaultValue = null;
                if (context.defaultValue != null)
                {
                    defaultValue = context.defaultValue.Text;
                }

                Add(new StepLookup(lookupName, lookup, defaultValue));
                return null; // Void
            }

            public override object VisitMatcherCleanVersion([NotNull] UserAgentTreeWalkerParser.MatcherCleanVersionContext context)
            {
                Visit(context.matcher());
                FromHereItCannotBeInHashMapAnymore();
                Add(new StepCleanVersion());
                return null; // Void
            }

            public override object VisitMatcherNormalizeBrand([NotNull] UserAgentTreeWalkerParser.MatcherNormalizeBrandContext context)
            {
                Visit(context.matcher());
                FromHereItCannotBeInHashMapAnymore();
                Add(new StepNormalizeBrand());
                return null; // Void
            }

            public override object VisitMatcherConcat([NotNull] UserAgentTreeWalkerParser.MatcherConcatContext context)
            {
                Visit(context.matcher());
                FromHereItCannotBeInHashMapAnymore();
                Add(new StepConcat(context.prefix.Text, context.postfix.Text));
                return null; // Void
            }

            public override object VisitMatcherConcatPrefix([NotNull] UserAgentTreeWalkerParser.MatcherConcatPrefixContext context)
            {
                Visit(context.matcher());
                FromHereItCannotBeInHashMapAnymore();
                Add(new StepConcatPrefix(context.prefix.Text));
                return null; // Void
            }

            public override object VisitMatcherWordRange([NotNull] UserAgentTreeWalkerParser.MatcherWordRangeContext context)
            {
                Visit(context.matcher());
                FromHereItCannotBeInHashMapAnymore();
                Add(new StepWordRange(WordRangeVisitor.getRange(context.wordRange())));
                return null; // Void
            }

            public override object VisitMatcherPathIsNull([NotNull] UserAgentTreeWalkerParser.MatcherPathIsNullContext context)
            {
                // Always add this one, it's special
                walkList.steps.Add(new StepIsNull());
                Visit(context.matcher());
                return null; // Void
            }

            public override object VisitPathVariable([NotNull] UserAgentTreeWalkerParser.PathVariableContext context)
            {
                FromHereItCannotBeInHashMapAnymore();
                VisitNext(context.nextStep);
                return null; // Void
            }

            public override object VisitPathFixedValue([NotNull] UserAgentTreeWalkerParser.PathFixedValueContext context)
            {
                Add(new StepFixedString(context.value.Text));
                return null; // Void
            }

            public override object VisitPathWalk([NotNull] UserAgentTreeWalkerParser.PathWalkContext context)
            {
                VisitNext(context.nextStep);
                return null; // Void
            }

            public override object VisitStepDown([NotNull] UserAgentTreeWalkerParser.StepDownContext context)
            {
                Add(new StepDown(context.numberRange(), context.name.Text));
                VisitNext(context.nextStep);
                return null; // Void
            }

            public override object VisitStepUp([NotNull] UserAgentTreeWalkerParser.StepUpContext context)
            {
                FromHereItCannotBeInHashMapAnymore();
                Add(new StepUp());
                VisitNext(context.nextStep);
                return null; // Void
            }

            public override object VisitStepNext([NotNull] UserAgentTreeWalkerParser.StepNextContext context)
            {
                FromHereItCannotBeInHashMapAnymore();
                Add(new StepNext());
                VisitNext(context.nextStep);
                return null; // Void
            }

            private object DoStepNextN(UserAgentTreeWalkerParser.PathContext nextStep, int nextSteps)
            {
                FromHereItCannotBeInHashMapAnymore();
                Add(new StepNextN(nextSteps));
                VisitNext(nextStep);
                return null; // Void
            }

            public override object VisitStepNext2(UserAgentTreeWalkerParser.StepNext2Context ctx)
            {
                return DoStepNextN(ctx.nextStep, 2);
            }

            public override object VisitStepNext3(UserAgentTreeWalkerParser.StepNext3Context ctx)
            {
                return DoStepNextN(ctx.nextStep, 3);
            }

           
            public override object VisitStepNext4(UserAgentTreeWalkerParser.StepNext4Context ctx)
            {
                return DoStepNextN(ctx.nextStep, 4);
            }

            public override object VisitStepPrev([NotNull] UserAgentTreeWalkerParser.StepPrevContext context)
            {
                FromHereItCannotBeInHashMapAnymore();
                Add(new StepPrev());
                VisitNext(context.nextStep);
                return null; // Void
            }

            private object DoStepPrevN(UserAgentTreeWalkerParser.PathContext nextStep, int prevSteps)
            {
                FromHereItCannotBeInHashMapAnymore();
                Add(new StepPrevN(prevSteps));
                VisitNext(nextStep);
                return null; // Void
            }

            public override object VisitStepPrev2(UserAgentTreeWalkerParser.StepPrev2Context ctx)
            {
                return DoStepPrevN(ctx.nextStep, 2);
            }

            public override object VisitStepPrev3(UserAgentTreeWalkerParser.StepPrev3Context ctx)
            {
                return DoStepPrevN(ctx.nextStep, 3);
            }

            public override object VisitStepPrev4(UserAgentTreeWalkerParser.StepPrev4Context ctx)
            {
                return DoStepPrevN(ctx.nextStep, 4);
            }

            public override object VisitStepEqualsValue([NotNull] UserAgentTreeWalkerParser.StepEqualsValueContext context)
            {
                Add(new StepEquals(context.value.Text));
                FromHereItCannotBeInHashMapAnymore();
                VisitNext(context.nextStep);
                return null; // Void
            }

            public override object VisitStepNotEqualsValue([NotNull] UserAgentTreeWalkerParser.StepNotEqualsValueContext context)
            {
                FromHereItCannotBeInHashMapAnymore();
                Add(new StepNotEquals(context.value.Text));
                VisitNext(context.nextStep);
                return null; // Void
            }

            public override object VisitStepIsInSet([NotNull] UserAgentTreeWalkerParser.StepIsInSetContext context)
            {
                FromHereItCannotBeInHashMapAnymore();

                string lookupSetName = context.set.Text;
                HashSet<string> lookupSet = walkList.lookupSets.ContainsKey(lookupSetName) ? walkList.lookupSets[lookupSetName] : null;
                if (lookupSet == null)
                {
                    Dictionary<string, string> lookup = walkList.lookups.ContainsKey(lookupSetName) ? walkList.lookups[lookupSetName] : null;
                    if (lookup != null)
                    {
                        lookupSet = new HashSet<string>(lookup.Keys);
                    }
                }
                if (lookupSet == null)
                {
                    throw new InvalidParserConfigurationException("Missing lookupSet \"" + lookupSetName + "\" ");
                }

                Add(new StepIsInSet(lookupSetName, lookupSet));
                VisitNext(context.nextStep);
                return null; // Void
            }


            public override object VisitStepStartsWithValue([NotNull] UserAgentTreeWalkerParser.StepStartsWithValueContext context)
            {
                bool skipIfShortEnough = StillGoingToHashMap();
                FromHereItCannotBeInHashMapAnymore();
                string value = context.value.Text;

                bool addTheStep = true;
                if (skipIfShortEnough)
                {
                    // If the compare value is short enough that the ENTIRE value was in the hashmap then
                    // the actual compare is not longer required
                    if (value.Length <= UserAgentAnalyzerDirect.MAX_PREFIX_HASH_MATCH)
                    {
                        addTheStep = false;
                    }
                }

                if (addTheStep)
                {
                    Add(new StepStartsWith(value));
                }
                VisitNext(context.nextStep);
                return null; // Void
            }

            public override object VisitStepEndsWithValue([NotNull] UserAgentTreeWalkerParser.StepEndsWithValueContext context)
            {
                FromHereItCannotBeInHashMapAnymore();
                Add(new StepEndsWith(context.value.Text));
                VisitNext(context.nextStep);
                return null; // Void
            }

            public override object VisitStepContainsValue([NotNull] UserAgentTreeWalkerParser.StepContainsValueContext context)
            {
                FromHereItCannotBeInHashMapAnymore();
                Add(new StepContains(context.value.Text));
                VisitNext(context.nextStep);
                return null; // Void
            }

            public override object VisitStepWordRange([NotNull] UserAgentTreeWalkerParser.StepWordRangeContext context)
            {
                WordRangeVisitor.Range range = WordRangeVisitor.getRange(context.wordRange());
                Add(new StepWordRange(range));
                VisitNext(context.nextStep);
                return null; // Void
            }

            public override object VisitStepBackToFull([NotNull] UserAgentTreeWalkerParser.StepBackToFullContext context)
            {
                Add(new StepBackToFull());
                VisitNext(context.nextStep);
                return null; // Void
            }
        }
    }
}
