//-----------------------------------------------------------------------
// <copyright file="TestBuilder.cs" company="OrbintSoft">
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
namespace OrbintSoft.Yauaa.Testing.Tests.Analyze
{
    using FluentAssertions;
    using OrbintSoft.Yauaa.Analyze;
    using OrbintSoft.Yauaa.Analyzer;
    using OrbintSoft.Yauaa.Testing.Fixtures;
    using OrbintSoft.Yauaa.Tests;
    using System;
    using Xunit;

    /// <summary>
    /// With this class I test the <see cref="UserAgentAnalyzer"/> Builder
    /// </summary>
    public class TestBuilder : IClassFixture<LogFixture>
    {
        /// <summary>
        /// This is an helper method to test if the created <see cref="UserAgentAnalyzer"/> works as exected. 
        /// </summary>
        /// <param name="userAgentAnalyzer">The userAgentAnalyzer<see cref="UserAgentAnalyzerDirect"/></param>
        private void RunTestCase(UserAgentAnalyzerDirect userAgentAnalyzer)
        {
            var parsedAgent = userAgentAnalyzer.Parse("Mozilla/5.0 (Linux; Android 7.0; Nexus 6 Build/NBD90Z) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.124 Mobile Safari/537.36");

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

        /// <summary>
        /// This method should test the <see cref="UserAgentAnalyzerDirect.UserAgentAnalyzerDirectBuilder{TUAA, TB}"/>, but by now I am unable to do in c# :(
        /// </summary>
        [Fact]
        public void TestLimitedFieldsDirect()
        {
            var builder = UserAgentAnalyzer.NewBuilder();
            var userAgentAnalyzer = builder
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

            this.RunTestCase(userAgentAnalyzer);
        }

        /// <summary>
        /// Here I test the <see cref="UserAgentAnalyzer.UserAgentAnalyzerBuilder"/> with limited fields.
        /// </summary>
        [Fact]
        public void TestLimitedFields()
        {
            var builder = UserAgentAnalyzer.NewBuilder();
            var userAgentAnalyzer = builder
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

            this.RunTestCase(userAgentAnalyzer);
        }

        /// <summary>
        /// Here I test if I can add addinional rules (custom fields) with the builder.
        /// </summary>
        [Fact]
        public void TestLoadAdditionalRules()
        {
            var userAgentAnalyzer =
                UserAgentAnalyzer
                    .NewBuilder()
                    .WithField("DeviceClass")
                    .WithoutCache()
                    .HideMatcherLoadStats()
                    .AddResources(Config.RESOURCES_PATH, "ExtraLoadedRule1.yaml")
                    .WithField("ExtraValue2")
                    .WithField("ExtraValue1")
                    .AddResources(Config.RESOURCES_PATH, "ExtraLoadedRule2.yaml")
                    .Build();

            var parsedAgent = userAgentAnalyzer.Parse("Mozilla/5.0 (Linux; Android 7.0; Nexus 6 Build/NBD90Z) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.124 Mobile Safari/537.36");

            // The requested fields
            parsedAgent.GetValue("DeviceClass").Should().Be("Phone");
            parsedAgent.GetValue("ExtraValue1").Should().Be("One");
            parsedAgent.GetValue("ExtraValue2").Should().Be("Two");
        }

        /// <summary>
        /// Here I test if I can load only custom rules and fields.
        /// </summary>
        [Fact]
        public void TestLoadOnlyCustomRules()
        {
            var userAgentAnalyzer =
                UserAgentAnalyzer
                    .NewBuilder()
                    .WithoutCache()
                    .HideMatcherLoadStats()
                    .AddResources(Config.RESOURCES_PATH, "ExtraLoadedRule1.yaml")
                    .WithField("ExtraValue2")
                    .WithField("ExtraValue1")
                    .AddResources(Config.RESOURCES_PATH, "ExtraLoadedRule2.yaml")
                    .Build();

            var parsedAgent = userAgentAnalyzer.Parse("Mozilla/5.0 (Linux; Android 7.0; Nexus 6 Build/NBD90Z) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.124 Mobile Safari/537.36");

            // The requested fields
            parsedAgent.GetValue("ExtraValue1").Should().Be("One");
            parsedAgent.GetValue("ExtraValue2").Should().Be("Two");
        }

        /// <summary>
        /// Here I test if I can load only custom comany internal user agents.
        /// </summary>
        [Fact]
        public void TestLoadOnlyCompanyCustomFormatRules()
        {
            var userAgentAnalyzer =
                UserAgentAnalyzer
                    .NewBuilder()
                    .WithoutCache()
                    .HideMatcherLoadStats()
                    .DropDefaultResources()
                    .AddResources(Config.RESOURCES_PATH, "CompanyInternalUserAgents.yaml")
                    .WithFields("ApplicationName", "ApplicationVersion")
                    .WithFields("ApplicationInstance", "ApplicationGitCommit")
                    .WithField("ServerName")
                    .Build();

            var parsedAgent = userAgentAnalyzer.Parse("TestApplication/1.2.3 (node123.datacenter.example.nl; 1234; d71922715c2bfe29343644b14a4731bf5690e66e)");

            // The requested fields
            parsedAgent.GetValue("ApplicationName").Should().Be("TestApplication");
            parsedAgent.GetValue("ApplicationVersion").Should().Be("1.2.3");
            parsedAgent.GetValue("ApplicationInstance").Should().Be("1234");
            parsedAgent.GetValue("ApplicationGitCommit").Should().Be("d71922715c2bfe29343644b14a4731bf5690e66e");
            parsedAgent.GetValue("ServerName").Should().Be("node123.datacenter.example.nl");
        }

        /// <summary>
        /// Here I test if I ask to build with an impossible field(non existent) the builder should throw a <see cref="InvalidParserConfigurationException"/>
        /// </summary>
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
            var a = new Action(() => uaa.Build());
            a.Should().Throw<InvalidParserConfigurationException>().WithMessage("We cannot provide these fields: [FirstNonexistentField] [SecondNonexistentField]");
        }

