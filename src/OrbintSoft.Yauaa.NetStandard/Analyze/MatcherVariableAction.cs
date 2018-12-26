//-----------------------------------------------------------------------
// <copyright file="MatcherVariableAction.cs" company="OrbintSoft">
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
    using Antlr4.Runtime;
    using log4net;
    using OrbintSoft.Yauaa.Analyze.TreeWalker.Steps;
    using OrbintSoft.Yauaa.Antlr4Source;

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
            this.VariableName = variableName;
            this.expression = config;
            this.Init(config, matcher);
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
            if (this.Verbose)
            {
                Log.Info(string.Format("INFO  : VARIABLE ({0}): {1}", this.VariableName, key));
                Log.Info(string.Format("NEED  : VARIABLE ({0}): {1}", this.VariableName, this.MatchExpression));
            }

            /*
             * We know the tree is parsed from left to right.
             * This is also the priority in the fields.
             * So we always use the first value we find.
             */
            if (this.foundValue == null)
            {
                this.foundValue = newlyFoundValue;
                if (this.Verbose)
                {
                    Log.Info(string.Format("KEPT  : VARIABLE ({0}): {1}", this.VariableName, key));
                }

                if (this.interestedActions != null && this.interestedActions.Count != 0)
                {
                    foreach (var action in this.interestedActions)
                    {
                        action.Inform(this.VariableName, newlyFoundValue.Value, newlyFoundValue.Tree);
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
            this.ProcessInformedMatches();
            return this.foundValue != null;
        }

        /// <summary>
        /// The Reset
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            this.foundValue = null;
        }

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            return "VARIABLE: (" + this.VariableName + "): " + this.expression;
        }

        /// <summary>
        /// The SetInterestedActions
        /// </summary>
        /// <param name="newInterestedActions">The newInterestedActions<see cref="ISet{MatcherAction}"/></param>
        public void SetInterestedActions(ISet<MatcherAction> newInterestedActions)
        {
            this.interestedActions = newInterestedActions;
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
