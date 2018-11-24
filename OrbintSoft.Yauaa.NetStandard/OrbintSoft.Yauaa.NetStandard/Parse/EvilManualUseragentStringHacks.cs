//<copyright file="EvilManualUseragentStringHacks.cs" company="OrbintSoft">
//	Yet Another UserAgent Analyzer.NET Standard
//	Porting realized by Stefano Balzarotti, Copyright (C) OrbintSoft
//
//	Original Author and License:
//
//	Yet Another UserAgent Analyzer
//	Copyright(C) 2013-2018 Niels Basjes
//
//	Licensed under the Apache License, Version 2.0 (the "License");
//	you may not use this file except in compliance with the License.
//	You may obtain a copy of the License at
//
//	https://www.apache.org/licenses/LICENSE-2.0
//
//	Unless required by applicable law or agreed to in writing, software
//	distributed under the License is distributed on an "AS IS" BASIS,
//	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//	See the License for the specific language governing permissions and
//	limitations under the License.
//
//</copyright>
//<author>Stefano Balzarotti, Niels Basjes</author>
//<date>2018, 8, 14, 01:40</date>
//<summary></summary>

namespace OrbintSoft.Yauaa.Parse
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    /// <summary>
    /// Defines the <see cref="EvilManualUseragentStringHacks" />
    /// </summary>
    public sealed class EvilManualUseragentStringHacks
    {
        /// <summary>
        /// Defines the MissingProductAtStart
        /// </summary>
        private static readonly Regex MissingProductAtStart = new Regex("^\\(( |;|null|compatible|windows|android|linux).*", RegexOptions.IgnoreCase);

        /// <summary>
        /// Defines the MissingSpace
        /// </summary>
        private static readonly Regex MissingSpace = new Regex("(/[0-9]+\\.[0-9]+)([A-Z][a-z][a-z][a-z]+ )");

        /// <summary>
        /// Defines the MultipleSpaces
        /// </summary>
        private static readonly Regex MultipleSpaces = new Regex("(?: {2,})");

        /// <summary>
        /// Prevents a default instance of the <see cref="EvilManualUseragentStringHacks"/> class from being created.
        /// </summary>
        private EvilManualUseragentStringHacks()
        {
        }

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

            result = MultipleSpaces.Replace(result, " ");

            if (result[0] == ' ')
            {
                result = result.Trim();
            }

            result = ReplaceString(result, "SSL/TLS", "SSL TLS");

            // We have seen problems causes by " Version/4.0Mobile Safari/530.17"
            result = MissingSpace.Replace(result, "$1 $2");

            // This one is a single useragent that hold significant traffic
            result = ReplaceString(result, " (Macintosh); ", " (Macintosh; ");

            // This one is a single useragent that hold significant traffic
            result = ReplaceString(result, "Microsoft Windows NT 6.2.9200.0);", "Microsoft Windows NT 6.2.9200.0;");

            // The VM_Vertis 4010 You Build/VM is missing a ')'
            result = ReplaceString(result, "You Build/VM", "You Build/VM)");

            // Some agents are providing comment values that are ONLY a version
            result = ReplaceString(result, "(/", "(Unknown/");
            result = ReplaceString(result, "; /", "; Unknown/");

            // Repair certain cases of broken useragents (like we see for the Facebook app a lot)
            if (MissingProductAtStart.IsMatch(result))
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
                catch (Exception)
                {
                    // Ignore and continue.
                }
            }
            return result; // 99.99% of the cases nothing will have changed.
        }

        /// <summary>
        /// The ReplaceString
        /// </summary>
        /// <param name="input">The input<see cref="string"/></param>
        /// <param name="searchFor">The searchFor<see cref="string"/></param>
        /// <param name="replaceWith">The replaceWith<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
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
