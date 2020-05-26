//-----------------------------------------------------------------------
// <copyright file="DeviceClassExtension.cs" company="OrbintSoft">
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
    using System;

    /// <summary>
    /// Defines the <see cref="DeviceClassExtension" />.
    /// </summary>
    public static class DeviceClassExtension
    {
        /// <summary>
        /// Used to convert the <see cref="DeviceClass"/> enum into it's string representation.
        /// </summary>
        /// <param name="deviceClass">The <see cref="DeviceClass"/>.</param>
        /// <returns>The value.</returns>
        public static string GetValue(this DeviceClass deviceClass)
        {
            switch (deviceClass)
            {
                case DeviceClass.Desktop:
                    return "Desktop";
                case DeviceClass.Anonymized:
                    return "Anonymized";
                case DeviceClass.Mobile:
                    return "Mobile";
                case DeviceClass.Tablet:
                    return "Tablet";
                case DeviceClass.Phone:
                    return "Phone";
                case DeviceClass.Watch:
                    return "Watch";
                case DeviceClass.VirtualReality:
                    return "Virtual Reality";
                case DeviceClass.EReader:
                    return "eReader";
                case DeviceClass.SetTopBox:
                    return "Set-top box";
                case DeviceClass.TV:
                    return "TV";
                case DeviceClass.GameConsole:
                    return "Game Console";
                case DeviceClass.HandheldGameConsole:
                    return "Handheld Game Console";
                case DeviceClass.Robot:
                    return "Robot";
                case DeviceClass.RobotMobile:
                    return "Robot Mobile";
                case DeviceClass.RobotImitator:
                    return "Robot Imitator";
                case DeviceClass.Hacker:
                    return "Hacker";
                case DeviceClass.Unknown:
                    return "Unknown";
                case DeviceClass.Unclassified:
                    return "Unclassified";
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
