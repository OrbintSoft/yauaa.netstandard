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
