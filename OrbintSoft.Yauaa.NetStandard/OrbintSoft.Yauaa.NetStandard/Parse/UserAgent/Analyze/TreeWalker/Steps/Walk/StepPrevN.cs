/*
 * Yet Another UserAgent Analyzer .NET Standard
 * Porting realized by Balzarotti Stefano, Copyright (C) OrbintSoft
 * 
 * Original Author and License:
 * 
 * Yet Another UserAgent Analyzer
 * Copyright (C) 2013-2018 Niels Basjes
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * All rights should be reserved to the original author Niels Basjes
 */

using Antlr4.Runtime.Tree;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgent.Analyze.TreeWalker.Steps.Walk
{
    public class StepPrevN: Step
    {
        private static readonly int SIZE = 20;
        readonly IParseTree[] children = new IParseTree[SIZE];

        private readonly int steps;

        public StepPrevN(int steps)
        {
            this.steps = steps;
        }

        private IParseTree Prev(IParseTree tree)
        {
            IParseTree parent = Up(tree);

            int lastChildIndex = -1;
            IParseTree child = null;
            int i;
            for (i = 0; i < parent.ChildCount; i++)
            {
                if (!TreeIsSeparator(child))
                {
                    lastChildIndex++;
                    children[lastChildIndex] = child;
                }
                child = parent.GetChild(i);
                if (child == tree)
                {
                    if (lastChildIndex < steps)
                    {
                        break; // There is no previous
                    }
                    return children[lastChildIndex - steps + 1];
                }
            }
            return null; // There is no previous
        }

        public override WalkList.WalkResult Walk(IParseTree tree, string value)
        {
            IParseTree nextTree = Prev(tree);
            if (nextTree == null)
            {
                return null;
            }

            return WalkNextStep(nextTree, null);
        }

        public override string ToString()
        {
            return "Prev(" + steps + ")";
        }
    }
}
