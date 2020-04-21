//-----------------------------------------------------------------------
// <copyright file="TreeExpressionEvaluator.cs" company="OrbintSoft">
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

namespace OrbintSoft.Yauaa.Analyze.TreeWalker
{
    using System;
    using Antlr4.Runtime;
    using Antlr4.Runtime.Misc;
    using Antlr4.Runtime.Tree;
    using log4net;
    using OrbintSoft.Yauaa.Analyze.TreeWalker.Steps;
    using OrbintSoft.Yauaa.Antlr4Source;

    /// <summary>
    /// This class gets the symbol table (1 value) uses that to evaluate
    /// the expression against the parsed user agent.
    /// </summary>
    [Serializable]
    public class TreeExpressionEvaluator
    {
        /// <summary>
        /// Defines the logger.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(TreeExpressionEvaluator));

        /// <summary>
        /// Defines the matcher.
        /// </summary>
        private readonly Matcher matcher;

        /// <summary>
        /// The required text pattern.
        /// </summary>
        private readonly string requiredPatternText;

        /// <summary>
        /// True if verbose logging is enabled.
        /// </summary>
        private readonly bool verbose;

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeExpressionEvaluator"/> class.
        /// </summary>
        /// <param name="requiredPattern">The requiredPattern<see cref="ParserRuleContext"/>.</param>
        /// <param name="matcher">The <see cref="Matcher"/>.</param>
        /// <param name="verbose">True to enable verbose logging.</param>
        public TreeExpressionEvaluator(ParserRuleContext requiredPattern, Matcher matcher, bool verbose)
        {
            this.requiredPatternText = requiredPattern.GetText();
            this.matcher = matcher;
            this.verbose = verbose;
            this.FixedValue = this.CalculateFixedValue(requiredPattern);
            this.WalkListForUnitTesting = new WalkList(requiredPattern, matcher.Lookups, matcher.LookupSets, verbose);
        }

        /// <summary>
        /// Gets the fixed value in case of a fixed value. NULL if a dynamic value.
        /// </summary>
        public string FixedValue { get; }

        /// <summary>
        /// Gets a value indicating whether walkList for unit testing uses null.
        /// </summary>
        public bool UsesIsNull => this.WalkListForUnitTesting.UsesIsNull;

        /// <summary>
        /// Gets the walklist for Unit testing.
        /// </summary>
        internal WalkList WalkListForUnitTesting { get; }

        /// <summary>
        /// Evaluates the tree.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="WalkList.WalkResult"/>.</returns>
        public WalkList.WalkResult Evaluate(IParseTree tree, string key, string value)
        {
            if (this.verbose)
            {
                Log.Info($"Evaluate: {key} => {value}");
                Log.Info($"Pattern : {this.requiredPatternText}");
                Log.Info($"WalkList: {this.WalkListForUnitTesting}");
            }

            var result = this.WalkListForUnitTesting.Walk(tree, value);
            if (this.verbose)
            {
                Log.Info($"Evaluate: Result = {(result is null ? "null" : result.Value)}");
            }

            return result;
        }

        /// <summary>
        /// Finds the last step that cannot fail, and removes the following steps.
        /// </summary>
        public void PruneTrailingStepsThatCannotFail()
        {
            this.WalkListForUnitTesting.PruneTrailingStepsThatCannotFail();
        }

        /// <summary>
        /// Calculated the fixed value from pattern.
        /// </summary>
        /// <param name="requiredPattern">The pattern.</param>
        /// <returns>The result.</returns>
        private string CalculateFixedValue(ParserRuleContext requiredPattern)
        {
            return new DerivedUserAgentTreeWalkerBaseVisitor(this.matcher).Visit(requiredPattern);
        }

        /// <summary>
        /// This class derives and implement the tree walker user agent visitor.
        /// </summary>
        private class DerivedUserAgentTreeWalkerBaseVisitor : UserAgentTreeWalkerBaseVisitor<string>
        {
            /// <summary>
            /// Defines the matcher.
            /// </summary>
            private readonly Matcher matcher;

            /// <summary>
            /// Initializes a new instance of the <see cref="DerivedUserAgentTreeWalkerBaseVisitor"/> class.
            /// </summary>
            /// <param name="matcher">The <see cref="Matcher"/>.</param>
            public DerivedUserAgentTreeWalkerBaseVisitor(Matcher matcher)
            {
                this.matcher = matcher;
            }

            /// <summary>
            /// When match a path lookup visit the lookups.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherPathLookupContext"/>.</param>
            /// <returns>The <see cref="string"/>.</returns>
            public override string VisitMatcherPathLookup([NotNull] UserAgentTreeWalkerParser.MatcherPathLookupContext context)
            {
                return this.VisitLookups(context.matcher(), context.lookup, context.defaultValue);
            }

            /// <summary>
            /// When visit a path lookup visits the lookups.
            /// </summary>
            /// <param name="ctx">The ctx<see cref="UserAgentTreeWalkerParser.PathFixedValueContext"/>.</param>
            /// <returns>The <see cref="string"/>.</returns>
            public override string VisitPathFixedValue(UserAgentTreeWalkerParser.PathFixedValueContext ctx)
            {
                return ctx.value.Text;
            }

            /// <summary>
            /// If next result is available returs the next result otherwise the aggreagate.
            /// </summary>
            /// <param name="aggregate">The aggregate.</param>
            /// <param name="nextResult">The nextResult.</param>
            /// <returns>The result.</returns>
            protected override string AggregateResult(string aggregate, string nextResult)
            {
                return nextResult ?? aggregate;
            }

            /// <summary>
            /// If we should visit the next child.
            /// </summary>
            /// <param name="node">The node.</param>
            /// <param name="currentResult">The currentResult.</param>
            /// <returns>True if current result is null.</returns>
            protected override bool ShouldVisitNextChild([NotNull] IRuleNode node, string currentResult)
            {
                return currentResult is null;
            }

            /// <summary>
            /// Visits the lookups.
            /// </summary>
            /// <param name="matcherTree">The matcher tree.</param>
            /// <param name="lookup">The lookup.</param>
            /// <param name="defaultValue">The default value.</param>
            /// <returns>The value in the lookup or the default value if no value found.</returns>
            private string VisitLookups(IParseTree matcherTree, IToken lookup, IToken defaultValue)
            {
                var value = this.Visit(matcherTree);
                if (value is null)
                {
                    return null;
                }

                // Now we know this is a fixed value. Yet we can have a problem in the lookup that was
                // configured. If we have this then this is a FATAL error (it will fail always everywhere).
                if (!this.matcher.Lookups.ContainsKey(lookup.Text))
                {
                    throw new InvalidParserConfigurationException($"Missing lookup \"{lookup.Text}\" ");
                }

                var lookupMap = this.matcher.Lookups[lookup.Text];
                if (lookupMap.ContainsKey(value.ToLower()))
                {
                    return lookupMap[value.ToLower()];
                }
                else
                {
                    if (defaultValue != null)
                    {
                        return defaultValue.Text;
                    }

                    throw new InvalidParserConfigurationException($"Fixed value >>{value}<< is missing in lookup: \"{lookup.Text}\" ");
                }
            }
        }
    }
}
