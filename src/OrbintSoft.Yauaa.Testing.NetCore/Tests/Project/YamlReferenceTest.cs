//-----------------------------------------------------------------------
// <copyright file="YamlReferenceTest.cs" company="OrbintSoft">
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
// <date>2019, 8, 2, 07:33</date>
//-----------------------------------------------------------------------
using FluentAssertions;
using OrbintSoft.Yauaa.Testing.Fixtures;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Xunit;

namespace OrbintSoft.Yauaa.Testing.Tests.Project
{
    /// <summary>
    /// With this class I verify that all resources in the project are correctly referenced.
    /// </summary>
    public class YamlReferenceTest : IClassFixture<LogFixture>
    {
        private static readonly string ProjectDirectory = $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}OrbintSoft.Yauaa.NetStandard";
        private static readonly string TestProjectDirectory = $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..";
        private static readonly string CsProjName = "OrbintSoft.Yauaa.NetStandard.csproj";
        private static readonly string TestCsProjName = "OrbintSoft.Yauaa.Testing.NetCore.csproj";
        private static readonly string YamlResourcesDirectory = "YamlResources";

        [Fact]
        public void TestProjectResources()
        {
            var csproj = CheckAndGetCsproj(ProjectDirectory, CsProjName);
            CheckResourcesReference(csproj, out var removedYaml, out var contentYaml);
            foreach (var singleYaml in contentYaml)
            {
                CheckCopyToOutput(singleYaml);
                CheckReferenceNugetPack(singleYaml);
            }
            CheckYamlFilesWIthReferences(ProjectDirectory, YamlResourcesDirectory, removedYaml, contentYaml);
        }

        [Fact]
        public void TestTestProjectResources()
        {
            var csproj = CheckAndGetCsproj(TestProjectDirectory, TestCsProjName);
            CheckResourcesReference(csproj, out var removedYaml, out var contentYaml);
            foreach (var singleYaml in contentYaml)
            {
                CheckCopyToOutput(singleYaml);
            }
            CheckYamlFilesWIthReferences(TestProjectDirectory, YamlResourcesDirectory, removedYaml, contentYaml);
        }

        private static void CheckYamlFilesWIthReferences(string projectDirectory, string yamlResoucesPath, IEnumerable<XElement> removedYaml, IEnumerable<XElement> contentYaml)
        {
            var yamlPath = Path.Combine(projectDirectory, yamlResoucesPath);
            var allfiles = Directory.GetFiles(yamlPath, "*.yaml", SearchOption.AllDirectories);
            allfiles.Any().Should().BeTrue();
            foreach (var file in allfiles)
            {
                var name = Path.GetFileName(file);
                removedYaml.Any(r => r.Attributes().Any(a => a.Name.LocalName == "Remove" && a.Value.EndsWith(name))).Should().BeTrue();
                contentYaml.Any(r => r.Attributes().Any(a => a.Name.LocalName == "Include" && a.Value.EndsWith(name))).Should().BeTrue();
            }
        }

        private static void CheckReferenceNugetPack(XElement singleYaml)
        {
            var pack = singleYaml.Elements().Where(e => e.NodeType == XmlNodeType.Element && e.Name.LocalName == "Pack");
            pack.Count().Should().Be(1);
            pack.FirstOrDefault().Value.Should().Be("true");
            var packageCopyToOutput = singleYaml.Elements().Where(e => e.NodeType == XmlNodeType.Element && e.Name.LocalName == "PackageCopyToOutput");
            pack.Count().Should().Be(1);
            pack.FirstOrDefault().Value.Should().Be("true");
        }

        private static void CheckCopyToOutput(XElement singleYaml)
        {
            var copyToOutputDirectory = singleYaml.Elements().Where(e => e.NodeType == XmlNodeType.Element && e.Name.LocalName == "CopyToOutputDirectory");
            copyToOutputDirectory.Count().Should().Be(1);
            copyToOutputDirectory.FirstOrDefault().Value.Should().Be("PreserveNewest");
        }

        private static void CheckResourcesReference(XDocument csproj, out IEnumerable<XElement> removedYaml, out IEnumerable<XElement> contentYaml)
        {
            var itemGroup = csproj.Root.Elements().Where(d => d.NodeType == XmlNodeType.Element && d.Name.LocalName == "ItemGroup");
            itemGroup.Any().Should().BeTrue();
            var items = itemGroup.SelectMany(i => i.Elements()).Where(e => e.NodeType == XmlNodeType.Element);
            removedYaml = items.Where(e => e.Name.LocalName == "None" && e.Attributes().Any(a => a.Name.LocalName == "Remove" && a.Value.EndsWith(".yaml")));
            removedYaml.Any().Should().BeTrue();
            contentYaml = items.Where(e => e.Name.LocalName == "Content" && e.Attributes().Any(a => a.Name.LocalName == "Include" && a.Value.EndsWith(".yaml")));
            contentYaml.Any().Should().BeTrue();
        }

        private static XDocument CheckAndGetCsproj(string projectDirectory, string csprojPath)
        {
            var projectPath = Path.Combine(projectDirectory, csprojPath);
            File.Exists(projectPath).Should().BeTrue();
            var csproj = XDocument.Load(projectPath);
            csproj.Should().NotBeNull();
            csproj.Root.NodeType.Should().Be(XmlNodeType.Element);
            csproj.Root.Name.LocalName.Should().Be("Project");
            return csproj;
        }
    }
}
