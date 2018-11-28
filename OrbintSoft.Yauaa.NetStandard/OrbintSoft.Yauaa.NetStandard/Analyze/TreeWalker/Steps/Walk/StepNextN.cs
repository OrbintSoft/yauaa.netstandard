﻿//-----------------------------------------------------------------------
// <copyright file="StepNextN.cs" company="OrbintSoft">
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
    using Antlr4.Runtime.Tree;
    using System;

    /// <summary>
    /// Defines the <see cref="StepNextN" />
    /// </summary>
    [Serializable]
    public class StepNextN : Step
    {
        /// <summary>
        /// Defines the steps
        /// </summary>
        private readonly int steps;

        /// <summary>
        /// Initializes a new instance of the <see cref="StepNextN"/> class.
        /// </summary>
        /// <param name="steps">The steps<see cref="int"/></param>
        public StepNextN(int steps)
        {
            this.steps = steps;
        }

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            return "Next(" + this.steps + ")";
        }

        /// <summary>
        /// The Walk
        /// </summary>
        /// <param name="tree">The tree<see cref="IParseTree"/></param>
        /// <param name="value">The value<see cref="string"/></param>
        /// <returns>The <see cref="WalkList.WalkResult"/></returns>
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
        /// The Next
        /// </summary>
        /// <param name="tree">The tree<see cref="IParseTree"/></param>
        /// <returns>The <see cref="IParseTree"/></returns>
        private IParseTree Next(IParseTree tree)
        {
            var parent = this.Up(tree);
            IParseTree child;
            var foundCurrent = false;
            var stepsToDo = this.steps;
            for (var i = 0; i < parent.ChildCount; i++)
            {
                child = parent.GetChild(i);
                if (foundCurrent)
                {
                    if (TreeIsSeparator(child))
                    {
                        continue;
                    }

                    stepsToDo--;
                    if (stepsToDo == 0)
                    {
                        return child;
                    }
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
