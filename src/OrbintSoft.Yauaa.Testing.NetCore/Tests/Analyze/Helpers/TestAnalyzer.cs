//-----------------------------------------------------------------------
// <copyright file="TestAnalyzer.cs" company="OrbintSoft">
//    Yet Another User Agent Analyzer for .NET Standard
//    porting realized by Stefano Balzarotti, Copyright 2019 (C) OrbintSoft
//
//    Original Author and License:
//
//    Yet Another UserAgent Analyzer
//    Copyright(C) 2013-2019 Niels Basjes
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
// <date>2019, 8, 12, 09:20</date>
// <summary></summary>
//-----------------------------------------------------------------------
using System.Collections.Generic;
using Antlr4.Runtime.Tree;
using OrbintSoft.Yauaa.Analyze;

namespace OrbintSoft.Yauaa.Testing.Tests.Analyze.Helpers
{
    /// <summary>
    /// This class creates a custom Analyzer just for testing purpose.
    /// </summary>
    public class TestAnalyzer : IAnalyzer
    {
        private IDictionary<string, IDictionary<string, string>> lookups;
        private IDictionary<string, ISet<string>> lookupSets;

        public TestAnalyzer(IDictionary<string, IDictionary<string, string>> lookups, IDictionary<string, ISet<string>> lookupSets)
        {
            this.lookups = lookups;
            this.lookupSets = lookupSets;
        }

        public IDictionary<string, IDictionary<string, string>> GetLookups()
        {
            return this.lookups;
        }

        public IDictionary<string, ISet<string>> GetLookupSets()
        {
            return this.lookupSets;
        }

        public ISet<WordRangeVisitor.Range> GetRequiredInformRanges(string treeName)
        {
            return new HashSet<WordRangeVisitor.Range>();
        }

        public ISet<int?> GetRequiredPrefixLengths(string treeName)
        {
            return new HashSet<int?>();
        }

        public void Inform(string path, string value, IParseTree context)
        {
            // Not used during tests
        }

        public void InformMeAbout(MatcherAction matcherAction, string keyPattern)
        {
            // Not used during tests
        }

        public void InformMeAboutPrefix(MatcherAction matcherAction, string treeName, string prefix)
        {
            // Not used during tests
        }

        public void LookingForRange(string treeName, WordRangeVisitor.Range range)
        {
            // Not used during tests
        }

        public void ReceivedInput(Matcher matcher)
        {
           // Not used during tests
        }
    }
}
