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

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Utils
{
    public class VersionSplitter: Splitter
    {
        private VersionSplitter()
        {
        }

        private static VersionSplitter instance;
        public static VersionSplitter GetInstance()
        {
            if (instance == null)
            {
                instance = new VersionSplitter();
            }
            return instance;
        }

        public override bool IsSeparator(char c)
        {
            switch (c)
            {
                case '.':
                case '_':
                case '-':
                    return true;
                default:
                    return false;
            }
        }
        
        public override bool IsEndOfStringSeparator(char c)
        {
            return false;
        }

        private bool looksLikeEmailOrWebaddress(string value)
        {
            // Simple quick and dirty way to avoid splitting email and web addresses
            return (value.StartsWith("www.") || value.StartsWith("http") || (value.Contains("@") && value.Contains(".")));
        }

        public override string GetSingleSplit(string value, int split)
        {
            if (looksLikeEmailOrWebaddress(value))
            {
                return (split == 1) ? value : null;
            }

            char[] characters = value.ToCharArray();
            int start = FindSplitStart(characters, split);
            if (start == -1)
            {
                return null;
            }
            int end = FindSplitEnd(characters, start);
            return value.Substring(start, end - start);
        }

        public override string GetFirstSplits(string value, int split)
        {
            if (looksLikeEmailOrWebaddress(value))
            {
                return (split == 1) ? value : null;
            }

            char[] characters = value.ToCharArray();
            int start = FindSplitStart(characters, split);
            if (start == -1)
            {
                return null;
            }
            int end = FindSplitEnd(characters, start);
            return value.Substring(0, end);
        }

    }
}
