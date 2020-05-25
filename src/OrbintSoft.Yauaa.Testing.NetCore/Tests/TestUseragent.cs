//-----------------------------------------------------------------------
// <copyright file="TestUseragent.cs" company="OrbintSoft">
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
using FluentAssertions;
using log4net;
using OrbintSoft.Yauaa.Analyzer;
using OrbintSoft.Yauaa.Testing.Fixtures;
using System.Collections.Generic;
using Xunit;

namespace OrbintSoft.Yauaa.Testing.Tests
{
    public class TestUseragent: IClassFixture<LogFixture>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TestUseragent));

        [Fact]
        public void Useragent()
        {
            string uaString = "Foo Bar";
            UserAgent agent = new UserAgent(uaString);
            agent.Get(DefaultUserAgentFields.USERAGENT_FIELDNAME).GetValue().Should().Be(uaString);
            agent.Get(DefaultUserAgentFields.USERAGENT_FIELDNAME).GetConfidence().Should().Be(0);
            agent.GetValue(DefaultUserAgentFields.USERAGENT_FIELDNAME).Should().Be(uaString);
            agent.GetConfidence(DefaultUserAgentFields.USERAGENT_FIELDNAME).Should().Be(0L);
        }

        [Fact]
        public void TestUseragentValues()
        {
            TestUseragentValuesDebug(true);
            TestUseragentValuesDebug(false);
        }

        private void TestUseragentValuesDebug(bool debug)
        {
            string name = "Attribute";
            string uaString = "Foo Bar";
            UserAgent agent = new UserAgent(uaString)
            {
                IsDebug = debug
            };

            // Setting unknown new attributes
            agent.Get("UnknownOne").Should().BeNull();
            agent.GetValue("UnknownOne").Should().Be("Unknown");
            agent.GetConfidence("UnknownOne").Should().Be(-1);
            agent.Set("UnknownOne", "One", 111);
            Check(agent, "UnknownOne", "One", 111);
            agent.Get("UnknownOne").Reset();
            Check(agent, "UnknownOne", null, -1);

            // Setting unknown new attributes FORCED
            agent.Get("UnknownTwo").Should().BeNull();
            agent.GetValue("UnknownTwo").Should().Be("Unknown");
            agent.GetConfidence("UnknownTwo").Should().Be(-1);
            agent.SetForced("UnknownTwo", "Two", 222);
            Check(agent, "UnknownTwo", "Two", 222);
            agent.Get("UnknownTwo").Reset();
            Check(agent, "UnknownTwo", null, -1);

            // Setting known attributes
            Check(agent, "AgentClass", "Unknown", -1);
            agent.Set("AgentClass", "One", 111);
            Check(agent, "AgentClass", "One", 111);
            agent.Get("AgentClass").Reset();
            Check(agent, "AgentClass", "Unknown", -1);

            // Setting known attributes FORCED
            Check(agent, "AgentVersion", "??", -1);
            agent.SetForced("AgentVersion", "Two", 222);
            Check(agent, "AgentVersion", "Two", 222);
            agent.Get("AgentVersion").Reset();
            Check(agent, "AgentVersion", "??", -1);

            agent.Set(name, "One", 111);
            Check(agent, name, "One", 111);

            agent.Set(name, "Two", 22); // Should be ignored
            Check(agent, name, "One", 111);

            agent.Set(name, "Three", 333); // Should be used
            Check(agent, name, "Three", 333);

            agent.SetForced(name, "Four", 4); // Should be used
            Check(agent, name, "Four", 4);

            agent.Set(name, "<<<null>>>", 2); // Should be ignored
            Check(agent, name, "Four", 4);

            agent.Set(name, "<<<null>>>", 5); // Should be used
            Check(agent, name, null, -1); // -1 --> SPECIAL CASE!!!

            agent.Set(name, "Four", 4); // Should be IGNORED (special case remember)
            Check(agent, name, null, -1); // -1 --> SPECIAL CASE!!!

            // Set a 'normal' value again.
            agent.Set(name, "Three", 333); // Should be used
            Check(agent, name, "Three", 333);

            var field = agent.Get(name);
            field.SetValueForced("Five", 5); // Should be used
            Check(agent, name, "Five", 5);
            field.SetValueForced("<<<null>>>", 4); // Should be used
            Check(agent, name, null, -1); // -1 --> SPECIAL CASE!!!
        }

        private void Check(UserAgent agent, string name, string expectedValue, long expectedConfidence)
        {
            agent.Get(name).GetValue().Should().Be(expectedValue);
            agent.GetValue(name).Should().Be(expectedValue);
            agent.Get(name).GetConfidence().Should().Be(expectedConfidence);
            agent.GetConfidence(name).Should().Be(expectedConfidence);
        }

        [Fact]
        public void TestCopying()
        {
            var origNull = new AgentField(null);
            origNull.SetValue("One", 1);
            var copyNull = new AgentField("Foo"); // Different default!
            copyNull.SetValue(origNull);

            copyNull.GetValue().Should().Be("One");
            copyNull.GetConfidence().Should().Be(1);
            copyNull.Reset();
            copyNull.GetValue().Should().Be("Foo"); // The default should NOT be modified
            copyNull.GetConfidence().Should().Be(-1);


            var origFoo = new AgentField("Foo");
            origFoo.SetValue("Two", 2);
            var copyFoo = new AgentField(null); // Different default!
            copyFoo.SetValue(origFoo);

            copyFoo.GetValue().Should().Be("Two");
            copyFoo.GetConfidence().Should().Be(2);
            copyFoo.Reset();
            copyFoo.GetValue().Should().BeNull(); // The default should NOT be modified
            copyFoo.GetConfidence().Should().Be(-1);
        }

        [Fact]
        public void ComparingUserAgents()
        {
            UserAgent baseAgent = new UserAgent("Something 2");
            UserAgent agent0 = new UserAgent("Something 2");
            UserAgent agent1 = new UserAgent("Something 1");
            UserAgent agent2 = new UserAgent("Something 2");
            UserAgent agent3 = new UserAgent("Something 2");
            UserAgent agent4 = new UserAgent("Something 2");

            var field0 = new AgentField("Foo");
            field0.SetValue("One", 1);

            var field1 = new AgentField("Foo");
            field1.SetValue("One", 1);

            var field2 = new AgentField("Foo"); // Same, different value
            field2.SetValue("Two", 1);

            var field3 = new AgentField("Foo"); // Same, different confidence
            field3.SetValue("One", 2);

            var field4 = new AgentField(null); // Same, different default
            field4.SetValue("One", 1);

            // We compare the base agent with 4 variations
            baseAgent.SetImmediateForTesting("Field", field0);
            agent0.SetImmediateForTesting("Field", field1); // Same
            agent1.SetImmediateForTesting("Field", field1); // Different useragent
            agent2.SetImmediateForTesting("Field", field2); // Different field value
            agent3.SetImmediateForTesting("Field", field3); // Different field confidence
            agent4.SetImmediateForTesting("Field", field4); // Different field default value

            // Check em
            baseAgent.Should().BeEquivalentTo(baseAgent);
            baseAgent.Should().BeEquivalentTo(agent0);
            agent0.Should().BeEquivalentTo(baseAgent);
            baseAgent.GetHashCode().Should().Be(agent0.GetHashCode());

            Log.Info(baseAgent.ToString("Field"));


            baseAgent.Equals(agent2).Should().BeFalse();
            baseAgent.Equals(agent3).Should().BeFalse();
            baseAgent.Equals(agent4).Should().BeFalse();

            agent1.Equals("String").Should().BeFalse();
            "String".Equals(agent1).Should().BeFalse();
        }

        [Fact]
        public void FullToString()
        {
            UserAgent userAgent = new UserAgent("Some Agent");
            var expected =
                "  - user_agent_string: '\"Some Agent\"'\n" +
                "    DeviceClass                      : 'Unknown'\n" +
                "    DeviceName                       : 'Unknown'\n" +
                "    DeviceBrand                      : 'Unknown'\n" +
                "    OperatingSystemClass             : 'Unknown'\n" +
                "    OperatingSystemName              : 'Unknown'\n" +
                "    OperatingSystemVersion           : '??'\n" +
                "    OperatingSystemVersionMajor      : '??'\n" +
                "    OperatingSystemNameVersion       : 'Unknown ??'\n" +
                "    OperatingSystemNameVersionMajor  : 'Unknown ??'\n" +
                "    LayoutEngineClass                : 'Unknown'\n" +
                "    LayoutEngineName                 : 'Unknown'\n" +
                "    LayoutEngineVersion              : '??'\n" +
                "    LayoutEngineVersionMajor         : '??'\n" +
                "    LayoutEngineNameVersion          : 'Unknown ??'\n" +
                "    LayoutEngineNameVersionMajor     : 'Unknown ??'\n" +
                "    AgentClass                       : 'Unknown'\n" +
                "    AgentName                        : 'Unknown'\n" +
                "    AgentVersion                     : '??'\n" +
                "    AgentVersionMajor                : '??'\n" +
                "    AgentNameVersion                 : 'Unknown ??'\n" +
                "    AgentNameVersionMajor            : 'Unknown ??'\n";
            userAgent.ToString().Should().Be(expected);
        }

        [Fact]
        public void LimitedToString()
        {
            List<string> wanted = new List<string> { "DeviceClass", "AgentVersion", "SomethingElse" };

            // When only asking for a limited set of fields then the internal datastructures are
            // initialized with only the known attributes for which we have 'non standard' default values.
            UserAgent userAgent = new UserAgent("Some Agent", wanted);

            var expected =
                "  - user_agent_string: '\"Some Agent\"'\n" +
                "    DeviceClass   : 'Unknown'\n" +
                "    AgentVersion  : '??'\n";
           
            var result = userAgent.ToString();
            result.Should().Be(expected);

            userAgent.GetValue("DeviceClass").Should().Be("Unknown");
            userAgent.GetValue("AgentVersion").Should().Be("??");
            userAgent.GetValue("SomethingElse").Should().Be("Unknown");
        }
    }
}
