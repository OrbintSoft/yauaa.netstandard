﻿//-----------------------------------------------------------------------
// <copyright file="TestMatchesList.cs" company="OrbintSoft">
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
// <date>2018, 11, 24, 17:39</date>
// <summary></summary>
//-----------------------------------------------------------------------
namespace OrbintSoft.Yauaa.Testing.Tests.Analyze
{
    using FluentAssertions;
    using OrbintSoft.Yauaa.Analyze;
    using OrbintSoft.Yauaa.Testing.Fixtures;
    using System;
    using System.Collections.Generic;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="TestMatchesList" />
    /// </summary>
    public class TestMatchesList : IClassFixture<LogFixture>
    {
        /// <summary>
        /// The TestNormalUse
        /// </summary>
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
        public void TestTooMany()
        {
            MatchesList list = new MatchesList(5)
            {
                { "one", "two", null },
                { "three", "four", null }
            };
            IEnumerator<MatchesList.Match> iterator = list.GetEnumerator();

            iterator.MoveNext().Should().BeTrue();
            iterator.Current.Should().NotBeNull();
            iterator.MoveNext().Should().BeTrue();
            iterator.Current.Should().NotBeNull();
            iterator.MoveNext().Should().BeFalse();
            Action a = new Action(() => { var c = iterator.Current; }); 
            a.Should().Throw<IndexOutOfRangeException>();
        }

        /// <summary>
        /// The TestUnsupportedAdd
        /// </summary>
        [Fact]
        public void TestUnsupportedAdd()
        {
            new MatchesList(1).Invoking(m => m.Add(null)).Should().Throw<NotImplementedException>();
        }

        /// <summary>
        /// The TestUnsupportedRemove
        /// </summary>
        [Fact]
        public void TestUnsupportedRemove()
        {
            new MatchesList(1).Invoking(m => m.Remove(null)).Should().Throw<NotImplementedException>();
        }

        /// <summary>
        /// The TestUnsupportedContains
        /// </summary>
        [Fact]
        public void TestUnsupportedContains()
        {
            new MatchesList(1).Invoking(m => m.Contains(null)).Should().Throw<NotImplementedException>();
        }
    }
}