//-----------------------------------------------------------------------
// <copyright file="MatcherExtractAction.cs" company="OrbintSoft">
//   Yet Another User Agent Analyzer for .NET Standard
//   porting realized by Stefano Balzarotti, Copyright 2018-2019 (C) OrbintSoft
//
//   Original Author and License:
//
//   Yet Another UserAgent Analyzer
//   Copyright(C) 2013-2019 Niels Basjes
//
//  Licensed under the Apache License, Version 2.0 (the "License");
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
    using log4net;
    using OrbintSoft.Yauaa.Analyze.TreeWalker.Steps;
    using OrbintSoft.Yauaa.Analyzer;
    using OrbintSoft.Yauaa.Antlr4Source;

    /// <summary>
    /// Defines the <see cref="MatcherExtractAction" />.
    /// </summary>
    [Serializable]
    public class MatcherExtractAction : MatcherAction
    {
        /// <summary>
        /// Defines the Log.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(MatcherExtractAction));

        /// <summary>
        /// Defines the confidence.
        /// </summary>
        private readonly long confidence;

        /// <summary>
        /// Defines the expression.
        /// </summary>
        private readonly string expression;

        /// <summary>
        /// Defines the fixedValue.
        /// </summary>
        private string fixedValue = null;

        /// <summary>
        /// Defines the foundValue.
        /// </summary>
        private string foundValue = null;

        /// <summary>
        /// Defines the resultAgentField.
        /// </summary>
        private AgentField resultAgentField = null;

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
        /// Gets the Attribute.
        /// </summary>
        public string Attribute { get; }

        /// <summary>
        /// The Inform.
        /// </summary>
        /// <param name="key">The key<see cref="string"/>.</param>
        /// <param name="newlyFoundValue">The newlyFoundValue<see cref="WalkList.WalkResult"/>.</param>
        public override void Inform(string key, WalkList.WalkResult newlyFoundValue)
        {
            if (this.Verbose)
            {
                Log.Info(string.Format("INFO  : EXTRACT ({0}): {1}", this.Attribute, key));
                Log.Info(string.Format("NEED  : EXTRACT ({0}): {1}", this.Attribute, this.MatchExpression));
            }

            /*
             * We know the tree is parsed from left to right.
             * This is also the priority in the fields.
             * So we always use the first value we find.
             */
            if (this.foundValue == null)
            {
                this.foundValue = newlyFoundValue.Value;
                if (this.Verbose)
                {
                    Log.Info(string.Format("KEPT  : EXTRACT ({0}): {1}", this.Attribute, key));
                }
            }
        }

        /// <summary>
        /// The IsFixedValue.
        /// </summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool IsFixedValue()
        {
            return this.fixedValue != null;
        }

        /// <summary>
        /// The ObtainResult.
        /// </summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool ObtainResult()
        {
            this.ProcessInformedMatches();
            if (this.fixedValue != null)
            {
                if (this.Verbose)
                {
                    Log.Info(string.Format("Set fixedvalue ({0})[{1}]: {2}", this.Attribute, this.confidence, this.fixedValue));
                }

                this.resultAgentField.SetValueForced(this.fixedValue, this.confidence);
                return true;
            }

            if (this.foundValue != null)
            {
                if (this.Verbose)
                {
                    Log.Info(string.Format("Set parsevalue ({0})[{1}]: {2}", this.Attribute, this.confidence, this.foundValue));
                }

                this.resultAgentField.SetValueForced(this.foundValue, this.confidence);
                return true;
            }

            if (this.Verbose)
            {
                Log.Info(string.Format("Nothing found for {0}", this.Attribute));
            }

            return false;
        }

        /// <summary>
        /// The Reset.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            this.foundValue = null;
        }

        /// <summary>
        /// The SetResultAgentField.
        /// </summary>
        /// <param name="newResultAgentField">The newResultAgentField<see cref="UserAgent.AgentField"/>.</param>
        public void SetResultAgentField(AgentField newResultAgentField)
        {
            this.resultAgentField = newResultAgentField;
        }

        /// <summary>
        /// The ToString.
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        public override string ToString()
        {
            if (this.IsFixedValue())
            {
                return "FIXED  : (" + this.Attribute + ", " + this.confidence + ") =   \"" + this.fixedValue + "\"";
            }
            else
            {
                return "DYNAMIC: (" + this.Attribute + ", " + this.confidence + "):    " + this.expression;
            }
        }

        /// <summary>
        /// The ParseWalkerExpression.
        /// </summary>
        /// <param name="parser">The parser<see cref="UserAgentTreeWalkerParser"/>.</param>
        /// <returns>The <see cref="ParserRuleContext"/>.</returns>
        protected override ParserRuleContext ParseWalkerExpression(UserAgentTreeWalkerParser parser)
        {
            return parser.matcherExtract();
        }

        /// <summary>
        /// The SetFixedValue.
        /// </summary>
        /// <param name="newFixedValue">The newFixedValue<see cref="string"/>.</param>
        protected override void SetFixedValue(string newFixedValue)
        {
            if (this.Verbose)
            {
                Log.Info(string.Format("-- set Fixed value({0} , {1} , {2})", this.Attribute, this.confidence, newFixedValue));
            }

            this.fixedValue = newFixedValue;
        }
    }
}
