//-----------------------------------------------------------------------
// <copyright file="EvilManualUseragentStringHacks.cs" company="OrbintSoft">
// Yet Another User Agent Analyzer for .NET Standard
// porting realized by Stefano Balzarotti, Copyright 2019 (C) OrbintSoft
//
// Original Author and License:
//
// Yet Another UserAgent Analyzer
// Copyright(C) 2013-2019 Niels Basjes
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// <author>Stefano Balzarotti, Niels Basjes</author>
// <date>2018, 11, 24, 12:49</date>
// <summary></summary>
//-----------------------------------------------------------------------
namespace OrbintSoft.Yauaa.Parse
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using OrbintSoft.Yauaa.Utils;

    /// <summary>
    /// Defines the <see cref="EvilManualUseragentStringHacks" />.
    /// </summary>
    public sealed class EvilManualUseragentStringHacks
    {
        /// <summary>
        /// Defines the MissingProductAtStart.
        /// </summary>
        private static readonly Regex MissingProductAtStart = new Regex("^\\(( |;|null|compatible|windows|android|linux).*", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Defines the MissingSpace.
        /// </summary>
        private static readonly Regex MissingSpace = new Regex("(/[0-9]+\\.[0-9]+)([A-Z][a-z][a-z][a-z]+ )", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Defines the MultipleSpaces.
        /// </summary>
        private static readonly Regex MultipleSpaces = new Regex("(?: {2,})", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Defines the AvoidBase64Match.
        /// </summary>
        private static readonly Regex AvoidBase64Match = new Regex("(android/[0-9]+)(/)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Defines the AndroidDashVersion.
        /// </summary>
        private static readonly Regex AndroidDashVersion = new Regex("(android)-([0-9]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

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
        /// <param name="useragent">useragent Raw useragent.</param>
        /// <returns>Cleaned useragent.</returns>
        public static string FixIt(string useragent)
        {
            if (string.IsNullOrEmpty(useragent))
            {
                return useragent;
            }

            var result = useragent;

            result = MultipleSpaces.Replace(result, " ");

            // The first one is a special kind of space: https://unicodemap.org/details/0x2002/index.html
            result = Normalize.ReplaceString(result, "\u2002", " ");

            if (result[0] == ' ')
            {
                result = result.Trim();
            }

            result = Normalize.ReplaceString(result, "SSL/TLS", "SSL TLS");

            if (result.Contains("MSIE"))
            {
                result = Normalize.ReplaceString(result, "MSIE7", "MSIE 7");
                result = Normalize.ReplaceString(result, "MSIE8", "MSIE 8");
                result = Normalize.ReplaceString(result, "MSIE9", "MSIE 9");
            }

            result = Normalize.ReplaceString(result, "Ant.com Toolbar", "Ant.com_Toolbar");

            // Something like Android-4.0.3 is seen as a text instead of a product.
            result = AndroidDashVersion.Replace(result, "$1 $2");

            // We have seen problems causes by " Version/4.0Mobile Safari/530.17"
            result = MissingSpace.Replace(result, "$1 $2");

            // Sometimes a case like  "Android/9/something/" matches the pattern of Base84 which breaks everything
            // So those cases we simply insert a space to avoid this match and without changing the resulting tree.
            result = AvoidBase64Match.Replace(result, "$1 $2");

            // We have seen problem cases like "Java1.0.21.0"
            result = Normalize.ReplaceString(result, "Java", "Java ");

            // We have seen problem cases like "Wazzup1.1.100"
            result = Normalize.ReplaceString(result, "Wazzup", "Wazzup ");

            // This one is a single useragent that hold significant traffic
            result = Normalize.ReplaceString(result, " (Macintosh); ", " (Macintosh; ");

            // This one is a single useragent that hold significant traffic
            result = Normalize.ReplaceString(result, "Microsoft Windows NT 6.2.9200.0);", "Microsoft Windows NT 6.2.9200.0;");

            // The VM_Vertis 4010 You Build/VM is missing a ')'
            result = Normalize.ReplaceString(result, "You Build/VM", "You Build/VM)");

            // Some agents are providing comment values that are ONLY a version
            result = Normalize.ReplaceString(result, "(/", "(Unknown/");
            result = Normalize.ReplaceString(result, "; /", "; Unknown/");

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
            result = Normalize.ReplaceString(result, ",gzip(gfe)", string.Empty);

            // The Weibo useragent This one is a single useragent that hold significant traffic
            result = Normalize.ReplaceString(result, "__", " ");

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
        /// The ReplaceString.
        /// </summary>
        /// <param name="input">The input<see cref="string"/>.</param>
        /// <param name="searchFor">The searchFor<see cref="string"/>.</param>
        /// <param name="replaceWith">The replaceWith<see cref="string"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        [Obsolete("Use Normalize.ReplaceString")]
        public static string ReplaceString(string input, string searchFor, string replaceWith)
        {
            return Normalize.ReplaceString(input, searchFor, replaceWith);
        }
    }
}
