//-----------------------------------------------------------------------
// <copyright file="VersionSplitter.cs" company="OrbintSoft">
//    Yet Another UserAgent Analyzer.NET Standard
//    orting realized by Stefano Balzarotti, Copyright (C) OrbintSoft
//
//    Original Author and License:
//
//    Yet Another UserAgent Analyzer
//    Copyright(C) 2013-2018 Niels Basjes
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//    https://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//   
// </copyright>
// <author>Stefano Balzarotti, Niels Basjes</author>
// <date>2018, 11, 24, 12:49</date>
// <summary></summary>
//-----------------------------------------------------------------------
namespace OrbintSoft.Yauaa.Utils
{
    /// <summary>
    /// Defines the <see cref="VersionSplitter" />
    /// </summary>
    public class VersionSplitter : Splitter
    {
        /// <summary>
        /// Defines the instance
        /// </summary>
        private static VersionSplitter instance;

        /// <summary>
        /// Prevents a default instance of the <see cref="VersionSplitter"/> class from being created.
        /// </summary>
        private VersionSplitter()
        {
        }

        /// <summary>
        /// The GetInstance
        /// </summary>
        /// <returns>The <see cref="VersionSplitter"/></returns>
        public static VersionSplitter GetInstance()
        {
            if (instance == null)
            {
                instance = new VersionSplitter();
            }
            return instance;
        }

        /// <summary>
        /// The IsSeparator
        /// </summary>
        /// <param name="c">The c<see cref="char"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public override bool IsSeparator(char c)
        {
            switch (c)
            {
                case '.':
                case '_':
                case '-':
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// The IsEndOfStringSeparator
        /// </summary>
        /// <param name="c">The c<see cref="char"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public override bool IsEndOfStringSeparator(char c)
        {
            return false;
        }

        /// <summary>
        /// The LooksLikeEmailOrWebaddress
        /// </summary>
        /// <param name="value">The value<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool LooksLikeEmailOrWebaddress(string value)
        {
            // Simple quick and dirty way to avoid splitting email and web addresses
            return (value.StartsWith("www.") || value.StartsWith("http") || (value.Contains("@") && value.Contains(".")));
        }

        /// <summary>
        /// The GetSingleSplit
        /// </summary>
        /// <param name="value">The value<see cref="string"/></param>
        /// <param name="split">The split<see cref="int"/></param>
        /// <returns>The <see cref="string"/></returns>
        public override string GetSingleSplit(string value, int split)
        {
            if (LooksLikeEmailOrWebaddress(value))
            {
                return (split == 1) ? value : null;
            }

            char[] characters = value.ToCharArray();
            int start = FindSplitStart(characters, split);
            if (start == -1)
            {
                return null;
            }
            int end = FindSplitEnd(characters, start);
            return value.Substring(start, end - start);
        }

        /// <summary>
        /// The GetFirstSplits
        /// </summary>
        /// <param name="value">The value<see cref="string"/></param>
        /// <param name="split">The split<see cref="int"/></param>
        /// <returns>The <see cref="string"/></returns>
        public override string GetFirstSplits(string value, int split)
        {
            if (LooksLikeEmailOrWebaddress(value))
            {
                return (split == 1) ? value : null;
            }

            char[] characters = value.ToCharArray();
            int start = FindSplitStart(characters, split);
            if (start == -1)
            {
                return null;
            }
            int end = FindSplitEnd(characters, start);
            return value.Substring(0, end);
        }
    }
}
