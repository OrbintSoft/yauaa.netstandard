using OrbintSoft.Yauaa.Analyzer;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrbintSoft.Yauaa.WebSample.AspNetCore.Models
{
    public class DemoViewModel
    {
        [Required(ErrorMessage = "The User Agent is required")]
        public string UserAgent { get; set; }

        public Dictionary<string, UserAgent.AgentField> Fields;

        public string Version { get; set; }

        public string ElapsedTime { get; set; }
    }
}
