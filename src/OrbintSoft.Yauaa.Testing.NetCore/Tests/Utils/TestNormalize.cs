//-----------------------------------------------------------------------
// <copyright file="TestNormalize.cs" company="OrbintSoft">
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
// <date>2018, 11, 24, 17:39</date>
// <summary></summary>
//-----------------------------------------------------------------------
namespace OrbintSoft.Yauaa.Testing.Tests.Utils
{
    using FluentAssertions;
    using OrbintSoft.Yauaa.Testing.Fixtures;
    using OrbintSoft.Yauaa.Utils;
    using Xunit;

    /// <summary>
    /// This class is usded to test the normalize utility.    
    /// </summary>
    public class TestNormalize : IClassFixture<LogFixture>
    {
        /// <summary>
        /// I test if I am able to normalize a brand with one char.
        /// I expect the char to be capitalized.
        /// </summary>
        [Fact]
        public void CheckBrandOne()
        {
            Normalize.Brand("n").Should().Be("N");
            Normalize.Brand("N").Should().Be("N");
        }

        /// <summary>
        /// I test if I am able to normalize a brand with two chars.
        /// I expect the chars to be capitalized.
        /// </summary>
        [Fact]
        public void CheckBrandTwo()
        {
            Normalize.Brand("nb").Should().Be("NB");
            Normalize.Brand("nB").Should().Be("NB");
            Normalize.Brand("Nb").Should().Be("NB");
            Normalize.Brand("NB").Should().Be("NB");
        }

        /// <summary>
        /// I test if I am able to normalize a brand with three chars.
        /// I expect the chars to be capitalized.
        /// </summary>
        [Fact]
        public void CheckBrandThree()
        {
            Normalize.Brand("nba").Should().Be("NBA");
            Normalize.Brand("nBa").Should().Be("NBA");
            Normalize.Brand("Nba").Should().Be("NBA");
            Normalize.Brand("NBA").Should().Be("NBA");
        }

        /// <summary>
        /// I test if I am able to normalize a brand with three and four chars separated bt a slash.        
        /// </summary>
        [Fact]
        public void CheckBrandThreeFour()
        {
            Normalize.Brand("nba/kLmN").Should().Be("NBA/Klmn");
            Normalize.Brand("nBa/KlMn").Should().Be("NBA/Klmn");
            Normalize.Brand("Nba/klmn").Should().Be("NBA/Klmn");
            Normalize.Brand("NBA/KLMN").Should().Be("NBA/Klmn");
        }

        /// <summary>
        /// I test if I am able to normalize a word with more than 3 chars.
        /// </summary>
        [Fact]
        public void CheckBrandNormalizationWord()
        {
            Normalize.Brand("niels").Should().Be("Niels");
            Normalize.Brand("Niels").Should().Be("Niels");
            Normalize.Brand("NiElS").Should().Be("Niels");
            Normalize.Brand("nIELS").Should().Be("Niels");
            Normalize.Brand("NIELS").Should().Be("Niels");
        }

        /// <summary>
        /// I try to normalize different words.
        /// </summary>
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

        /// <summary>
        /// I test the normalization of different brands and device names.
        /// </summary>
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

        /// <summary>
        /// I test the normalization of email.
        /// </summary>
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

        /// <summary>
        /// I test that the normalization don't fail with bad input data.
        /// </summary>
        [Fact]
        public void CheckBadInputData()
        {
            // This used to trigger an exception in the underlying RegEx.
            Normalize.CleanupDeviceBrandName("${N", "${N.Foo");
        }
    }
}
