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
using Antlr4.Runtime;

namespace OrbintSoft.Yauaa.Analyze
{
    using Antlr4.Runtime.Atn;
    using Antlr4.Runtime.Dfa;
    using Antlr4.Runtime.Misc;
    using Antlr4.Runtime.Sharpen;
    using Antlr4.Runtime.Tree;
    using log4net;
    using OrbintSoft.Yauaa.Analyze.TreeWalker;
    using OrbintSoft.Yauaa.Analyze.TreeWalker.Steps;
    using OrbintSoft.Yauaa.Antlr4Source;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the <see cref="MatcherAction" />
    /// </summary>
    [Serializable]
    public abstract class MatcherAction
    {
        /// <summary>
        /// Defines the verbose
        /// </summary>
        internal bool verbose = false;

        /// <summary>
        /// Defines the evaluator
        /// </summary>
        protected TreeExpressionEvaluator evaluator = null;

        /// <summary>
        /// Defines the Log
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(MatcherAction));

        private static readonly IDictionary<Type, CalculateInformPathFunction> CALCULATE_INFORM_PATH = new Dictionary<Type, CalculateInformPathFunction>();

        /// <summary>
        /// Defines the matchExpression
        /// </summary>
        private string matchExpression = null;


        /// <summary>
        /// Defines the matcher
        /// </summary>
        private Matcher matcher = null;

        /// <summary>
        /// Defines the matches
        /// </summary>
        private MatchesList matches = null;

        /// <summary>
        /// Defines the verbosePermanent
        /// </summary>
        private bool verbosePermanent = false;

        /// <summary>
        /// Defines the verboseTemporary
        /// </summary>
        private bool verboseTemporary = false;

        /// <summary>
        /// Gets a value indicating whether MustHaveMatches
        /// </summary>
        internal bool MustHaveMatches { get; private set; } = false;

