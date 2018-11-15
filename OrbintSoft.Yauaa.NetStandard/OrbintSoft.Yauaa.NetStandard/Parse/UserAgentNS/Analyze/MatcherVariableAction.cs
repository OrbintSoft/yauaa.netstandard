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

using Antlr4.Runtime;
using log4net;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze.TreeWalker.Steps;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Antlr4Source;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze
{
    [Serializable]
    public class MatcherVariableAction :MatcherAction
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string variableName;
        private readonly string expression;

        private WalkList.WalkResult foundValue = null;       
        private ISet<MatcherAction> interestedActions = null;

        public MatcherVariableAction(string variableName, string config, Matcher matcher)
        {
            this.variableName = variableName;
            expression = config;
            Init(config, matcher);
        }

        protected override ParserRuleContext ParseWalkerExpression(UserAgentTreeWalkerParser parser)
        {
            return parser.matcher();
        }

        protected override void SetFixedValue(string fixedValue)
        {
            throw new InvalidParserConfigurationException(
                "It is useless to put a fixed value \"" + fixedValue + "\" in the variable section.");
        }

        public string GetVariableName()
        {
            return variableName;
        }

        public override void Inform(string key, WalkList.WalkResult newlyFoundValue)
        {
            if (verbose)
            {
                Log.Info(string.Format("INFO  : VARIABLE ({0}): {1}", variableName, key));
                Log.Info(string.Format("NEED  : VARIABLE ({0}): {1}", variableName, GetMatchExpression()));
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
                    Log.Info(string.Format("KEPT  : VARIABLE ({0}): {1}", variableName, key));
                }

                if (interestedActions != null && interestedActions.Count != 0)
                {
                    foreach (MatcherAction action in interestedActions)
                    {
                        action.Inform(variableName, newlyFoundValue.GetValue(), newlyFoundValue.GetTree());
                    }
                }
            }
        }

        public override bool ObtainResult()
        {
            ProcessInformedMatches();
            return foundValue != null;
        }

        public override void Reset()
        {
            base.Reset();
            foundValue = null;
        }

        
        public override string ToString()
        {
            return "VARIABLE: (" + variableName + "): " + expression;
        }

        public void SetInterestedActions(ISet<MatcherAction> newInterestedActions)
        {
            interestedActions = newInterestedActions;
        }
    }
}