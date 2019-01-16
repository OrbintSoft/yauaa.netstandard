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
    using System;
    using System.Collections.Generic;
    using System.Text;
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
        /// <param name="lookups">The lookups</param>
        /// <param name="lookupSets">The lookupSets</param>
        /// <param name="verbose">The verbose<see cref="bool"/></param>
        public WalkList(ParserRuleContext requiredPattern, IDictionary<string, IDictionary<string, string>> lookups, IDictionary<string, ISet<string>> lookupSets, bool verbose)
        {
            this.lookups = lookups;
            this.lookupSets = lookupSets;
            this.verbose = verbose;

            // Generate the walkList from the requiredPattern
            new WalkListBuilder(this).Visit(requiredPattern);
            this.LinkSteps();

            var i = 1;
            if (verbose)
            {
                Log.Info("------------------------------------");
                Log.Info(string.Format("Required: {0}", requiredPattern.GetText()));
                foreach (var step in this.steps)
                {
                    step.SetVerbose(true);
                    Log.Info(string.Format("{0}: {1}", i++, step));
                }
            }
        }

        /// <summary>
        /// Gets the FirstStep
        /// </summary>
        public Step FirstStep => this.steps.Count == 0 ? null : this.steps[0];

        /// <summary>
        /// Gets a value indicating whether UsesIsNull
        /// </summary>
        public bool UsesIsNull
        {
            get
            {
                if (this.usesIsNull != null)
                {
                    return this.usesIsNull.Value;
                }

                var step = this.FirstStep;
                while (step != null)
                {
                    if (step is StepIsNull)
                    {
                        this.usesIsNull = true;
                        return true;
                    }

                    step = step.NextStep;
                }

                this.usesIsNull = false;
                return false;
            }
        }

        /// <summary>
        /// The PruneTrailingStepsThatCannotFail
        /// </summary>
        public void PruneTrailingStepsThatCannotFail()
        {
            var lastStepThatCannotFail = int.MaxValue;
            for (var i = this.steps.Count - 1; i >= 0; i--)
            {
                var current = this.steps[i];
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
                    this.steps.Clear();
                }
                else
                {
                    var lastRelevantStepIndex = lastStepThatCannotFail - 1;
                    var lastRelevantStep = this.steps[lastRelevantStepIndex];
                    lastRelevantStep.SetNextStep(lastRelevantStepIndex, null);

                    this.steps.RemoveRange(lastRelevantStepIndex + 1, this.steps.Count - lastRelevantStepIndex - 1);
                }
            }
        }

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            if (this.steps.Count == 0)
            {
                return "Empty";
            }

            var sb = new StringBuilder(128);
            foreach (var step in this.steps)
            {
                sb.Append(" --> ").Append(step);
            }

            return sb.ToString();
        }

        /// <summary>
        /// The Walk
        /// </summary>
        /// <param name="tree">The tree<see cref="IParseTree"/></param>
        /// <param name="value">The value<see cref="string"/></param>
        /// <returns>The <see cref="WalkResult"/></returns>
        public WalkResult Walk(IParseTree tree, string value)
        {
            if (this.steps.Count == 0)
            {
                return new WalkResult(tree, value);
            }

            var firstStep = this.steps[0];
            if (this.verbose)
            {
                Step.Log.Info(string.Format("Tree: >>>{0}<<<", tree.GetText()));
                Step.Log.Info(string.Format("Enter step: {0}", firstStep));
            }

            var result = firstStep.Walk(tree, value);
            if (this.verbose)
            {
                Step.Log.Info(string.Format("Leave step ({0}): {1}", result == null ? "-" : "+", firstStep));
            }

            return result;
        }

        /// <summary>
        /// The LinkSteps
        /// </summary>
        private void LinkSteps()
        {
            Step nextStep = null;
            for (var i = this.steps.Count - 1; i >= 0; i--)
            {
                var current = this.steps[i];
                current.SetNextStep(i, nextStep);
                nextStep = current;
            }
        }

        /// <summary>
        /// Defines the <see cref="WalkResult" />
        /// </summary>
        [Serializable]
        public sealed class WalkResult
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="WalkResult"/> class.
            /// </summary>
            /// <param name="tree">The tree<see cref="IParseTree"/></param>
            /// <param name="value">The value<see cref="string"/></param>
            public WalkResult(IParseTree tree, string value)
            {
                this.Tree = tree;
                this.Value = value;
            }

            /// <summary>
            /// Gets the Tree
            /// </summary>
            public IParseTree Tree { get; }

            /// <summary>
            /// Gets the Value
            /// </summary>
            public string Value { get; }

            /// <summary>
            /// The ToString
            /// </summary>
            /// <returns>The <see cref="string"/></returns>
            public override string ToString()
            {
                return "WalkResult{" +
                    "tree=" + (this.Tree == null ? ">>>NULL<<<" : this.Tree.GetText()) +
                    ", value=" + (this.Value == null ? ">>>NULL<<<" : '\'' + this.Value + '\'') +
                    '}';
            }
        }

        /// <summary>
        /// Defines the <see cref="WalkListBuilder" />
        /// </summary>
        private class WalkListBuilder : UserAgentTreeWalkerBaseVisitor<object>
        {
            /// <summary>
            /// Defines the walkList
            /// </summary>
            private readonly WalkList walkList = null;

            /// <summary>
            /// Defines the foundHashEntryPoint
            /// </summary>
            private bool foundHashEntryPoint = false;

            /// <summary>
            /// Initializes a new instance of the <see cref="WalkListBuilder"/> class.
            /// </summary>
            /// <param name="walkList">The walkList<see cref="WalkList"/></param>
            public WalkListBuilder(WalkList walkList)
                : base()
            {
                this.walkList = walkList;
            }

            /// <summary>
            /// The VisitMatcherCleanVersion
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherCleanVersionContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitMatcherCleanVersion([NotNull] UserAgentTreeWalkerParser.MatcherCleanVersionContext context)
            {
                this.Visit(context.matcher());
                this.FromHereItCannotBeInHashMapAnymore();
                this.Add(new StepCleanVersion());
                return null; // Void
            }

            /// <summary>
            /// The VisitMatcherConcat
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherConcatContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitMatcherConcat([NotNull] UserAgentTreeWalkerParser.MatcherConcatContext context)
            {
                this.Visit(context.matcher());
                this.FromHereItCannotBeInHashMapAnymore();
                this.Add(new StepConcat(context.prefix.Text, context.postfix.Text));
                return null; // Void
            }

            /// <summary>
            /// The VisitMatcherConcatPostfix
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherConcatPostfixContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitMatcherConcatPostfix([NotNull] UserAgentTreeWalkerParser.MatcherConcatPostfixContext context)
            {
                this.Visit(context.matcher());
                this.FromHereItCannotBeInHashMapAnymore();
                this.Add(new StepConcatPostfix(context.postfix.Text));
                return null; // Void
            }

            /// <summary>
            /// The VisitMatcherConcatPrefix
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherConcatPrefixContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitMatcherConcatPrefix([NotNull] UserAgentTreeWalkerParser.MatcherConcatPrefixContext context)
            {
                this.Visit(context.matcher());
                this.FromHereItCannotBeInHashMapAnymore();
                this.Add(new StepConcatPrefix(context.prefix.Text));
                return null; // Void
            }

            /// <summary>
            /// The VisitMatcherNormalizeBrand
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherNormalizeBrandContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitMatcherNormalizeBrand([NotNull] UserAgentTreeWalkerParser.MatcherNormalizeBrandContext context)
            {
                this.Visit(context.matcher());
                this.FromHereItCannotBeInHashMapAnymore();
                this.Add(new StepNormalizeBrand());
                return null; // Void
            }

            /// <summary>
            /// The VisitMatcherPath
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherPathContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitMatcherPath([NotNull] UserAgentTreeWalkerParser.MatcherPathContext context)
            {
                this.Visit(context.basePath());
                return null; // Void
            }

            /// <summary>
            /// The VisitMatcherPathIsInLookupPrefix
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherPathIsInLookupPrefixContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitMatcherPathIsInLookupPrefix([NotNull] UserAgentTreeWalkerParser.MatcherPathIsInLookupPrefixContext context)
            {
                this.Visit(context.matcher());

                this.FromHereItCannotBeInHashMapAnymore();

                var lookupName = context.lookup.Text;
                var lookup = this.GetLookup(lookupName);

                this.Add(new StepIsInLookupPrefix(lookupName, lookup));
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
                this.walkList.steps.Add(new StepIsNull());
                this.Visit(context.matcher());
                return null; // Void
            }

            /// <summary>
            /// The VisitMatcherPathLookup
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherPathLookupContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitMatcherPathLookup([NotNull] UserAgentTreeWalkerParser.MatcherPathLookupContext context)
            {
                this.Visit(context.matcher());

                this.FromHereItCannotBeInHashMapAnymore();

                var lookupName = context.lookup.Text;
                var lookup = this.GetLookup(lookupName);

                string defaultValue = null;
                if (context.defaultValue != null)
                {
                    defaultValue = context.defaultValue.Text;
                }

                this.Add(new StepLookup(lookupName, lookup, defaultValue));
                return null; // Void
            }

            /// <summary>
            /// The VisitMatcherPathLookupPrefix
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherPathLookupPrefixContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitMatcherPathLookupPrefix([NotNull] UserAgentTreeWalkerParser.MatcherPathLookupPrefixContext context)
            {
                this.Visit(context.matcher());

                this.FromHereItCannotBeInHashMapAnymore();

                var lookupName = context.lookup.Text;
                var lookup = this.GetLookup(lookupName);

                string defaultValue = null;
                if (context.defaultValue != null)
                {
                    defaultValue = context.defaultValue.Text;
                }

                this.Add(new StepLookupPrefix(lookupName, lookup, defaultValue));
                return null; // Void
            }

            /// <summary>
            /// The VisitMatcherWordRange
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherWordRangeContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitMatcherWordRange([NotNull] UserAgentTreeWalkerParser.MatcherWordRangeContext context)
            {
                this.Visit(context.matcher());
                this.FromHereItCannotBeInHashMapAnymore();
                this.Add(new StepWordRange(WordRangeVisitor.GetRange(context.wordRange())));
                return null; // Void
            }

            /// <summary>
            /// The VisitPathVariable
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.PathVariableContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitPathVariable([NotNull] UserAgentTreeWalkerParser.PathVariableContext context)
            {
                this.FromHereItCannotBeInHashMapAnymore();
                this.VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// The VisitPathWalk
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.PathWalkContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitPathWalk([NotNull] UserAgentTreeWalkerParser.PathWalkContext context)
            {
                this.VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// The VisitStepBackToFull
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepBackToFullContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepBackToFull([NotNull] UserAgentTreeWalkerParser.StepBackToFullContext context)
            {
                this.Add(new StepBackToFull());
                this.VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// The VisitStepContainsValue
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepContainsValueContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepContainsValue([NotNull] UserAgentTreeWalkerParser.StepContainsValueContext context)
            {
                this.FromHereItCannotBeInHashMapAnymore();
                this.Add(new StepContains(context.value.Text));
                this.VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// The VisitStepDown
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepDownContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepDown([NotNull] UserAgentTreeWalkerParser.StepDownContext context)
            {
                this.Add(new StepDown(context.numberRange(), context.name.Text));
                this.VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// The VisitStepEndsWithValue
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepEndsWithValueContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepEndsWithValue([NotNull] UserAgentTreeWalkerParser.StepEndsWithValueContext context)
            {
                this.FromHereItCannotBeInHashMapAnymore();
                this.Add(new StepEndsWith(context.value.Text));
                this.VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// The VisitStepEqualsValue
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepEqualsValueContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepEqualsValue([NotNull] UserAgentTreeWalkerParser.StepEqualsValueContext context)
            {
                this.Add(new StepEquals(context.value.Text));
                this.FromHereItCannotBeInHashMapAnymore();
                this.VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// The VisitStepIsInSet
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepIsInSetContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepIsInSet([NotNull] UserAgentTreeWalkerParser.StepIsInSetContext context)
            {
                this.FromHereItCannotBeInHashMapAnymore();

                var lookupSetName = context.set.Text;
                var lookupSet = this.walkList.lookupSets.ContainsKey(lookupSetName) ? this.walkList.lookupSets[lookupSetName] : null;
                if (lookupSet == null)
                {
                    if (this.walkList.lookups.ContainsKey(lookupSetName))
                    {
                        lookupSet = new HashSet<string>(this.walkList.lookups[lookupSetName].Keys);
                    }
                }

                if (lookupSet == null)
                {
                    throw new InvalidParserConfigurationException("Missing lookupSet \"" + lookupSetName + "\" ");
                }

                this.Add(new StepIsInSet(lookupSetName, lookupSet));
                this.VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// The VisitStepNext
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepNextContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepNext([NotNull] UserAgentTreeWalkerParser.StepNextContext context)
            {
                this.FromHereItCannotBeInHashMapAnymore();
                this.Add(new StepNext());
                this.VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// The VisitStepNext2
            /// </summary>
            /// <param name="ctx">The ctx<see cref="UserAgentTreeWalkerParser.StepNext2Context"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepNext2(UserAgentTreeWalkerParser.StepNext2Context ctx)
            {
                return this.DoStepNextN(ctx.nextStep, 2);
            }

            /// <summary>
            /// The VisitStepNext3
            /// </summary>
            /// <param name="ctx">The ctx<see cref="UserAgentTreeWalkerParser.StepNext3Context"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepNext3(UserAgentTreeWalkerParser.StepNext3Context ctx)
            {
                return this.DoStepNextN(ctx.nextStep, 3);
            }

            /// <summary>
            /// The VisitStepNext4
            /// </summary>
            /// <param name="ctx">The ctx<see cref="UserAgentTreeWalkerParser.StepNext4Context"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepNext4(UserAgentTreeWalkerParser.StepNext4Context ctx)
            {
                return this.DoStepNextN(ctx.nextStep, 4);
            }

            /// <summary>
            /// The VisitStepNotEqualsValue
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepNotEqualsValueContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepNotEqualsValue([NotNull] UserAgentTreeWalkerParser.StepNotEqualsValueContext context)
            {
                this.FromHereItCannotBeInHashMapAnymore();
                this.Add(new StepNotEquals(context.value.Text));
                this.VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// The VisitStepPrev
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepPrevContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepPrev([NotNull] UserAgentTreeWalkerParser.StepPrevContext context)
            {
                this.FromHereItCannotBeInHashMapAnymore();
                this.Add(new StepPrev());
                this.VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// The VisitStepPrev2
            /// </summary>
            /// <param name="ctx">The ctx<see cref="UserAgentTreeWalkerParser.StepPrev2Context"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepPrev2(UserAgentTreeWalkerParser.StepPrev2Context ctx)
            {
                return this.DoStepPrevN(ctx.nextStep, 2);
            }

            /// <summary>
            /// The VisitStepPrev3
            /// </summary>
            /// <param name="ctx">The ctx<see cref="UserAgentTreeWalkerParser.StepPrev3Context"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepPrev3(UserAgentTreeWalkerParser.StepPrev3Context ctx)
            {
                return this.DoStepPrevN(ctx.nextStep, 3);
            }

            /// <summary>
            /// The VisitStepPrev4
            /// </summary>
            /// <param name="ctx">The ctx<see cref="UserAgentTreeWalkerParser.StepPrev4Context"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepPrev4(UserAgentTreeWalkerParser.StepPrev4Context ctx)
            {
                return this.DoStepPrevN(ctx.nextStep, 4);
            }

            /// <summary>
            /// The VisitStepStartsWithValue
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepStartsWithValueContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepStartsWithValue([NotNull] UserAgentTreeWalkerParser.StepStartsWithValueContext context)
            {
                var skipIfShortEnough = this.StillGoingToHashMap();
                this.FromHereItCannotBeInHashMapAnymore();
                var value = context.value.Text;

                var addTheStep = true;
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
                    this.Add(new StepStartsWith(value));
                }

                this.VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// The VisitStepUp
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepUpContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepUp([NotNull] UserAgentTreeWalkerParser.StepUpContext context)
            {
                this.FromHereItCannotBeInHashMapAnymore();
                this.Add(new StepUp());
                this.VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// The VisitStepWordRange
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepWordRangeContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepWordRange([NotNull] UserAgentTreeWalkerParser.StepWordRangeContext context)
            {
                var range = WordRangeVisitor.GetRange(context.wordRange());
                this.Add(new StepWordRange(range));
                this.VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// The Add
            /// </summary>
            /// <param name="step">The step<see cref="Step"/></param>
            private void Add(Step step)
            {
                if (this.foundHashEntryPoint)
                {
                    this.walkList.steps.Add(step);
                }
            }

            /// <summary>
            /// The DoStepNextN
            /// </summary>
            /// <param name="nextStep">The nextStep<see cref="UserAgentTreeWalkerParser.PathContext"/></param>
            /// <param name="nextSteps">The nextSteps<see cref="int"/></param>
            /// <returns>The <see cref="object"/></returns>
            private object DoStepNextN(UserAgentTreeWalkerParser.PathContext nextStep, int nextSteps)
            {
                this.FromHereItCannotBeInHashMapAnymore();
                this.Add(new StepNextN(nextSteps));
                this.VisitNext(nextStep);
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
                this.FromHereItCannotBeInHashMapAnymore();
                this.Add(new StepPrevN(prevSteps));
                this.VisitNext(nextStep);
                return null; // Void
            }

            /// <summary>
            /// The FromHereItCannotBeInHashMapAnymore
            /// </summary>
            private void FromHereItCannotBeInHashMapAnymore()
            {
                this.foundHashEntryPoint = true;
            }

            /// <summary>
            /// The GetLookup
            /// </summary>
            /// <param name="lookupName">The lookupName<see cref="string"/></param>
            /// <returns>The Lookup</returns>
            private IDictionary<string, string> GetLookup(string lookupName)
            {
                if (!this.walkList.lookups.ContainsKey(lookupName))
                {
                    throw new InvalidParserConfigurationException("Missing lookup \"" + lookupName + "\" ");
                }

                return this.walkList.lookups[lookupName];
            }

            /// <summary>
            /// The StillGoingToHashMap
            /// </summary>
            /// <returns>The <see cref="bool"/></returns>
            private bool StillGoingToHashMap()
            {
                return !this.foundHashEntryPoint;
            }

            /// <summary>
            /// The VisitNext
            /// </summary>
            /// <param name="nextStep">The nextStep<see cref="UserAgentTreeWalkerParser.PathContext"/></param>
            private void VisitNext(UserAgentTreeWalkerParser.PathContext nextStep)
            {
                if (nextStep != null)
                {
                    this.Visit(nextStep);
                }
            }
        }
    }
}