        /// <summary>
        /// Here I delay the initialization and call Build() a second time, builder can only return a single istance so it should thrown an exception.
        /// Sorry the builder is not thread safe.
        /// </summary>
        [Fact]
        public void TestDualBuilderUsageNoSecondInstance()
        {
            var builder = UserAgentAnalyzer.NewBuilder().DelayInitialization();

            builder.Build().Should().NotBeNull("We should get a first instance from a single builder.");
            // And calling build() again should fail with an exception
            var a = new Action(() => builder.Build());
            a.Should().Throw<Exception>();
        }

        /// <summary>
        /// Here I test that I can't set a property (example cache) from builder after I call Build(), even if initialization has been delayed, because
        /// the builder is not thread safe.
        /// </summary>
        [Fact]
        public void TestDualBuilderUsageUseSetterAfterBuild()
        {
            var builder = UserAgentAnalyzer.NewBuilder().DelayInitialization();

            builder.Build().Should().NotBeNull("We should get a first instance from a single builder.");

            // And calling a setter after the build() should fail with an exception
            var a = new Action(() => builder.WithCache(1234));
            a.Should().Throw<Exception>();
        }

        /// <summary>
        /// Here I test that I can't load other resources after I called Build();
        /// </summary>
        [Fact]
        public void TestLoadMoreResources()
        {
            var builder = UserAgentAnalyzer.NewBuilder().DelayInitialization().WithField("DeviceClass");

            var uaa = builder.Build();
            builder.Should().NotBeNull("We should get a first instance from a single builder.");

            uaa.InitializeMatchers();
            var a = new Action(() => uaa.LoadResources("Something extra"));
            a.Should().Throw<Exception>();
        }

        /// <summary>
        /// Here I test Preheat to optimize parsing performance after first startup.
        /// </summary>
        [Fact]
        public void TestPostPreheatDroptests()
        {
            var userAgentAnalyzer =
                UserAgentAnalyzer
                    .NewBuilder()
                    .ImmediateInitialization()
                    // Without .preheat(100)
                    .DropTests()
                    .HideMatcherLoadStats()
                    .WithField("AgentName")
                    .Build();
            userAgentAnalyzer.NumberOfTestCases.Should().Be(0);

            userAgentAnalyzer =
                UserAgentAnalyzer
                    .NewBuilder()
                    .ImmediateInitialization()
                    .Preheat(100) // With .preheat(100)
                    .DropTests()
                    .HideMatcherLoadStats()
                    .WithField("AgentName")
                    .Build();
            userAgentAnalyzer.NumberOfTestCases.Should().Be(0);
        }

        /// <summary>
        /// Here I test Preheat to optimize parsing performance after first startup, but keeping also tests.
        /// </summary>
        [Fact]
        public void TestPreheatNoTests()
        {
            var userAgentAnalyzer =
                UserAgentAnalyzer
                    .NewBuilder()
                    .KeepTests()
                    .HideMatcherLoadStats()
                    .WithField("AgentName")
                    .Build();

            userAgentAnalyzer.NumberOfTestCases.Should().BeGreaterThan(100);
            userAgentAnalyzer.PreHeat(0).Should().Be(0);
            userAgentAnalyzer.PreHeat(-1).Should().Be(0);
            userAgentAnalyzer.PreHeat(1000000000L).Should().Be(0);

            userAgentAnalyzer.DropTests();
            userAgentAnalyzer.NumberOfTestCases.Should().Be(0);
            userAgentAnalyzer.PreHeat().Should().Be(0);
        }
    }
}
