using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Utils;
using FluentAssertions;
using Xunit;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS.Utils
{
    public class TestNormalize
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
            //Normalize.CleanupDeviceBrandName("Sony", "sonyx1").Should().Be("Sony X1");
            //Normalize.CleanupDeviceBrandName("hP", "SlateBook 10 X2 PC").Should().Be("HP SlateBook 10 X2 PC");
            //Normalize.CleanupDeviceBrandName("Samsung", "GT - 1234").Should().Be("Samsung GT-1234");
        }
    }
}
