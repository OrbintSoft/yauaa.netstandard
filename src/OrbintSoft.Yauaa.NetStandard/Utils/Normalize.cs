//-----------------------------------------------------------------------
// <copyright file="Normalize.cs" company="OrbintSoft">
//    Yet Another User Agent Analyzer for .NET Standard
//    porting realized by Stefano Balzarotti, Copyright 2018 (C) OrbintSoft
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
// <date>2018, 11, 24, 12:49</date>
// <summary></summary>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Utils
{
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Defines the <see cref="Normalize" />
    /// </summary>
    public sealed class Normalize
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="Normalize"/> class from being created.
        /// </summary>
        private Normalize()
        {
        }

        /// <summary>
        /// The Brand
        /// </summary>
        /// <param name="brand">The brand<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
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

            return sb.ToString();
        }

        /// <summary>
        /// The CleanupDeviceBrandName
        /// </summary>
        /// <param name="deviceBrand">The deviceBrand<see cref="string"/></param>
        /// <param name="deviceName">The deviceName<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        public static string CleanupDeviceBrandName(string deviceBrand, string deviceName)
        {
            var lowerDeviceBrand = deviceBrand.ToLower(CultureInfo.InvariantCulture);

            deviceName = Regex.Replace(deviceName, "_", " ");
            deviceName = Regex.Replace(deviceName, "- +", "-");
            deviceName = Regex.Replace(deviceName, " +-", "-");
            deviceName = Regex.Replace(deviceName, " +", " ");

            var lowerDeviceName = deviceName.ToLower(CultureInfo.InvariantCulture);

            // In some cases it does start with the brand but without a separator following the brand
            if (lowerDeviceName.StartsWith(lowerDeviceBrand))
            {
                deviceName = Regex.Replace(deviceName, "_", " ");

                // (?i) means: case insensitive
                deviceName = Regex.Replace(deviceName, "(?i)^" + Regex.Escape(deviceBrand) + "([^ ].*)$", Regex.Escape(deviceBrand) + " $1");
                deviceName = Regex.Replace(deviceName, "( -| )+", " ");
            }
            else
            {
                deviceName = deviceBrand + ' ' + deviceName;
            }

            var result = Brand(deviceName);

            if (result.Contains("I"))
            {
                result = Regex.Replace(result, "Ipad", "iPad");
                result = Regex.Replace(result, "Ipod", "iPod");
                result = Regex.Replace(result, "Iphone", "iPhone");
                result = Regex.Replace(result, "IOS", "iOS");
            }

            return result;
        }

        /// <summary>
        /// The Email
        /// </summary>
        /// <param name="email">The email<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
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
        /// The IsTokenSeparator
        /// </summary>
        /// <param name="letter">The letter<see cref="char"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private static bool IsTokenSeparator(char letter)
        {
            switch (letter)
            {
                case ' ':
                case '-':
                case '_':
                    return true;
                default:
                    return false;
            }
        }
    }
}
