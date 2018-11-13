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

using FluentAssertions;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze;
using OrbintSoft.Yauaa.Analyzer.Test.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS.Analyze
{
    public class TestNumberRangeVisitor : IClassFixture<LogFixture>
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

        [Fact]
        public void TestRangeCompare()
        {
            WordRangeVisitor.Range range1 = new WordRangeVisitor.Range(1, 2);
            WordRangeVisitor.Range range1b = new WordRangeVisitor.Range(1, 2);
            WordRangeVisitor.Range range2 = new WordRangeVisitor.Range(2, 1);
            WordRangeVisitor.Range range3 = new WordRangeVisitor.Range(1, 1);
            WordRangeVisitor.Range range4 = new WordRangeVisitor.Range(2, 2);
            string notARange = "Range";

            range1.Should().BeEquivalentTo(range1b);
            range1.Equals(range2).Should().BeFalse();
            range1.Equals(range3).Should().BeFalse();
            range1.Equals(range4).Should().BeFalse();
            range1.Equals(notARange).Should().BeFalse();
        }
    }
}
