using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Utils;
using Xunit;
using FluentAssertions;
using System.Collections.Generic;
using System;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS.Analyze
{
    public class TestWordSplitter
    {
        [Fact]
        public void VersionSplitterEmpty()
        {
            string value = "";
            Splitter splitter = VersionSplitter.GetInstance();
            splitter.GetSingleSplit(value, 0).Should().BeNull();
            splitter.GetSingleSplit(value, 1).Should().BeNull();
            splitter.GetSingleSplit(value, 2).Should().BeNull();

            splitter.GetFirstSplits(value, 0).Should().BeNull();
            splitter.GetFirstSplits(value, 1).Should().BeNull();
            splitter.GetFirstSplits(value, 2).Should().BeNull();
        }

        [Fact]
        public void VersionSplitterDOT()
        {
            string value = "1.2.3";
            Splitter splitter = VersionSplitter.GetInstance();

            splitter.GetSingleSplit(value, 0).Should().BeNull();
            splitter.GetSingleSplit(value, 1).Should().Be("1");
            splitter.GetSingleSplit(value, 2).Should().Be("2");
            splitter.GetSingleSplit(value, 3).Should().Be("3");
            splitter.GetSingleSplit(value, 4).Should().BeNull();

            splitter.GetFirstSplits(value, 0).Should().BeNull();
            splitter.GetFirstSplits(value, 1).Should().Be("1");
            splitter.GetFirstSplits(value, 2).Should().Be("1.2");
            splitter.GetFirstSplits(value, 3).Should().Be("1.2.3");
            splitter.GetFirstSplits(value, 4).Should().BeNull();
        }

        [Fact]
        public void VersionSplitterUS()
        {
            string value = "1_2_3";
            Splitter splitter = VersionSplitter.GetInstance();

            splitter.GetSingleSplit(value, 0).Should().BeNull();
            splitter.GetSingleSplit(value, 1).Should().Be("1");
            splitter.GetSingleSplit(value, 2).Should().Be("2");
            splitter.GetSingleSplit(value, 3).Should().Be("3");
            splitter.GetSingleSplit(value, 4).Should().BeNull();

            splitter.GetFirstSplits(value, 0).Should().BeNull();
            splitter.GetFirstSplits(value, 1).Should().Be("1");
            splitter.GetFirstSplits(value, 2).Should().Be("1_2");
            splitter.GetFirstSplits(value, 3).Should().Be("1_2_3");
            splitter.GetFirstSplits(value, 4).Should().BeNull();
        }

        [Fact]
        public void VersionSplitterMIX1()
        {
            string value = "1_2.3";
            Splitter splitter = VersionSplitter.GetInstance();

            splitter.GetSingleSplit(value, 0).Should().BeNull();
            splitter.GetSingleSplit(value, 1).Should().Be("1");
            splitter.GetSingleSplit(value, 2).Should().Be("2");
            splitter.GetSingleSplit(value, 3).Should().Be("3");
            splitter.GetSingleSplit(value, 4).Should().BeNull();

            splitter.GetFirstSplits(value, 0).Should().BeNull();
            splitter.GetFirstSplits(value, 1).Should().Be("1");
            splitter.GetFirstSplits(value, 2).Should().Be("1_2");
            splitter.GetFirstSplits(value, 3).Should().Be("1_2.3");
            splitter.GetFirstSplits(value, 4).Should().BeNull();
        }

        [Fact]
        public void VersionSplitterMIX2()
        {
            string value = "1.2_3";
            Splitter splitter = VersionSplitter.GetInstance();

            splitter.GetSingleSplit(value, 0).Should().BeNull();
            splitter.GetSingleSplit(value, 1).Should().Be("1");
            splitter.GetSingleSplit(value, 2).Should().Be("2");
            splitter.GetSingleSplit(value, 3).Should().Be("3");
            splitter.GetSingleSplit(value, 4).Should().BeNull();

            splitter.GetFirstSplits(value, 0).Should().BeNull();
            splitter.GetFirstSplits(value, 1).Should().Be("1");
            splitter.GetFirstSplits(value, 2).Should().Be("1.2");
            splitter.GetFirstSplits(value, 3).Should().Be("1.2_3");
            splitter.GetFirstSplits(value, 4).Should().BeNull();
        }

        [Fact]
        public void VersionSplitterWWW1()
        {
            string value = "www.bar.com";
            Splitter splitter = VersionSplitter.GetInstance();

            splitter.GetSingleSplit(value, 1).Should().Be("www.bar.com");
            splitter.GetSingleSplit(value, 2).Should().BeNull();

            splitter.GetFirstSplits(value, 1).Should().Be("www.bar.com");
            splitter.GetFirstSplits(value, 2).Should().BeNull();
        }

        [Fact]
        public void VersionSplitterEMail()
        {
            string value = "foo@bar.com";
            Splitter splitter = VersionSplitter.GetInstance();

            splitter.GetSingleSplit(value, 1).Should().Be("foo@bar.com");
            splitter.GetSingleSplit(value, 2).Should().BeNull();

            splitter.GetFirstSplits(value, 1).Should().Be("foo@bar.com");
            splitter.GetFirstSplits(value, 2).Should().BeNull();
        }

        [Fact]
        public void VersionSplitterRanges()
        {
            string value = "1.2.3.4.5";
            Splitter splitter = VersionSplitter.GetInstance();

            // Single version
            splitter.GetSplitRange(value, 0, 0).Should().BeNull();
            splitter.GetSplitRange(value, 1, 1).Should().Be("1");
            splitter.GetSplitRange(value, 2, 2).Should().Be("2");
            splitter.GetSplitRange(value, 3, 3).Should().Be("3");
            splitter.GetSplitRange(value, 4, 4).Should().Be("4");
            splitter.GetSplitRange(value, 5, 5).Should().Be("5");
            splitter.GetSplitRange(value, 6, 6).Should().BeNull();

            // First versions
            splitter.GetSplitRange(value, 1, 0).Should().BeNull();
            splitter.GetSplitRange(value, 1, 1).Should().Be("1");
            splitter.GetSplitRange(value, 1, 2).Should().Be("1.2");
            splitter.GetSplitRange(value, 1, 3).Should().Be("1.2.3");
            splitter.GetSplitRange(value, 1, 4).Should().Be("1.2.3.4");
            splitter.GetSplitRange(value, 1, 5).Should().Be("1.2.3.4.5");
            splitter.GetSplitRange(value, 1, 6).Should().BeNull();

            //// Last versions
            splitter.GetSplitRange(value, 0, -1).Should().BeNull();
            splitter.GetSplitRange(value, 1, -1).Should().Be("1.2.3.4.5");
            splitter.GetSplitRange(value, 2, -1).Should().Be("2.3.4.5");
            splitter.GetSplitRange(value, 3, -1).Should().Be("3.4.5");
            splitter.GetSplitRange(value, 4, -1).Should().Be("4.5");
            splitter.GetSplitRange(value, 5, -1).Should().Be("5");
            splitter.GetSplitRange(value, 6, -1).Should().BeNull();

            //// 2 version slice
            splitter.GetSplitRange(value, 0, 1).Should().BeNull();
            splitter.GetSplitRange(value, 1, 2).Should().Be("1.2");
            splitter.GetSplitRange(value, 2, 3).Should().Be("2.3");
            splitter.GetSplitRange(value, 3, 4).Should().Be("3.4");
            splitter.GetSplitRange(value, 4, 5).Should().Be("4.5");
            splitter.GetSplitRange(value, 5, 6).Should().BeNull();
        }

        [Fact]
        public void WordSplitterEmpty()
        {
            string value = "";
            Splitter splitter = WordSplitter.GetInstance();
            splitter.GetSingleSplit(value, 0).Should().BeNull();
            splitter.GetSingleSplit(value, 1).Should().BeNull();
            splitter.GetSingleSplit(value, 2).Should().BeNull();

            splitter.GetFirstSplits(value, 0).Should().BeNull();
            splitter.GetFirstSplits(value, 1).Should().BeNull();
            splitter.GetFirstSplits(value, 2).Should().BeNull();
        }

        [Fact]
        public void WordSplitterTest()
        {
            string value = "one two/3 four-4 five(some more)";
            Splitter splitter = WordSplitter.GetInstance();

            splitter.GetSingleSplit(value, 0).Should().BeNull();
            splitter.GetSingleSplit(value, 1).Should().Be("one");
            splitter.GetSingleSplit(value, 2).Should().Be("two");
            splitter.GetSingleSplit(value, 3).Should().Be("3");
            splitter.GetSingleSplit(value, 4).Should().Be("four");
            splitter.GetSingleSplit(value, 5).Should().Be("4");
            splitter.GetSingleSplit(value, 6).Should().Be("five");
            splitter.GetSingleSplit(value, 7).Should().BeNull();

            splitter.GetFirstSplits(value, 0).Should().BeNull();
            splitter.GetFirstSplits(value, 1).Should().Be("one");
            splitter.GetFirstSplits(value, 2).Should().Be("one two");
            splitter.GetFirstSplits(value, 3).Should().Be("one two/3");
            splitter.GetFirstSplits(value, 4).Should().Be("one two/3 four");
            splitter.GetFirstSplits(value, 5).Should().Be("one two/3 four-4");
            splitter.GetFirstSplits(value, 6).Should().Be("one two/3 four-4 five");
            splitter.GetFirstSplits(value, 7).Should().BeNull();
        }

        [Fact]
        public void WordSplitterRange()
        {
            string value = "one two/3 four-4 five(some more)";
            // The '(' is one of the string terminators for this string splitter

            Splitter splitter = WordSplitter.GetInstance();

            // Single word
            splitter.GetSplitRange(value, 0, 0).Should().BeNull();
            splitter.GetSplitRange(value, 1, 1).Should().Be("one");
            splitter.GetSplitRange(value, 2, 2).Should().Be("two");
            splitter.GetSplitRange(value, 3, 3).Should().Be("3");
            splitter.GetSplitRange(value, 4, 4).Should().Be("four");
            splitter.GetSplitRange(value, 5, 5).Should().Be("4");
            splitter.GetSplitRange(value, 6, 6).Should().Be("five");
            splitter.GetSplitRange(value, 7, 7).Should().BeNull();

            // First words
            splitter.GetSplitRange(value, 1, 1).Should().Be("one");
            splitter.GetSplitRange(value, 1, 2).Should().Be("one two");
            splitter.GetSplitRange(value, 1, 3).Should().Be("one two/3");
            splitter.GetSplitRange(value, 1, 4).Should().Be("one two/3 four");
            splitter.GetSplitRange(value, 1, 5).Should().Be("one two/3 four-4");
            splitter.GetSplitRange(value, 1, 6).Should().Be("one two/3 four-4 five");
            splitter.GetSplitRange(value, 1, 7).Should().BeNull();

            // Last words
            splitter.GetSplitRange(value, 1, -1).Should().Be("one two/3 four-4 five");
            splitter.GetSplitRange(value, 2, -1).Should().Be("two/3 four-4 five");
            splitter.GetSplitRange(value, 3, -1).Should().Be("3 four-4 five");
            splitter.GetSplitRange(value, 4, -1).Should().Be("four-4 five");
            splitter.GetSplitRange(value, 5, -1).Should().Be("4 five");
            splitter.GetSplitRange(value, 6, -1).Should().Be("five");
            splitter.GetSplitRange(value, 7, -1).Should().BeNull();

            // 2 word slices
            splitter.GetSplitRange(value, 0, 1).Should().BeNull();
            splitter.GetSplitRange(value, 1, 2).Should().Be("one two");
            splitter.GetSplitRange(value, 2, 3).Should().Be("two/3");
            splitter.GetSplitRange(value, 3, 4).Should().Be("3 four");
            splitter.GetSplitRange(value, 4, 5).Should().Be("four-4");
            splitter.GetSplitRange(value, 5, 6).Should().Be("4 five");
            splitter.GetSplitRange(value, 6, 7).Should().BeNull();

            // 3 word slices
            splitter.GetSplitRange(value, 0, 2).Should().BeNull();
            splitter.GetSplitRange(value, 1, 3).Should().Be("one two/3");
            splitter.GetSplitRange(value, 2, 4).Should().Be("two/3 four");
            splitter.GetSplitRange(value, 3, 5).Should().Be("3 four-4");
            splitter.GetSplitRange(value, 4, 6).Should().Be("four-4 five");
            splitter.GetSplitRange(value, 5, 7).Should().BeNull();

            // Edge cases
            splitter.GetSplitRange(value, 0, 0).Should().BeNull();
            splitter.GetSplitRange(value, 0, -1).Should().BeNull();
            splitter.GetSplitRange(value, -1, -1).Should().BeNull();
        }

        [Fact]
        public void TestSplitList()
        {
            string value = "one two/3 four-4 five(some more)";
            Splitter splitter = WordSplitter.GetInstance();

            List<Tuple<int, int>> splitList = splitter.CreateSplitList(value);

            // Single word
            splitter.GetSplitRange(value, splitList, 0, 0).Should().BeNull();
            splitter.GetSplitRange(value, splitList, 1, 1).Should().Be("one");
            splitter.GetSplitRange(value, splitList, 2, 2).Should().Be("two");
            splitter.GetSplitRange(value, splitList, 3, 3).Should().Be("3");
            splitter.GetSplitRange(value, splitList, 4, 4).Should().Be("four");
            splitter.GetSplitRange(value, splitList, 5, 5).Should().Be("4");
            splitter.GetSplitRange(value, splitList, 6, 6).Should().Be("five");
            splitter.GetSplitRange(value, splitList, 7, 7).Should().BeNull();

            // First words
            splitter.GetSplitRange(value, splitList, 1, 1).Should().Be("one");
            splitter.GetSplitRange(value, splitList, 1, 2).Should().Be("one two");
            splitter.GetSplitRange(value, splitList, 1, 3).Should().Be("one two/3");
            splitter.GetSplitRange(value, splitList, 1, 4).Should().Be("one two/3 four");
            splitter.GetSplitRange(value, splitList, 1, 5).Should().Be("one two/3 four-4");
            splitter.GetSplitRange(value, splitList, 1, 6).Should().Be("one two/3 four-4 five");
            splitter.GetSplitRange(value, splitList, 1, 7).Should().BeNull();

            // Last words
            splitter.GetSplitRange(value, splitList, 1, -1).Should().Be("one two/3 four-4 five");
            splitter.GetSplitRange(value, splitList, 2, -1).Should().Be("two/3 four-4 five");
            splitter.GetSplitRange(value, splitList, 3, -1).Should().Be("3 four-4 five");
            splitter.GetSplitRange(value, splitList, 4, -1).Should().Be("four-4 five");
            splitter.GetSplitRange(value, splitList, 5, -1).Should().Be("4 five");
            splitter.GetSplitRange(value, splitList, 6, -1).Should().Be("five");
            splitter.GetSplitRange(value, splitList, 7, -1).Should().BeNull();

            // 2 word slices
            splitter.GetSplitRange(value, splitList, 0, 1).Should().BeNull();
            splitter.GetSplitRange(value, splitList, 1, 2).Should().Be("one two");
            splitter.GetSplitRange(value, splitList, 2, 3).Should().Be("two/3");
            splitter.GetSplitRange(value, splitList, 3, 4).Should().Be("3 four");
            splitter.GetSplitRange(value, splitList, 4, 5).Should().Be("four-4");
            splitter.GetSplitRange(value, splitList, 5, 6).Should().Be("4 five");
            splitter.GetSplitRange(value, splitList, 6, 7).Should().BeNull();

            // 3 word slices
            splitter.GetSplitRange(value, splitList, 0, 2).Should().BeNull();
            splitter.GetSplitRange(value, splitList, 1, 3).Should().Be("one two/3");
            splitter.GetSplitRange(value, splitList, 2, 4).Should().Be("two/3 four");
            splitter.GetSplitRange(value, splitList, 3, 5).Should().Be("3 four-4");
            splitter.GetSplitRange(value, splitList, 4, 6).Should().Be("four-4 five");
            splitter.GetSplitRange(value, splitList, 5, 7).Should().BeNull();

            // Edge cases
            splitter.GetSplitRange(value, splitList, 0, 0).Should().BeNull();
            splitter.GetSplitRange(value, splitList, 0, -1).Should().BeNull();
            splitter.GetSplitRange(value, splitList, -1, -1).Should().BeNull();
        }

        [Fact]
        public void BadCalls()
        {
            string value = "one two/3 four-4 five(some more)";
            Splitter splitter = WordSplitter.GetInstance();

            splitter.GetSplitRange(null, 1, 2).Should().BeNull();
            splitter.GetSplitRange(value, -1, 2).Should().BeNull();
            splitter.GetSplitRange(value, 1, -2).Should().BeNull();
            splitter.GetSplitRange(value, 3, 2).Should().BeNull();
        }

    }
}
