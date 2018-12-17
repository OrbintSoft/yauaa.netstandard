Yauaa: Yet Another User Agent Analyzer
========================================
[![Build Status](https://dev.azure.com/orbintsoft/yauaa.netstandard/_apis/build/status/yauaa.netstandard-pipeline)](https://dev.azure.com/orbintsoft/yauaa.netstandard/_build/latest?definitionId=3)
[![NuGet](https://img.shields.io/nuget/v/Orbintsoft.Yauaa.NetStandard.svg)](https://www.nuget.org/packages/Orbintsoft.Yauaa.NetStandard/)
[![License](https://img.shields.io/:license-apache-purple.svg)](https://www.apache.org/licenses/LICENSE-2.0.html)
[![Donations via PayPal](https://img.shields.io/badge/Donations-via%20Paypal-orange.svg)](https://www.paypal.me/orbintsoft) 

This is a .NET porting of Java library that tries to parse and analyze the useragent string and extract as many relevant attributes as possible.

You can see the original project at this link: https://github.com/nielsbasjes/yauaa

The library has been completely rewritten in C# from scratch to be optimized and .NET friendly 

A bit more background about this useragent parser can be found in this blog which the author Niels Basjes wrote about it: [https://techlab.bol.com/making-sense-user-agent-string/](https://partnerprogramma.bol.com/click/click?p=1&t=url&s=2171&f=TXL&url=https%3A%2F%2Ftechlab.bol.com%2Fmaking-sense-user-agent-string%2F&name=yauaa)

The Java documentation can be found here https://yauaa.basjes.nl, soon will be provided a .NET documentation specific for this project

You can download the .nuget package there: https://www.nuget.org/packages/OrbintSoft.Yauaa.NetStandard

HIGH Profile release notes:
===========================

5.5-beta.2
--------
With Google Chrome 70 the useragent string pattern has been changed on Android ( https://www.chromestatus.com/feature/4558585463832576 ) . As a consequence the detection of the DeviceBrand failed and you always get "Unknown". This has been fixed in Yauaa 5.5.

**Warning** Before 5.5-beta.2, if you use this library throught .nuget, you may experience a reference issue with .yaml definitions,
since they aren't automatically copied to output folder.

5.4-stable.1
--------
This is latest stable preview, with 5.5 I plan to remove prerelease prefix

- Target equivalent Java 5.4 Version
- Detect more Iron variations
- Major change in the Android Chrome 70 pattern --> broke DeviceBrand
- Detect Vivo brand
- Change of namespace to be more clean 
- Created a new commandline application
- Major code refactoring 
- Implemented continuous integration with Azure Devops
- Changing of strong name singin to use snk instead of pfx
- New nuget package

Example output
==============
As an example the useragent of a phone:

    Mozilla/5.0 (Linux; Android 7.0; Nexus 6 Build/NBD90Z) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.124 Mobile Safari/537.36

is converted into this set of fields:

| Field name | Value |
| --- | --- |
|  **Device**Class                      | 'Phone'                |
|  **Device**Name                       | 'Google Nexus 6'       |
|  **Device**Brand                      | 'Google'               |
|  **OperatingSystem**Class             | 'Mobile'               |
|  **OperatingSystem**Name              | 'Android'              |
|  **OperatingSystem**Version           | '7.0'                  |
|  **OperatingSystem**NameVersion       | 'Android 7.0'          |
|  **OperatingSystem**VersionBuild      | 'NBD90Z'               |
|  **LayoutEngine**Class                | 'Browser'              |
|  **LayoutEngine**Name                 | 'Blink'                |
|  **LayoutEngine**Version              | '53.0'                 |
|  **LayoutEngine**VersionMajor         | '53'                   |
|  **LayoutEngine**NameVersion          | 'Blink 53.0'           |
|  **LayoutEngine**NameVersionMajor     | 'Blink 53'             |
|  **Agent**Class                       | 'Browser'              |
|  **Agent**Name                        | 'Chrome'               |
|  **Agent**Version                     | '53.0.2785.124'        |
|  **Agent**VersionMajor                | '53'                   |
|  **Agent**NameVersion                 | 'Chrome 53.0.2785.124' |
|  **Agent**NameVersionMajor            | 'Chrome 53'            |

Try it!
=======
You can try online the Java version with your own browser here: [https://try.yauaa.basjes.nl/](https://try.yauaa.basjes.nl/).

Soon will be available  a test with .NET library

Meanwhile, don't forget to download the .nuget package to try by yourself:  https://www.nuget.org/packages/OrbintSoft.Yauaa.NetStandard

**NOTES**

1. This runs under a "Free quota" on Google AppEngine. If this quota is exceeded then it will simply become unavailable for that day.
2. After a while of inactivity the instance is terminated so the first page may take 15-30 seconds to load.
3. If you really like this then run it on your local systems. It's much faster that way.



Contribute
===
If you like this project or if has business value for you then don't hesitate to support me or the author.

To support the original project you can make a small donation to:
[![Donations via PayPal](https://img.shields.io/badge/Donations-via%20Paypal-blue.svg)](https://www.paypal.me/nielsbasjes) **Niels Basjes, Original Author**

To support this porting, don't esitate to contribute to code sending a Pull request or with reporting issues

Otherwise a small paypal donation can also be apreciated
[![Donations via PayPal](https://img.shields.io/badge/Donations-via%20Paypal-blue.svg)](https://www.paypal.me/orbintsoft) **Stefano Balzarotti, Author of porting**

Don't forget to be thankful to the original author Nicolaas Basjes, because he did most of the effort.

License
=======
    Yet Another UserAgent Analyzer for .NET Standard
  	Porting realized by Balzarotti Stefano, Copyright (C) OrbintSoft
  
  	Original Author and License:
 
	Yet Another UserAgent Analyzer
	Copyright (C) 2013-2018 Niels Basjes
 
	Licensed under the Apache License, Version 2.0 (the "License");
	you may not use this file except in compliance with the License.
	You may obtain a copy of the License at
 
	https://www.apache.org/licenses/LICENSE-2.0
 
	Unless required by applicable law or agreed to in writing, software
	distributed under the License is distributed on an "AS IS" BASIS,
	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
	See the License for the specific language governing permissions and
	limitations under the License.
 
