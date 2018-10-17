using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Annotate;
using OrbintSoft.Yauaa.Analyzer.Test.Fixtures;
using System;
using System.Collections.Generic;
using System.Text;
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

            //assertEquals("Desktop", record.deviceClass);
            //assertEquals("Chrome 48.0.2564.82", record.agentNameVersion);
        }
    }
}
