//-----------------------------------------------------------------------
// <copyright file="UserAgentAnalyzerDirect.cs" company="OrbintSoft">
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
// <date>2018, 11, 24, 12:51</date>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Analyzer
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// This analzyer is for internal use, this can be used as base class to build your custom analyzer or it can be used for tests.
    /// With this class you can call some direct function used in parsing.
    /// </summary>
    [Serializable]
    public class UserAgentAnalyzerDirect : AbstractUserAgentAnalyzerDirect
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserAgentAnalyzerDirect"/> class.
        /// </summary>
        protected UserAgentAnalyzerDirect()
        {
        }

        /// <summary>
        /// Gets all paths from the user agent string.
        /// </summary>
        /// <param name="agent">The user agent.</param>
        /// <returns>The paths.</returns>
        [Obsolete("Use base method")]
        public static new IList<string> GetAllPaths(string agent)
        {
            return new GetAllPathsAnalyzerClass(agent).Values;
        }

        /// <summary>
        /// Get the Path Analyzer.
        /// </summary>
        /// <param name="agent">The user agent string.</param>
        /// <returns>The Path analyzer.</returns>
        [Obsolete("Use base method")]
        public static new GetAllPathsAnalyzerClass GetAllPathsAnalyzer(string agent)
        {
            return new GetAllPathsAnalyzerClass(agent);
        }

        /// <summary>
        /// Create a new Builder to construct and initialize the <see cref="UserAgentAnalyzerDirect"/>.
        /// </summary>
        /// <typeparam name="TUAA">Type of UserAgent Analyzer.</typeparam>
        /// <typeparam name="TB">Type of builder.</typeparam>
        /// <returns>The <see cref="UserAgentAnalyzerDirectBuilder{UAA, B}"/>.</returns>
        [Obsolete("Use non generic method")]
        public static UserAgentAnalyzerDirectBuilder<TUAA, TB> NewBuilder<TUAA, TB>()
            where TUAA : UserAgentAnalyzerDirect, new()
            where TB : UserAgentAnalyzerDirectBuilder<TUAA, TB>, new()
        {
            var a = new TUAA();
            var b = new TB();
            b.SetUAA(a);
            return b;
        }

        /// <summary>
        /// Create a new Builder to construct and initialize the <see cref="UserAgentAnalyzerDirect"/>.
        /// </summary>
        /// <returns>The <see cref="UserAgentAnalyzerDirectBuilder"/>.</returns>
        public static UserAgentAnalyzerDirectBuilder NewBuilder()
        {
            return new UserAgentAnalyzerDirectBuilder(new UserAgentAnalyzerDirect());
        }

        /// <summary>
        /// Removes all tests.
        /// </summary>
        /// <returns>The <see cref="UserAgentAnalyzerDirect"/> for chaining.</returns>
        [Obsolete("Use base method")]
        public new UserAgentAnalyzerDirect DropTests()
        {
            return base.DropTests() as UserAgentAnalyzerDirect;
        }

        /// <summary>
        /// Gets the User Agent max length.
        /// </summary>
        /// <returns>The length.</returns>
        [Obsolete("Use UserAgentMaxLength property")]
        public int GetUserAgentMaxLength()
        {
            return this.UserAgentMaxLength;
        }

        /// <summary>
        /// The analyzer will keep all tests.
        /// </summary>
        /// <returns>The <see cref="UserAgentAnalyzerDirect"/>.</returns>
        [Obsolete("Use base method")]
        public new UserAgentAnalyzerDirect KeepTests()
        {
            return base.KeepTests() as UserAgentAnalyzerDirect;
        }

        /// <summary>
        /// Enable statistics logging.
        /// </summary>
        /// <param name="newShowMatcherStats">Trye to enable statistics.</param>
        /// <returns>The <see cref="UserAgentAnalyzerDirect"/> for chaining.</returns>
        [Obsolete("Use base method.")]
        public new UserAgentAnalyzerDirect SetShowMatcherStats(bool newShowMatcherStats)
        {
            return base.SetShowMatcherStats(newShowMatcherStats) as UserAgentAnalyzerDirect;
        }

        /// <summary>
        /// This analyzer is for testing all paths.
        /// </summary>
        [Obsolete]
        public class GetAllPathsAnalyzerClass : GetAllPathsAnalyzer
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="GetAllPathsAnalyzerClass"/> class.
            /// </summary>
            /// <param name="useragent">The user agent string.</param>
            internal GetAllPathsAnalyzerClass(string useragent)
                : base(useragent)
            {
            }
        }

        /// <summary>
        /// This is used to build a <see cref="UserAgentAnalyzerDirect"/>.
        /// </summary>
        /// <typeparam name="TUAA">the UserAgent Analyzer.</typeparam>
        /// <typeparam name="TB">the Builder.</typeparam>
        [Obsolete("Use non generic class")]
        public class UserAgentAnalyzerDirectBuilder<TUAA, TB> : AbstractUserAgentAnalyzerDirectBuilder<TUAA, TB>
            where TUAA : UserAgentAnalyzerDirect
            where TB : UserAgentAnalyzerDirectBuilder<TUAA, TB>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="UserAgentAnalyzerDirectBuilder{UAA, B}"/> class.
            /// </summary>
            /// <param name="newUaa">The newUaa.</param>
            protected UserAgentAnalyzerDirectBuilder(TUAA newUaa)
                : base(newUaa)
            {
            }
        }
    }
}