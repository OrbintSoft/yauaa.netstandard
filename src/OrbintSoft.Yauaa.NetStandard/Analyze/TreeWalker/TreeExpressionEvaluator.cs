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
        /// Defines the Log.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(TreeExpressionEvaluator));

        /// <summary>
        /// Defines the matcher.
        /// </summary>
        private readonly Matcher matcher;

        /// <summary>
        /// Defines the requiredPatternText.
        /// </summary>
        private readonly string requiredPatternText;

        /// <summary>
        /// Defines the verbose.
        /// </summary>
        private readonly bool verbose;

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeExpressionEvaluator"/> class.
        /// </summary>
        /// <param name="requiredPattern">The requiredPattern<see cref="ParserRuleContext"/>.</param>
        /// <param name="matcher">The matcher<see cref="Matcher"/>.</param>
        /// <param name="verbose">The verbose<see cref="bool"/>.</param>
        public TreeExpressionEvaluator(ParserRuleContext requiredPattern, Matcher matcher, bool verbose)
        {
            this.requiredPatternText = requiredPattern.GetText();
            this.matcher = matcher;
            this.verbose = verbose;
            this.FixedValue = this.CalculateFixedValue(requiredPattern);
            this.WalkListForUnitTesting = new WalkList(requiredPattern, matcher.Lookups, matcher.LookupSets, verbose);
        }

        /// <summary>
        /// Gets the FixedValue
        /// Get the fixed value in case of a fixed value. NULL if a dynamic value.
        /// </summary>
        public string FixedValue { get; }

        /// <summary>
        /// Gets a value indicating whether UsesIsNull.
        /// </summary>
        public bool UsesIsNull => this.WalkListForUnitTesting.UsesIsNull;

        /// <summary>
        /// Gets the WalkListForUnitTesting.
        /// </summary>
        public WalkList WalkListForUnitTesting { get; }

        /// <summary>
        /// The Evaluate.
        /// </summary>
        /// <param name="tree">The tree<see cref="IParseTree"/>.</param>
        /// <param name="key">The key<see cref="string"/>.</param>
        /// <param name="value">The value<see cref="string"/>.</param>
        /// <returns>The <see cref="WalkList.WalkResult"/>.</returns>
        public WalkList.WalkResult Evaluate(IParseTree tree, string key, string value)
        {
            if (this.verbose)
            {
                Log.Info(string.Format("Evaluate: {0} => {1}", key, value));
                Log.Info(string.Format("Pattern : {0}", this.requiredPatternText));
                Log.Info(string.Format("WalkList: {0}", this.WalkListForUnitTesting.ToString()));
            }

            var result = this.WalkListForUnitTesting.Walk(tree, value);
            if (this.verbose)
            {
                Log.Info(string.Format("Evaluate: Result = {0}", result == null ? "null" : result.Value));
            }

            return result;
        }

        /// <summary>
        /// The PruneTrailingStepsThatCannotFail.
        /// </summary>
        public void PruneTrailingStepsThatCannotFail()
        {
            this.WalkListForUnitTesting.PruneTrailingStepsThatCannotFail();
        }

        /// <summary>
        /// The CalculateFixedValue.
        /// </summary>
        /// <param name="requiredPattern">The requiredPattern<see cref="ParserRuleContext"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string CalculateFixedValue(ParserRuleContext requiredPattern)
        {
            return new DerivedUserAgentTreeWalkerBaseVisitor(this.matcher).Visit(requiredPattern);
        }

        /// <summary>
        /// Defines the <see cref="DerivedUserAgentTreeWalkerBaseVisitor" />.
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
            /// <param name="matcher">The matcher<see cref="Matcher"/>.</param>
            public DerivedUserAgentTreeWalkerBaseVisitor(Matcher matcher)
            {
                this.matcher = matcher;
            }

            /// <summary>
            /// The VisitMatcherPathLookup.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherPathLookupContext"/>.</param>
            /// <returns>The <see cref="string"/>.</returns>
            public override string VisitMatcherPathLookup([NotNull] UserAgentTreeWalkerParser.MatcherPathLookupContext context)
            {
                return this.VisitLookups(context.matcher(), context.lookup, context.defaultValue);
            }

            /// <summary>
            /// The VisitPathFixedValue.
            /// </summary>
            /// <param name="ctx">The ctx<see cref="UserAgentTreeWalkerParser.PathFixedValueContext"/>.</param>
            /// <returns>The <see cref="string"/>.</returns>
            public override string VisitPathFixedValue(UserAgentTreeWalkerParser.PathFixedValueContext ctx)
            {
                return ctx.value.Text;
            }

            /// <summary>
            /// The AggregateResult.
            /// </summary>
            /// <param name="aggregate">The aggregate<see cref="string"/>.</param>
            /// <param name="nextResult">The nextResult<see cref="string"/>.</param>
            /// <returns>The <see cref="string"/>.</returns>
            protected override string AggregateResult(string aggregate, string nextResult)
            {
                return nextResult ?? aggregate;
            }

            /// <summary>
            /// The ShouldVisitNextChild.
            /// </summary>
            /// <param name="node">The node<see cref="IRuleNode"/>.</param>
            /// <param name="currentResult">The currentResult<see cref="string"/>.</param>
            /// <returns>The <see cref="bool"/>.</returns>
            protected override bool ShouldVisitNextChild([NotNull] IRuleNode node, string currentResult)
            {
                return currentResult == null;
            }

            /// <summary>
            /// The VisitLookups.
            /// </summary>
            /// <param name="matcherTree">The matcherTree<see cref="IParseTree"/>.</param>
            /// <param name="lookup">The lookup<see cref="IToken"/>.</param>
            /// <param name="defaultValue">The defaultValue<see cref="IToken"/>.</param>
            /// <returns>The <see cref="string"/>.</returns>
            private string VisitLookups(IParseTree matcherTree, IToken lookup, IToken defaultValue)
            {
                var value = this.Visit(matcherTree);
                if (value == null)
                {
                    return null;
                }

                // Now we know this is a fixed value. Yet we can have a problem in the lookup that was
                // configured. If we have this then this is a FATAL error (it will fail always everywhere).
                if (!this.matcher.Lookups.ContainsKey(lookup.Text))
                {
                    throw new InvalidParserConfigurationException("Missing lookup \"" + lookup.Text + "\" ");
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

                    throw new InvalidParserConfigurationException(
                        "Fixed value >>" + value + "<< is missing in lookup: \"" + lookup.Text + "\" ");
                }
            }
        }
    }
}
