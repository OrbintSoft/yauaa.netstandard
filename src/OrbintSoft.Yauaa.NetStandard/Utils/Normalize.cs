//-----------------------------------------------------------------------
// <copyright file="Normalize.cs" company="OrbintSoft">
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
// <date>2018, 11, 24, 12:49</date>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Utils
{
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Utility class to normalize user agent strings.
    /// </summary>
    public static class Normalize
    {
        /// <summary>
        /// Normalize the brand name.
        /// </summary>
        /// <param name="brand">The brand name.</param>
        /// <returns>The normalized brand.</returns>
        public static string Brand(string brand)
        {
            if (brand.Length <= 3)
            {
                return brand.ToUpper(CultureInfo.InvariantCulture);
            }

            var sb = new StringBuilder(brand.Length);
            var nameChars = brand.ToCharArray();

            var wordBuilder = new StringBuilder(brand.Length);

            var lowerChars = 0;
            var wordHasNumbers = false;
            for (var i = 0; i < nameChars.Length; i++)
            {
                var thisChar = nameChars[i];
                if (char.IsDigit(thisChar))
                {
                    wordHasNumbers = true;
                }

                if (IsTokenSeparator(thisChar))
                {
                    if (wordBuilder.Length <= 3 || wordHasNumbers)
                    {
                        sb.Append(wordBuilder.ToString().ToUpper(CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        sb.Append(wordBuilder);
                    }

                    wordBuilder.Length = 0;
                    lowerChars = 0; // Next word
                    wordHasNumbers = false;
                    sb.Append(thisChar);
                }
                else
                {
                    if (wordBuilder.Length == 0)
                    { // First letter of a word
                        wordBuilder.Append(char.ToUpper(thisChar));
                    }
                    else
                    {
                        var isUpperCase = char.IsUpper(thisChar);

                        if (isUpperCase)
                        {
                            if (lowerChars >= 3)
                            {
                                wordBuilder.Append(thisChar);
                            }
                            else
                            {
                                wordBuilder.Append(char.ToLower(thisChar));
                            }

                            lowerChars = 0;
                        }
                        else
                        {
                            wordBuilder.Append(char.ToLower(thisChar));
                            lowerChars++;
                        }
                    }

                    // This was the last letter?
                    if (i == (nameChars.Length - 1))
                    {
                        if (wordBuilder.Length <= 3 || wordHasNumbers)
                        {
                            sb.Append(wordBuilder.ToString().ToUpper(CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            sb.Append(wordBuilder);
                        }

                        wordBuilder.Length = 0;
                        lowerChars = 0; // Next word
                        wordHasNumbers = false;
                    }
                }
            }

            return sb.ToString().Trim();
        }

        /// <summary>
        /// Custom replace string.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="searchFor">The string to be replaced.</param>
        /// <param name="replaceWith">The replacement.</param>
        /// <returns>The resulting string.</returns>
        public static string ReplaceString(string input, string searchFor, string replaceWith)
        {
            // startIdx and idxSearchFor delimit various chunks of input; these
            // chunks always end where searchFor begins
            var startIdx = 0;
            var idxSearchFor = input.IndexOf(searchFor, startIdx);
            if (idxSearchFor < 0)
            {
                return input;
            }

            var result = new StringBuilder(input.Length + 32);

            while (idxSearchFor >= 0)
            {
                // grab a part of input which does not include searchFor
                result.Append(input.Substring(startIdx, idxSearchFor - startIdx));

                // add replaceWith to take place of searchFor
                result.Append(replaceWith);

                // reset the startIdx to just after the current match, to see
                // if there are any further matches
                startIdx = idxSearchFor + searchFor.Length;
                idxSearchFor = input.IndexOf(searchFor, startIdx);
            }

            // the final chunk will go to the end of input
            result.Append(input.Substring(startIdx));
            return result.ToString();
        }

        /// <summary>
        /// Cleanum the device brand name.
        /// </summary>
        /// <param name="deviceBrand">The device brand name.</param>
        /// <param name="deviceName">The device name.</param>
        /// <returns>The cleaned device name.</returns>
        public static string CleanupDeviceBrandName(string deviceBrand, string deviceName)
        {
            var lowerDeviceBrand = deviceBrand.ToLower(CultureInfo.InvariantCulture);

            deviceName = ReplaceString(deviceName, "'", " ");
            deviceName = ReplaceString(deviceName, "_", " ");

            deviceName = Regex.Replace(deviceName, "- +", "-");
            deviceName = Regex.Replace(deviceName, " +-", "-");
            deviceName = Regex.Replace(deviceName, " +", " ");

            var lowerDeviceName = deviceName.ToLower(CultureInfo.InvariantCulture);

            // In some cases it does start with the brand but without a separator following the brand
            if (lowerDeviceName.StartsWith(lowerDeviceBrand))
            {
                deviceName = ReplaceString(deviceName, "_", " ");

                // (?i) means: case insensitive
                deviceName = Regex.Replace(deviceName, "(?i)^" + Regex.Escape(deviceBrand) + "([^ ].*)$", deviceBrand.Replace("$", "$$") + " $1");
                deviceName = Regex.Replace(deviceName, "( -| )+", " ");
            }
            else
            {
                deviceName = deviceBrand + ' ' + deviceName;
            }

            var result = Brand(deviceName);

            if (result.Contains("I"))
            {
                result = ReplaceString(result, "Ipad", "iPad");
                result = ReplaceString(result, "Ipod", "iPod");
                result = ReplaceString(result, "Iphone", "iPhone");
                result = ReplaceString(result, "IOS", "iOS");
            }

            return result;
        }

        /// <summary>
        /// Normalize an email address.
        /// </summary>
        /// <param name="email">The email to be normalized.</param>
        /// <returns>The normalized email.</returns>
        public static string Email(string email)
        {
            var cleaned = email;
            cleaned = Regex.Replace(cleaned, "\\[at]", "@");

            cleaned = Regex.Replace(cleaned, "\\[\\\\xc3\\\\xa07]", "@");
            cleaned = Regex.Replace(cleaned, "\\[dot]", ".");
            cleaned = Regex.Replace(cleaned, "\\\\", " ");
            cleaned = Regex.Replace(cleaned, " at ", "@");
            cleaned = Regex.Replace(cleaned, "dot", ".");
            cleaned = Regex.Replace(cleaned, " dash ", "-");
            cleaned = Regex.Replace(cleaned, " ", string.Empty);
            return cleaned;
        }

        /// <summary>
        /// Checks if a character is a token separator.
        /// </summary>
        /// <param name="letter">The character.</param>
        /// <returns>True if separator.</returns>
        private static bool IsTokenSeparator(char letter)
        {
            switch (letter)
            {
                case ' ':
                case '-':
                case '_':
                case '/':
                    return true;
                default:
                    return false;
            }
        }
    }
}
