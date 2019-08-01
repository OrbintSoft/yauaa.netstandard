//-----------------------------------------------------------------------
// <copyright file="ThisVersion.cs" company="OrbintSoft">
// Yet Another User Agent Analyzer for .NET Standard
// porting realized by Stefano Balzarotti, Copyright 2019 (C) OrbintSoft
//
// Original Author and License:
//
// Yet Another UserAgent Analyzer
// Copyright(C) 2013-2019 Niels Basjes
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
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
    /// Defines the <see cref="ThisVersion" />.
    /// </summary>
    public class ThisVersion
    {
        /// <summary>
        /// Defines the File informations of the the current assembly.
        /// </summary>
        private static readonly FileInfo Fi;

        /// <summary>
        /// The current assembly.
        /// </summary>
        private static readonly Assembly CurrentAssembly;

        /// <summary>
        /// The assenmbly name of current assembly.
        /// </summary>
        private static readonly AssemblyName AssemblyName;

        /// <summary>
        /// Initializes static members of the <see cref="ThisVersion"/> class.
        /// </summary>
        static ThisVersion()
        {
            CurrentAssembly = Assembly.GetExecutingAssembly();
            AssemblyName = CurrentAssembly.GetName();
            Fi = new FileInfo(CurrentAssembly.Location);
        }

        /// <summary>
        /// Gets the BuildTimestamp.
        /// </summary>
        public static string BuildTimestamp => ((Fi.LastWriteTime.Ticks - 621355968000000000) / 10000).ToString();

        /// <summary>
        /// Gets the Copyright.
        /// </summary>
        public static string Copyright => $"Copyright (C) 2013-{DateTime.Now.Year} Niels Basjes, Ported in .NET By Balzarotti Stefano (OrbintSoft)";

        /// <summary>
        /// Gets the GitCommitIdDescribeShort.
        /// </summary>
        public static string GitCommitIdDescribeShort => ThisAssembly.Git.Commit;

        /// <summary>
        /// Gets the License.
        /// </summary>
        public static string License => "License Apache 2.0";

        /// <summary>
        /// Gets the ProjectVersion.
        /// </summary>
        public static string ProjectVersion => string.Format($"{AssemblyName.Version.Major}.{AssemblyName.Version.Minor}{GetPreReleaseByPatch(AssemblyName.Version.Build)}.{AssemblyName.Version.Revision}");

        /// <summary>
        /// Returns the prerelease SemVer 2.0 identifier based on assembly patch version.
        /// </summary>
        /// <param name="patch">The patch number.</param>
        /// <returns>The prerelease name.</returns>
        public static string GetPreReleaseByPatch(int patch)
        {
            switch (patch)
            {
                case 1: return "-alpha";
                case 2: return "-beta";
                case 3: return "-rc";
                case 4: return "-stable";
                case 5: return string.Empty;
                default: return "-undefined";
            }
        }
    }
}
