//-----------------------------------------------------------------------
// <copyright file="TestPredefinedBrowsersPerField.cs" company="OrbintSoft">
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
    using log4net;
    using OrbintSoft.Yauaa.Analyzer;
    using OrbintSoft.Yauaa.Debug;
    using OrbintSoft.Yauaa.Testing.Fixtures;
    using System.Collections.Generic;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="TestPredefinedBrowsersPerField" />
    /// </summary>
    public class TestPredefinedBrowsersPerField : IClassFixture<LogFixture>
    {
        /// <summary>
        /// Defines the LOG
        /// </summary>
        private static readonly ILog LOG = LogManager.GetLogger(typeof(TestPredefinedBrowsersPerField));

        /// <summary>
        /// The Data
        /// </summary>
        /// <returns>The <see cref="IEnumerable{object[]}"/></returns>
        public static IEnumerable<object[]> Data()
        {
            var fieldNames = UserAgentAnalyzer
                .NewBuilder()
                .HideMatcherLoadStats()
                .DelayInitialization()
                .Build()
                .GetAllPossibleFieldNamesSorted();
            foreach (var fieldName in fieldNames)
            {
                yield return new object[] { fieldName };
            }
        }

        /// <summary>
        /// The ValidateAllPredefinedBrowsersForField
        /// </summary>
        /// <param name="fieldName">The fieldName<see cref="string"/></param>
        [Theory]
        [MemberData(nameof(Data))]
        public void ValidateAllPredefinedBrowsersForField(string fieldName)
        {
            ICollection<string> singleFieldList = new string[] { fieldName };
            LOG.Info("==============================================================");
            LOG.Info(string.Format("Validating when ONLY asking for {0}", fieldName));
            LOG.Info("--------------------------------------------------------------");
            UserAgentAnalyzerTester userAgentAnalyzer =
                UserAgentAnalyzerTester
                    .NewBuilder()
                    .WithoutCache()
                    .WithField(fieldName)
                    .HideMatcherLoadStats()
                    .Build() as UserAgentAnalyzerTester;
            userAgentAnalyzer.Should().NotBeNull();
            userAgentAnalyzer.RunTests(false, true, singleFieldList, false, false).Should().BeTrue();
        }
    }
}
