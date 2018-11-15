//<copyright file="Step.cs" company="OrbintSoft">
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
//<date>2018, 8, 12, 14:08</date>
//<summary></summary>

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using log4net;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Antlr4Source;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Utils;
using System;
using System.Reflection;
using System.Text;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze.TreeWalker.Steps
{
    [Serializable]
    public abstract class Step
    {
        internal static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private int stepNr = 0;
        protected string logprefix = "";
        private Step nextStep = null;
#if VERBOSE
        protected bool verbose = true;
#else
        protected bool verbose = false;
#endif

        public void SetVerbose(bool newVerbose)
        {
            verbose = newVerbose;
        }

        public void SetNextStep(int newStepNr, Step newNextStep)
        {
            stepNr = newStepNr;
            nextStep = newNextStep;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < newStepNr + 1; i++)
            {
                sb.Append("-->");
            }
            logprefix = sb.ToString();
        }

        protected WalkList.WalkResult WalkNextStep(IParseTree tree, string value)
        {
            if (nextStep == null)
            {
                string res = value;
                if (value == null)
                {
                    res = AntlrUtils.GetSourceText((ParserRuleContext)tree);
                }
                if (verbose)
                {
                    Log.Info(string.Format("{0} Final (implicit) step: {1}", logprefix, res));
                }
                return new WalkList.WalkResult(tree, res);
            }
            if (verbose)
            {
                Log.Info(string.Format("{0} Tree: >>>{1}<<<", logprefix, AntlrUtils.GetSourceText((ParserRuleContext)tree)));
                Log.Info(string.Format("{0} Enter step({1}): {2}", logprefix, stepNr, nextStep));
            }
            WalkList.WalkResult result = nextStep.Walk(tree, value);
            if (verbose)
            {
                Log.Info(string.Format("{0} Result: >>>{1}<<<", logprefix, result == null ? "null" : result.ToString()));
                Log.Info(string.Format("{0} Leave step({1}): {2}", logprefix, result == null ? "-" : "+", nextStep));
            }
            return result;           
        }

        protected IParseTree Up(IParseTree tree)
        {
            IParseTree parent = tree.Parent;
            // Needed because of the way the ANTLR rules have been defined.
            if (parent is UserAgentParser.ProductNameContext ||
                parent is UserAgentParser.ProductVersionContext ||
                parent is UserAgentParser.ProductVersionWithCommasContext
            ) {
                return Up(parent);
            }
            return parent;
        }

        public static bool TreeIsSeparator(IParseTree tree)
        {
            return tree is UserAgentParser.CommentSeparatorContext || tree is ITerminalNode;
        }

        protected string GetActualValue(IParseTree tree, String value)
        {
            if (value == null)
            {
                return AntlrUtils.GetSourceText((ParserRuleContext)tree);
            }
            return value;
        }

        /// <summary>
        /// This will walk into the tree and recurse through all the remaining steps.
        /// This must iterate of all possibilities and return the first matching result.
        /// </summary>
        /// <param name="tree">The tree to walk into.</param>
        /// <param name="value">
        /// The string representation of the previous step (needed for compare and lookup operations).
        /// The null value means to use the implicit 'full' value (i.e. getSourceText(tree) )
        /// </param>
        /// <returns>Either null or the actual value that was found.</returns>
        public abstract WalkList.WalkResult Walk(IParseTree tree, String value);

        public Step GetNextStep()
        {
            return nextStep;
        }

    }
}
