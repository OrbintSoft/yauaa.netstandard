//-----------------------------------------------------------------------
// <copyright file="PrefixLookup.cs" company="OrbintSoft">
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
// <date>2018, 12, 30, 11:29</date>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Utils
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Utility to lookup an item by prexix using a trie.
    /// </summary>
    [Serializable]
    public class PrefixLookup
    {
        /// <summary>
        /// Defines the prefixPrefixTrie.
        /// </summary>
        private readonly PrefixTrie prefixPrefixTrie;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrefixLookup"/> class.
        /// </summary>
        /// <param name="prefixList">The dictionary of lookup prefixes.</param>
        /// <param name="caseSensitive">Ttrue if the lookup should be case sensitive.</param>
        public PrefixLookup(IDictionary<string, string> prefixList, bool caseSensitive)
        {
            // Translate the map into a different structure.
            this.prefixPrefixTrie = new PrefixTrie(caseSensitive);

            foreach (var item in prefixList)
            {
                this.prefixPrefixTrie.Add(item.Key, item.Value);
            }
        }

        /// <summary>
        /// Find and item by longest matching prefix.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The item.</returns>
        public string FindLongestMatchingPrefix(string input)
        {
            return this.prefixPrefixTrie.Find(input);
        }

        /// <summary>
        /// Trie utility for fast prefix lookup.
        /// </summary>
        [Serializable]
        public class PrefixTrie
        {
            /// <summary>
            /// Defines whether the trie should be case sensitive.
            /// </summary>
            private readonly bool caseSensitive;

            /// <summary>
            /// Defines the cahr index.
            /// </summary>
            private readonly int charIndex;

            /// <summary>
            /// Defines the child nodes.
            /// </summary>
            private PrefixTrie[] childNodes;

            /// <summary>
            /// Defines the value.
            /// </summary>
            private string theValue;

            /// <summary>
            /// Initializes a new instance of the <see cref="PrefixTrie"/> class.
            /// </summary>
            /// <param name="caseSensitive">True if the trie should be case sensitive.</param>
            public PrefixTrie(bool caseSensitive)
                : this(caseSensitive, 0)
            {
            }

            /// <summary>
            /// Prevents a default instance of the <see cref="PrefixTrie"/> class from being created.
            /// </summary>
            /// <param name="caseSensitive">True if the trie should be case sensitive.</param>
            /// <param name="charIndex">The default char index.</param>
            private PrefixTrie(bool caseSensitive, int charIndex)
            {
                this.caseSensitive = caseSensitive;
                this.charIndex = charIndex;
            }

            /// <summary>
            /// Finds an element in the trie.
            /// </summary>
            /// <param name="input">The input prefix.</param>
            /// <returns>The item.</returns>
            public string Find(string input)
            {
                if (this.charIndex == input.Length)
                {
                    return this.theValue;
                }

                var myChar = input[this.charIndex]; // This will give us the ASCII value of the char
                if (myChar < 32 || myChar > 126)
                {
                    return this.theValue; // Cannot store these, so this is where it ends.
                }

                if (this.childNodes == null)
                {
                    return this.theValue;
                }

                var child = this.childNodes[myChar];
                if (child == null)
                {
                    return this.theValue;
                }

                var returnValue = child.Find(input);
                return returnValue ?? this.theValue;
            }

            /// <summary>
            /// Adds an item to the trie.
            /// </summary>
            /// <param name="prefix">The prefix.</param>
            /// <param name="value">The value.</param>
            internal void Add(string prefix, string value)
            {
                if (this.charIndex == prefix.Length)
                {
                    this.theValue = value;
                    return;
                }

                var myChar = prefix[this.charIndex]; // This will give us the ASCII value of the char
                if (myChar < 32 || myChar > 126)
                {
                    throw new ArgumentException("Only readable ASCII is allowed as key !!!");
                }

                if (this.childNodes is null)
                {
                    this.childNodes = new PrefixTrie[128];
                }

                if (this.caseSensitive)
                {
                    // If case sensitive we 'just' build the tree
                    if (this.childNodes[myChar] == null)
                    {
                        this.childNodes[myChar] = new PrefixTrie(true, this.charIndex + 1);
                    }

                    this.childNodes[myChar].Add(prefix, value);
                }
                else
                {
                    // If case INsensitive we build the tree
                    // and we link the same child to both the
                    // lower and uppercase entries in the child array.
                    var lower = char.ToLower(myChar);
                    var upper = char.ToUpper(myChar);

                    if (this.childNodes[lower] == null)
                    {
                        this.childNodes[lower] = new PrefixTrie(false, this.charIndex + 1);
                    }

                    this.childNodes[lower].Add(prefix, value);

                    if (this.childNodes[upper] == null)
                    {
                        this.childNodes[upper] = this.childNodes[lower];
                    }
                }
            }
        }
    }
}
