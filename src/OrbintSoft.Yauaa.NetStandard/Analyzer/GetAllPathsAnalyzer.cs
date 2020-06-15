//-----------------------------------------------------------------------
// <copyright file="GetAllPathsAnalyzer.cs" company="OrbintSoft">
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
// <date>2020, 06, 08, 19:47</date>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Analyzer
{
    using System.Collections.Generic;
    using Antlr4.Runtime.Tree;
    using OrbintSoft.Yauaa.Analyze;
    using OrbintSoft.Yauaa.Parse;

    /// <summary>
    /// This analyzer is for testing all paths.
    /// </summary>
    public class GetAllPathsAnalyzer : IAnalyzer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetAllPathsAnalyzer"/> class.
        /// </summary>
        /// <param name="useragent">The user agent string.</param>
        internal GetAllPathsAnalyzer(string useragent)
        {
            var flattener = new UserAgentTreeFlattener(this);
            this.Result = flattener.Parse(useragent);
        }

        /// <summary>
        /// Gets the parsed <see cref="UserAgent"/>.
        /// </summary>
        public UserAgent Result { get; }

        /// <summary>
        /// Gets the parsed values.
        /// </summary>
        public IList<string> Values { get; } = new List<string>();

        /// <summary>
        /// Gets all lookups.
        /// </summary>
        /// <returns>The dictionary of lookups.</returns>
        public IDictionary<string, IDictionary<string, string>> GetLookups()
        {
            return new Dictionary<string, IDictionary<string, string>>();
        }

        /// <summary>
        /// Gets all lookup sets.
        /// </summary>
        /// <returns>The lookups sets.</returns>
        public IDictionary<string, ISet<string>> GetLookupSets()
        {
            return new Dictionary<string, ISet<string>>();
        }

        /// <summary>
        /// Gets the required inform ranges.
        /// </summary>
        /// <param name="treeName">The name of the tree.</param>
        /// <returns>The ranges.</returns>
        public ISet<WordRangeVisitor.Range> GetRequiredInformRanges(string treeName)
        {
            // Not needed to only get all paths
            return new HashSet<WordRangeVisitor.Range>();
        }

        /// <summary>
        /// Gets the required prefix lenghts.
        /// </summary>
        /// <param name="treeName">The name of the tree.</param>
        /// <returns>The prefix lengths.</returns>
        public ISet<int?> GetRequiredPrefixLengths(string treeName)
        {
            // Not needed to only get all paths
            return new HashSet<int?>();
        }

        /// <summary>
        /// Informs about parsed values in path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="value">The value.</param>
        /// <param name="ctx">The context node.</param>
        public void Inform(string path, string value, IParseTree ctx)
        {
            this.Values.Add(path);
            this.Values.Add($"{path}=\"{value}\"");
            this.Values.Add($"{path}{{\"{AbstractUserAgentAnalyzerDirect.FirstCharactersForPrefixHash(value, AbstractUserAgentAnalyzerDirect.MAX_PREFIX_HASH_MATCH)}\"");
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="matcherAction">The <see cref="MatcherAction"/>.</param>
        /// <param name="keyPattern">The key pattern.</param>
        public void InformMeAbout(MatcherAction matcherAction, string keyPattern)
        {
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="matcherAction">The <see cref="MatcherAction"/>.</param>
        /// <param name="treeName">name of the tree.</param>
        /// <param name="prefix">The prefix.</param>
        public void InformMeAboutPrefix(MatcherAction matcherAction, string treeName, string prefix)
        {
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="treeName">The name of the tree.</param>
        /// <param name="range">The <see cref="WordRangeVisitor.Range"/>.</param>
        public void LookingForRange(string treeName, WordRangeVisitor.Range range)
        {
        }

        /// <inheritdoc/>
        public void ReceivedInput(Matcher matcher)
        {
        }
    }
}
