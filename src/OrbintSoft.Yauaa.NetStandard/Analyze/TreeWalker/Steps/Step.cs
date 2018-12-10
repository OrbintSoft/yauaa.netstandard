//-----------------------------------------------------------------------
// <copyright file="Step.cs" company="OrbintSoft">
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

namespace OrbintSoft.Yauaa.Analyze.TreeWalker.Steps
{
    using Antlr4.Runtime;
    using Antlr4.Runtime.Tree;
    using log4net;
    using OrbintSoft.Yauaa.Antlr4Source;
    using OrbintSoft.Yauaa.Utils;
    using System;
    using System.Runtime.Serialization;
    using System.Text;

    /// <summary>
    /// Defines the <see cref="Step" />
    /// </summary>
    [Serializable]
    public abstract class Step: ISerializable
    {
        /// <summary>
        /// Defines the Log
        /// </summary>
        internal static readonly ILog Log = LogManager.GetLogger(typeof(Step));

        /// <summary>
        /// Defines the stepNr
        /// </summary>
        private int stepNr = 0;

        public Step()
        {

        }

        public Step(SerializationInfo info, StreamingContext context)
        {
            this.Logprefix = (string)info.GetValue("Logprefix", typeof(string));
            this.Verbose = (bool)info.GetValue("Verbose", typeof(bool));
            this.NextStep = (Step)info.GetValue("NextStep", typeof(Step));
            this.stepNr = (int)info.GetValue("stepNr", typeof(int));
        }


        /// <summary>
        /// Gets the NextStep
        /// </summary>
        public Step NextStep { get; private set; } = null;

        /// <summary>
        /// Gets or sets the Logprefix
        /// Gets the Logprefix
        /// Defines the logprefix
        /// </summary>
        protected string Logprefix { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether Verbose
        /// </summary>
#if VERBOSE
        protected bool Verbose { get; set; } = true;
#else
        protected bool Verbose { get; set; } = false;
#endif
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
        /// Some steps cannot fail.
        /// For a require rule if the last step cannot fail then this can be removed from the require list
        /// to improve performance at run time.
        /// </summary>
        /// <returns>If this specific step can or cannot fail.</returns>
        public virtual bool CanFail()
        {
            return true; // Default is to assume the step is always needed.
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Logprefix", this.Logprefix, typeof(string));
            info.AddValue("Verbose", this.Verbose, typeof(bool));
            info.AddValue("NextStep", this.NextStep, typeof(Step));
            info.AddValue("stepNr", this.stepNr, typeof(int));
        }

        /// <summary>
        /// The SetNextStep
        /// </summary>
        /// <param name="newStepNr">The newStepNr<see cref="int"/></param>
        /// <param name="newNextStep">The newNextStep<see cref="Step"/></param>
        public void SetNextStep(int newStepNr, Step newNextStep)
        {
            this.stepNr = newStepNr;
            this.NextStep = newNextStep;
            var sb = new StringBuilder();
            for (var i = 0; i < newStepNr + 1; i++)
            {
                sb.Append("-->");
            }

            this.Logprefix = sb.ToString();           
        }

        /// <summary>
        /// The SetVerbose
        /// </summary>
        /// <param name="newVerbose">The newVerbose<see cref="bool"/></param>
        public void SetVerbose(bool newVerbose)
        {
            this.Verbose = newVerbose;
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

        /// <summary>
        /// The Up
        /// </summary>
        /// <param name="tree">The tree<see cref="IParseTree"/></param>
        /// <returns>The <see cref="IParseTree"/></returns>
        protected IParseTree Up(IParseTree tree)
        {
            var parent = tree.Parent;

            // Needed because of the way the ANTLR rules have been defined.
            if (parent is UserAgentParser.ProductNameContext ||
                parent is UserAgentParser.ProductVersionContext ||
                parent is UserAgentParser.ProductVersionWithCommasContext)
            {
                return this.Up(parent);
            }

            return parent;
        }

        /// <summary>
        /// The WalkNextStep
        /// </summary>
        /// <param name="tree">The tree<see cref="IParseTree"/></param>
        /// <param name="value">The value<see cref="string"/></param>
        /// <returns>The <see cref="WalkList.WalkResult"/></returns>
        protected WalkList.WalkResult WalkNextStep(IParseTree tree, string value)
        {
            if (this.NextStep == null)
            {
                var res = value;
                if (value == null)
                {
                    res = AntlrUtils.GetSourceText((ParserRuleContext)tree);
                }

                if (this.Verbose)
                {
                    Log.Info(string.Format("{0} Final (implicit) step: {1}", this.Logprefix, res));
                }

                return new WalkList.WalkResult(tree, res);
            }

            if (this.Verbose)
            {
                Log.Info(string.Format("{0} Tree: >>>{1}<<<", this.Logprefix, AntlrUtils.GetSourceText((ParserRuleContext)tree)));
                Log.Info(string.Format("{0} Enter step({1}): {2}", this.Logprefix, this.stepNr, this.NextStep));
            }

            var result = this.NextStep.Walk(tree, value);
            if (this.Verbose)
            {
                Log.Info(string.Format("{0} Result: >>>{1}<<<", this.Logprefix, result == null ? "null" : result.ToString()));
                Log.Info(string.Format("{0} Leave step({1}): {2}", this.Logprefix, result == null ? "-" : "+", this.NextStep));
            }

            return result;
        }
    }
}
