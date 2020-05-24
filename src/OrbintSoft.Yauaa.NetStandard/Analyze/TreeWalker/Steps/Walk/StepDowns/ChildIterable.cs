//-----------------------------------------------------------------------
// <copyright file="ChildIterable.cs" company="OrbintSoft">
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

namespace OrbintSoft.Yauaa.Analyze.TreeWalker.Steps.Walk.StepDowns
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Antlr4.Runtime;
    using Antlr4.Runtime.Tree;

    /// <summary>
    /// This class is used to iterate throught the childres of a <see cref="IParseTree"/>.
    /// </summary>
    public class ChildIterable
    {
        /// <summary>
        /// Defines the start index of the interation.
        /// </summary>
        private readonly int start;

        /// <summary>
        /// Defines the end index of the interation.
        /// </summary>
        private readonly int end;

        /// <summary>
        /// Defines a predicate to filter the wanted node type.
        /// </summary>
        private readonly Predicate<IParseTree> isWantedClassPredicate;

        /// <summary>
        /// Defines if the range is private.
        /// </summary>
        private readonly bool privateNumberRange;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildIterable"/> class.
        /// </summary>
        /// <param name="privateNumberRange">Defines if the range to iterate is private.</param>
        /// <param name="start">The start index of iteration.</param>
        /// <param name="end">The end index of the iteration.</param>
        /// <param name="isWantedClassPredicate">A predicate to filter only the wanted classes.</param>
        public ChildIterable(bool privateNumberRange, int start, int end, Predicate<IParseTree> isWantedClassPredicate)
        {
            this.privateNumberRange = privateNumberRange;
            this.start = start;
            this.end = end;
            this.isWantedClassPredicate = isWantedClassPredicate;
        }

        /// <summary>
        /// It returns the enumerator by the current parser rule of tree context.
        /// </summary>
        /// <param name="treeContext">The tree context<see cref="ParserRuleContext"/>.</param>
        /// <returns>The enumerator of the tree.</returns>
        public IEnumerator<IParseTree> GetEnumerator(ParserRuleContext treeContext)
        {
            return new ChildEnumerator(this, treeContext);
        }

        /// <summary>
        /// This class is used as the enumerator of a <see cref="IParseTree"/>.
        /// </summary>
        internal class ChildEnumerator : IEnumerator<IParseTree>
        {
            /// <summary>
            /// Defines private iterator of child.
            /// </summary>
            private readonly IEnumerator<IParseTree> childIter;

            /// <summary>
            /// Defines the referenced child iterable class.
            /// </summary>
            private readonly ChildIterable childIterable;

            /// <summary>
            /// Defines the current index, -1 as default because iteration has not started.
            /// </summary>
            private int index = -1;

            /// <summary>
            /// True if I alread iterated all the nodes in the tree context.
            /// </summary>
            private bool endReached = false;

            /// <summary>
            /// Initializes a new instance of the <see cref="ChildEnumerator"/> class.
            /// This class is used to initialize the enumerator.
            /// </summary>
            /// <param name="childIterable">The referenced <see cref="ChildIterable"/> class.</param>
            /// <param name="treeContext">The tree context, <see cref="ParserRuleContext"/>.</param>
            internal ChildEnumerator(ChildIterable childIterable, ParserRuleContext treeContext)
            {
                this.childIterable = childIterable;
                if (treeContext.children is null)
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
            /// Gets the current node.
            /// </summary>
            public IParseTree Current { get; private set; } = null;

            /// <summary>
            /// Gets the current node.
            /// </summary>
            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }

            /// <summary>
            /// It is used to dispose the enumerator.
            /// </summary>
            public void Dispose()
            {
                if (this.childIter != null)
                {
                    this.childIter.Dispose();
                }
            }

            /// <summary>
            /// Find and set the nextChild.
            /// </summary>
            /// <returns>If there is a next.</returns>
            public bool MoveNext()
            {
                if (this.childIter != null && !this.endReached)
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
            /// I reset the enumerator, so I can restart the iteration.
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
