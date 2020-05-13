//-----------------------------------------------------------------------
// <copyright file="TestMatchesList.cs" company="OrbintSoft">
//    Yet Another User Agent Analyzer for .NET Standard
//    porting realized by Stefano Balzarotti, Copyright 2018-2019 (C) OrbintSoft
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
// <date>2018, 11, 24, 17:39</date>
// <summary></summary>
//-----------------------------------------------------------------------
using FluentAssertions;
using OrbintSoft.Yauaa.Analyze;
using OrbintSoft.Yauaa.Testing.Fixtures;
using System;
using System.Collections.Generic;
using Xunit;

namespace OrbintSoft.Yauaa.Testing.Tests.Analyze
{
    /// <summary>
    /// This class tests that iteration thought  MatchesList works as expected.
    /// </summary>
    public class TestMatchesList : IClassFixture<LogFixture>
    {
        /// <summary>
        /// I test a normal use of the list.
        /// </summary>
        [Fact]
        public void TestNormalUse()
        {
            var list = new MatchesList(5);
            list.Count.Should().Be(0);
            list.Add("one", "two", null);
            list.Count.Should().Be(1);
            list.Add("three", "four", null);
            list.Count.Should().Be(2);
            var iterator = list.GetEnumerator();
            var next = iterator.MoveNext();
            next.Should().BeTrue();
            var match1 = iterator.Current;
            next = iterator.MoveNext();
            next.Should().BeTrue();
            var match2 = iterator.Current;
            next = iterator.MoveNext();
            next.Should().BeFalse();
            match1.Key.Should().Be("one");
            match1.Value.Should().Be("two");
            match1.Result.Should().BeNull();
            match2.Key.Should().Be("three");
            match2.Value.Should().Be("four");
            match2.Result.Should().BeNull();
        }

        /// <summary>
        /// I test if I try to iterate for too many elements, it should throw an exception. 
        /// </summary>
        [Fact]
        public void TestTooMany()
        {
            var list = new MatchesList(5)
            {
                { "one", "two", null },
                { "three", "four", null }
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
    }
}
