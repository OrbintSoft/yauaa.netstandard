//-----------------------------------------------------------------------
// <copyright file="HostnameExtractor.cs" company="OrbintSoft">
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
// <date>2020, 06, 15, 07:33</date>
//-----------------------------------------------------------------------
namespace OrbintSoft.Yauaa.Utils
{
    /// <summary>
    /// Utility to extract the host name from an uri string.
    /// </summary>
    public static class HostnameExtractor
    {
        public static string ExtractHostname(string uriString)
        {
            if (string.IsNullOrEmpty(uriString))
            {
                return null; // Nothing to do here
            }

            int firstQuestionMark = uriString.IndexOf('?');
            int firstAmpersand = uriString.IndexOf('&');
            int cutIndex = -1;
            if (firstAmpersand != -1)
            {
                if (firstQuestionMark != -1)
                {
                    cutIndex = firstQuestionMark;
                }
                else
                {
                    cutIndex = firstAmpersand;
                }
            }
            else
            {
                if (firstQuestionMark != -1)
                {
                    cutIndex = firstQuestionMark;
                }
            }
            if (cutIndex != -1)
            {
                uriString = uriString.Substring(0, cutIndex);
            }
        }
    }
}
