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

using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using log4net;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze.TreeWalker.Steps;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Antlr4Source;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze.TreeWalker
{
    /// <summary>
    /// This class gets the symbol table (1 value) uses that to evaluate
    /// the expression against the parsed user agent
    /// </summary>
    public class TreeExpressionEvaluator
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(TreeExpressionEvaluator));
        private readonly bool verbose;

        private readonly string requiredPatternText;
        private readonly Matcher matcher;
        private readonly WalkList walkList;
        private readonly string fixedValue;

        public TreeExpressionEvaluator(ParserRuleContext requiredPattern, Matcher matcher, bool verbose)
        {
            requiredPatternText = requiredPattern.GetText();
            this.matcher = matcher;
            this.verbose = verbose;
            fixedValue = CalculateFixedValue(requiredPattern);
            walkList = new WalkList(requiredPattern, matcher.GetLookups(), matcher.GetLookupSets(), verbose);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>The fixed value in case of a fixed value. NULL if a dynamic value</returns>
        public string GetFixedValue()
        {
            return fixedValue;
        }

        private class DerivedUserAgentTreeWalkerBaseVisitor: UserAgentTreeWalkerBaseVisitor<string>
        {
            private readonly Matcher matcher;
            public DerivedUserAgentTreeWalkerBaseVisitor(Matcher matcher)
            {
                this.matcher = matcher;
            }

            public override string VisitMatcherPathLookup([NotNull] UserAgentTreeWalkerParser.MatcherPathLookupContext context)
            {
                string value = Visit(context.matcher());
                if (value == null)
                {
                    return null;
                }
                // No we know this is a fixed value. Yet we can have a problem in the lookup that was
                // configured. If we have this then this is a FATAL error (it will fail always everywhere).

                Dictionary<string, string> lookup = matcher.GetLookups()[(context.lookup.Text)];
                if (lookup == null)
                {
                    throw new InvalidParserConfigurationException("Missing lookup \"" + context.lookup.Text + "\" ");
                }

                string resultingValue = lookup[value.ToLower()];
                if (resultingValue == null)
                {
                    if (context.defaultValue != null)
                    {
                        return context.defaultValue.Text;
                    }
                    throw new InvalidParserConfigurationException(
                        "Fixed value >>" + value + "<< is missing in lookup: \"" + context.lookup.Text + "\" ");
                }
                return resultingValue;
            }
        }

        private string CalculateFixedValue(ParserRuleContext requiredPattern)
        {
            return new DerivedUserAgentTreeWalkerBaseVisitor(matcher).Visit(requiredPattern); 
        }

        public WalkList.WalkResult Evaluate(IParseTree tree, string key, string value)
        {
            if (verbose)
            {
                LOG.Info(string.Format("Evaluate: {0} => {1}", key, value));
                LOG.Info(string.Format("Pattern : {0}", requiredPatternText));
                LOG.Info(string.Format("WalkList: {0}", walkList.ToString()));
            }
            WalkList.WalkResult result = walkList.Walk(tree, value);
            if (verbose)
            {
                LOG.Info(string.Format("Evaluate: Result = {0}", result == null ? "null" : result.GetValue()));
            }
            return result;
        }

        public bool UsesIsNull()
        {
            return walkList.UsesIsNull;            
        }

        public WalkList GetWalkListForUnitTesting()
        {
            return walkList;
        }
    }
}

