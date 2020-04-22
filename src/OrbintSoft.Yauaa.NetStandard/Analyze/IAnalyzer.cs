//-----------------------------------------------------------------------
// <copyright file="IAnalyzer.cs" company="OrbintSoft">
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
//-----------------------------------------------------------------------
namespace OrbintSoft.Yauaa.Analyze
{
    using System.Collections.Generic;
    using Antlr4.Runtime.Tree;

    /// <summary>
    /// This interface defines methods used by the user agent analyzer.
    /// </summary>
    public interface IAnalyzer
    {
        /// <summary>
        /// Inform about a path and value in the context.
        /// </summary>
        /// <param name="path">The flat path in the tree.</param>
        /// <param name="value">The value of the node.</param>
        /// <param name="context">The tree node context.</param>
        void Inform(string path, string value, IParseTree context);

        /// <summary>
        /// Informs the analyzer about a key pattern in a matcher action.
        /// </summary>
        /// <param name="matcherAction">The <see cref="MatcherAction"/>.</param>
        /// <param name="keyPattern">The keyPattern.</param>
        void InformMeAbout(MatcherAction matcherAction, string keyPattern);

        /// <summary>
        /// Looks for a range by the tree name.
        /// </summary>
        /// <param name="treeName">The tree name.</param>
        /// <param name="range">The range.</param>
        void LookingForRange(string treeName, WordRangeVisitor.Range range);

        /// <summary>
        /// Gets the required range from tree name.
        /// </summary>
        /// <param name="treeName">The tree name.</param>
        /// <returns>The ranges.</returns>
        ISet<WordRangeVisitor.Range> GetRequiredInformRanges(string treeName);

        /// <summary>
        /// Informe me about a prefix.
        /// </summary>
        /// <param name="matcherAction">The <see cref="MatcherAction"/>.</param>
        /// <param name="treeName">The tree name.</param>
        /// <param name="prefix">The prefix.</param>
        void InformMeAboutPrefix(MatcherAction matcherAction, string treeName, string prefix);

        /// <summary>
        /// For a tree gets required prefix lenghts.
        /// </summary>
        /// <param name="treeName">The treeName<see cref="string"/>.</param>
        /// <returns>The prefix lenghts/>.</returns>
        ISet<int?> GetRequiredPrefixLengths(string treeName);

        /// <summary>
        /// When received an input.
        /// </summary>
        /// <param name="matcher">The matcher.</param>
        void ReceivedInput(Matcher matcher);

        /// <summary>
        /// Gets the lookups.
        /// </summary>
        /// <returns>The dictionary of lookups.</returns>
        IDictionary<string, IDictionary<string, string>> GetLookups();

        /// <summary>
        /// Gets the sets of lookup.
        /// </summary>
        /// <returns>A dictionary of lookups sets.</returns>
        IDictionary<string, ISet<string>> GetLookupSets();
    }
}
