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

using Antlr4.Runtime.Misc;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Antlr4Source;
using System;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze
{
    [Serializable]
    public sealed class WordRangeVisitor: UserAgentTreeWalkerBaseVisitor<WordRangeVisitor.Range>
    {
        private static readonly WordRangeVisitor Instance = new WordRangeVisitor();

        private WordRangeVisitor()
        {
        }

        public static Range GetRange(UserAgentTreeWalkerParser.WordRangeContext ctx)
        {
            return Instance.Visit(ctx);
        }

        public override Range VisitWordRangeStartToEnd([NotNull] UserAgentTreeWalkerParser.WordRangeStartToEndContext context)
        {
            return new Range(int.Parse(context.firstWord.Text), int.Parse(context.lastWord.Text));
        }

        public override Range VisitWordRangeFirstWords([NotNull] UserAgentTreeWalkerParser.WordRangeFirstWordsContext context)
        {
            return new Range(1, int.Parse(context.lastWord.Text));
        }

        public override Range VisitWordRangeLastWords([NotNull] UserAgentTreeWalkerParser.WordRangeLastWordsContext context)
        {
            return new Range(int.Parse(context.firstWord.Text),-1);
        }

        public override Range VisitWordRangeSingleWord([NotNull] UserAgentTreeWalkerParser.WordRangeSingleWordContext context)
        {
            int wordNumber = int.Parse(context.singleWord.Text);
            return new Range(wordNumber, wordNumber);
        }

        [Serializable]
        public class Range
        {
            private string rangeString = null;

            public Range(int first, int last)
            {
                First = first;
                Last = last;
            }

            public int First { get; }

            public int Last { get; }

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

            public override bool Equals(object obj)
            {
                if (!(obj is Range))
                {
                    return false;
                }
                Range range = (Range)obj;
                return First == range.First && Last == range.Last;
            }

            public override int GetHashCode()
            {
                return ValueTuple.Create(First, Last).GetHashCode();
            }
        }
    }
}
