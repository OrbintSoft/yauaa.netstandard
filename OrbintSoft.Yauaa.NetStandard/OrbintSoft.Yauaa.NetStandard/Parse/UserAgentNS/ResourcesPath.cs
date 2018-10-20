namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS
{
    public class ResourcesPath
    {
        public string Directory { get; }
        public string Filter { get; }

        public ResourcesPath(string directory, string filter = ".yaml")
        {
            Directory = directory;
            Filter = filter;
        }
    }
}
