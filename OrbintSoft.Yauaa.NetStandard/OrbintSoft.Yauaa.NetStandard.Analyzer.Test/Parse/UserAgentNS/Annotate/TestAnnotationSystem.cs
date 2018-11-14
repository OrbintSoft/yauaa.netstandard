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
    public class TestAnnotationSystem : IClassFixture<LogFixture>
    {

        public class TestRecord
        {
            internal readonly string useragent;
            internal string deviceClass;
            internal string agentNameVersion;

            public TestRecord(string useragent)
            {
                this.useragent = useragent;
            }
        }

        public class MyBaseMapper: IUserAgentAnnotationMapper<TestRecord>
        {
            private readonly UserAgentAnnotationAnalyzer<TestRecord> userAgentAnalyzer;

            public MyBaseMapper()
            {
                userAgentAnalyzer = new UserAgentAnnotationAnalyzer<TestRecord>();
                userAgentAnalyzer.Initialize(this);
            }

            public TestRecord Enrich(TestRecord record)
            {
                return userAgentAnalyzer.Map(record);
            }
        
            public string GetUserAgentString(TestRecord record)
            {
                return record.useragent;
            }
        }

        public class MyMapper: MyBaseMapper
        {
            [YauaaField("DeviceClass")]
            public void SetDeviceClass(TestRecord record, string value)
            {
                record.deviceClass = value;
            }

            [YauaaField("AgentNameVersion")]
            public void SetAgentNameVersion(TestRecord record, string value)
            {
                record.agentNameVersion = value;
            }
        }

        [Fact]
        public void TestAnnotationBasedParser()
        {
            MyMapper mapper = new MyMapper();

            TestRecord record = new TestRecord("Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) " +
                "Chrome/48.0.2564.82 Safari/537.36");

            record = mapper.Enrich(record);
            record.deviceClass.Should().Be("Desktop");
            record.agentNameVersion.Should().Be("Chrome 48.0.2564.82");
        }

        public class ImpossibleFieldMapper : MyBaseMapper
        {
            [YauaaField("NielsBasjes")]
            public void SetImpossibleField(TestRecord record, string value)
            {
                record.agentNameVersion = value;
            }
        }

        [Fact]
        public void TestImpossibleField()
        {
            Action a = new Action(() => new ImpossibleFieldMapper());
            a.Should().Throw<InvalidParserConfigurationException>().WithMessage("We cannot provide these fields: [NielsBasjes]");
        }


        public class InaccessibleSetterMapper : MyBaseMapper
        {
            [YauaaField("DeviceClass")]
            private void InaccessibleSetter(TestRecord record, string value)
            {
                throw new Xunit.Sdk.XunitException("May NEVER call this method");
            }
        }

        [Fact]
        public void TestInaccessibleSetter()
        {
            Action a = new Action(() => new InaccessibleSetterMapper());
            a.Should().Throw<InvalidParserConfigurationException>().WithMessage("Method annotated with YauaaField is not public: inaccessibleSetter");
        }

        public class TooManyParameters : MyBaseMapper
        {
            [YauaaField("DeviceClass")]
            public void WrongSetter(TestRecord record, string value, string extra)
            {
                throw new Xunit.Sdk.XunitException("May NEVER call this method");
            }
        }

        [Fact]
        public void TestTooManyParameters()
        {
            Action a = new Action(() => new TooManyParameters());
            a.Should().Throw<InvalidParserConfigurationException>().WithMessage("In class [TooManyParameters] the method [WrongSetter] has been annotated with YauaaField but it has the wrong method signature. It must look like [ public void WrongSetter(TestRecord record, String value) ]");
        }

        public class WrongTypeParameters1: MyBaseMapper
        {
            [YauaaField("DeviceClass")]
            public void WrongSetter(string record, string value)
            {
                throw new Xunit.Sdk.XunitException("May NEVER call this method");
            }
        }

        [Fact]
        public void TestWrongTypeParameters1()
        {
            Action a = new Action(() => new WrongTypeParameters1());
            a.Should().Throw<InvalidParserConfigurationException>().WithMessage("In class [WrongTypeParameters1] the method [WrongSetter] has been annotated with YauaaField but it has the wrong method signature. It must look like [ public void WrongSetter(TestRecord record, String value) ]");
        }

        public class WrongTypeParameters2 : MyBaseMapper
        {
            [YauaaField("DeviceClass")]
            public void WrongSetter(TestRecord record, double value)
            {
                throw new Xunit.Sdk.XunitException("May NEVER call this method");
            }
        }

        [Fact]
        public void TestWrongTypeParameters2()
        {
            Action a = new Action(() => new WrongTypeParameters2());
            a.Should().Throw<InvalidParserConfigurationException>().WithMessage("In class [WrongTypeParameters2] the method [WrongSetter] has been annotated with YauaaField but it has the wrong method signature. It must look like [ public void WrongSetter(TestRecord record, String value) ]");
        }

        public class MissingAnnotations : MyBaseMapper
        {
            public void SetWasNotAnnotated(TestRecord record, double value)
            {
                throw new Xunit.Sdk.XunitException("May NEVER call this method");
            }
        }

        [Fact]
        public void TestMissingAnnotations()
        {
            Action a = new Action(() => new MissingAnnotations());
            a.Should().Throw<InvalidParserConfigurationException>().WithMessage("You MUST specify at least 1 field to extract.");
        }

        [Fact]
        public void TestBadGeneric()
        {          
            UserAgentAnnotationAnalyzer<object> userAgentAnalyzer = new UserAgentAnnotationAnalyzer<object>();
            Action a = new Action(() => userAgentAnalyzer.Map("Foo"));
            a.Should().Throw<InvalidParserConfigurationException>().WithMessage("[Map] The mapper instance is null.");
        }

}
}
