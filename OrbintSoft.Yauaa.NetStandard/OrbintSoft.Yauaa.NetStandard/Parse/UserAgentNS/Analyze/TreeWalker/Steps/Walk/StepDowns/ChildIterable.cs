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

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Collections;
using System.Collections.Generic;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze.TreeWalker.Steps.Walk.StepDowns
{
    public class ChildIterable
    {
        private readonly bool privateNumberRange;
        private readonly int start;
        private readonly int end;

        private readonly Predicate<ParserRuleContext> isWantedClassPredicate;

        public ChildIterable(bool privateNumberRange, int start, int end, Predicate<ParserRuleContext> isWantedClassPredicate)
        {
            this.privateNumberRange = privateNumberRange;
            this.start = start;
            this.end = end;
            this.isWantedClassPredicate = isWantedClassPredicate;
        }

        public IEnumerator<ParserRuleContext> Iterator(ParserRuleContext treeContext)
        {
            return new ChildIterator(this, treeContext);
        }

        internal class ChildIterator : IEnumerator<ParserRuleContext> {
            private readonly IEnumerator<IParseTree> childIterator;
            private int index = -1;
            private readonly ChildIterable childIterable;

            public ParserRuleContext Current { get; private set; } = null;

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            internal ChildIterator(ChildIterable childIterable, ParserRuleContext treeContext)
            {
                this.childIterable = childIterable;
                if (treeContext.children == null)
                {
                    childIterator = null;
                    Current = null;
                    index = -1;
                }
                else
                {
                    childIterator = treeContext.children.GetEnumerator();
                }
            }

            /// <summary>
            /// Find and set the nextChild
            /// </summary>
            /// <returns>If there is a next</returns>
            public bool MoveNext()
            {
                while (childIterator.MoveNext())
                {
                    IParseTree nextParseTree = childIterator.Current;
                    if (Step.TreeIsSeparator(nextParseTree))
                    {
                        continue;
                    }
                    if (!(nextParseTree is ParserRuleContext)) {
                        continue;
                    }
                    if (!childIterable.privateNumberRange)
                    {
                        index++;
                    }
                    ParserRuleContext possibleNextChild = (ParserRuleContext)nextParseTree;
                    if (!childIterable.isWantedClassPredicate(possibleNextChild))
                    {
                        continue;
                    }
                    if (childIterable.privateNumberRange)
                    {
                        index++;
                    }
                    if (index + 1 > childIterable.end)
                    {
                        Current = null;
                        return false;
                    }
                    if (childIterable.start <= index + 1)
                    {
                        Current = possibleNextChild;
                        return true;
                    }
                }

                // We found nothing
                Current = null;
                return false;
            }
        
            public void Reset()
            {
                index = -1;
                Current = null;
                childIterator.Reset();
            }

            public void Dispose()
            {
                childIterator.Dispose();
            }
        }
    }
}
