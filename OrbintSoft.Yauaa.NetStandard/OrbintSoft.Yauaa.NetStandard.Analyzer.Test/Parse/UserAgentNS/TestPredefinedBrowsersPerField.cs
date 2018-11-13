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
using log4net;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Debug;
using OrbintSoft.Yauaa.Analyzer.Test.Fixtures;
using System.Collections.Generic;
using Xunit;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS
{
    public class TestPredefinedBrowsersPerField : IClassFixture<LogFixture>
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(TestPredefinedBrowsersPerField));

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

        [Theory]
        [MemberData(nameof(Data))]
        public void ValidateAllPredefinedBrowsersForField(string fieldName)
        {
            HashSet<string> singleFieldList = new HashSet<string>();
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
            singleFieldList.Clear();
            singleFieldList.Add(fieldName);
            userAgentAnalyzer.Should().NotBeNull();
            userAgentAnalyzer.RunTests(false, true, singleFieldList, false, false).Should().BeTrue();
        }
    }
}
