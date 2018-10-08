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

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Parse
{
    public sealed class EvilManualUseragentStringHacks
    {
        private EvilManualUseragentStringHacks() { }

        private static readonly Regex MISSING_PRODUCT_AT_START = new Regex("^\\(( |;|null|compatible|windows|android|linux).*", RegexOptions.IgnoreCase);
        private static readonly Regex MISSING_SPACE = new Regex("(/[0-9]+\\.[0-9]+)([A-Z][a-z][a-z][a-z]+ )", RegexOptions.IgnoreCase);

        /// <summary>
        /// There are a few situations where in order to parse the useragent we need to 'fix it'.
        /// Yes, all of this is pure evil but we "have to".
        /// </summary>
        /// <param name="useragent">useragent Raw useragent</param>
        /// <returns>Cleaned useragent</returns>
        public static string FixIt(string useragent)
        {
            if (string.IsNullOrEmpty(useragent))
            {
                return useragent;
            }
            string result = useragent;

            if (result[0] == ' ')
            {
                result = result.Trim();
            }

            // We have seen problems causes by " Version/4.0Mobile Safari/530.17"
            result = MISSING_SPACE.Replace(result, "$1 $2");

            // This one is a single useragent that hold significant traffic
            result = ReplaceString(result, " (Macintosh); ", " (Macintosh; ");

            // This one is a single useragent that hold significant traffic
            result = ReplaceString(result, "Microsoft Windows NT 6.2.9200.0);", "Microsoft Windows NT 6.2.9200.0;");

            // Repair certain cases of broken useragents (like we see for the Facebook app a lot)
            if (MISSING_PRODUCT_AT_START.IsMatch(result))
            {
                // We simply prefix a fake product name to continue parsing.
                result = "Mozilla/5.0 " + result;
            }
            else
            {
                // This happens occasionally
                if (result[0] == '/')
                {
                    // We simply prefix a fake product name to continue parsing.
                    result = "Mozilla" + result;
                }
            }

            // Kick some garbage that sometimes occurs.
            result = ReplaceString(result, ",gzip(gfe)", "");

            // The Weibo useragent This one is a single useragent that hold significant traffic
            result = ReplaceString(result, "__", " ");

            if (result.Contains("%20"))
            {
                try
                {
                    result = HttpUtility.UrlDecode(result, Encoding.UTF8);
                }
                catch (Exception  e) {
                    // UnsupportedEncodingException: Can't happen because the UTF-8 is hardcoded here.
                    // IllegalArgumentException: Probably bad % encoding in there somewhere.
                    // Ignore and continue.
                }
            }
            return result; // 99.99% of the cases nothing will have changed.
        }

        public static string ReplaceString(string input, string searchFor, string replaceWith)
        {
            //startIdx and idxSearchFor delimit various chunks of input; these
            //chunks always end where searchFor begins
            int startIdx = 0;
            int idxSearchFor = input.IndexOf(searchFor, startIdx);
            if (idxSearchFor < 0)
            {
                return input;
            }
            StringBuilder result = new StringBuilder(input.Length + 32);

            while (idxSearchFor >= 0)
            {
                //grab a part of input which does not include searchFor
                result.Append(input.Substring(startIdx, idxSearchFor - startIdx));
                //add replaceWith to take place of searchFor
                result.Append(replaceWith);

                //reset the startIdx to just after the current match, to see
                //if there are any further matches
                startIdx = idxSearchFor + searchFor.Length;
                idxSearchFor = input.IndexOf(searchFor, startIdx);
            }
            //the final chunk will go to the end of input
            result.Append(input.Substring(startIdx));
            return result.ToString();
        }


    }
}
