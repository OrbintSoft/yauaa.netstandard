﻿/*
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
using log4net;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze.TreeWalker.Steps;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Antlr4Source;
using System;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze
{
    [Serializable]
    public class MatcherRequireAction : MatcherAction
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MatcherRequireAction(string config, Matcher matcher)
        {
            Init(config, matcher);
        }

        protected override ParserRuleContext ParseWalkerExpression(UserAgentTreeWalkerParser parser)
        {
            return parser.matcherRequire();
        }

        protected override void SetFixedValue(string fixedValue)
        {
            throw new InvalidParserConfigurationException(
                    "It is useless to put a fixed value \"" + fixedValue + "\" in the require section.");
        }

        private bool foundRequiredValue = false;

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

        public override bool ObtainResult()
        {
            if (IsValidIsNull())
            {
                foundRequiredValue = true;
            }
            ProcessInformedMatches();
            return foundRequiredValue;
        }

        public override void Reset()
        {
            base.Reset();
            foundRequiredValue = false;
        }

        public override string ToString()
        {
            return "Require: " + GetMatchExpression();
        }
    }
}