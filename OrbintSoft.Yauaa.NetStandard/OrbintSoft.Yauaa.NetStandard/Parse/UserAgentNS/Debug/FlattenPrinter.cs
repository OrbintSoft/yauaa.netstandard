/*
 * Yet Another UserAgent Analyzer .NET Standard
 * Porting realized by Balzarotti Stefano, Copyright (C) OrbintSoft
 * 
 * Original Author and License:
 * 
 * Yet Another UserAgent Analyzer
 * Copyright (C) 2013-2018 Niels Basjes
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * All rights should be reserved to the original author Niels Basjes
 */

using Antlr4.Runtime.Tree;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze;
using System;
using System.Collections.Generic;
using System.IO;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Debug
{
    public class FlattenPrinter : IAnalyzer
    {
        internal readonly StreamWriter outputStream;

        public FlattenPrinter(StreamWriter outputStream)
        {
            this.outputStream = outputStream;
        }

        public void Inform(string path, string value, IParseTree ctx)
        {
            outputStream.WriteLine(path);
        }

        public void InformMeAbout(MatcherAction matcherAction, string keyPattern)
        {

        }

        public void LookingForRange(string treeName, WordRangeVisitor.Range range)
        {
            // Never called
        }

        public ISet<WordRangeVisitor.Range> GetRequiredInformRanges(string treeName)
        {
            // Never called
            return new HashSet<WordRangeVisitor.Range>();
        }

        public void InformMeAboutPrefix(MatcherAction matcherAction, string treeName, string prefix)
        {
            // Never called
        }

        public ISet<int?> GetRequiredPrefixLengths(string treeName)
        {
            // Never called
            return new HashSet<int?>();
        }

        



    }
}
