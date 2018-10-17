/*
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

using System;
using Antlr4.Runtime;
using log4net;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze.TreeWalker.Steps;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Antlr4Source;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze
{
    public class MatcherExtractAction: MatcherAction
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(MatcherExtractAction));

        private readonly string attribute;
        private readonly long confidence;
        private string foundValue = null;
        private string fixedValue = null;
        private readonly string expression;
        private UserAgent.AgentField resultAgentField;

        public MatcherExtractAction(string attribute, long confidence, string config, Matcher matcher)
        {
            this.attribute = attribute;
            this.confidence = confidence;
            expression = config;
            Init(config, matcher);
        }

        public void SetResultAgentField(UserAgent.AgentField newResultAgentField)
        {
            resultAgentField = newResultAgentField;
        }

        protected override ParserRuleContext ParseWalkerExpression(UserAgentTreeWalkerParser parser)
        {
            return parser.matcher();
        }

        public bool IsFixedValue()
        {
            return fixedValue != null;
        }

        protected override void SetFixedValue(string newFixedValue)
        {
            if (verbose)
            {
                LOG.Info(string.Format("-- set Fixed value({0} , {1} , {2})", attribute, confidence, newFixedValue));
            }
            fixedValue = newFixedValue;
        }

        public string GetAttribute()
        {
            return attribute;
        }

        public override void Inform(string key, WalkList.WalkResult newlyFoundValue)
        {
            if (verbose)
            {
                LOG.Info(string.Format("INFO  : EXTRACT ({0}): {1}", attribute, key));
                LOG.Info(string.Format("NEED  : EXTRACT ({0}): {1}", attribute, GetMatchExpression()));
            }
            /*
             * We know the tree is parsed from left to right.
             * This is also the priority in the fields.
             * So we always use the first value we find.
             */
            if (this.foundValue == null)
            {
                foundValue = newlyFoundValue.GetValue();
                if (verbose)
                {
                    LOG.Info(string.Format("KEPT  : EXTRACT ({0}): {1}", attribute, key));
                }
            }
            else
            {
                if (verbose)
                {
                    LOG.Info(string.Format("IGNORE: EXTRACT ({0}): {1}", attribute, key));
                }
            }
        }

        public override bool ObtainResult()
        {
            ProcessInformedMatches();
            if (fixedValue != null)
            {
                if (verbose)
                {
                    LOG.Info(string.Format("Set fixedvalue ({0})[{1}]: {2}", attribute, confidence, fixedValue));
                }
                resultAgentField.SetValueForced(fixedValue, confidence);
                return true;
            }
            if (foundValue != null)
            {
                if (verbose)
                {
                    LOG.Info(string.Format("Set parsevalue ({0})[{1}]: {2}", attribute, confidence, foundValue));
                }
                resultAgentField.SetValueForced(foundValue, confidence);
                return true;
            }
            if (verbose)
            {
                LOG.Info(string.Format("Nothing found for {0}", attribute));
            }
            return false;
        }

        public override void Reset()
        {
            base.Reset();
            foundValue = null;
        }

        public override string ToString()
        {
            if (IsFixedValue())
            {
                return "FIXED  : (" + attribute + ", " + confidence + ") =   \"" + fixedValue + "\"";
            }
            else
            {
                return "DYNAMIC: (" + attribute + ", " + confidence + "):    " + expression;
            }
        }
    }
}