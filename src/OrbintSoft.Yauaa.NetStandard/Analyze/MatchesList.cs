//-----------------------------------------------------------------------
// <copyright file="MatchesList.cs" company="OrbintSoft">
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

namespace OrbintSoft.Yauaa.Analyze
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Antlr4.Runtime.Tree;

    /// <summary>
    /// This class defines an efficient collection od matches.
    /// </summary>
    [Serializable]
    public sealed class MatchesList : IReadOnlyCollection<MatchesList.Match>
    {
        /// <summary>
        /// Defines the capacity increase of the array.
        /// </summary>
        private const int CAPACITY_INCREASE = 3;

        /// <summary>
        /// Defines the array of matches.
        /// </summary>
        private Match[] allElements = null;

        /// <summary>
        /// Defines the max size of the array.
        /// </summary>
        private int maxSize = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchesList"/> class.
        /// </summary>
        /// <param name="newMaxSize">The new max size.</param>
        public MatchesList(int newMaxSize)
        {
            this.maxSize = newMaxSize;

            this.Initialize();
        }

        /// <summary>
        /// Gets the number of matches contained in the collection.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this collecion is read only.
        /// </summary>
        public bool IsReadOnly => true;

        /// <summary>
        /// Gets a list of string representations of the matches in the collection.
        /// </summary>
        /// <returns>The list of strings.</returns>
        public IList<string> ToStrings()
        {
            var result = new List<string>();
            foreach (var match in this)
            {
                result.Add($"{{\"{match.Key}\"=\"{match.Value}\"}}");
            }

            return result;
        }

        /// <summary>
        /// Gets the enumerator for the collection.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<Match> GetEnumerator()
        {
            return new MatchEnumerator(this.allElements, this.Count);
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new MatchEnumerator(this.allElements, this.Count);
        }

        /// <summary>
        /// Clears the collection.
        /// </summary>
        internal void Clear()
        {
            this.Count = 0;
        }

        /// <summary>
        /// Adds a match to the collection giving a key, value and resulting tree.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="result">The tree.</param>
        /// <returns>True match added successfully.</returns>
        internal bool Add(string key, string value, IParseTree result)
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
        /// Increase the internal capacity of the list.
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
        /// This is used to initialize the collection with a preallocation of null matches.
        /// </summary>
        private void Initialize()
        {
            this.Count = 0;
            this.allElements = new Match[this.maxSize];
            for (var i = 0; i < this.maxSize; i++)
            {
                this.allElements[i] = new Match(null, null, null);
            }
        }

        /// <summary>
        /// Defines the <see cref="Match" />.
        /// </summary>
        [Serializable]
        public class Match
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Match"/> class.
            /// </summary>
            /// <param name="key">The key<see cref="string"/>.</param>
            /// <param name="value">The value<see cref="string"/>.</param>
            /// <param name="result">The result<see cref="IParseTree"/>.</param>
            public Match(string key, string value, IParseTree result)
            {
                this.Fill(key, value, result);
            }

            /// <summary>
            /// Gets the resulting tree.
            /// </summary>
            public IParseTree Result { get; private set; }

            /// <summary>
            /// Gets the Key.
            /// </summary>
            public string Key { get; private set; } = null;

            /// <summary>
            /// Gets the Value.
            /// </summary>
            public string Value { get; private set; } = null;

            /// <summary>
            /// Fills the match with key, string and value.
            /// </summary>
            /// <param name="key">The key.</param>
            /// <param name="value">The value.</param>
            /// <param name="result">The result.</param>
            public void Fill(string key, string value, IParseTree result)
            {
                this.Key = key;
                this.Value = value;
                this.Result = result;
            }

            /// <summary>
            /// Gets the resulting tree.
            /// </summary>
            /// <returns>The resulting tree.</returns>
            [Obsolete("Use Result property")]
            public IParseTree GetResult()
            {
                return this.Result;
            }
        }

        /// <summary>
        /// Supports iteration over a <see cref="T:Match[]"/> limited by count.
        /// </summary>
        public class MatchEnumerator : IEnumerator<Match>
        {
            /// <summary>
            /// Defines the enumeration count.
            /// </summary>
            private readonly int count = 0;

            /// <summary>
            /// Defines the matches array.
            /// </summary>
            private readonly Match[] matches;

            /// <summary>
            /// Defines the offset.
            /// </summary>
            private int offset = -1;

            /// <summary>
            /// Initializes a new instance of the <see cref="MatchEnumerator"/> class.
            /// </summary>
            /// <param name="matches">The array matches.</param>
            /// <param name="count">The count of matches in the array.</param>
            internal MatchEnumerator(Match[] matches, int count)
            {
                this.matches = matches;
                this.count = count;
            }

            /// <summary>
            /// Gets the Match.
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

            /// <inheritdoc/>
            object IEnumerator.Current => this.Current;

            /// <inheritdoc/>
            public void Dispose()
            {
            }

            /// <summary>
            /// Moves to the next Match in the collection.
            /// </summary>
            /// <returns>True if there is a next element.</returns>
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

            /// <inheritdoc/>
            public void Reset()
            {
                this.offset = -1;
            }
        }
    }
}
