// <copyright file="MatcherList.cs" company="OrbintSoft">
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

namespace OrbintSoft.Yauaa.Analyze
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// This class represent a collection of matchers.
    /// </summary>
    [Serializable]
    public class MatcherList: ICollection<Matcher>
    {
        private const int CAPACITY_INCREASE = 3;
        private int maxSize;
        private Matcher[] allElements;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatcherList"/> class.
        /// </summary>
        /// <param name="newMaxSize">The collection size.</param>
        public MatcherList(int newMaxSize)
        {
            this.maxSize = newMaxSize;
            this.Initialize();
        }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Gets a value indicating whether  the collection is read only, no it's not read only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Add a matcher to the collection.
        /// </summary>
        /// <param name="matcher">The matcher.</param>
        public void Add(Matcher matcher)
        {
            if (this.Count >= this.maxSize)
            {
                this.IncreaseCapacity();
            }

            this.allElements[this.Count] = matcher;
            this.Count++;
        }

        /// <summary>
        /// Clears the collection.
        /// </summary>
        public void Clear()
        {
            this.Count = 0;
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="item">The matcher.</param>
        /// <returns>True or False.</returns>
        public bool Contains(Matcher item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Copies the entire collection to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">The arrayIndex.</param>
        public void CopyTo(Matcher[] array, int arrayIndex)
        {
            for (var i = 0; i < this.Count; i++)
            {
                array.SetValue(this.allElements[i], arrayIndex);
                arrayIndex += 1;
            }
        }

        /// <summary>
        /// The GetEnumerator.
        /// </summary>
        /// <returns>The <see cref="IEnumerator{Matcher}"/>.</returns>
        public IEnumerator<Matcher> GetEnumerator()
        {
            return new MatcherEnumerator(this.allElements, this.Count);
        }

        /// <summary>
        /// The remove.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>True if removed.</returns>
        public bool Remove(Matcher item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private void Initialize()
        {
            this.Count = 0;
            this.allElements = new Matcher[this.maxSize];
            for (var i = 0; i < this.maxSize; i++)
            {
                this.allElements[i] = null;
            }
        }

        private void IncreaseCapacity()
        {
            var newMaxSize = this.maxSize + CAPACITY_INCREASE;
            var newAllElements = new Matcher[newMaxSize];
            this.allElements.CopyTo(newAllElements, 0);
            for (var i = this.maxSize; i < newMaxSize; i++)
            {
                newAllElements[i] = null;
            }

            this.allElements = newAllElements;
            this.maxSize = newMaxSize;
        }

        /// <summary>
        /// The Matcher enumerator
        /// </summary>
        public class MatcherEnumerator : IEnumerator<Matcher>
        {
            /// <summary>
            /// Defines the count.
            /// </summary>
            private readonly int count = 0;

            /// <summary>
            /// Defines the matches.
            /// </summary>
            private readonly Matcher[] matchers;

            /// <summary>
            /// Defines the offset.
            /// </summary>
            private int offset = -1;

            /// <summary>
            /// Initializes a new instance of the <see cref="MatcherEnumerator"/> class.
            /// </summary>
            /// <param name="matches">The matches.</param>
            /// <param name="count">The count<see cref="int"/>.</param>
            public MatcherEnumerator(Matcher[] matches, int count)
            {
                this.matchers = matches;
                this.count = count;
            }

            /// <summary>
            /// Gets the Current.
            /// </summary>
            public Matcher Current
            {
                get
                {
                    if (this.count > this.offset)
                    {
                        return this.matchers[this.offset];
                    }
                    else
                    {
                        throw new IndexOutOfRangeException("Array index out of bounds");
                    }
                }
            }

            /// <summary>
            /// Gets the Current.
            /// </summary>
            object IEnumerator.Current => this.Current;

            /// <summary>
            /// The Dispose.
            /// </summary>
            public void Dispose()
            {
            }

            /// <summary>
            /// The MoveNext.
            /// </summary>
            /// <returns>The <see cref="bool"/>.</returns>
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
            /// The Reset.
            /// </summary>
            public void Reset()
            {
                this.offset = -1;
            }
        }
    }
}
