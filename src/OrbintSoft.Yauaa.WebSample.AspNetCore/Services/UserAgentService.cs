using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrbintSoft.Yauaa.WebSample.AspNetCore.Services
{
    public class UserAgentService : IUserAgentService
    {
        private readonly IUserAgentMapper userAgentMapper;
        public UserAgentService(IUserAgentMapper userAgentMapper)
        {
            this.userAgentMapper = userAgentMapper;
        }

        public IUserAgentModel Parse(string userAgentString)
        {
            return userAgentMapper.Enrich(userAgentString);
        }
    }
}
