using Microsoft.AspNetCore.Mvc;
using OrbintSoft.Yauaa.WebSample.AspNetCore.Services;
using System.Linq;

namespace OrbintSoft.Yauaa.WebSample.AspNetCore.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class ApiController : ControllerBase
    {
        private readonly IUserAgentService userAgentService;

        public ApiController(IUserAgentService userAgentService)
        {
            this.userAgentService = userAgentService;
        }

        [HttpGet]
        public IUserAgentModel Get()
        {
            string userAgent = this.HttpContext?.Request?.Headers?.FirstOrDefault(s => s.Key.ToLower() == "user-agent").Value;
            return userAgentService.Parse(userAgent);
        }

        [HttpPost]
        public IUserAgentModel Post(string userAgent)
        {            
            return userAgentService.Parse(userAgent);
        }
    }
}
