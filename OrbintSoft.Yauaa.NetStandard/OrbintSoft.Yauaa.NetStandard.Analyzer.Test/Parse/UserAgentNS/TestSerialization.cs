using log4net;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Debug;
using OrbintSoft.Yauaa.Analyzer.Test.Fixtures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Xunit;
using FluentAssertions;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS
{
    public class TestSerialization : IClassFixture<LogFixture>
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(TestPredefinedBrowsersPerField));

        public UserAgentAnalyzerTester SerializeAndDeserializeUAA(bool delay)
        {
            LOG.Info("==============================================================");
            LOG.Info("Create");
            LOG.Info("--------------------------------------------------------------");
            var uaa = new UserAgentAnalyzerTester();
            uaa.SetShowMatcherStats(false);
            if (delay)
            {
                uaa.DelayInitialization();
            }
            else
            {
                uaa.ImmediateInitialization();
            }
           
            uaa.Initialize();

            LOG.Info("--------------------------------------------------------------");
            LOG.Info("Serialize");
            byte[] bytes;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, uaa);
                bytes = memoryStream.ToArray();
            }              

            LOG.Info(string.Format("The UserAgentAnalyzer was serialized into {0} bytes", bytes.LongLength));
            LOG.Info("--------------------------------------------------------------");
            LOG.Info("Deserialize");

            using (MemoryStream memoryStream = new MemoryStream(bytes))
            {
                var formatter = new BinaryFormatter();
                object obj = formatter.Deserialize(memoryStream);
                obj.Should().BeOfType<UserAgentAnalyzerTester>();
                uaa = obj as UserAgentAnalyzerTester;
            }           
            
            LOG.Info("Done");
            LOG.Info("==============================================================");

            return uaa;
        }


        [Fact]
        public void ValidateAllPredefinedBrowsers()
        {
            UserAgentAnalyzerTester uaa = SerializeAndDeserializeUAA(false);
            LOG.Info("==============================================================");
            LOG.Info("Validating when getting all fields");
            LOG.Info("--------------------------------------------------------------");
            uaa.RunTests(false, true).Should().BeTrue();
        }
    }
}
