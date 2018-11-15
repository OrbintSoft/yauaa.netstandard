//<copyright file="Normalize.cs" company="OrbintSoft">
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
//<date>2018, 8, 14, 13:12</date>
//<summary></summary>

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Utils
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

            deviceName = Regex.Replace(deviceName, "_", " ");
            deviceName = Regex.Replace(deviceName, "- +", "-");
            deviceName = Regex.Replace(deviceName, " +-", "-");
            deviceName = Regex.Replace(deviceName, " +", " ");

            string lowerDeviceName = deviceName.ToLower(CultureInfo.InvariantCulture);

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
            string result = Brand(deviceName);

            if (result.Contains("I"))
            {
                result = Regex.Replace(result, "Ipad", "iPad");
                result = Regex.Replace(result, "Ipod", "iPod");
                result = Regex.Replace(result, "Iphone", "iPhone");
                result = Regex.Replace(result, "IOS", "iOS");
            }
            return result;
        }

        public static string Email(string email)
        {
            string cleaned = email;
            cleaned = Regex.Replace(cleaned,"\\[at]", "@");

            cleaned = Regex.Replace(cleaned, "\\[\\\\xc3\\\\xa07]", "@");
            cleaned = Regex.Replace(cleaned, "\\[dot]", ".");
            cleaned = Regex.Replace(cleaned, "\\\\", " ");
            cleaned = Regex.Replace(cleaned, " at ", "@");
            cleaned = Regex.Replace(cleaned, "dot", ".");
            cleaned = Regex.Replace(cleaned, " dash ", "-");
            cleaned = Regex.Replace(cleaned, " ", "");
            return cleaned;
        }
    }
}
