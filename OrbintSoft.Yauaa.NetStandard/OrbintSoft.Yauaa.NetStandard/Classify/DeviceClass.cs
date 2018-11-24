//-----------------------------------------------------------------------
// <copyright file="DeviceClass.cs" company="OrbintSoft">
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
// <date>2018, 11, 24, 12:49</date>
// <summary></summary>
//-----------------------------------------------------------------------
namespace OrbintSoft.Yauaa.Classify
{
    using System;

    /// <summary>
    /// Defines the DeviceClass
    /// </summary>
    public enum DeviceClass
    {
        /// <summary>
        /// The device is assessed as a Desktop/Laptop class device.
        /// </summary>
        Desktop,
        /// <summary>
        /// In some cases the useragent has been altered by anonimization software.
        /// </summary>
        Anonymized,
        /// <summary>
        /// A device that is mobile yet we do not know if it is a eReader/Tablet/Phone or Watch.
        /// </summary>
        Mobile,
        /// <summary>
        /// A mobile device with a rather large screen (common &gt; 7").
        /// </summary>
        Tablet,
        /// <summary>
        /// A mobile device with a small screen (common &lt; 7").
        /// </summary>
        Phone,
        /// <summary>
        /// A mobile device with a tiny screen (common &lt; 2"). Normally these are an additional screen for a phone/tablet type device.
        /// </summary>
        Watch,
        /// <summary>
        /// A mobile device with a VR capabilities.
        /// </summary>
        VirtualReality,
        /// <summary>
        /// Similar to a Tablet yet in most cases with an eInk screen.
        /// </summary>
        eReader,
        /// <summary>
        /// A connected device that allows interacting via a TV sized screen.
        /// </summary>
        SetTopBox,
        /// <summary>
        /// Similar to Set-top box yet here this is built into the TV.
        /// </summary>
        TV,
        /// <summary>
        /// 'Fixed' game systems like the PlayStation and XBox.
        /// </summary>
        GameConsole,
        /// <summary>
        /// 'Mobile' game systems like the 3DS.
        /// </summary>
        HandheldGameConsole,
        /// <summary>
        /// Robots that visit the site.
        /// </summary>
        Robot,
        /// <summary>
        /// Robots that visit the site indicating they want to be seen as a Mobile visitor.
        /// </summary>
        RobotMobile,
        /// <summary>
        /// Robots that visit the site pretending they are robots like google, but they are not.
        /// </summary>
        Spy,
        /// <summary>
        /// In case scripting is detected in the useragent string, also fallback in really broken situations.
        /// </summary>
        Hacker,
        /// <summary>
        /// We really don't know, these are usually useragents that look normal yet contain almost no information about the device.
        /// </summary>
        Unknown,
        /// <summary>
        ///  We found a deviceclass string that we have no enum value for.
        /// </summary>
        Unclassified
    }

    /// <summary>
    /// Defines the <see cref="DeviceClassExtension" />
    /// </summary>
    public static class DeviceClassExtension
    {
        /// <summary>
        /// The GetValue
        /// </summary>
        /// <param name="deviceClass">The deviceClass<see cref="DeviceClass"/></param>
        /// <returns>The <see cref="string"/></returns>
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
                case DeviceClass.eReader:
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
                case DeviceClass.Spy:
                    return "Spy";
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
