//-----------------------------------------------------------------------
// <copyright file="StepPrevN.cs" company="OrbintSoft">
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

namespace OrbintSoft.Yauaa.Analyze.TreeWalker.Steps.Walk
{
    using System;
    using System.Runtime.Serialization;
    using Antlr4.Runtime.Tree;

    /// <summary>
    /// Defines the <see cref="StepPrevN" />.
    /// </summary>
    [Serializable]
    public class StepPrevN : Step
    {
        /// <summary>
        /// Defines the SIZE.
        /// </summary>
        private const int SIZE = 20;

        /// <summary>
        /// Defines the steps.
        /// </summary>
        private readonly int steps;

        /// <summary>
        /// Defines the children.
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
        /// </summary>
        /// <param name="info">The info<see cref="SerializationInfo"/>.</param>
        /// <param name="context">The context<see cref="StreamingContext"/>.</param>
        public StepPrevN(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.steps = (int)info.GetValue("steps", typeof(int));
        }

        /// <summary>
        /// The GetObjectData.
        /// </summary>
        /// <param name="info">The info<see cref="SerializationInfo"/>.</param>
        /// <param name="context">The context<see cref="StreamingContext"/>.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("steps", this.steps, typeof(int));
        }

        /// <summary>
        /// The ToString.
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        public override string ToString()
        {
            return "Prev(" + this.steps + ")";
        }

        /// <summary>
        /// The Walk.
        /// </summary>
        /// <param name="tree">The tree<see cref="IParseTree"/>.</param>
        /// <param name="value">The value<see cref="string"/>.</param>
        /// <returns>The <see cref="WalkList.WalkResult"/>.</returns>
        public override WalkList.WalkResult Walk(IParseTree tree, string value)
        {
            var prevTree = this.Prev(tree);
            if (prevTree == null)
            {
                return null;
            }

            return this.WalkNextStep(prevTree, null);
        }

        /// <summary>
        /// The Prev.
        /// </summary>
        /// <param name="tree">The tree<see cref="IParseTree"/>.</param>
        /// <returns>The <see cref="IParseTree"/>.</returns>
        private IParseTree Prev(IParseTree tree)
        {
            var parent = this.Up(tree);

            if (this.children == null)
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
