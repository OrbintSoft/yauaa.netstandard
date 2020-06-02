using OrbintSoft.Yauaa.Analyzer;
using OrbintSoft.Yauaa.Annotate;
using System;

namespace OrbintSoft.Yauaa.WebSample.AspNetCore.Services
{
    public class UserAgentModel : IUserAgentModel
    {
        public string UserAgentString { get; }       
        public string AgentClass { get; set; }        
        public string AgentName { get; set; }       
        public Version AgentVersion { get; set; }
        public string DeviceBrand { get; set; }
        public string DeviceClass { get; set; }
        public string DeviceName { get; set; }        
        public string LayoutEngineClass { get; set; }        
        public string LayoutEngineName { get; set; }
        public Version LayoutEngineVersion { get; set; }
        public string OperatingSystemClass { get; set; }
        public string OperatingSystemName { get; set; }
        public Version OperatingSystemVersion { get; set; }
        public string DeviceCpu { get; set; }
        public string DeviceCpuBits { get; set; }

        public UserAgentModel(string useragentString)
        {
            UserAgentString = useragentString;
        }
    }
}
