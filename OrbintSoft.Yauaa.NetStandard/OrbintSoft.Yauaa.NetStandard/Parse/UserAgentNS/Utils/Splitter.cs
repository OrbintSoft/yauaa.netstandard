//<copyright file="Splitter.cs" company="OrbintSoft">
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
//<date>2018, 8, 14, 20:34</date>
//<summary></summary>

using System;
using System.Collections.Generic;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Utils
{
    public abstract class Splitter
    {

        public abstract bool IsSeparator(char c);

        public abstract bool IsEndOfStringSeparator(char c);

        /**
         * Find the start offset of next split
         *
         * @param chars The input in which we are seeking
         * @param offset The start offset from where to seek
         * @return The offset of the next split
         */
        public int FindNextSplitStart(char[] chars, int offset)
        {
            for (int charNr = offset; charNr < chars.Length; charNr++)
            {
                char theChar = chars[charNr];
                if (IsEndOfStringSeparator(theChar))
                {
                    return -1;
                }
                if (!IsSeparator(theChar))
                {
                    return charNr;
                }
            }
            return -1;
        }

        /**
         * Find the end of the string
         *
         * @param chars The input in which we are seeking
         * @param offset The start offset from where to seek
         * @return The offset of the last character of the last split.
         */
        public int FindEndOfString(char[] chars, int offset)
        {
            for (int charNr = offset; charNr < chars.Length; charNr++)
            {
                char theChar = chars[charNr];
                if (IsEndOfStringSeparator(theChar))
                {
                    return charNr;
                }
            }
            return chars.Length;
        }

        /**
         * Find the start offset of split
         *
         * @param chars The input in which we are seeking
         * @param split  The split number for which we are looking for the start
         * @return The offset or -1 if it does not exist
         */
        public int FindSplitStart(char[] chars, int split)
        {
            if (split <= 0)
            {
                return -1;
            }
            // We expect the chars to start with a split.
            int charNr = 0;
            bool inSplit = false;
            int currentSplit = 0;
            foreach (char theChar in chars)
            {
                if (IsEndOfStringSeparator(theChar))
                {
                    return -1;
                }

                if (IsSeparator(theChar))
                {
                    if (inSplit)
                    {
                        inSplit = false;
                    }
                }
                else
                {
                    if (!inSplit)
                    {
                        inSplit = true;
                        currentSplit++;
                        if (currentSplit == split)
                        {
                            return charNr;
                        }
                    }
                }
                charNr++;
            }
            return -1;
        }

        public int FindSplitEnd(char[] chars, int startOffset)
        {
            int i = startOffset;
            while (i < chars.Length)
            {
                if (IsSeparator(chars[i]))
                {
                    return i;
                }
                i++;
            }
            return chars.Length; // == The end of the string
        }

        public virtual string GetSingleSplit(string value, int split)
        {
            char[] characters = value.ToCharArray();
            int start = FindSplitStart(characters, split);
            if (start == -1)
            {
                return null;
            }
            int end = FindSplitEnd(characters, start);
            return value.Substring(start, end - start);
        }

        public virtual string GetFirstSplits(string value, int split)
        {
            char[] characters = value.ToCharArray();
            int start = FindSplitStart(characters, split);
            if (start == -1)
            {
                return null;
            }
            int end = FindSplitEnd(characters, start);
            return value.Substring(0, end);
        }

        public string GetSplitRange(string value, int firstSplit, int lastSplit)
        {
            if (value == null || (lastSplit > 0 && lastSplit < firstSplit))
            {
                return null;
            }
            char[] characters = value.ToCharArray();
            int firstCharOfFirstSplit = FindSplitStart(characters, firstSplit);
            if (firstCharOfFirstSplit == -1)
            {
                return null;
            }

            if (lastSplit == -1)
            {
                return value.Substring(firstCharOfFirstSplit, FindEndOfString(characters, firstCharOfFirstSplit) - firstCharOfFirstSplit);
            }
            int firstCharOfLastSplit = firstCharOfFirstSplit;
            if (lastSplit != firstSplit)
            {
                firstCharOfLastSplit = FindSplitStart(characters, lastSplit);
                if (firstCharOfLastSplit == -1)
                {
                    return null;
                }
            }

            int lastCharOfLastSplit = FindSplitEnd(characters, firstCharOfLastSplit);

            return value.Substring(firstCharOfFirstSplit, lastCharOfLastSplit - firstCharOfFirstSplit);
        }

        public string GetSplitRange(string value, Analyze.WordRangeVisitor.Range range)
        {
            return GetSplitRange(value, range.GetFirst(), range.GetLast());
        }

        public string GetSplitRange(string value, List<Tuple<int, int>> splitList, Analyze.WordRangeVisitor.Range range)
        {
            return GetSplitRange(value, splitList, range.GetFirst(), range.GetLast());
        }

        public string GetSplitRange(string value, List<Tuple<int, int>> splitList, int first, int last)
        {
            int lastIndex = last - 1;
            int firstIndex = first - 1;
            int splits = splitList.Count;

            if (last == -1)
            {
                lastIndex = splits - 1;
            }
            if (firstIndex < 0 || lastIndex < 0)
            {
                return null;
            }
            if (firstIndex >= splits || lastIndex >= splits)
            {
                return null;
            }

            return value.Substring(splitList[firstIndex].Item1, splitList[lastIndex].Item2 - splitList[firstIndex].Item1);
        }

        public List<Tuple<int, int>> CreateSplitList(string characters)
        {
            return CreateSplitList(characters.ToCharArray());
        }

        public List<Tuple<int, int>> CreateSplitList(char[] characters)
        {
            List<Tuple<int, int>> result = new List<Tuple<int, int>>();

            int offset = FindSplitStart(characters, 1);
            if (offset == -1)
            {
                return result; // Nothing at all. So we are already done
            }
            while (offset != -1)
            {

                int start = offset;
                int end = FindSplitEnd(characters, start);

                result.Add(new Tuple<int, int>(start, end));
                offset = FindNextSplitStart(characters, end);
            }
            return result;
        }
    }
}
