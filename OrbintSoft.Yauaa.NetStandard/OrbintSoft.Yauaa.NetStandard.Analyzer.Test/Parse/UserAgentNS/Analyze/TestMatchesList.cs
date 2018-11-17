//<copyright file="TestMatchesList.cs" company="OrbintSoft">
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
//<date>2018, 10, 8, 07:29</date>
//<summary></summary>

using FluentAssertions;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze;
using OrbintSoft.Yauaa.Analyzer.Test.Fixtures;
using System;
using System.Collections.Generic;
using Xunit;

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
            match1.Key.Should().Be("one");
            match1.Value.Should().Be("two");
            match1.GetResult().Should().BeNull();
            match2.Key.Should().Be("three");
            match2.Value.Should().Be("four");
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
