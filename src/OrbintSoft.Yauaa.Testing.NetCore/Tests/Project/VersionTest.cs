//-----------------------------------------------------------------------
// <copyright file="VersionTest.cs" company="OrbintSoft">
//    Yet Another User Agent Analyzer for .NET Standard
//    porting realized by Stefano Balzarotti, Copyright 2018-2019 (C) OrbintSoft
//
//    Original Author and License:
//
//    Yet Another UserAgent Analyzer
//    Copyright(C) 2013-2019 Niels Basjes
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
// <date>2019, 8, 18, 08:40</date>
//-----------------------------------------------------------------------
using FluentAssertions;
using OrbintSoft.Yauaa.Testing.Fixtures;
using Semver;
using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Xunit;

namespace OrbintSoft.Yauaa.Testing.Tests.Project
{
    public class VersionTest : IClassFixture<LogFixture>
    {
        private static readonly string SolutionDirectory = $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..";

        [Fact]
        public void TestProjectVersions()
        {
            SemVersion solutionVersion = null;
            var csprojFiles = Directory.GetFiles(SolutionDirectory, "*.csproj", SearchOption.AllDirectories);
            foreach (var prjFile in csprojFiles)
            {
                if (!prjFile.Contains(".localhistory"))
                {
                    var csproj = XDocument.Load(prjFile);
                    csproj.Should().NotBeNull();
                    csproj.Root.NodeType.Should().Be(XmlNodeType.Element);
                    csproj.Root.Name.LocalName.Should().Be("Project");
                    var propertyGroup = csproj.Root.Elements().Where(d => d.NodeType == XmlNodeType.Element && d.Name.LocalName == "PropertyGroup");
                    propertyGroup.Any().Should().BeTrue();
                    var properties = propertyGroup.SelectMany(p => p.Elements()).Where(e => e.NodeType == XmlNodeType.Element);
                    var version = properties.Where(p => p.Name.LocalName == "Version").FirstOrDefault();
                    var assemblyVersionXml = properties.Where(p => p.Name.LocalName == "AssemblyVersion").FirstOrDefault();
                    var fileVersionXml = properties.Where(p => p.Name.LocalName == "FileVersion").FirstOrDefault();
                    version.Should().NotBeNull();
                    assemblyVersionXml.Should().NotBeNull();
                    fileVersionXml.Should().NotBeNull();
                    var projVersion = SemVersion.Parse(version.Value);
                    var assemblyVersion = Version.Parse(assemblyVersionXml.Value);
                    var fileVersion = Version.Parse(fileVersionXml.Value);
                    if (solutionVersion == null)
                    {
                        solutionVersion = projVersion;
                    }
                    else
                    {
                        projVersion.Major.Should().Be(solutionVersion.Major);
                        projVersion.Minor.Should().Be(solutionVersion.Minor);
                        projVersion.Patch.Should().Be(solutionVersion.Patch);
                        projVersion.Prerelease.Should().Be(solutionVersion.Prerelease);
                    }
                    assemblyVersion.Major.Should().Be(solutionVersion.Major);
                    assemblyVersion.Minor.Should().Be(solutionVersion.Minor);
                    fileVersion.Major.Should().Be(solutionVersion.Major);
                    fileVersion.Minor.Should().Be(solutionVersion.Minor);
                    if (!string.IsNullOrWhiteSpace(solutionVersion.Prerelease))
                    {
                        var preReleaseSplit = solutionVersion.Prerelease.Split('.');
                        preReleaseSplit.Count().Should().Be(2);
                        var prefix = preReleaseSplit[0];
                        var preBuild = preReleaseSplit[1];
                    }
                }                
            }
        }


    }
}
