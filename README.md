Yauaa: Yet Another UserAgent Analyzer
========================================
This is a .NET porting of Java library that tries to parse and analyze the useragent string and extract as many relevant attributes as possible.

You can see the original project at this link: https://github.com/nielsbasjes/yauaa

A bit more background about this useragent parser can be found in this blog which the author Niels Basjes wrote about it: [https://techlab.bol.com/making-sense-user-agent-string/](https://partnerprogramma.bol.com/click/click?p=1&t=url&s=2171&f=TXL&url=https%3A%2F%2Ftechlab.bol.com%2Fmaking-sense-user-agent-string%2F&name=yauaa)

The Java documentation can be found here https://yauaa.basjes.nl, soon will be provided a .NET documentation specific for this project

You can download the .nuget package there: https://www.nuget.org/packages/OrbintSoft.Yauaa.NetStandard.Analyzer

STATUS: 5.2-beta.2 latest stable, 5.3-alpha.1 in progress
=========

You can consider the version 5.2-beta.2 stable and equivalent to the same 5.2 Java version in functionalities, for all features that are covered by unit testings.

I can't release the version 5.2 as stable because I am planning a major code refactoring that will break the compabilities with public API.

A stable version will be relased as soon as I complete the following tasks:

* Improve performance
* Add more tests
* Make more C# like
* Clean the code
* Add documenation

Consider to contribute to the project to release soon a stable version

**Why 5.3-alpha before 5.2 stable?**

I am working on this project alone, and every day new user agents coming out, I don't want let be a too huge gab between Java and .NET version, so keep this library updated is a priority.

However you can consider the code of 5.2-beta.2 stable even if not perfect.

**5.3-alpha.1**

All code has been upgrated to target 5.3 Java version, but the library is not yet covered by unit testings, so I can't tell if stable, but all tests with all predefiniote browsers works correctly. 

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

Meanwhile, don't forget to download the .nuget package to try by yourself:  https://www.nuget.org/packages/OrbintSoft.Yauaa.NetStandard.Analyzer

**NOTES**

1. This runs under a "Free quota" on Google AppEngine. If this quota is exceeded then it will simply become unavailable for that day.
2. After a while of inactivity the instance is terminated so the first page may take 15-30 seconds to load.
3. If you really like this then run it on your local systems. It's much faster that way.



Contribute
===
If you like this project or if has business value for you then don't hesitate to support me or the author.

To support the original project you can make a small donation to:
[![Donations via PayPal](https://img.shields.io/badge/Donations-via%20Paypal-blue.svg)](https://www.paypal.me/nielsbasjes) **Niels Basjes, Original Author**

To support this porting, don't esitate to contribute to code sending a Pull request

Otherwise a small paypal donation can also be apreciated
[![Donations via PayPal](https://img.shields.io/badge/Donations-via%20Paypal-blue.svg)](https://www.paypal.me/orbintsoft) **Stefano Balzarotti, Author of porting**

Don't forget to be thankful to the original author Nicolaas Basjes, he did most of the effort.

License
=======
    Yet Another UserAgent Analyzer .NET Standard
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
  
	All rights should be reserved to the original author Niels Basjes
 
