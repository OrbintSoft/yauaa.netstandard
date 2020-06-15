//-----------------------------------------------------------------------
// <copyright file="AbstractUserAgentAnalyzerBuilder.cs" company="OrbintSoft">
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
// <date>2020, 06, 09, 18:07</date>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Analyzer
{
    /// <summary>
    /// This class is used to build a <see cref="AbstractUserAgentAnalyzer"/>.
    /// </summary>
    /// <typeparam name="TUAA">the UserAgent Analyzer.</typeparam>
    /// <typeparam name="TB">the Builder.</typeparam>
    public class AbstractUserAgentAnalyzerBuilder<TUAA, TB> : AbstractUserAgentAnalyzerDirectBuilder<TUAA, TB>
        where TUAA : AbstractUserAgentAnalyzer
        where TB : AbstractUserAgentAnalyzerBuilder<TUAA, TB>
    {
        /// <summary>
        /// Defines the user agent analyzer.
        /// </summary>
        private readonly TUAA uaa;

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractUserAgentAnalyzerBuilder{TUAA, TB}"/> class.
        /// </summary>
        /// <param name="newUaa">The newUaa<see cref="UserAgentAnalyzer"/>.</param>
        public AbstractUserAgentAnalyzerBuilder(TUAA newUaa)
            : base(newUaa)
        {
            this.uaa = newUaa;
        }

        /// <summary>
        /// Build the user agent analyzer.
        /// </summary>
        /// <returns>The analyzer.</returns>
        public override TUAA Build()
        {
            return base.Build();
        }

        /// <summary>
        /// Sets the cache size.
        /// </summary>
        /// <param name="newCacheSize">The new cache size.</param>
        /// <returns>The builder for chaining.</returns>
        public AbstractUserAgentAnalyzerBuilder<TUAA, TB> WithCache(int newCacheSize)
        {
            this.FailIfAlreadyBuilt();
            this.uaa.SetCacheSize(newCacheSize);
            return this;
        }

        /// <summary>
        /// Sets to don't use cache.
        /// </summary>
        /// <returns>The builder for chaining.</returns>
        public AbstractUserAgentAnalyzerBuilder<TUAA, TB> WithoutCache()
        {
            this.FailIfAlreadyBuilt();
            this.uaa.SetCacheSize(0);
            return this;
        }
    }
}
