//-----------------------------------------------------------------------
// <copyright file="UserAgentClassifier.cs" company="OrbintSoft">
//   Yet Another User Agent Analyzer for .NET Standard
//   porting realized by Stefano Balzarotti, Copyright 2018-2020 (C) OrbintSoft
//
//   Original Author and License:
//
//   Yet Another UserAgent Analyzer
//   Copyright(C) 2013-2020 Niels Basjes
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//   https://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
// <author>Stefano Balzarotti, Niels Basjes</author>
// <date>2018, 11, 24, 12:49</date>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Classify
{
    using OrbintSoft.Yauaa.Analyzer;

    /// <summary>
    /// Defines the <see cref="UserAgentClassifier" />.
    /// </summary>
    public class UserAgentClassifier
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="UserAgentClassifier"/> class from being created.
        /// </summary>
        private UserAgentClassifier()
        {
        }

        /// <summary>
        /// The GetDeviceClass.
        /// </summary>
        /// <param name="userAgent">The userAgent<see cref="UserAgent"/>.</param>
        /// <returns>The <see cref="DeviceClass"/>.</returns>
        public static DeviceClass GetDeviceClass(UserAgent userAgent)
        {
            switch (userAgent.GetValue(UserAgent.DEVICE_CLASS))
            {
                case "Desktop": return DeviceClass.Desktop;
                case "Anonymized": return DeviceClass.Anonymized;
                case "Mobile": return DeviceClass.Mobile;
                case "Tablet": return DeviceClass.Tablet;
                case "Phone": return DeviceClass.Phone;
                case "Watch": return DeviceClass.Watch;
                case "Virtual Reality": return DeviceClass.VirtualReality;
                case "eReader": return DeviceClass.EReader;
                case "Set-top box": return DeviceClass.SetTopBox;
                case "TV": return DeviceClass.TV;
                case "Game Console": return DeviceClass.GameConsole;
                case "Handheld Game Console": return DeviceClass.HandheldGameConsole;
                case "Robot": return DeviceClass.Robot;
                case "Robot Mobile": return DeviceClass.RobotMobile;
                case "Spy":
                case "Robot Imitator": return DeviceClass.RobotImitator;
                case "Hacker": return DeviceClass.Hacker;
                case "Unknown": return DeviceClass.Unknown;
                default: return DeviceClass.Unclassified;
            }
        }

        /// <summary>
        /// The IsDeliberateMisuse.
        /// </summary>
        /// <param name="userAgent">The instance that needs to be classified.</param>
        /// <returns>Do we see this as deliberate misuse?.</returns>
        public static bool IsDeliberateMisuse(UserAgent userAgent)
        {
            switch (GetDeviceClass(userAgent))
            {
                case DeviceClass.Anonymized:
                case DeviceClass.RobotImitator:
                case DeviceClass.Hacker:
                    return true;

                case DeviceClass.Desktop:
                case DeviceClass.Mobile:
                case DeviceClass.Tablet:
                case DeviceClass.Phone:
                case DeviceClass.Watch:
                case DeviceClass.VirtualReality:
                case DeviceClass.EReader:
                case DeviceClass.SetTopBox:
                case DeviceClass.TV:
                case DeviceClass.GameConsole:
                case DeviceClass.HandheldGameConsole:
                case DeviceClass.Robot:
                case DeviceClass.RobotMobile:
                case DeviceClass.Unknown:
                case DeviceClass.Unclassified:
                default:
                    return false;
            }
        }

        /// <summary>
        /// The IsHuman.
        /// </summary>
        /// <param name="userAgent">The instance that needs to be classified.</param>
        /// <returns>If this is probably a human using the device.</returns>
        public static bool IsHuman(UserAgent userAgent)
        {
            switch (GetDeviceClass(userAgent))
            {
                case DeviceClass.Desktop:
                case DeviceClass.Mobile:
                case DeviceClass.Tablet:
                case DeviceClass.Phone:
                case DeviceClass.Watch:
                case DeviceClass.VirtualReality:
                case DeviceClass.EReader:
                case DeviceClass.SetTopBox:
                case DeviceClass.TV:
                case DeviceClass.GameConsole:
                case DeviceClass.HandheldGameConsole:
                case DeviceClass.Anonymized:
                    return true;

                case DeviceClass.Robot:
                case DeviceClass.RobotMobile:
                case DeviceClass.RobotImitator:
                case DeviceClass.Hacker:
                case DeviceClass.Unknown:
                case DeviceClass.Unclassified:
                default:
                    return false;
            }
        }

        /// <summary>
        /// The IsMobile.
        /// </summary>
        /// <param name="userAgent">The instance that needs to be classified.</param>
        /// <returns>Is this a 'mobile' device. (includes robots that want to be treated as mobile).</returns>
        public static bool IsMobile(UserAgent userAgent)
        {
            switch (GetDeviceClass(userAgent))
            {
                case DeviceClass.Mobile:
                case DeviceClass.Tablet:
                case DeviceClass.Phone:
                case DeviceClass.Watch:
                case DeviceClass.VirtualReality:
                case DeviceClass.EReader:
                case DeviceClass.HandheldGameConsole:
                case DeviceClass.RobotMobile:
                    return true;

                case DeviceClass.Desktop:
                case DeviceClass.SetTopBox:
                case DeviceClass.TV:
                case DeviceClass.GameConsole:
                case DeviceClass.Anonymized:
                case DeviceClass.Robot:
                case DeviceClass.RobotImitator:
                case DeviceClass.Hacker:
                case DeviceClass.Unknown:
                case DeviceClass.Unclassified:
                default:
                    return false;
            }
        }

        /// <summary>
        /// The IsNormalConsumerDevice.
        /// </summary>
        /// <param name="userAgent">The instance that needs to be classified.</param>
        /// <returns>Is this a 'normal' consumer device that can simply be bought/downloaded and used as intended.</returns>
        public static bool IsNormalConsumerDevice(UserAgent userAgent)
        {
            switch (GetDeviceClass(userAgent))
            {
                case DeviceClass.Desktop:
                case DeviceClass.Mobile:
                case DeviceClass.Tablet:
                case DeviceClass.Phone:
                case DeviceClass.Watch:
                case DeviceClass.VirtualReality:
                case DeviceClass.EReader:
                case DeviceClass.SetTopBox:
                case DeviceClass.TV:
                case DeviceClass.GameConsole:
                case DeviceClass.HandheldGameConsole:
                    return true;

                case DeviceClass.Anonymized:
                case DeviceClass.Robot:
                case DeviceClass.RobotMobile:
                case DeviceClass.RobotImitator:
                case DeviceClass.Hacker:
                case DeviceClass.Unknown:
                case DeviceClass.Unclassified:
                default:
                    return false;
            }
        }
    }
}
