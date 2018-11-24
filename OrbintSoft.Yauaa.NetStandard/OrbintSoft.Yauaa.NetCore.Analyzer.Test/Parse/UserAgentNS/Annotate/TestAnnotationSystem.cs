//<copyright file="TestAnnotationSystem.cs" company="OrbintSoft">
//	Yet Another UserAgent Analyzer.NET Standard
//	Porting realized by Stefano Balzarotti, Copyright (C) OrbintSoft
//
//	Original Author and License:
//
//	Yet Another UserAgent Analyzer
//	Copyright(C) 2013-2018 Niels Basjes
//
//	Licensed under the Apache License, Version 2.0 (the "License");
//	you may not use this file except in compliance with the License.
//	You may obtain a copy of the License at
//
//	https://www.apache.org/licenses/LICENSE-2.0
//
//	Unless required by applicable law or agreed to in writing, software
//	distributed under the License is distributed on an "AS IS" BASIS,
//	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//	See the License for the specific language governing permissions and
//	limitations under the License.
//
//</copyright>
//<author>Stefano Balzarotti, Niels Basjes</author>
//<date>2018, 10, 13, 20:51</date>
//<summary></summary>

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS.Annotate
{
    using FluentAssertions;
    using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze;
    using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Annotate;
    using OrbintSoft.Yauaa.Analyzer.Test.Fixtures;
    using System;
    using Xunit;
    using Xunit.Sdk;

    /// <summary>
    /// Defines the <see cref="TestAnnotationSystem" />
    /// </summary>
    public class TestAnnotationSystem : IClassFixture<LogFixture>
    {
        /// <summary>
        /// Defines the <see cref="TestRecord" />
        /// </summary>
        public class TestRecord
        {
            /// <summary>
            /// Defines the useragent
            /// </summary>
            internal readonly string useragent;

            /// <summary>
            /// Defines the deviceClass
            /// </summary>
            internal string deviceClass;

            /// <summary>
            /// Defines the agentNameVersion
            /// </summary>
            internal string agentNameVersion;

            /// <summary>
            /// Initializes a new instance of the <see cref="TestRecord"/> class.
            /// </summary>
            /// <param name="useragent">The useragent<see cref="string"/></param>
            public TestRecord(string useragent)
            {
                this.useragent = useragent;
            }
        }

        /// <summary>
        /// Defines the <see cref="MyBaseMapper" />
        /// </summary>
        public class MyBaseMapper : IUserAgentAnnotationMapper<TestRecord>
        {
            /// <summary>
            /// Defines the userAgentAnalyzer
            /// </summary>
            private readonly UserAgentAnnotationAnalyzer<TestRecord> userAgentAnalyzer;

            /// <summary>
            /// Initializes a new instance of the <see cref="MyBaseMapper"/> class.
            /// </summary>
            public MyBaseMapper()
            {
                userAgentAnalyzer = new UserAgentAnnotationAnalyzer<TestRecord>();
                userAgentAnalyzer.Initialize(this);
            }

            /// <summary>
            /// The Enrich
            /// </summary>
            /// <param name="record">The record<see cref="TestRecord"/></param>
            /// <returns>The <see cref="TestRecord"/></returns>
            public TestRecord Enrich(TestRecord record)
            {
                return userAgentAnalyzer.Map(record);
            }

            /// <summary>
            /// The GetUserAgentString
            /// </summary>
            /// <param name="record">The record<see cref="TestRecord"/></param>
            /// <returns>The <see cref="string"/></returns>
            public string GetUserAgentString(TestRecord record)
            {
                return record.useragent;
            }
        }

        /// <summary>
        /// Defines the <see cref="MyMapper" />
        /// </summary>
        public class MyMapper : MyBaseMapper
        {
            /// <summary>
            /// The SetDeviceClass
            /// </summary>
            /// <param name="record">The record<see cref="TestRecord"/></param>
            /// <param name="value">The value<see cref="string"/></param>
            [YauaaField("DeviceClass")]
            public void SetDeviceClass(TestRecord record, string value)
            {
                record.deviceClass = value;
            }

            /// <summary>
            /// The SetAgentNameVersion
            /// </summary>
            /// <param name="record">The record<see cref="TestRecord"/></param>
            /// <param name="value">The value<see cref="string"/></param>
            [YauaaField("AgentNameVersion")]
            public void SetAgentNameVersion(TestRecord record, string value)
            {
                record.agentNameVersion = value;
            }
        }

        /// <summary>
        /// The TestAnnotationBasedParser
        /// </summary>
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

        /// <summary>
        /// Defines the <see cref="ImpossibleFieldMapper" />
        /// </summary>
        public class ImpossibleFieldMapper : MyBaseMapper
        {
            /// <summary>
            /// The SetImpossibleField
            /// </summary>
            /// <param name="record">The record<see cref="TestRecord"/></param>
            /// <param name="value">The value<see cref="string"/></param>
            [YauaaField("NielsBasjes")]
            public void SetImpossibleField(TestRecord record, string value)
            {
                record.agentNameVersion = value;
            }
        }

        /// <summary>
        /// The TestImpossibleField
        /// </summary>
        [Fact]
        public void TestImpossibleField()
        {
            Action a = new Action(() => new ImpossibleFieldMapper());
            a.Should().Throw<InvalidParserConfigurationException>().WithMessage("We cannot provide these fields: [NielsBasjes]");
        }

        /// <summary>
        /// Defines the <see cref="InaccessibleSetterMapper" />
        /// </summary>
        public class InaccessibleSetterMapper : MyBaseMapper
        {
            /// <summary>
            /// The InaccessibleSetter
            /// </summary>
            /// <param name="record">The record<see cref="TestRecord"/></param>
            /// <param name="value">The value<see cref="string"/></param>
            [YauaaField("DeviceClass")]
            private void InaccessibleSetter(TestRecord record, string value)
            {
                throw new Xunit.Sdk.XunitException("May NEVER call this method");
            }
        }

        /// <summary>
        /// The TestInaccessibleSetter
        /// </summary>
        [Fact]
        public void TestInaccessibleSetter()
        {
            Action a = new Action(() => new InaccessibleSetterMapper());
            a.Should().Throw<InvalidParserConfigurationException>().WithMessage("Method annotated with YauaaField is not public: inaccessibleSetter");
        }

        /// <summary>
        /// Defines the <see cref="TooManyParameters" />
        /// </summary>
        public class TooManyParameters : MyBaseMapper
        {
            /// <summary>
            /// The WrongSetter
            /// </summary>
            /// <param name="record">The record<see cref="TestRecord"/></param>
            /// <param name="value">The value<see cref="string"/></param>
            /// <param name="extra">The extra<see cref="string"/></param>
            [YauaaField("DeviceClass")]
            public void WrongSetter(TestRecord record, string value, string extra)
            {
                throw new Xunit.Sdk.XunitException("May NEVER call this method");
            }
        }

        /// <summary>
        /// The TestTooManyParameters
        /// </summary>
        [Fact]
        public void TestTooManyParameters()
        {
            Action a = new Action(() => new TooManyParameters());
            a.Should().Throw<InvalidParserConfigurationException>().WithMessage("In class [TooManyParameters] the method [WrongSetter] has been annotated with YauaaField but it has the wrong method signature. It must look like [ public void WrongSetter(TestRecord record, String value) ]");
        }

        /// <summary>
        /// Defines the <see cref="WrongTypeParameters1" />
        /// </summary>
        public class WrongTypeParameters1 : MyBaseMapper
        {
            /// <summary>
            /// The WrongSetter
            /// </summary>
            /// <param name="record">The record<see cref="string"/></param>
            /// <param name="value">The value<see cref="string"/></param>
            [YauaaField("DeviceClass")]
            public void WrongSetter(string record, string value)
            {
                throw new XunitException("May NEVER call this method");
            }
        }

        /// <summary>
        /// The TestWrongTypeParameters1
        /// </summary>
        [Fact]
        public void TestWrongTypeParameters1()
        {
            Action a = new Action(() => new WrongTypeParameters1());
            a.Should().Throw<InvalidParserConfigurationException>().WithMessage("In class [WrongTypeParameters1] the method [WrongSetter] has been annotated with YauaaField but it has the wrong method signature. It must look like [ public void WrongSetter(TestRecord record, String value) ]");
        }

        /// <summary>
        /// Defines the <see cref="WrongTypeParameters2" />
        /// </summary>
        public class WrongTypeParameters2 : MyBaseMapper
        {
            /// <summary>
            /// The WrongSetter
            /// </summary>
            /// <param name="record">The record<see cref="TestRecord"/></param>
            /// <param name="value">The value<see cref="double"/></param>
            [YauaaField("DeviceClass")]
            public void WrongSetter(TestRecord record, double value)
            {
                throw new Xunit.Sdk.XunitException("May NEVER call this method");
            }
        }

        /// <summary>
        /// The TestWrongTypeParameters2
        /// </summary>
        [Fact]
        public void TestWrongTypeParameters2()
        {
            Action a = new Action(() => new WrongTypeParameters2());
            a.Should().Throw<InvalidParserConfigurationException>().WithMessage("In class [WrongTypeParameters2] the method [WrongSetter] has been annotated with YauaaField but it has the wrong method signature. It must look like [ public void WrongSetter(TestRecord record, String value) ]");
        }

        /// <summary>
        /// Defines the <see cref="MissingAnnotations" />
        /// </summary>
        public class MissingAnnotations : MyBaseMapper
        {
            /// <summary>
            /// The SetWasNotAnnotated
            /// </summary>
            /// <param name="record">The record<see cref="TestRecord"/></param>
            /// <param name="value">The value<see cref="double"/></param>
            public void SetWasNotAnnotated(TestRecord record, double value)
            {
                throw new XunitException("May NEVER call this method");
            }
        }

        /// <summary>
        /// The TestMissingAnnotations
        /// </summary>
        [Fact]
        public void TestMissingAnnotations()
        {
            Action a = new Action(() => new MissingAnnotations());
            a.Should().Throw<InvalidParserConfigurationException>().WithMessage("You MUST specify at least 1 field to extract.");
        }

        /// <summary>
        /// The TestBadGeneric
        /// </summary>
        [Fact]
        public void TestBadGeneric()
        {
            UserAgentAnnotationAnalyzer<object> userAgentAnalyzer = new UserAgentAnnotationAnalyzer<object>();
            Action a = new Action(() => userAgentAnalyzer.Map("Foo"));
            a.Should().Throw<InvalidParserConfigurationException>().WithMessage("[Map] The mapper instance is null.");
        }

        public class WrongReturnType: MyBaseMapper
        {
            [YauaaField("DeviceClass")]
            public bool NonVoidSetter(TestRecord record, string value)
            {
                throw new XunitException("May NEVER call this method");
            }
        }


        [Fact]
        public void TestNonVoidSetter()
        {
            Action a = new Action(() => new WrongReturnType());
            a.Should().Throw<InvalidParserConfigurationException>().WithMessage("In class [WrongReturnType] the method [NonVoidSetter] has been annotated with YauaaField but it has the wrong method signature. It must look like [ public void NonVoidSetter(TestRecord record, String value) ]");
        }

        private sealed class PrivateTestRecord
        {
            internal readonly string useragent;
            internal readonly string deviceClass;
            internal readonly string agentNameVersion;

            private PrivateTestRecord(string useragent)
            {
                this.useragent = useragent;
            }
        }

        private class PrivateMyBaseMapper: IUserAgentAnnotationMapper<PrivateTestRecord>
        {
            private readonly UserAgentAnnotationAnalyzer<PrivateTestRecord> userAgentAnalyzer;

            public PrivateMyBaseMapper()
            {
                userAgentAnalyzer = new UserAgentAnnotationAnalyzer<PrivateTestRecord>();
                userAgentAnalyzer.Initialize(this);
            }

            public PrivateTestRecord Enrich(PrivateTestRecord record)
            {
                return userAgentAnalyzer.Map(record);
            }
       
            public string GetUserAgentString(PrivateTestRecord record)
            {
                return record.useragent;
            }
        }

        private class InaccessibleSetterMapperClass: PrivateMyBaseMapper
        {
            [YauaaField("DeviceClass")]
            public void CorrectSetter(PrivateTestRecord record, string value)
            {
                throw new Exception("May NEVER call this method");
            }
        }

        [Fact]
        public void TestInaccessibleSetterClass()
        {
            Action a = new Action(() => new InaccessibleSetterMapperClass());
            a.Should().Throw<InvalidParserConfigurationException>().WithMessage("The class PrivateTestRecord is not public.");
        }

}
}
