//-----------------------------------------------------------------------
// <copyright file="DocumentationExample.cs" company="OrbintSoft">
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
namespace OrbintSoft.Yauaa.Testing.Tests.Basic
{
    using FluentAssertions;
    using OrbintSoft.Yauaa.Debug;
    using OrbintSoft.Yauaa.Testing.Fixtures;
    using OrbintSoft.Yauaa.Tests;
    using Xunit;

    /// <summary>
    /// This test class, tests tthe examples used in the Yauaa Documentation.
    /// </summary>
    public class DocumentationExample : IClassFixture<LogFixture>
    {
        /// <summary>
        /// This tests the user agent : Mozilla/5.0 (Linux; Android 7.0; Nexus 6 Build/NBD90Z) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.124 Mobile Safari/537.36
        /// If the test results change, the documentation must be updated.
        /// </summary>
        [Fact]
        public void RunDocumentationExample()
        {
            var uaa = UserAgentAnalyzerTester
            .NewBuilder()
            .DropDefaultResources()
            .AddResources(Config.RESOURCES_PATH, "DocumentationExample.yaml")
            .Build() as UserAgentAnalyzerTester;           
            uaa.RunTests(false, true).Should().BeTrue();
        }
    }
}
