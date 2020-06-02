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
    /// This class is used to classify the user agent.
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
        /// Returns the device class of the user agent (Desktop, mobile, Robot, TV, ...).
        /// </summary>
        /// <param name="userAgent">The <see cref="UserAgent"/>.</param>
        /// <returns>The <see cref="DeviceClass"/>.</returns>
        public static DeviceClass GetDeviceClass(UserAgent userAgent)
        {
            switch (userAgent.GetValue(DefaultUserAgentFields.DEVICE_CLASS))
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
        /// Indicates if there is a misuse of the user agent (user agent has been anonymized, an hacker edited, a robot is trying to imitate a broser...).
        /// </summary>
        /// <param name="userAgent">The <see cref="UserAgent"/>.</param>
        /// <returns>True if deliberate misuse.</returns>
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
        /// Indicates if the user agent is a human using the device(broswer or application).
        /// </summary>
        /// <param name="userAgent">The <see cref="UserAgent"/>.</param>
        /// <returns>True If this is probably a human using the device.</returns>
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
        ///  Indicates if the device is a mobile device or a robot that want to be treated as mobile.
        /// </summary>
        /// <param name="userAgent">The <see cref="UserAgent"/>.</param>
        /// <returns>True if this is a mobile.</returns>
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
        /// Indicates if this a 'normal' consumer device that can simply be bought/downloaded and used as intended.
        /// </summary>
        /// <param name="userAgent">The <see cref="UserAgent"/>.</param>
        /// <returns>True if normal consumer device.</returns>
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