        static MatcherAction()
        {
            // -------------
            CALCULATE_INFORM_PATH[typeof(UserAgentTreeWalkerParser.MatcherBaseContext)] = 
                (action, treeName, tree) => CalculateInformPath(action, treeName, ((UserAgentTreeWalkerParser.MatcherBaseContext) tree).matcher());
            CALCULATE_INFORM_PATH[typeof(UserAgentTreeWalkerParser.MatcherPathIsNullContext)] = 
                (action, treeName, tree) => CalculateInformPath(action, treeName, ((UserAgentTreeWalkerParser.MatcherPathIsNullContext) tree).matcher());

            // -------------
            CALCULATE_INFORM_PATH[typeof(UserAgentTreeWalkerParser.MatcherExtractContext)] = 
                (action, treeName, tree) => CalculateInformPath(action, treeName, ((UserAgentTreeWalkerParser.MatcherExtractContext)tree).expression);

            // -------------
            CALCULATE_INFORM_PATH[typeof(UserAgentTreeWalkerParser.MatcherVariableContext)] =
                (action, treeName, tree) => CalculateInformPath(action, treeName, ((UserAgentTreeWalkerParser.MatcherVariableContext)tree).expression);

            // -------------
            CALCULATE_INFORM_PATH[typeof(UserAgentTreeWalkerParser.MatcherPathContext)] = (action, treeName, tree) => CalculateInformPath(action, treeName, ((UserAgentTreeWalkerParser.MatcherPathContext) tree).basePath());
            CALCULATE_INFORM_PATH[typeof(UserAgentTreeWalkerParser.MatcherConcatContext)] = (action, treeName, tree) => CalculateInformPath(action, treeName, ((UserAgentTreeWalkerParser.MatcherConcatContext) tree).matcher());
            CALCULATE_INFORM_PATH[typeof(UserAgentTreeWalkerParser.MatcherConcatPrefixContext)] = (action, treeName, tree) => CalculateInformPath(action, treeName, ((UserAgentTreeWalkerParser.MatcherConcatPrefixContext) tree).matcher());
            CALCULATE_INFORM_PATH[typeof(UserAgentTreeWalkerParser.MatcherConcatPostfixContext)] = (action, treeName, tree) => CalculateInformPath(action, treeName, ((UserAgentTreeWalkerParser.MatcherConcatPostfixContext) tree).matcher());
            CALCULATE_INFORM_PATH[typeof(UserAgentTreeWalkerParser.MatcherNormalizeBrandContext)] = (action, treeName, tree) => CalculateInformPath(action, treeName, ((UserAgentTreeWalkerParser.MatcherNormalizeBrandContext) tree).matcher());
            CALCULATE_INFORM_PATH[typeof(UserAgentTreeWalkerParser.MatcherCleanVersionContext)] = (action, treeName, tree) => CalculateInformPath(action, treeName, ((UserAgentTreeWalkerParser.MatcherCleanVersionContext) tree).matcher());
            CALCULATE_INFORM_PATH[typeof(UserAgentTreeWalkerParser.MatcherPathLookupContext)] = (action, treeName, tree) => CalculateInformPath(action, treeName, ((UserAgentTreeWalkerParser.MatcherPathLookupContext) tree).matcher());
            CALCULATE_INFORM_PATH[typeof(UserAgentTreeWalkerParser.MatcherWordRangeContext)] = (action, treeName, tree) => CalculateInformPath(action, treeName, ((UserAgentTreeWalkerParser.MatcherWordRangeContext) tree).matcher());

            // -------------
            CALCULATE_INFORM_PATH[typeof(UserAgentTreeWalkerParser.PathVariableContext)] = (action, treeName, tree) =>
            {
                action.matcher.InformMeAboutVariable(action, ((UserAgentTreeWalkerParser.PathVariableContext) tree).variable.Text);
                return 0;
            };

            CALCULATE_INFORM_PATH[typeof(UserAgentTreeWalkerParser.PathWalkContext)] = (action, treeName, tree) => CalculateInformPath(action, treeName, ((UserAgentTreeWalkerParser.PathWalkContext) tree).nextStep);
		   
            // -------------
            CALCULATE_INFORM_PATH[typeof(UserAgentTreeWalkerParser.StepDownContext)] = (action, treeName, tree) => 
            {
                UserAgentTreeWalkerParser.StepDownContext thisTree = ((UserAgentTreeWalkerParser.StepDownContext)tree);
                int informs = 0;
                foreach (int number in NumberRangeVisitor.Instance.Visit(thisTree.numberRange())) 
                {
                    informs += CalculateInformPath(action, treeName + '.' + "(" + number + ")" + thisTree.name.Text, thisTree.nextStep);
                }
                return informs;
            };

            CALCULATE_INFORM_PATH[typeof(UserAgentTreeWalkerParser.StepEqualsValueContext)] = (action, treeName, tree) => 
            {
                UserAgentTreeWalkerParser.StepEqualsValueContext thisTree = ((UserAgentTreeWalkerParser.StepEqualsValueContext)tree);
                action.matcher.InformMeAbout(action, treeName + "=\"" + thisTree.value.Text + "\"");
                return 1;
            };

            CALCULATE_INFORM_PATH[typeof(UserAgentTreeWalkerParser.StepStartsWithValueContext)] = (action, treeName, tree) => 
            {
                UserAgentTreeWalkerParser.StepStartsWithValueContext thisTree = ((UserAgentTreeWalkerParser.StepStartsWithValueContext)tree);
                action.matcher.InformMeAboutPrefix(action, treeName, thisTree.value.Text);
                return 1;
            };

            CALCULATE_INFORM_PATH[typeof(UserAgentTreeWalkerParser.StepWordRangeContext)] = (action, treeName, tree) => 
            {
                UserAgentTreeWalkerParser.StepWordRangeContext thisTree = ((UserAgentTreeWalkerParser.StepWordRangeContext)tree);
                WordRangeVisitor.Range range = WordRangeVisitor.GetRange(thisTree.wordRange());
                action.matcher.LookingForRange(treeName, range);
											  
                return CalculateInformPath(action, treeName + range, thisTree.nextStep);
            };
        }


        delegate int CalculateInformPathFunction(MatcherAction action, string treeName, ParserRuleContext tree);

        /// <summary>
        /// The GetEvaluatorForUnitTesting
        /// </summary>
        /// <returns>The <see cref="TreeExpressionEvaluator"/></returns>
        internal TreeExpressionEvaluator EvaluatorForUnitTesting
        {
            get => evaluator;
        }

