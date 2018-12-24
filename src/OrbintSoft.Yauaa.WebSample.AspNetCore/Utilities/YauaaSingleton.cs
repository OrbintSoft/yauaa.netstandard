using OrbintSoft.Yauaa.Analyzer;

namespace OrbintSoft.Yauaa.WebSample.AspNetCore.Utilities
{
    public static class YauaaSingleton
    {
        private static UserAgentAnalyzer.UserAgentAnalyzerBuilder Builder { get; }

        private static UserAgentAnalyzer analyzer = null;

        public static UserAgentAnalyzer Analyzer
        {
            get
            {
                if (analyzer == null)
                {
                    analyzer = Builder.Build();
                }
                return analyzer;
            }
        }

        static YauaaSingleton()
        {
            Builder = UserAgentAnalyzer.NewBuilder();
            Builder.DropTests();
            Builder.DelayInitialization();
            Builder.WithCache(100);
            Builder.HideMatcherLoadStats();
            Builder.WithAllFields();
        }


    }
}
