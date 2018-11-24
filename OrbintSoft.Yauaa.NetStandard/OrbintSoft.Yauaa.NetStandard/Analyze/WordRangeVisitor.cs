//<copyright file="WordRangeVisitor.cs" company="OrbintSoft">
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
//<date>2018, 7, 26, 23:01</date>
//<summary></summary>

namespace OrbintSoft.Yauaa.Analyze
{
    using Antlr4.Runtime.Misc;
    using OrbintSoft.Yauaa.Antlr4Source;
    using System;

    /// <summary>
    /// Defines the <see cref="WordRangeVisitor" />
    /// </summary>
    [Serializable]
    public sealed class WordRangeVisitor : UserAgentTreeWalkerBaseVisitor<WordRangeVisitor.Range>
    {
        /// <summary>
        /// Defines the Instance
        /// </summary>
        private static readonly WordRangeVisitor Instance = new WordRangeVisitor();

        /// <summary>
        /// Prevents a default instance of the <see cref="WordRangeVisitor"/> class from being created.
        /// </summary>
        private WordRangeVisitor()
        {
        }

        /// <summary>
        /// The GetRange
        /// </summary>
        /// <param name="ctx">The ctx<see cref="UserAgentTreeWalkerParser.WordRangeContext"/></param>
        /// <returns>The <see cref="Range"/></returns>
        public static Range GetRange(UserAgentTreeWalkerParser.WordRangeContext ctx)
        {
            return Instance.Visit(ctx);
        }

        /// <summary>
        /// The VisitWordRangeStartToEnd
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.WordRangeStartToEndContext"/></param>
        /// <returns>The <see cref="Range"/></returns>
        public override Range VisitWordRangeStartToEnd([NotNull] UserAgentTreeWalkerParser.WordRangeStartToEndContext context)
        {
            return new Range(int.Parse(context.firstWord.Text), int.Parse(context.lastWord.Text));
        }

        /// <summary>
        /// The VisitWordRangeFirstWords
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.WordRangeFirstWordsContext"/></param>
        /// <returns>The <see cref="Range"/></returns>
        public override Range VisitWordRangeFirstWords([NotNull] UserAgentTreeWalkerParser.WordRangeFirstWordsContext context)
        {
            return new Range(1, int.Parse(context.lastWord.Text));
        }

        /// <summary>
        /// The VisitWordRangeLastWords
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.WordRangeLastWordsContext"/></param>
        /// <returns>The <see cref="Range"/></returns>
        public override Range VisitWordRangeLastWords([NotNull] UserAgentTreeWalkerParser.WordRangeLastWordsContext context)
        {
            return new Range(int.Parse(context.firstWord.Text), -1);
        }

        /// <summary>
        /// The VisitWordRangeSingleWord
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.WordRangeSingleWordContext"/></param>
        /// <returns>The <see cref="Range"/></returns>
        public override Range VisitWordRangeSingleWord([NotNull] UserAgentTreeWalkerParser.WordRangeSingleWordContext context)
        {
            int wordNumber = int.Parse(context.singleWord.Text);
            return new Range(wordNumber, wordNumber);
        }

        /// <summary>
        /// Defines the <see cref="Range" />
        /// </summary>
        [Serializable]
        public class Range
        {
            /// <summary>
            /// Defines the rangeString
            /// </summary>
            private string rangeString = null;

            /// <summary>
            /// Initializes a new instance of the <see cref="Range"/> class.
            /// </summary>
            /// <param name="first">The first<see cref="int"/></param>
            /// <param name="last">The last<see cref="int"/></param>
            public Range(int first, int last)
            {
                First = first;
                Last = last;
            }

            /// <summary>
            /// Gets the First
            /// </summary>
            public int First { get; }

            /// <summary>
            /// Gets the Last
            /// </summary>
            public int Last { get; }

            /// <summary>
            /// The ToString
            /// </summary>
            /// <returns>The <see cref="string"/></returns>
            public override string ToString()
            {
                if (rangeString == null)
                {
                    if (Last == -1)
                    {
                        rangeString = "[" + First + "-]";
                    }
                    else
                    {
                        rangeString = "[" + First + "-" + Last + "]";
                    }
                }
                return rangeString;
            }

            /// <summary>
            /// The Equals
            /// </summary>
            /// <param name="obj">The obj<see cref="object"/></param>
            /// <returns>The <see cref="bool"/></returns>
            public override bool Equals(object obj)
            {
                if (!(obj is Range))
                {
                    return false;
                }
                Range range = (Range)obj;
                return First == range.First && Last == range.Last;
            }

            /// <summary>
            /// The GetHashCode
            /// </summary>
            /// <returns>The <see cref="int"/></returns>
            public override int GetHashCode()
            {
                return ValueTuple.Create(First, Last).GetHashCode();
            }
        }
    }
}
