//-----------------------------------------------------------------------
// <copyright file="StepPrevN.cs" company="OrbintSoft">
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
    /// This class defines the PrevN Step, it is used in parsing to go to walk with N steps to the N previous node.
    /// </summary>
    [Serializable]
    public class StepPrevN : Step
    {
        /// <summary>
        /// Defines the size of the array of children.
        /// </summary>
        private const int SIZE = 20;

        /// <summary>
        /// The number of steps to walk to the Previous N node.
        /// </summary>
        private readonly int steps;

        /// <summary>
        /// The children of the tree.
        /// </summary>
        [NonSerialized]
        private IParseTree[] children = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="StepPrevN"/> class.
        /// </summary>
        /// <param name="steps">The steps<see cref="int"/>.</param>
        public StepPrevN(int steps)
        {
            this.steps = steps;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepPrevN"/> class.
        /// This is used only for binary deserialization.
        /// </summary>
        /// <param name="info">The info <see cref="SerializationInfo"/>.</param>
        /// <param name="context">The context <see cref="StreamingContext"/>.</param>
        public StepPrevN(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.steps = (int)info.GetValue(nameof(this.steps), typeof(int));
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
            info.AddValue(nameof(this.steps), this.steps, typeof(int));
        }

        /// <summary>
        /// The ToString.
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        public override string ToString()
        {
            return $"Prev({this.steps})";
        }

        /// <summary>
        /// If there are N previuos nodes to the current walks to N node.
        /// </summary>
        /// <param name="tree">The tree to walk into.</param>
        /// <param name="value">Not used.</param>
        /// <returns>Either null or the actual value that was found.</returns>
        public override WalkList.WalkResult Walk(IParseTree tree, string value)
        {
            var prevTree = this.Prev(tree);
            if (prevTree is null)
            {
                return null;
            }

            return this.WalkNextStep(prevTree, null);
        }

        /// <summary>
        /// Tries to find the previuos node with N steps to the current.
        /// </summary>
        /// <param name="tree">The tree<see cref="IParseTree"/>.</param>
        /// <returns>The previuos N node.</returns>
        private IParseTree Prev(IParseTree tree)
        {
            var parent = this.Up(tree);

            if (this.children is null)
            {
                this.children = new IParseTree[SIZE];
            }

            var lastChildIndex = -1;
            IParseTree child = null;
            int i;
            for (i = 0; i < parent.ChildCount; i++)
            {
                if (!TreeIsSeparator(child))
                {
                    lastChildIndex++;
                    this.children[lastChildIndex] = child;
                }

                child = parent.GetChild(i);
                if (child == tree)
                {
                    if (lastChildIndex < this.steps)
                    {
                        break; // There is no previous
                    }

                    return this.children[lastChildIndex - this.steps + 1];
                }
            }

            return null; // There is no previous
        }
    }
}
