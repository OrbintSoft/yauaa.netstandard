﻿//-----------------------------------------------------------------------
// <copyright file="CalculateNetworkType.cs" company="OrbintSoft">
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
// <date>2020, 06, 07, 15:96</date>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Calculate
{
    using System;
    using OrbintSoft.Yauaa.Analyzer;
    using OrbintSoft.Yauaa.Utils;

    /// <summary>
    /// Utility to calculate the <see cref="DefaultUserAgentFields.NETWORK_TYPE"/> field.
    /// </summary>
    [Serializable]
    public class CalculateNetworkType : IFieldCalculator
    {
        /// <summary>
        /// Calculate the <see cref="DefaultUserAgentFields.NETWORK_TYPE"/> field.
        /// </summary>
        /// <param name="userAgent">The <see cref="UserAgent"/>.</param>
        public void Calculate(UserAgent userAgent)
        {
            // Make sure the DeviceName always starts with the DeviceBrand
            var networkType = userAgent.Get(DefaultUserAgentFields.NETWORK_TYPE);
            if (networkType != null && networkType.GetConfidence() >= 0)
            {
                userAgent.SetForced(
                    DefaultUserAgentFields.NETWORK_TYPE,
                    Normalize.Brand(networkType.GetValue()),
                    networkType.GetConfidence());
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Calculate {DefaultUserAgentFields.NETWORK_TYPE}";
        }
    }
}
