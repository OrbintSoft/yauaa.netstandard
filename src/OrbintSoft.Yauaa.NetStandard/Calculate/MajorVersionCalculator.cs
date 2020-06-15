//-----------------------------------------------------------------------
// <copyright file="MajorVersionCalculator.cs" company="OrbintSoft">
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
// <date>2020, 04, 16, 14:29</date>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Calculate
{
    using System;
    using OrbintSoft.Yauaa.Analyzer;
    using OrbintSoft.Yauaa.Utils;

    /// <summary>
    /// This calculator extracts major version from other fields.
    /// </summary>
    [Serializable]
    public class MajorVersionCalculator : IFieldCalculator
    {
        private readonly string versionName;
        private readonly string majorVersionName;

        /// <summary>
        /// Initializes a new instance of the <see cref="MajorVersionCalculator"/> class.
        /// </summary>
        /// <param name="majorVersionName">The major version with name.</param>
        /// <param name="versionName">The version name.</param>
        public MajorVersionCalculator(string majorVersionName, string versionName)
        {
            this.majorVersionName = majorVersionName;
            this.versionName = versionName;
        }

        /// <summary>
        /// Calculate the major version from other fields.
        /// </summary>
        /// <param name="userAgent">The <see cref="UserAgent"/>.</param>
        public void Calculate(UserAgent userAgent)
        {
            var agentVersionMajor = userAgent.Get(this.majorVersionName);
            if (agentVersionMajor is null || agentVersionMajor.GetConfidence() == -1)
            {
                var agentVersion = userAgent.Get(this.versionName);
                if (agentVersion != null)
                {
                    string version = agentVersion.GetValue();
                    if (version != null)
                    {
                        version = VersionSplitter.Instance.GetSingleSplit(agentVersion.GetValue(), 1);
                    }
                    else
                    {
                        version = "??";
                    }

                    userAgent.SetForced(
                        this.majorVersionName,
                        version,
                        agentVersion.GetConfidence());
                }
            }
        }
    }
}
