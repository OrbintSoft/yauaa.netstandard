﻿using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS;
using FluentAssertions;
using Xunit;
using OrbintSoft.Yauaa.Analyzer.Test.Fixtures;
using System;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze;


namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS.Analyze
{
    public class TestBuilder : IClassFixture<LogFixture>
    {
        private void RunTestCase(UserAgentAnalyzerDirect userAgentAnalyzer)
        {
            UserAgent parsedAgent = userAgentAnalyzer.Parse("Mozilla/5.0 (Linux; Android 7.0; Nexus 6 Build/NBD90Z) " +
                "AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.124 Mobile Safari/537.36");

            // The requested fields
            parsedAgent.GetValue("DeviceClass").Should().Be("Phone"); // Phone
            parsedAgent.GetValue("AgentNameVersionMajor").Should().Be("Chrome 53"); // Chrome 53

            // The fields that are internally needed to build the requested fields
            parsedAgent.GetValue("AgentName").Should().Be("Chrome"); // Chrome
            parsedAgent.GetValue("AgentVersion").Should().Be("53.0.2785.124"); // 53.0.2785.124
            parsedAgent.GetValue("AgentVersionMajor").Should().Be("53"); // 53

             long min1 = -1;

            // The rest must be at confidence -1 (i.e. no rules fired)
            parsedAgent.GetConfidence("DeviceName").Should().Be(min1); // Nexus 6
            parsedAgent.GetConfidence("DeviceBrand").Should().Be(min1); // Google
            parsedAgent.GetConfidence("OperatingSystemClass").Should().Be(min1); // Mobile
            parsedAgent.GetConfidence("OperatingSystemName").Should().Be(min1); // Android
            parsedAgent.GetConfidence("OperatingSystemVersion").Should().Be(min1); // 7.0
            parsedAgent.GetConfidence("OperatingSystemNameVersion").Should().Be(min1); // Android 7.0
            parsedAgent.GetConfidence("OperatingSystemVersionBuild").Should().Be(min1); // NBD90Z
            parsedAgent.GetConfidence("LayoutEngineClass").Should().Be(min1); // Browser
            parsedAgent.GetConfidence("LayoutEngineName").Should().Be(min1); // Blink
            parsedAgent.GetConfidence("LayoutEngineVersion").Should().Be(min1); // 53.0
            parsedAgent.GetConfidence("LayoutEngineVersionMajor").Should().Be(min1); // 53
            parsedAgent.GetConfidence("LayoutEngineNameVersion").Should().Be(min1); // Blink 53.0
            parsedAgent.GetConfidence("LayoutEngineNameVersionMajor").Should().Be(min1); // Blink 53
            parsedAgent.GetConfidence("AgentClass").Should().Be(min1); // Browser
            parsedAgent.GetConfidence("AgentNameVersion").Should().Be(min1); // Chrome 53.0.2785.124
        }

        [Fact]
        public void TestLimitedFieldsDirect()
        {
            UserAgentAnalyzerDirect userAgentAnalyzer =
                UserAgentAnalyzer
                    .NewBuilder()
                    .Preheat(100)
                    .Preheat()
                    .HideMatcherLoadStats()
                    .ShowMatcherLoadStats()
                    .WithAllFields()
                    .WithField("DeviceClass")
                    .WithField("AgentNameVersionMajor")
                    .WithUserAgentMaxLength(1234)
                    .Build();

            userAgentAnalyzer.GetUserAgentMaxLength().Should().Be(1234);

            RunTestCase(userAgentAnalyzer);
        }

        [Fact]
        public void TestLimitedFields()
        {
            UserAgentAnalyzer userAgentAnalyzer =
                UserAgentAnalyzer
                    .NewBuilder()
                    .Preheat(100)
                    .Preheat()
                    .WithCache(42)
                    .WithoutCache()
                    .HideMatcherLoadStats()
                    .ShowMatcherLoadStats()
                    .WithAllFields()
                    .WithField("DeviceClass")
                    .WithField("AgentNameVersionMajor")
                    .WithUserAgentMaxLength(1234)
                    .Build();

            userAgentAnalyzer.GetUserAgentMaxLength().Should().Be(1234);

            RunTestCase(userAgentAnalyzer);
        }

        [Fact]
        public void TestLoadAdditionalRules()
        { 
            UserAgentAnalyzer userAgentAnalyzer =
                UserAgentAnalyzer
                    .NewBuilder()
                    .WithField("DeviceClass")
                    .WithoutCache()
                    .HideMatcherLoadStats()
                    .AddResources("YamlResources","ExtraLoadedRule1.yaml")
                    .WithField("ExtraValue2")
                    .WithField("ExtraValue1")
                    .AddResources("YamlResources", "ExtraLoadedRule2.yaml")
                    .Build();

            UserAgent parsedAgent = userAgentAnalyzer.Parse("Mozilla/5.0 (Linux; Android 7.0; Nexus 6 Build/NBD90Z) " +
                "AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.124 Mobile Safari/537.36");

            // The requested fields
            parsedAgent.GetValue("DeviceClass").Should().Be("Phone");
            parsedAgent.GetValue("ExtraValue1").Should().Be("One");
            parsedAgent.GetValue("ExtraValue2").Should().Be("Two");
        }

        [Fact]
        public void TestLoadOnlyCustomRules()
        {
            UserAgentAnalyzer userAgentAnalyzer =
                UserAgentAnalyzer
                    .NewBuilder()
                    .WithoutCache()
                    .HideMatcherLoadStats()
                    .AddResources("YamlResources", "ExtraLoadedRule1.yaml")
                    .WithField("ExtraValue2")
                    .WithField("ExtraValue1")
                    .AddResources("YamlResources", "ExtraLoadedRule2.yaml")
                    .Build();

            UserAgent parsedAgent = userAgentAnalyzer.Parse("Mozilla/5.0 (Linux; Android 7.0; Nexus 6 Build/NBD90Z) " +
                "AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.124 Mobile Safari/537.36");

            // The requested fields
            parsedAgent.GetValue("ExtraValue1").Should().Be("One");
            parsedAgent.GetValue("ExtraValue2").Should().Be("Two");
        }

        [Fact]
        public void TestLoadOnlyCompanyCustomFormatRules()
        {
            UserAgentAnalyzer userAgentAnalyzer =
                UserAgentAnalyzer
                    .NewBuilder()
                    .WithoutCache()
                    .HideMatcherLoadStats()
                    .DropDefaultResources()
                    .AddResources("YamlResources", "CompanyInternalUserAgents.yaml")
                    .WithField("ApplicationName")
                    .WithField("ApplicationVersion")
                    .WithField("ApplicationInstance")
                    .WithField("ApplicationGitCommit")
                    .WithField("ServerName")
                    .Build();

            UserAgent parsedAgent = userAgentAnalyzer.Parse(
                "TestApplication/1.2.3 (node123.datacenter.example.nl; 1234; d71922715c2bfe29343644b14a4731bf5690e66e)");

            // The requested fields
            parsedAgent.GetValue("ApplicationName").Should().Be("TestApplication");
            parsedAgent.GetValue("ApplicationVersion").Should().Be("1.2.3");
            parsedAgent.GetValue("ApplicationInstance").Should().Be("1234");
            parsedAgent.GetValue("ApplicationGitCommit").Should().Be("d71922715c2bfe29343644b14a4731bf5690e66e");
            parsedAgent.GetValue("ServerName").Should().Be("node123.datacenter.example.nl");
        }

        [Fact]
        public void TestAskingForImpossibleField()
        {
            var uaa = UserAgentAnalyzer
            .NewBuilder()
            .WithoutCache()
            .HideMatcherLoadStats()
            .DelayInitialization()
            .WithField("FirstNonexistentField")
            .WithField("DeviceClass")
            .WithField("SecondNonexistentField");
            Action a = new Action(() => uaa.Build());
            a.Should().Throw<InvalidParserConfigurationException>().WithMessage("We cannot provide these fields: [FirstNonexistentField] [SecondNonexistentField]");            
        }

        [Fact]
        public void TestDualBuilderUsageNoSecondInstance()
        {
            UserAgentAnalyzer.UserAgentAnalyzerBuilder builder = UserAgentAnalyzer.NewBuilder().DelayInitialization();

            builder.Build().Should().NotBeNull("We should get a first instance from a single builder.");
            // And calling build() again should fail with an exception
            Action a = new Action(() => builder.Build());
            a.Should().Throw<Exception>();
        }

        [Fact]
        public void TestDualBuilderUsageUseSetterAfterBuild()
        {
            UserAgentAnalyzer.UserAgentAnalyzerBuilder builder = UserAgentAnalyzer.NewBuilder().DelayInitialization();

            builder.Build().Should().NotBeNull("We should get a first instance from a single builder.");

            // And calling a setter after the build() should fail with an exception
            Action a = new Action(() => builder.WithCache(1234));
            a.Should().Throw<Exception>();
        }


        [Fact]
        public void TestLoadMoreResources()
        {
            UserAgentAnalyzer.UserAgentAnalyzerBuilder builder = UserAgentAnalyzer.NewBuilder().DelayInitialization().WithField("DeviceClass");

            UserAgentAnalyzer uaa = builder.Build();
            builder.Should().NotBeNull("We should get a first instance from a single builder.");

            uaa.InitializeMatchers();
            Action a = new Action(() => uaa.LoadResources("Something extra"));
            a.Should().Throw<Exception>();            
        }

        [Fact]
        public void TestPostPreheatDroptests()
        {
            UserAgentAnalyzer userAgentAnalyzer =
                UserAgentAnalyzer
                    .NewBuilder()
                    .ImmediateInitialization()
                    // Without .preheat(100)
                    .DropTests()
                    .HideMatcherLoadStats()
                    .WithField("AgentName")
                    .Build();
            userAgentAnalyzer.GetNumberOfTestCases().Should().Be(0);

            userAgentAnalyzer =
                UserAgentAnalyzer
                    .NewBuilder()
                    .ImmediateInitialization()
                    .Preheat(100) // With .preheat(100)
                    .DropTests()
                    .HideMatcherLoadStats()
                    .WithField("AgentName")
                    .Build();
            userAgentAnalyzer.GetNumberOfTestCases().Should().Be(0);
        }

    }
}
