using System;

namespace OrbintSoft.Yauaa.WebSample.AspNetCore.Services
{
    public interface IUserAgentModel
    {
        string AgentClass { get; set; }
        string AgentName { get; set; }
        Version AgentVersion { get; set; }
        string DeviceBrand { get; set; }
        string DeviceClass { get; set; }
        string DeviceCpu { get; set; }
        string DeviceCpuBits { get; set; }
        string DeviceName { get; set; }
        string LayoutEngineClass { get; set; }
        string LayoutEngineName { get; set; }
        Version LayoutEngineVersion { get; set; }
        string OperatingSystemClass { get; set; }
        string OperatingSystemName { get; set; }
        Version OperatingSystemVersion { get; set; }
        string UserAgentString { get; }
    }
}