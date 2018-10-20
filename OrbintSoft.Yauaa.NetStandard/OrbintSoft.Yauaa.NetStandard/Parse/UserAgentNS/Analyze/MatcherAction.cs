/*
 * Yet Another UserAgent Analyzer .NET Standard
 * Porting realized by Balzarotti Stefano, Copyright (C) OrbintSoft
 * 
 * Original Author and License:
 * 
 * Yet Another UserAgent Analyzer
 * Copyright (C) 2013-2018 Niels Basjes
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * All rights should be reserved to the original author Niels Basjes
 */

using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;
using Antlr4.Runtime.Tree;
using log4net;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze.TreeWalker;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze.TreeWalker.Steps;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Antlr4Source;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze
{
    public abstract class MatcherAction
    {
        private string matchExpression;
        private TreeExpressionEvaluator evaluator;

        internal TreeExpressionEvaluator GetEvaluatorForUnitTesting()
        {
            return evaluator;
        }

        private static readonly ILog LOG = LogManager.GetLogger(typeof(MatcherAction));


        private Matcher matcher;
        private MatchesList matches;
        internal bool MustHaveMatches { get; private set; } = false;

        public bool verbose = false;
        private bool verbosePermanent = false;
        private bool verboseTemporary = false;

        private void SetVerbose(bool newVerbose)
        {
            SetVerbose(newVerbose, false);
        }

        public void SetVerbose(bool newVerbose, bool temporary)
        {
            verbose = newVerbose;
            if (!temporary)
            {
                verbosePermanent = newVerbose;
            }
            verboseTemporary = temporary;
        }

        public string GetMatchExpression()
        {
            return matchExpression;
        }

        internal class InitErrorListener<T>: IAntlrErrorListener<T>
        {
            public MatcherAction MatcherAction { get; set; }

            public InitErrorListener(MatcherAction matcherAction) :base()
            {
                MatcherAction = matcherAction;
            }

            public void ReportAmbiguity(Parser recognizer, DFA dfa, int startIndex, int stopIndex, bool exact, BitSet ambigAlts, ATNConfigSet configs)
            {
            }

            public void ReportAttemptingFullContext(Parser recognizer, DFA dfa, int startIndex, int stopIndex, BitSet conflictingAlts, SimulatorState conflictState)
            {
            }

            public void ReportContextSensitivity(Parser recognizer, DFA dfa, int startIndex, int stopIndex, int prediction, SimulatorState acceptState)
            {
            }

            public void SyntaxError(IRecognizer recognizer, T offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
            {
                LOG.Error("Syntax error");
                LOG.Error(string.Format("Source : {0}", MatcherAction.matchExpression));
                LOG.Error(string.Format("Message: {0}", msg));
                throw new InvalidParserConfigurationException("Syntax error \"" + msg + "\" caused by \"" + MatcherAction.matchExpression + "\".");
            }
        }

        internal void Init(string newMatchExpression, Matcher newMatcher)
        {
            matcher = newMatcher;
            matchExpression = newMatchExpression;
            SetVerbose(newMatcher.GetVerbose());
        }

        public void Initialize()
        {
            InitErrorListener<int> lexerErrorListener = new InitErrorListener<int>(this);
            AntlrInputStream input = new AntlrInputStream(matchExpression);
            UserAgentTreeWalkerLexer lexer = new UserAgentTreeWalkerLexer(input);

            lexer.AddErrorListener(lexerErrorListener);

            InitErrorListener<IToken> parserErrorListener = new InitErrorListener<IToken>(this);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            UserAgentTreeWalkerParser parser = new UserAgentTreeWalkerParser(tokens);

            parser.AddErrorListener(parserErrorListener);

            //        parser.setTrace(true);
            ParserRuleContext requiredPattern = ParseWalkerExpression(parser);

            if (requiredPattern == null)
            {
                throw new InvalidParserConfigurationException("NO pattern ?!?!?");
            }

            // We couldn't ditch the double quotes around the fixed values in the parsing phase.
            // So we ditch them here. We simply walk the tree and modify some of the tokens.
            new UnQuoteValues().Visit(requiredPattern);

            // Now we create an evaluator instance
            evaluator = new TreeExpressionEvaluator(requiredPattern, matcher, verbose);

            // Is a fixed value (i.e. no events will ever be fired)?
            string fixedValue = evaluator.GetFixedValue();
            if (fixedValue != null)
            {
                SetFixedValue(fixedValue);
                MustHaveMatches = false;
                matches = new MatchesList(0);
                return; // Not interested in any patterns
            }

            MustHaveMatches = !evaluator.UsesIsNull();

            int informs = CalculateInformPath("agent", requiredPattern);

            // If this is based on a variable we do not need any matches from the hashmap.
            if (MustHaveMatches && informs == 0)
            {
                MustHaveMatches = false;
            }

            int listSize = 0;
            if (informs > 0)
            {
                listSize = 1;
            }
            matches = new MatchesList(listSize);
        }

        protected abstract ParserRuleContext ParseWalkerExpression(UserAgentTreeWalkerParser parser);

        private class UnQuoteValues : UserAgentTreeWalkerBaseVisitor<object>
        {
            private void UnQuoteToken(IToken token)
            {
                if (token is CommonToken commonToken)
                {
                    commonToken.StartIndex = commonToken.StartIndex + 1;
                    commonToken.StopIndex = commonToken.StopIndex - 1;
                }
            }

            public override object VisitMatcherPathLookup([NotNull] UserAgentTreeWalkerParser.MatcherPathLookupContext context)
            {
                UnQuoteToken(context.defaultValue);
                return base.VisitMatcherPathLookup(context);
            }

            public override object VisitPathFixedValue([NotNull] UserAgentTreeWalkerParser.PathFixedValueContext context)
            {
                UnQuoteToken(context.value);
                return base.VisitPathFixedValue(context);
            }

            public override object VisitMatcherConcat([NotNull] UserAgentTreeWalkerParser.MatcherConcatContext context)
            {
                UnQuoteToken(context.prefix);
                UnQuoteToken(context.postfix);
                return base.VisitMatcherConcat(context);
            }

            public override object VisitMatcherConcatPrefix([NotNull] UserAgentTreeWalkerParser.MatcherConcatPrefixContext context)
            {
                UnQuoteToken(context.prefix);
                return base.VisitMatcherConcatPrefix(context);
            }

            public override object VisitMatcherConcatPostfix([NotNull] UserAgentTreeWalkerParser.MatcherConcatPostfixContext context)
            {
                UnQuoteToken(context.postfix);
                return base.VisitMatcherConcatPostfix(context);
            }

            public override object VisitStepEqualsValue([NotNull] UserAgentTreeWalkerParser.StepEqualsValueContext context)
            {
                UnQuoteToken(context.value);
                return base.VisitStepEqualsValue(context);
            }

            public override object VisitStepNotEqualsValue([NotNull] UserAgentTreeWalkerParser.StepNotEqualsValueContext context)
            {
                UnQuoteToken(context.value);
                return base.VisitStepNotEqualsValue(context);
            }

            public override object VisitStepStartsWithValue([NotNull] UserAgentTreeWalkerParser.StepStartsWithValueContext context)
            {
                UnQuoteToken(context.value);
                return base.VisitStepStartsWithValue(context);
            }

            public override object VisitStepEndsWithValue([NotNull] UserAgentTreeWalkerParser.StepEndsWithValueContext context)
            {
                UnQuoteToken(context.value);
                return base.VisitStepEndsWithValue(context);
            }

            public override object VisitStepContainsValue([NotNull] UserAgentTreeWalkerParser.StepContainsValueContext context)
            {
                UnQuoteToken(context.value);
                return base.VisitStepContainsValue(context);
            }
        }

        protected abstract void SetFixedValue(string newFixedValue);

        /// <summary>
        /// For each key that this action wants to be notified for this method is called.
        /// Note that on a single parse event the same name CAN be called multiple times!!
        /// </summary>
        /// <param name="key">The key of the node</param>
        /// <param name="value"></param>
        /// <param name="result"> The node in the parser tree where the match occurred</param>
        public void Inform(string key, string value, IParseTree result)
        {
            // Only if this needs input we tell the matcher on the first one.
            if (MustHaveMatches && matches.Count == 0)
            {
                matcher.GotMyFirstStartingPoint();
            }
            matches.Add(key, value, result);
        }

        public abstract void Inform(string key, WalkList.WalkResult foundValue);

        /// <summary>
        /// If it is impossible that this can be valid it returns true, else false.
        /// </summary>
        /// <returns></returns>
        internal bool CannotBeValid()
        {
            return MustHaveMatches && matches.Count == 0;
        }

        /// <summary>
        /// Called after all nodes have been notified.
        /// </summary>
        /// <returns>true if the obtainResult result was valid. False will fail the entire matcher this belongs to.</returns>
        public abstract bool ObtainResult();

        internal bool IsValidIsNull()
        {
            return matches.Count == 0 && evaluator.UsesIsNull();
        }

        /// <summary>
        /// Optimization: Only if there is a possibility that all actions for this matcher CAN be valid do we
        /// actually perform the analysis and do the(expensive) tree walking and matching.
        /// </summary>
        internal void ProcessInformedMatches()
        {
            foreach (MatchesList.Match match in matches)
            {
                WalkList.WalkResult matchedValue = evaluator.Evaluate(match.GetResult(), match.GetKey(), match.GetValue());
                if (matchedValue != null)
                {
                    Inform(match.GetKey(), matchedValue);
                    break; // We always stick to the first match
                }
            }
        }

        private int CalculateInformPath(string treeName, ParserRuleContext tree)
        {
            if (tree is UserAgentTreeWalkerParser.MatcherRequireContext) {
                return CalculateInformPath(treeName, ((UserAgentTreeWalkerParser.MatcherRequireContext) tree));
             }
            if (tree is UserAgentTreeWalkerParser.MatcherContext) {
                return CalculateInformPath(treeName, ((UserAgentTreeWalkerParser.MatcherContext) tree));
            }
            return 0;
        }

        private int CalculateInformPath(string treeName, UserAgentTreeWalkerParser.MatcherRequireContext tree)
        {
            if (tree is UserAgentTreeWalkerParser.MatcherBaseContext) {
                return CalculateInformPath(treeName, ((UserAgentTreeWalkerParser.MatcherBaseContext)tree).matcher());
            }
            if (tree is UserAgentTreeWalkerParser.MatcherPathIsNullContext)
            {
                return CalculateInformPath(treeName, ((UserAgentTreeWalkerParser.MatcherPathIsNullContext)tree).matcher());
            }
            return 0;
        }

        private int CalculateInformPath(string treeName, UserAgentTreeWalkerParser.MatcherContext tree)
        {
            if (tree is UserAgentTreeWalkerParser.MatcherPathContext) {
                return CalculateInformPath(treeName, ((UserAgentTreeWalkerParser.MatcherPathContext)tree).basePath());
            }
            if (tree is UserAgentTreeWalkerParser.MatcherCleanVersionContext)
            {
                return CalculateInformPath(treeName, ((UserAgentTreeWalkerParser.MatcherCleanVersionContext)tree).matcher());
            }
            if (tree is UserAgentTreeWalkerParser.MatcherNormalizeBrandContext)
            {
                return CalculateInformPath(treeName, ((UserAgentTreeWalkerParser.MatcherNormalizeBrandContext)tree).matcher());
            }
            if (tree is UserAgentTreeWalkerParser.MatcherPathLookupContext)
            {
                return CalculateInformPath(treeName, ((UserAgentTreeWalkerParser.MatcherPathLookupContext)tree).matcher());
            }
            if (tree is UserAgentTreeWalkerParser.MatcherWordRangeContext)
            {
                return CalculateInformPath(treeName, ((UserAgentTreeWalkerParser.MatcherWordRangeContext)tree).matcher());
            }
            if (tree is UserAgentTreeWalkerParser.MatcherConcatContext)
            {
                return CalculateInformPath(treeName, ((UserAgentTreeWalkerParser.MatcherConcatContext)tree).matcher());
            }
            if (tree is UserAgentTreeWalkerParser.MatcherConcatPrefixContext)
            {
                return CalculateInformPath(treeName, ((UserAgentTreeWalkerParser.MatcherConcatPrefixContext)tree).matcher());
            }
            if (tree is UserAgentTreeWalkerParser.MatcherConcatPostfixContext)
            {
                return CalculateInformPath(treeName, ((UserAgentTreeWalkerParser.MatcherConcatPostfixContext)tree).matcher());
            }
            return 0;
        }

        private int CalculateInformPath(string treeName, UserAgentTreeWalkerParser.BasePathContext tree)
        {
            // Useless to register a fixed value
            //             case "PathFixedValueContext"         : calculateInformPath(treeName, (PathFixedValueContext)         tree); break;
            if (tree is UserAgentTreeWalkerParser.PathVariableContext) {
                matcher.InformMeAboutVariable(this, ((UserAgentTreeWalkerParser.PathVariableContext)tree).variable.Text);
                return 0;
            }
            if (tree is UserAgentTreeWalkerParser.PathWalkContext) {
                return CalculateInformPath(treeName, ((UserAgentTreeWalkerParser.PathWalkContext)tree).nextStep);
            }
            return 0;
        }

        private int CalculateInformPath(string treeName, UserAgentTreeWalkerParser.PathContext tree)
        {
            if (tree != null)
            {
                if (tree is UserAgentTreeWalkerParser.StepDownContext)
                {
                    return CalculateInformPath(treeName, (UserAgentTreeWalkerParser.StepDownContext)tree);
                }
                if (tree is UserAgentTreeWalkerParser.StepEqualsValueContext)
                {
                    return CalculateInformPath(treeName, (UserAgentTreeWalkerParser.StepEqualsValueContext)tree);
                }
                if (tree is UserAgentTreeWalkerParser.StepStartsWithValueContext)
                {
                    return CalculateInformPath(treeName, (UserAgentTreeWalkerParser.StepStartsWithValueContext)tree);
                }
                if (tree is UserAgentTreeWalkerParser.StepWordRangeContext) {
                    return CalculateInformPath(treeName, (UserAgentTreeWalkerParser.StepWordRangeContext)tree);
                }
            }
            matcher.InformMeAbout(this, treeName);
            return 1;
        }

        private int CalculateInformPath(string treeName, UserAgentTreeWalkerParser.StepDownContext tree)
        {
            if (treeName.Length == 0)
            {
                return CalculateInformPath(treeName + '.' + tree.name.Text, tree.nextStep);
            }

            int informs = 0;
            foreach (int? number in NumberRangeVisitor.NUMBER_RANGE_VISITOR.Visit(tree.numberRange()))
            {
                informs += CalculateInformPath(treeName + '.' + "(" + number + ")" + tree.name.Text, tree.nextStep);
            }
            return informs;
        }

        private int CalculateInformPath(string treeName, UserAgentTreeWalkerParser.StepEqualsValueContext tree)
        {
            matcher.InformMeAbout(this, treeName + "=\"" + tree.value.Text + "\"");
            return 1;
        }

        private int CalculateInformPath(string treeName, UserAgentTreeWalkerParser.StepStartsWithValueContext tree)
        {
            matcher.InformMeAboutPrefix(this, treeName, tree.value.Text);
            return 1;
        }

        private int CalculateInformPath(string treeName, UserAgentTreeWalkerParser.StepWordRangeContext tree)
        {
            WordRangeVisitor.Range range = WordRangeVisitor.GetRange(tree.wordRange());
            matcher.LookingForRange(treeName, range);
            return CalculateInformPath(treeName + range, tree.nextStep);
        }

        public virtual void Reset()
        {
            matches.Clear();
            if (verboseTemporary)
            {
                verbose = verbosePermanent;
            }
        }

        public MatchesList GetMatches()
        {
            return matches;
        }
    }
}