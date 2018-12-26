//-----------------------------------------------------------------------
// <copyright file="MatchesList.cs" company="OrbintSoft">
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
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Antlr4.Runtime.Tree;

    /// <summary>
    /// Defines the <see cref="MatchesList" />
    /// </summary>
    [Serializable]
    public sealed class MatchesList : ICollection<MatchesList.Match>
    {
        /// <summary>
        /// Defines the CAPACITY_INCREASE
        /// </summary>
        private const int CAPACITY_INCREASE = 3;

        /// <summary>
        /// Defines the allElements
        /// </summary>
        private Match[] allElements = null;

        /// <summary>
        /// Defines the maxSize
        /// </summary>
        private int maxSize = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchesList"/> class.
        /// </summary>
        /// <param name="newMaxSize">The newMaxSize<see cref="int"/></param>
        public MatchesList(int newMaxSize)
        {
            this.maxSize = newMaxSize;

            this.Count = 0;
            this.allElements = new Match[this.maxSize];
            for (var i = 0; i < this.maxSize; i++)
            {
                this.allElements[i] = new Match(null, null, null);
            }
        }

        /// <summary>
        /// Gets the Count
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Gets a value indicating whether IsReadOnly
        /// </summary>
        public bool IsReadOnly => true;

        /// <summary>
        /// The Add
        /// </summary>
        /// <param name="item">The item<see cref="Match"/></param>
        public void Add(Match item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The Add
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="value">The value<see cref="string"/></param>
        /// <param name="result">The result<see cref="IParseTree"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public bool Add(string key, string value, IParseTree result)
        {
            if (this.Count >= this.maxSize)
            {
                this.IncreaseCapacity();
            }

            this.allElements[this.Count].Fill(key, value, result);
            this.Count++;
            return true;
        }

        /// <summary>
        /// The Clear
        /// </summary>
        public void Clear()
        {
            this.Count = 0;
        }

        /// <summary>
        /// The Contains
        /// </summary>
        /// <param name="item">The item<see cref="Match"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public bool Contains(Match item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The CopyTo
        /// </summary>
        /// <param name="array">The array</param>
        /// <param name="arrayIndex">The arrayIndex</param>
        public void CopyTo(Match[] array, int arrayIndex)
        {
            for (var i = 0; i < this.Count; i++)
            {
                array.SetValue(this.allElements[i], arrayIndex);
                arrayIndex = arrayIndex + 1;
            }
        }

        /// <summary>
        /// The GetEnumerator
        /// </summary>
        /// <returns>The <see cref="IEnumerator{Match}"/></returns>
        public IEnumerator<Match> GetEnumerator()
        {
            return new MatchEnumerator(this.allElements, this.Count);
        }

        /// <summary>
        /// The Remove
        /// </summary>
        /// <param name="item">The item<see cref="Match"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public bool Remove(Match item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The ToStrings
        /// </summary>
        /// <returns>The list of strings</returns>
        public IList<string> ToStrings()
        {
            var result = new List<string>();
            foreach (var match in this)
            {
                result.Add("{ \"" + match.Key + "\"=\"" + match.Value + "\" }");
            }

            return result;
        }

        /// <summary>
        /// The GetEnumerator
        /// </summary>
        /// <returns>The <see cref="IEnumerator"/></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new MatchEnumerator(this.allElements, this.Count);
        }

        /// <summary>
        /// The IncreaseCapacity
        /// </summary>
        private void IncreaseCapacity()
        {
            var newMaxSize = this.maxSize + CAPACITY_INCREASE;
            var newAllElements = new Match[newMaxSize];
            this.allElements.CopyTo(newAllElements, 0);
            for (var i = this.maxSize; i < newMaxSize; i++)
            {
                newAllElements[i] = new Match(null, null, null);
            }

            this.allElements = newAllElements;
            this.maxSize = newMaxSize;
        }

        /// <summary>
        /// Defines the <see cref="Match" />
        /// </summary>
        [Serializable]
        public class Match
        {
            /// <summary>
            /// Defines the result
            /// </summary>
            private IParseTree result = null;

            /// <summary>
            /// Initializes a new instance of the <see cref="Match"/> class.
            /// </summary>
            /// <param name="key">The key<see cref="string"/></param>
            /// <param name="value">The value<see cref="string"/></param>
            /// <param name="result">The result<see cref="IParseTree"/></param>
            public Match(string key, string value, IParseTree result)
            {
                this.Fill(key, value, result);
            }

            /// <summary>
            /// Gets the Key
            /// </summary>
            public string Key { get; private set; } = null;

            /// <summary>
            /// Gets the Value
            /// </summary>
            public string Value { get; private set; } = null;

            /// <summary>
            /// The Fill
            /// </summary>
            /// <param name="key">The nKey<see cref="string"/></param>
            /// <param name="value">The nValue<see cref="string"/></param>
            /// <param name="result">The nResult<see cref="IParseTree"/></param>
            public void Fill(string key, string value, IParseTree result)
            {
                this.Key = key;
                this.Value = value;
                this.result = result;
            }

            /// <summary>
            /// The GetResult
            /// </summary>
            /// <returns>The <see cref="IParseTree"/></returns>
            public IParseTree GetResult()
            {
                return this.result;
            }
        }

        /// <summary>
        /// Defines the <see cref="MatchEnumerator" />
        /// </summary>
        public class MatchEnumerator : IEnumerator<Match>
        {
            /// <summary>
            /// Defines the count
            /// </summary>
            private readonly int count = 0;

            /// <summary>
            /// Defines the matches
            /// </summary>
            private readonly Match[] matches;

            /// <summary>
            /// Defines the offset
            /// </summary>
            private int offset = -1;

            /// <summary>
            /// Initializes a new instance of the <see cref="MatchEnumerator"/> class.
            /// </summary>
            /// <param name="matches">The matches</param>
            /// <param name="count">The count<see cref="int"/></param>
            public MatchEnumerator(Match[] matches, int count)
            {
                this.matches = matches;
                this.count = count;
            }

            /// <summary>
            /// Gets the Current
            /// </summary>
            public Match Current
            {
                get
                {
                    if (this.count > this.offset)
                    {
                        return this.matches[this.offset];
                    }
                    else
                    {
                        throw new IndexOutOfRangeException("Array index out of bounds");
                    }
                }
            }

            /// <summary>
            /// Gets the Current
            /// </summary>
            object IEnumerator.Current => this.Current;

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
                this.offset++;
                if (this.offset < this.count)
                {
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
