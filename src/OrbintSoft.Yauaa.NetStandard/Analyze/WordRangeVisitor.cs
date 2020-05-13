//-----------------------------------------------------------------------
// <copyright file="WordRangeVisitor.cs" company="OrbintSoft">
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
// <summary></summary>
//-----------------------------------------------------------------------
namespace OrbintSoft.Yauaa.Analyze
{
    using System;
    using Antlr4.Runtime.Misc;
    using OrbintSoft.Yauaa.Antlr4Source;

    /// <summary>
    /// This vititor is used to parse word ranges in the tree.
    /// </summary>
    [Serializable]
    public sealed class WordRangeVisitor : UserAgentTreeWalkerBaseVisitor<WordRangeVisitor.Range>
    {
        /// <summary>
        /// Defines the singleton instance.
        /// </summary>
        private static readonly WordRangeVisitor Instance = new WordRangeVisitor();

        /// <summary>
        /// Prevents a default instance of the <see cref="WordRangeVisitor"/> class from being created.
        /// </summary>
        private WordRangeVisitor()
        {
        }

        /// <summary>
        /// Gets The range from the context.
        /// </summary>
        /// <param name="ctx">The context.</param>
        /// <returns>The range.</returns>
        public static Range GetRange(UserAgentTreeWalkerParser.WordRangeContext ctx)
        {
            return Instance.Visit(ctx);
        }

        /// <summary>
        /// Visits a word range from first to last word.
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.WordRangeStartToEndContext"/>.</param>
        /// <returns>The resulting <see cref="Range"/>.</returns>
        public override Range VisitWordRangeStartToEnd([NotNull] UserAgentTreeWalkerParser.WordRangeStartToEndContext context)
        {
            return new Range(int.Parse(context.firstWord.Text), int.Parse(context.lastWord.Text));
        }

        /// <summary>
        /// Visits a word range from begin (1) to last word.
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.WordRangeFirstWordsContext"/>.</param>
        /// <returns>TThe resulting <see cref="Range"/>.</returns>
        public override Range VisitWordRangeFirstWords([NotNull] UserAgentTreeWalkerParser.WordRangeFirstWordsContext context)
        {
            return new Range(1, int.Parse(context.lastWord.Text));
        }

        /// <summary>
        /// Visits a word range from first to open end (-1).
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.WordRangeLastWordsContext"/>.</param>
        /// <returns>The resulting <see cref="Range"/>.</returns>
        public override Range VisitWordRangeLastWords([NotNull] UserAgentTreeWalkerParser.WordRangeLastWordsContext context)
        {
            return new Range(int.Parse(context.firstWord.Text), -1);
        }

        /// <summary>
        /// Visits a word range with a single word.
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.WordRangeSingleWordContext"/>.</param>
        /// <returns>The resulting <see cref="Range"/>.</returns>
        public override Range VisitWordRangeSingleWord([NotNull] UserAgentTreeWalkerParser.WordRangeSingleWordContext context)
        {
            var wordNumber = int.Parse(context.singleWord.Text);
            return new Range(wordNumber, wordNumber);
        }

        /// <summary>
        /// Defines a Range in a string.
        /// </summary>
        [Serializable]
        public class Range : IEquatable<Range>
        {
            /// <summary>
            /// A string representation of the range.
            /// </summary>
            private string rangeString = null;

            /// <summary>
            /// Initializes a new instance of the <see cref="Range"/> class.
            /// </summary>
            /// <param name="first">The first word index.</param>
            /// <param name="last">The last word index.</param>
            public Range(int first, int last)
            {
                this.First = first;
                this.Last = last;
            }

            /// <summary>
            /// Gets the index of the first word.
            /// </summary>
            public int First { get; }

            /// <summary>
            /// Gets the index of the last word.
            /// </summary>
            public int Last { get; }

            /// <inheritdoc/>
            public override string ToString()
            {
                if (this.rangeString is null)
                {
                    if (this.Last == -1)
                    {
                        this.rangeString = $"[{this.First}-]";
                    }
                    else
                    {
                        this.rangeString = $"[{this.First}-{this.Last}]";
                    }
                }

                return this.rangeString;
            }

            /// <summary>
            /// Checks if the specified <see cref="Range"/> is equal to the current object.
            /// </summary>
            /// <param name="other">The other range.</param>
            /// <returns>True if equals.</returns>
            public bool Equals(Range other)
            {
                if (ReferenceEquals(this, other))
                {
                    return true;
                }

                if (other is null)
                {
                    return false;
                }

                return this.First == other.First && this.Last == other.Last;
            }

            /// <inheritdoc/>
            public override bool Equals(object other)
            {
                if (!(other is Range))
                {
                    return false;
                }

                return this.Equals((Range)other);
            }

            /// <inheritdoc/>
            public override int GetHashCode()
            {
                return ValueTuple.Create(this.First, this.Last).GetHashCode();
            }
        }
    }
}
