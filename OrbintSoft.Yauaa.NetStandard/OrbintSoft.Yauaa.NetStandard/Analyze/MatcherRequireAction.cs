//-----------------------------------------------------------------------
// <copyright file="MatcherRequireAction.cs" company="OrbintSoft">
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
    using Antlr4.Runtime;
    using log4net;
    using OrbintSoft.Yauaa.Analyze.TreeWalker.Steps;
    using OrbintSoft.Yauaa.Antlr4Source;
    using System;

    /// <summary>
    /// Defines the <see cref="MatcherRequireAction" />
    /// </summary>
    [Serializable]
    public class MatcherRequireAction : MatcherAction
    {
        /// <summary>
        /// Defines the Log
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(MatcherRequireAction));

        /// <summary>
        /// Defines the foundRequiredValue
        /// </summary>
        private bool foundRequiredValue = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatcherRequireAction"/> class.
        /// </summary>
        /// <param name="config">The config<see cref="string"/></param>
        /// <param name="matcher">The matcher<see cref="Matcher"/></param>
        public MatcherRequireAction(string config, Matcher matcher)
        {
            Init(config, matcher);
        }

        public override void Initialize()
        {
            base.Initialize();
            evaluator.PruneTrailingStepsThatCannotFail();
        }

        /// <summary>
        /// The Inform
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="foundValue">The foundValue<see cref="WalkList.WalkResult"/></param>
        public override void Inform(string key, WalkList.WalkResult foundValue)
        {
            foundRequiredValue = true;
            if (verbose)
            {
                Log.Info(string.Format("Info REQUIRE: {0}", key));
                Log.Info(string.Format("NEED REQUIRE: {0}", GetMatchExpression()));
                Log.Info(string.Format("KEPT REQUIRE: {0}", key));
            }
        }

        /// <summary>
        /// The ObtainResult
        /// </summary>
        /// <returns>The <see cref="bool"/></returns>
        public override bool ObtainResult()
        {
            if (IsValidIsNull())
            {
                foundRequiredValue = true;
            }
            ProcessInformedMatches();
            return foundRequiredValue;
        }

        /// <summary>
        /// The Reset
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            foundRequiredValue = false;
        }

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            return "Require: " + GetMatchExpression();
        }

        /// <summary>
        /// The ParseWalkerExpression
        /// </summary>
        /// <param name="parser">The parser<see cref="UserAgentTreeWalkerParser"/></param>
        /// <returns>The <see cref="ParserRuleContext"/></returns>
        protected override ParserRuleContext ParseWalkerExpression(UserAgentTreeWalkerParser parser)
        {
            return parser.matcherRequire();
        }

        /// <summary>
        /// The SetFixedValue
        /// </summary>
        /// <param name="fixedValue">The fixedValue<see cref="string"/></param>
        protected override void SetFixedValue(string fixedValue)
        {
            throw new InvalidParserConfigurationException(
                    "It is useless to put a fixed value \"" + fixedValue + "\" in the require section.");
        }
    }
}
