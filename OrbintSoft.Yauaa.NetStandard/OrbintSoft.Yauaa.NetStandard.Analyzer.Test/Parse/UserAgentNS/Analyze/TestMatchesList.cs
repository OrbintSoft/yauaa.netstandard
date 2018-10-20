using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze;
using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using OrbintSoft.Yauaa.Analyzer.Test.Fixtures;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS.Analyze
{
    public class TestMatchesList : IClassFixture<LogFixture>
    {
        [Fact]
        public void TestNormalUse()
        {
            MatchesList list = new MatchesList(5);
            list.Count.Should().Be(0);
            list.Add("one", "two", null);
            list.Count.Should().Be(1);
            list.Add("three", "four", null);
            list.Count.Should().Be(2);
            IEnumerator<MatchesList.Match> iterator = list.GetEnumerator();
            var next = iterator.MoveNext();
            next.Should().BeTrue();
            MatchesList.Match match1 = iterator.Current;
            next = iterator.MoveNext();
            next.Should().BeTrue();
            MatchesList.Match match2 = iterator.Current;
            next = iterator.MoveNext();
            next.Should().BeFalse();
            match1.GetKey().Should().Be("one");
            match1.GetValue().Should().Be("two");
            match1.GetResult().Should().BeNull();
            match2.GetKey().Should().Be("three");
            match2.GetValue().Should().Be("four");
            match2.GetResult().Should().BeNull();
        }

        [Fact]
        public void TestUnsupportedAdd()
        {
            new MatchesList(1).Invoking(m => m.Add(null)).Should().Throw<NotImplementedException>();
        }

        [Fact]
        public void TestUnsupportedRemove()
        {
            new MatchesList(1).Invoking(m => m.Remove(null)).Should().Throw<NotImplementedException>();
        }

        [Fact]
        public void TestUnsupportedContains()
        {
            new MatchesList(1).Invoking(m => m.Contains(null)).Should().Throw<NotImplementedException>();
        }
    }
}
