//-----------------------------------------------------------------------
// <copyright file="ChildIterable.cs" company="OrbintSoft">
// Yet Another User Agent Analyzer for .NET Standard
// porting realized by Stefano Balzarotti, Copyright 2019 (C) OrbintSoft
//
// Original Author and License:
//
// Yet Another UserAgent Analyzer
// Copyright(C) 2013-2019 Niels Basjes
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// <author>Stefano Balzarotti, Niels Basjes</author>
// <date>2018, 11, 24, 12:48</date>
// <summary></summary>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Analyze.TreeWalker.Steps.Walk.StepDowns
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Antlr4.Runtime;
    using Antlr4.Runtime.Tree;

    /// <summary>
    /// Defines the <see cref="ChildIterable" />.
    /// </summary>
    public class ChildIterable
    {
        /// <summary>
        /// Defines the end.
        /// </summary>
        private readonly int end;

        /// <summary>
        /// Defines the isWantedClassPredicate.
        /// </summary>
        private readonly Predicate<IParseTree> isWantedClassPredicate;

        /// <summary>
        /// Defines the privateNumberRange.
        /// </summary>
        private readonly bool privateNumberRange;

        /// <summary>
        /// Defines the start.
        /// </summary>
        private readonly int start;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildIterable"/> class.
        /// </summary>
        /// <param name="privateNumberRange">The privateNumberRange<see cref="bool"/>.</param>
        /// <param name="start">The start<see cref="int"/>.</param>
        /// <param name="end">The end<see cref="int"/>.</param>
        /// <param name="isWantedClassPredicate">The isWantedClassPredicate<see cref="Predicate{ParserRuleContext}"/>.</param>
        public ChildIterable(bool privateNumberRange, int start, int end, Predicate<IParseTree> isWantedClassPredicate)
        {
            this.privateNumberRange = privateNumberRange;
            this.start = start;
            this.end = end;
            this.isWantedClassPredicate = isWantedClassPredicate;
        }

        /// <summary>
        /// The Iterator.
        /// </summary>
        /// <param name="treeContext">The treeContext<see cref="ParserRuleContext"/>.</param>
        /// <returns>The <see cref="IEnumerator{IParseTree}"/>.</returns>
        public IEnumerator<IParseTree> Iterator(ParserRuleContext treeContext)
        {
            return new ChildIterator(this, treeContext);
        }

        /// <summary>
        /// Defines the <see cref="ChildIterator" />.
        /// </summary>
        internal class ChildIterator : IEnumerator<IParseTree>
        {
            /// <summary>
            /// Defines the childIter.
            /// </summary>
            private readonly IEnumerator<IParseTree> childIter;

            /// <summary>
            /// Defines the childIterable.
            /// </summary>
            private readonly ChildIterable childIterable;

            /// <summary>
            /// Defines the index.
            /// </summary>
            private int index = -1;

            /// <summary>
            /// True if I alread iterated all the tree.
            /// </summary>
            private bool endReached = false;

            /// <summary>
            /// Initializes a new instance of the <see cref="ChildIterator"/> class.
            /// </summary>
            /// <param name="childIterable">The childIterable<see cref="ChildIterable"/>.</param>
            /// <param name="treeContext">The treeContext<see cref="ParserRuleContext"/>.</param>
            internal ChildIterator(ChildIterable childIterable, ParserRuleContext treeContext)
            {
                this.childIterable = childIterable;
                if (treeContext.children == null)
                {
                    this.childIter = null;
                    this.Current = null;
                    this.index = -1;
                }
                else
                {
                    this.childIter = treeContext.children.GetEnumerator();
                }
            }

            /// <summary>
            /// Gets the Current.
            /// </summary>
            public IParseTree Current { get; private set; } = null;

            /// <summary>
            /// Gets the Current.
            /// </summary>
            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }

            /// <summary>
            /// The Dispose.
            /// </summary>
            public void Dispose()
            {
                this.childIter.Dispose();
            }

            /// <summary>
            /// Find and set the nextChild.
            /// </summary>
            /// <returns>If there is a next.</returns>
            public bool MoveNext()
            {
                if (this.childIter != null)
                {
                    while (this.childIter.MoveNext())
                    {
                        var nextParseTree = this.childIter.Current;
                        if (Step.TreeIsSeparator(nextParseTree))
                        {
                            continue;
                        }

                        if (!this.childIterable.privateNumberRange)
                        {
                            this.index++;
                        }

                        if (!this.childIterable.isWantedClassPredicate(nextParseTree))
                        {
                            continue;
                        }

                        if (this.childIterable.privateNumberRange)
                        {
                            this.index++;
                        }

                        if (this.index + 1 > this.childIterable.end)
                        {
                            this.Current = null;
                            return false;
                        }

                        if (this.childIterable.start <= this.index + 1)
                        {
                            this.Current = nextParseTree;
                            return true;
                        }
                    }
                }

                // We found nothing
                this.Current = null;
                this.endReached = true;
                return false;
            }

            /// <summary>
            /// The Reset.
            /// </summary>
            public void Reset()
            {
                this.index = -1;
                this.Current = null;
                this.endReached = false;
                this.childIter.Reset();
            }
        }
    }
}
