//-----------------------------------------------------------------------
// <copyright file="MatcherAction.cs" company="OrbintSoft">
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

namespace OrbintSoft.Yauaa.Analyze
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Antlr4.Runtime;
    using Antlr4.Runtime.Atn;
    using Antlr4.Runtime.Dfa;
    using Antlr4.Runtime.Misc;
    using Antlr4.Runtime.Sharpen;
    using Antlr4.Runtime.Tree;
    using log4net;
    using OrbintSoft.Yauaa.Analyze.TreeWalker;
    using OrbintSoft.Yauaa.Analyze.TreeWalker.Steps;
    using OrbintSoft.Yauaa.Antlr4Source;

    /// <summary>
    /// Defines the <see cref="MatcherAction" />.
    /// </summary>
    [Serializable]
    public abstract class MatcherAction
    {
        /// <summary>
        /// Defines the CalculateInformPaths.
        /// </summary>
        private static readonly IDictionary<Type, CalculateInformPathFunction> CalculateInformPaths = new Dictionary<Type, CalculateInformPathFunction>();

        /// <summary>
        /// Defines the Log.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(MatcherAction));

        /// <summary>
        /// Defines the matcher.
        /// </summary>
        private Matcher matcher = null;

        /// <summary>
        /// Defines the verbosePermanent.
        /// </summary>
        private bool verbosePermanent = false;

        /// <summary>
        /// Defines the verboseTemporary.
        /// </summary>
        private bool verboseTemporary = false;

        /// <summary>
        /// Initializes static members of the <see cref="MatcherAction"/> class.
        /// </summary>
        static MatcherAction()
        {
            // -------------
            CalculateInformPaths[typeof(UserAgentTreeWalkerParser.MatcherBaseContext)] =
                (action, treeName, tree) => CalculateInformPath(action, treeName, ((UserAgentTreeWalkerParser.MatcherBaseContext)tree).matcher());
            CalculateInformPaths[typeof(UserAgentTreeWalkerParser.MatcherPathIsNullContext)] =
                (action, treeName, tree) => CalculateInformPath(action, treeName, ((UserAgentTreeWalkerParser.MatcherPathIsNullContext)tree).matcher());

            // -------------
            CalculateInformPaths[typeof(UserAgentTreeWalkerParser.MatcherExtractContext)] =
                (action, treeName, tree) => CalculateInformPath(action, treeName, ((UserAgentTreeWalkerParser.MatcherExtractContext)tree).expression);

            // -------------
            CalculateInformPaths[typeof(UserAgentTreeWalkerParser.MatcherVariableContext)] =
                (action, treeName, tree) => CalculateInformPath(action, treeName, ((UserAgentTreeWalkerParser.MatcherVariableContext)tree).expression);

            // -------------
            CalculateInformPaths[typeof(UserAgentTreeWalkerParser.MatcherPathContext)] = (action, treeName, tree) => CalculateInformPath(action, treeName, ((UserAgentTreeWalkerParser.MatcherPathContext)tree).basePath());
            CalculateInformPaths[typeof(UserAgentTreeWalkerParser.MatcherConcatContext)] = (action, treeName, tree) => CalculateInformPath(action, treeName, ((UserAgentTreeWalkerParser.MatcherConcatContext)tree).matcher());
            CalculateInformPaths[typeof(UserAgentTreeWalkerParser.MatcherConcatPrefixContext)] = (action, treeName, tree) => CalculateInformPath(action, treeName, ((UserAgentTreeWalkerParser.MatcherConcatPrefixContext)tree).matcher());
            CalculateInformPaths[typeof(UserAgentTreeWalkerParser.MatcherConcatPostfixContext)] = (action, treeName, tree) => CalculateInformPath(action, treeName, ((UserAgentTreeWalkerParser.MatcherConcatPostfixContext)tree).matcher());
            CalculateInformPaths[typeof(UserAgentTreeWalkerParser.MatcherNormalizeBrandContext)] = (action, treeName, tree) => CalculateInformPath(action, treeName, ((UserAgentTreeWalkerParser.MatcherNormalizeBrandContext)tree).matcher());
            CalculateInformPaths[typeof(UserAgentTreeWalkerParser.MatcherCleanVersionContext)] = (action, treeName, tree) => CalculateInformPath(action, treeName, ((UserAgentTreeWalkerParser.MatcherCleanVersionContext)tree).matcher());
            CalculateInformPaths[typeof(UserAgentTreeWalkerParser.MatcherPathLookupContext)] = (action, treeName, tree) => CalculateInformPath(action, treeName, ((UserAgentTreeWalkerParser.MatcherPathLookupContext)tree).matcher());
            CalculateInformPaths[typeof(UserAgentTreeWalkerParser.MatcherPathLookupPrefixContext)] = (action, treeName, tree) => CalculateInformPath(action, treeName, ((UserAgentTreeWalkerParser.MatcherPathLookupPrefixContext)tree).matcher());
            CalculateInformPaths[typeof(UserAgentTreeWalkerParser.MatcherPathIsInLookupPrefixContext)] = (action, treeName, tree) => CalculateInformPath(action, treeName, ((UserAgentTreeWalkerParser.MatcherPathIsInLookupPrefixContext)tree).matcher());
            CalculateInformPaths[typeof(UserAgentTreeWalkerParser.MatcherWordRangeContext)] = (action, treeName, tree) => CalculateInformPath(action, treeName, ((UserAgentTreeWalkerParser.MatcherWordRangeContext)tree).matcher());

            // -------------
            CalculateInformPaths[typeof(UserAgentTreeWalkerParser.PathVariableContext)] = (action, treeName, tree) =>
            {
                action.matcher.InformMeAboutVariable(action, ((UserAgentTreeWalkerParser.PathVariableContext)tree).variable.Text);
                return 0;
            };

            CalculateInformPaths[typeof(UserAgentTreeWalkerParser.PathWalkContext)] = (action, treeName, tree) => CalculateInformPath(action, treeName, ((UserAgentTreeWalkerParser.PathWalkContext)tree).nextStep);

            // -------------
            CalculateInformPaths[typeof(UserAgentTreeWalkerParser.StepDownContext)] = (action, treeName, tree) =>
            {
                var thisTree = (UserAgentTreeWalkerParser.StepDownContext)tree;
                var informs = 0;
                foreach (var number in NumberRangeVisitor.Instance.Visit(thisTree.numberRange()))
                {
                    informs += CalculateInformPath(action, treeName + '.' + "(" + number + ")" + thisTree.name.Text, thisTree.nextStep);
                }

                return informs;
            };

            CalculateInformPaths[typeof(UserAgentTreeWalkerParser.StepEqualsValueContext)] = (action, treeName, tree) =>
            {
                var thisTree = (UserAgentTreeWalkerParser.StepEqualsValueContext)tree;
                action.matcher.InformMeAbout(action, treeName + "=\"" + thisTree.value.Text + "\"");
                return 1;
            };

            CalculateInformPaths[typeof(UserAgentTreeWalkerParser.StepStartsWithValueContext)] = (action, treeName, tree) =>
            {
                var thisTree = (UserAgentTreeWalkerParser.StepStartsWithValueContext)tree;
                action.matcher.InformMeAboutPrefix(action, treeName, thisTree.value.Text);
                return 1;
            };

            CalculateInformPaths[typeof(UserAgentTreeWalkerParser.StepWordRangeContext)] = (action, treeName, tree) =>
            {
                var thisTree = (UserAgentTreeWalkerParser.StepWordRangeContext)tree;
                var range = WordRangeVisitor.GetRange(thisTree.wordRange());
                action.matcher.LookingForRange(treeName, range);

                return CalculateInformPath(action, treeName + range, thisTree.nextStep);
            };
        }

        /// <summary>
        /// The CalculateInformPathFunction.
        /// </summary>
        /// <param name="action">The action<see cref="MatcherAction"/>.</param>
        /// <param name="treeName">The treeName<see cref="string"/>.</param>
        /// <param name="tree">The tree<see cref="ParserRuleContext"/>.</param>
        /// <returns>The <see cref="int"/>.</returns>
        internal delegate int CalculateInformPathFunction(MatcherAction action, string treeName, ParserRuleContext tree);

        /// <summary>
        /// Gets the Matches.
        /// </summary>
        public MatchesList Matches { get; private set; } = null;

        /// <summary>
        /// Gets the MatchExpression.
        /// </summary>
        public string MatchExpression { get; private set; } = null;

        /// <summary>
        /// Gets the EvaluatorForUnitTesting
        /// The GetEvaluatorForUnitTesting.
        /// </summary>
        internal TreeExpressionEvaluator EvaluatorForUnitTesting { get => this.Evaluator; }

        /// <summary>
        /// Gets a value indicating whether MustHaveMatches.
        /// </summary>
        internal bool MustHaveMatches { get; private set; } = false;

        /// <summary>
        /// Gets a value indicating whether Verbose.
        /// </summary>
        internal bool Verbose { get; private set; } = false;

        /// <summary>
        /// Gets or sets the Evaluator
        /// Defines the evaluator.
        /// </summary>
        protected TreeExpressionEvaluator Evaluator { get; set; } = null;

        /// <summary>
        /// The Inform.
        /// </summary>
        /// <param name="key">The key<see cref="string"/>.</param>
        /// <param name="value">The value<see cref="string"/>.</param>
        /// <param name="result">The result<see cref="IParseTree"/>.</param>
        public void Inform(string key, string value, IParseTree result)
        {
            // Only if this needs input we tell the matcher on the first one.
            if (this.MustHaveMatches && this.Matches.Count == 0)
            {
                this.matcher.GotMyFirstStartingPoint();
            }

            this.Matches.Add(key, value, result);
        }

        /// <summary>
        /// The Inform.
        /// </summary>
        /// <param name="key">The key<see cref="string"/>.</param>
        /// <param name="foundValue">The foundValue<see cref="WalkList.WalkResult"/>.</param>
        public abstract void Inform(string key, WalkList.WalkResult foundValue);

        /// <summary>
        /// The Initialize.
        /// </summary>
        public virtual void Initialize()
        {
            var lexerErrorListener = new InitErrorListener<int>(this);
            var input = new AntlrInputStream(this.MatchExpression);

            var lexer = new UserAgentTreeWalkerLexer(input);

            lexer.AddErrorListener(lexerErrorListener);

            var parserErrorListener = new InitErrorListener<IToken>(this);
            var tokens = new CommonTokenStream(lexer);
            var parser = new UserAgentTreeWalkerParser(tokens);

            parser.AddErrorListener(parserErrorListener);

            var requiredPattern = this.ParseWalkerExpression(parser);

            // We couldn't ditch the double quotes around the fixed values in the parsing phase.
            // So we ditch them here. We simply walk the tree and modify some of the tokens.
            new UnQuoteValues().Visit(requiredPattern);

            // Now we create an evaluator instance
            this.Evaluator = new TreeExpressionEvaluator(requiredPattern, this.matcher, this.Verbose);

            // Is a fixed value (i.e. no events will ever be fired)?
            var fixedValue = this.Evaluator.FixedValue;
            if (fixedValue != null)
            {
                this.SetFixedValue(fixedValue);
                this.MustHaveMatches = false;
                this.Matches = new MatchesList(0);
                return; // Not interested in any patterns
            }

            this.MustHaveMatches = !this.Evaluator.UsesIsNull;

            var informs = CalculateInformPath(this, "agent", requiredPattern);

            // If this is based on a variable we do not need any matches from the hashmap.
            if (this.MustHaveMatches && informs == 0)
            {
                this.MustHaveMatches = false;
            }

            var listSize = 0;
            if (informs > 0)
            {
                listSize = 1;
            }

            this.Matches = new MatchesList(listSize);
        }

        /// <summary>
        /// Called after all nodes have been notified.
        /// </summary>
        /// <returns>true if the obtainResult result was valid. False will fail the entire matcher this belongs to.</returns>
        public abstract bool ObtainResult();

        /// <summary>
        /// The Reset.
        /// </summary>
        public virtual void Reset()
        {
            this.Matches.Clear();
            if (this.verboseTemporary)
            {
                this.Verbose = this.verbosePermanent;
            }
        }

        /// <summary>
        /// The SetVerbose.
        /// </summary>
        /// <param name="newVerbose">The newVerbose<see cref="bool"/>.</param>
        /// <param name="temporary">The temporary<see cref="bool"/>.</param>
        public void SetVerbose(bool newVerbose, bool temporary)
        {
            this.Verbose = newVerbose;
            if (!temporary)
            {
                this.verbosePermanent = newVerbose;
            }

            this.verboseTemporary = temporary;
        }

        /// <summary>
        /// The CannotBeValid
        /// If it is impossible that this can be valid it returns true, else false.
        /// </summary>
        /// <returns>The <see cref="bool"/>.</returns>
        internal bool CannotBeValid()
        {
            return this.MustHaveMatches && this.Matches.Count == 0;
        }

        /// <summary>
        /// Initialize the matcher.
        /// </summary>
        /// <param name="newMatchExpression">The newMatchExpression<see cref="string"/>.</param>
        /// <param name="newMatcher">The newMatcher<see cref="Matcher"/>.</param>
        internal void Init(string newMatchExpression, Matcher newMatcher)
        {
            this.matcher = newMatcher;
            this.MatchExpression = newMatchExpression;
            this.SetVerbose(newMatcher.Verbose);
        }

        /// <summary>
        /// The IsValidIsNull.
        /// </summary>
        /// <returns>The <see cref="bool"/>.</returns>
        internal bool IsValidIsNull()
        {
            return this.Matches.Count == 0 && this.Evaluator.UsesIsNull;
        }

        /// <summary>
        /// Optimization: Only if there is a possibility that all actions for this matcher CAN be valid do we
        /// actually perform the analysis and do the(expensive) tree walking and matching.
        /// </summary>
        internal void ProcessInformedMatches()
        {
            foreach (var match in this.Matches)
            {
                var matchedValue = this.Evaluator.Evaluate(match.GetResult(), match.Key, match.Value);
                if (matchedValue != null)
                {
                    this.Inform(match.Key, matchedValue);
                    break; // We always stick to the first match
                }
            }
        }

        /// <summary>
        /// The ParseWalkerExpression.
        /// </summary>
        /// <param name="parser">The parser<see cref="UserAgentTreeWalkerParser"/>.</param>
        /// <returns>The <see cref="ParserRuleContext"/>.</returns>
        protected abstract ParserRuleContext ParseWalkerExpression(UserAgentTreeWalkerParser parser);

        /// <summary>
        /// The SetFixedValue.
        /// </summary>
        /// <param name="newFixedValue">The newFixedValue<see cref="string"/>.</param>
        protected abstract void SetFixedValue(string newFixedValue);

        /// <summary>
        /// The CalculateInformPath.
        /// </summary>
        /// <param name="action">The action<see cref="MatcherAction"/>.</param>
        /// <param name="treeName">The treeName<see cref="string"/>.</param>
        /// <param name="tree">The tree<see cref="ParserRuleContext"/>.</param>
        /// <returns>The <see cref="int"/>.</returns>
        private static int CalculateInformPath(MatcherAction action, string treeName, ParserRuleContext tree)
        {
            if (tree == null)
            {
                action.matcher.InformMeAbout(action, treeName);
                return 1;
            }

            var type = tree.GetType();
            if (CalculateInformPaths.ContainsKey(type))
            {
                return CalculateInformPaths[type].Invoke(action, treeName, tree);
            }

            action.matcher.InformMeAbout(action, treeName);
            return 1;
        }

        /// <summary>
        /// The SetVerbose.
        /// </summary>
        /// <param name="newVerbose">The newVerbose<see cref="bool"/>.</param>
        private void SetVerbose(bool newVerbose)
        {
            this.SetVerbose(newVerbose, false);
        }

        /// <summary>
        /// Defines the <see cref="InitErrorListener{T}" />.
        /// </summary>
        /// <typeparam name="T">The type if listener.</typeparam>
        internal class InitErrorListener<T> : IAntlrErrorListener<T>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="InitErrorListener{T}"/> class.
            /// </summary>
            /// <param name="matcherAction">The matcherAction<see cref="MatcherAction"/>.</param>
            public InitErrorListener(MatcherAction matcherAction)
                : base()
            {
                this.MatcherAction = matcherAction;
            }

            /// <summary>
            /// Gets or sets the MatcherAction.
            /// </summary>
            public MatcherAction MatcherAction { get; set; }

            /// <summary>
            /// The ReportAmbiguity.
            /// </summary>
            /// <param name="recognizer">The recognizer<see cref="Parser"/>.</param>
            /// <param name="dfa">The dfa<see cref="DFA"/>.</param>
            /// <param name="startIndex">The startIndex<see cref="int"/>.</param>
            /// <param name="stopIndex">The stopIndex<see cref="int"/>.</param>
            /// <param name="exact">The exact<see cref="bool"/>.</param>
            /// <param name="ambigAlts">The ambigAlts<see cref="BitSet"/>.</param>
            /// <param name="configs">The configs<see cref="ATNConfigSet"/>.</param>
            public void ReportAmbiguity(Parser recognizer, DFA dfa, int startIndex, int stopIndex, bool exact, BitSet ambigAlts, ATNConfigSet configs)
            {
            }

            /// <summary>
            /// The ReportAttemptingFullContext.
            /// </summary>
            /// <param name="recognizer">The recognizer<see cref="Parser"/>.</param>
            /// <param name="dfa">The dfa<see cref="DFA"/>.</param>
            /// <param name="startIndex">The startIndex<see cref="int"/>.</param>
            /// <param name="stopIndex">The stopIndex<see cref="int"/>.</param>
            /// <param name="conflictingAlts">The conflictingAlts<see cref="BitSet"/>.</param>
            /// <param name="conflictState">The conflictState<see cref="SimulatorState"/>.</param>
            public void ReportAttemptingFullContext(Parser recognizer, DFA dfa, int startIndex, int stopIndex, BitSet conflictingAlts, SimulatorState conflictState)
            {
            }

            /// <summary>
            /// The ReportContextSensitivity.
            /// </summary>
            /// <param name="recognizer">The recognizer<see cref="Parser"/>.</param>
            /// <param name="dfa">The dfa<see cref="DFA"/>.</param>
            /// <param name="startIndex">The startIndex<see cref="int"/>.</param>
            /// <param name="stopIndex">The stopIndex<see cref="int"/>.</param>
            /// <param name="prediction">The prediction<see cref="int"/>.</param>
            /// <param name="acceptState">The acceptState<see cref="SimulatorState"/>.</param>
            public void ReportContextSensitivity(Parser recognizer, DFA dfa, int startIndex, int stopIndex, int prediction, SimulatorState acceptState)
            {
            }

            /// <summary>
            ///  The SyntaxError.
            /// </summary>
            /// <param name="output">The output <see cref="TextWriter"/>.</param>
            /// <param name="recognizer">The recognizer.</param>
            /// <param name="offendingSymbol">The offendingSymbol.</param>
            /// <param name="line">The line.</param>
            /// <param name="charPositionInLine">The charPositionInLine.</param>
            /// <param name="msg">The msg.</param>
            /// <param name="e">The e.</param>
            public void SyntaxError(TextWriter output, IRecognizer recognizer, T offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
            {
                Log.Error("Syntax error");
                Log.Error(string.Format("Source : {0}", this.MatcherAction.MatchExpression));
                Log.Error(string.Format("Message: {0}", msg));
                throw new InvalidParserConfigurationException("Syntax error \"" + msg + "\" caused by \"" + this.MatcherAction.MatchExpression + "\".");
            }
        }

        /// <summary>
        /// Defines the <see cref="UnQuoteValues" />.
        /// </summary>
        private class UnQuoteValues : UserAgentTreeWalkerBaseVisitor<object>
        {
            /// <summary>
            /// The VisitMatcherConcat.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherConcatContext"/>.</param>
            /// <returns>The <see cref="object"/>.</returns>
            public override object VisitMatcherConcat([NotNull] UserAgentTreeWalkerParser.MatcherConcatContext context)
            {
                this.UnQuoteToken(context.prefix);
                this.UnQuoteToken(context.postfix);
                return base.VisitMatcherConcat(context);
            }

            /// <summary>
            /// The VisitMatcherConcatPostfix.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherConcatPostfixContext"/>.</param>
            /// <returns>The <see cref="object"/>.</returns>
            public override object VisitMatcherConcatPostfix([NotNull] UserAgentTreeWalkerParser.MatcherConcatPostfixContext context)
            {
                this.UnQuoteToken(context.postfix);
                return base.VisitMatcherConcatPostfix(context);
            }

            /// <summary>
            /// The VisitMatcherConcatPrefix.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherConcatPrefixContext"/>.</param>
            /// <returns>The <see cref="object"/>.</returns>
            public override object VisitMatcherConcatPrefix([NotNull] UserAgentTreeWalkerParser.MatcherConcatPrefixContext context)
            {
                this.UnQuoteToken(context.prefix);
                return base.VisitMatcherConcatPrefix(context);
            }

            /// <summary>
            /// The VisitMatcherPathLookup.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherPathLookupContext"/>.</param>
            /// <returns>The <see cref="object"/>.</returns>
            public override object VisitMatcherPathLookup([NotNull] UserAgentTreeWalkerParser.MatcherPathLookupContext context)
            {
                this.UnQuoteToken(context.defaultValue);
                return base.VisitMatcherPathLookup(context);
            }

            /// <summary>
            /// The VisitMatcherPathLookupPrefix.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherPathLookupPrefixContext"/>.</param>
            /// <returns>The <see cref="object"/>.</returns>
            public override object VisitMatcherPathLookupPrefix([NotNull] UserAgentTreeWalkerParser.MatcherPathLookupPrefixContext context)
            {
                this.UnQuoteToken(context.defaultValue);
                return base.VisitMatcherPathLookupPrefix(context);
            }

            /// <summary>
            /// The VisitPathFixedValue.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.PathFixedValueContext"/>.</param>
            /// <returns>The <see cref="object"/>.</returns>
            public override object VisitPathFixedValue([NotNull] UserAgentTreeWalkerParser.PathFixedValueContext context)
            {
                this.UnQuoteToken(context.value);
                return base.VisitPathFixedValue(context);
            }

            /// <summary>
            /// The VisitStepContainsValue.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepContainsValueContext"/>.</param>
            /// <returns>The <see cref="object"/>.</returns>
            public override object VisitStepContainsValue([NotNull] UserAgentTreeWalkerParser.StepContainsValueContext context)
            {
                this.UnQuoteToken(context.value);
                return base.VisitStepContainsValue(context);
            }

            /// <summary>
            /// The VisitStepEndsWithValue.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepEndsWithValueContext"/>.</param>
            /// <returns>The <see cref="object"/>.</returns>
            public override object VisitStepEndsWithValue([NotNull] UserAgentTreeWalkerParser.StepEndsWithValueContext context)
            {
                this.UnQuoteToken(context.value);
                return base.VisitStepEndsWithValue(context);
            }

            /// <summary>
            /// The VisitStepEqualsValue.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepEqualsValueContext"/>.</param>
            /// <returns>The <see cref="object"/>.</returns>
            public override object VisitStepEqualsValue([NotNull] UserAgentTreeWalkerParser.StepEqualsValueContext context)
            {
                this.UnQuoteToken(context.value);
                return base.VisitStepEqualsValue(context);
            }

            /// <summary>
            /// The VisitStepNotEqualsValue.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepNotEqualsValueContext"/>.</param>
            /// <returns>The <see cref="object"/>.</returns>
            public override object VisitStepNotEqualsValue([NotNull] UserAgentTreeWalkerParser.StepNotEqualsValueContext context)
            {
                this.UnQuoteToken(context.value);
                return base.VisitStepNotEqualsValue(context);
            }

            /// <summary>
            /// The VisitStepStartsWithValue.
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepStartsWithValueContext"/>.</param>
            /// <returns>The <see cref="object"/>.</returns>
            public override object VisitStepStartsWithValue([NotNull] UserAgentTreeWalkerParser.StepStartsWithValueContext context)
            {
                this.UnQuoteToken(context.value);
                return base.VisitStepStartsWithValue(context);
            }

            /// <summary>
            /// The UnQuoteToken.
            /// </summary>
            /// <param name="token">The token<see cref="IToken"/>.</param>
            private void UnQuoteToken(IToken token)
            {
                if (token is CommonToken commonToken)
                {
                    commonToken.StartIndex += 1;
                    commonToken.StopIndex -= 1;
                }
            }
        }
    }
}
