//-----------------------------------------------------------------------
// <copyright file="TestVersionCollisionChecks.cs" company="OrbintSoft">
//    Yet Another User Agent Analyzer for .NET Standard
//    porting realized by Stefano Balzarotti, Copyright 2018 (C) OrbintSoft
//
//    Original Author and License:
//
//    Yet Another UserAgent Analyzer
//    Copyright(C) 2013-2018 Niels Basjes
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
    using OrbintSoft.Yauaa.Analyze;
    using OrbintSoft.Yauaa.Analyzer;
    using OrbintSoft.Yauaa.Testing.Fixtures;
    using OrbintSoft.Yauaa.Tests;
    using System;
    using System.IO;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="TestVersionCollisionChecks" />
    /// </summary>
    public class TestVersionCollisionChecks : IClassFixture<LogFixture>
    {
        /// <summary>
        /// I test a yaml file with a bad version, loafd should fail
        /// </summary>
        [Fact]
        public void TestBadVersion()
        {
            var uaaB = UserAgentAnalyzer
             .NewBuilder()
             .DropDefaultResources()
             .AddResources($"{Config.RESOURCES_PATH}{Path.DirectorySeparatorChar}Versions", "BadVersion.yaml")
             .DelayInitialization();

            var a = new Action(() => uaaB.Build());
            a.Should().Throw<InvalidParserConfigurationException>().Where(e => e.Message.Contains("Found unexpected config entry: bad"));
        }

        /// <summary>
        /// The TestBadVersionNotMap
        /// </summary>
        [Fact]
        public void TestBadVersionNotMap()
        {

            var uaaB = UserAgentAnalyzer
            .NewBuilder()
            .DropDefaultResources()
            .AddResources($"{Config.RESOURCES_PATH}{Path.DirectorySeparatorChar}Versions", "BadVersionNotMap.yaml")
            .DelayInitialization();

            var a = new Action(() => uaaB.Build());
            a.Should().Throw<InvalidParserConfigurationException>().Where(e => e.Message.Contains("The value should be a string but it is a Sequence"));
        }

        /// <summary>
        /// The TestDifferentVersion
        /// </summary>
        [Fact]
        public void TestDifferentVersion()
        {
            var uaaB = UserAgentAnalyzer
             .NewBuilder()
             .DelayInitialization()
             .AddResources($"{Config.RESOURCES_PATH}{Path.DirectorySeparatorChar}Versions", "DifferentVersion.yaml");

            var a = new Action(() => uaaB.Build());
            a.Should().Throw<InvalidParserConfigurationException>().Where(e => e.Message.Contains("Two different Yauaa versions have been loaded:"));
        }

        /// <summary>
        /// The TestDoubleLoadedResources
        /// </summary>
        [Fact]
        public void TestDoubleLoadedResources()
        {
            var uaaB = UserAgentAnalyzer
             .NewBuilder()
             .DelayInitialization()
             .AddResources("YamlResources/Useragents");

            var a = new Action(() => uaaB.Build());
            a.Should().Throw<InvalidParserConfigurationException>().Where(e => e.Message.Contains("resources for the second time"));
        }
    }
}
