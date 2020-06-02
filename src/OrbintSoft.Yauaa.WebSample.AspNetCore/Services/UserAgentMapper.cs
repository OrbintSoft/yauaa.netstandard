using OrbintSoft.Yauaa.Analyzer;
using OrbintSoft.Yauaa.Annotate;
using System;

namespace OrbintSoft.Yauaa.WebSample.AspNetCore.Services
{
    public class UserAgentMapper : IUserAgentAnnotationMapper<IUserAgentModel>, IUserAgentMapper
    {
        private readonly UserAgentAnnotationAnalyzer<IUserAgentModel> userAgentAnalyzer;

        public UserAgentMapper()
        {
            userAgentAnalyzer = new UserAgentAnnotationAnalyzer<IUserAgentModel>();
            userAgentAnalyzer.Initialize(this);
        }

        [YauaaField(DefaultUserAgentFields.AGENT_CLASS)]
        public void SetAgentClass(UserAgentModel record, string value)
        {
            record.AgentClass = value;
        }

        [YauaaField(DefaultUserAgentFields.AGENT_NAME)]
        public void SetAgentName(UserAgentModel record, string value)
        {
            record.AgentName = value;
        }

        [YauaaField(DefaultUserAgentFields.AGENT_VERSION)]
        public void SetAgentVersion(UserAgentModel record, string value)
        {
            Version.TryParse(value, out var version);
            record.AgentVersion = version;
        }

        [YauaaField(DefaultUserAgentFields.DEVICE_BRAND)]
        public void SetDeviceBrand(UserAgentModel record, string value)
        {
            record.DeviceBrand = value;
        }

        [YauaaField(DefaultUserAgentFields.DEVICE_CLASS)]
        public void SetDeviceClass(UserAgentModel record, string value)
        {
            record.DeviceClass = value;
        }

        [YauaaField(DefaultUserAgentFields.DEVICE_NAME)]
        public void SetDeviceName(UserAgentModel record, string value)
        {
            record.DeviceName = value;
        }

        [YauaaField(DefaultUserAgentFields.LAYOUT_ENGINE_CLASS)]
        public void SetLayoutEngineClass(UserAgentModel record, string value)
        {
            record.LayoutEngineClass = value;
        }

        [YauaaField(DefaultUserAgentFields.LAYOUT_ENGINE_NAME)]
        public void SetLayoutEngineName(UserAgentModel record, string value)
        {
            record.LayoutEngineName = value;
        }

        [YauaaField(DefaultUserAgentFields.LAYOUT_ENGINE_VERSION)]
        public void SetLayoutEngineVersion(UserAgentModel record, string value)
        {
            Version.TryParse(value, out var version);
            record.LayoutEngineVersion = version;
        }

        [YauaaField(DefaultUserAgentFields.OPERATING_SYSTEM_CLASS)]
        public void SetOperatingSystemClass(UserAgentModel record, string value)
        {
            record.OperatingSystemClass = value;
        }

        [YauaaField(DefaultUserAgentFields.OPERATING_SYSTEM_NAME)]
        public void SetOperatingSystemName(UserAgentModel record, string value)
        {
            record.OperatingSystemName = value;
        }

        [YauaaField(DefaultUserAgentFields.OPERATING_SYSTEM_VERSION)]
        public void SetOperatingSystemVersion(UserAgentModel record, string value)
        {
            Version.TryParse(value, out var version);
            record.OperatingSystemVersion = version;
        }

        [YauaaField(DefaultUserAgentFields.DEVICE_CPU)]
        public void SetDeviceCpu(UserAgentModel record, string value)
        {            
            record.DeviceCpu = value;
        }

        [YauaaField(DefaultUserAgentFields.DEVICE_CPU_BITS)]
        public void SetDeviceCpuBits(UserAgentModel record, string value)
        {
            record.DeviceCpuBits = value;
        }

        public IUserAgentModel Enrich(string userAgent)
        {
            var model = new UserAgentModel(userAgent);
            return userAgentAnalyzer.Map(model);
        }

        public string GetUserAgentString(IUserAgentModel record)
        {
            return record.UserAgentString;
        }
    }
}
