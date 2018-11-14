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
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Classify;
using OrbintSoft.Yauaa.Analyzer.Test.Fixtures;
using Xunit;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS.Classify
{
    public class TestClassifier : IClassFixture<LogFixture>
    {
        [Fact]
        public void TestEnumCreation()
        {
            VerifyEnum("Desktop");
            VerifyEnum("Anonymized");
            VerifyEnum("Mobile");
            VerifyEnum("Tablet");
            VerifyEnum("Phone");
            VerifyEnum("Watch");
            VerifyEnum("Virtual Reality");
            VerifyEnum("eReader");
            VerifyEnum("Set-top box");
            VerifyEnum("TV");
            VerifyEnum("Game Console");
            VerifyEnum("Handheld Game Console");
            VerifyEnum("Robot");
            VerifyEnum("Robot Mobile");
            VerifyEnum("Spy");
            VerifyEnum("Hacker");
            VerifyEnum("Unknown");
        }

        private void VerifyEnum(string deviceClass)
        {
            UserAgent userAgent = new UserAgent();
            userAgent.Set(UserAgent.DEVICE_CLASS, deviceClass, 1);
            deviceClass.Should().Be(UserAgentClassifier.GetDeviceClass(userAgent).GetValue());
        }

        [Fact]
        public void ClassifierTest()
        {
            // DeviceClass,          human, mobile, normal, misuse
            VerifyDeviceClass(DeviceClass.Desktop, true, false, true, false);
            VerifyDeviceClass(DeviceClass.Anonymized, true, false, false, true);
            VerifyDeviceClass(DeviceClass.Mobile, true, true, true, false);
            VerifyDeviceClass(DeviceClass.Tablet, true, true, true, false);
            VerifyDeviceClass(DeviceClass.Phone, true, true, true, false);
            VerifyDeviceClass(DeviceClass.Watch, true, true, true, false);
            VerifyDeviceClass(DeviceClass.VirtualReality, true, true, true, false);
            VerifyDeviceClass(DeviceClass.eReader, true, true, true, false);
            VerifyDeviceClass(DeviceClass.SetTopBox, true, false, true, false);
            VerifyDeviceClass(DeviceClass.TV, true, false, true, false);
            VerifyDeviceClass(DeviceClass.GameConsole, true, false, true, false);
            VerifyDeviceClass(DeviceClass.HandheldGameConsole, true, true, true, false);
            VerifyDeviceClass(DeviceClass.Robot, false, false, false, false);
            VerifyDeviceClass(DeviceClass.RobotMobile, false, true, false, false);
            VerifyDeviceClass(DeviceClass.Spy, false, false, false, true);
            VerifyDeviceClass(DeviceClass.Hacker, false, false, false, true);
            VerifyDeviceClass(DeviceClass.Unknown, false, false, false, false);
            VerifyDeviceClass(DeviceClass.Unclassified, false, false, false, false);
        }


        private void VerifyDeviceClass(DeviceClass deviceClass, bool human, bool mobile, bool normal, bool misuse)
        {
            UserAgent userAgent = new UserAgent();

            userAgent.Set(UserAgent.DEVICE_CLASS, deviceClass.GetValue(), 1);
            UserAgentClassifier.IsHuman(userAgent).Should().Be(human, "For the DeviceClass " + deviceClass.ToString() + " the isHuman() should be correct");
            UserAgentClassifier.IsMobile(userAgent).Should().Be(mobile, "For the DeviceClass " + deviceClass.ToString() + " the isMobile() should be correct");
            UserAgentClassifier.IsNormalConsumerDevice(userAgent).Should().Be(normal, "For the DeviceClass " + deviceClass.ToString() + " the isNormalConsumerDevice() should be correct");
            UserAgentClassifier.IsDeliberateMisuse(userAgent).Should().Be(misuse, "For the DeviceClass " + deviceClass.ToString() + " the isDeliberateMisuse() should be correct");
        }
    }
}
