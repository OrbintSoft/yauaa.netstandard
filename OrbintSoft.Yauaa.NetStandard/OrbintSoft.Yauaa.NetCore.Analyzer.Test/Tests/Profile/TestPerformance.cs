//-----------------------------------------------------------------------
// <copyright file="TestPerformance.cs" company="OrbintSoft">
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
namespace OrbintSoft.Yauaa.Testing.Tests.Profile
{
    using FluentAssertions;
    using log4net;
    using OrbintSoft.Yauaa.Analyzer;
    using OrbintSoft.Yauaa.Debug;
    using OrbintSoft.Yauaa.Testing.Fixtures;
    using System.Diagnostics;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="TestPerformance" />
    /// </summary>
    public class TestPerformance : IClassFixture<LogFixture>
    {
        /// <summary>
        /// Defines the LOG
        /// </summary>
        private static readonly ILog LOG = LogManager.GetLogger(typeof(TestMemoryFootprint));

        /// <summary>
        /// The ValidateAllPredefinedBrowsersPerformance
        /// </summary>
        [Fact]
        public void ValidateAllPredefinedBrowsersPerformance()
        {
            UserAgentAnalyzerTester uaa =
                UserAgentAnalyzerTester.NewBuilder()
                .ShowMatcherLoadStats()
                .ImmediateInitialization()
                .Build() as UserAgentAnalyzerTester;
            uaa.RunTests(false, false, null, true, true).Should().BeTrue();
        }

        /// <summary>
        /// The CheckAllPossibleFieldsFastSpeed
        /// </summary>
        [Fact]
        public void CheckAllPossibleFieldsFastSpeed()
        {
            LOG.Info("Create analyzer");
            Stopwatch stopwatch = Stopwatch.StartNew();
            UserAgentAnalyzer uaa = UserAgentAnalyzer
                .NewBuilder()
                .KeepTests()
                .DelayInitialization()
                .Build();
            stopwatch.Stop();
            long constructMsecs = stopwatch.ElapsedMilliseconds; ;
            LOG.Info(string.Format("-- Construction time: {0}ms", constructMsecs));

            LOG.Info("List fieldnames");
            stopwatch.Restart();
            uaa.GetAllPossibleFieldNamesSorted().ForEach(n => LOG.Info(n));
            stopwatch.Stop();
            long listFieldNamesMsecs = stopwatch.ElapsedMilliseconds;
            LOG.Info(string.Format("-- List fieldnames: {0}ms", listFieldNamesMsecs));
            listFieldNamesMsecs.Should().BeLessThan(500, "Just listing the field names should only take a few ms");

            LOG.Info("Initializing the datastructures");
            stopwatch.Restart();
            uaa.InitializeMatchers();
            stopwatch.Stop();
            long initializeMsecs = stopwatch.ElapsedMilliseconds;
            LOG.Info(string.Format("-- Initialization: {0}ms", initializeMsecs));
            initializeMsecs.Should().BeGreaterThan(1000, "The initialization should take several seconds");

            LOG.Info("Preheat");
            stopwatch.Restart();
            uaa.PreHeat();
            stopwatch.Stop();
            long preheatMsecs = stopwatch.ElapsedMilliseconds;
            LOG.Info(string.Format("-- Preheat : {0}ms", preheatMsecs));
        }
    }
}
