//-----------------------------------------------------------------------
// <copyright file="UserAgentAnalyzer.cs" company="OrbintSoft">
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
// <date>2018, 11, 24, 12:51</date>
// <summary></summary>
//-----------------------------------------------------------------------
namespace OrbintSoft.Yauaa.Analyzer
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the <see cref="UserAgentAnalyzer" />
    /// </summary>
    [Serializable]
    public class UserAgentAnalyzer : UserAgentAnalyzerDirect
    {
        /// <summary>
        /// Defines the DEFAULT_PARSE_CACHE_SIZE
        /// </summary>
        private const int DEFAULT_PARSE_CACHE_SIZE = 10000;

        /// <summary>
        /// Defines the cacheSize
        /// </summary>
        private int cacheSize = DEFAULT_PARSE_CACHE_SIZE;

        /// <summary>
        /// Defines the parseCache
        /// </summary>
        private Dictionary<string, UserAgent> parseCache = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAgentAnalyzer"/> class.
        /// </summary>
        protected UserAgentAnalyzer() : base()
        {
            InitializeCache();
        }

        /// <summary>
        /// The NewBuilder
        /// </summary>
        /// <returns>The <see cref="UserAgentAnalyzerBuilder"/></returns>
        public static UserAgentAnalyzerBuilder NewBuilder()
        {
            var a = new UserAgentAnalyzer();
            var b = new UserAgentAnalyzerBuilder(a);
            b.SetUAA(a);
            return b;
        }

        /// <summary>
        /// The DisableCaching
        /// </summary>
        public void DisableCaching()
        {
            SetCacheSize(0);
        }

        /// <summary>
        /// Sets the new size of the parsing cache.
        /// Note that this will also wipe the existing cache.
        /// </summary>
        /// <param name="newCacheSize">The size of the new LRU cache. As size of 0 will disable caching.</param>
        public void SetCacheSize(int newCacheSize)
        {
            cacheSize = newCacheSize > 0 ? newCacheSize : 0;
            InitializeCache();
        }

        /// <summary>
        /// The GetCacheSize
        /// </summary>
        /// <returns>The <see cref="int"/></returns>
        public int GetCacheSize()
        {
            return cacheSize;
        }

        /// <summary>
        /// The Parse
        /// </summary>
        /// <param name="userAgent">The userAgent<see cref="UserAgent"/></param>
        /// <returns>The <see cref="UserAgent"/></returns>
        public override UserAgent Parse(UserAgent userAgent)
        {
            lock (this)
            {
                if (userAgent == null)
                {
                    return null;
                }
                userAgent.Reset();

                if (parseCache == null)
                {
                    return base.Parse(userAgent);
                }

                string userAgentString = userAgent.UserAgentString;
                if (userAgentString != null)
                {
                    UserAgent cachedValue = parseCache.ContainsKey(userAgentString) ? parseCache[userAgentString] : null;
                    if (cachedValue != null)
                    {
                        userAgent.Clone(cachedValue);
                    }
                    else
                    {
                        cachedValue = new UserAgent(base.Parse(userAgent));
                        parseCache[userAgentString] = cachedValue;
                    }
                }
                // We have our answer.
                return userAgent;
            }
        }

        /// <summary>
        /// The InitializeCache
        /// </summary>
        private void InitializeCache()
        {
            if (cacheSize >= 1)
            {
                parseCache = new Dictionary<string, UserAgent>(cacheSize);
            }
            else
            {
                parseCache = null;
            }
        }

        /// <summary>
        /// Defines the <see cref="UserAgentAnalyzerBuilder" />
        /// </summary>
        public class UserAgentAnalyzerBuilder : UserAgentAnalyzerDirectBuilder<UserAgentAnalyzer, UserAgentAnalyzerBuilder>
        {
            /// <summary>
            /// Defines the uaa
            /// </summary>
            private readonly UserAgentAnalyzer uaa;

            /// <summary>
            /// Initializes a new instance of the <see cref="UserAgentAnalyzerBuilder"/> class.
            /// </summary>
            /// <param name="newUaa">The newUaa<see cref="UserAgentAnalyzer"/></param>
            public UserAgentAnalyzerBuilder(UserAgentAnalyzer newUaa) : base(newUaa)
            {
                uaa = newUaa;
            }

            /**
             * Specify a new cache size (0 = disable caching).
             * @param newCacheSize The new cache size value
             * @return the current Builder instance.
             */
            /// <summary>
            /// The WithCache
            /// </summary>
            /// <param name="newCacheSize">The newCacheSize<see cref="int"/></param>
            /// <returns>The <see cref="UserAgentAnalyzerBuilder"/></returns>
            public UserAgentAnalyzerBuilder WithCache(int newCacheSize)
            {
                FailIfAlreadyBuilt();
                uaa.SetCacheSize(newCacheSize);
                return this;
            }

            /**
             * Disable caching.
             * @return the current Builder instance.
             */
            /// <summary>
            /// The WithoutCache
            /// </summary>
            /// <returns>The <see cref="UserAgentAnalyzerBuilder"/></returns>
            public UserAgentAnalyzerBuilder WithoutCache()
            {
                FailIfAlreadyBuilt();
                uaa.SetCacheSize(0);
                return this;
            }

            // We must override the method because of the generic return value.
            /// <summary>
            /// The Build
            /// </summary>
            /// <returns>The <see cref="UserAgentAnalyzer"/></returns>
            public override UserAgentAnalyzer Build()
            {
                return base.Build();
            }
        }
    }
}
