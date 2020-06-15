//-----------------------------------------------------------------------
// <copyright file="AbstractUserAgentAnalyzer.cs" company="OrbintSoft">
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
// <date>2020, 06, 09, 18:05</date>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Analyzer
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Abstract implementation of user agent analyzer.
    /// </summary>
    [Serializable]
    public class AbstractUserAgentAnalyzer : AbstractUserAgentAnalyzerDirect
    {
        /// <summary>
        /// Defines the default cache to be used.
        /// </summary>
        public const int DEFAULT_PARSE_CACHE_SIZE = 10000;

        /// <summary>
        /// Used as parsing cache, stores all user agents that have already been parsed.
        /// </summary>
        private IDictionary<string, UserAgent> parseCache = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractUserAgentAnalyzer"/> class.
        /// </summary>
        protected internal AbstractUserAgentAnalyzer()
            : base()
        {
            this.InitializeCache();
        }

        /// <summary>
        /// Gets the cache size.
        /// </summary>
        public int CacheSize { get; private set; } = DEFAULT_PARSE_CACHE_SIZE;

        /// <summary>
        /// Gets the cache for testing.
        /// </summary>
        internal IDictionary<string, UserAgent> ParseCache => this.parseCache;

        /// <summary>
        /// Used to disable cache.
        /// </summary>
        public void DisableCaching()
        {
            this.SetCacheSize(0);
        }

        /// <summary>
        /// Parse the user agent and fills all fields with extracted values.
        /// </summary>
        /// <param name="userAgent">The user agent to be parsed.</param>
        /// <returns>The parsed user agent.</returns>
        public override UserAgent Parse(UserAgent userAgent)
        {
            lock (this)
            {
                if (userAgent is null)
                {
                    return null;
                }

                userAgent.Reset();
                if (this.parseCache is null)
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
        /// Initialize the cache.
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

    }
}
