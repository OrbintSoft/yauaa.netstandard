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
    using System;

    /// <summary>
    /// Utility to extract the host name from an uri string.
    /// </summary>
    public static class HostnameExtractor
    {
        /// <summary>
        /// Extract the host name from an uri string.
        /// </summary>
        /// <param name="uriString">The uri string.</param>
        /// <returns>The name of the host.</returns>
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

            Uri uri;
            try
            {
                if (uriString[0] == '/')
                {
                    if (uriString[1] == '/')
                    {
                        uri = new Uri(uriString);
                    }
                    else
                    {
                        // So no hostname
                        return null;
                    }
                }
                else
                {
                    if (uriString.Contains(":"))
                    {
                        uri = new Uri(uriString);
                    }
                    else
                    {
                        if (uriString.Contains("/"))
                        {
                            return uriString.Split(new char[] { '/' }, 2)[0];
                        }
                        else
                        {
                            return uriString;
                        }
                    }
                }
            }
            catch (UriFormatException)
            {
                return null;
            }

            return uri.Host;
        }
    }
}
