using System;
using Xunit;
using FluentAssertions;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Annotate;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS.Annotate
{
    public class TestAnnotationBadUsages
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
            private UserAgentAnnotationAnalyzer<object> userAgentAnalyzer;

            public MapperWithoutGenericType()
            {
                userAgentAnalyzer = new UserAgentAnnotationAnalyzer<object>();
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
            action.Should().Throw<InvalidParserConfigurationException>().Which.Message.Should().StartWith("Couldn't find the used generic type of the UserAgentAnnotationMapper.");
        }
}
}
