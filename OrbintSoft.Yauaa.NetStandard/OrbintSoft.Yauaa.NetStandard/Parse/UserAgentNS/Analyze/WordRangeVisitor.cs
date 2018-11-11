/*
 * Yet Another UserAgent Analyzer .NET Standard
 * Porting realized by Balzarotti Stefano, Copyright (C) OrbintSoft
 * 
 * Original Author and License:
 * 
 * Yet Another UserAgent Analyzer
 * Copyright (C) 2013-2018 Niels Basjes
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * All rights should be reserved to the original author Niels Basjes
 */

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
            private readonly int first;
            private readonly int last;

            private string rangeString = null;

            public Range(int first, int last)
            {
                this.first = first;
                this.last = last;
            }

            public int GetFirst()
            {
                return first;
            }

            public int GetLast()
            {
                return last;
            }

            public override string ToString()
            {
                if (rangeString == null)
                {
                    if (last == -1)
                    {
                        rangeString = "[" + first + "-]";
                    }
                    else
                    {
                        rangeString = "[" + first + "-" + last + "]";
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
                return first == range.first && last == range.last;
            }

            public override int GetHashCode()
            {
                return ValueTuple.Create(first, last).GetHashCode();
            }
        }
    }
}
