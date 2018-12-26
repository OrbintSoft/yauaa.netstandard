//-----------------------------------------------------------------------
// <copyright file="ThisVersion.cs" company="OrbintSoft">
//    Yet Another User Agent Analyzer for .NET Standard
//    porting realized by Stefano Balzarotti, Copyright 2018 (C) OrbintSoft
//
//    Original Author and License:
//
//    Yet Another UserAgent Analyzer
//    Copyright(C) 2013-2018 Niels Basjes
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//    https://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
// </copyright>
// <author>Stefano Balzarotti, Niels Basjes</author>
// <date>2018, 11, 24, 12:49</date>
// <summary></summary>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Utils
{
    using System;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Defines the <see cref="ThisVersion" />
    /// </summary>
    public class ThisVersion
    {
        /// <summary>
        /// Defines the Fi
        /// </summary>
        private static readonly FileInfo Fi;

        /// <summary>
        /// Initializes static members of the <see cref="ThisVersion"/> class.
        /// </summary>
        static ThisVersion()
        {
            var asm = Assembly.GetExecutingAssembly();
            Fi = new FileInfo(asm.Location);
        }

        /// <summary>
        /// Gets the BuildTimestamp
        /// </summary>
        public static string BuildTimestamp => Fi.LastWriteTime.Ticks.ToString();

        /// <summary>
        /// Gets the Copyright
        /// </summary>
        public static string Copyright => "Copyright (C) 2013-" + DateTime.Now.Year + " Niels Basjes, Ported in .NET By Balzarotti Stefano (OrbintSoft)";

        /// <summary>
        /// Gets the GitCommitIdDescribeShort
        /// </summary>
        public static string GitCommitIdDescribeShort => ThisAssembly.Git.Commit;

        /// <summary>
        /// Gets the License
        /// </summary>
        public static string License => "License Apache 2.0";

        /// <summary>
        /// Gets the ProjectVersion
        /// </summary>
        public static string ProjectVersion => new Version(Convert.ToInt16(ThisAssembly.Git.SemVer.Major), Convert.ToInt16(ThisAssembly.Git.SemVer.Minor), Convert.ToInt16(ThisAssembly.Git.SemVer.Patch)).ToString();
    }
}
