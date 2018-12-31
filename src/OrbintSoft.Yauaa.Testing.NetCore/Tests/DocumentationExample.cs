//-----------------------------------------------------------------------
// <copyright file="DocumentationExample.cs" company="OrbintSoft">
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
namespace OrbintSoft.Yauaa.Testing.Tests
{
    using FluentAssertions;
    using OrbintSoft.Yauaa.Debug;
    using OrbintSoft.Yauaa.Testing.Fixtures;
    using OrbintSoft.Yauaa.Tests;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="DocumentationExample" />
    /// </summary>
    public class DocumentationExample : IClassFixture<LogFixture>
    {
        /// <summary>
        /// The RunDocumentationExample
        /// </summary>
        [Fact]
        public void RunDocumentationExample()
        {
            var uaa = new UserAgentAnalyzerTester(Config.RESOURCES_PATH , "DocumentationExample.yaml");
            uaa.RunTests(false, true).Should().BeTrue();
        }
    }
}
