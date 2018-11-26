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
