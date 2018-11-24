//<copyright file="MatcherVariableAction.cs" company="OrbintSoft">
//	Yet Another UserAgent Analyzer.NET Standard
//	Porting realized by Stefano Balzarotti, Copyright (C) OrbintSoft
//
//	Original Author and License:
//
//	Yet Another UserAgent Analyzer
//	Copyright(C) 2013-2018 Niels Basjes
//
//	Licensed under the Apache License, Version 2.0 (the "License");
//	you may not use this file except in compliance with the License.
//	You may obtain a copy of the License at
//
//	https://www.apache.org/licenses/LICENSE-2.0
//
//	Unless required by applicable law or agreed to in writing, software
//	distributed under the License is distributed on an "AS IS" BASIS,
//	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//	See the License for the specific language governing permissions and
//	limitations under the License.
//
//</copyright>
//<author>Stefano Balzarotti, Niels Basjes</author>
//<date>2018, 7, 26, 23:01</date>
//<summary></summary>

namespace OrbintSoft.Yauaa.Analyze
{
    using Antlr4.Runtime;
    using log4net;
    using OrbintSoft.Yauaa.Analyze.TreeWalker.Steps;
    using OrbintSoft.Yauaa.Antlr4Source;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the <see cref="MatcherVariableAction" />
    /// </summary>
    [Serializable]
    public class MatcherVariableAction : MatcherAction
    {
        /// <summary>
        /// Defines the Log
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(MatcherVariableAction));

        /// <summary>
        /// Defines the expression
        /// </summary>
        private readonly string expression;

        /// <summary>
        /// Defines the foundValue
        /// </summary>
        [NonSerialized]
        private WalkList.WalkResult foundValue = null;

        /// <summary>
        /// Defines the interestedActions
        /// </summary>
        private ISet<MatcherAction> interestedActions = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatcherVariableAction"/> class.
        /// </summary>
        /// <param name="variableName">The variableName<see cref="string"/></param>
        /// <param name="config">The config<see cref="string"/></param>
        /// <param name="matcher">The matcher<see cref="Matcher"/></param>
        public MatcherVariableAction(string variableName, string config, Matcher matcher)
        {
            VariableName = variableName;
            expression = config;
            Init(config, matcher);
        }

        /// <summary>
        /// Gets the VariableName
        /// </summary>
        public string VariableName { get; }

        /// <summary>
        /// The Inform
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="newlyFoundValue">The newlyFoundValue<see cref="WalkList.WalkResult"/></param>
        public override void Inform(string key, WalkList.WalkResult newlyFoundValue)
        {
            if (verbose)
            {
                Log.Info(string.Format("INFO  : VARIABLE ({0}): {1}", VariableName, key));
                Log.Info(string.Format("NEED  : VARIABLE ({0}): {1}", VariableName, GetMatchExpression()));
            }
            /*
             * We know the tree is parsed from left to right.
             * This is also the priority in the fields.
             * So we always use the first value we find.
             */
            if (foundValue == null)
            {
                foundValue = newlyFoundValue;
                if (verbose)
                {
                    Log.Info(string.Format("KEPT  : VARIABLE ({0}): {1}", VariableName, key));
                }

                if (interestedActions != null && interestedActions.Count != 0)
                {
                    foreach (MatcherAction action in interestedActions)
                    {
                        action.Inform(VariableName, newlyFoundValue.GetValue(), newlyFoundValue.GetTree());
                    }
                }
            }
        }

        /// <summary>
        /// The ObtainResult
        /// </summary>
        /// <returns>The <see cref="bool"/></returns>
        public override bool ObtainResult()
        {
            ProcessInformedMatches();
            return foundValue != null;
        }

        /// <summary>
        /// The Reset
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            foundValue = null;
        }

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            return "VARIABLE: (" + VariableName + "): " + expression;
        }

        /// <summary>
        /// The SetInterestedActions
        /// </summary>
        /// <param name="newInterestedActions">The newInterestedActions<see cref="ISet{MatcherAction}"/></param>
        public void SetInterestedActions(ISet<MatcherAction> newInterestedActions)
        {
            interestedActions = newInterestedActions;
        }

        /// <summary>
        /// The ParseWalkerExpression
        /// </summary>
        /// <param name="parser">The parser<see cref="UserAgentTreeWalkerParser"/></param>
        /// <returns>The <see cref="ParserRuleContext"/></returns>
        protected override ParserRuleContext ParseWalkerExpression(UserAgentTreeWalkerParser parser)
        {
            return parser.matcherVariable();
        }

        /// <summary>
        /// The SetFixedValue
        /// </summary>
        /// <param name="fixedValue">The fixedValue<see cref="string"/></param>
        protected override void SetFixedValue(string fixedValue)
        {
            throw new InvalidParserConfigurationException(
                "It is useless to put a fixed value \"" + fixedValue + "\" in the variable section.");
        }
    }
}
