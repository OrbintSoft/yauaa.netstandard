//-----------------------------------------------------------------------
// <copyright file="StepWordRange.cs" company="OrbintSoft">
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

namespace OrbintSoft.Yauaa.Analyze.TreeWalker.Steps.Value
{
    using Antlr4.Runtime.Tree;
    using OrbintSoft.Yauaa.Antlr4Source;
    using OrbintSoft.Yauaa.Utils;
    using System;

    /// <summary>
    /// Defines the <see cref="StepWordRange" />
    /// </summary>
    [Serializable]
    public class StepWordRange : Step
    {
        /// <summary>
        /// Defines the firstWord
        /// </summary>
        private readonly int firstWord;

        /// <summary>
        /// Defines the lastWord
        /// </summary>
        private readonly int lastWord;

        /// <summary>
        /// Initializes a new instance of the <see cref="StepWordRange"/> class.
        /// </summary>
        /// <param name="range">The range<see cref="WordRangeVisitor.Range"/></param>
        public StepWordRange(WordRangeVisitor.Range range)
        {
            this.firstWord = range.First;
            this.lastWord = range.Last;
        }

        /// <summary>
        /// The CanFail
        /// </summary>
        /// <returns>The <see cref="bool"/></returns>
        public override bool CanFail()
        {
            // If you want the first word it cannot fail.
            // For all other numbers it can.
            return !(this.firstWord == 1 && this.lastWord == 1);
        }

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            return "WordRange([" + this.firstWord + ":" + this.lastWord + "])";
        }

        /// <summary>
        /// The Walk
        /// </summary>
        /// <param name="tree">The tree<see cref="IParseTree"/></param>
        /// <param name="value">The value<see cref="string"/></param>
        /// <returns>The <see cref="WalkList.WalkResult"/></returns>
        public override WalkList.WalkResult Walk(IParseTree tree, string value)
        {
            var actualValue = this.GetActualValue(tree, value);
            string filteredValue;
            if (tree.ChildCount == 1 && (
                  tree.GetChild(0) is UserAgentParser.SingleVersionContext ||
                  tree.GetChild(0) is UserAgentParser.SingleVersionWithCommasContext))
            {
                filteredValue = VersionSplitter.GetInstance().GetSplitRange(actualValue, this.firstWord, this.lastWord);
            }
            else
            {
                filteredValue = WordSplitter.GetInstance().GetSplitRange(actualValue, this.firstWord, this.lastWord);
            }

            if (filteredValue == null)
            {
                return null;
            }

            return this.WalkNextStep(tree, filteredValue);
        }
    }
}
