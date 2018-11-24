//-----------------------------------------------------------------------
// <copyright file="Step.cs" company="OrbintSoft">
//    Yet Another UserAgent Analyzer.NET Standard
//    orting realized by Stefano Balzarotti, Copyright (C) OrbintSoft
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
namespace OrbintSoft.Yauaa.Analyze.TreeWalker.Steps
{
    using Antlr4.Runtime;
    using Antlr4.Runtime.Tree;
    using log4net;
    using OrbintSoft.Yauaa.Antlr4Source;
    using OrbintSoft.Yauaa.Utils;
    using System;
    using System.Text;

    /// <summary>
    /// Defines the <see cref="Step" />
    /// </summary>
    [Serializable]
    public abstract class Step
    {
        /// <summary>
        /// Defines the Log
        /// </summary>
        internal static readonly ILog Log = LogManager.GetLogger(typeof(Step));

        /// <summary>
        /// Defines the stepNr
        /// </summary>
        private int stepNr = 0;

        /// <summary>
        /// Defines the logprefix
        /// </summary>
        protected string logprefix = "";

        /// <summary>
        /// Defines the nextStep
        /// </summary>
        private Step nextStep = null;

#if VERBOSE
        protected bool verbose = true;
#else
        /// <summary>
        /// Defines the verbose
        /// </summary>
        protected bool verbose = false;

#endif
        /// <summary>
        /// The SetVerbose
        /// </summary>
        /// <param name="newVerbose">The newVerbose<see cref="bool"/></param>
        public void SetVerbose(bool newVerbose)
        {
            verbose = newVerbose;
        }

        /// <summary>
        /// The SetNextStep
        /// </summary>
        /// <param name="newStepNr">The newStepNr<see cref="int"/></param>
        /// <param name="newNextStep">The newNextStep<see cref="Step"/></param>
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

        /// <summary>
        /// The TreeIsSeparator
        /// </summary>
        /// <param name="tree">The tree<see cref="IParseTree"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public static bool TreeIsSeparator(IParseTree tree)
        {
            return tree is UserAgentParser.CommentSeparatorContext || tree is ITerminalNode;
        }

        /// <summary>
        /// This will walk into the tree and recurse through all the remaining steps.
        /// This must iterate of all possibilities and return the first matching result.
        /// </summary>
        /// <param name="tree">The tree to walk into.</param>
        /// <param name="value">The value<see cref="string"/></param>
        /// <returns>Either null or the actual value that was found.</returns>
        public abstract WalkList.WalkResult Walk(IParseTree tree, string value);

        /// <summary>
        /// The GetNextStep
        /// </summary>
        /// <returns>The <see cref="Step"/></returns>
        public Step GetNextStep()
        {
            return nextStep;
        }

        /// <summary>
        /// The WalkNextStep
        /// </summary>
        /// <param name="tree">The tree<see cref="IParseTree"/></param>
        /// <param name="value">The value<see cref="string"/></param>
        /// <returns>The <see cref="WalkList.WalkResult"/></returns>
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

        /// <summary>
        /// Some steps cannot fail.
        /// For a require rule if the last step cannot fail then this can be removed from the require list
        /// to improve performance at run time.
        /// </summary>
        /// <returns>If this specific step can or cannot fail.</returns>
        public virtual bool CanFail()
        {
            return true; // Default is to assume the step is always needed.
        }


        /// <summary>
        /// The Up
        /// </summary>
        /// <param name="tree">The tree<see cref="IParseTree"/></param>
        /// <returns>The <see cref="IParseTree"/></returns>
        protected IParseTree Up(IParseTree tree)
        {
            IParseTree parent = tree.Parent;
            // Needed because of the way the ANTLR rules have been defined.
            if (parent is UserAgentParser.ProductNameContext ||
                parent is UserAgentParser.ProductVersionContext ||
                parent is UserAgentParser.ProductVersionWithCommasContext
            )
            {
                return Up(parent);
            }
            return parent;
        }



        /// <summary>
        /// The GetActualValue
        /// </summary>
        /// <param name="tree">The tree<see cref="IParseTree"/></param>
        /// <param name="value">The value<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        protected string GetActualValue(IParseTree tree, string value)
        {
            if (value == null)
            {
                return AntlrUtils.GetSourceText((ParserRuleContext)tree);
            }
            return value;
        }
    }
}
