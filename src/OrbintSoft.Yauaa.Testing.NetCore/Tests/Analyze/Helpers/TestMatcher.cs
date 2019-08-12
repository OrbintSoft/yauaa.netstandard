//-----------------------------------------------------------------------
// <copyright file="TestMatcher.cs" company="OrbintSoft">
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
// <date>2019, 8, 12, 10:32</date>
// <summary></summary>
//-----------------------------------------------------------------------
using OrbintSoft.Yauaa.Analyze;
using OrbintSoft.Yauaa.Analyzer;
using System.Collections.Generic;

namespace OrbintSoft.Yauaa.Testing.Tests.Analyze.Helpers
{
    public class TestMatcher: Matcher
    {
        internal readonly IList<string> receivedValues = new List<string>(128);

        internal TestMatcher(IDictionary<string, IDictionary<string, string>> lookups, IDictionary<string, ISet<string>> lookupSets) : base(new TestAnalyzer(lookups, lookupSets))
        {
        }

        public override void InformMeAbout(MatcherAction matcherAction, string keyPattern)
        {
            this.receivedValues.Add(keyPattern);
        }

        public override void InformMeAboutPrefix(MatcherAction matcherAction, string treeName, string prefix)
        {
            this.InformMeAbout(matcherAction, treeName + "{\"" + UserAgentAnalyzerDirect.FirstCharactersForPrefixHash(prefix, UserAgentAnalyzerDirect.MAX_PREFIX_HASH_MATCH) + "\"");
        }

        public override void Analyze(UserAgent userAgent)
        {
            // Do nothing
        }

        public override void LookingForRange(string treeName, WordRangeVisitor.Range range)
        {
            // Do nothing
        }
    }
}
