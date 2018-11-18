//<copyright file="NumberRangeList.cs" company="OrbintSoft">
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
//<date>2018, 8, 13, 14:56</date>
//<summary></summary>

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the <see cref="NumberRangeList" />
    /// </summary>
    public class NumberRangeList : IReadOnlyList<int>
    {
        /// <summary>
        /// Gets the Count
        /// </summary>
        public int Count => End - Start + 1;


        public int this[int index] => Start + index;
        /// <summary>
        /// Initializes a new instance of the <see cref="NumberRangeList"/> class.
        /// </summary>
        /// <param name="start">The start<see cref="int"/></param>
        /// <param name="end">The end<see cref="int"/></param>
        public NumberRangeList(int start, int end)
        {
            Start = start;
            End = end;
        }

        /// <summary>
        /// Gets the Start
        /// </summary>
        public int Start { get; }

        /// <summary>
        /// Gets the End
        /// </summary>
        public int End { get; }

        /// <summary>
        /// Defines the <see cref="NumberRangeEnumerator" />
        /// </summary>
        public class NumberRangeEnumerator : IEnumerator<int>
        {
            /// <summary>
            /// Defines the list
            /// </summary>
            internal readonly NumberRangeList list;

            /// <summary>
            /// Defines the offset
            /// </summary>
            internal int offset = -1;

            /// <summary>
            /// Gets the Current
            /// </summary>
            public int Current => list[offset];

            /// <summary>
            /// Gets the Current
            /// </summary>
            object IEnumerator.Current => list[offset];

            /// <summary>
            /// Initializes a new instance of the <see cref="NumberRangeEnumerator"/> class.
            /// </summary>
            /// <param name="list">The list<see cref="NumberRangeList"/></param>
            public NumberRangeEnumerator(NumberRangeList list)
            {
                this.list = list;
            }

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
                if (offset < list.Count - 1)
                {
                    offset++;
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
                offset = -1;
            }
        }

        /// <summary>
        /// The GetEnumerator
        /// </summary>
        /// <returns>The <see cref="IEnumerator{int}"/></returns>
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
            return GetEnumerator();
        }
    }
}
