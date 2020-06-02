using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrbintSoft.Yauaa.WebSample.AspNetCore.Services
{
    public interface IUserAgentService
    {
        IUserAgentModel Parse(string userAgentString);
    }
}
