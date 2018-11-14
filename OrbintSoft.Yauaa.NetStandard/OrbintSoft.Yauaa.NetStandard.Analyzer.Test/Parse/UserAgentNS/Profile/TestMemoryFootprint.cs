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

using log4net;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS;
using OrbintSoft.Yauaa.Analyzer.Test.Fixtures;
using System;
using System.Diagnostics;
using Xunit;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS.Profile
{

    public class TestMemoryFootprint : IClassFixture<LogFixture>
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(TestMemoryFootprint));

        private static readonly long MEGABYTE = 1024L * 1024L;

        public static long BytesToMegabytes(long bytes)
        {
            return bytes / MEGABYTE;
        }

        private void PrintMemoryUsage(int iterationsDone, long averageNanos)
        {
            // Calculate the used memory
            Process currentProcess = Process.GetCurrentProcess();
            long memory = currentProcess.WorkingSet64;
            LOG.Info(string.Format(
                "After {0} iterations and GC --> Used memory is {1} bytes ({2} MiB), Average time per parse {3} ns ( ~ {4} ms)",
                iterationsDone, memory, BytesToMegabytes(memory), averageNanos, averageNanos));
        }

        [SkippableFact]
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

        [SkippableFact]
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

        [SkippableFact]
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
