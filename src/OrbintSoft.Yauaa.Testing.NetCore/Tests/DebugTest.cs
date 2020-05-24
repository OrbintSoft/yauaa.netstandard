//-----------------------------------------------------------------------
// <copyright file="DebugTest.cs" company="OrbintSoft">
//    Yet Another User Agent Analyzer for .NET Standard
//    porting realized by Stefano Balzarotti, Copyright 2018-2019 (C) OrbintSoft
//
//    Original Author and License:
//
//    Yet Another UserAgent Analyzer
//    Copyright(C) 2013-2019 Niels Basjes
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
using OrbintSoft.Yauaa.Debug;
using OrbintSoft.Yauaa.Testing.Fixtures;
using Xunit;
using FluentAssertions;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

namespace OrbintSoft.Yauaa.Testing.Tests
{
    /// <summary>
    /// This class is intended for debugging purposes, no real unit testing here.
    /// </summary>
    public class DebugTest : IClassFixture<LogFixture>
    {

        readonly HashSet<string> singleFieldList = new HashSet<string>() { "DeviceBrand" };

        public UserAgentAnalyzerTester SerializeAndDeserializeUAA()
        {
            var uaa = UserAgentAnalyzerTester.NewBuilder()
                .HideMatcherLoadStats()
                .DropDefaultResources()
                .AddResources("YamlResources/UserAgents", "GoogleChrome.yaml")
                .WithFields(this.singleFieldList)
                .ImmediateInitialization()
                .Build();
            byte[] bytes;

            using (var memoryStream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, uaa);
                bytes = memoryStream.ToArray();
            }
            using (var memoryStream = new MemoryStream(bytes))
            {
                var formatter = new BinaryFormatter();
                var obj = formatter.Deserialize(memoryStream);
                obj.Should().BeOfType<UserAgentAnalyzerTester>();
                uaa = obj as UserAgentAnalyzerTester;
            }

            return uaa as UserAgentAnalyzerTester;
        }

        //[Fact]
        public void TestError()
        {
            var fieldName = "DeviceBrand";
            var userAgentAnalyzer =
                UserAgentAnalyzerTester
                    .NewBuilder()
                    .WithoutCache()
                    .WithFields(fieldName)
                    .HideMatcherLoadStats()
                    .DropDefaultResources()
                    .AddResources("YamlResources/UserAgents", "MobileBrand-rules.yaml")
                    .AddResources("YamlResources/UserAgents", "MobileBrands.yaml")
                    .Build() as UserAgentAnalyzerTester;

            userAgentAnalyzer.Should().NotBeNull();
            //userAgentAnalyzer.RunTests(false, true, singleFieldList, false, false).Should().BeTrue();
            var userAgent = userAgentAnalyzer.Parse("AndroidDownloadManager/6.0.1 (Linux; U; Android 6.0.1; A0001 Build/MMB29X)");
            var field = userAgent.Get(fieldName);
            field.GetValue().Should().Be("Oneplus");
        }

    }
}
