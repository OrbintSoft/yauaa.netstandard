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

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgent.Utils
{
    public sealed class Normalize
    {
        private Normalize() { }

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

        public static string Brand(string brand)
        {
            if (brand.Length <= 3)
            {
                return brand.ToUpper(CultureInfo.InvariantCulture);
            }

            StringBuilder sb = new StringBuilder(brand.Length);
            char[] nameChars = brand.ToCharArray();

            StringBuilder wordBuilder = new StringBuilder(brand.Length);

            int lowerChars = 0;
            bool wordHasNumbers = false;
            for (int i = 0; i < nameChars.Length; i++)
            {
                char thisChar = nameChars[i];
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
                        bool isUpperCase = char.IsUpper(thisChar);

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

        public static string CleanupDeviceBrandName(string deviceBrand, string deviceName)
        {
            string lowerDeviceBrand = deviceBrand.ToLower(CultureInfo.InvariantCulture);

            deviceName = deviceName.Replace("_", " ");
            deviceName = deviceName.Replace("- +", "-");
            deviceName = deviceName.Replace(" +-", "-");
            deviceName = deviceName.Replace(" +", " ");

            string lowerDeviceName = deviceName.ToLower(CultureInfo.InvariantCulture);

            // In some cases it does start with the brand but without a separator following the brand
            if (lowerDeviceName.StartsWith(lowerDeviceBrand))
            {
                deviceName = deviceName.Replace("_", " ");
                // (?i) means: case insensitive
                deviceName = deviceName.Replace("(?i)^" + Regex.Escape(deviceBrand) + "([^ ].*)$", Regex.Escape(deviceBrand) + " $1");
                deviceName = deviceName.Replace("( -| )+", " ");
            }
            else
            {
                deviceName = deviceBrand + ' ' + deviceName;
            }
            string result = Brand(deviceName);

            if (result.Contains("I"))
            {
                result = result
                    .Replace("Ipad", "iPad")
                    .Replace("Ipod", "iPod")
                    .Replace("Iphone", "iPhone")
                    .Replace("IOS ", "iOS ");
            }
            return result;
        }

        public static string Email(string email)
        {
            string cleaned = email;
            cleaned = cleaned.Replace("\\[at]", "@");

            cleaned = cleaned.Replace("\\[\\\\xc3\\\\xa07]", "@");
            cleaned = cleaned.Replace("\\[dot]", ".");
            cleaned = cleaned.Replace("\\\\", " ");
            cleaned = cleaned.Replace(" at ", "@");
            cleaned = cleaned.Replace("dot", ".");
            cleaned = cleaned.Replace(" dash ", "-");
            cleaned = cleaned.Replace(" ", "");
            return cleaned;
        }
    }
}
