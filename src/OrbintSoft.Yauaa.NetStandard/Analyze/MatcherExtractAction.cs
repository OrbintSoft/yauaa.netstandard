//-----------------------------------------------------------------------
// <copyright file="MatcherExtractAction.cs" company="OrbintSoft">
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
    using Antlr4.Runtime;
    using OrbintSoft.Yauaa.Analyze.TreeWalker.Steps;
    using OrbintSoft.Yauaa.Analyzer;
    using OrbintSoft.Yauaa.Antlr4Source;
    using OrbintSoft.Yauaa.Logger;

    /// <summary>
    /// This class is used to repesent the extract action associated to a matcher.
    /// </summary>
    [Serializable]
    public class MatcherExtractAction : MatcherAction
    {
        /// <summary>
        /// Defines the Loggger.
        /// </summary>
        private static readonly ILogger Logger = new Logger<MatcherExtractAction>();

        /// <summary>
        /// Defines the confidence value, higher is better.
        /// </summary>
        private readonly long confidence;

        /// <summary>
        /// Defines the parsing expression.
        /// </summary>
        private readonly string expression;

        /// <summary>
        /// Defines the fixed value to use.
        /// </summary>
        private string fixedValue = null;

        /// <summary>
        /// Defines the found value.
        /// </summary>
        private string foundValue = null;

        /// <summary>
        /// Defines the extracted user agent field.
        /// </summary>
        private IAgentField resultAgentField = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatcherExtractAction"/> class.
        /// </summary>
        /// <param name="attribute">The attribute<see cref="string"/>.</param>
        /// <param name="confidence">The confidence<see cref="long"/>.</param>
        /// <param name="config">The config<see cref="string"/>.</param>
        /// <param name="matcher">The matcher<see cref="Matcher"/>.</param>
        public MatcherExtractAction(string attribute, long confidence, string config, Matcher matcher)
        {
            this.Attribute = attribute;
            this.confidence = confidence;
            this.expression = config;
            this.Init(config, matcher);
        }

        /// <summary>
        /// Gets the name of the attribute.
        /// </summary>
        public string Attribute { get; }

        /// <summary>
        /// Informs when a new value is found.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="newlyFoundValue">The found value.</param>
        public override void Inform(string key, WalkList.WalkResult newlyFoundValue)
        {
            if (this.Verbose)
            {
                Logger.Info($"INFO  : EXTRACT ({this.Attribute}): {key}");
                Logger.Info($"NEED  : EXTRACT ({this.Attribute}): {this.MatchExpression}");
            }

            /*
             * We know the tree is parsed from left to right.
             * This is also the priority in the fields.
             * So we always use the first value we find.
             */
            if (this.foundValue is null)
            {
                this.foundValue = newlyFoundValue.Value;
                if (this.Verbose)
                {
                    Logger.Info($"KEPT  : EXTRACT ({this.Attribute}): {key}");
                }
            }
        }

        /// <summary>
        /// True if a fixed value is defined.
        /// </summary>
        /// <returns>True if fixed.</returns>
        public bool IsFixedValue()
        {
            return this.fixedValue != null;
        }

        /// <summary>
        /// Extracts the result.
        /// </summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool ObtainResult()
        {
            this.ProcessInformedMatches();
            if (this.fixedValue != null)
            {
                if (this.Verbose)
                {
                    Logger.Info($"Set fixedvalue ({this.Attribute})[{this.confidence}]: {this.fixedValue}");
                }

                this.resultAgentField.SetValueForced(this.fixedValue, this.confidence);
                return true;
            }

            if (this.foundValue != null)
            {
                if (this.Verbose)
                {
                    Logger.Info($"Set parsevalue ({this.Attribute})[{this.confidence}]: {this.foundValue}");
                }

                this.resultAgentField.SetValueForced(this.foundValue, this.confidence);
                return true;
            }

            if (this.Verbose)
            {
                Logger.Info($"Nothing found for {this.Attribute}");
            }

            return false;
        }

        /// <summary>
        /// Resets the matches.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            this.foundValue = null;
        }

        /// <summary>
        /// The sets the user agent field.
        /// </summary>
        /// <param name="newResultAgentField">The new user agent to set.</param>
        public void SetResultAgentField(IAgentField newResultAgentField)
        {
            this.resultAgentField = newResultAgentField;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (this.IsFixedValue())
            {
                return $"Extract FIXED. ({this.Matcher.MatcherSourceLocation}): ({this.Attribute}, {this.confidence}) =   \"{this.fixedValue}\"";
            }
            else
            {
                return $"Extract DYNAMIC. ({this.Matcher.MatcherSourceLocation}): ({this.Attribute}, {this.confidence}):    {this.expression}";
            }
        }

        /// <summary>
        /// Returns the matcher extract parser.
        /// </summary>
        /// <param name="parser">The parser<see cref="UserAgentTreeWalkerParser"/>.</param>
        /// <returns>The <see cref="ParserRuleContext"/>.</returns>
        protected override ParserRuleContext ParseWalkerExpression(UserAgentTreeWalkerParser parser)
        {
            return parser.matcherExtract();
        }

        /// <summary>
        /// Sets the fixed value with provided value.
        /// </summary>
        /// <param name="newFixedValue">The fixed value to set.</param>
        protected override void SetFixedValue(string newFixedValue)
        {
            if (this.Verbose)
            {
                Logger.Info($"-- set Fixed value({this.Attribute} , {this.confidence} , {newFixedValue})");
            }

            this.fixedValue = newFixedValue;
        }
    }
}
