//-----------------------------------------------------------------------
// <copyright file="CalculateDeviceName.cs" company="OrbintSoft">
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
// <date>2020, 04, 16, 08:28</date>

namespace OrbintSoft.Yauaa.Calculate
{
    using System;
    using System.Text.RegularExpressions;
    using OrbintSoft.Yauaa.Analyzer;
    using OrbintSoft.Yauaa.Utils;

    /// <summary>
    /// Utility to calculate the <see cref="DefaultUserAgentFields.DEVICE_NAME"/> field.
    /// </summary>
    [Serializable]
    public class CalculateDeviceName : IFieldCalculator
    {
        private static readonly Regex Clean1Pattern = new Regex("AppleWebKit", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Calculate the <see cref="DefaultUserAgentFields.DEVICE_NAME"/> field.
        /// </summary>
        /// <param name="userAgent">The <see cref="UserAgent"/>.</param>
        public void Calculate(UserAgent userAgent)
        {
            // Make sure the DeviceName always starts with the DeviceBrand
            var deviceName = userAgent.Get(DefaultUserAgentFields.DEVICE_NAME);
            if (deviceName.GetConfidence() >= 0)
            {
                var deviceBrand = userAgent.Get(DefaultUserAgentFields.DEVICE_BRAND);
                string deviceNameValue = this.RemoveBadSubStrings(deviceName.GetValue());
                string deviceBrandValue = deviceBrand.GetValue();
                if (deviceName.GetConfidence() >= 0 &&
                    deviceBrand.GetConfidence() >= 0 &&
                    !deviceBrandValue.Equals("Unknown"))
                {
                    // In some cases it does start with the brand but without a separator following the brand
                    deviceNameValue = Normalize.CleanupDeviceBrandName(deviceBrandValue, deviceNameValue);
                }
                else
                {
                    deviceNameValue = Normalize.Brand(deviceNameValue);
                }

                userAgent.SetForced(
                    DefaultUserAgentFields.DEVICE_NAME,
                    deviceNameValue,
                    deviceName.GetConfidence());
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Calculate {DefaultUserAgentFields.DEVICE_NAME}";
        }

        /// <summary>
        /// Removes bad substrings from device name.
        /// </summary>
        /// <param name="input">The string that should contain the device name.</param>
        /// <returns>The cleaned string.</returns>
        private string RemoveBadSubStrings(string input)
        {
            input = Clean1Pattern.Replace(input, string.Empty);
            return input;
        }
    }
}
