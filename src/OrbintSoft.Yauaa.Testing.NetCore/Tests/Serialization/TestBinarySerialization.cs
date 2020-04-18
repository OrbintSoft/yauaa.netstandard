//-----------------------------------------------------------------------
// <copyright file="TestSerialization.cs" company="OrbintSoft">
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
//-----------------------------------------------------------------------
namespace OrbintSoft.Yauaa.Testing.Tests
{
    using FluentAssertions;
    using log4net;
    using OrbintSoft.Yauaa.Debug;
    using OrbintSoft.Yauaa.Testing.Fixtures;
    using OrbintSoft.Yauaa.Tests;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="TestBinarySerialization" />
    /// </summary>
    public class TestBinarySerialization : IClassFixture<LogFixture>
    {
        /// <summary>
        /// Defines the LOG
        /// </summary>
        private static readonly ILog LOG = LogManager.GetLogger(typeof(TestPredefinedBrowsersPerField));

        /// <summary>
        /// The SerializeAndDeserializeUAA
        /// </summary>
        /// <param name="delay">The delay<see cref="bool"/></param>
        /// <returns>The <see cref="UserAgentAnalyzerTester"/></returns>
        public UserAgentAnalyzerTester SerializeAndDeserializeUAA(bool delay)
        {
            LOG.Info("==============================================================");
            LOG.Info("Create");
            LOG.Info("--------------------------------------------------------------");
            var uaab = UserAgentAnalyzerTester
                .NewBuilder()
                .KeepTests()
                .DropDefaultResources()
                .AddResources(Config.RESOURCES_PATH, "AllSteps.yaml")
                .AddResources(Config.RESOURCES_PATH, "AllFields-tests.yaml")
                .AddResources(Config.RESOURCES_PATH, "AllPossibleSteps.yaml")
                .AddResources(Config.RESOURCES_PATH, "DocumentationExample.yaml")
                .HideMatcherLoadStats();

            if (delay)
            {
                uaab.DelayInitialization();
            }
            else
            {
                uaab.ImmediateInitialization();
            }

            var uaa = uaab.Build() as UserAgentAnalyzerTester;
            LOG.Info("--------------------------------------------------------------");
            LOG.Info("Serialize");
            byte[] bytes;

            using (var memoryStream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, uaa);
                bytes = memoryStream.ToArray();
            }

            LOG.Info(string.Format("The UserAgentAnalyzer was serialized into {0} bytes", bytes.LongLength));
            LOG.Info("--------------------------------------------------------------");
            LOG.Info("Deserialize");

            using (var memoryStream = new MemoryStream(bytes))
            {
                var formatter = new BinaryFormatter();
                var obj = formatter.Deserialize(memoryStream);
                obj.Should().BeOfType<UserAgentAnalyzerTester>();
                uaa = obj as UserAgentAnalyzerTester;
            }

            LOG.Info("Done");
            LOG.Info("==============================================================");

            return uaa;
        }

        /// <summary>
        /// The ValidateAllPredefinedBrowsers
        /// </summary>
        /// <param name="delay">The delay<see cref="bool"/></param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ValidateAllPredefinedBrowsers(bool delay)
        {
            var uaa = this.SerializeAndDeserializeUAA(delay);
            LOG.Info("==============================================================");
            LOG.Info("Validating when getting all fields");
            LOG.Info("--------------------------------------------------------------");
            uaa.RunTests(false, false, null, false, false).Should().BeTrue();
        }
    }
}
