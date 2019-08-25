# Introduction

First of all, thank you to be here, any contribute here is welcome, and your help can be vital to keep this project alive and to make it a great library.

#Languages and framework

- Most of the library is written in C# 7.3
- Parsing rules are written in ANTLR4, this part is shared with the main Java library and usually we don't touch that in this repository.
- User agent definitions are witten in YAML, these are shared with the main Java library and usually we don't touch those in this repository.
- The libary is written in .NET Standard 2.0.
- Tools and applications based on the library, including unit tests are witten in .NET Core 2.2.

## Requirements
Here I define a standard enviroment and list of software suggested or required to develop without throubles.

If you prefer to use different software or enviroment you are free to do,but here we don't provide support for that.

### Recommended enviroment

The project is multi platform, but here I recommend:

- **Operating system:** Windows 10
- **IDE**: Visual Studio 2019 Community Edition

### Required Software

- **.NET Framework:** [.NET Core 2.2.300 SDK](https://dotnet.microsoft.com/download/dotnet-core/2.2)
- **Java JDK:** [Java SE Development Kit 12.0.1](https://www.oracle.com/technetwork/java/javase/downloads/jdk12-downloads-5295953.html)

### Enviroment Variables

- **JAVA_HOME**: Add JAVA_HOME to System enviroment variables, and set *C:\Program Files\Java\jdk-12.0.1* as PATH
- **PATH**: Add  %JAVA_HOME%\bin to Enviroment PATH System Variables

### Enable powershell script

Open powershell and launch this command: `set-executionpolicy unrestricted`

This will allow you to run unsigned scripts.

### Recommended extensions

The following Visual Studio extensions are not necessary, but they can be useful:

- Markdown editor
- Output enhamcer
- Powershell tools
- Real clean
- License Header Manager

