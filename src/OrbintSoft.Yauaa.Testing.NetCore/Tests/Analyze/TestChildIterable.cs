//-----------------------------------------------------------------------
// <copyright file="TestChildIterable.cs" company="OrbintSoft">
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
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using FluentAssertions;
using OrbintSoft.Yauaa.Analyze.TreeWalker.Steps.Walk.StepDowns;
using OrbintSoft.Yauaa.Testing.Fixtures;
using System.Collections.Generic;
using Xunit;

namespace OrbintSoft.Yauaa.Testing.Tests.Analyze
{
    public class TestChildIterable : IClassFixture<LogFixture>
    {
        [Fact]
        public void TestEdges()
        {
            ChildIterable ci = new ChildIterable(true, 1, 5, x=> (true));

            ParserRuleContext prc = new ParserRuleContext();

            IEnumerator<IParseTree> iterator = ci.Iterator(prc);

            iterator.Current.Should().BeNull();
            iterator.MoveNext().Should().BeFalse();            
            iterator.Current.Should().BeNull();            
        }
    }
}
