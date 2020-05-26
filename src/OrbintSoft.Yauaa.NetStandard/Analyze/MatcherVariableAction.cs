//-----------------------------------------------------------------------
// <copyright file="MatcherVariableAction.cs" company="OrbintSoft">
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

namespace OrbintSoft.Yauaa.Analyze
{
    using System;
    using System.Collections.Generic;
    using Antlr4.Runtime;
    using OrbintSoft.Yauaa.Analyze.TreeWalker.Steps;
    using OrbintSoft.Yauaa.Antlr4Source;
    using OrbintSoft.Yauaa.Logger;

    /// <summary>
    /// This class is used to define a variable value action for a matcher.
    /// </summary>
    [Serializable]
    public class MatcherVariableAction : MatcherAction
    {
        /// <summary>
        /// Defines the logger.
        /// </summary>
        private static readonly ILogger Logger = new Logger<MatcherVariableAction>();

        /// <summary>
        /// Defines the expression.
        /// </summary>
        private readonly string expression;

        /// <summary>
        /// Defines the found value.
        /// </summary>
        [NonSerialized]
        private WalkList.WalkResult foundValue = null;

        /// <summary>
        /// Defines the interested actions set.
        /// </summary>
        private ISet<MatcherAction> interestedActions = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatcherVariableAction"/> class.
        /// </summary>
        /// <param name="variableName">The variable name.</param>
        /// <param name="config">The configuration.</param>
        /// <param name="matcher">The matcher.</param>
        public MatcherVariableAction(string variableName, string config, Matcher matcher)
        {
            this.VariableName = variableName;
            this.expression = config;
            this.Init(config, matcher);
        }

        /// <summary>
        /// Gets the variable name.
        /// </summary>
        public string VariableName { get; }

        /// <summary>
        /// Informs the mayìtcher about a new found value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="newlyFoundValue">The new found value.</param>
        public override void Inform(string key, WalkList.WalkResult newlyFoundValue)
        {
            if (this.Verbose)
            {
                Logger.Info($"INFO  : VARIABLE ({this.VariableName}): {key}");
                Logger.Info($"NEED  : VARIABLE ({this.VariableName}): {this.MatchExpression}");
            }

            /*
             * We know the tree is parsed from left to right.
             * This is also the priority in the fields.
             * So we always use the first value we find.
             */
            if (this.foundValue is null)
            {
                this.foundValue = newlyFoundValue;
                if (this.Verbose)
                {
                    Logger.Info($"KEPT  : VARIABLE ({this.VariableName}): {key}");
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
        /// Process the natches and find a value.
        /// </summary>
        /// <returns>True if found a value.</returns>
        public override bool ObtainResult()
        {
            this.ProcessInformedMatches();
            return this.foundValue != null;
        }

        /// <summary>
        /// Resets the matches.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            this.foundValue = null;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"VARIABLE: ({this.VariableName}): {this.expression}";
        }

        /// <summary>
        /// Sets the new interested actions.
        /// </summary>
        /// <param name="newInterestedActions">The new interested actions.</param>
        public void SetInterestedActions(ISet<MatcherAction> newInterestedActions)
        {
            this.interestedActions = newInterestedActions;
        }

        /// <summary>
        /// Execute the matcher variable parser.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <returns>The context.</returns>
        protected override ParserRuleContext ParseWalkerExpression(UserAgentTreeWalkerParser parser)
        {
            return parser.matcherVariable();
        }

        /// <summary>
        /// A fixed value cannot be set for a variable action.
        /// </summary>
        /// <param name="fixedValue">The fixed value.</param>
        protected override void SetFixedValue(string fixedValue)
        {
            throw new InvalidParserConfigurationException($"It is useless to put a fixed value \"{fixedValue}\" in the variable section.");
        }
    }
}
