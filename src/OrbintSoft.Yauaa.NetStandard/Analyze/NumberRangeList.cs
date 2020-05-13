//-----------------------------------------------------------------------
// <copyright file="NumberRangeList.cs" company="OrbintSoft">
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
//   </copyright>
//   <author>Stefano Balzarotti, Niels Basjes</author>
// <date>2018, 11, 24, 12:48</date>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Analyze
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// This list respresents a range of numbers.
    /// </summary>
    public class NumberRangeList : IReadOnlyList<int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NumberRangeList"/> class.
        /// </summary>
        /// <param name="start">The start<see cref="int"/>.</param>
        /// <param name="end">The end<see cref="int"/>.</param>
        public NumberRangeList(int start, int end)
        {
            this.Start = start;
            this.End = end;
        }

        /// <inheritdoc/>
        public int Count => this.End - this.Start + 1;

        /// <summary>
        /// Gets the start of range.
        /// </summary>
        public int Start { get; }

        /// <summary>
        /// Gets the end of the range.
        /// </summary>
        public int End { get; }

        /// <summary>
        /// Gets a number in the range by an index.
        /// </summary>
        /// <param name="index">The index of the element.</param>
        /// <returns>The number in position index.</returns>
        public int this[int index] => this.Start + index;

        /// <summary>
        /// Returns an enumerator that iterates throught the range.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<int> GetEnumerator()
        {
            return new NumberRangeEnumerator(this);
        }

         /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Supports a simmple ieration over a <see cref="NumberRangeList"/>.
        /// </summary>
        public class NumberRangeEnumerator : IEnumerator<int>
        {
            /// <summary>
            /// Defines the list of numbers.
            /// </summary>
            private readonly NumberRangeList list;

            /// <summary>
            /// Defines the offset.
            /// </summary>
            private int offset = -1;

            /// <summary>
            /// Initializes a new instance of the <see cref="NumberRangeEnumerator"/> class.
            /// </summary>
            /// <param name="list">The list<see cref="NumberRangeList"/>.</param>
            public NumberRangeEnumerator(NumberRangeList list)
            {
                this.list = list;
            }

            /// <summary>
            /// Gets the current number in the range at the current position of the enumerator.
            /// </summary>
            public int Current => this.list[this.offset];

            /// <inheritdoc/>
            object IEnumerator.Current => this.list[this.offset];

            /// <inheritdoc/>
            public void Dispose()
            {
            }

            /// <summary>
            /// Advances the enumberator to the next element of the range.
            /// </summary>
            /// <returns>True if there is a next element.</returns>
            public bool MoveNext()
            {
                if (this.offset < this.list.Count - 1)
                {
                    this.offset++;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            /// <summary>
            /// Sets the enumerator at initial position, resets the enumerator.
            /// </summary>
            public void Reset()
            {
                this.offset = -1;
            }
        }
    }
}
