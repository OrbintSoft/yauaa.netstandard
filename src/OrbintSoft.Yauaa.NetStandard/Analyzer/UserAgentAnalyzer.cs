//-----------------------------------------------------------------------
// <copyright file="UserAgentAnalyzer.cs" company="OrbintSoft">
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

    /// <summary>
    /// This class is used to analyze/parse a user agent.
    /// </summary>
    [Serializable]
    public class UserAgentAnalyzer : AbstractUserAgentAnalyzer
    {
        /// <summary>
        /// Creates a new instance of <see cref="UserAgentAnalyzerBuilder"/>.
        /// </summary>
        /// <returns>The <see cref="UserAgentAnalyzerBuilder"/>.</returns>
        public static UserAgentAnalyzerBuilder NewBuilder()
        {
            var a = new UserAgentAnalyzer();
            var b = new UserAgentAnalyzerBuilder(a);
            b.SetUAA(a);
            return b;
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
        /// This class is used to build a <see cref="UserAgentAnalyzer"/>.
        /// </summary>
        public class UserAgentAnalyzerBuilder : AbstractUserAgentAnalyzerBuilder<UserAgentAnalyzer, UserAgentAnalyzerBuilder>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="UserAgentAnalyzerBuilder"/> class.
            /// </summary>
            /// <param name="newUaa">The user agent analyzer</param>
            public UserAgentAnalyzerBuilder(UserAgentAnalyzer newUaa)
                : base(newUaa)
            {
            }

            /// <summary>
            /// Build the user agent analyzer.
            /// </summary>
            /// <returns>The analyzer.</returns>
            public override UserAgentAnalyzer Build()
            {
                return base.Build();
            }
        }
    }
}
