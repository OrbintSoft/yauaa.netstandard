namespace OrbintSoft.Yauaa.Utils
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the <see cref="PrefixLookup" />
    /// </summary>
    [Serializable]
    public class PrefixLookup
    {
        /// <summary>
        /// Defines the prefixPrefixTrie
        /// </summary>
        private readonly PrefixTrie prefixPrefixTrie;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrefixLookup"/> class.
        /// </summary>
        /// <param name="prefixList">The prefixList<see cref="IDictionary{String, String}"/></param>
        /// <param name="caseSensitive">The caseSensitive<see cref="bool"/></param>
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
        /// The FindLongestMatchingPrefix
        /// </summary>
        /// <param name="input">The input<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        public string FindLongestMatchingPrefix(string input)
        {
            return this.prefixPrefixTrie.Find(input);
        }

        /// <summary>
        /// Defines the <see cref="PrefixTrie" />
        /// </summary>
        [Serializable]
        public class PrefixTrie
        {
            /// <summary>
            /// Defines the caseSensitive
            /// </summary>
            private readonly bool caseSensitive;

            /// <summary>
            /// Defines the charIndex
            /// </summary>
            private readonly int charIndex;

            /// <summary>
            /// Defines the childNodes
            /// </summary>
            private PrefixTrie[] childNodes;

            /// <summary>
            /// Defines the theValue
            /// </summary>
            private string theValue;

            /// <summary>
            /// Initializes a new instance of the <see cref="PrefixTrie"/> class.
            /// </summary>
            /// <param name="caseSensitive">The caseSensitive<see cref="bool"/></param>
            public PrefixTrie(bool caseSensitive)
                : this(caseSensitive, 0)
            {
            }

            /// <summary>
            /// Prevents a default instance of the <see cref="PrefixTrie"/> class from being created.
            /// </summary>
            /// <param name="caseSensitive">The caseSensitive<see cref="bool"/></param>
            /// <param name="charIndex">The charIndex<see cref="int"/></param>
            private PrefixTrie(bool caseSensitive, int charIndex)
            {
                this.caseSensitive = caseSensitive;
                this.charIndex = charIndex;
            }

            /// <summary>
            /// The find
            /// </summary>
            /// <param name="input">The input<see cref="string"/></param>
            /// <returns>The <see cref="string"/></returns>
            public string Find(string input)
            {
                if (this.charIndex == input.Length)
                {
                    return this.theValue;
                }

                char myChar = input[this.charIndex]; // This will give us the ASCII value of the char
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
            /// The Add
            /// </summary>
            /// <param name="prefix">The prefix<see cref="string"/></param>
            /// <param name="value">The value<see cref="string"/></param>
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

                if (this.childNodes == null)
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
