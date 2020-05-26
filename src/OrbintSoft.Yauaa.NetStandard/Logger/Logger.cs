//-----------------------------------------------------------------------
// <copyright file="Logger.cs" company="OrbintSoft">
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
// <date>2020, 05, 26, 09:49</date>

namespace OrbintSoft.Yauaa.Logger
{
    using System;
    using log4net;

    /// <summary>
    /// Used to log with log4net.
    /// </summary>
    /// <typeparam name="T">type to categorize logging.</typeparam>
    public class Logger<T> : ILogger
    {
        private readonly ILog log4NetLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger{T}"/> class.
        /// </summary>
        public Logger()
        {
            this.log4NetLogger = LogManager.GetLogger(typeof(T));
        }

        /// <inheritdoc/>
        public void Debug(FormattableString message)
        {
            this.log4NetLogger.DebugFormat(message.Format, message.GetArguments());
        }

        /// <inheritdoc/>
        public void Error(FormattableString message)
        {
            this.log4NetLogger.ErrorFormat(message.Format, message.GetArguments());
        }

        /// <inheritdoc/>
        public void Info(FormattableString message)
        {
            this.log4NetLogger.InfoFormat(message.Format, message.GetArguments());
        }

        /// <inheritdoc/>
        public void Warn(FormattableString message)
        {
            this.log4NetLogger.WarnFormat(message.Format, message.GetArguments());
        }
    }
}
