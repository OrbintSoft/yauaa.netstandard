//<copyright file="TreeExpressionEvaluator.cs" company="OrbintSoft">
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
//<date>2018, 7, 27, 11:17</date>
//<summary></summary>

using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using log4net;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze.TreeWalker.Steps;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Antlr4Source;
using System;
using System.Collections.Generic;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze.TreeWalker
{
    /// <summary>
    /// This class gets the symbol table (1 value) uses that to evaluate
    /// the expression against the parsed user agent
    /// </summary>
    [Serializable]
    public class TreeExpressionEvaluator
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TreeExpressionEvaluator));
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

        public WalkList.WalkResult Evaluate(IParseTree tree, string key, string value)
        {
            if (verbose)
            {
                Log.Info(string.Format("Evaluate: {0} => {1}", key, value));
                Log.Info(string.Format("Pattern : {0}", requiredPatternText));
                Log.Info(string.Format("WalkList: {0}", walkList.ToString()));
            }
            WalkList.WalkResult result = walkList.Walk(tree, value);
            if (verbose)
            {
                Log.Info(string.Format("Evaluate: Result = {0}", result == null ? "null" : result.GetValue()));
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

        private string CalculateFixedValue(ParserRuleContext requiredPattern)
        {
            return new DerivedUserAgentTreeWalkerBaseVisitor(matcher).Visit(requiredPattern);
        }

        private class DerivedUserAgentTreeWalkerBaseVisitor : UserAgentTreeWalkerBaseVisitor<string>
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
                // Now we know this is a fixed value. Yet we can have a problem in the lookup that was
                // configured. If we have this then this is a FATAL error (it will fail always everywhere).
                var lookups = matcher.GetLookups();
                IDictionary<string, string> lookup = lookups.ContainsKey(context.lookup.Text) ? lookups[context.lookup.Text] : null;
                if (lookup == null)
                {
                    throw new InvalidParserConfigurationException("Missing lookup \"" + context.lookup.Text + "\" ");
                }
                var l = value.ToLower();
                string resultingValue = lookup.ContainsKey(l) ? lookup[l] : null;
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

            public override string VisitPathFixedValue(UserAgentTreeWalkerParser.PathFixedValueContext ctx)
            {
                return ctx.value.Text;
            }
        }
    }
}

