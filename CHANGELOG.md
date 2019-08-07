This is intended as an overview of the major changes

v5.9
===
- Target equivalent Java 5.9 Version
- Detection for Google Go, Google Docs, Google Docs Script
- New class of Device and Agent: Voice
- Detection for CAT, General Mobile, Wileyfox, ZTE, Fairphone, Gigaset, PlayStation 3, Kobo Desktop Edition
- Improved Robot detection, most of them are now "Cloud" instead of "normal" hardware/os.
- Updated the way yaml files are loaded. An analyzer without any rules will fail faster.
- An imperfect fallback is attempted when the classloader cannot find the yaml files via a wildcard (happens on Android, OSGi, ...).
- Improved detection of Ubuntu
- Detection for very old Windows Phones, Nikto, Dell devices
- Renamed DeviceClass "Spy" to "Robot Imitator" 
- More consistently add the DeviceBrand to the DeviceName
- Detect Apple iOS Universal Links updater, Netsparker, CasperJs
- Fix the AirWatch scenario

v5.8
===
- Target equivalent Java 5.8 Version
- Lookup for MS-Edge versions (which are a MESS!)
- Fixed detection Chromium running in a snap on Ubuntu.
- Fixed detection Epiphany (Gnome Web)
- Report the actual version of Edge using a lookup.
- Detection MSOffice, Falkon, QupZilla
- Improved OS detection, added BeOS/Haiku
- Detection Colibri, Midori, Arora, WebKitGTK / luakit, Kodi
- Detection of Android on Sony Bravia TV

v5.7
===
- Migrate to Antlr 5.7.2 Standard to be more compliant

v5.6
===
- Target equivalent Java 5.6 Version
- Better extraction of NetType and Language tags as used by Tencent.
- Detect the brand better for many more devices (Blackberry, Huawei, HiSense, HTC, OnePlus, Oppo, QMobile, Wiko)
- Added two new functions for prefix matching lookups.
- Rewrote DeviceBrand detection to improve speed and memory usage.

v5.5
===
- Target equivalent Java 5.5 Version
- Fixed the Chrome 70 pattern for many brands
- Detect Alibaba Apps (like DingTalk)
- Fixed nuget packing problem, yaml definitions aren't copied automatically to output folder
- Released a sample ASP.NET Core 2.2 web app to parse the user agent
- Integrated Stylecop Analyzers
- Improvements to continuous integration

v5.4
===
- Target equivalent Java 5.4 Version
- Major change in the Android Chrome 70 pattern --> broke DeviceBrand
- Detect Vivo brand
- Change of namespace to be more clean
- Created a new commandline application
- Major code refactoring
- Implemented continuous integration with Azure Devops
- Changing of strong name singin to use snk instead of pfx
- New nuget package and release process

v5.3
===
- Target equivalent Java 5.3 Version
- Replace all http with https where possible 
- Detect Iron, Quark and Otter browsers
- Handle the 'too many spaces' effect.
- Major code refactoring
- Add benchmarking project
- Add strong name signing

v5.2
===
- First porting from Java to .NET

---------------------------------------------------------------------------------------------------------
For complete changelog of main library see: https://github.com/nielsbasjes/yauaa/blob/master/CHANGELOG.md