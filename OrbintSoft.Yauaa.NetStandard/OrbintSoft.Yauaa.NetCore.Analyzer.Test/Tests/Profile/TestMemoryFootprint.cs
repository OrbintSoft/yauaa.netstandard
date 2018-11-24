//<copyright file="TestMemoryFootprint.cs" company="OrbintSoft">
//	Yet Another UserAgent Analyzer.NET Standard
//	Porting realized by Stefano Balzarotti, Copyright (C) OrbintSoft
//
//	Original Author and License:
//
//	Yet Another UserAgent Analyzer
//	Copyright(C) 2013-2018 Niels Basjes
//
//	Licensed under the Apache License, Version 2.0 (the "License");
//	you may not use this file except in compliance with the License.
//	You may obtain a copy of the License at
//
//	https://www.apache.org/licenses/LICENSE-2.0
//
//	Unless required by applicable law or agreed to in writing, software
//	distributed under the License is distributed on an "AS IS" BASIS,
//	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//	See the License for the specific language governing permissions and
//	limitations under the License.
//
//</copyright>
//<author>Stefano Balzarotti, Niels Basjes</author>
//<date>2018, 11, 14, 20:22</date>
//<summary></summary>

namespace OrbintSoft.Yauaa.Testing.Tests.Profile
{
    using log4net;
    using OrbintSoft.Yauaa.Analyzer;
    using OrbintSoft.Yauaa.Testing.Fixtures;
    using System;
    using System.Diagnostics;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="TestMemoryFootprint" />
    /// </summary>
    public class TestMemoryFootprint : IClassFixture<LogFixture>
    {
        /// <summary>
        /// Defines the LOG
        /// </summary>
        private static readonly ILog LOG = LogManager.GetLogger(typeof(TestMemoryFootprint));

        /// <summary>
        /// Defines the MEGABYTE
        /// </summary>
        private static readonly long MEGABYTE = 1024L * 1024L;

        /// <summary>
        /// The BytesToMegabytes
        /// </summary>
        /// <param name="bytes">The bytes<see cref="long"/></param>
        /// <returns>The <see cref="long"/></returns>
        public static long BytesToMegabytes(long bytes)
        {
            return bytes / MEGABYTE;
        }

        /// <summary>
        /// The PrintMemoryUsage
        /// </summary>
        /// <param name="iterationsDone">The iterationsDone<see cref="int"/></param>
        /// <param name="averageNanos">The averageNanos<see cref="long"/></param>
        private void PrintMemoryUsage(int iterationsDone, long averageNanos)
        {
            // Calculate the used memory
            Process currentProcess = Process.GetCurrentProcess();
            long memory = currentProcess.WorkingSet64;
            LOG.Info(string.Format(
                "After {0} iterations and GC --> Used memory is {1} bytes ({2} MiB), Average time per parse {3} ns ( ~ {4} ms)",
                iterationsDone, memory, BytesToMegabytes(memory), averageNanos, averageNanos));
        }

        /// <summary>
        /// The CheckForMemoryLeaks
        /// </summary>
        [Fact]
        public void CheckForMemoryLeaks()
        {
            UserAgentAnalyzer uaa = UserAgentAnalyzer
                .NewBuilder()
                .WithoutCache()
                //            .withField("OperatingSystemName")
                //            .withField("OperatingSystemVersion")
                //            .withField("DeviceClass")
                .HideMatcherLoadStats()
                .KeepTests()
                .Build();

            LOG.Info("Init complete");
            int iterationsDone = 0;
            int iterationsPerLoop = 1000;
            for (int i = 0; i < 100; i++)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                uaa.PreHeat(iterationsPerLoop, false);
                stopwatch.Stop();
                iterationsDone += iterationsPerLoop;

                long averageNanos = (stopwatch.ElapsedMilliseconds) / iterationsPerLoop;
                PrintMemoryUsage(iterationsDone, averageNanos);
            }
        }

        /// <summary>
        /// The AssesMemoryImpactPerFieldName
        /// </summary>
        [Fact]
        public void AssesMemoryImpactPerFieldName()
        {
            UserAgentAnalyzer uaa = UserAgentAnalyzer
                .NewBuilder()
                .HideMatcherLoadStats()
                .WithoutCache()
                .KeepTests()
                .Build();

            uaa.PreHeat();
            GC.Collect();
            uaa.PreHeat();
            GC.Collect();

            // Calculate the used memory
            Process currentProcess = Process.GetCurrentProcess();
            long memory = currentProcess.VirtualMemorySize64;
            LOG.Error(string.Format(
                "Querying for 'All fields' and GC --> Used memory is {0} bytes ({1} MiB)",
                memory, BytesToMegabytes(memory)));

            foreach (string fieldName in uaa.GetAllPossibleFieldNamesSorted())
            {
                uaa = UserAgentAnalyzer
                    .NewBuilder()
                    .WithoutCache()
                    .WithField(fieldName)
                    .HideMatcherLoadStats()
                    .KeepTests()
                    .Build();

                uaa.PreHeat();
                GC.Collect();
                uaa.PreHeat();
                GC.Collect();

                // Calculate the used memory
                memory = memory = currentProcess.WorkingSet64;
                LOG.Error(string.Format(
                    "Querying for {0} and GC --> Used memory is {1} bytes ({2} MiB)",
                    fieldName, memory, BytesToMegabytes(memory)));
            }
        }

        /// <summary>
        /// The PrintCurrentMemoryProfile
        /// </summary>
        /// <param name="label">The label<see cref="string"/></param>
        private void PrintCurrentMemoryProfile(string label)
        {
            GC.Collect();
            Process currentProcess = Process.GetCurrentProcess();
            // Calculate the used memory
            long memory = currentProcess.PrivateMemorySize64;
            LOG.Info(string.Format(
                "----- %{0}: Used memory is {1} bytes ({2} MiB / {3} MiB)",
                label, memory, BytesToMegabytes(memory), BytesToMegabytes(currentProcess.PeakWorkingSet64)));
        }

        /// <summary>
        /// The ProfileMemoryFootprint
        /// </summary>
        [Fact]
        public void ProfileMemoryFootprint()
        {
            PrintCurrentMemoryProfile("Before ");

            UserAgentAnalyzer uaa = UserAgentAnalyzer
                .NewBuilder()
                .HideMatcherLoadStats()
                .WithoutCache()
                .KeepTests()
                .Build();
            PrintCurrentMemoryProfile("Loaded ");

            uaa.InitializeMatchers();
            PrintCurrentMemoryProfile("Init   ");

            GC.Collect();
            PrintCurrentMemoryProfile("Post GC");

            uaa.SetCacheSize(1000);
            uaa.PreHeat();
            GC.Collect();
            PrintCurrentMemoryProfile("Cache 1K");

            uaa.SetCacheSize(10000);
            uaa.PreHeat();
            GC.Collect();
            PrintCurrentMemoryProfile("Cache 10K");

            uaa.DropTests();
            GC.Collect();
            PrintCurrentMemoryProfile("NoTest ");
        }
    }
}
