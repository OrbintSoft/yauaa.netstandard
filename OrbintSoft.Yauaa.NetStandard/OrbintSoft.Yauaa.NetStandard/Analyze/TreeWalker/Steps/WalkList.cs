//-----------------------------------------------------------------------
// <copyright file="WalkList.cs" company="OrbintSoft">
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
// <date>2018, 11, 24, 12:48</date>
// <summary></summary>
//-----------------------------------------------------------------------
namespace OrbintSoft.Yauaa.Analyze.TreeWalker.Steps
{
    using Antlr4.Runtime;
    using Antlr4.Runtime.Misc;
    using Antlr4.Runtime.Tree;
    using log4net;
    using OrbintSoft.Yauaa.Analyze.TreeWalker.Steps.Compare;
    using OrbintSoft.Yauaa.Analyze.TreeWalker.Steps.Lookup;
    using OrbintSoft.Yauaa.Analyze.TreeWalker.Steps.Value;
    using OrbintSoft.Yauaa.Analyze.TreeWalker.Steps.Walk;
    using OrbintSoft.Yauaa.Analyzer;
    using OrbintSoft.Yauaa.Antlr4Source;
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// This class gets the symbol table (1 value) uses that to evaluate
    /// the expression against the parsed user agent
    /// </summary>
    [Serializable]
    public class WalkList
    {
        /// <summary>
        /// Defines the Log
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(WalkList));

        /// <summary>
        /// Defines the lookups
        /// </summary>
        private readonly IDictionary<string, IDictionary<string, string>> lookups;

        /// <summary>
        /// Defines the lookupSets
        /// </summary>
        private readonly IDictionary<string, ISet<string>> lookupSets;

        /// <summary>
        /// Defines the steps
        /// </summary>
        private readonly List<Step> steps = new List<Step>();

        /// <summary>
        /// Defines the verbose
        /// </summary>
        private readonly bool verbose;

