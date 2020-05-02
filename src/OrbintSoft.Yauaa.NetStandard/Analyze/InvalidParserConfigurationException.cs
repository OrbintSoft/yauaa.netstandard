//-----------------------------------------------------------------------
// <copyright file="InvalidParserConfigurationException.cs" company="OrbintSoft">
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
// <date>2018, 11, 24, 12:48</date>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Analyze
{
    using System;

    /// <summary>
    /// This defines an exception that is thrown when there is a misconfiguration in the analyzer.
    /// It can be an invalid yaml definition or a problem with the builder parameters.
    /// </summary>
    public class InvalidParserConfigurationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidParserConfigurationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public InvalidParserConfigurationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidParserConfigurationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="e">The inner exception.</param>
        public InvalidParserConfigurationException(string message, Exception e)
            : base(message, e)
        {
        }
    }
}
