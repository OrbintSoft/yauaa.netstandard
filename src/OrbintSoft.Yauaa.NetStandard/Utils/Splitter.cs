﻿//-----------------------------------------------------------------------
// <copyright file="Splitter.cs" company="OrbintSoft">
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
// <date>2018, 11, 24, 12:49</date>
//-----------------------------------------------------------------------
namespace OrbintSoft.Yauaa.Utils
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Abstract class to implement a string splitter.
    /// </summary>
    public abstract class Splitter
    {
        /// <summary>
        /// Checks if the character is a separator.
        /// </summary>
        /// <param name="c">The character separator.</param>
        /// <returns>True if the character is a separator.</returns>
        public abstract bool IsSeparator(char c);

        /// <summary>
        /// Checks if the character is a string terminator.
        /// </summary>
        /// <param name="c">The character.</param>
        /// <returns>The if a end of string serparator.</returns>
        public abstract bool IsEndOfStringSeparator(char c);

        /// <summary>
        /// Find the start offset of next split.
        /// </summary>
        /// <param name="chars">The input in which we are seeking.</param>
        /// <param name="offset">The start offset from where to seek.</param>
        /// <returns>The offset of the next split.</returns>
        public int FindNextSplitStart(char[] chars, int offset)
        {
            for (var charNr = offset; charNr < chars.Length; charNr++)
            {
                var theChar = chars[charNr];
                if (this.IsEndOfStringSeparator(theChar))
                {
                    return -1;
                }

                if (!this.IsSeparator(theChar))
                {
                    return charNr;
                }
            }

            return -1;
        }

        /// <summary>
        /// Finds the end of the string.
        /// </summary>
        /// <param name="chars">The chars input in which we are seeking>.</param>
        /// <param name="offset">The start offset from where to seek.</param>
        /// <returns>The offset of the last character of the last split.<see cref="int"/>.</returns>
        public int FindEndOfString(char[] chars, int offset)
        {
            for (var charNr = offset; charNr < chars.Length; charNr++)
            {
                var theChar = chars[charNr];
                if (this.IsEndOfStringSeparator(theChar))
                {
                    return charNr;
                }
            }

            return chars.Length;
        }

        /// <summary>
        /// Finds the start offset of split.
        /// </summary>
        /// <param name="chars">The chars input in which we are seeking.</param>
        /// <param name="split">The split number for which we are looking for the start.</param>
        /// <returns>The offset or -1 if it does not exist.</returns>
        public int FindSplitStart(char[] chars, int split)
        {
            if (split <= 0)
            {
                return -1;
            }

            // We expect the chars to start with a split.
            var charNr = 0;
            var inSplit = false;
            var currentSplit = 0;
            foreach (var theChar in chars)
            {
                if (this.IsEndOfStringSeparator(theChar))
                {
                    return -1;
                }

                if (this.IsSeparator(theChar))
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

        /// <summary>
        /// Finds the end offeset of the split.
        /// </summary>
        /// <param name="chars">The chars.</param>
        /// <param name="startOffset">The start offset.</param>
        /// <returns>The end offset.</returns>
        public int FindSplitEnd(char[] chars, int startOffset)
        {
            var i = startOffset;
            while (i < chars.Length)
            {
                if (this.IsSeparator(chars[i]))
                {
                    return i;
                }

                i++;
            }

            return chars.Length; // == The end of the string
        }

        /// <summary>
        /// Gets a single split from a string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="split">The split offset.</param>
        /// <returns>The single split.</returns>
        public virtual string GetSingleSplit(string value, int split)
        {
            var characters = value.ToCharArray();
            var start = this.FindSplitStart(characters, split);
            if (start == -1)
            {
                return null;
            }

            var end = this.FindSplitEnd(characters, start);
            return value.Substring(start, end - start);
        }

        /// <summary>
        /// Gets the first split froma string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="split">The split offset.</param>
        /// <returns>The first split.</returns>
        public virtual string GetFirstSplits(string value, int split)
        {
            var characters = value.ToCharArray();
            var start = this.FindSplitStart(characters, split);
            if (start == -1)
            {
                return null;
            }

            var end = this.FindSplitEnd(characters, start);
            return value.Substring(0, end);
        }

        /// <summary>
        /// Gets a split from a range.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="firstSplit">The first split.</param>
        /// <param name="lastSplit">The last split.</param>
        /// <returns>The split range.</returns>
        public string GetSplitRange(string value, int firstSplit, int lastSplit)
        {
            if (value is null || (lastSplit > 0 && lastSplit < firstSplit))
            {
                return null;
            }

            var characters = value.ToCharArray();
            var firstCharOfFirstSplit = this.FindSplitStart(characters, firstSplit);
            if (firstCharOfFirstSplit == -1)
            {
                return null;
            }

            if (lastSplit == -1)
            {
                return value.Substring(firstCharOfFirstSplit, this.FindEndOfString(characters, firstCharOfFirstSplit) - firstCharOfFirstSplit);
            }

            var firstCharOfLastSplit = firstCharOfFirstSplit;
            if (lastSplit != firstSplit)
            {
                firstCharOfLastSplit = this.FindSplitStart(characters, lastSplit);
                if (firstCharOfLastSplit == -1)
                {
                    return null;
                }
            }

            var lastCharOfLastSplit = this.FindSplitEnd(characters, firstCharOfLastSplit);

            return value.Substring(firstCharOfFirstSplit, lastCharOfLastSplit - firstCharOfFirstSplit);
        }

        /// <summary>
        /// Gets a split from a range.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="range">The <see cref="Analyze.WordRangeVisitor.Range"/>.</param>
        /// <returns>The split.</returns>
        public string GetSplitRange(string value, Analyze.WordRangeVisitor.Range range)
        {
            return this.GetSplitRange(value, range.First, range.Last);
        }

        /// <summary>
        /// Gets a split from a range.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="splitList">The split list.</param>
        /// <param name="range">The <see cref="Analyze.WordRangeVisitor.Range"/>.</param>
        /// <returns>The split.</returns>
        public string GetSplitRange(string value, IList<Tuple<int, int>> splitList, Analyze.WordRangeVisitor.Range range)
        {
            return this.GetSplitRange(value, splitList, range.First, range.Last);
        }

        /// <summary>
        /// Gets a split from a range.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="splitList">The split list.</param>
        /// <param name="first">The start of range.</param>
        /// <param name="last">The end of range.</param>
        /// <returns>The split.</returns>
        public string GetSplitRange(string value, IList<Tuple<int, int>> splitList, int first, int last)
        {
            var lastIndex = last - 1;
            var firstIndex = first - 1;
            var splits = splitList.Count;

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

        /// <summary>
        /// Creates a split list from a string.
        /// </summary>
        /// <param name="characters">The characters string.</param>
        /// <returns>The split list.</returns>
        public IList<Tuple<int, int>> CreateSplitList(string characters)
        {
            return this.CreateSplitList(characters.ToCharArray());
        }

        /// <summary>
        ///  Creates a split list from a char array.
        /// </summary>
        /// <param name="characters">The characters.</param>
        /// <returns>The split list.</returns>
        public IList<Tuple<int, int>> CreateSplitList(char[] characters)
        {
            var result = new List<Tuple<int, int>>();

            var offset = this.FindSplitStart(characters, 1);
            if (offset == -1)
            {
                // Nothing at all. So we are already done
                return result;
            }

            while (offset != -1)
            {
                var start = offset;
                var end = this.FindSplitEnd(characters, start);

                result.Add(new Tuple<int, int>(start, end));
                offset = this.FindNextSplitStart(characters, end);
            }

            return result;
        }
    }
}
