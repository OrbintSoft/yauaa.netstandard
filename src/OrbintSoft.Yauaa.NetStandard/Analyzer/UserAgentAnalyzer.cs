//-----------------------------------------------------------------------
// <copyright file="UserAgentAnalyzer.cs" company="OrbintSoft">
//   Yet Another User Agent Analyzer for .NET Standard
//   porting realized by Stefano Balzarotti, Copyright 2018-2019 (C) OrbintSoft
//
//   Original Author and License:
//
//   Yet Another UserAgent Analyzer
//   Copyright(C) 2013-2019 Niels Basjes
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
    /// Defines the <see cref="UserAgentAnalyzer" />.
    /// </summary>
    [Serializable]
    public class UserAgentAnalyzer : UserAgentAnalyzerDirect
    {
        /// <summary>
        /// Defines the DEFAULT_PARSE_CACHE_SIZE.
        /// </summary>
        public const int DEFAULT_PARSE_CACHE_SIZE = 10000;

        /// <summary>
        /// Defines the parseCache.
        /// </summary>
        private IDictionary<string, UserAgent> parseCache = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAgentAnalyzer"/> class.
        /// </summary>
        protected UserAgentAnalyzer()
            : base()
        {
            this.InitializeCache();
        }

        /// <summary>
        /// Gets the CacheSize.
        /// </summary>
        public int CacheSize { get; private set; } = DEFAULT_PARSE_CACHE_SIZE;

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
        /// The DisableCaching.
        /// </summary>
        public void DisableCaching()
        {
            this.SetCacheSize(0);
        }

        /// <summary>
        /// The Parse.
        /// </summary>
        /// <param name="userAgent">The userAgent<see cref="UserAgent"/>.</param>
        /// <returns>The <see cref="UserAgent"/>.</returns>
        public override UserAgent Parse(UserAgent userAgent)
        {
            lock (this)
            {
                if (userAgent == null)
                {
                    return null;
                }

                userAgent.Reset();
                if (this.parseCache == null)
                {
                    return base.Parse(userAgent);
                }

                var userAgentString = userAgent.UserAgentString;
                if (userAgentString != null)
                {
                    if (this.parseCache.ContainsKey(userAgentString))
                    {
                        userAgent.Clone(this.parseCache[userAgentString]);
                    }
                    else
                    {
                        this.parseCache[userAgentString] = new UserAgent(base.Parse(userAgent));
                    }
                }

                // We have our answer.
                return userAgent;
            }
        }

        /// <summary>
        /// Sets the new size of the parsing cache.
        /// Note that this will also wipe the existing cache.
        /// </summary>
        /// <param name="newCacheSize">The size of the new LRU cache. As size of 0 will disable caching.</param>
        public void SetCacheSize(int newCacheSize)
        {
            this.CacheSize = newCacheSize > 0 ? newCacheSize : 0;
            this.InitializeCache();
        }

        /// <summary>
        /// The InitializeCache.
        /// </summary>
        private void InitializeCache()
        {
            if (this.CacheSize >= 1)
            {
                this.parseCache = new Dictionary<string, UserAgent>(this.CacheSize);
            }
            else
            {
                this.parseCache = null;
            }
        }

        /// <summary>
        /// Defines the <see cref="UserAgentAnalyzerBuilder" />.
        /// </summary>
        public class UserAgentAnalyzerBuilder : UserAgentAnalyzerDirectBuilder<UserAgentAnalyzer, UserAgentAnalyzerBuilder>
        {
            /// <summary>
            /// Defines the uaa.
            /// </summary>
            private readonly UserAgentAnalyzer uaa;

            /// <summary>
            /// Initializes a new instance of the <see cref="UserAgentAnalyzerBuilder"/> class.
            /// </summary>
            /// <param name="newUaa">The newUaa<see cref="UserAgentAnalyzer"/>.</param>
            public UserAgentAnalyzerBuilder(UserAgentAnalyzer newUaa)
                : base(newUaa)
            {
                this.uaa = newUaa;
            }

            /// <summary>
            /// The Build.
            /// </summary>
            /// <returns>The <see cref="UserAgentAnalyzer"/>.</returns>
            public override UserAgentAnalyzer Build()
            {
                return base.Build();
            }

            /// <summary>
            /// The WithCache.
            /// </summary>
            /// <param name="newCacheSize">The newCacheSize<see cref="int"/>.</param>
            /// <returns>The <see cref="UserAgentAnalyzerBuilder"/>.</returns>
            public UserAgentAnalyzerBuilder WithCache(int newCacheSize)
            {
                this.FailIfAlreadyBuilt();
                this.uaa.SetCacheSize(newCacheSize);
                return this;
            }

            /// <summary>
            /// The WithoutCache.
            /// </summary>
            /// <returns>The <see cref="UserAgentAnalyzerBuilder"/>.</returns>
            public UserAgentAnalyzerBuilder WithoutCache()
            {
                this.FailIfAlreadyBuilt();
                this.uaa.SetCacheSize(0);
                return this;
            }
        }
    }
}
