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
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Annotate;
using OrbintSoft.Yauaa.Analyzer.Test.Fixtures;
using System;
using Xunit;

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

        public abstract class BaseMapperWithoutGenericType<T> : IUserAgentAnnotationMapper<T>
        {
            public abstract string GetUserAgentString(T record);
        }

        public class MapperWithoutGenericType : BaseMapperWithoutGenericType<dynamic>
        {
            private UserAgentAnnotationAnalyzer<dynamic> userAgentAnalyzer;

            public MapperWithoutGenericType()
            {
                Type generic = typeof(UserAgentAnnotationAnalyzer<>);
                Type[] typeArgs = { null };
                
                var makeme = generic.MakeGenericType(typeArgs);
                userAgentAnalyzer = Activator.CreateInstance(makeme) as UserAgentAnnotationAnalyzer<dynamic>;
                userAgentAnalyzer.Initialize(this);
            }

            public object Enrich(object record)
            {
                return record;
            }

            public override string GetUserAgentString(dynamic record)
            {
                return null;
            }
        }

        [Fact]
        public void TestMissingTypeParameter()
        {
            
            //Action action = () => new MapperWithoutGenericType();
            ////in C# is not possible
            //action.Should().Throw<InvalidParserConfigurationException>().Which.Message.Should().StartWith("Couldn't find the used generic type of the UserAgentAnnotationMapper.");
        }
}
}
