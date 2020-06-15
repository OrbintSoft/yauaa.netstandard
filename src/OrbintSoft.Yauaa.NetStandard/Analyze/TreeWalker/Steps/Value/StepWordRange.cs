//-----------------------------------------------------------------------
// <copyright file="StepWordRange.cs" company="OrbintSoft">
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

namespace OrbintSoft.Yauaa.Analyze.TreeWalker.Steps.Value
{
    using System;
    using System.Runtime.Serialization;
    using Antlr4.Runtime.Tree;
    using OrbintSoft.Yauaa.Antlr4Source;
    using OrbintSoft.Yauaa.Utils;

    /// <summary>
    /// This class defines the WordRange Step, it is used in parsing to extract a substring of words from the actual value.
    /// </summary>
    [Serializable]
    public class StepWordRange : Step
    {
        /// <summary>
        /// Defines the index of firstWord.
        /// </summary>
        private readonly int firstWord;

        /// <summary>
        /// Defines the index of the lastWord.
        /// </summary>
        private readonly int lastWord;

        /// <summary>
        /// Initializes a new instance of the <see cref="StepWordRange"/> class.
        /// This is used only for binary deserialization.
        /// </summary>
        /// <param name="info">The info <see cref="SerializationInfo"/>.</param>
        /// <param name="context">The context <see cref="StreamingContext"/>.</param>
        public StepWordRange(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.firstWord = (int)info.GetValue(nameof(this.firstWord), typeof(int));
            this.lastWord = (int)info.GetValue(nameof(this.lastWord), typeof(int));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepWordRange"/> class.
        /// </summary>
        /// <param name="range">The range of words <see cref=" WordRangeVisitor.Range"/>.</param>
        public StepWordRange(WordRangeVisitor.Range range)
        {
            this.firstWord = range.First;
            this.lastWord = range.Last;
        }

        /// <summary>
        /// It reurns true if it can fail.
        /// </summary>
        /// <returns>True or false.</returns>
        public override bool CanFail()
        {
            // If you want the first word it cannot fail.
            // For all other numbers it can.
            return !(this.firstWord == 1 && this.lastWord == 1);
        }

        /// <summary>
        /// This is used for binary serialization.
        /// Populates a SerializationInfo with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(this.firstWord), this.firstWord, typeof(int));
            info.AddValue(nameof(this.lastWord), this.lastWord, typeof(int));
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"WordRange([{this.firstWord}:{this.lastWord}])";
        }

        /// <summary>
        /// It finds a substring from the actual value between a frirs word and a last word range.
        /// Then it will walk into the tree and recurse through all the remaining steps or reutn null if no range.
        /// </summary>
        /// <param name="tree">The tree to walk into.</param>
        /// <param name="value">The actual value of the node or null to get the root.</param>
        /// <returns>Either null or the actual value that was found.</returns>
        public override WalkList.WalkResult Walk(IParseTree tree, string value)
        {
            var actualValue = this.GetActualValue(tree, value);
            string filteredValue;
            if (tree.ChildCount == 1)
            {
                var child = tree.GetChild(0);
                if (child is UserAgentParser.SingleVersionContext || child is UserAgentParser.SingleVersionWithCommasContext)
                {
                    filteredValue = VersionSplitter.Instance.GetSplitRange(actualValue, this.firstWord, this.lastWord);
                }
                else
                {
                    filteredValue = WordSplitter.GetInstance().GetSplitRange(actualValue, this.firstWord, this.lastWord);
                }
            }
            else
            {
                filteredValue = WordSplitter.GetInstance().GetSplitRange(actualValue, this.firstWord, this.lastWord);
            }

            if (filteredValue is null)
            {
                return null;
            }
            else
            {
                return this.WalkNextStep(tree, filteredValue);
            }
        }
    }
}
