﻿//-----------------------------------------------------------------------
// <copyright file="DebugTest.cs" company="OrbintSoft">
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
using OrbintSoft.Yauaa.Debug;
using OrbintSoft.Yauaa.Testing.Fixtures;
using Xunit;
using FluentAssertions;
using log4net;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using OrbintSoft.Yauaa.Tests;
using System.Collections.Generic;

namespace OrbintSoft.Yauaa.Testing.Tests
{
    public class DebugTest : IClassFixture<LogFixture>
    {

        readonly HashSet<string> singleFieldList = new HashSet<string>() { "AgentName" };

        public UserAgentAnalyzerTester SerializeAndDeserializeUAA()
        {
            var uaa = UserAgentAnalyzerTester.NewBuilder()
                .HideMatcherLoadStats()
                .DropDefaultResources()
                .AddResources("YamlResources/UserAgents", "GoogleChrome.yaml")
                .WithFields(singleFieldList)
                .ImmediateInitialization()
                .Build();
            byte[] bytes;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, uaa);
                bytes = memoryStream.ToArray();
            }
            using (MemoryStream memoryStream = new MemoryStream(bytes))
            {
                var formatter = new BinaryFormatter();
                object obj = formatter.Deserialize(memoryStream);
                obj.Should().BeOfType<UserAgentAnalyzerTester>();
                uaa = obj as UserAgentAnalyzerTester;
            }

            return uaa as UserAgentAnalyzerTester;
        }

        //[Fact]
        //public void TestError()
        //{
        //    UserAgentAnalyzerTester uaa = UserAgentAnalyzerTester
        //        .NewBuilder()
        //        .HideMatcherLoadStats()
        //        .DelayInitialization()
        //        .DropDefaultResources()
        //        .DropTests()
        //        .AddResources("YamlResources/UserAgents", "GooglePixel.yaml")
        //        .Build() as UserAgentAnalyzerTester;
        //    uaa.SetShowMatcherStats(false);
        //    uaa.RunTests(false, true).Should().BeFalse();  // This test must return an error state
        //}

        //[Fact]
        //public void TestSerialization()
        //{
        //    UserAgentAnalyzerTester uaa = this.SerializeAndDeserializeUAA();

        //    var userAgent = uaa.Parse("Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US) AppleWebKit/525.13 (KHTML, like Gecko) Chrome/0.2.149.27 Safari/525.13");
        //    userAgent.Get("AgentName").Value.Should().Be("Chrome");
        //}
    }
}
