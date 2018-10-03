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
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * All rights should be reserved to the original author Niels Basjes
 */

using System.Collections.Generic;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgent
{
    public class UserAgentAnalyzer: UserAgentAnalyzerDirect
    {
        private static readonly int DEFAULT_PARSE_CACHE_SIZE = 10000;

        private int cacheSize = DEFAULT_PARSE_CACHE_SIZE;
        private Dictionary<string, UserAgent> parseCache = null;

        protected UserAgentAnalyzer(): base()
        { 
            InitializeCache();
        }

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

        public int GetCacheSize()
        {
            return cacheSize;
        }


        public override UserAgent Parse(UserAgent userAgent)
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

            string userAgentString = userAgent.GetUserAgentString();
            UserAgent cachedValue = parseCache[userAgentString];
            if (cachedValue != null)
            {
                userAgent.Clone(cachedValue);
            }
            else
            {
                cachedValue = new UserAgent(base.Parse(userAgent));
                parseCache[userAgentString] = cachedValue;
            }
            // We have our answer.
            return userAgent;
        }

        public static new UserAgentAnalyzerBuilder<UAA, B> NewBuilder<UAA, B>()
            where UAA : UserAgentAnalyzer, new()
            where B : UserAgentAnalyzerBuilder<UAA, B>, new()
        {
            var a = new UAA();
            var b = new B();
            b.SetUAA(a);
            return b;
        }


        public class UserAgentAnalyzerBuilder<UAA, B>: UserAgentAnalyzerDirectBuilder<UAA, B> where UAA : UserAgentAnalyzer where B : UserAgentAnalyzerBuilder<UAA, B>
        {
            private readonly UAA uaa;

            public UserAgentAnalyzerBuilder(UAA newUaa):base(newUaa)
            {                
                uaa = newUaa;
            }

            /**
             * Specify a new cache size (0 = disable caching).
             * @param newCacheSize The new cache size value
             * @return the current Builder instance.
             */
            public B WithCache(int newCacheSize)
            {
                FailIfAlreadyBuilt();
                uaa.SetCacheSize(newCacheSize);
                return (B)this;
            }

            /**
             * Disable caching.
             * @return the current Builder instance.
             */
            public B WithoutCache()
            {
                FailIfAlreadyBuilt();
                uaa.SetCacheSize(0);
                return (B)this;
            }

            // We must override the method because of the generic return value.
            public override UAA Build()
            {
                return base.Build();
            }
        }
    }
}
