//-----------------------------------------------------------------------
// <copyright file="WalkList.cs" company="OrbintSoft">
//   Yet Another User Agent Analyzer for .NET Standard
//   porting realized by Stefano Balzarotti, Copyright 2018-2020 (C) OrbintSoft
//
//   Original Author and License:
//
//   Yet Another UserAgent Analyzer
//   Copyright(C) 2013-2020 Niels Basjes
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//   https://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
// <author>Stefano Balzarotti, Niels Basjes</author>
// <date>2018, 11, 24, 12:48</date>
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
    /// the expression against the parsed user agent.
    /// </summary>
    [Serializable]
    public class WalkList
    {
        /// <summary>
        /// Defines the Logger.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(WalkList));

        /// <summary>
        /// Defines the lookups dictionary.
        /// </summary>
        private readonly IDictionary<string, IDictionary<string, string>> lookups;

        /// <summary>
        /// Defines the lookup sets dictionary.
        /// </summary>
        private readonly IDictionary<string, ISet<string>> lookupSets;

        /// <summary>
        /// Defines the list of steps.
        /// </summary>
        private readonly List<Step> steps = new List<Step>();

        /// <summary>
        /// Defines if verbose logging is enabled.
        /// </summary>
        private readonly bool verbose;

        /// <summary>
        /// Defines if the walking list uses null step.
        /// Null if not defined yet.
        /// </summary>
        private bool? usesIsNull = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="WalkList"/> class.
        /// </summary>
        /// <param name="requiredPattern">The require pattern.</param>
        /// <param name="lookups">The lookups.</param>
        /// <param name="lookupSets">The lookupSets.</param>
        /// <param name="verbose">True to enable verbose logging, or false.</param>
        public WalkList(ParserRuleContext requiredPattern, IDictionary<string, IDictionary<string, string>> lookups, IDictionary<string, ISet<string>> lookupSets, bool verbose)
        {
            this.lookups = lookups;
            this.lookupSets = lookupSets;
            this.verbose = verbose;

            // Generate the walkList from the requiredPattern
            new WalkListBuilder(this).Visit(requiredPattern);
            this.LinkSteps();

            if (verbose)
            {
                var i = 1;
                Log.Info("------------------------------------");
                Log.Info($"Required: {requiredPattern.GetText()}");
                foreach (var step in this.steps)
                {
                    step.SetVerbose(true);
                    Log.Info($"{i++}: {step}");
                }
            }
        }

        /// <summary>
        /// Gets the First step in the list of steps.
        /// </summary>
        public Step FirstStep => this.steps.Count == 0 ? null : this.steps[0];

        /// <summary>
        /// Gets a value indicating whether there is a <see cref="StepIsNull"/>.
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
        /// Finds the lastr step that cannot fail, and removes the following steps.
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

        /// <inheritdoc/>
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
        /// Takes the first step and walks into it, or returns the <see cref="WalkResult"/>.
        /// </summary>
        /// <param name="tree">The node tree.</param>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="WalkResult"/>.</returns>
        public WalkResult Walk(IParseTree tree, string value)
        {
            if (this.steps.Count == 0)
            {
                return new WalkResult(tree, value);
            }

            var firstStep = this.steps[0];
            if (this.verbose)
            {
                Step.Log.Info($"Tree: >>>{tree.GetText()}<<<");
                Step.Log.Info($"Enter step: {firstStep}");
            }

            var result = firstStep.Walk(tree, value);
            if (this.verbose)
            {
                Step.Log.Info(string.Format($"Leave step ({(result is null ? " - " : " + ")}): {firstStep}"));
            }

            return result;
        }

        /// <summary>
        /// Links the steps in this.steps together as next steps.
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
        /// This class represent a result in walking a node.
        /// </summary>
        [Serializable]
        public sealed class WalkResult
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="WalkResult"/> class.
            /// </summary>
            /// <param name="tree">The tree<see cref="IParseTree"/>.</param>
            /// <param name="value">The value<see cref="string"/>.</param>
            public WalkResult(IParseTree tree, string value)
            {
                this.Tree = tree;
                this.Value = value;
            }

            /// <summary>
            /// Gets the tree.
            /// </summary>
            public IParseTree Tree { get; }

            /// <summary>
            /// Gets the Value.
            /// </summary>
            public string Value { get; }

            /// <inheritdoc/>
            public override string ToString()
            {
                return "WalkResult{" +
                    "tree=" + (this.Tree is null ? ">>>NULL<<<" : this.Tree.GetText()) +
                    ", value=" + (this.Value is null ? ">>>NULL<<<" : '\'' + this.Value + '\'') +
                    '}';
            }
        }

        /// <summary>
        /// This class is a utility to build the <see cref="WalkList"/> based on the <see cref="UserAgentTreeWalkerBaseVisitor&lt;object&gt;"/>.
        /// </summary>
        private class WalkListBuilder : UserAgentTreeWalkerBaseVisitor<object>
        {
            /// <summary>
            /// The walk list.
            /// </summary>
            private readonly WalkList walkList = null;

            /// <summary>
            /// True if found an hash entry point.
            /// </summary>
            private bool foundHashEntryPoint = false;

            /// <summary>
            /// Initializes a new instance of the <see cref="WalkListBuilder"/> class.
            /// </summary>
            /// <param name="walkList">The <see cref="WalkList"/>.</param>
            public WalkListBuilder(WalkList walkList)
                : base()
            {
                this.walkList = walkList;
            }

            /// <summary>
            /// When match a clean version context adds a <see cref="StepCleanVersion"/>.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherCleanVersionContext"/>.</param>
            /// <returns>null.</returns>
            public override object VisitMatcherCleanVersion([NotNull] UserAgentTreeWalkerParser.MatcherCleanVersionContext context)
            {
                this.Visit(context.matcher());
                this.FromHereItCannotBeInHashMapAnymore();
                this.Add(new StepCleanVersion());
                return null; // Void
            }

            /// <summary>
            /// When match a concat context adds a <see cref="StepConcat"/>.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherConcatContext"/>.</param>
            /// <returns>null.</returns>
            public override object VisitMatcherConcat([NotNull] UserAgentTreeWalkerParser.MatcherConcatContext context)
            {
                this.Visit(context.matcher());
                this.FromHereItCannotBeInHashMapAnymore();
                this.Add(new StepConcat(context.prefix.Text, context.postfix.Text));
                return null; // Void
            }

            /// <summary>
            /// When match a concat postfix context adds a <see cref="StepConcatPostfix"/>.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherConcatPostfixContext"/>.</param>
            /// <returns>null.</returns>
            public override object VisitMatcherConcatPostfix([NotNull] UserAgentTreeWalkerParser.MatcherConcatPostfixContext context)
            {
                this.Visit(context.matcher());
                this.FromHereItCannotBeInHashMapAnymore();
                this.Add(new StepConcatPostfix(context.postfix.Text));
                return null; // Void
            }

            /// <summary>
            /// When match a concat prefix context adds a <see cref="StepConcatPrefix"/>.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherConcatPrefixContext"/>.</param>
            /// <returns>null.</returns>
            public override object VisitMatcherConcatPrefix([NotNull] UserAgentTreeWalkerParser.MatcherConcatPrefixContext context)
            {
                this.Visit(context.matcher());
                this.FromHereItCannotBeInHashMapAnymore();
                this.Add(new StepConcatPrefix(context.prefix.Text));
                return null; // Void
            }

            /// <summary>
            /// When match a normalize brand prefix context adds a <see cref="StepNormalizeBrand"/>.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherNormalizeBrandContext"/>.</param>
            /// <returns>null.</returns>
            public override object VisitMatcherNormalizeBrand([NotNull] UserAgentTreeWalkerParser.MatcherNormalizeBrandContext context)
            {
                this.Visit(context.matcher());
                this.FromHereItCannotBeInHashMapAnymore();
                this.Add(new StepNormalizeBrand());
                return null; // Void
            }

            /// <summary>
            /// When match a path context visits the base path of the context.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherPathContext"/>.</param>
            /// <returns>null.</returns>
            public override object VisitMatcherPath([NotNull] UserAgentTreeWalkerParser.MatcherPathContext context)
            {
                this.Visit(context.basePath());
                return null; // Void
            }

            /// <summary>
            /// When match a path is in lookup prefix context adds a <see cref="StepIsInLookupPrefix"/>.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherPathIsInLookupPrefixContext"/>.</param>
            /// <returns>null.</returns>
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
            /// When match a null path  context adds a <see cref="StepIsNull"/>.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherPathIsNullContext"/>.</param>
            /// <returns>null.</returns>
            public override object VisitMatcherPathIsNull([NotNull] UserAgentTreeWalkerParser.MatcherPathIsNullContext context)
            {
                // Always add this one, it's special
                this.walkList.steps.Add(new StepIsNull());
                this.Visit(context.matcher());
                return null; // Void
            }

            /// <summary>
            /// When match a path lookup context adds a <see cref="StepLookup"/>.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherPathLookupContext"/>.</param>
            /// <returns>null.</returns>
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
            /// When match a path lookup prefix context adds a <see cref="StepLookupPrefix"/>.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherPathLookupPrefixContext"/>.</param>
            /// <returns>null.</returns>
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
            /// When match a word range context adds a <see cref="StepWordRange"/>.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherWordRangeContext"/>.</param>
            /// <returns>null.</returns>
            public override object VisitMatcherWordRange([NotNull] UserAgentTreeWalkerParser.MatcherWordRangeContext context)
            {
                this.Visit(context.matcher());
                this.FromHereItCannotBeInHashMapAnymore();
                this.Add(new StepWordRange(WordRangeVisitor.GetRange(context.wordRange())));
                return null; // Void
            }

            /// <summary>
            /// When visit a path variable context visits next step.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.PathVariableContext"/>.</param>
            /// <returns>null.</returns>
            public override object VisitPathVariable([NotNull] UserAgentTreeWalkerParser.PathVariableContext context)
            {
                this.FromHereItCannotBeInHashMapAnymore();
                this.VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// When visit a path walk context visits next step.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.PathWalkContext"/>.</param>
            /// <returns>null.</returns>
            public override object VisitPathWalk([NotNull] UserAgentTreeWalkerParser.PathWalkContext context)
            {
                this.VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// When visit a step back to full context adds a <see cref="StepBackToFull"/>.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepBackToFullContext"/>.</param>
            /// <returns>null.</returns>
            public override object VisitStepBackToFull([NotNull] UserAgentTreeWalkerParser.StepBackToFullContext context)
            {
                this.Add(new StepBackToFull());
                this.VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// When visit a step that contains value adds a <see cref="StepContains"/>.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepContainsValueContext"/>.</param>
            /// <returns>null.</returns>
            public override object VisitStepContainsValue([NotNull] UserAgentTreeWalkerParser.StepContainsValueContext context)
            {
                this.FromHereItCannotBeInHashMapAnymore();
                this.Add(new StepContains(context.value.Text));
                this.VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// When visit a step down adds a <see cref="StepDown"/>.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepDownContext"/>.</param>
            /// <returns>null.</returns>
            public override object VisitStepDown([NotNull] UserAgentTreeWalkerParser.StepDownContext context)
            {
                this.Add(new StepDown(context.numberRange(), context.name.Text));
                this.VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// When visit a step that ends with value adds a <see cref="StepEndsWith"/>.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepEndsWithValueContext"/>.</param>
            /// <returns>null.</returns>
            public override object VisitStepEndsWithValue([NotNull] UserAgentTreeWalkerParser.StepEndsWithValueContext context)
            {
                this.FromHereItCannotBeInHashMapAnymore();
                this.Add(new StepEndsWith(context.value.Text));
                this.VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// When visit a step that equals with value adds a <see cref="StepEquals"/>.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepEqualsValueContext"/>.</param>
            /// <returns>null.</returns>
            public override object VisitStepEqualsValue([NotNull] UserAgentTreeWalkerParser.StepEqualsValueContext context)
            {
                this.Add(new StepEquals(context.value.Text));
                this.FromHereItCannotBeInHashMapAnymore();
                this.VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            ///  When visit a step that is in a set value adds a <see cref="StepIsInSet"/>.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepIsInSetContext"/>.</param>
            /// <returns>null.</returns>
            public override object VisitStepIsInSet([NotNull] UserAgentTreeWalkerParser.StepIsInSetContext context)
            {
                this.FromHereItCannotBeInHashMapAnymore();

                var lookupSetName = context.set.Text;
                var lookupSet = this.walkList.lookupSets.ContainsKey(lookupSetName) ? this.walkList.lookupSets[lookupSetName] : null;
                if (lookupSet is null)
                {
                    if (this.walkList.lookups.ContainsKey(lookupSetName))
                    {
                        lookupSet = new HashSet<string>(this.walkList.lookups[lookupSetName].Keys);
                    }
                }

                if (lookupSet is null)
                {
                    throw new InvalidParserConfigurationException($"Missing lookupSet \"{lookupSetName}\"");
                }

                this.Add(new StepIsInSet(lookupSetName, lookupSet));
                this.VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// When visit a next step adds a <see cref="StepNext"/>.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepNextContext"/>.</param>
            /// <returns>null.</returns>
            public override object VisitStepNext([NotNull] UserAgentTreeWalkerParser.StepNextContext context)
            {
                this.FromHereItCannotBeInHashMapAnymore();
                this.Add(new StepNext());
                this.VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// When visit 2 next step adds a <see cref="StepNextN"/> with N = 2.
            /// </summary>
            /// <param name="ctx">The ctx<see cref="UserAgentTreeWalkerParser.StepNext2Context"/>.</param>
            /// <returns>null.</returns>
            public override object VisitStepNext2(UserAgentTreeWalkerParser.StepNext2Context ctx)
            {
                return this.DoStepNextN(ctx.nextStep, 2);
            }

            /// <summary>
            /// When visit 3 next step adds a <see cref="StepNextN"/> with N = 3.
            /// </summary>
            /// <param name="ctx">The ctx<see cref="UserAgentTreeWalkerParser.StepNext3Context"/>.</param>
            /// <returns>null.</returns>
            public override object VisitStepNext3(UserAgentTreeWalkerParser.StepNext3Context ctx)
            {
                return this.DoStepNextN(ctx.nextStep, 3);
            }

            /// <summary>
            /// When visit 4 next step adds a <see cref="StepNextN"/> with N = 4.
            /// </summary>
            /// <param name="ctx">The ctx<see cref="UserAgentTreeWalkerParser.StepNext4Context"/>.</param>
            /// <returns>null.</returns>
            public override object VisitStepNext4(UserAgentTreeWalkerParser.StepNext4Context ctx)
            {
                return this.DoStepNextN(ctx.nextStep, 4);
            }

            /// <summary>
            /// When visit a step that is not equal to value adds a <see cref="StepNotEquals"/>.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepNotEqualsValueContext"/>.</param>
            /// <returns>null.</returns>
            public override object VisitStepNotEqualsValue([NotNull] UserAgentTreeWalkerParser.StepNotEqualsValueContext context)
            {
                this.FromHereItCannotBeInHashMapAnymore();
                this.Add(new StepNotEquals(context.value.Text));
                this.VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// When visit a previous step adds a <see cref="StepPrev"/>.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepPrevContext"/>.</param>
            /// <returns>null.</returns>
            public override object VisitStepPrev([NotNull] UserAgentTreeWalkerParser.StepPrevContext context)
            {
                this.FromHereItCannotBeInHashMapAnymore();
                this.Add(new StepPrev());
                this.VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// When visit 2 previous steps adds a <see cref="StepPrevN"/> with N = 2.
            /// </summary>
            /// <param name="ctx">The ctx<see cref="UserAgentTreeWalkerParser.StepPrev2Context"/>.</param>
            /// <returns>The <see cref="object"/>.</returns>
            public override object VisitStepPrev2(UserAgentTreeWalkerParser.StepPrev2Context ctx)
            {
                return this.DoStepPrevN(ctx.nextStep, 2);
            }

            /// <summary>
            /// When visit 3 previous steps adds a <see cref="StepPrevN"/> with N = 3.
            /// </summary>
            /// <param name="ctx">The ctx<see cref="UserAgentTreeWalkerParser.StepPrev3Context"/>.</param>
            /// <returns>The <see cref="object"/>.</returns>
            public override object VisitStepPrev3(UserAgentTreeWalkerParser.StepPrev3Context ctx)
            {
                return this.DoStepPrevN(ctx.nextStep, 3);
            }

            /// <summary>
            /// When visit 4 previous steps adds a <see cref="StepPrevN"/> with N = 4.
            /// </summary>
            /// <param name="ctx">The ctx<see cref="UserAgentTreeWalkerParser.StepPrev4Context"/>.</param>
            /// <returns>The <see cref="object"/>.</returns>
            public override object VisitStepPrev4(UserAgentTreeWalkerParser.StepPrev4Context ctx)
            {
                return this.DoStepPrevN(ctx.nextStep, 4);
            }

            /// <summary>
            /// When visit a step that starts with a value adds a <see cref="StepStartsWith"/>.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepStartsWithValueContext"/>.</param>
            /// <returns>The <see cref="object"/>.</returns>
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
            /// When visit an up step adds a <see cref="StepUp"/>.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepUpContext"/>.</param>
            /// <returns>The <see cref="object"/>.</returns>
            public override object VisitStepUp([NotNull] UserAgentTreeWalkerParser.StepUpContext context)
            {
                this.FromHereItCannotBeInHashMapAnymore();
                this.Add(new StepUp());
                this.VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// When visit a word range step adds a <see cref="StepWordRange"/>.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepWordRangeContext"/>.</param>
            /// <returns>The <see cref="object"/>.</returns>
            public override object VisitStepWordRange([NotNull] UserAgentTreeWalkerParser.StepWordRangeContext context)
            {
                var range = WordRangeVisitor.GetRange(context.wordRange());
                this.Add(new StepWordRange(range));
                this.VisitNext(context.nextStep);
                return null; // Void
            }

            /// <summary>
            /// Used to add a step.
            /// </summary>
            /// <param name="step">The <see cref="Step"/> to be added.</param>
            private void Add(Step step)
            {
                if (this.foundHashEntryPoint)
                {
                    this.walkList.steps.Add(step);
                }
            }

            /// <summary>
            /// Used to elaborate a next step with N.
            /// </summary>
            /// <param name="nextStep">The next step context.</param>
            /// <param name="nextSteps">How many steps.</param>
            /// <returns>null.</returns>
            private object DoStepNextN(UserAgentTreeWalkerParser.PathContext nextStep, int nextSteps)
            {
                this.FromHereItCannotBeInHashMapAnymore();
                this.Add(new StepNextN(nextSteps));
                this.VisitNext(nextStep);
                return null; // Void
            }

            /// <summary>
            /// Used to elaborate a previous step with N.
            /// </summary>
            /// <param name="nextStep">The previous step context.</param>
            /// <param name="prevSteps">How many steps.</param>
            /// <returns>null.</returns>
            private object DoStepPrevN(UserAgentTreeWalkerParser.PathContext nextStep, int prevSteps)
            {
                this.FromHereItCannotBeInHashMapAnymore();
                this.Add(new StepPrevN(prevSteps));
                this.VisitNext(nextStep);
                return null; // Void
            }

            /// <summary>
            /// Used to set that from this point, there can't be anymore hash entry points.
            /// </summary>
            private void FromHereItCannotBeInHashMapAnymore()
            {
                this.foundHashEntryPoint = true;
            }

            /// <summary>
            /// Return a lookup by name.
            /// </summary>
            /// <param name="lookupName">The name of the lookup.</param>
            /// <returns>null.</returns>
            private IDictionary<string, string> GetLookup(string lookupName)
            {
                if (!this.walkList.lookups.ContainsKey(lookupName))
                {
                    throw new InvalidParserConfigurationException($"Missing lookup \"{lookupName}\" ");
                }

                return this.walkList.lookups[lookupName];
            }

            /// <summary>
            /// If we have not yet found an hash entry point.
            /// </summary>
            /// <returns>True if not foyn an entry point yet.</returns>
            private bool StillGoingToHashMap()
            {
                return !this.foundHashEntryPoint;
            }

            /// <summary>
            /// Used to visit a next step, if there is a next step.
            /// </summary>
            /// <param name="nextStep">The nextStep<see cref="UserAgentTreeWalkerParser.PathContext"/>.</param>
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
