//-----------------------------------------------------------------------
// <copyright file="NumberRangeList.cs" company="OrbintSoft">
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

namespace OrbintSoft.Yauaa.Analyze
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the <see cref="NumberRangeList" />
    /// </summary>
    public class NumberRangeList : IReadOnlyList<int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NumberRangeList"/> class.
        /// </summary>
        /// <param name="start">The start<see cref="int"/></param>
        /// <param name="end">The end<see cref="int"/></param>
        public NumberRangeList(int start, int end)
        {
            this.Start = start;
            this.End = end;
        }

        /// <summary>
        /// Gets the Count
        /// </summary>
        public int Count => this.End - this.Start + 1;

        /// <summary>
        /// Gets the Start
        /// </summary>
        public int Start { get; }

        /// <summary>
        /// Gets the End
        /// </summary>
        public int End { get; }

        /// <summary>
        /// The element of list
        /// </summary>
        /// <param name="index">The index of the element</param>
        /// <returns>The element in position index</returns>
        public int this[int index] => this.Start + index;

        /// <summary>
        /// The GetEnumerator
        /// </summary>
        /// <returns>The enumerator</returns>
        public IEnumerator<int> GetEnumerator()
        {
            return new NumberRangeEnumerator(this);
        }

        /// <summary>
        /// The GetEnumerator
        /// </summary>
        /// <returns>The <see cref="IEnumerator"/></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Defines the <see cref="NumberRangeEnumerator" />
        /// </summary>
        public class NumberRangeEnumerator : IEnumerator<int>
        {
            /// <summary>
            /// Defines the list
            /// </summary>
            private readonly NumberRangeList list;

            /// <summary>
            /// Defines the offset
            /// </summary>
            private int offset = -1;

            /// <summary>
            /// Initializes a new instance of the <see cref="NumberRangeEnumerator"/> class.
            /// </summary>
            /// <param name="list">The list<see cref="NumberRangeList"/></param>
            public NumberRangeEnumerator(NumberRangeList list)
            {
                this.list = list;
            }

            /// <summary>
            /// Gets the Current
            /// </summary>
            public int Current => this.list[this.offset];

            /// <summary>
            /// Gets the Current
            /// </summary>
            object IEnumerator.Current => this.list[this.offset];

            /// <summary>
            /// The Dispose
            /// </summary>
            public void Dispose()
            {
            }

            /// <summary>
            /// The MoveNext
            /// </summary>
            /// <returns>The <see cref="bool"/></returns>
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
            /// The Reset
            /// </summary>
            public void Reset()
            {
                this.offset = -1;
            }
        }
    }
}
