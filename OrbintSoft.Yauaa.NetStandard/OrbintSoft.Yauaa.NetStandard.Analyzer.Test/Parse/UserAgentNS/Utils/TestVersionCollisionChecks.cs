/*
 * Yet Another UserAgent Analyzer .NET Standard
 * Porting realized by Balzarotti Stefano, Copyright (C) OrbintSoft
 * 
 * Original Author and License:
 * 
 * Yet Another UserAgent Analyzer
 * Copyright (C) 2013-2018 Niels Basjes
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * All rights should be reserved to the original author Niels Basjes
 */

using FluentAssertions;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze;
using OrbintSoft.Yauaa.Analyzer.Test.Fixtures;
using System;
using Xunit;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS.Utils
{
    public class TestVersionCollisionChecks : IClassFixture<LogFixture>
    {
        [Fact]
        public void TestBadVersion()
        {
           UserAgentAnalyzer uaa = UserAgentAnalyzer
            .NewBuilder()
            .DropDefaultResources()
            .DelayInitialization()
            .Build();

            Action a = new Action(() => uaa.LoadResources("YamlResources/Versions", "BadVersion.yaml"));
            a.Should().Throw<InvalidParserConfigurationException>().Where(e => e.Message.Contains("Found unexpected config entry: bad"));
        }

        [Fact]
        public void TestBadVersionNotMap()
        {

            UserAgentAnalyzer uaa = UserAgentAnalyzer
            .NewBuilder()
            .DropDefaultResources()
            .DelayInitialization()
            .Build();

            Action a = new Action(() => uaa.LoadResources("YamlResources/Versions", "BadVersionNotMap.yaml"));
            a.Should().Throw<InvalidParserConfigurationException>().Where(e => e.Message.Contains("The value should be a string but it is a sequence"));
        }

    [Fact]
        public void TestDifferentVersion()
        {
            UserAgentAnalyzer uaa = UserAgentAnalyzer
             .NewBuilder()
             .DelayInitialization()
             .Build();

            Action a = new Action(() => uaa.LoadResources("YamlResources/Versions", "DifferentVersion.yaml"));
            a.Should().Throw<InvalidParserConfigurationException>().Where(e => e.Message.Contains("Two different Yauaa versions have been loaded:"));
        }

        [Fact]
        public void TestDoubleLoadedResources()
        {
            UserAgentAnalyzer uaa = UserAgentAnalyzer
             .NewBuilder()
             .DelayInitialization()
             .Build();

            Action a = new Action(() => uaa.LoadResources("YamlResources/Useragents"));
            a.Should().Throw<InvalidParserConfigurationException>().Where(e => e.Message.Contains("resources for the second time"));
        }
    }
}
