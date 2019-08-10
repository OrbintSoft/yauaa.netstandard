//-----------------------------------------------------------------------
// <copyright file="DeviceClass.cs" company="OrbintSoft">
// Yet Another User Agent Analyzer for .NET Standard
// porting realized by Stefano Balzarotti, Copyright 2019 (C) OrbintSoft
//
// Original Author and License:
//
// Yet Another UserAgent Analyzer
// Copyright(C) 2013-2019 Niels Basjes
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// <author>Stefano Balzarotti, Niels Basjes</author>
// <date>2018, 11, 24, 12:49</date>
// <summary></summary>
//-----------------------------------------------------------------------
using System;

namespace OrbintSoft.Yauaa.Classify
{
    /// <summary>
    /// Defines the DeviceClass.
    /// </summary>
    public enum DeviceClass
    {
        /// <summary>
        /// The device is assessed as a Desktop/Laptop class device.
        /// </summary>
        Desktop = 1,

        /// <summary>
        /// In some cases the useragent has been altered by anonimization software.
        /// </summary>
        Anonymized = 2,

        /// <summary>
        /// A device that is mobile yet we do not know if it is a eReader/Tablet/Phone or Watch.
        /// </summary>
        Mobile = 3,

        /// <summary>
        /// A mobile device with a rather large screen (common &gt; 7").
        /// </summary>
        Tablet = 4,

        /// <summary>
        /// A mobile device with a small screen (common &lt; 7").
        /// </summary>
        Phone = 5,

        /// <summary>
        /// A mobile device with a tiny screen (common &lt; 2"). Normally these are an additional screen for a phone/tablet type device.
        /// </summary>
        Watch = 6,

        /// <summary>
        /// A mobile device with a VR capabilities.
        /// </summary>
        VirtualReality = 7,

        /// <summary>
        /// Similar to a Tablet yet in most cases with an eInk screen.
        /// </summary>
        EReader = 8,

        /// <summary>
        /// A connected device that allows interacting via a TV sized screen.
        /// </summary>
        SetTopBox = 9,

        /// <summary>
        /// Similar to Set-top box yet here this is built into the TV.
        /// </summary>
        TV = 10,

        /// <summary>
        /// 'Fixed' game systems like the PlayStation and XBox.
        /// </summary>
        GameConsole = 11,

        /// <summary>
        /// 'Mobile' game systems like the 3DS.
        /// </summary>
        HandheldGameConsole = 12,

        /// <summary>
        /// Robots that visit the site.
        /// </summary>
        Robot = 13,

        /// <summary>
        /// Robots that visit the site indicating they want to be seen as a Mobile visitor.
        /// </summary>
        RobotMobile = 14,

        /// <summary>
        /// Robots that visit the site pretending they are robots like google, but they are not.
        /// </summary>
        [Obsolete("use RobotImitator")]
        Spy = 15,

        /// <summary>
        /// Robots that visit the site pretending they are robots like google, but they are not.
        /// </summary>
        RobotImitator = 15,

        /// <summary>
        /// In case scripting is detected in the useragent string, also fallback in really broken situations.
        /// </summary>
        Hacker = 16,

        /// <summary>
        /// We really don't know, these are usually useragents that look normal yet contain almost no information about the device.
        /// </summary>
        Unknown = 17,

        /// <summary>
        ///  We found a deviceclass string that we have no enum value for.
        /// </summary>
        Unclassified = 18,
    }
}
