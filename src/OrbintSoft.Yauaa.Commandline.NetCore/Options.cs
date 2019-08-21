//-----------------------------------------------------------------------
// <copyright file="Options.cs" company="OrbintSoft">
//    Yet Another User Agent Analyzer for .NET Standard
//    porting realized by Stefano Balzarotti, Copyright 2018-2019 (C) OrbintSoft
//
//    Original Author and License:
//
//    Yet Another UserAgent Analyzer
//    Copyright(C) 2013-2019 Niels Basjes
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
// <date>2018, 11, 25, 19:51</date>
//-----------------------------------------------------------------------
using CommandLine;
using OrbintSoft.Yauaa.Analyzer;
using System.Collections.Generic;

namespace OrbintSoft.Yauaa.Commandline
{
    public class Options
    {
        [Option(longName: "ua", HelpText = "A single useragent string", SetName = "mode", Required = true)]
        public string Useragent { get; set; } = null;

        [Option(longName: "in", HelpText = "Location of input file", SetName = "mode", Required = true)]
        public string InFile { get; set; } = null;

        [Option(longName: "yaml", HelpText = "Output in yaml testcase format", SetName = "format")]
        public bool YamlFormat { get; set; } = false;

        [Option(longName: "csv", HelpText = "Output in csv format", SetName = "format")]
        public bool CsvFormat { get; set; } = false;

        [Option(longName: "json", HelpText = "Output in json format", SetName = "format")]
        public bool JsonFormat { get; set; } = false;

        [Option(longName: "xml", HelpText = "Output in xml format", SetName = "format")]
        public bool XmlFormat { get; set; } = false;

        [Option(longName: "fields", Separator =',', HelpText = "A list of the desired fieldnames (use '" + UserAgent.USERAGENT_FIELDNAME + "' if you want the input value as well)")]
        public IList<string> Fields { get; set; } = null;

        [Option(longName: "cache", HelpText = "The number of elements that can be cached (LRU).")]
        public int CacheSize { get; set; } = 10000;

        [Option(longName: "bad", HelpText = "Output only cases that have a problem")]
        public bool OutputOnlyBadResults { get; set; } = false;

        [Option(longName: "debug", HelpText = "Set to enable debugging.")]
        public bool Debug { get; set; } = false;

        [Option(longName: "fullFlatten", HelpText = "Set to flatten each parsed agent string.")]
        public bool FullFlatten { get; set; } = false;

        [Option(longName: "matchedFlatten", HelpText = "Set to get the flattened values that were relevant for the Matchers.")]
        public bool MatchedFlatten { get; set; } = false;
    }
}
