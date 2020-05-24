//-----------------------------------------------------------------------
// <copyright file="CalculateDeviceBrand.cs" company="OrbintSoft">
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
// <date>2020, 04, 16, 01:36</date>

namespace OrbintSoft.Yauaa.Calculate
{
    using System;
    using System.Collections.Generic;
    using DomainParser.Library;
    using OrbintSoft.Yauaa.Analyzer;
    using OrbintSoft.Yauaa.Utils;

    /// <summary>
    /// CalculateDeviceBrand.
    /// </summary>
    [Serializable]
    public class CalculateDeviceBrand : IFieldCalculator
    {
        private readonly HashSet<string> unwantedUrlBrands;
        private readonly HashSet<string> unwantedEmailBrands;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculateDeviceBrand"/> class.
        /// </summary>
        public CalculateDeviceBrand()
        {
            this.unwantedUrlBrands = new HashSet<string>
            {
                "Github",
                "Gitlab",
            };

            this.unwantedEmailBrands = new HashSet<string>
            {
                "Gmail",
                "Outlook",
            };
        }

        /// <inheritdoc/>
        public void Calculate(UserAgent userAgent)
        {
            // The device brand field is a mess.
            var deviceBrand = userAgent.Get(DefaultUserAgentFields.DEVICE_BRAND);
            if (!deviceBrand.IsDefaultValue)
            {
                userAgent.SetForced(
                    DefaultUserAgentFields.DEVICE_BRAND,
                    Normalize.Brand(deviceBrand.GetValue()),
                    deviceBrand.GetConfidence());
            }

            if (deviceBrand.IsDefaultValue)
            {
                // If no brand is known then try to extract something that looks like a Brand from things like URL and Email addresses.
                var newDeviceBrand = this.DetermineDeviceBrand(userAgent);
                if (newDeviceBrand != null)
                {
                    userAgent.SetForced(
                        DefaultUserAgentFields.DEVICE_BRAND,
                        newDeviceBrand,
                        1);
                }
            }
        }

        /// <summary>
        /// The DetermineDeviceBrand.
        /// </summary>
        /// <param name="userAgent">The userAgent<see cref="UserAgent"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string DetermineDeviceBrand(UserAgent userAgent)
        {
            // If no brand is known but we do have a URL then we assume the hostname to be the brand.
            // We put this AFTER the creation of the DeviceName because we choose to not have
            // this brandname in the DeviceName.
            var informationUrl = userAgent.Get(DefaultUserAgentFields.AGENT_INFORMATION_URL);
            if (informationUrl != null && informationUrl.GetConfidence() >= 0)
            {
                var hostname = informationUrl.GetValue();
                try
                {
                    var url = new Uri(hostname);
                    hostname = url.Host;
                }
                catch (Exception)
                {
                    // Ignore any exception and continue.
                }

                hostname = this.ExtractCompanyFromHostName(hostname, this.unwantedUrlBrands);
                if (hostname != null)
                {
                    return hostname;
                }
            }

            var informationEmail = userAgent.Get(DefaultUserAgentFields.AGENT_INFORMATION_EMAIL);
            if (informationEmail != null && informationEmail.GetConfidence() >= 0)
            {
                var hostname = informationEmail.GetValue();
                var atOffset = hostname.IndexOf('@');
                if (atOffset >= 0)
                {
                    hostname = hostname.Substring(atOffset + 1);
                }

                hostname = this.ExtractCompanyFromHostName(hostname, this.unwantedEmailBrands);
                if (hostname != null)
                {
                    return hostname;
                }
            }

            return null;
        }

        /// <summary>
        /// The ExtractCompanyFromHostName.
        /// </summary>
        /// <param name="hostname">The hostname<see cref="string"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string ExtractCompanyFromHostName(string hostname, ISet<string> blackList)
        {
            if (DomainName.TryParse(hostname, out var outDomain))
            {
                var brand = Normalize.Brand(outDomain.Domain?.ToLower());
                if (blackList.Contains(brand))
                {
                    return null;
                }

                return brand;
            }
            else
            {
                return null;
            }
        }
    }
}
