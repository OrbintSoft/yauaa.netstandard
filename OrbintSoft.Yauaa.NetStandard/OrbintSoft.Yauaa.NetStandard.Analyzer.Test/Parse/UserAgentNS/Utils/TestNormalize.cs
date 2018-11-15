//<copyright file="TestNormalize.cs" company="OrbintSoft">
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
//<date>2018, 10, 13, 17:40</date>
//<summary></summary>

using FluentAssertions;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Utils;
using OrbintSoft.Yauaa.Analyzer.Test.Fixtures;
using Xunit;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS.Utils
{
    public class TestNormalize : IClassFixture<LogFixture>
    {
        [Fact]
        public void CheckBrandOne()
        {
            Normalize.Brand("n").Should().Be("N");
            Normalize.Brand("N").Should().Be("N");
        }

        [Fact]
        public void CheckBrandTwo()
        {
            Normalize.Brand("nb").Should().Be("NB");
            Normalize.Brand("nB").Should().Be("NB");
            Normalize.Brand("Nb").Should().Be("NB");
            Normalize.Brand("NB").Should().Be("NB");
        }

        [Fact]
        public void CheckBrandThree()
        {
            Normalize.Brand("nba").Should().Be("NBA");
            Normalize.Brand("nBa").Should().Be("NBA");
            Normalize.Brand("Nba").Should().Be("NBA");
            Normalize.Brand("NBA").Should().Be("NBA");
        }

        [Fact]
        public void CheckBrandNormalizationWord()
        {
            Normalize.Brand("niels").Should().Be("Niels");
            Normalize.Brand("Niels").Should().Be("Niels");
            Normalize.Brand("NiElS").Should().Be("Niels");
            Normalize.Brand("nIELS").Should().Be("Niels");
            Normalize.Brand("NIELS").Should().Be("Niels");
        }

        [Fact]
        public void CheckBrandNormalizationExamples()
        {
            // At least 3 lowercase
            Normalize.Brand("NielsBasjes").Should().Be("NielsBasjes");
            Normalize.Brand("NIelsBasJES").Should().Be("NielsBasjes");
            Normalize.Brand("BlackBerry").Should().Be("BlackBerry");

            // Less than 3 lowercase
            Normalize.Brand("NIelSBasJES").Should().Be("Nielsbasjes");
            Normalize.Brand("BLACKBERRY").Should().Be("Blackberry");

            // Multiple words. Short words (1,2,3 letters) go full uppercase
            Normalize.Brand("NIels NbA BasJES").Should().Be("Niels NBA Basjes");
            Normalize.Brand("lG").Should().Be("LG");
            Normalize.Brand("hTc").Should().Be("HTC");
            Normalize.Brand("sOnY").Should().Be("Sony");
            Normalize.Brand("aSuS").Should().Be("Asus");
        }

        [Fact]
        public void CheckCombiningDeviceNameAndBrand()
        {
            Normalize.CleanupDeviceBrandName("AsUs", "something t123").Should().Be("Asus Something T123");
            Normalize.CleanupDeviceBrandName("Sony", "sony x1").Should().Be("Sony X1");
            Normalize.CleanupDeviceBrandName("Sony", "sony-x1").Should().Be("Sony X1");
            Normalize.CleanupDeviceBrandName("Sony", "sonyx1").Should().Be("Sony X1");
            Normalize.CleanupDeviceBrandName("hP", "SlateBook 10 X2 PC").Should().Be("HP SlateBook 10 X2 PC");
            Normalize.CleanupDeviceBrandName("Samsung", "GT - 1234").Should().Be("Samsung GT-1234");
        }

        [Fact]
        public void CheckEmailNormalization()
        {
            Normalize.Email("support [at] zite [dot] com").Should().Be("support@zite.com");
            Normalize.Email("austin at affectv dot co dot uk").Should().Be("austin@affectv.co.uk");
            Normalize.Email("epicurus at gmail dot com").Should().Be("epicurus@gmail.com");
            Normalize.Email("buibui[dot]bot[\\xc3\\xa07]moquadv[dot]com").Should().Be("buibui.bot@moquadv.com");
            Normalize.Email("maxpoint.crawler at maxpointinteractive dot com").Should().Be("maxpoint.crawler@maxpointinteractive.com");
            Normalize.Email("help@moz.com").Should().Be("help@moz.com");
            Normalize.Email("crawler at example dot com").Should().Be("crawler@example.com");
            Normalize.Email("yelpbot at yelp dot com").Should().Be("yelpbot@yelp.com");
            Normalize.Email("support [at] zite [dot] com").Should().Be("support@zite.com");
            Normalize.Email("support [at] safedns [dot] com").Should().Be("support@safedns.com");
            Normalize.Email("search_comments\\at\\sensis\\dot\\com\\dot\\au").Should().Be("search_comments@sensis.com.au");
            Normalize.Email("mms dash mmaudvidcrawler dash support at yahoo dash inc dot com").Should().Be("mms-mmaudvidcrawler-support@yahoo-inc.com");
        }

        [Fact]
        public void CheckBadInputData()
        {
            // This used to trigger an exception in the underlying RegEx.
            Normalize.CleanupDeviceBrandName("${N", "${N.Foo");
        }
    }
}
