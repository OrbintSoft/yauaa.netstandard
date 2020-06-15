//-----------------------------------------------------------------------
// <copyright file="MatcherRequireAction.cs" company="OrbintSoft">
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
    using OrbintSoft.Yauaa.Antlr4Source;
    using OrbintSoft.Yauaa.Logger;

    /// <summary>
    /// This class is used to define a require value action for a matcher.
    /// </summary>
    [Serializable]
    public class MatcherRequireAction : MatcherAction
    {
        /// <summary>
        /// Defines the logger.
        /// </summary>
        private static readonly ILogger Logger = new Logger<MatcherRequireAction>();

        /// <summary>
        /// Defines wether a required value is found.
        /// </summary>
        private bool foundRequiredValue = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatcherRequireAction"/> class.
        /// </summary>
        /// <param name="config">The matcher configuration.</param>
        /// <param name="matcher">The <see cref="Matcher"/>.</param>
        public MatcherRequireAction(string config, Matcher matcher)
        {
            this.Init(config, matcher);
        }

        /// <summary>
        /// Itialize the matcher with require action.
        /// </summary>
        /// <returns>The number of steps.</returns>
        public override long Initialize()
        {
            var newEntries = base.Initialize();
            newEntries -= this.Evaluator.PruneTrailingStepsThatCannotFail();
            return newEntries;
        }

        /// <summary>
        /// The Inform.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="foundValue">Not used.</param>
        public override void Inform(string key, WalkList.WalkResult foundValue)
        {
            this.foundRequiredValue = true;
            if (this.Verbose)
            {
                Logger.Info($"Info REQUIRE: {key}");
                Logger.Info($"NEED REQUIRE: {this.MatchExpression}");
                Logger.Info($"KEPT REQUIRE: {key}");
            }
        }

        /// <summary>
        /// Find required value.
        /// </summary>
        /// <returns>True if required value is found.</returns>
        public override bool ObtainResult()
        {
            if (this.IsValidIsNull())
            {
                this.foundRequiredValue = true;
            }
            else
            {
                this.ProcessInformedMatches();
            }

            return this.foundRequiredValue;
        }

        /// <summary>
        /// Reset the matches.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            this.foundRequiredValue = false;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Require. {this.Matcher.MatcherSourceLocation}: {this.MatchExpression}";
        }

        /// <summary>
        /// Execute the matcher require parser.
        /// </summary>
        /// <param name="parser">The parser<see cref="UserAgentTreeWalkerParser"/>.</param>
        /// <returns>The <see cref="ParserRuleContext"/>.</returns>
        protected override ParserRuleContext ParseWalkerExpression(UserAgentTreeWalkerParser parser)
        {
            return parser.matcherRequire();
        }

        /// <summary>
        /// Cannot set a fixed value for a require action.
        /// </summary>
        /// <param name="fixedValue">The fixed Value.</param>
        protected override void SetFixedValue(string fixedValue)
        {
            throw new InvalidParserConfigurationException($"It is useless to put a fixed value \"{fixedValue}\" in the require section.");
        }
    }
}
