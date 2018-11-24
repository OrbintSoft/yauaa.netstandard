//<copyright file="StepPrev.cs" company="OrbintSoft">
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
//<date>2018, 8, 16, 02:43</date>
//<summary></summary>

namespace OrbintSoft.Yauaa.Analyze.TreeWalker.Steps.Walk
{
    using Antlr4.Runtime.Tree;
    using System;

    /// <summary>
    /// Defines the <see cref="StepPrev" />
    /// </summary>
    [Serializable]
    public class StepPrev : Step
    {
        /// <summary>
        /// The Prev
        /// </summary>
        /// <param name="tree">The tree<see cref="IParseTree"/></param>
        /// <returns>The <see cref="IParseTree"/></returns>
        private IParseTree Prev(IParseTree tree)
        {
            IParseTree parent = Up(tree);

            IParseTree prevChild = null;
            IParseTree child = null;
            int i;
            for (i = 0; i < parent.ChildCount; i++)
            {
                if (!TreeIsSeparator(child))
                {
                    prevChild = child;
                }
                child = parent.GetChild(i);
                if (child == tree)
                {
                    break;
                }
            }
            return prevChild;
        }

        /// <summary>
        /// The Walk
        /// </summary>
        /// <param name="tree">The tree<see cref="IParseTree"/></param>
        /// <param name="value">The value<see cref="string"/></param>
        /// <returns>The <see cref="WalkList.WalkResult"/></returns>
        public override WalkList.WalkResult Walk(IParseTree tree, string value)
        {
            IParseTree prevTree = Prev(tree);
            if (prevTree == null)
            {
                return null;
            }

            return WalkNextStep(prevTree, null);
        }

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            return "Prev(1)";
        }
    }
}
