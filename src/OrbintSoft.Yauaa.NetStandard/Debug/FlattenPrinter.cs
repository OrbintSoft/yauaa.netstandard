﻿//-----------------------------------------------------------------------
// <copyright file="FlattenPrinter.cs" company="OrbintSoft">
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

namespace OrbintSoft.Yauaa.Debug
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Antlr4.Runtime.Tree;
    using OrbintSoft.Yauaa.Analyze;

    /// <summary>
    /// This is used to print the flatten tree of the analyzer.
    /// Useful for debug and testing and to implement new custom yaml definitions.
    /// </summary>
    public class FlattenPrinter : IAnalyzer
    {
        /// <summary>
        /// Defines the outputStream.
        /// </summary>
        [NonSerialized]
        private readonly StreamWriter outputStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlattenPrinter"/> class.
        /// </summary>
        /// <param name="outputStream">The outputStream<see cref="StreamWriter"/>.</param>
        public FlattenPrinter(StreamWriter outputStream)
        {
            this.outputStream = outputStream;
        }

        /// <summary>
        /// Adds the the inform path to output stream.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="value">The value.</param>
        /// <param name="ctx">The context.</param>
        public void Inform(string path, string value, IParseTree ctx)
        {
            this.outputStream.WriteLine(path);
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="matcherAction">The matcher action.</param>
        /// <param name="keyPattern">The key pattern.</param>
        public void InformMeAbout(MatcherAction matcherAction, string keyPattern)
        {
            // Not needed
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="treeName">The treeName<see cref="string"/>.</param>
        /// <param name="range">The range<see cref="WordRangeVisitor.Range"/>.</param>
        public void LookingForRange(string treeName, WordRangeVisitor.Range range)
        {
            // Never called
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="treeName">The treeName<see cref="string"/>.</param>
        /// <returns>The range.</returns>
        public ISet<WordRangeVisitor.Range> GetRequiredInformRanges(string treeName)
        {
            // Never called
            return new HashSet<WordRangeVisitor.Range>();
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="matcherAction">The matcherAction<see cref="MatcherAction"/>.</param>
        /// <param name="treeName">The treeName<see cref="string"/>.</param>
        /// <param name="prefix">The prefix<see cref="string"/>.</param>
        public void InformMeAboutPrefix(MatcherAction matcherAction, string treeName, string prefix)
        {
            // Never called
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="treeName">The treeName<see cref="string"/>.</param>
        /// <returns>The required prefix lenghts.</returns>
        public ISet<int?> GetRequiredPrefixLengths(string treeName)
        {
            // Never called
            return new HashSet<int?>();
        }

        /// <inheritdoc/>
        public void ReceivedInput(Matcher matcher)
        {
        }

        /// <inheritdoc/>
        public IDictionary<string, IDictionary<string, string>> GetLookups()
        {
            return new Dictionary<string, IDictionary<string, string>>();
        }

        /// <inheritdoc/>
        public IDictionary<string, ISet<string>> GetLookupSets()
        {
            return new Dictionary<string, ISet<string>>();
        }
    }
}
