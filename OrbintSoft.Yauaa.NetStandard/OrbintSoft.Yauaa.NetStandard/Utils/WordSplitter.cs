//-----------------------------------------------------------------------
// <copyright file="WordSplitter.cs" company="OrbintSoft">
//    Yet Another User Agent Analyzer for .NET Standard
//    porting realized by Stefano Balzarotti, Copyright 2018 (C) OrbintSoft
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
    /// Defines the <see cref="WordSplitter" />
    /// </summary>
    public sealed class WordSplitter : Splitter
    {
        /// <summary>
        /// Defines the instance
        /// </summary>
        private static WordSplitter instance = null;

        /// <summary>
        /// Prevents a default instance of the <see cref="WordSplitter"/> class from being created.
        /// </summary>
        private WordSplitter()
        {
        }

        /// <summary>
        /// The GetInstance
        /// </summary>
        /// <returns>The <see cref="WordSplitter"/></returns>
        public static WordSplitter GetInstance()
        {
            if (instance == null)
            {
                instance = new WordSplitter();
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
                case ' ':
                case '.':
                case ':':
                case ';':
                case '=':
                case '/':
                case '\\':
                case '+':
                case '-':
                case '_':
                case '<':
                case '>':
                case '~':
                case '(': // EndOfString marker
                case ')': // EndOfString marker
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
            return c == '(' || c == ')';
        }
    }
}
