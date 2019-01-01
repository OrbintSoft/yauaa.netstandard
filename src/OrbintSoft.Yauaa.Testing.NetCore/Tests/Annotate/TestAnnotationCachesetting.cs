//-----------------------------------------------------------------------
// <copyright file="TestAnnotationCachesetting.cs" company="OrbintSoft">
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

namespace OrbintSoft.Yauaa.Testing.Tests.Annotate
{
    using FluentAssertions;
    using OrbintSoft.Yauaa.Analyzer;
    using OrbintSoft.Yauaa.Annotate;
    using OrbintSoft.Yauaa.Testing.Fixtures;
    using System.Reflection;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="TestAnnotationCachesetting" />
    /// </summary>
    public class TestAnnotationCachesetting : IClassFixture<LogFixture>
    {
        /// <summary>
        /// The TestAnnotationCacheSetting1
        /// </summary>
        [Fact]
        public void TestAnnotationCacheSetting1()
        {
            var mapper = new MyMapper();

            var userAgentAnnotationAnalyzer = new UserAgentAnnotationAnalyzer<TestRecord>();

            // To make sure the internals behave as expected
            var userAgentAnalyzerField = userAgentAnnotationAnalyzer.GetType().GetField("userAgentAnalyzer", BindingFlags.NonPublic | BindingFlags.Instance);
            userAgentAnalyzerField.GetValue(userAgentAnnotationAnalyzer).Should().BeNull();

            // Initial value
            userAgentAnnotationAnalyzer.CacheSize.Should().Be(UserAgentAnalyzer.DEFAULT_PARSE_CACHE_SIZE);

            // Setting and getting while no UserAgentAnalyzer exists
            userAgentAnnotationAnalyzer.SetCacheSize(1234);
            userAgentAnnotationAnalyzer.CacheSize.Should().Be(1234);

            userAgentAnnotationAnalyzer.DisableCaching();
            userAgentAnnotationAnalyzer.CacheSize.Should().Be(0);

            userAgentAnnotationAnalyzer.SetCacheSize(4567);
            userAgentAnnotationAnalyzer.CacheSize.Should().Be(4567);

            userAgentAnnotationAnalyzer.Initialize(new MyMapper());

            var userAgentAnalyzer = (UserAgentAnalyzer)userAgentAnalyzerField.GetValue(userAgentAnnotationAnalyzer);
            userAgentAnalyzer.Should().NotBeNull();

            // Setting and getting while there IS a UserAgentAnalyzer
            userAgentAnnotationAnalyzer.SetCacheSize(1234);
            userAgentAnnotationAnalyzer.CacheSize.Should().Be(1234);
            userAgentAnalyzer.CacheSize.Should().Be(1234);

            userAgentAnnotationAnalyzer.DisableCaching();
            userAgentAnnotationAnalyzer.CacheSize.Should().Be(0);
            userAgentAnalyzer.CacheSize.Should().Be(0);

            userAgentAnnotationAnalyzer.SetCacheSize(4567);
            userAgentAnnotationAnalyzer.CacheSize.Should().Be(4567);
            userAgentAnalyzer.CacheSize.Should().Be(4567);
        }

        /// <summary>
        /// The TestAnnotationCacheSetting2
        /// </summary>
        [Fact]
        public void TestAnnotationCacheSetting2()
        {
            var mapper = new MyMapper();

            var userAgentAnnotationAnalyzer = new UserAgentAnnotationAnalyzer<TestRecord>();

            // To make sure the internals behave as expected
            var userAgentAnalyzerField = userAgentAnnotationAnalyzer.GetType().GetField("userAgentAnalyzer", BindingFlags.NonPublic | BindingFlags.Instance);
            userAgentAnalyzerField.GetValue(userAgentAnnotationAnalyzer).Should().BeNull();

            // Initial value
            userAgentAnnotationAnalyzer.CacheSize.Should().Be(UserAgentAnalyzer.DEFAULT_PARSE_CACHE_SIZE);

            userAgentAnnotationAnalyzer.Initialize(new MyMapper());

            var userAgentAnalyzer = (UserAgentAnalyzer)userAgentAnalyzerField.GetValue(userAgentAnnotationAnalyzer);
            userAgentAnalyzer.Should().NotBeNull();
            // Initial value
            userAgentAnnotationAnalyzer.CacheSize.Should().Be(UserAgentAnalyzer.DEFAULT_PARSE_CACHE_SIZE);
            userAgentAnalyzer.CacheSize.Should().Be(UserAgentAnalyzer.DEFAULT_PARSE_CACHE_SIZE);

            // Setting and getting while there IS a UserAgentAnalyzer
            userAgentAnnotationAnalyzer.SetCacheSize(1234);
            userAgentAnnotationAnalyzer.CacheSize.Should().Be(1234);
            userAgentAnalyzer.CacheSize.Should().Be(1234);

            userAgentAnnotationAnalyzer.DisableCaching();
            userAgentAnnotationAnalyzer.CacheSize.Should().Be(0);
            userAgentAnalyzer.CacheSize.Should().Be(0);

            userAgentAnnotationAnalyzer.SetCacheSize(4567);
            userAgentAnnotationAnalyzer.CacheSize.Should().Be(4567);
            userAgentAnalyzer.CacheSize.Should().Be(4567);
        }

        /// <summary>
        /// Defines the <see cref="MyMapper" />
        /// </summary>
        public class MyMapper : IUserAgentAnnotationMapper<TestRecord>
        {
            /// <summary>
            /// The GetUserAgentString
            /// </summary>
            /// <param name="record">The record<see cref="TestRecord"/></param>
            /// <returns>The <see cref="string"/></returns>
            public string GetUserAgentString(TestRecord record)
            {
                return record.useragent;
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
        }

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
            /// Defines the agentNameVersion
            /// </summary>
            internal string agentNameVersion;

            /// <summary>
            /// Defines the deviceClass
            /// </summary>
            internal string deviceClass;

            /// <summary>
            /// Initializes a new instance of the <see cref="TestRecord"/> class.
            /// </summary>
            /// <param name="useragent">The useragent<see cref="string"/></param>
            public TestRecord(string useragent)
            {
                this.useragent = useragent;
            }
        }
    }
}
