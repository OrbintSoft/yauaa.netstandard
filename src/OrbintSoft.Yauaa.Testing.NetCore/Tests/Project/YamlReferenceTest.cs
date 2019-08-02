using OrbintSoft.Yauaa.Testing.Fixtures;
using System.IO;
using System.Xml;
using Xunit;
using FluentAssertions;
using System.Xml.Linq;
using System.Linq;

namespace OrbintSoft.Yauaa.Testing.Tests.Project
{
    public class YamlReferenceTest : IClassFixture<LogFixture>
    {
        public const string ProjectPath = @"..\..\..\..\OrbintSoft.Yauaa.NetStandard\OrbintSoft.Yauaa.NetStandard.csproj";
        public const string YamlPath = @"..\..\..\..\OrbintSoft.Yauaa.NetStandard\YamlResources\UserAgents";

        [Fact]
        public void TestAllResources()
        {
            var test = Path.GetFullPath(@"..\..\..\..\OrbintSoft.Yauaa.NetStandard.csproj");
            File.Exists(ProjectPath).Should().BeTrue();
            var csproj = XDocument.Load(ProjectPath);
            csproj.Should().NotBeNull();
            csproj.Root.NodeType.Should().Be(XmlNodeType.Element);
            csproj.Root.Name.LocalName.Should().Be("Project");
            var itemGroup = csproj.Root.Elements().Where(d => d.NodeType == XmlNodeType.Element && d.Name.LocalName == "ItemGroup");
            itemGroup.Any().Should().BeTrue();
            var items = itemGroup.SelectMany(i => i.Elements()).Where(e => e.NodeType == XmlNodeType.Element);
            var removedYaml = items.Where(e => e.Name.LocalName == "None" && e.Attributes().Any(a => a.Name.LocalName == "Remove" && a.Value.EndsWith(".yaml")));
            removedYaml.Any().Should().BeTrue();
            var contentYaml = items.Where(e => e.Name.LocalName == "Content" && e.Attributes().Any(a => a.Name.LocalName == "Include" && a.Value.EndsWith(".yaml")));
            contentYaml.Any().Should().BeTrue();
            foreach (var singleYaml in contentYaml)
            {
                var copyToOutputDirectory = singleYaml.Elements().Where(e => e.NodeType == XmlNodeType.Element && e.Name.LocalName == "CopyToOutputDirectory");
                copyToOutputDirectory.Count().Should().Be(1);
                copyToOutputDirectory.FirstOrDefault().Value.Should().Be("PreserveNewest");
                var pack = singleYaml.Elements().Where(e => e.NodeType == XmlNodeType.Element && e.Name.LocalName == "Pack");
                pack.Count().Should().Be(1);
                pack.FirstOrDefault().Value.Should().Be("true");
                var packageCopyToOutput = singleYaml.Elements().Where(e => e.NodeType == XmlNodeType.Element && e.Name.LocalName == "PackageCopyToOutput");
                pack.Count().Should().Be(1);
                pack.FirstOrDefault().Value.Should().Be("true");
            }

            var allfiles = Directory.GetFiles(YamlPath, "*.yaml");
            allfiles.Any().Should().BeTrue();
            foreach (var file in allfiles)
            {
                var name = Path.GetFileName(file);
                removedYaml.Any(r => r.Attributes().Any(a => a.Name.LocalName == "Remove" && a.Value.EndsWith(name))).Should().BeTrue();
                contentYaml.Any(r => r.Attributes().Any(a => a.Name.LocalName == "Include" && a.Value.EndsWith(name))).Should().BeTrue();
            }
        }
    }
}
