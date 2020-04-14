//-----------------------------------------------------------------------
// <copyright file="StepNext.cs" company="OrbintSoft">
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
// <summary></summary>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Analyze.TreeWalker.Steps.Walk
{
    using System;
    using System.Runtime.Serialization;
    using Antlr4.Runtime.Tree;

    /// <summary>
    /// This class defines the Next Step, it is used in parsing to go to the next node.
    /// </summary>
    [Serializable]
    public class StepNext : Step
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StepNext"/> class.
        /// </summary>
        public StepNext()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepNext"/> class.
        /// This is used only for binary deserialization.
        /// </summary>
        /// <param name="info">The info <see cref="SerializationInfo"/>.</param>
        /// <param name="context">The context <see cref="StreamingContext"/>.</param>
        public StepNext(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "Next(1)";
        }

        /// <summary>
        /// If there is a next node to the current walks to that.
        /// </summary>
        /// <param name="tree">The tree to walk into.</param>
        /// <param name="value">Not used.</param>
        /// <returns>Either null or the actual value that was found.</returns>
        public override WalkList.WalkResult Walk(IParseTree tree, string value)
        {
            var nextTree = this.Next(tree);
            if (nextTree == null)
            {
                return null;
            }

            return this.WalkNextStep(nextTree, null);
        }

        /// <summary>
        /// Tries tio find the next node to the current.
        /// </summary>
        /// <param name="tree">The tree<see cref="IParseTree"/>.</param>
        /// <returns>The next node.</returns>
        private IParseTree Next(IParseTree tree)
        {
            var parent = this.Up(tree);
            IParseTree child;
            var foundCurrent = false;
            for (var i = 0; i < parent.ChildCount; i++)
            {
                child = parent.GetChild(i);
                if (foundCurrent)
                {
                    if (TreeIsSeparator(child))
                    {
                        continue;
                    }

                    return child;
                }

                if (child == tree)
                {
                    foundCurrent = true;
                }
            }

            return null; // There is no next
        }
    }
}
