//-----------------------------------------------------------------------
// <copyright file="SplitterBenchmarks.cs" company="OrbintSoft">
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
// <date>2018, 11, 19, 16:21</date>
// <summary></summary>
//-----------------------------------------------------------------------
namespace OrbintSoft.Yauaa.Benchmarking
{
    using BenchmarkDotNet.Attributes;
    using OrbintSoft.Yauaa.Analyze;
    using OrbintSoft.Yauaa.Utils;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the <see cref="SplitterBenchmarks" />
    /// </summary>
    public class SplitterBenchmarks
    {
        /// <summary>
        /// Defines the TEXT
        /// </summary>
        private const string TEXT = "one two/3 four-4 five(some more)";

        /// <summary>
        /// Defines the splitter
        /// </summary>
        private WordSplitter splitter;

        /// <summary>
        /// Defines the allRanges
        /// </summary>
        private List<WordRangeVisitor.Range> allRanges;

        /// <summary>
        /// Defines the ranges1
        /// </summary>
        private List<WordRangeVisitor.Range> ranges1;

        /// <summary>
        /// Defines the ranges2
        /// </summary>
        private List<WordRangeVisitor.Range> ranges2;

        /// <summary>
        /// Defines the ranges3
        /// </summary>
        private List<WordRangeVisitor.Range> ranges3;

        /// <summary>
        /// Defines the ranges4
        /// </summary>
        private List<WordRangeVisitor.Range> ranges4;

        /// <summary>
        /// Defines the ranges5
        /// </summary>
        private List<WordRangeVisitor.Range> ranges5;

        /// <summary>
        /// Defines the ranges6
        /// </summary>
        private List<WordRangeVisitor.Range> ranges6;

        /// <summary>
        /// Defines the ranges7
        /// </summary>
        private List<WordRangeVisitor.Range> ranges7;

        /// <summary>
        /// Defines the ranges8
        /// </summary>
        private List<WordRangeVisitor.Range> ranges8;

        /// <summary>
        /// Defines the ranges9
        /// </summary>
        private List<WordRangeVisitor.Range> ranges9;

        /// <summary>
        /// The GlobalSetup
        /// </summary>
        [GlobalSetup]
        public void GlobalSetup()
        {
            splitter = WordSplitter.GetInstance();
            allRanges = new List<WordRangeVisitor.Range>(32)
            {
                new WordRangeVisitor.Range(1, 1),
                new WordRangeVisitor.Range(1, 2),
                new WordRangeVisitor.Range(3, 4),
                new WordRangeVisitor.Range(2, 4),
                new WordRangeVisitor.Range(4, 5),
                new WordRangeVisitor.Range(5, 6),
                new WordRangeVisitor.Range(3, 5),
                new WordRangeVisitor.Range(4, 6),
                new WordRangeVisitor.Range(2, 2),
                new WordRangeVisitor.Range(1, 3)
            };

            ranges1 = allRanges.GetRange(1, 1);
            ranges2 = allRanges.GetRange(1, 2);
            ranges3 = allRanges.GetRange(1, 3);
            ranges4 = allRanges.GetRange(1, 4);
            ranges5 = allRanges.GetRange(1, 5);
            ranges6 = allRanges.GetRange(1, 6);
            ranges7 = allRanges.GetRange(1, 7);
            ranges8 = allRanges.GetRange(1, 8);
            ranges9 = allRanges.GetRange(1, 9);
        }

        /// <summary>
        /// The RunDirect
        /// </summary>
        /// <param name="splitter">The splitter<see cref="Splitter"/></param>
        /// <param name="ranges">The ranges<see cref="List{WordRangeVisitor.Range}"/></param>
        public void RunDirect(Splitter splitter, List<WordRangeVisitor.Range> ranges)
        {
            foreach (WordRangeVisitor.Range range in ranges)
            {
                splitter.GetSplitRange(TEXT, range);
            }
        }

        /// <summary>
        /// The RunSplitList
        /// </summary>
        /// <param name="splitter">The splitter<see cref="Splitter"/></param>
        /// <param name="ranges">The ranges<see cref="List{WordRangeVisitor.Range}"/></param>
        public void RunSplitList(Splitter splitter, List<WordRangeVisitor.Range> ranges)
        {
            List<Tuple<int, int>> splitList = splitter.CreateSplitList(TEXT);
            foreach (WordRangeVisitor.Range range in ranges)
            {
                splitter.GetSplitRange(TEXT, splitList, range);
            }
        }

        /// <summary>
        /// The DirectRange1
        /// </summary>
        [Benchmark]
        public void DirectRange1()
        {
            RunDirect(splitter, ranges1);
        }

        /// <summary>
        /// The SplitLRange1
        /// </summary>
        [Benchmark]
        public void SplitLRange1()
        {
            RunSplitList(splitter, ranges1);
        }

        /// <summary>
        /// The DirectRange2
        /// </summary>
        [Benchmark]
        public void DirectRange2()
        {
            RunDirect(splitter, ranges2);
        }

        /// <summary>
        /// The SplitLRange2
        /// </summary>
        [Benchmark]
        public void SplitLRange2()
        {
            RunSplitList(splitter, ranges2);
        }

        /// <summary>
        /// The DirectRange3
        /// </summary>
        [Benchmark]
        public void DirectRange3()
        {
            RunDirect(splitter, ranges3);
        }

        /// <summary>
        /// The SplitLRange3
        /// </summary>
        [Benchmark]
        public void SplitLRange3()
        {
            RunSplitList(splitter, ranges3);
        }

        /// <summary>
        /// The DirectRange4
        /// </summary>
        [Benchmark]
        public void DirectRange4()
        {
            RunDirect(splitter, ranges4);
        }

        /// <summary>
        /// The SplitLRange4
        /// </summary>
        [Benchmark]
        public void SplitLRange4()
        {
            RunSplitList(splitter, ranges4);
        }

        /// <summary>
        /// The DirectRange5
        /// </summary>
        [Benchmark]
        public void DirectRange5()
        {
            RunDirect(splitter, ranges5);
        }

        /// <summary>
        /// The SplitLRange5
        /// </summary>
        [Benchmark]
        public void SplitLRange5()
        {
            RunSplitList(splitter, ranges5);
        }

        /// <summary>
        /// The DirectRange6
        /// </summary>
        [Benchmark]
        public void DirectRange6()
        {
            RunDirect(splitter, ranges6);
        }

        /// <summary>
        /// The SplitLRange6
        /// </summary>
        [Benchmark]
        public void SplitLRange6()
        {
            RunSplitList(splitter, ranges6);
        }

        /// <summary>
        /// The DirectRange7
        /// </summary>
        [Benchmark]
        public void DirectRange7()
        {
            RunDirect(splitter, ranges7);
        }

        /// <summary>
        /// The SplitLRange7
        /// </summary>
        [Benchmark]
        public void SplitLRange7()
        {
            RunSplitList(splitter, ranges7);
        }

        /// <summary>
        /// The DirectRange8
        /// </summary>
        [Benchmark]
        public void DirectRange8()
        {
            RunDirect(splitter, ranges8);
        }

        /// <summary>
        /// The SplitLRange8
        /// </summary>
        [Benchmark]
        public void SplitLRange8()
        {
            RunSplitList(splitter, ranges8);
        }

        /// <summary>
        /// The DirectRange9
        /// </summary>
        [Benchmark]
        public void DirectRange9()
        {
            RunDirect(splitter, ranges9);
        }

        /// <summary>
        /// The SplitLRange9
        /// </summary>
        [Benchmark]
        public void SplitLRange9()
        {
            RunSplitList(splitter, ranges9);
        }
    }
}
