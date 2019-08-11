//-----------------------------------------------------------------------
// <copyright file="TestClassifier.cs" company="OrbintSoft">
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
namespace OrbintSoft.Yauaa.Testing.Tests.Classify
{
    using FluentAssertions;
    using OrbintSoft.Yauaa.Analyzer;
    using OrbintSoft.Yauaa.Classify;
    using OrbintSoft.Yauaa.Testing.Fixtures;
    using Xunit;

    /// <summary>
    /// This class is used to test if devices are classified in the right way.
    /// </summary>
    public class TestClassifier : IClassFixture<LogFixture>
    {
        /// <summary>
        /// This methos test if the enum used for device clasaification is created in the right way a correctly traslated in the corrisponding string representation.
        /// </summary>
        [Fact]
        public void TestEnumCreation()
        {
            this.VerifyEnum("Desktop");
            this.VerifyEnum("Anonymized");
            this.VerifyEnum("Mobile");
            this.VerifyEnum("Tablet");
            this.VerifyEnum("Phone");
            this.VerifyEnum("Watch");
            this.VerifyEnum("Virtual Reality");
            this.VerifyEnum("eReader");
            this.VerifyEnum("Set-top box");
            this.VerifyEnum("TV");
            this.VerifyEnum("Game Console");
            this.VerifyEnum("Handheld Game Console");
            this.VerifyEnum("Robot");
            this.VerifyEnum("Robot Mobile");
            this.VerifyEnum("Robot Imitator");
            this.VerifyEnum("Hacker");
            this.VerifyEnum("Unknown");
        }

        /// <summary>
        /// This is a methis helper to verify the emum creation.
        /// </summary>
        /// <param name="deviceClass">The deviceClass enum as string.</param>
        private void VerifyEnum(string deviceClass)
        {
            var userAgent = new UserAgent();
            userAgent.Set(UserAgent.DEVICE_CLASS, deviceClass, 1);
            deviceClass.Should().Be(UserAgentClassifier.GetDeviceClass(userAgent).GetValue());
        }

        /// <summary>
        /// This method cgecks if the device class is classified correctly.
        /// </summary>
        [Fact]
        public void ClassifierTest()
        {
            //                                 DeviceClass,         human, mobile, normal, misuse
            this.VerifyDeviceClass(DeviceClass.Desktop,             true,  false,  true,   false);
            this.VerifyDeviceClass(DeviceClass.Anonymized,          true,  false,  false,  true);
            this.VerifyDeviceClass(DeviceClass.Mobile,              true,  true,   true,   false);
            this.VerifyDeviceClass(DeviceClass.Tablet,              true,  true,   true,   false);
            this.VerifyDeviceClass(DeviceClass.Phone,               true,  true,   true,   false);
            this.VerifyDeviceClass(DeviceClass.Watch,               true,  true,   true,   false);
            this.VerifyDeviceClass(DeviceClass.VirtualReality,      true,  true,   true,   false);
            this.VerifyDeviceClass(DeviceClass.EReader,             true,  true,   true,   false);
            this.VerifyDeviceClass(DeviceClass.SetTopBox,           true,  false,  true,   false);
            this.VerifyDeviceClass(DeviceClass.TV,                  true,  false,  true,   false);
            this.VerifyDeviceClass(DeviceClass.GameConsole,         true,  false,  true,   false);
            this.VerifyDeviceClass(DeviceClass.HandheldGameConsole, true,  true,   true,   false);
            this.VerifyDeviceClass(DeviceClass.Robot,               false, false,  false,  false);
            this.VerifyDeviceClass(DeviceClass.RobotMobile,         false, true,   false,  false);
            this.VerifyDeviceClass(DeviceClass.RobotImitator,       false, false,  false,  true);
            this.VerifyDeviceClass(DeviceClass.Hacker,              false, false,  false,  true);
            this.VerifyDeviceClass(DeviceClass.Unknown,             false, false,  false,  false);
            this.VerifyDeviceClass(DeviceClass.Unclassified,        false, false,  false,  false);
        }

        /// <summary>
        /// This is an helper method to verify device class classification.
        /// </summary>
        /// <param name="deviceClass">The deviceClass<see cref="DeviceClass"/></param>
        /// <param name="human">If the device is used by a human (like  a browser)</param>
        /// <param name="mobile">If the device is mobile</param>
        /// <param name="normal">If its a normal consumer device.</param>
        /// <param name="misuse">If this user agent is a misuse like an hacker.</param>
        private void VerifyDeviceClass(DeviceClass deviceClass, bool human, bool mobile, bool normal, bool misuse)
        {
            var userAgent = new UserAgent();

            userAgent.Set(UserAgent.DEVICE_CLASS, deviceClass.GetValue(), 1);
            UserAgentClassifier.IsHuman(userAgent).Should().Be(human, "For the DeviceClass " + deviceClass.ToString() + " the isHuman() should be correct");
            UserAgentClassifier.IsMobile(userAgent).Should().Be(mobile, "For the DeviceClass " + deviceClass.ToString() + " the isMobile() should be correct");
            UserAgentClassifier.IsNormalConsumerDevice(userAgent).Should().Be(normal, "For the DeviceClass " + deviceClass.ToString() + " the isNormalConsumerDevice() should be correct");
            UserAgentClassifier.IsDeliberateMisuse(userAgent).Should().Be(misuse, "For the DeviceClass " + deviceClass.ToString() + " the isDeliberateMisuse() should be correct");
        }
    }
}
