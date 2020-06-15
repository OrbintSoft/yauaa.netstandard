//-----------------------------------------------------------------------
// <copyright file="DefaultUserAgentFields.cs" company="OrbintSoft">
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
// <date>2020, 05, 15, 00:30</date>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Analyzer
{
    /// <summary>
    /// In this class you can find the default field names costants.
    /// </summary>
    public static class DefaultUserAgentFields
    {
        /// <summary>
        /// This field represents the browser/agent build number.
        /// </summary>
        public const string AGENT_BUILD = "AgentBuild";

        /// <summary>
        /// This field represents the browser/agent class (Phone, Desktop, etc...)
        /// </summary>
        public const string AGENT_CLASS = "AgentClass";

        /// <summary>
        /// This field represents an e-mail associated to the agent.
        /// </summary>
        public const string AGENT_INFORMATION_EMAIL = "AgentInformationEmail";

        /// <summary>
        /// This field represents an url associated to the agent.
        /// </summary>
        public const string AGENT_INFORMATION_URL = "AgentInformationUrl";

        /// <summary>
        /// This field represents the language/culture of the browser/agent.
        /// </summary>
        public const string AGENT_LANGUAGE = "AgentLanguage";

        /// <summary>
        /// This field represents the language/culture code of the browser/agent.
        /// </summary>
        public const string AGENT_LANGUAGE_CODE = "AgentLanguageCode";

        /// <summary>
        /// This field represents the name of the browser/agent.
        /// </summary>
        public const string AGENT_NAME = "AgentName";

        /// <summary>
        /// This field represents the name and version of the browser/agent.
        /// </summary>
        public const string AGENT_NAME_VERSION = "AgentNameVersion";

        /// <summary>
        /// This field represents the name and major version of the browser/agent.
        /// </summary>
        public const string AGENT_NAME_VERSION_MAJOR = "AgentNameVersionMajor";

        /// <summary>
        /// This field represents security informations about the browser/agent.
        /// </summary>
        public const string AGENT_SECURITY = "AgentSecurity";

        /// <summary>
        /// This field represents an uuid associated to the agent.
        /// </summary>
        public const string AGENT_UUID = "AgentUuid";

        /// <summary>
        /// This field represents the version of the browser/agent.
        /// </summary>
        public const string AGENT_VERSION = "AgentVersion";

        /// <summary>
        /// This field represents the major version of the browser/agent.
        /// </summary>
        public const string AGENT_VERSION_MAJOR = "AgentVersionMajor";

        /// <summary>
        /// This field represents informations about a user agent that has been anonymized.
        /// </summary>
        public const string ANONYMIZED = "Anonymized";

        /// <summary>
        /// This field represents a device brand name.
        /// </summary>
        public const string DEVICE_BRAND = "DeviceBrand";

        /// <summary>
        /// This field represents a device class (phone, tablet, TV,...).
        /// </summary>
        public const string DEVICE_CLASS = "DeviceClass";

        /// <summary>
        /// This field represents a device cpu (Intel, ARM,...).
        /// </summary>
        public const string DEVICE_CPU = "DeviceCpu";

        /// <summary>
        /// This fields repesents cpu bits (16, 32, 64,...)
        /// </summary>
        public const string DEVICE_CPU_BITS = "DeviceCpuBits";

        /// <summary>
        /// This fields repesents the device firmware version.
        /// </summary>
        public const string DEVICE_FIRMWARE_VERSION = "DeviceFirmwareVersion";

        /// <summary>
        /// This fields repesents the device name (Desktop, iPhone, iMac, Samsung TV ...)
        /// </summary>
        public const string DEVICE_NAME = "DeviceName";

        /// <summary>
        /// This fields repesents the device version.
        /// </summary>
        public const string DEVICE_VERSION = "DeviceVersion";

        /// <summary>
        /// This fields repesents the facebook carrier (OrangeB, TelfortNL, ...)
        /// </summary>
        public const string FACEBOOK_CARRIER = "FacebookCarrier";

        /// <summary>
        /// This fields repesents the facebook device class (same as device class, but provided by facebook).
        /// </summary>
        public const string FACEBOOK_DEVICE_CLASS = "FacebookDeviceClass";

        /// <summary>
        /// This fields repesents the facebook device name (same as device name, but provided by facebook).
        /// </summary>
        public const string FACEBOOK_DEVICE_NAME = "FacebookDeviceName";

        /// <summary>
        /// This fields repesents the facebook device version (same as device version, but provided by facebook).
        /// </summary>
        public const string FACEBOOK_DEVICE_VERSION = "FacebookDeviceVersion";

        /// <summary>
        /// Specific field of facebook user agent.
        /// </summary>
        public const string FACEBOOK_F_B_O_P = "FacebookFBOP";

        /// <summary>
        /// Specific field of facebook user agent.
        /// </summary>
        public const string FACEBOOK_F_B_S_S = "FacebookFBSS";

        /// <summary>
        /// This fields repesents the facebook operating system name (same as operating system name, but provided by facebook).
        /// </summary>
        public const string FACEBOOK_OPERATING_SYSTEM_NAME = "FacebookOperatingSystemName";

        /// <summary>
        /// This fields repesents the facebook operating system version (same as operating system version, but provided by facebook).
        /// </summary>
        public const string FACEBOOK_OPERATING_SYSTEM_VERSION = "FacebookOperatingSystemVersion";

        /// <summary>
        /// This fields repesents the kind of attack vector.
        /// </summary>
        public const string HACKER_ATTACK_VECTOR = "HackerAttackVector";

        /// <summary>
        /// This fields repesents the name of hacker toolkit.
        /// </summary>
        public const string HACKER_TOOLKIT = "HackerToolkit";

        /// <summary>
        /// This fields repesents the emulated compatibility name and version of internet explorer.
        /// </summary>
        public const string IE_COMPATIBILITY_NAME_VERSION = "IECompatibilityNameVersion";

        /// <summary>
        /// This fields repesents the emulated compatibility name and major version of internet explorer.
        /// </summary>
        public const string IE_COMPATIBILITY_NAME_VERSION_MAJOR = "IECompatibilityNameVersionMajor";

        /// <summary>
        /// This fields repesents the emulated compatibility version of internet explorer.
        /// </summary>
        public const string IE_COMPATIBILITY_VERSION = "IECompatibilityVersion";

        /// <summary>
        /// This fields repesents the emulated compatibility major version of internet explorer.
        /// </summary>
        public const string IE_COMPATIBILITY_VERSION_MAJOR = "IECompatibilityVersionMajor";

        /// <summary>
        /// This fields repesents the name of kobo affiliate.
        /// </summary>
        public const string KOBO_AFFILIATE = "KoboAffiliate";

        /// <summary>
        /// This fields repesents the kobo platform id.
        /// </summary>
        public const string KOBO_PLATFORM_ID = "KoboPlatformId";

        /// <summary>
        /// This fields repesents the layout engine build number.
        /// </summary>
        public const string LAYOUT_ENGINE_BUILD = "LayoutEngineBuild";

        /// <summary>
        /// This fields repesents the layout engine class (Browser, cloud, tool).
        /// </summary>
        public const string LAYOUT_ENGINE_CLASS = "LayoutEngineClass";

        /// <summary>
        /// This fields repesents the layout engine name (Presto, Webkit, Trident, ...).
        /// </summary>
        public const string LAYOUT_ENGINE_NAME = "LayoutEngineName";

        /// <summary>
        /// This fields repesents the layout engine name and version.
        /// </summary>
        public const string LAYOUT_ENGINE_NAME_VERSION = "LayoutEngineNameVersion";

        /// <summary>
        /// This fields repesents the layout engine version major.
        /// </summary>
        public const string LAYOUT_ENGINE_NAME_VERSION_MAJOR = "LayoutEngineNameVersionMajor";

        /// <summary>
        /// This fields repesents the layout engine version.
        /// </summary>
        public const string LAYOUT_ENGINE_VERSION = "LayoutEngineVersion";

        /// <summary>
        /// This fields repesents the layout engine version major.
        /// </summary>
        public const string LAYOUT_ENGINE_VERSION_MAJOR = "LayoutEngineVersionMajor";

        /// <summary>
        /// This fields repesents the network type (3g, wifi, lte...)
        /// </summary>
        public const string NETWORK_TYPE = "NetworkType";

        /// <summary>
        /// For internal use (null user agent).
        /// </summary>
        public const string NULL_VALUE = "<<<null>>>";

        /// <summary>
        /// This fields repesents the Operating System class (Mobile, Desktop, ...).
        /// </summary>
        public const string OPERATING_SYSTEM_CLASS = "OperatingSystemClass";

        /// <summary>
        /// This fields repesents the Operating System name.
        /// </summary>
        public const string OPERATING_SYSTEM_NAME = "OperatingSystemName";

        /// <summary>
        /// This fields repesents the Operating System name and version.
        /// </summary>
        public const string OPERATING_SYSTEM_NAME_VERSION = "OperatingSystemNameVersion";

        /// <summary>
        /// This fields repesents the Operating System name and major version.
        /// </summary>
        public const string OPERATING_SYSTEM_NAME_VERSION_MAJOR = "OperatingSystemNameVersionMajor";

        /// <summary>
        /// This fields repesents the Operating System version.
        /// </summary>
        public const string OPERATING_SYSTEM_VERSION = "OperatingSystemVersion";

        /// <summary>
        /// This fields repesents the Operating System version build.
        /// </summary>
        public const string OPERATING_SYSTEM_VERSION_BUILD = "OperatingSystemVersionBuild";

        /// <summary>
        /// This fields repesents the Operating System major version.
        /// </summary>
        public const string OPERATING_SYSTEM_VERSION_MAJOR = "OperatingSystemVersionMajor";

        /// <summary>
        /// For internal use (used to retrieve all fields).
        /// </summary>
        public const string SET_ALL_FIELDS = "__Set_ALL_Fields__";

        /// <summary>
        /// For internal use (used to retrieve the syntax error in user agent string).
        /// </summary>
        public const string SYNTAX_ERROR = "__SyntaxError__";

        /// <summary>
        /// This fields repesents an unnknow/non categorized value retrieved from the user agent.
        /// </summary>
        public const string UNKNOWN_VALUE = "Unknown";

        /// <summary>
        /// This fields repesents an unnknow/non categorized version retrieved from the user agent.
        /// </summary>
        public const string UNKNOWN_VERSION = "??";

        /// <summary>
        /// This fields repesents an unnknow/non categorized name and version retrieved from the user agent.
        /// </summary>
        public const string UNKNOWN_NAME_VERSION = "Unknown ??";

        /// <summary>
        /// Used to retrieve the full user agent.
        /// </summary>
        public const string USERAGENT_FIELDNAME = "Useragent";

        /// <summary>
        /// This fields repesents the name of the app that is running the webview.
        /// </summary>
        public const string WEBVIEW_APP_NAME = "WebviewAppName";

        /// <summary>
        /// This fields repesents the name and version of the app that is running the webview.
        /// </summary>
        public const string WEBVIEW_APP_NAME_VERSION_MAJOR = "WebviewAppNameVersionMajor";

        /// <summary>
        /// This fields repesents the version of the app that is running the webview.
        /// </summary>
        public const string WEBVIEW_APP_VERSION = "WebviewAppVersion";

        /// <summary>
        /// This fields repesents the major version of the app that is running the webview.
        /// </summary>
        public const string WEBVIEW_APP_VERSION_MAJOR = "WebviewAppVersionMajor";
    }
}
