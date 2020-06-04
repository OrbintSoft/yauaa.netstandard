//-----------------------------------------------------------------------
// <copyright file="ILogger.cs" company="OrbintSoft">
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
// <date>2020, 05, 26, 09:42</date>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Logger
{
    using System;

    /// <summary>
    /// Interface to make logging more generic and indipendent from the used library.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Log a message for debug purpose.
        /// </summary>
        /// <param name="message">The message.</param>
        void Debug(FormattableString message);

        /// <summary>
        /// Log an informational message.
        /// </summary>
        /// <param name="message">The message.</param>
        void Info(FormattableString message);

        /// <summary>
        /// Log a warning.
        /// </summary>
        /// <param name="message">The message.</param>
        void Warn(FormattableString message);

        /// <summary>
        /// Log an error.
        /// </summary>
        /// <param name="message">The message.</param>
        void Error(FormattableString message);
    }
}
