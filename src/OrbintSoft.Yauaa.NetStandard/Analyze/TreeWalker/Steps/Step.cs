//-----------------------------------------------------------------------
// <copyright file="Step.cs" company="OrbintSoft">
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

namespace OrbintSoft.Yauaa.Analyze.TreeWalker.Steps
{
    using System;
    using System.Runtime.Serialization;
    using System.Text;
    using Antlr4.Runtime;
    using Antlr4.Runtime.Tree;
    using log4net;
    using OrbintSoft.Yauaa.Antlr4Source;
    using OrbintSoft.Yauaa.Utils;

    /// <summary>
    /// This base class is used to define the implementation of a step in parsing the tree.
    /// </summary>
    [Serializable]
    public abstract class Step : ISerializable
    {
        /// <summary>
        /// The logger.
        /// </summary>
        internal static readonly ILog Log = LogManager.GetLogger(typeof(Step));

        /// <summary>
        /// The step number.
        /// </summary>
        private int stepNr = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Step"/> class.
        /// </summary>
        public Step()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Step"/> class.
        /// This is used only for binary deserialization.
        /// </summary>
        /// <param name="info">The info <see cref="SerializationInfo"/>.</param>
        /// <param name="context">The context <see cref="StreamingContext"/>.</param>
        public Step(SerializationInfo info, StreamingContext context)
        {
            this.Logprefix = (string)info.GetValue(nameof(this.Logprefix), typeof(string));
            this.Verbose = (bool)info.GetValue(nameof(this.Verbose), typeof(bool));
            this.NextStep = (Step)info.GetValue(nameof(this.NextStep), typeof(Step));
            this.stepNr = (int)info.GetValue(nameof(this.stepNr), typeof(int));
        }

        /// <summary>
        /// Gets the NextStep.
        /// </summary>
        public Step NextStep { get; private set; } = null;

        /// <summary>
        /// Gets or sets the Logprefix
        /// It is s a prefix used in logging to improve formatting and readibility.
        /// </summary>
        protected string Logprefix { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether verbose logging is enabled.
        /// </summary>
        protected bool Verbose { get; set; } = false;

        /// <summary>
        /// Checks if the node is a comment separator or a terminal node.
        /// </summary>
        /// <param name="tree">The tree node.</param>
        /// <returns>True if the node is a separator.</returns>
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

        /// <summary>
        /// This is used for binary serialization.
        /// Populates a SerializationInfo with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(this.Logprefix), this.Logprefix, typeof(string));
            info.AddValue(nameof(this.Verbose), this.Verbose, typeof(bool));
            info.AddValue(nameof(this.NextStep), this.NextStep, typeof(Step));
            info.AddValue(nameof(this.stepNr), this.stepNr, typeof(int));
        }

        /// <summary>
        /// Sets the next step to be executed with the step number.
        /// </summary>
        /// <param name="newStepNr">The number of the new step.</param>
        /// <param name="newNextStep">The new next step.</param>
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
        /// Sets the value of verbose to the new value.
        /// It is used to enable verbose logging.
        /// </summary>
        /// <param name="newVerbose">The new verbose value.</param>
        public void SetVerbose(bool newVerbose)
        {
            this.Verbose = newVerbose;
        }

        /// <summary>
        /// This will walk into the tree and recurse through all the remaining steps.
        /// This must iterate of all possibilities and return the first matching result.
        /// </summary>
        /// <param name="tree">The tree to walk into.</param>
        /// <param name="value">The value<see cref="string"/>.</param>
        /// <returns>Either null or the actual value that was found.</returns>
        public abstract WalkList.WalkResult Walk(IParseTree tree, string value);

        /// <summary>
        /// If value is null returns the value of the text value of the node.
        /// Otherwise it just return the provided value.
        /// </summary>
        /// <param name="tree">The node tree.</param>
        /// <param name="value">The provided value.</param>
        /// <returns>The value.</returns>
        protected string GetActualValue(IParseTree tree, string value)
        {
            if (value is null)
            {
                return AntlrUtils.GetSourceText((ParserRuleContext)tree);
            }

            return value;
        }

        /// <summary>
        /// Returns the parent node in the tree.
        /// </summary>
        /// <param name="tree">The node tree.</param>
        /// <returns>The parent node.</returns>
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
        /// It walks to the next step in the tree.
        /// If the next step is null returns the provided value or source text of the node as result.
        /// </summary>
        /// <param name="tree">The node tree.</param>
        /// <param name="value">The provided value.</param>
        /// <returns>The <see cref="WalkList.WalkResult"/>.</returns>
        protected WalkList.WalkResult WalkNextStep(IParseTree tree, string value)
        {
            if (this.NextStep is null)
            {
                var res = value;
                if (value is null)
                {
                    res = AntlrUtils.GetSourceText((ParserRuleContext)tree);
                }

                if (this.Verbose)
                {
                    Log.Info($"{this.Logprefix} Final (implicit) step: {res}");
                }

                return new WalkList.WalkResult(tree, res);
            }

            if (this.Verbose)
            {
                Log.Info($"{this.Logprefix} Tree: >>>{AntlrUtils.GetSourceText((ParserRuleContext)tree)}<<<");
                Log.Info($"{this.Logprefix} Enter step({this.stepNr}): {this.NextStep}");
            }

            var result = this.NextStep.Walk(tree, value);
            if (this.Verbose)
            {
                Log.Info($"{this.Logprefix} Result: >>>{(result is null ? "null" : result.ToString())}<<<");
                Log.Info($"{this.Logprefix} Leave step({(result is null ? "-" : "+")}): {this.NextStep}");
            }

            return result;
        }
    }
}