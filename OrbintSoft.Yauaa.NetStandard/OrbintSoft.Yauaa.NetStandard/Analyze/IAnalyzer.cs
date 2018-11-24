//-----------------------------------------------------------------------
// <copyright file="IAnalyzer.cs" company="OrbintSoft">
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
// <date>2018, 11, 24, 12:48</date>
// <summary></summary>
//-----------------------------------------------------------------------
namespace OrbintSoft.Yauaa.Analyze
{
    using Antlr4.Runtime.Tree;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the <see cref="IAnalyzer" />
    /// </summary>
    public interface IAnalyzer
    {
        /// <summary>
        /// The Inform
        /// </summary>
        /// <param name="path">The path<see cref="string"/></param>
        /// <param name="value">The value<see cref="string"/></param>
        /// <param name="ctx">The ctx<see cref="IParseTree"/></param>
        void Inform(string path, string value, IParseTree ctx);

        /// <summary>
        /// The InformMeAbout
        /// </summary>
        /// <param name="matcherAction">The matcherAction<see cref="MatcherAction"/></param>
        /// <param name="keyPattern">The keyPattern<see cref="string"/></param>
        void InformMeAbout(MatcherAction matcherAction, string keyPattern);

        /// <summary>
        /// The LookingForRange
        /// </summary>
        /// <param name="treeName">The treeName<see cref="string"/></param>
        /// <param name="range">The range<see cref="WordRangeVisitor.Range"/></param>
        void LookingForRange(string treeName, WordRangeVisitor.Range range);

        /// <summary>
        /// The GetRequiredInformRanges
        /// </summary>
        /// <param name="treeName">The treeName<see cref="string"/></param>
        /// <returns>The <see cref="ISet{WordRangeVisitor.Range}"/></returns>
        ISet<WordRangeVisitor.Range> GetRequiredInformRanges(string treeName);

        /// <summary>
        /// The InformMeAboutPrefix
        /// </summary>
        /// <param name="matcherAction">The matcherAction<see cref="MatcherAction"/></param>
        /// <param name="treeName">The treeName<see cref="string"/></param>
        /// <param name="prefix">The prefix<see cref="string"/></param>
        void InformMeAboutPrefix(MatcherAction matcherAction, string treeName, string prefix);

        /// <summary>
        /// The GetRequiredPrefixLengths
        /// </summary>
        /// <param name="treeName">The treeName<see cref="string"/></param>
        /// <returns>The <see cref="ISet{int?}"/></returns>
        ISet<int?> GetRequiredPrefixLengths(string treeName);
    }
}
