using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze;
using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using System.Linq;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS.Analyze
{
    public class TestNumberRangeVisitor
    {
        [Fact]
        public void RangeSingleValue()
        {
            IReadOnlyList<int> values = new NumberRangeList(5, 5);
            values.Count.Should().Be(1);
            values.Contains(1).Should().BeFalse();
            values.Contains(2).Should().BeFalse();
            values.Contains(3).Should().BeFalse();
            values.Contains(4).Should().BeFalse();

            values.Contains(5).Should().BeTrue();

            values.Contains(6).Should().BeFalse();
            values.Contains(7).Should().BeFalse();
            values.Contains(8).Should().BeFalse();
            values.Contains(9).Should().BeFalse();
        }

        [Fact]
        public void RangeMultipleValues()
        {
            IReadOnlyList<int> values = new NumberRangeList(3, 5);
            values.Count.Should().Be(3);
            values.Contains(1).Should().BeFalse();
            values.Contains(2).Should().BeFalse();

            values.Contains(3).Should().BeTrue();
            values.Contains(4).Should().BeTrue();
            values.Contains(5).Should().BeTrue();

            values.Contains(6).Should().BeFalse();
            values.Contains(7).Should().BeFalse();
            values.Contains(8).Should().BeFalse();
            values.Contains(9).Should().BeFalse();
        }
    }
}
