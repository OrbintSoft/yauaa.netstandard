//-----------------------------------------------------------------------
// <copyright file="VersionSplitter.cs" company="OrbintSoft">
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

    /// <summary>
    /// Utility to split a version string.
    /// </summary>
    public class VersionSplitter : Splitter
    {
        /// <summary>
        /// Lazy thread safe singleton instance.
        /// </summary>
        private static readonly Lazy<VersionSplitter> LazyInstance = new Lazy<VersionSplitter>(() => new VersionSplitter());

        /// <summary>
        /// Defines the instance.
        /// </summary>
        [Obsolete]
        private static VersionSplitter instance;

        /// <summary>
        /// Prevents a default instance of the <see cref="VersionSplitter"/> class from being created.
        /// </summary>
        private VersionSplitter()
        {
        }

        /// <summary>
        /// Gets the Singleton instance.
        /// </summary>
        public static VersionSplitter Instance => LazyInstance.Value;

        /// <summary>
        /// The GetInstance.
        /// </summary>
        /// <returns>The <see cref="VersionSplitter"/>.</returns>
        [Obsolete("Use Instance property")]
        public static VersionSplitter GetInstance()
        {
            if (instance == null)
            {
                instance = new VersionSplitter();
            }

            return instance;
        }

        /// <summary>
        /// Gets the first splits.
        /// </summary>
        /// <param name="value">The value to split.</param>
        /// <param name="split">The split start position.</param>
        /// <returns>The splits.</returns>
        public override string GetFirstSplits(string value, int split)
        {
            if (this.LooksLikeEmailOrWebaddress(value))
            {
                return (split == 1) ? value : null;
            }

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
        /// Gets the a single split.
        /// </summary>
        /// <param name="value">The value to split.</param>
        /// <param name="split">The split start position.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public override string GetSingleSplit(string value, int split)
        {
            if (this.LooksLikeEmailOrWebaddress(value))
            {
                return (split == 1) ? value : null;
            }

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
        /// There are no end string separators.
        /// </summary>
        /// <param name="c">The character to check.</param>
        /// <returns>false.</returns>
        public override bool IsEndOfStringSeparator(char c)
        {
            return false;
        }

        /// <inheritdoc/>
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
        /// Checks if the string value looks like an email or web address.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>True if looks like an email or a web address.</returns>
        private bool LooksLikeEmailOrWebaddress(string value)
        {
            // Simple quick and dirty way to avoid splitting email and web addresses
            return value.StartsWith("www.") || value.StartsWith("http") || (value.Contains("@") && value.Contains("."));
        }
    }
}