        /// <summary>
        /// The SetVerbose
        /// </summary>
        /// <param name="newVerbose">The newVerbose<see cref="bool"/></param>
        private void SetVerbose(bool newVerbose)
        {
            SetVerbose(newVerbose, false);
        }

        /// <summary>
        /// The SetVerbose
        /// </summary>
        /// <param name="newVerbose">The newVerbose<see cref="bool"/></param>
        /// <param name="temporary">The temporary<see cref="bool"/></param>
        public void SetVerbose(bool newVerbose, bool temporary)
        {
            verbose = newVerbose;
            if (!temporary)
            {
                verbosePermanent = newVerbose;
            }
            verboseTemporary = temporary;
        }

        /// <summary>
        /// The GetMatchExpression
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public string GetMatchExpression()
        {
            return matchExpression;
        }

        /// <summary>
        /// The Initialize
        /// </summary>
        public virtual void Initialize()
        {
            InitErrorListener<int> lexerErrorListener = new InitErrorListener<int>(this);
            AntlrInputStream input = new AntlrInputStream(matchExpression);

            UserAgentTreeWalkerLexer lexer = new UserAgentTreeWalkerLexer(input);

            lexer.AddErrorListener(lexerErrorListener);

            InitErrorListener<IToken> parserErrorListener = new InitErrorListener<IToken>(this);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            UserAgentTreeWalkerParser parser = new UserAgentTreeWalkerParser(tokens);

            parser.AddErrorListener(parserErrorListener);

            ParserRuleContext requiredPattern = ParseWalkerExpression(parser);

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

            int informs = CalculateInformPath(this, "agent", requiredPattern);

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

        /// <summary>
        /// The Reset
        /// </summary>
        public virtual void Reset()
        {
            matches.Clear();
            if (verboseTemporary)
            {
                verbose = verbosePermanent;
            }
        }

        /// <summary>
        /// The GetMatches
        /// </summary>
        /// <returns>The <see cref="MatchesList"/></returns>
        public MatchesList GetMatches()
        {
            return matches;
        }

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

        /// <summary>
        /// The Inform
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="foundValue">The foundValue<see cref="WalkList.WalkResult"/></param>
        public abstract void Inform(string key, WalkList.WalkResult foundValue);

        /// <summary>
        /// Called after all nodes have been notified.
        /// </summary>
        /// <returns>true if the obtainResult result was valid. False will fail the entire matcher this belongs to.</returns>
        public abstract bool ObtainResult();

        /// <summary>
        /// The IsValidIsNull
        /// </summary>
        /// <returns>The <see cref="bool"/></returns>
        internal bool IsValidIsNull()
        {
            return matches.Count == 0 && evaluator.UsesIsNull();
        }

        /// <summary>
        /// If it is impossible that this can be valid it returns true, else false.
        /// </summary>
        /// <returns></returns>
        internal bool CannotBeValid()
        {
            return MustHaveMatches && matches.Count == 0;
        }

        /// <summary>
        /// Optimization: Only if there is a possibility that all actions for this matcher CAN be valid do we
        /// actually perform the analysis and do the(expensive) tree walking and matching.
        /// </summary>
        internal void ProcessInformedMatches()
        {
            foreach (MatchesList.Match match in matches)
            {
                WalkList.WalkResult matchedValue = evaluator.Evaluate(match.GetResult(), match.Key, match.Value);
                if (matchedValue != null)
                {
                    Inform(match.Key, matchedValue);
                    break; // We always stick to the first match
                }
            }
        }

        /// <summary>
        /// The Init
        /// </summary>
        /// <param name="newMatchExpression">The newMatchExpression<see cref="string"/></param>
        /// <param name="newMatcher">The newMatcher<see cref="Matcher"/></param>
        internal void Init(string newMatchExpression, Matcher newMatcher)
        {
            matcher = newMatcher;
            matchExpression = newMatchExpression;
            SetVerbose(newMatcher.GetVerbose());
        }

        /// <summary>
        /// The ParseWalkerExpression
        /// </summary>
        /// <param name="parser">The parser<see cref="UserAgentTreeWalkerParser"/></param>
        /// <returns>The <see cref="ParserRuleContext"/></returns>
        protected abstract ParserRuleContext ParseWalkerExpression(UserAgentTreeWalkerParser parser);

        /// <summary>
        /// The SetFixedValue
        /// </summary>
        /// <param name="newFixedValue">The newFixedValue<see cref="string"/></param>
        protected abstract void SetFixedValue(string newFixedValue);

        /// <summary>
        /// The CalculateInformPath
        /// </summary>
        /// <param name="treeName">The treeName<see cref="string"/></param>
        /// <param name="tree">The tree<see cref="ParserRuleContext"/></param>
        /// <returns>The <see cref="int"/></returns>
        private static int CalculateInformPath(MatcherAction action, string treeName, ParserRuleContext tree)
        {
            if (tree == null)
            {
                action.matcher.InformMeAbout(action, treeName);
                return 1;
            }

            Type type = tree.GetType();
            if (CALCULATE_INFORM_PATH.ContainsKey(type))
            {
                return CALCULATE_INFORM_PATH[type].Invoke(action, treeName, tree);
            }

            action.matcher.InformMeAbout(action, treeName);
            return 1;
        }

        
        /// <summary>
        /// Defines the <see cref="InitErrorListener{T}" />
        /// </summary>
        /// <typeparam name="T"></typeparam>
        internal class InitErrorListener<T> : IAntlrErrorListener<T>
        {
            /// <summary>
            /// Gets or sets the MatcherAction
            /// </summary>
            public MatcherAction MatcherAction { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="InitErrorListener{T}"/> class.
            /// </summary>
            /// <param name="matcherAction">The matcherAction<see cref="MatcherAction"/></param>
            public InitErrorListener(MatcherAction matcherAction) : base()
            {
                MatcherAction = matcherAction;
            }

            /// <summary>
            /// The ReportAmbiguity
            /// </summary>
            /// <param name="recognizer">The recognizer<see cref="Parser"/></param>
            /// <param name="dfa">The dfa<see cref="DFA"/></param>
            /// <param name="startIndex">The startIndex<see cref="int"/></param>
            /// <param name="stopIndex">The stopIndex<see cref="int"/></param>
            /// <param name="exact">The exact<see cref="bool"/></param>
            /// <param name="ambigAlts">The ambigAlts<see cref="BitSet"/></param>
            /// <param name="configs">The configs<see cref="ATNConfigSet"/></param>
            public void ReportAmbiguity(Parser recognizer, DFA dfa, int startIndex, int stopIndex, bool exact, BitSet ambigAlts, ATNConfigSet configs)
            {
            }

            /// <summary>
            /// The ReportAttemptingFullContext
            /// </summary>
            /// <param name="recognizer">The recognizer<see cref="Parser"/></param>
            /// <param name="dfa">The dfa<see cref="DFA"/></param>
            /// <param name="startIndex">The startIndex<see cref="int"/></param>
            /// <param name="stopIndex">The stopIndex<see cref="int"/></param>
            /// <param name="conflictingAlts">The conflictingAlts<see cref="BitSet"/></param>
            /// <param name="conflictState">The conflictState<see cref="SimulatorState"/></param>
            public void ReportAttemptingFullContext(Parser recognizer, DFA dfa, int startIndex, int stopIndex, BitSet conflictingAlts, SimulatorState conflictState)
            {
            }

            /// <summary>
            /// The ReportContextSensitivity
            /// </summary>
            /// <param name="recognizer">The recognizer<see cref="Parser"/></param>
            /// <param name="dfa">The dfa<see cref="DFA"/></param>
            /// <param name="startIndex">The startIndex<see cref="int"/></param>
            /// <param name="stopIndex">The stopIndex<see cref="int"/></param>
            /// <param name="prediction">The prediction<see cref="int"/></param>
            /// <param name="acceptState">The acceptState<see cref="SimulatorState"/></param>
            public void ReportContextSensitivity(Parser recognizer, DFA dfa, int startIndex, int stopIndex, int prediction, SimulatorState acceptState)
            {
            }

            /// <summary>
            /// The SyntaxError
            /// </summary>
            /// <param name="recognizer">The recognizer<see cref="IRecognizer"/></param>
            /// <param name="offendingSymbol">The offendingSymbol<see cref="T"/></param>
            /// <param name="line">The line<see cref="int"/></param>
            /// <param name="charPositionInLine">The charPositionInLine<see cref="int"/></param>
            /// <param name="msg">The msg<see cref="string"/></param>
            /// <param name="e">The e<see cref="RecognitionException"/></param>
            public void SyntaxError(IRecognizer recognizer, T offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
            {
                Log.Error("Syntax error");
                Log.Error(string.Format("Source : {0}", MatcherAction.matchExpression));
                Log.Error(string.Format("Message: {0}", msg));
                throw new InvalidParserConfigurationException("Syntax error \"" + msg + "\" caused by \"" + MatcherAction.matchExpression + "\".");
            }
        }

        /// <summary>
        /// Defines the <see cref="UnQuoteValues" />
        /// </summary>
        private class UnQuoteValues : UserAgentTreeWalkerBaseVisitor<object>
        {
            /// <summary>
            /// The UnQuoteToken
            /// </summary>
            /// <param name="token">The token<see cref="IToken"/></param>
            private void UnQuoteToken(IToken token)
            {
                if (token is CommonToken commonToken)
                {
                    commonToken.StartIndex = commonToken.StartIndex + 1;
                    commonToken.StopIndex = commonToken.StopIndex - 1;
                }
            }

            /// <summary>
            /// The VisitMatcherPathLookup
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherPathLookupContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitMatcherPathLookup([NotNull] UserAgentTreeWalkerParser.MatcherPathLookupContext context)
            {
                UnQuoteToken(context.defaultValue);
                return base.VisitMatcherPathLookup(context);
            }

            /// <summary>
            /// The VisitPathFixedValue
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.PathFixedValueContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitPathFixedValue([NotNull] UserAgentTreeWalkerParser.PathFixedValueContext context)
            {
                UnQuoteToken(context.value);
                return base.VisitPathFixedValue(context);
            }

            /// <summary>
            /// The VisitMatcherConcat
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherConcatContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitMatcherConcat([NotNull] UserAgentTreeWalkerParser.MatcherConcatContext context)
            {
                UnQuoteToken(context.prefix);
                UnQuoteToken(context.postfix);
                return base.VisitMatcherConcat(context);
            }

            /// <summary>
            /// The VisitMatcherConcatPrefix
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherConcatPrefixContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitMatcherConcatPrefix([NotNull] UserAgentTreeWalkerParser.MatcherConcatPrefixContext context)
            {
                UnQuoteToken(context.prefix);
                return base.VisitMatcherConcatPrefix(context);
            }

            /// <summary>
            /// The VisitMatcherConcatPostfix
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.MatcherConcatPostfixContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitMatcherConcatPostfix([NotNull] UserAgentTreeWalkerParser.MatcherConcatPostfixContext context)
            {
                UnQuoteToken(context.postfix);
                return base.VisitMatcherConcatPostfix(context);
            }

            /// <summary>
            /// The VisitStepEqualsValue
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepEqualsValueContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepEqualsValue([NotNull] UserAgentTreeWalkerParser.StepEqualsValueContext context)
            {
                UnQuoteToken(context.value);
                return base.VisitStepEqualsValue(context);
            }

            /// <summary>
            /// The VisitStepNotEqualsValue
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepNotEqualsValueContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepNotEqualsValue([NotNull] UserAgentTreeWalkerParser.StepNotEqualsValueContext context)
            {
                UnQuoteToken(context.value);
                return base.VisitStepNotEqualsValue(context);
            }

            /// <summary>
            /// The VisitStepStartsWithValue
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepStartsWithValueContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepStartsWithValue([NotNull] UserAgentTreeWalkerParser.StepStartsWithValueContext context)
            {
                UnQuoteToken(context.value);
                return base.VisitStepStartsWithValue(context);
            }

            /// <summary>
            /// The VisitStepEndsWithValue
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepEndsWithValueContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepEndsWithValue([NotNull] UserAgentTreeWalkerParser.StepEndsWithValueContext context)
            {
                UnQuoteToken(context.value);
                return base.VisitStepEndsWithValue(context);
            }

            /// <summary>
            /// The VisitStepContainsValue
            /// </summary>
            /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.StepContainsValueContext"/></param>
            /// <returns>The <see cref="object"/></returns>
            public override object VisitStepContainsValue([NotNull] UserAgentTreeWalkerParser.StepContainsValueContext context)
            {
                UnQuoteToken(context.value);
                return base.VisitStepContainsValue(context);
            }
        }
    }
}
