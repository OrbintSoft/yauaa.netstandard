//-----------------------------------------------------------------------
// <copyright file="TestMatcherList.cs" company="OrbintSoft">
//    Yet Another User Agent Analyzer for .NET Standard
//    porting realized by Stefano Balzarotti, Copyright 2019 (C) OrbintSoft
//
//    Original Author and License:
//
//    Yet Another UserAgent Analyzer
//    Copyright(C) 2013-2019 Niels Basjes
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
// <date>2019, 8, 11, 23:42</date>
// <summary></summary>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Testing.Tests.Analyze
{
    using FluentAssertions;
    using OrbintSoft.Yauaa.Analyze;
    using OrbintSoft.Yauaa.Testing.Fixtures;
    using System;
    using System.Linq;
    using Xunit;

    public class TestMatcherList : IClassFixture<LogFixture>
    {
        /// <summary>
        /// I check that collection iteration works as expected.
        /// </summary>
        [Fact]
        public void TestNormalUse()
        {
            var list = new MatcherList(1);
            list.Count.Should().Be(0);
            list.Any().Should().BeFalse();
            list.Add(new Matcher(null));
            list.Count.Should().Be(1);
            list.Any().Should().BeTrue();
            list.Add(new Matcher(null));
            list.Count.Should().Be(2);
            list.Any().Should().BeTrue();
            var iterator = list.GetEnumerator();
            iterator.MoveNext().Should().BeTrue();
            var match1 = iterator.Current;
            match1.Should().NotBeNull();
            iterator.MoveNext().Should().BeTrue();
            var match2 = iterator.Current;
            match2.Should().NotBeNull();
            iterator.MoveNext().Should().BeFalse();

            list.Clear();

            list.Count.Should().Be(0);
            list.Any().Should().BeFalse();
            list.Add(new Matcher(null));
            list.Count.Should().Be(1);
            list.Any().Should().BeTrue();
            list.Add(new Matcher(null));
            list.Count.Should().Be(2);
            list.Any().Should().BeTrue();
            iterator = list.GetEnumerator();
            iterator.MoveNext().Should().BeTrue();
            match1 = iterator.Current;
            match1.Should().NotBeNull();
            iterator.MoveNext().Should().BeTrue();
            match2 = iterator.Current;
            match2.Should().NotBeNull();
            iterator.MoveNext().Should().BeFalse();
        }

        /// <summary>
        /// I check that if loop outside the range it throws an IndexOutOfRange Exception.
        /// </summary>
        [Fact]
        public void TestTooMany()
        {
            var list = new MatcherList(5)
            {
                new Matcher(null),
                new Matcher(null)
            };
            var iterator = list.GetEnumerator();

            iterator.MoveNext().Should().BeTrue();
            iterator.Current.Should().NotBeNull();
            iterator.MoveNext().Should().BeTrue();
            iterator.Current.Should().NotBeNull();
            iterator.MoveNext().Should().BeFalse();
            var a = new Action(() => { var c = iterator.Current; });
            a.Should().Throw<IndexOutOfRangeException>();
        }

        /// <summary>
        /// For now Remove is not supported, as in Java library, but I don't like this kind of partial implementation, I should implement it in future.
        /// </summary>
        [Fact]
        public void TestUnsupportedRemove()
        {
            new MatcherList(1).Invoking(m => m.Remove(null)).Should().Throw<NotImplementedException>();
        }

        /// <summary>
        /// For now Contains is not supported, as in Java library, but I don't like this kind of partial implementation, I should implement it in future.
        /// </summary>
        [Fact]
        public void TestUnsupportedContains()
        {
            new MatcherList(1).Invoking(m => m.Contains(null)).Should().Throw<NotImplementedException>();
        }
    }
}