        /// <summary>
        /// Defines the usesIsNull
        /// </summary>
        private bool? usesIsNull = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="WalkList"/> class.
        /// </summary>
        /// <param name="requiredPattern">The requiredPattern<see cref="ParserRuleContext"/></param>
        /// <param name="lookups">The lookups<see cref="IDictionary{string, IDictionary{string, string}}"/></param>
        /// <param name="lookupSets">The lookupSets<see cref="IDictionary{string, ISet{string}}"/></param>
        /// <param name="verbose">The verbose<see cref="bool"/></param>
        public WalkList(ParserRuleContext requiredPattern, IDictionary<string, IDictionary<string, string>> lookups, IDictionary<string, ISet<string>> lookupSets, bool verbose)
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
                Log.Info("------------------------------------");
                Log.Info(string.Format("Required: {0}",  requiredPattern.GetText()));
                foreach (Step step in steps)
                {
                    step.SetVerbose(true);
                    Log.Info(string.Format("{0}: {1}", i++, step));
                }
            }
        }



        /// <summary>
        /// Gets a value indicating whether UsesIsNull
        /// </summary>
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

        /// <summary>
        /// The Walk
        /// </summary>
        /// <param name="tree">The tree<see cref="IParseTree"/></param>
        /// <param name="value">The value<see cref="string"/></param>
        /// <returns>The <see cref="WalkResult"/></returns>
        public WalkResult Walk(IParseTree tree, string value)
        {
            if (steps.Count == 0)
            {
                return new WalkResult(tree, value);
            }
            Step firstStep = steps[0];
            if (verbose)
            {
                Step.Log.Info(string.Format("Tree: >>>{0}<<<", tree.GetText()));
                Step.Log.Info(string.Format("Enter step: {0}", firstStep));
            }
            WalkResult result = firstStep.Walk(tree, value);
            if (verbose)
            {
                Step.Log.Info(string.Format("Leave step ({0}): {1}", result == null ? "-" : "+", firstStep));
            }
            return result;
        }

        public void PruneTrailingStepsThatCannotFail()
        {
            int lastStepThatCannotFail = int.MaxValue;
            for (int i = steps.Count - 1; i >= 0; i--)
            {
                Step current = steps[i];
                if (current.CanFail())
                {
                    break; // We're done. We have the last step that CAN fail.
                }
                lastStepThatCannotFail = i;
            }
            if (lastStepThatCannotFail != int.MaxValue)
            {
                if (lastStepThatCannotFail == 0)
                {
                    steps.Clear();
                }
                else
                {
                    int lastRelevantStepIndex = lastStepThatCannotFail - 1;
                    Step lastRelevantStep = steps[lastRelevantStepIndex];
                    lastRelevantStep.SetNextStep(lastRelevantStepIndex, null);

                    steps.RemoveRange(lastRelevantStepIndex + 1, steps.Count - lastRelevantStepIndex - 1);
                }
            }
        }

        /// <summary>
        /// The GetFirstStep
        /// </summary>
        /// <returns>The <see cref="Step"/></returns>
        public Step GetFirstStep()
        {
            return steps.Count == 0 ? null : steps[0];
        }

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
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

        /// <summary>
        /// The LinkSteps
        /// </summary>
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

        /// <summary>
        /// Defines the <see cref="WalkListBuilder" />
        /// </summary>
        private class WalkListBuilder : UserAgentTreeWalkerBaseVisitor<object>
        {
            // Because we are jumping in 'mid way' we need to skip creating steps until that point.
            /// <summary>
            /// Defines the foundHashEntryPoint
            /// </summary>
            internal bool foundHashEntryPoint = false;

            /// <summary>
            /// Defines the walkList
            /// </summary>
            private readonly WalkList walkList = null;

            /// <summary>
            /// Initializes a new instance of the <see cref="WalkListBuilder"/> class.
            /// </summary>
            /// <param name="walkList">The walkList<see cref="WalkList"/></param>
            public WalkListBuilder(WalkList walkList) : base()
            {
                this.walkList = walkList;
            }

            /// <summary>
            /// The FromHereItCannotBeInHashMapAnymore
            /// </summary>
            private void FromHereItCannotBeInHashMapAnymore()
            {
                foundHashEntryPoint = true;
            }

            /// <summary>
            /// The StillGoingToHashMap
            /// </summary>
            /// <returns>The <see cref="bool"/></returns>
            private bool StillGoingToHashMap()
            {
                return !foundHashEntryPoint;
            }

            /// <summary>
            /// The Add
            /// </summary>
            /// <param name="step">The step<see cref="Step"/></param>
            private void Add(Step step)
            {
                if (foundHashEntryPoint)
                {
                    walkList.steps.Add(step);
                }
            }

            /// <summary>
            /// The VisitNext
            /// </summary>
            /// <param name="nextStep">The nextStep<see cref="UserAgentTreeWalkerParser.PathContext"/></param>
            private void VisitNext(UserAgentTreeWalkerParser.PathContext nextStep)
            {
                if (nextStep != null)
                {
                    Visit(nextStep);
                }
            }

            /// <summary>
            /// The VisitMatcherPath
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherPathContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitMatcherPath([NotNull] UserAgentTreeWalkerParser.MatcherPathContext context)
            {
                Visit(context.basePath());
                return null; // Void
            }

            /// <summary>
            /// The VisitMatcherPathLookup
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherPathLookupContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitMatcherPathLookup([NotNull] UserAgentTreeWalkerParser.MatcherPathLookupContext context)
            {
                Visit(context.matcher());

                FromHereItCannotBeInHashMapAnymore();

                string lookupName = context.lookup.Text;
                IDictionary<string, string> lookup = walkList.lookups.ContainsKey(lookupName) ? walkList.lookups[lookupName] : null;
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

            /// <summary>
            /// The VisitMatcherCleanVersion
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherCleanVersionContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitMatcherCleanVersion([NotNull] UserAgentTreeWalkerParser.MatcherCleanVersionContext context)
            {
                Visit(context.matcher());
                FromHereItCannotBeInHashMapAnymore();
                Add(new StepCleanVersion());
                return null; // Void
            }

            /// <summary>
            /// The VisitMatcherNormalizeBrand
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherNormalizeBrandContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitMatcherNormalizeBrand([NotNull] UserAgentTreeWalkerParser.MatcherNormalizeBrandContext context)
            {
                Visit(context.matcher());
                FromHereItCannotBeInHashMapAnymore();
                Add(new StepNormalizeBrand());
                return null; // Void
            }

            /// <summary>
            /// The VisitMatcherConcat
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherConcatContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitMatcherConcat([NotNull] UserAgentTreeWalkerParser.MatcherConcatContext context)
            {
                Visit(context.matcher());
                FromHereItCannotBeInHashMapAnymore();
                Add(new StepConcat(context.prefix.Text, context.postfix.Text));
                return null; // Void
            }

            /// <summary>
            /// The VisitMatcherConcatPrefix
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherConcatPrefixContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitMatcherConcatPrefix([NotNull] UserAgentTreeWalkerParser.MatcherConcatPrefixContext context)
            {
                Visit(context.matcher());
                FromHereItCannotBeInHashMapAnymore();
                Add(new StepConcatPrefix(context.prefix.Text));
                return null; // Void
            }

            /// <summary>
            /// The VisitMatcherConcatPostfix
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherConcatPostfixContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitMatcherConcatPostfix([NotNull] UserAgentTreeWalkerParser.MatcherConcatPostfixContext context)
            {
                Visit(context.matcher());
                FromHereItCannotBeInHashMapAnymore();
                Add(new StepConcatPostfix(context.postfix.Text));
                return null; // Void
            }

            /// <summary>
            /// The VisitMatcherWordRange
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherWordRangeContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitMatcherWordRange([NotNull] UserAgentTreeWalkerParser.MatcherWordRangeContext context)
            {
                Visit(context.matcher());
                FromHereItCannotBeInHashMapAnymore();
                Add(new StepWordRange(WordRangeVisitor.GetRange(context.wordRange())));
                return null; // Void
            }

            /// <summary>
            /// The VisitMatcherPathIsNull
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherPathIsNullContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitMatcherPathIsNull([NotNull] UserAgentTreeWalkerParser.MatcherPathIsNullContext context)
            {
                // Always add this one, it's special
                walkList.steps.Add(new StepIsNull());
                Visit(context.matcher());
                return null; // Void
            }

            /// <summary>
            /// The VisitPathVariable
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.PathVariableContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitPathVariable([NotNull] UserAgentTreeWalkerParser.PathVariableContext context)
            {
                FromHereItCannotBeInHashMapAnymore();
                VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// The VisitPathWalk
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.PathWalkContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitPathWalk([NotNull] UserAgentTreeWalkerParser.PathWalkContext context)
            {
                VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// The VisitStepDown
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepDownContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepDown([NotNull] UserAgentTreeWalkerParser.StepDownContext context)
            {
                Add(new StepDown(context.numberRange(), context.name.Text));
                VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// The VisitStepUp
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepUpContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepUp([NotNull] UserAgentTreeWalkerParser.StepUpContext context)
            {
                FromHereItCannotBeInHashMapAnymore();
                Add(new StepUp());
                VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// The VisitStepNext
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepNextContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepNext([NotNull] UserAgentTreeWalkerParser.StepNextContext context)
            {
                FromHereItCannotBeInHashMapAnymore();
                Add(new StepNext());
                VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// The DoStepNextN
            /// </summary>
            /// <param name="nextStep">The nextStep<see cref="UserAgentTreeWalkerParser.PathContext"/></param>
            /// <param name="nextSteps">The nextSteps<see cref="int"/></param>
            /// <returns>The <see cref="object"/></returns>
            private object DoStepNextN(UserAgentTreeWalkerParser.PathContext nextStep, int nextSteps)
            {
                FromHereItCannotBeInHashMapAnymore();
                Add(new StepNextN(nextSteps));
                VisitNext(nextStep);
                return null; // Void
            }

            /// <summary>
            /// The VisitStepNext2
            /// </summary>
            /// <param name="ctx">The ctx<see cref="UserAgentTreeWalkerParser.StepNext2Context"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepNext2(UserAgentTreeWalkerParser.StepNext2Context ctx)
            {
                return DoStepNextN(ctx.nextStep, 2);
            }

            /// <summary>
            /// The VisitStepNext3
            /// </summary>
            /// <param name="ctx">The ctx<see cref="UserAgentTreeWalkerParser.StepNext3Context"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepNext3(UserAgentTreeWalkerParser.StepNext3Context ctx)
            {
                return DoStepNextN(ctx.nextStep, 3);
            }

            /// <summary>
            /// The VisitStepNext4
            /// </summary>
            /// <param name="ctx">The ctx<see cref="UserAgentTreeWalkerParser.StepNext4Context"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepNext4(UserAgentTreeWalkerParser.StepNext4Context ctx)
            {
                return DoStepNextN(ctx.nextStep, 4);
            }

            /// <summary>
            /// The VisitStepPrev
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepPrevContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepPrev([NotNull] UserAgentTreeWalkerParser.StepPrevContext context)
            {
                FromHereItCannotBeInHashMapAnymore();
                Add(new StepPrev());
                VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// The DoStepPrevN
            /// </summary>
            /// <param name="nextStep">The nextStep<see cref="UserAgentTreeWalkerParser.PathContext"/></param>
            /// <param name="prevSteps">The prevSteps<see cref="int"/></param>
            /// <returns>The <see cref="object"/></returns>
            private object DoStepPrevN(UserAgentTreeWalkerParser.PathContext nextStep, int prevSteps)
            {
                FromHereItCannotBeInHashMapAnymore();
                Add(new StepPrevN(prevSteps));
                VisitNext(nextStep);
                return null; // Void
            }

            /// <summary>
            /// The VisitStepPrev2
            /// </summary>
            /// <param name="ctx">The ctx<see cref="UserAgentTreeWalkerParser.StepPrev2Context"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepPrev2(UserAgentTreeWalkerParser.StepPrev2Context ctx)
            {
                return DoStepPrevN(ctx.nextStep, 2);
            }

            /// <summary>
            /// The VisitStepPrev3
            /// </summary>
            /// <param name="ctx">The ctx<see cref="UserAgentTreeWalkerParser.StepPrev3Context"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepPrev3(UserAgentTreeWalkerParser.StepPrev3Context ctx)
            {
                return DoStepPrevN(ctx.nextStep, 3);
            }

            /// <summary>
            /// The VisitStepPrev4
            /// </summary>
            /// <param name="ctx">The ctx<see cref="UserAgentTreeWalkerParser.StepPrev4Context"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepPrev4(UserAgentTreeWalkerParser.StepPrev4Context ctx)
            {
                return DoStepPrevN(ctx.nextStep, 4);
            }

            /// <summary>
            /// The VisitStepEqualsValue
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepEqualsValueContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepEqualsValue([NotNull] UserAgentTreeWalkerParser.StepEqualsValueContext context)
            {
                Add(new StepEquals(context.value.Text));
                FromHereItCannotBeInHashMapAnymore();
                VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// The VisitStepNotEqualsValue
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepNotEqualsValueContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepNotEqualsValue([NotNull] UserAgentTreeWalkerParser.StepNotEqualsValueContext context)
            {
                FromHereItCannotBeInHashMapAnymore();
                Add(new StepNotEquals(context.value.Text));
                VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// The VisitStepIsInSet
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepIsInSetContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepIsInSet([NotNull] UserAgentTreeWalkerParser.StepIsInSetContext context)
            {
                FromHereItCannotBeInHashMapAnymore();

                string lookupSetName = context.set.Text;
                ISet<string> lookupSet = walkList.lookupSets.ContainsKey(lookupSetName) ? walkList.lookupSets[lookupSetName] : null;
                if (lookupSet == null)
                {
                    IDictionary<string, string> lookup = walkList.lookups.ContainsKey(lookupSetName) ? walkList.lookups[lookupSetName] : null;
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

            /// <summary>
            /// The VisitStepStartsWithValue
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepStartsWithValueContext"/></param>
            /// <returns>The <see cref="object"/></returns>
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

            /// <summary>
            /// The VisitStepEndsWithValue
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepEndsWithValueContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepEndsWithValue([NotNull] UserAgentTreeWalkerParser.StepEndsWithValueContext context)
            {
                FromHereItCannotBeInHashMapAnymore();
                Add(new StepEndsWith(context.value.Text));
                VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// The VisitStepContainsValue
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepContainsValueContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepContainsValue([NotNull] UserAgentTreeWalkerParser.StepContainsValueContext context)
            {
                FromHereItCannotBeInHashMapAnymore();
                Add(new StepContains(context.value.Text));
                VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// The VisitStepWordRange
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepWordRangeContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepWordRange([NotNull] UserAgentTreeWalkerParser.StepWordRangeContext context)
            {
                WordRangeVisitor.Range range = WordRangeVisitor.GetRange(context.wordRange());
                Add(new StepWordRange(range));
                VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// The VisitStepBackToFull
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepBackToFullContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepBackToFull([NotNull] UserAgentTreeWalkerParser.StepBackToFullContext context)
            {
                Add(new StepBackToFull());
                VisitNext(context.nextStep);
                return null; // Void
            }
        }

        /// <summary>
        /// Defines the <see cref="WalkResult" />
        /// </summary>
        [Serializable]
        public sealed class WalkResult
        {
            /// <summary>
            /// Defines the tree
            /// </summary>
            private readonly IParseTree tree;

            /// <summary>
            /// Defines the value
            /// </summary>
            private readonly string value;

            /// <summary>
            /// Initializes a new instance of the <see cref="WalkResult"/> class.
            /// </summary>
            /// <param name="tree">The tree<see cref="IParseTree"/></param>
            /// <param name="value">The value<see cref="string"/></param>
            public WalkResult(IParseTree tree, string value)
            {
                this.tree = tree;
                this.value = value;
            }

            /// <summary>
            /// The GetTree
            /// </summary>
            /// <returns>The <see cref="IParseTree"/></returns>
            public IParseTree GetTree()
            {
                return tree;
            }

            /// <summary>
            /// The GetValue
            /// </summary>
            /// <returns>The <see cref="string"/></returns>
            public string GetValue()
            {
                return value;
            }

            /// <summary>
            /// The ToString
            /// </summary>
            /// <returns>The <see cref="string"/></returns>
            public override string ToString()
            {
                return "WalkResult{" +
                    "tree=" + (tree == null ? ">>>NULL<<<" : tree.GetText()) +
                    ", value=" + (value == null ? ">>>NULL<<<" : '\'' + value + '\'') +
                    '}';
            }
        }
    }
}
