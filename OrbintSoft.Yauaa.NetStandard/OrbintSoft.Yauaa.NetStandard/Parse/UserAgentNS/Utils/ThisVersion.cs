//<copyright file="ThisVersion.cs" company="OrbintSoft">
//	Yet Another UserAgent Analyzer.NET Standard
//	Porting realized by Stefano Balzarotti, Copyright (C) OrbintSoft
//
//	Original Author and License:
//
//	Yet Another UserAgent Analyzer
//	Copyright(C) 2013-2018 Niels Basjes
//
//	Licensed under the Apache License, Version 2.0 (the "License");
//	you may not use this file except in compliance with the License.
//	You may obtain a copy of the License at
//
//	https://www.apache.org/licenses/LICENSE-2.0
//
//	Unless required by applicable law or agreed to in writing, software
//	distributed under the License is distributed on an "AS IS" BASIS,
//	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//	See the License for the specific language governing permissions and
//	limitations under the License.
//
//</copyright>
//<author>Stefano Balzarotti, Niels Basjes</author>
//<date>2018, 10, 2, 18:32</date>
//<summary></summary>

using System;
using System.IO;
using System.Reflection;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Utils
{
    public class ThisVersion
    {
        private static readonly FileInfo fi;
        static ThisVersion()
        {
            
            Assembly asm = Assembly.GetExecutingAssembly();
            fi = new FileInfo(asm.Location);
        }
        public static string GetGitCommitIdDescribeShort()
        {
            return ThisAssembly.Git.Commit;
        }

        public static string GetBuildTimestamp()
        {

            return fi.LastWriteTime.Ticks.ToString();
        }

        public static string GetProjectVersion()
        {
            return new Version(Convert.ToInt16(ThisAssembly.Git.SemVer.Major), Convert.ToInt16(ThisAssembly.Git.SemVer.Minor), Convert.ToInt16(ThisAssembly.Git.SemVer.Patch)).ToString();
        }

        public static string GetCopyright()
        {
            return "Copyright (C) 2013-" + DateTime.Now.Year + " Niels Basjes, Ported in .NET By Balzarotti Stefano (OrbintSoft)";
        }

        public static string GetLicense()
        {
            return "License Apache 2.0";
        }
    }
}
