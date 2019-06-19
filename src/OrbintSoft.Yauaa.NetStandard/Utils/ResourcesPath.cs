//-----------------------------------------------------------------------
// <copyright file="ResourcesPath.cs" company="OrbintSoft">
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
// <date>2018, 11, 24, 12:51</date>
// <summary></summary>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Utils
{
    using System;
    using System.IO;

    /// <summary>
    /// Represent a set of resources based on a directory and a file filter.
    /// </summary>
    public class ResourcesPath
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourcesPath"/> class.
        /// </summary>
        /// <param name="directory">The directory<see cref="string"/>.</param>
        /// <param name="filter">The filter<see cref="string"/>.</param>
        public ResourcesPath(string directory, string filter = ".yaml")
        {
            if (!Path.IsPathRooted(directory))
            {
                directory = Path.Combine(BasePath, directory);
            }

            this.Directory = directory;
            this.Filter = filter;
        }

        /// <summary>
        /// Gets or sets the BasePath.
        /// </summary>
        public static string BasePath { get; set; } = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// Gets the Directory
        /// Relative or absolute path to the directory where resource files are stored.
        /// </summary>
        public string Directory { get; }

        /// <summary>
        /// Gets the Filter
        /// A search pattern to load only files that correspond to a provided criteria, useful also to load only one file.
        /// </summary>
        public string Filter { get; }
    }
}
