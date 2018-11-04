using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Debug;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using OrbintSoft.Yauaa.Analyzer.Test.Fixtures;
using log4net;
using System.Diagnostics;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS.Profile
{
    public class TestPerformance : IClassFixture<LogFixture>
    {

        private static readonly ILog LOG = LogManager.GetLogger(typeof(TestMemoryFootprint));

        [SkippableFact]
        public void ValidateAllPredefinedBrowsersPerformance()
        {
            UserAgentAnalyzerTester uaa =
                UserAgentAnalyzerTester.NewBuilder()
                .ShowMatcherLoadStats()
                .ImmediateInitialization()
                .Build() as UserAgentAnalyzerTester;
            uaa.RunTests(false, false, null, true, true).Should().BeTrue();
        }

        [SkippableFact]
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
