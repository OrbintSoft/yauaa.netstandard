using OrbintSoft.Yauaa.Testing.Fixtures;
using OrbintSoft.Yauaa.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using System.Diagnostics;
using log4net;

namespace OrbintSoft.Yauaa.Testing.Tests.Utils
{
    public class TestPrefixLookup : IClassFixture<LogFixture>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TestPrefixLookup));

        [Fact]
        public void TestLookupCaseSensitive()
        {
            var prefixMap = new Dictionary<string, string>
            {
                ["A"] = "Result A",
                ["AB"] = "Result AB",
                ["ABC"] = "Result ABC",
                // The ABCD is missing !!!
                ["ABCDE"] = "Result ABCDE",
                ["ABCX"] = "Result ABCX",
                ["ABCDX"] = "Result ABCDX"
            };

            var prefixLookup = new PrefixLookup(prefixMap, true);

            prefixLookup.FindLongestMatchingPrefix("MisMatch").Should().BeNull();

            prefixLookup.FindLongestMatchingPrefix("A").Should().Be("Result A") ;
            prefixLookup.FindLongestMatchingPrefix("AB").Should().Be("Result AB");
            prefixLookup.FindLongestMatchingPrefix("ABC").Should().Be("Result ABC");
            prefixLookup.FindLongestMatchingPrefix("ABCD").Should().Be("Result ABC");
            prefixLookup.FindLongestMatchingPrefix("ABCDE").Should().Be("Result ABCDE");

            prefixLookup.FindLongestMatchingPrefix("ABCD").Should().Be("Result ABC");
            prefixLookup.FindLongestMatchingPrefix("AB Something").Should().Be("Result AB");
            prefixLookup.FindLongestMatchingPrefix("AAAA").Should().Be("Result A");

            prefixLookup.FindLongestMatchingPrefix("AB€").Should().Be("Result AB");
            prefixLookup.FindLongestMatchingPrefix("AB\t").Should().Be("Result AB");
        }

        [Fact]
        public void TestLookupCaseInsensitive()
        {
            var prefixMap = new Dictionary<string, string>
            {
                ["A"] = "Result A",
                ["AB"] = "Result AB",
                ["ABC"] = "Result ABC",
                // The ABCD is missing !!!
                ["ABCDE"] = "Result ABCDE",
                ["ABCX"] = "Result ABCX",
                ["ABCDX"] = "Result ABCDX"
            };

            var prefixLookup = new PrefixLookup(prefixMap, false);

            prefixLookup.FindLongestMatchingPrefix("MisMatch").Should().BeNull();

            prefixLookup.FindLongestMatchingPrefix("A").Should().Be("Result A");
            prefixLookup.FindLongestMatchingPrefix("AB").Should().Be("Result AB");
            prefixLookup.FindLongestMatchingPrefix("ABC").Should().Be("Result ABC");
            prefixLookup.FindLongestMatchingPrefix("ABCD").Should().Be("Result ABC");
            prefixLookup.FindLongestMatchingPrefix("ABCDE").Should().Be("Result ABCDE");

            prefixLookup.FindLongestMatchingPrefix("ABCD").Should().Be("Result ABC");
            prefixLookup.FindLongestMatchingPrefix("AB Something").Should().Be("Result AB");
            prefixLookup.FindLongestMatchingPrefix("AAAA").Should().Be("Result A");

            prefixLookup.FindLongestMatchingPrefix("AB€").Should().Be("Result AB");
            prefixLookup.FindLongestMatchingPrefix("AB\t").Should().Be("Result AB");

            prefixLookup.FindLongestMatchingPrefix("a").Should().Be("Result A");
            prefixLookup.FindLongestMatchingPrefix("ab").Should().Be("Result AB");
            prefixLookup.FindLongestMatchingPrefix("abc").Should().Be("Result ABC");
            prefixLookup.FindLongestMatchingPrefix("abcd").Should().Be("Result ABC");
            prefixLookup.FindLongestMatchingPrefix("abcde").Should().Be("Result ABCDE");
            
            prefixLookup.FindLongestMatchingPrefix("abcd").Should().Be("Result ABC");
            prefixLookup.FindLongestMatchingPrefix("ab something").Should().Be("Result AB");
            prefixLookup.FindLongestMatchingPrefix("aaaa").Should().Be("Result A");

            prefixLookup.FindLongestMatchingPrefix("ab€").Should().Be("Result AB");
            prefixLookup.FindLongestMatchingPrefix("ab\t").Should().Be("Result AB");
        }

        [Fact]
        public void TestStoreNonASCII()
        {
            var prefixMap = new Dictionary<string, string>
            {
                ["12€"] = "Euro"
            };
            var action = new Action(() => { new PrefixLookup(prefixMap, false); }).Should().Throw<ArgumentException>();
        }

        [Fact]
        public void TestStoreNonASCIITab()
        {
            var prefixMap = new Dictionary<string, string>
            {
                ["12\t"] = "Euro"
            };
            var action = new Action(() => { new PrefixLookup(prefixMap, false); }).Should().Throw<ArgumentException>();
        }

        [Fact]
        public void TestLookupSpeed()
        {
            var prefixMap = new Dictionary<string, string>
            {
                ["1"] = "Result 1"
            };
            for (var i = 10; i < 1000; i++)
            {
                prefixMap["" + i] = "Something";
            }
            var prefixLookup = new PrefixLookup(prefixMap, false);

            long iterations = 100_000_000;

            var stopWatch = Stopwatch.StartNew();
            for (var i = 0; i < iterations; i++)
            {
                prefixLookup.FindLongestMatchingPrefix("999");
            }
            stopWatch.Stop();
            Log.Info(string.Format("Speed stats: {0} runs took {1}ms --> {2}us",
                iterations, stopWatch.ElapsedMilliseconds, stopWatch.Elapsed.TotalMilliseconds * 1000));
            prefixLookup.FindLongestMatchingPrefix("1").Should().Be("Result 1");
        }
    }
}
