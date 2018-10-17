using System;
using Xunit;
using FluentAssertions;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Annotate;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze;
using OrbintSoft.Yauaa.Analyzer.Test.Fixtures;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS.Annotate
{
    public class TestAnnotationBadUsages : IClassFixture<LogFixture>
    {
        [Fact]
        public void TestNullInitAnalyzer()
        {
            UserAgentAnnotationAnalyzer<string> userAgentAnalyzer = new UserAgentAnnotationAnalyzer<string>();
            userAgentAnalyzer.Invoking(u => u.Initialize(null)).Should().Throw<InvalidParserConfigurationException>().Which.Message.Should().StartWith("[Initialize] The mapper instance is null.");
        }

        [Fact]
        public void TestNullInit()
        {
            UserAgentAnnotationAnalyzer<string> userAgentAnalyzer = new UserAgentAnnotationAnalyzer<string>();
            userAgentAnalyzer.Map(null).Should().BeNull();
        }

        [Fact]
        public void TestNoInit()
        {
            UserAgentAnnotationAnalyzer<string> userAgentAnalyzer = new UserAgentAnnotationAnalyzer<string>();
            userAgentAnalyzer.Invoking(u => u.Map("Foo")).Should().Throw<InvalidParserConfigurationException>().Which.Message.Should().StartWith("[Map] The mapper instance is null.");
        }

        public class MapperWithoutGenericType : IUserAgentAnnotationMapper<object>
        {
            private UserAgentAnnotationAnalyzer<dynamic> userAgentAnalyzer;

            public MapperWithoutGenericType()
            {
                userAgentAnalyzer = new UserAgentAnnotationAnalyzer<dynamic>();
                userAgentAnalyzer.Initialize(this);
            }

            public object Enrich(object record)
            {
                return record;
            }

            public string GetUserAgentString(object record)
            {
                return null;
            }
        }

        [Fact]
        public void TestMissingTypeParameter()
        {
            
            Action action = () => new MapperWithoutGenericType();
            //in C# is not possible
            //action.Should().Throw<InvalidParserConfigurationException>().Which.Message.Should().StartWith("Couldn't find the used generic type of the UserAgentAnnotationMapper.");
        }
}
}
